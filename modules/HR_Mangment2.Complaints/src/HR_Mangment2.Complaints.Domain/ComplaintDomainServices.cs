using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Common;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Notifications;
using HR_Mangment2.Settings;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace HR_Mangment2.Complaints;

/// <summary>Every notification about a complaint carries the same generic text. No name, subject or level ever goes in one.</summary>
public class ComplaintNotifier : ITransientDependency
{
    private readonly ComplaintAccessPolicy _policy;
    private readonly HrNotificationPublisher _notifications;

    public ComplaintNotifier(ComplaintAccessPolicy policy, HrNotificationPublisher notifications)
    {
        _policy = policy;
        _notifications = notifications;
    }

    public virtual async Task NotifyHandlersAsync(Complaint complaint, Guid? exceptUserId = null)
    {
        foreach (var handler in await _policy.GetHandlerUserIdsAsync(complaint))
        {
            if (handler != exceptUserId)
            {
                await _notifications.PublishAsync(handler, NotificationTemplateCode.SecureItemUpdateForHandler, NotificationResourceType.SecureItem, complaint.Id);
            }
        }
    }

    public virtual Task NotifyComplainantAsync(Complaint complaint) =>
        _notifications.PublishAsync(complaint.ComplainantUserId, NotificationTemplateCode.SecureItemUpdateForOwner, NotificationResourceType.SecureItem, complaint.Id);
}

/// <summary>QUESTIONS.md item 36: a normal complaint waiting for the old manager goes straight to HR. It is never counted as a moved item.</summary>
public class ComplaintManagerChangeEffects : IManagerChangeEffects, ITransientDependency
{
    private readonly IRepository<Complaint, Guid> _complaints;
    private readonly ComplaintNotifier _notifier;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public ComplaintManagerChangeEffects(
        IRepository<Complaint, Guid> complaints,
        ComplaintNotifier notifier,
        IGuidGenerator guidGenerator,
        IClock clock,
        ICurrentUser currentUser)
    {
        _complaints = complaints;
        _notifier = notifier;
        _guidGenerator = guidGenerator;
        _clock = clock;
        _currentUser = currentUser;
    }

    public virtual async Task<int> MovePendingItemsAsync(Guid employeeId, Guid? previousManagerEmployeeId, Guid newManagerEmployeeId)
    {
        var query = (await _complaints.WithDetailsAsync(x => x.Actions))
            .Where(x => x.ComplainantEmployeeId == employeeId && x.Status == ComplaintStatus.PendingDepartmentManager);

        foreach (var complaint in await _complaints.AsyncExecuter.ToListAsync(query))
        {
            complaint.MoveToHr();
            complaint.AddAction(_guidGenerator.Create(), ComplaintActionType.Forwarded, _currentUser.Id ?? complaint.ComplainantUserId, null, _clock.Now);
            await _complaints.UpdateAsync(complaint, autoSave: true);
            await _notifier.NotifyHandlersAsync(complaint);
        }

        return 0;
    }
}

/// <summary>QUESTIONS.md item B: an approved assignment makes two-step verification mandatory, from approval until it ends.</summary>
public class ComplaintHandlerMfaRequirementContributor : IMfaRequirementContributor, ITransientDependency
{
    private readonly IRepository<ComplaintHandlerAssignment, Guid> _assignments;
    private readonly IClock _clock;

    public ComplaintHandlerMfaRequirementContributor(IRepository<ComplaintHandlerAssignment, Guid> assignments, IClock clock)
    {
        _assignments = assignments;
        _clock = clock;
    }

    /// <summary>The access gate calls this from middleware, before any request unit of work starts.</summary>
    [UnitOfWork]
    public virtual Task<bool> IsRequiredAsync(Guid userId)
    {
        var now = _clock.Now;
        return _assignments.AnyAsync(x =>
            x.UserId == userId &&
            x.Status == ComplaintHandlerAssignmentStatus.Approved &&
            (x.To == null || x.To > now));
    }
}

