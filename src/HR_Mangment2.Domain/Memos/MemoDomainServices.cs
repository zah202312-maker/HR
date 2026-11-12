using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Delegations;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Notifications;
using HR_Mangment2.Requests;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Validation;

namespace HR_Mangment2.Memos;

public record PlannedMemoStage(MemoStatus Status, string Role, bool IsLinkedManager);

/// <summary>QUESTIONS.md item 24: where a memo goes first. The row order is the matrix's.</summary>
public static class MemoRoutePlanner
{
    /// <returns>Null for the executive director, who does not write memos.</returns>
    public static PlannedMemoStage? FirstStage(RequestOwnerRow row) => row switch
    {
        RequestOwnerRow.Employee => new PlannedMemoStage(MemoStatus.PendingManager, HrRoles.DepartmentManager, true),
        // Decision 2026-11-10: the department manager reports to the HR manager, so the memo starts at HR.
        RequestOwnerRow.DepartmentManager => new PlannedMemoStage(MemoStatus.PendingHr, HrRoles.HrManager, false),
        RequestOwnerRow.SystemAdmin or RequestOwnerRow.HrManager => new PlannedMemoStage(MemoStatus.PendingExecutive, HrRoles.ExecutiveDirector, false),
        RequestOwnerRow.Reviewer => new PlannedMemoStage(MemoStatus.PendingHr, HrRoles.HrManager, false),
        _ => null
    };
}

/// <summary>Who to tell when a memo or incident reaches a stage: the named user or the role's active holders, plus their delegates.</summary>
public class MemoIncidentAudience : ITransientDependency
{
    private readonly IdentityUserManager _userManager;
    private readonly IRepository<Delegation, Guid> _delegations;
    private readonly IClock _clock;

    public MemoIncidentAudience(IdentityUserManager userManager, IRepository<Delegation, Guid> delegations, IClock clock)
    {
        _userManager = userManager;
        _delegations = delegations;
        _clock = clock;
    }

    /// <param name="exceptUserId">The writer or submitter, who is told separately and never acts on their own item.</param>
    public virtual async Task<IReadOnlyCollection<Guid>> GetAsync(string role, Guid? userId, Guid exceptUserId)
    {
        var holders = userId != null
            ? new List<Guid> { userId.Value }
            : (await _userManager.GetUsersInRoleAsync(role)).Where(x => x.IsActive).Select(x => x.Id).ToList();

        var now = _clock.Now;
        var delegates = (await _delegations.GetListAsync(x =>
                holders.Contains(x.FromUserId) &&
                x.CancelledAt == null &&
                x.StartsAt <= now &&
                now < x.EndsAt &&
                (x.Scope == DelegationScope.AllApprovals || x.Scope == DelegationScope.MemosAndIncidents)))
            .Select(x => x.ToUserId);

        return holders.Concat(delegates).Where(x => x != exceptUserId).Distinct().ToList();
    }
}

/// <summary>Creating an incident, directly or from a memo.</summary>
public class IncidentCreator : ITransientDependency
{
    private readonly IRepository<Incident, Guid> _incidents;
    private readonly IRequestStockGateway _stock;
    private readonly MemoIncidentAudience _audience;
    private readonly HrNotificationPublisher _notifications;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly IClock _clock;

    public IncidentCreator(
        IRepository<Incident, Guid> incidents,
        IRequestStockGateway stock,
        MemoIncidentAudience audience,
        HrNotificationPublisher notifications,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IClock clock)
    {
        _incidents = incidents;
        _stock = stock;
        _audience = audience;
        _notifications = notifications;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _clock = clock;
    }

    /// <param name="managerUserId">Null starts the incident at HR.</param>
    public virtual async Task<Incident> CreateAsync(Employee owner, Guid ownerUserId, Guid? itemId, string description, Guid? managerUserId, Guid? sourceMemoId, Guid actorUserId)
    {
        if (itemId != null && !await _stock.IsItemActiveAsync(itemId.Value))
        {
            // Neither operation lists a code for this, and until release 2 no item exists at all.
            const string message = "The item does not exist.";
            throw new AbpValidationException(message, new List<ValidationResult> { new(message, new[] { "itemId" }) });
        }

        var incident = new Incident(_guidGenerator.Create(), _currentTenant.Id, owner.Id, ownerUserId, itemId, description, managerUserId, sourceMemoId);
        incident.AddTimeline(_guidGenerator.Create(), _clock.Now, IncidentTimelineActions.Submitted, actorUserId, null);
        await _incidents.InsertAsync(incident, autoSave: true);

        foreach (var recipient in await _audience.GetAsync(incident.CurrentHandlerRole!, managerUserId, ownerUserId))
        {
            await _notifications.PublishAsync(recipient, NotificationTemplateCode.IncidentUpdate, NotificationResourceType.Incident, incident.Id);
        }

        return incident;
    }
}

/// <summary>QUESTIONS.md item 7: memos and incidents waiting for the old manager move to the new one.</summary>
public class MemoIncidentManagerChangeEffects : IManagerChangeEffects, ITransientDependency
{
    private readonly IRepository<Memo, Guid> _memos;
    private readonly IRepository<Incident, Guid> _incidents;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly HrNotificationPublisher _notifications;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;

    public MemoIncidentManagerChangeEffects(
        IRepository<Memo, Guid> memos,
        IRepository<Incident, Guid> incidents,
        IRepository<Employee, Guid> employees,
        HrNotificationPublisher notifications,
        IGuidGenerator guidGenerator,
        IClock clock)
    {
        _memos = memos;
        _incidents = incidents;
        _employees = employees;
        _notifications = notifications;
        _guidGenerator = guidGenerator;
        _clock = clock;
    }

    public virtual async Task<int> MovePendingItemsAsync(Guid employeeId, Guid? previousManagerEmployeeId, Guid newManagerEmployeeId)
    {
        var newManagerUserId = (await _employees.GetAsync(newManagerEmployeeId)).UserId;
        if (newManagerUserId == null)
        {
            return 0;
        }

        var memoQuery = (await _memos.WithDetailsAsync(x => x.Handlers))
            .Where(x => x.EmployeeId == employeeId && x.Status == MemoStatus.PendingManager);
        var memos = await _memos.AsyncExecuter.ToListAsync(memoQuery);

        foreach (var memo in memos)
        {
            memo.ReassignManager(_guidGenerator.Create(), newManagerUserId.Value, _clock.Now);
            await _memos.UpdateAsync(memo, autoSave: true);
            await _notifications.PublishAsync(newManagerUserId.Value, NotificationTemplateCode.MemoUpdate, NotificationResourceType.Memo, memo.Id);
        }

        var incidents = await _incidents.GetListAsync(x => x.EmployeeId == employeeId && x.Status == IncidentStatus.PendingManager);

        foreach (var incident in incidents)
        {
            incident.ReassignManager(newManagerUserId.Value);
            await _incidents.UpdateAsync(incident, autoSave: true);
            await _notifications.PublishAsync(newManagerUserId.Value, NotificationTemplateCode.IncidentUpdate, NotificationResourceType.Incident, incident.Id);
        }

        return memos.Count + incidents.Count;
    }
}
