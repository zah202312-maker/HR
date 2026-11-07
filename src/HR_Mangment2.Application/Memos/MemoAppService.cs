using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Identity;
using HR_Mangment2.Leaves;
using HR_Mangment2.Notifications;
using HR_Mangment2.Permissions;
using HR_Mangment2.Requests;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace HR_Mangment2.Memos;

[Authorize]
public class MemoAppService : HR_Mangment2AppService, IMemoAppService
{
    private const string CreateOperation = "Memo_Create";

    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["creationTime"] = nameof(Memo.CreationTime),
        ["status"] = nameof(Memo.Status),
        ["kind"] = nameof(Memo.Kind)
    };

    private readonly IRepository<Memo, Guid> _memos;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly ManagerLinkService _managerLinks;
    private readonly IdentityUserManager _userManager;
    private readonly RequestDeciderReader _deciders;
    private readonly MemoIncidentAudience _audience;
    private readonly IncidentCreator _incidents;
    private readonly LeavePlanner _planner;
    private readonly LeaveSubmissionService _leaves;
    private readonly HrNotificationPublisher _notifications;
    private readonly AdministrativeTaskLogger _taskLog;
    private readonly EmployeeReferenceReader _references;
    private readonly IdempotencyStore _idempotency;

    public MemoAppService(
        IRepository<Memo, Guid> memos,
        IRepository<Employee, Guid> employees,
        ManagerLinkService managerLinks,
        IdentityUserManager userManager,
        RequestDeciderReader deciders,
        MemoIncidentAudience audience,
        IncidentCreator incidents,
        LeavePlanner planner,
        LeaveSubmissionService leaves,
        HrNotificationPublisher notifications,
        AdministrativeTaskLogger taskLog,
        EmployeeReferenceReader references,
        IdempotencyStore idempotency)
    {
        _memos = memos;
        _employees = employees;
        _managerLinks = managerLinks;
        _userManager = userManager;
        _deciders = deciders;
        _audience = audience;
        _incidents = incidents;
        _planner = planner;
        _leaves = leaves;
        _notifications = notifications;
        _taskLog = taskLog;
        _references = references;
        _idempotency = idempotency;
    }

    [Authorize(HrPermissions.MemoPermissions.Create)]
    public virtual async Task<MemoDto> CreateAsync(MemoCreateInputDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return await GetAsync(earlier.Value);
        }

        var me = CurrentUser.GetId();
        var writer = await _employees.FirstOrDefaultAsync(x => x.UserId == me) ?? throw new AbpAuthorizationException();
        var row = RequestRoutePlanner.ResolveRow(await _managerLinks.GetRolesAsync(writer));

        // QUESTIONS.md item 24: the executive does not write memos, even when another role grants the permission.
        var stage = MemoRoutePlanner.FirstStage(row) ?? throw new AbpAuthorizationException();
        var handlerUserId = await ResolveFirstHandlerAsync(writer, stage);

        var memo = new Memo(GuidGenerator.Create(), CurrentTenant.Id, writer.Id, me, input.Kind!.Value, input.Content, row);
        memo.ArriveAt(GuidGenerator.Create(), stage.Status, stage.Role, handlerUserId, Clock.Now);
        memo.AddTimeline(GuidGenerator.Create(), Clock.Now, MemoTimelineActions.Submitted, me, null);
        await _memos.InsertAsync(memo, autoSave: true);

        await NotifyCurrentHandlersAsync(memo);
        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, input, memo.Id);
        return await ToDtoAsync(memo);
    }

    [Authorize(HrPermissions.MemoPermissions.ViewOwn)]
    public virtual async Task<MemoListItemPagedResultDto> GetMyAsync(MemoMyListInput input)
    {
        var me = CurrentUser.GetId();
        var query = (await _memos.GetQueryableAsync()).Where(x => x.WriterUserId == me);
        return await ToPageAsync(ApplyFilters(query, input), input, nameof(Memo.CreationTime) + " desc");
    }

    public virtual async Task<MemoListItemPagedResultDto> GetInboxAsync(MemoInboxInput input)
    {
        var decider = await _deciders.GetForMemosAndIncidentsAsync(CurrentUser.GetId());
        await CheckGrantedOrDelegatedAsync(HrPermissions.MemoPermissions.Process, decider);

        var memos = await _memos.GetQueryableAsync();
        var query = input.Status == null ? MemoInboxQuery.WaitingFor(memos, decider) : MemoInboxQuery.PassedThrough(memos, decider);

        if (input.EmployeeId != null)
        {
            query = query.Where(x => x.EmployeeId == input.EmployeeId);
        }

        return await ToPageAsync(ApplyFilters(query, input), input, nameof(Memo.CreationTime));
    }

    [Authorize(HrPermissions.MemoPermissions.View)]
    public virtual async Task<MemoDto> GetAsync(Guid id)
    {
        var memo = await LoadAsync(id);
        if (!CanView(memo, await _deciders.GetForMemosAndIncidentsAsync(CurrentUser.GetId())))
        {
            throw new AbpAuthorizationException();
        }

        return await ToDtoAsync(memo);
    }

    public virtual async Task<MemoDto> ForwardAsync(Guid id, NoteInputDto input)
    {
        var decider = await _deciders.GetForMemosAndIncidentsAsync(CurrentUser.GetId());
        await CheckGrantedOrDelegatedAsync(HrPermissions.MemoPermissions.Forward, decider);

        var memo = await LoadAsync(id);
        CheckCanAct(memo, decider);

        if (!memo.CanForwardToHr)
        {
            throw new BusinessException(HrErrorCodes.Memo.InvalidState);
        }

        var now = Clock.Now;
        memo.ArriveAt(GuidGenerator.Create(), MemoStatus.PendingHr, HrRoles.HrManager, null, now);
        memo.AddTimeline(GuidGenerator.Create(), now, MemoTimelineActions.ForwardedToHr, decider.UserId, input.Note);
        await _memos.UpdateAsync(memo, autoSave: true);

        await _taskLog.LogAsync(decider.UserId, AdministrativeTaskType.MemoProcessed, memo.Id);
        await NotifyCurrentHandlersAsync(memo);
        await NotifyWriterAsync(memo);
        return await ToDtoAsync(memo);
    }

    public virtual async Task<MemoDto> ConvertAsync(Guid id, MemoConvertInputDto input)
    {
        var decider = await _deciders.GetForMemosAndIncidentsAsync(CurrentUser.GetId());
        await CheckGrantedOrDelegatedAsync(HrPermissions.MemoPermissions.Process, decider);

        var memo = await LoadAsync(id);
        CheckCanAct(memo, decider);

        var toLeave = input.Target == MemoConvertTarget.LeaveRequest;
        if (toLeave != (memo.Kind == MemoKind.Leave))
        {
            throw new BusinessException(HrErrorCodes.Memo.ConvertTargetMismatch);
        }

        var writer = await _employees.GetAsync(memo.EmployeeId);
        var now = Clock.Now;

        if (toLeave)
        {
            var leave = await SubmitLeaveAsync(memo, writer, input, decider.UserId);
            memo.MarkConvertedToRequest(leave.Id);
            memo.AddTimeline(GuidGenerator.Create(), now, MemoTimelineActions.ConvertedToRequest, decider.UserId, input.Note);
        }
        else
        {
            if (input.Leave != null)
            {
                throw new BusinessException(HrErrorCodes.Memo.LeaveDetailsInvalid);
            }

            var incident = await _incidents.CreateAsync(writer, memo.WriterUserId, input.ItemId, memo.Content, null, memo.Id, decider.UserId);
            memo.MarkConvertedToIncident(incident.Id);
            memo.AddTimeline(GuidGenerator.Create(), now, MemoTimelineActions.ConvertedToIncident, decider.UserId, input.Note);
        }

        await _memos.UpdateAsync(memo, autoSave: true);

        await _taskLog.LogAsync(decider.UserId, AdministrativeTaskType.MemoProcessed, memo.Id);
        await NotifyWriterAsync(memo);
        return await ToDtoAsync(memo);
    }

    public virtual async Task<MemoDto> CloseAsync(Guid id, ReasonInputDto input)
    {
        var decider = await _deciders.GetForMemosAndIncidentsAsync(CurrentUser.GetId());
        await CheckGrantedOrDelegatedAsync(HrPermissions.MemoPermissions.Close, decider);

        var memo = await LoadAsync(id);
        CheckCanAct(memo, decider);

        if (!memo.IsAtLastStage)
        {
            throw new BusinessException(HrErrorCodes.Memo.InvalidState);
        }

        memo.Close(input.Reason);
        memo.AddTimeline(GuidGenerator.Create(), Clock.Now, MemoTimelineActions.Closed, decider.UserId, input.Reason);
        await _memos.UpdateAsync(memo, autoSave: true);

        await _taskLog.LogAsync(decider.UserId, AdministrativeTaskType.MemoProcessed, memo.Id);
        await NotifyWriterAsync(memo);
        return await ToDtoAsync(memo);
    }

    private async Task<Guid?> ResolveFirstHandlerAsync(Employee writer, PlannedMemoStage stage)
    {
        if (!stage.IsLinkedManager)
        {
            if (!(await _userManager.GetUsersInRoleAsync(stage.Role)).Any(x => x.IsActive))
            {
                throw new BusinessException(HrErrorCodes.Memo.NoManagerLinked);
            }

            return null;
        }

        var manager = writer.ManagerEmployeeId == null ? null : await _employees.FindAsync(writer.ManagerEmployeeId.Value);
        if (manager?.UserId == null || manager.Status != EmployeeStatus.Active)
        {
            throw new BusinessException(HrErrorCodes.Memo.NoManagerLinked);
        }

        var user = await _userManager.FindByIdAsync(manager.UserId.Value.ToString());
        return user is { IsActive: true } ? manager.UserId : throw new BusinessException(HrErrorCodes.Memo.NoManagerLinked);
    }

    /// <summary>Own memo first, then 403 for a stranger, then the state: someone it already passed through is told it moved on.</summary>
    private static void CheckCanAct(Memo memo, RequestDeciderContext decider)
    {
        if (memo.WriterUserId == decider.UserId)
        {
            throw new BusinessException(HrErrorCodes.Memo.SelfActionNotAllowed);
        }

        if (!CanView(memo, decider))
        {
            throw new AbpAuthorizationException();
        }

        var holdsNow = !memo.IsFinal &&
                       (decider.HoldsDirectly(memo.CurrentHandlerUserId, memo.CurrentHandlerRole!) ||
                        decider.FindDelegator(memo.CurrentHandlerUserId, memo.CurrentHandlerRole!) != null);

        if (!holdsNow)
        {
            throw new BusinessException(HrErrorCodes.Memo.InvalidState);
        }
    }

    private static bool CanView(Memo memo, RequestDeciderContext decider) =>
        memo.WriterUserId == decider.UserId ||
        memo.Handlers.Any(x => decider.HoldsDirectly(x.HandlerUserId, x.HandlerRole) || decider.FindDelegator(x.HandlerUserId, x.HandlerRole) != null);

    /// <summary>A delegation carries the right to act, and any active employee can receive one.</summary>
    private async Task CheckGrantedOrDelegatedAsync(string permission, RequestDeciderContext decider)
    {
        if (decider.Delegators.Count == 0)
        {
            await AuthorizationService.CheckAsync(permission);
        }
    }

    private async Task<LeaveRequest> SubmitLeaveAsync(Memo memo, Employee writer, MemoConvertInputDto input, Guid actorUserId)
    {
        var details = input.Leave;
        if (details == null || input.ItemId != null || !AreDetailsComplete(details))
        {
            throw new BusinessException(HrErrorCodes.Memo.LeaveDetailsInvalid);
        }

        LeavePlan plan;
        try
        {
            plan = await _planner.PlanAsync(writer, new LeavePlanInput(
                details.Kind!.Value, details.LeaveDate, details.FromTime, details.ToTime, details.StartDate, details.EndDate, details.OtherLeaveTypeId));
        }
        catch (EntityNotFoundException)
        {
            throw new BusinessException(HrErrorCodes.Memo.LeaveDetailsInvalid);
        }

        // A type that needs a document cannot be converted: the employee submits it directly with the document.
        if (plan.OtherLeaveType is { RequiresAttachment: true })
        {
            throw new BusinessException(HrErrorCodes.Memo.LeaveAttachmentRequired);
        }

        if (plan.Issues.Count > 0)
        {
            if (plan.Issues[0] == HrErrorCodes.Leave.InsufficientBalance)
            {
                throw new BusinessException(HrErrorCodes.Leave.InsufficientBalance)
                    .WithData("availableDays", plan.AvailableDaysBefore)
                    .WithData("requestedDays", plan.AnnualDays)
                    .WithData("shortageDays", plan.ShortageDays);
            }

            throw new BusinessException(HrErrorCodes.Memo.LeaveDetailsInvalid);
        }

        return await _leaves.SubmitAsync(new LeaveSubmission(
            writer, plan, actorUserId, false, null, Array.Empty<LeaveAttachment>(), memo.Id, actorUserId));
    }

    private static bool AreDetailsComplete(MemoLeaveConversionDto details) => details.Kind switch
    {
        LeaveKind.Hourly => details.LeaveDate != null && details.FromTime != null && details.ToTime != null &&
                            details.StartDate == null && details.EndDate == null && details.OtherLeaveTypeId == null,
        LeaveKind.Annual => details.StartDate != null && details.EndDate != null && details.OtherLeaveTypeId == null &&
                            details.LeaveDate == null && details.FromTime == null && details.ToTime == null,
        LeaveKind.Other => details.StartDate != null && details.EndDate != null && details.OtherLeaveTypeId != null &&
                           details.LeaveDate == null && details.FromTime == null && details.ToTime == null,
        _ => false
    };

    private async Task NotifyCurrentHandlersAsync(Memo memo)
    {
        foreach (var recipient in await _audience.GetAsync(memo.CurrentHandlerRole!, memo.CurrentHandlerUserId, memo.WriterUserId))
        {
            await _notifications.PublishAsync(recipient, NotificationTemplateCode.MemoUpdate, NotificationResourceType.Memo, memo.Id);
        }
    }

    private Task NotifyWriterAsync(Memo memo) =>
        _notifications.PublishAsync(memo.WriterUserId, NotificationTemplateCode.MemoUpdate, NotificationResourceType.Memo, memo.Id);

    private async Task<Memo> LoadAsync(Guid id)
    {
        var query = (await _memos.WithDetailsAsync(x => x.Handlers, x => x.Timeline)).Where(x => x.Id == id);
        return await AsyncExecuter.FirstOrDefaultAsync(query) ?? throw new EntityNotFoundException(typeof(Memo), id);
    }

    private static IQueryable<Memo> ApplyFilters(IQueryable<Memo> query, MemoMyListInput input)
    {
        if (input.Status != null)
        {
            query = query.Where(x => x.Status == input.Status);
        }

        if (input.Kind != null)
        {
            query = query.Where(x => x.Kind == input.Kind);
        }

        return query;
    }

    private async Task<MemoListItemPagedResultDto> ToPageAsync(IQueryable<Memo> query, MemoMyListInput input, string defaultSorting)
    {
        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, defaultSorting, SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));
        var employees = await _references.GetRefsAsync(items.Select(x => x.EmployeeId));

        return new MemoListItemPagedResultDto
        {
            TotalCount = total,
            Items = items.Select(x => new MemoListItemDto
            {
                Id = x.Id,
                Employee = employees[x.EmployeeId],
                Kind = x.Kind,
                ContentPreview = x.Content.Length <= Memo.PreviewLength ? x.Content : x.Content[..Memo.PreviewLength],
                Status = x.Status,
                StatusLabel = StatusLabel(x),
                CreationTime = x.CreationTime
            }).ToList()
        };
    }

    private async Task<MemoDto> ToDtoAsync(Memo memo)
    {
        var timeline = memo.Timeline.OrderBy(x => x.OccurredAt).ThenBy(x => x.Id).ToList();
        var users = await _references.GetUserRefsAsync(timeline.Where(x => x.ActorUserId != null).Select(x => x.ActorUserId!.Value));
        var employees = await _references.GetRefsAsync(new[] { memo.EmployeeId });

        return new MemoDto
        {
            Id = memo.Id,
            Employee = employees[memo.EmployeeId],
            Kind = memo.Kind,
            Content = memo.Content,
            Status = memo.Status,
            StatusLabel = StatusLabel(memo),
            CurrentHandlerRole = memo.CurrentHandlerRole == null ? null : Enum.Parse<AppRole>(memo.CurrentHandlerRole, ignoreCase: true),
            CloseReason = memo.CloseReason,
            ConvertedRequestId = memo.ConvertedRequestId,
            ConvertedIncidentId = memo.ConvertedIncidentId,
            Timeline = timeline.Select(x => new TimelineEntryDto
            {
                OccurredAt = x.OccurredAt,
                Action = x.Action,
                Actor = x.ActorUserId == null ? null : users[x.ActorUserId.Value],
                Note = x.Note
            }).ToList(),
            CreationTime = memo.CreationTime,
            LastModificationTime = memo.LastModificationTime
        };
    }

    private string StatusLabel(Memo memo) =>
        memo.Status == MemoStatus.PendingManager && memo.CurrentHandlerRole == HrRoles.SystemAdmin
            ? L["Memo:Status:pendingSystemAdmin"]
            : L["Memo:Status:" + JsonNamingPolicy.CamelCase.ConvertName(memo.Status.ToString())];
}

