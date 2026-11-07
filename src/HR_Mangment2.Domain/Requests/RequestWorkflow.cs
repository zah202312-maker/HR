using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Delegations;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Identity;
using HR_Mangment2.Notifications;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;

namespace HR_Mangment2.Requests;

/// <summary>The leave and attendance-correction modules react to final decisions on their requests through this.</summary>
public interface IRequestLifecycleHandler
{
    Task OnApprovedAsync(Request request);

    Task OnRejectedAsync(Request request);

    Task OnCancelledAsync(Request request);
}

/// <summary>What the requests module needs from the store module.</summary>
public interface IRequestStockGateway
{
    Task<bool> IsItemActiveAsync(Guid itemId);

    /// <summary>Creates the stock issue when a request reaches the store, and notifies the store officers.</summary>
    Task CreateIssueAsync(Request request);

    /// <summary>False once the store has acknowledged the issue.</summary>
    Task<bool> CanCancelIssueAsync(Request request);

    Task CancelIssueAsync(Request request);
}

public enum RequestAuthorityKind
{
    None,
    Direct,
    Delegated
}

public record RequestAuthority(RequestAuthorityKind Kind, Guid? DelegatedFromUserId);

/// <summary>A user's roles and the delegations reaching them right now, both read from the database.</summary>
public class RequestDeciderContext
{
    public RequestDeciderContext(Guid userId, IReadOnlySet<string> roles, IReadOnlyDictionary<Guid, IReadOnlyCollection<string>> delegators)
    {
        UserId = userId;
        Roles = roles;
        Delegators = delegators;
    }

    public Guid UserId { get; }
    public IReadOnlySet<string> Roles { get; }

    /// <summary>The users who delegated request decisions to this user, each with their roles. A delegation is never passed on, because only the delegator's own authority counts.</summary>
    public IReadOnlyDictionary<Guid, IReadOnlyCollection<string>> Delegators { get; }

    /// <summary>The user's own authority over the current step, ignoring whose request it is.</summary>
    public bool HoldsCurrentStepDirectly(Request request) =>
        request.IsPending && HoldsDirectly(request.CurrentApproverUserId, request.CurrentApproverRole!);

    public bool HoldsDirectly(Guid? approverUserId, string approverRole) =>
        approverUserId != null ? approverUserId == UserId : Roles.Contains(approverRole);

    public Guid? FindDelegator(Guid? approverUserId, string approverRole)
    {
        if (approverUserId != null)
        {
            return Delegators.ContainsKey(approverUserId.Value) ? approverUserId : null;
        }

        foreach (var delegator in Delegators)
        {
            if (delegator.Value.Contains(approverRole))
            {
                return delegator.Key;
            }
        }

        return null;
    }

    /// <summary>Who may act on the current step. A delegate never decides their own request: it stays with the principal.</summary>
    public RequestAuthority Resolve(Request request)
    {
        if (!request.IsPending)
        {
            return new RequestAuthority(RequestAuthorityKind.None, null);
        }

        if (HoldsCurrentStepDirectly(request))
        {
            return new RequestAuthority(RequestAuthorityKind.Direct, null);
        }

        if (request.OwnerUserId == UserId)
        {
            return new RequestAuthority(RequestAuthorityKind.None, null);
        }

        var delegator = FindDelegator(request.CurrentApproverUserId, request.CurrentApproverRole!);
        return delegator == null
            ? new RequestAuthority(RequestAuthorityKind.None, null)
            : new RequestAuthority(RequestAuthorityKind.Delegated, delegator);
    }

    /// <summary>QUESTIONS.md item 22: the accountant or store officer may carry out their own request, as execution rather than approval.</summary>
    public bool IsSelfExecution(Request request) =>
        request.OwnerUserId == UserId && request.Status == RequestStatus.PendingDestination && HoldsCurrentStepDirectly(request);

    public bool CanApprove(Request request) =>
        !(request.Destination == RequestDestination.Store && request.Status == RequestStatus.PendingDestination) &&
        Resolve(request).Kind != RequestAuthorityKind.None &&
        (request.OwnerUserId != UserId || IsSelfExecution(request));

    public bool CanReject(Request request) =>
        request.OwnerUserId != UserId && Resolve(request).Kind != RequestAuthorityKind.None;
}