/// <summary>
/// Lateness only reminds: it changes neither the level nor the handler, and gives the person reminded no access.
/// Someone who cannot open the complaint gets the reminder without a link to it.
/// </summary>
public class ComplaintOverdueReminderService : ITransientDependency
{
    private readonly IRepository<Complaint, Guid> _complaints;
    private readonly IdentityUserManager _userManager;
    private readonly ComplaintAccessPolicy _policy;
    private readonly WorkCalendarService _calendar;
    private readonly HrSettingsStore _settings;
    private readonly HrNotificationPublisher _notifications;
    private readonly ICompanyClock _companyClock;

    public ComplaintOverdueReminderService(
        IRepository<Complaint, Guid> complaints,
        IdentityUserManager userManager,
        ComplaintAccessPolicy policy,
        WorkCalendarService calendar,
        HrSettingsStore settings,
        HrNotificationPublisher notifications,
        ICompanyClock companyClock)
    {
        _complaints = complaints;
        _userManager = userManager;
        _policy = policy;
        _calendar = calendar;
        _settings = settings;
        _notifications = notifications;
        _companyClock = companyClock;
    }

    /// <returns>How many complaints had a reminder sent.</returns>
    public virtual async Task<int> SendDueAsync(DateTime now)
    {
        var interval = (await _settings.PeekValueAsync<ComplaintSettings>()).OverdueReminderIntervalWorkingDays;
        var zone = await _companyClock.GetTimeZoneAsync();
        var today = CompanyClock.ToLocalDate(now, zone);

        var overdue = await _complaints.GetListAsync(x => x.Status != ComplaintStatus.Closed && x.DueAt != null && x.DueAt < now);
        var sent = 0;

        foreach (var complaint in overdue)
        {
            if (complaint.LastOverdueReminderAt != null)
            {
                // With no interval set, one reminder is enough.
                if (interval is null or <= 0)
                {
                    continue;
                }

                var since = CompanyClock.ToLocalDate(complaint.LastOverdueReminderAt.Value, zone);
                if (await _calendar.CountWorkingDaysAfterAsync(since, today) < interval)
                {
                    continue;
                }
            }

            var role = complaint.Level == ComplaintLevel.HighlyConfidential ? HrRoles.ExecutiveDirector : HrRoles.HrManager;
            foreach (var holder in (await _userManager.GetUsersInRoleAsync(role)).Where(x => x.IsActive))
            {
                if (holder.Id == complaint.ComplainantUserId || holder.Id == complaint.RespondentUserId)
                {
                    continue;
                }

                var canOpen = (await _policy.GetContextAsync(holder.Id)).CanHandle(complaint);
                await _notifications.PublishAsync(
                    holder.Id,
                    NotificationTemplateCode.SecureItemUpdateForHandler,
                    canOpen ? NotificationResourceType.SecureItem : NotificationResourceType.None,
                    canOpen ? complaint.Id : null);
            }

            complaint.MarkOverdueReminded(now);
            await _complaints.UpdateAsync(complaint);
            sent++;
        }

        return sent;
    }
}

public class ComplaintOverdueReminderWorker : AsyncPeriodicBackgroundWorkerBase
{
    private const int FirstHourOfDay = 8;

    public ComplaintOverdueReminderWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = (int)TimeSpan.FromHours(1).TotalMilliseconds;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var services = workerContext.ServiceProvider;
        var unitOfWorkManager = services.GetRequiredService<IUnitOfWorkManager>();
        var currentTenant = services.GetRequiredService<ICurrentTenant>();
        var now = services.GetRequiredService<IClock>().Now;

        List<Guid> tenantIds;
        using (var read = unitOfWorkManager.Begin(requiresNew: true))
        {
            tenantIds = (await services.GetRequiredService<ITenantRepository>().GetListAsync()).Select(x => x.Id).ToList();
            await read.CompleteAsync();
        }

        foreach (var tenantId in tenantIds)
        {
            using (currentTenant.Change(tenantId))
            using (var unitOfWork = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                var zone = await services.GetRequiredService<ICompanyClock>().GetTimeZoneAsync();
                if (TimeZoneInfo.ConvertTimeFromUtc(now, zone).Hour >= FirstHourOfDay)
                {
                    await services.GetRequiredService<ComplaintOverdueReminderService>().SendDueAsync(now);
                }

                await unitOfWork.CompleteAsync();
            }
        }
    }
}