/// <summary>The single definition of a memo's inbox, shared by the list and the summary counts.</summary>
public static class MemoInboxQuery
{
    /// <summary>What sits with this user now, directly or through a delegation. A writer never handles their own memo.</summary>
    public static IQueryable<Memo> WaitingFor(IQueryable<Memo> query, RequestDeciderContext decider)
    {
        var me = decider.UserId;
        var myRoles = decider.Roles.ToList();
        var delegatorIds = decider.Delegators.Keys.ToList();
        var delegatorRoles = decider.Delegators.Values.SelectMany(x => x).Distinct().ToList();

        return query.Where(x =>
            x.CurrentHandlerRole != null &&
            x.WriterUserId != me &&
            (
                x.CurrentHandlerUserId == me ||
                (x.CurrentHandlerUserId != null && delegatorIds.Contains(x.CurrentHandlerUserId.Value)) ||
                (x.CurrentHandlerUserId == null && (myRoles.Contains(x.CurrentHandlerRole) || delegatorRoles.Contains(x.CurrentHandlerRole)))
            ));
    }

    /// <summary>Everything that has reached this user, wherever it is now.</summary>
    public static IQueryable<Memo> PassedThrough(IQueryable<Memo> query, RequestDeciderContext decider)
    {
        var me = decider.UserId;
        var myRoles = decider.Roles.ToList();
        var delegatorIds = decider.Delegators.Keys.ToList();
        var delegatorRoles = decider.Delegators.Values.SelectMany(x => x).Distinct().ToList();

        return query.Where(x =>
            x.WriterUserId != me &&
            x.Handlers.Any(h =>
                h.HandlerUserId == me ||
                (h.HandlerUserId != null && delegatorIds.Contains(h.HandlerUserId.Value)) ||
                (h.HandlerUserId == null && (myRoles.Contains(h.HandlerRole) || delegatorRoles.Contains(h.HandlerRole)))));
    }
}

public class MemoCreateInputDtoValidator : AbstractValidator<MemoCreateInputDto>
{
    public MemoCreateInputDtoValidator()
    {
        RuleFor(x => x.Kind).NotNull();
        RuleFor(x => x.Content).NotNull().Length(1, Memo.MaxContentLength);
    }
}

public class MemoConvertInputDtoValidator : AbstractValidator<MemoConvertInputDto>
{
    public MemoConvertInputDtoValidator()
    {
        RuleFor(x => x.Target).NotNull();
        RuleFor(x => x.Note).MaximumLength(Memo.MaxNoteLength);
    }
}