/// <summary>Reads who can decide what. Every check uses the database, never the token.</summary>
public class RequestDeciderReader : ITransientDependency
{
    private readonly IIdentityUserRepository _users;
    private readonly IRepository<Delegation, Guid> _delegations;
    private readonly IClock _clock;

    public RequestDeciderReader(IIdentityUserRepository users, IRepository<Delegation, Guid> delegations, IClock clock)
    {
        _users = users;
        _delegations = delegations;
        _clock = clock;
    }

    public virtual Task<RequestDeciderContext> GetAsync(Guid userId) => GetAsync(userId, DelegationScope.Requests);

    /// <summary>The same reading for memos and incidents, which have their own delegation scope.</summary>
    public virtual Task<RequestDeciderContext> GetForMemosAndIncidentsAsync(Guid userId) => GetAsync(userId, DelegationScope.MemosAndIncidents);

    private async Task<RequestDeciderContext> GetAsync(Guid userId, DelegationScope scope)
    {
        var roles = (await _users.GetRoleNamesAsync(userId)).ToHashSet();
        var now = _clock.Now;
        var delegations = await _delegations.GetListAsync(x =>
            x.ToUserId == userId &&
            x.CancelledAt == null &&
            x.StartsAt <= now &&
            now < x.EndsAt &&
            (x.Scope == DelegationScope.AllApprovals || x.Scope == scope));

        var delegators = new Dictionary<Guid, IReadOnlyCollection<string>>();
        foreach (var fromUserId in delegations.Select(x => x.FromUserId).Distinct())
        {
            delegators[fromUserId] = (await _users.GetRoleNamesAsync(fromUserId)).ToList();
        }

        return new RequestDeciderContext(userId, roles, delegators);
    }
}

/// <param name="DetailsSummaryAr">The server-made summary. The module that owns the request builds it in both languages.</param>
/// <param name="ManagerStepApprovedByUserId">QUESTIONS.md item 25: converting a memo counts as the manager's approval, whoever converts it.</param>
public record RequestSubmission(
    RequestType Type,
    Employee Owner,
    Guid SubmittedByUserId,
    bool IsOnBehalf,
    string? Description,
    string DetailsSummaryAr,
    string DetailsSummaryEn,
    Guid? ItemId = null,
    decimal? Quantity = null,
    Guid? SourceMemoId = null,
    Guid? ManagerStepApprovedByUserId = null);

/// <summary>
/// The approval matrix in action: building a route, moving along it, and ending it. The leave and
/// attendance-correction modules submit through here too, so every request type shares these rules.
/// </summary>
public class RequestWorkflowService : ITransientDependency
{
    private readonly IRepository<Request, Guid> _requests;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IRepository<Delegation, Guid> _delegations;
    private readonly IdentityUserManager _userManager;
    private readonly IIdentityUserRepository _users;
    private readonly ManagerLinkService _managerLinks;
    private readonly IRequestStockGateway _stock;
    private readonly IEnumerable<IRequestLifecycleHandler> _handlers;
    private readonly HrNotificationPublisher _notifications;
    private readonly AdministrativeTaskLogger _taskLog;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly IClock _clock;

    public RequestWorkflowService(
        IRepository<Request, Guid> requests,
        IRepository<Employee, Guid> employees,
        IRepository<Delegation, Guid> delegations,
        IdentityUserManager userManager,
        IIdentityUserRepository users,
        ManagerLinkService managerLinks,
        IRequestStockGateway stock,
        IEnumerable<IRequestLifecycleHandler> handlers,
        HrNotificationPublisher notifications,
        AdministrativeTaskLogger taskLog,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IClock clock)
    {
        _requests = requests;
        _employees = employees;
        _delegations = delegations;
        _userManager = userManager;
        _users = users;
        _managerLinks = managerLinks;
        _stock = stock;
        _handlers = handlers;
        _notifications = notifications;
        _taskLog = taskLog;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _clock = clock;
    }

    /// <summary>The request's owner, applying the on-behalf rules: a department manager, for someone in their own team, in the employee row of the matrix.</summary>
    public virtual async Task<(Employee Owner, bool IsOnBehalf)> ResolveOwnerAsync(Guid currentUserId, Guid? onBehalfOfEmployeeId)
    {
        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == currentUserId);
        if (onBehalfOfEmployeeId == null)
        {
            // Every account that can sign in and submit has a file. One without a file has no route to follow.
            return (me ?? throw new AbpAuthorizationException(), false);
        }

        var myRoles = await _users.GetRoleNamesAsync(currentUserId);
        var owner = await _employees.FindAsync(onBehalfOfEmployeeId.Value);
        if (me == null || !myRoles.Contains(HrRoles.DepartmentManager) || owner == null || owner.ManagerEmployeeId != me.Id)
        {
            throw new AbpAuthorizationException();
        }

        var ownerRow = RequestRoutePlanner.ResolveRow(await _managerLinks.GetRolesAsync(owner));
        if (ownerRow != RequestOwnerRow.Employee || owner.Status != EmployeeStatus.Active)
        {
            throw new BusinessException(HrErrorCodes.Request.OnBehalfNotAllowed);
        }

        return (owner, true);
    }

    public virtual async Task<Request> SubmitAsync(RequestSubmission submission)
    {
        var owner = submission.Owner;
        var row = RequestRoutePlanner.ResolveRow(await _managerLinks.GetRolesAsync(owner));
        var plan = RequestRoutePlanner.Plan(row, submission.Type.Destination)
                   ?? throw new BusinessException(HrErrorCodes.Request.ExecutiveCannotSubmit);

        var now = _clock.Now;
        var request = new Request(
            _guidGenerator.Create(),
            _currentTenant.Id,
            submission.Type,
            owner.Id,
            owner.UserId,
            submission.SubmittedByUserId,
            submission.IsOnBehalf,
            submission.Description,
            submission.DetailsSummaryAr,
            submission.DetailsSummaryEn,
            submission.ItemId,
            submission.Quantity,
            submission.SourceMemoId);

        var order = 1;
        foreach (var planned in plan)
        {
            Guid? approverUserId = null;
            if (planned.IsLinkedManager)
            {
                approverUserId = submission.IsOnBehalf
                    ? submission.SubmittedByUserId
                    : submission.ManagerStepApprovedByUserId ?? await GetLinkedManagerUserIdAsync(owner);
            }
            else
            {
                await CheckRoleHasHolderAsync(planned.ApproverRole);
            }

            request.Steps.Add(new RequestStep(_guidGenerator.Create(), _currentTenant.Id, request.Id, order++, planned.Stage, planned.ApproverRole, approverUserId));
        }

        if (submission.IsOnBehalf)
        {
            // The manager's own submission counts as the first approval.
            request.Steps.OrderBy(x => x.Order).First()
                .Decide(RequestStepDecision.ApprovedBySubmission, submission.SubmittedByUserId, null, now, null);
        }
        else if (submission.ManagerStepApprovedByUserId != null)
        {
            // A row without a manager step simply starts from its first step.
            request.Steps.FirstOrDefault(x => x.Stage == RequestStatus.PendingManager)
                ?.Decide(RequestStepDecision.ApprovedBySubmission, submission.ManagerStepApprovedByUserId.Value, null, now, null);
        }

        request.AddTimeline(
            _guidGenerator.Create(),
            now,
            submission.IsOnBehalf ? RequestTimelineActions.SubmittedOnBehalf : RequestTimelineActions.Submitted,
            submission.SubmittedByUserId,
            null);
        request.AdvanceToNextStep(now);

        await _requests.InsertAsync(request, autoSave: true);

        if (submission.IsOnBehalf)
        {
            await _taskLog.LogAsync(submission.SubmittedByUserId, AdministrativeTaskType.RequestSubmittedOnBehalf, request.Id);
            await NotifyOwnerAsync(request, NotificationTemplateCode.RequestSubmittedOnBehalf);
        }

        await AnnounceCurrentStepAsync(request);
        return request;
    }

    public virtual async Task ApproveAsync(Request request, RequestDeciderContext decider, string? note)
    {
        CheckPending(request);

        var selfExecution = decider.IsSelfExecution(request);
        if (request.OwnerUserId == decider.UserId && !selfExecution)
        {
            throw new BusinessException(HrErrorCodes.Request.SelfApprovalNotAllowed);
        }

        var authority = decider.Resolve(request);
        if (authority.Kind == RequestAuthorityKind.None)
        {
            throw new AbpAuthorizationException();
        }

        if (request.Destination == RequestDestination.Store && request.Status == RequestStatus.PendingDestination)
        {
            throw new BusinessException(HrErrorCodes.Request.UseStockIssueFlow);
        }

        var now = _clock.Now;
        request.CurrentStep!.Decide(RequestStepDecision.Approved, decider.UserId, authority.DelegatedFromUserId, now, note);

        if (!selfExecution)
        {
            request.AddTimeline(_guidGenerator.Create(), now, RequestTimelineActions.Approved, decider.UserId, note);
            await _taskLog.LogAsync(decider.UserId, AdministrativeTaskType.RequestApproved, request.Id);
        }

        request.AdvanceToNextStep(now);

        if (request.Status == RequestStatus.Completed)
        {
            // For the accountant, approving is the execution. The note stays on the completion when no approval entry carries it.
            request.AddTimeline(_guidGenerator.Create(), now, RequestTimelineActions.Completed, decider.UserId, selfExecution ? note : null);
        }

        await _requests.UpdateAsync(request, autoSave: true);

        if (request.Status == RequestStatus.Approved)
        {
            foreach (var handler in _handlers)
            {
                await handler.OnApprovedAsync(request);
            }
        }

        await NotifyOwnerAsync(request, NotificationTemplateCode.RequestStatusChanged);
        await AnnounceCurrentStepAsync(request);
    }

    public virtual async Task RejectAsync(Request request, RequestDeciderContext decider, string reason)
    {
        CheckPending(request);

        // The self-approval ban covers rejecting, including at the destination.
        if (request.OwnerUserId == decider.UserId)
        {
            throw new BusinessException(HrErrorCodes.Request.SelfApprovalNotAllowed);
        }

        var authority = decider.Resolve(request);
        if (authority.Kind == RequestAuthorityKind.None)
        {
            throw new AbpAuthorizationException();
        }

        var now = _clock.Now;
        var atStore = request.Destination == RequestDestination.Store && request.Status == RequestStatus.PendingDestination;

        request.CurrentStep!.Decide(RequestStepDecision.Rejected, decider.UserId, authority.DelegatedFromUserId, now, reason);
        request.AddTimeline(_guidGenerator.Create(), now, RequestTimelineActions.Rejected, decider.UserId, reason);
        request.Reject(reason);
        await _requests.UpdateAsync(request, autoSave: true);

        if (atStore)
        {
            await _stock.CancelIssueAsync(request);
        }

        foreach (var handler in _handlers)
        {
            await handler.OnRejectedAsync(request);
        }

        await _taskLog.LogAsync(decider.UserId, AdministrativeTaskType.RequestRejected, request.Id);
        await NotifyOwnerAsync(request, NotificationTemplateCode.RequestStatusChanged);
    }

    /// <summary>The store moved the request without deciding it: the request stays at the store, and its owner sees the move.</summary>
    public virtual async Task RecordStoreProgressAsync(Request request, string action, Guid userId, string? note)
    {
        request.AddTimeline(_guidGenerator.Create(), _clock.Now, action, userId, note);
        await _requests.UpdateAsync(request, autoSave: true);
        await NotifyOwnerAsync(request, NotificationTemplateCode.RequestStatusChanged);
    }

    /// <summary>Delivery is the store's execution of the request. QUESTIONS.md item 22: the officer may do this for their own request, and it is recorded.</summary>
    public virtual async Task CompleteAtStoreAsync(Request request, Guid userId, string? note)
    {
        var now = _clock.Now;
        request.CurrentStep!.Decide(RequestStepDecision.Approved, userId, null, now, note);
        request.AddTimeline(_guidGenerator.Create(), now, RequestTimelineActions.StockIssueDelivered, userId, note);
        request.AdvanceToNextStep(now);

        // A millisecond later, because the timeline is read in time order and these two happen together.
        request.AddTimeline(_guidGenerator.Create(), now.AddMilliseconds(1), RequestTimelineActions.Completed, userId, null);
        await _requests.UpdateAsync(request, autoSave: true);
        await NotifyOwnerAsync(request, NotificationTemplateCode.RequestStatusChanged);
    }

    /// <summary>The owner or actual submitter, and only before execution starts.</summary>
    public virtual async Task CancelAsync(Request request, Guid userId, string? note)
    {
        if (request.OwnerUserId != userId && request.SubmittedByUserId != userId)
        {
            throw new AbpAuthorizationException();
        }

        if (!await CanCancelAsync(request))
        {
            throw new BusinessException(HrErrorCodes.Request.CannotCancelAtThisStage);
        }

        var atStore = request.Destination == RequestDestination.Store && request.Status == RequestStatus.PendingDestination;
        var approvers = await GetCurrentApproverUserIdsAsync(request);

        request.AddTimeline(_guidGenerator.Create(), _clock.Now, RequestTimelineActions.Cancelled, userId, note);
        request.Cancel();
        await _requests.UpdateAsync(request, autoSave: true);

        if (atStore)
        {
            await _stock.CancelIssueAsync(request);
        }

        foreach (var handler in _handlers)
        {
            await handler.OnCancelledAsync(request);
        }

        foreach (var approver in approvers.Where(x => x != userId))
        {
            await _notifications.PublishAsync(approver, NotificationTemplateCode.RequestStatusChanged, NotificationResourceType.Request, request.Id);
        }

        if (request.OwnerUserId != userId)
        {
            await NotifyOwnerAsync(request, NotificationTemplateCode.RequestStatusChanged);
        }
    }

    public virtual async Task<bool> CanCancelAsync(Request request) => request.Status switch
    {
        RequestStatus.PendingManager or RequestStatus.PendingHr or RequestStatus.PendingExecutive => true,
        RequestStatus.PendingDestination => request.Destination != RequestDestination.Store || await _stock.CanCancelIssueAsync(request),
        _ => false
    };

    /// <summary>The owner, actual submitter, HR, anyone on the route, and the owner's current manager.</summary>
    public virtual async Task<bool> CanViewAsync(Request request, RequestDeciderContext decider)
    {
        if (request.OwnerUserId == decider.UserId || request.SubmittedByUserId == decider.UserId || decider.Roles.Contains(HrRoles.HrManager))
        {
            return true;
        }

        foreach (var step in request.Steps)
        {
            if (step.DecidedByUserId == decider.UserId)
            {
                return true;
            }

            // A store officer reaches a request only once it has reached their step.
            if (step.ApproverRole == HrRoles.StoreOfficer && step.Decision == RequestStepDecision.NotReached)
            {
                continue;
            }

            if (decider.HoldsDirectly(step.ApproverUserId, step.ApproverRole) || decider.FindDelegator(step.ApproverUserId, step.ApproverRole) != null)
            {
                return true;
            }
        }

        var owner = await _employees.FindAsync(request.EmployeeId);
        if (owner?.ManagerEmployeeId == null)
        {
            return false;
        }

        return await _employees.AnyAsync(x => x.Id == owner.ManagerEmployeeId && x.UserId == decider.UserId);
    }

    /// <summary>Everyone who can act on the current step: the named approver or the role's active holders, plus their active delegates.</summary>
    public virtual async Task<IReadOnlyCollection<Guid>> GetCurrentApproverUserIdsAsync(Request request)
    {
        if (!request.IsPending)
        {
            return Array.Empty<Guid>();
        }

        var holders = request.CurrentApproverUserId != null
            ? new List<Guid> { request.CurrentApproverUserId.Value }
            : (await _userManager.GetUsersInRoleAsync(request.CurrentApproverRole!)).Where(x => x.IsActive).Select(x => x.Id).ToList();

        var now = _clock.Now;
        var delegates = (await _delegations.GetListAsync(x =>
                holders.Contains(x.FromUserId) &&
                x.CancelledAt == null &&
                x.StartsAt <= now &&
                now < x.EndsAt &&
                (x.Scope == DelegationScope.AllApprovals || x.Scope == DelegationScope.Requests)))
            .Select(x => x.ToUserId);

        // The owner is told about their own request separately, and only holds this step at the destination.
        var ownerMayAct = request.Status == RequestStatus.PendingDestination;
        return holders.Where(x => ownerMayAct || x != request.OwnerUserId)
            .Concat(delegates.Where(x => x != request.OwnerUserId))
            .Distinct()
            .ToList();
    }

    private static void CheckPending(Request request)
    {
        if (!request.IsPending)
        {
            throw new BusinessException(HrErrorCodes.Request.NotPending);
        }
    }

    private async Task<Guid> GetLinkedManagerUserIdAsync(Employee owner)
    {
        var manager = owner.ManagerEmployeeId == null ? null : await _employees.FindAsync(owner.ManagerEmployeeId.Value);
        if (manager?.UserId == null || manager.Status != EmployeeStatus.Active)
        {
            throw new BusinessException(HrErrorCodes.Request.NoApproverAvailable);
        }

        var user = await _userManager.FindByIdAsync(manager.UserId.Value.ToString());
        if (user is not { IsActive: true })
        {
            throw new BusinessException(HrErrorCodes.Request.NoApproverAvailable);
        }

        return manager.UserId.Value;
    }

    private async Task CheckRoleHasHolderAsync(string role)
    {
        if (!(await _userManager.GetUsersInRoleAsync(role)).Any(x => x.IsActive))
        {
            throw new BusinessException(HrErrorCodes.Request.NoApproverAvailable);
        }
    }

    /// <summary>The store module tells its own officers, through stockIssuePending, when the issue is created.</summary>
    private async Task AnnounceCurrentStepAsync(Request request)
    {
        if (request.Destination == RequestDestination.Store && request.Status == RequestStatus.PendingDestination)
        {
            await _stock.CreateIssueAsync(request);
            return;
        }

        foreach (var approver in await GetCurrentApproverUserIdsAsync(request))
        {
            await _notifications.PublishAsync(approver, NotificationTemplateCode.RequestPendingAction, NotificationResourceType.Request, request.Id);
        }
    }

    private async Task NotifyOwnerAsync(Request request, NotificationTemplateCode template)
    {
        if (request.OwnerUserId != null)
        {
            await _notifications.PublishAsync(request.OwnerUserId.Value, template, NotificationResourceType.Request, request.Id);
        }
    }
}

/// <summary>When a manager changes, the employee's requests waiting for the old manager move to the new one.</summary>
public class RequestManagerChangeEffects : IManagerChangeEffects, ITransientDependency
{
    private readonly IRepository<Request, Guid> _requests;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly HrNotificationPublisher _notifications;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;
    private readonly Volo.Abp.Users.ICurrentUser _currentUser;

    public RequestManagerChangeEffects(
        IRepository<Request, Guid> requests,
        IRepository<Employee, Guid> employees,
        HrNotificationPublisher notifications,
        IGuidGenerator guidGenerator,
        IClock clock,
        Volo.Abp.Users.ICurrentUser currentUser)
    {
        _requests = requests;
        _employees = employees;
        _notifications = notifications;
        _guidGenerator = guidGenerator;
        _clock = clock;
        _currentUser = currentUser;
    }

    public virtual async Task<int> MovePendingItemsAsync(Guid employeeId, Guid? previousManagerEmployeeId, Guid newManagerEmployeeId)
    {
        var newManagerUserId = (await _employees.GetAsync(newManagerEmployeeId)).UserId;
        if (newManagerUserId == null)
        {
            return 0;
        }

        var query = (await _requests.WithDetailsAsync(x => x.Steps, x => x.Timeline))
            .Where(x => x.EmployeeId == employeeId && x.Status == RequestStatus.PendingManager && x.CurrentApproverUserId != null);
        var pending = await _requests.AsyncExecuter.ToListAsync(query);

        foreach (var request in pending)
        {
            request.ReassignCurrentApprover(newManagerUserId.Value);
            request.AddTimeline(_guidGenerator.Create(), _clock.Now, RequestTimelineActions.ApproverReassigned, _currentUser.Id, null);
            await _requests.UpdateAsync(request, autoSave: true);
            await _notifications.PublishAsync(newManagerUserId.Value, NotificationTemplateCode.RequestPendingAction, NotificationResourceType.Request, request.Id);
        }

        return pending.Count;
    }
}
