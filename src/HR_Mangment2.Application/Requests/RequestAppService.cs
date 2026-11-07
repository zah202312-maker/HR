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
using HR_Mangment2.Localization;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace HR_Mangment2.Requests;

/// <summary>Stock issue details for request screens, supplied by the store module.</summary>
public interface IStockIssueStatusReader
{
    Task<Dictionary<Guid, StockIssueStatusDto>> GetByRequestIdsAsync(IReadOnlyCollection<Guid> requestIds);
}

[Authorize]
public class RequestAppService : HR_Mangment2AppService, IRequestAppService
{
    private const string SubmitOperation = "Request_Submit";
    private const int GeneratedSummaryLength = 200;

    private readonly IRepository<Request, Guid> _requests;
    private readonly IRepository<RequestType, Guid> _types;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly RequestWorkflowService _workflow;
    private readonly RequestDeciderReader _deciders;
    private readonly IRequestStockGateway _stock;
    private readonly IStockIssueStatusReader _stockReader;
    private readonly EmployeeReferenceReader _references;
    private readonly IdempotencyStore _idempotency;
    private readonly ICompanyClock _companyClock;

    public RequestAppService(
        IRepository<Request, Guid> requests,
        IRepository<RequestType, Guid> types,
        IRepository<Employee, Guid> employees,
        RequestWorkflowService workflow,
        RequestDeciderReader deciders,
        IRequestStockGateway stock,
        IStockIssueStatusReader stockReader,
        EmployeeReferenceReader references,
        IdempotencyStore idempotency,
        ICompanyClock companyClock)
    {
        _requests = requests;
        _types = types;
        _employees = employees;
        _workflow = workflow;
        _deciders = deciders;
        _stock = stock;
        _stockReader = stockReader;
        _references = references;
        _idempotency = idempotency;
        _companyClock = companyClock;
    }

    private static Dictionary<string, string> ListSortFields => new()
    {
        ["creationTime"] = nameof(Request.CreationTime),
        ["lastModificationTime"] = nameof(Request.LastModificationTime),
        ["status"] = nameof(Request.Status)
    };

    private static Dictionary<string, string> InboxSortFields => new()
    {
        ["waitingSince"] = nameof(Request.CurrentStepSince),
        ["creationTime"] = nameof(Request.CreationTime),
        ["status"] = nameof(Request.Status)
    };

    [Authorize(HrPermissions.RequestPermissions.Create)]
    public virtual async Task<RequestDetailsDto> SubmitAsync(RequestSubmitInputDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(SubmitOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return await GetAsync(earlier.Value);
        }

        var me = CurrentUser.GetId();
        var type = await _types.GetAsync(input.RequestTypeId!.Value);

        if (!type.IsActive)
        {
            throw new BusinessException(HrErrorCodes.Request.TypeInactive);
        }

        if (RequestTypeCodes.UsesDedicatedEndpoint(type.Code))
        {
            throw new BusinessException(HrErrorCodes.Request.UseDedicatedEndpoint);
        }

        var (owner, isOnBehalf) = await _workflow.ResolveOwnerAsync(me, input.OnBehalfOfEmployeeId);
        await CheckItemFieldsAsync(type, input);

        var summary = input.Description.Length <= GeneratedSummaryLength ? input.Description : input.Description[..GeneratedSummaryLength];
        var request = await _workflow.SubmitAsync(new RequestSubmission(
            type, owner, me, isOnBehalf, input.Description, summary, summary, input.ItemId, input.Quantity));

        await _idempotency.SaveAsync(SubmitOperation, idempotencyKey, input, request.Id);
        return await ToDetailsAsync(request, await _deciders.GetAsync(me));
    }

    /// <summary>HR sees the whole company. A department manager sees the people linked to them, and nothing else widens that.</summary>
    [Authorize(HrPermissions.RequestPermissions.ViewList)]
    public virtual async Task<RequestSummaryPagedResultDto> GetListAsync(RequestListInput input)
    {
        var decider = await _deciders.GetAsync(CurrentUser.GetId());
        var query = await _requests.GetQueryableAsync();

        if (!decider.Roles.Contains(HrRoles.HrManager))
        {
            var meEmployee = await _employees.FirstOrDefaultAsync(x => x.UserId == decider.UserId);
            if (meEmployee == null || !decider.Roles.Contains(HrRoles.DepartmentManager))
            {
                return new RequestSummaryPagedResultDto();
            }

            var team = (await _employees.GetQueryableAsync()).Where(x => x.ManagerEmployeeId == meEmployee.Id).Select(x => x.Id);
            query = query.Where(x => team.Contains(x.EmployeeId));
        }

        if (input.Status != null)
        {
            query = query.Where(x => x.Status == input.Status);
        }

        if (input.RequestTypeId != null)
        {
            query = query.Where(x => x.RequestTypeId == input.RequestTypeId);
        }

        if (input.EmployeeId != null)
        {
            query = query.Where(x => x.EmployeeId == input.EmployeeId);
        }

        if (input.FromDate != null)
        {
            var from = await _companyClock.StartOfDayUtcAsync(input.FromDate.Value);
            query = query.Where(x => x.CreationTime >= from);
        }

        if (input.ToDate != null)
        {
            var to = await _companyClock.StartOfDayUtcAsync(input.ToDate.Value.AddDays(1));
            query = query.Where(x => x.CreationTime < to);
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(Request.CreationTime) + " desc", ListSortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        var refs = await LoadRefsAsync(items);
        return new RequestSummaryPagedResultDto
        {
            TotalCount = total,
            Items = items.Select(x => new RequestSummaryDto
            {
                Id = x.Id,
                Type = refs.Type(x),
                Employee = refs.Employees[x.EmployeeId],
                SubmittedBy = refs.Users[x.SubmittedByUserId],
                IsOnBehalf = x.IsOnBehalf,
                DetailsSummary = CanSeeDetails(x, decider) ? Summary(x) : null,
                Status = x.Status,
                CurrentApproverRole = ToAppRole(x.CurrentApproverRole),
                CreationTime = x.CreationTime,
                LastModificationTime = x.LastModificationTime
            }).ToList()
        };
    }

    [Authorize(HrPermissions.RequestPermissions.ViewOwn)]
    public virtual async Task<MyRequestPagedResultDto> GetMyAsync(MyRequestListInput input)
    {
        var me = CurrentUser.GetId();
        var query = (await _requests.GetQueryableAsync()).Where(x => x.OwnerUserId == me);

        if (input.RequestTypeId != null)
        {
            query = query.Where(x => x.RequestTypeId == input.RequestTypeId);
        }

        if (input.EmployeeStatus != null)
        {
            var statuses = Enum.GetValues<RequestStatus>().Where(x => RequestRoutePlanner.ToEmployeeStatus(x) == input.EmployeeStatus).ToList();
            query = query.Where(x => statuses.Contains(x.Status));
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(Request.CreationTime) + " desc", ListSortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        var refs = await LoadRefsAsync(items);
        var stock = await _stockReader.GetByRequestIdsAsync(items.Select(x => x.Id).ToList());

        return new MyRequestPagedResultDto
        {
            TotalCount = total,
            Items = items.Select(x => new MyRequestDto
            {
                Id = x.Id,
                Type = refs.Type(x),
                DetailsSummary = Summary(x),
                EmployeeStatus = RequestRoutePlanner.ToEmployeeStatus(x.Status),
                StatusLabel = EmployeeStatusLabel(x),
                CurrentStageRole = ToAppRole(x.CurrentApproverRole),
                StockIssueStatusLabel = stock.GetValueOrDefault(x.Id)?.StatusLabel,
                RejectionReason = x.RejectionReason,
                IsOnBehalf = x.IsOnBehalf,
                SubmittedBy = refs.Users[x.SubmittedByUserId],
                CreationTime = x.CreationTime,
                LastModificationTime = x.LastModificationTime
            }).ToList()
        };
    }

    public virtual async Task<RequestInboxItemPagedResultDto> GetInboxAsync(RequestInboxInput input)
    {
        var decider = await _deciders.GetAsync(CurrentUser.GetId());
        await CheckGrantedOrDelegatedAsync(HrPermissions.RequestPermissions.Inbox, decider);

        var query = RequestInboxQuery.Apply(await _requests.GetQueryableAsync(), decider);

        if (input.RequestTypeId != null)
        {
            query = query.Where(x => x.RequestTypeId == input.RequestTypeId);
        }

        if (input.Status != null)
        {
            query = query.Where(x => x.Status == input.Status);
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(Request.CurrentStepSince), InboxSortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        var refs = await LoadRefsAsync(items, items.Select(x => decider.Resolve(x).DelegatedFromUserId).Where(x => x != null).Select(x => x!.Value));
        var stock = await _stockReader.GetByRequestIdsAsync(items.Select(x => x.Id).ToList());

        return new RequestInboxItemPagedResultDto
        {
            TotalCount = total,
            Items = items.Select(x =>
            {
                var delegatedFrom = decider.Resolve(x).DelegatedFromUserId;
                return new RequestInboxItemDto
                {
                    Id = x.Id,
                    Type = refs.Type(x),
                    Employee = refs.Employees[x.EmployeeId],
                    SubmittedBy = refs.Users[x.SubmittedByUserId],
                    IsOnBehalf = x.IsOnBehalf,
                    DetailsSummary = CanSeeDetails(x, decider) ? Summary(x) : null,
                    Status = x.Status,
                    CurrentApproverRole = ToAppRole(x.CurrentApproverRole)!.Value,
                    ActionKind = RequestInboxQuery.IsStockIssue(x) ? RequestInboxActionKind.StockIssue : RequestInboxActionKind.Decide,
                    StockIssueId = stock.GetValueOrDefault(x.Id)?.Id,
                    DelegatedFrom = delegatedFrom == null ? null : refs.Users[delegatedFrom.Value],
                    WaitingSince = x.CurrentStepSince,
                    CreationTime = x.CreationTime
                };
            }).ToList()
        };
    }

    [Authorize(HrPermissions.RequestPermissions.View)]
    public virtual async Task<RequestDetailsDto> GetAsync(Guid id)
    {
        var request = await LoadAsync(id);
        var decider = await _deciders.GetAsync(CurrentUser.GetId());

        if (!await _workflow.CanViewAsync(request, decider))
        {
            throw new AbpAuthorizationException();
        }

        return await ToDetailsAsync(request, decider);
    }

    public virtual async Task<RequestDetailsDto> ApproveAsync(Guid id, NoteInputDto input)
    {
        var decider = await _deciders.GetAsync(CurrentUser.GetId());
        await CheckGrantedOrDelegatedAsync(HrPermissions.RequestPermissions.Decide, decider);

        var request = await LoadAsync(id);
        await _workflow.ApproveAsync(request, decider, input.Note);
        return await ToDetailsAsync(request, decider);
    }

    public virtual async Task<RequestDetailsDto> RejectAsync(Guid id, ReasonInputDto input)
    {
        var decider = await _deciders.GetAsync(CurrentUser.GetId());
        await CheckGrantedOrDelegatedAsync(HrPermissions.RequestPermissions.Reject, decider);

        var request = await LoadAsync(id);
        await _workflow.RejectAsync(request, decider, input.Reason);
        return await ToDetailsAsync(request, decider);
    }

    [Authorize(HrPermissions.RequestPermissions.Cancel)]
    public virtual async Task<RequestDetailsDto> CancelAsync(Guid id, NoteInputDto input)
    {
        var request = await LoadAsync(id);
        await _workflow.CancelAsync(request, CurrentUser.GetId(), input.Note);
        return await ToDetailsAsync(request, await _deciders.GetAsync(CurrentUser.GetId()));
    }

    /// <summary>A delegation carries the right to decide, and any active employee can receive one, so holding the permission cannot be the only way in.</summary>
    private async Task CheckGrantedOrDelegatedAsync(string permission, RequestDeciderContext decider)
    {
        if (decider.Delegators.Count == 0)
        {
            await AuthorizationService.CheckAsync(permission);
        }
    }

    private async Task CheckItemFieldsAsync(RequestType type, RequestSubmitInputDto input)
    {
        if (type.Destination != RequestDestination.Store)
        {
            if (input.ItemId != null || input.Quantity != null)
            {
                throw new BusinessException(HrErrorCodes.Request.ItemFieldsInvalid);
            }

            return;
        }

        if (input.ItemId == null || input.Quantity is null or <= 0)
        {
            throw new BusinessException(HrErrorCodes.Request.ItemFieldsInvalid);
        }

        if (!await _stock.IsItemActiveAsync(input.ItemId.Value))
        {
            throw new BusinessException(HrErrorCodes.Request.ItemInactive);
        }
    }

    private async Task<Request> LoadAsync(Guid id)
    {
        var query = (await _requests.WithDetailsAsync(x => x.Steps, x => x.Timeline)).Where(x => x.Id == id);
        return await AsyncExecuter.FirstOrDefaultAsync(query) ?? throw new EntityNotFoundException(typeof(Request), id);
    }

    /// <summary>
    /// QUESTIONS.md item A: details of a request to the accountant stay with HR, the accountant, the owner and an executive on its route. Everyone
    /// else on its route sees the type only, and holding the department manager or store officer role also removes the
    /// accountant's access, because financial data never reaches those roles however roles combine.
    /// </summary>
    private static bool CanSeeDetails(Request request, RequestDeciderContext decider)
    {
        if (request.Destination != RequestDestination.Accountant || request.OwnerUserId == decider.UserId || decider.Roles.Contains(HrRoles.HrManager))
        {
            return true;
        }

        // Whoever approves must know what they approve (Q45).
        if (decider.Roles.Contains(HrRoles.ExecutiveDirector) && request.Steps.Any(x => x.ApproverRole == HrRoles.ExecutiveDirector))
        {
            return true;
        }

        return decider.Roles.Contains(HrRoles.Accountant)
               && !decider.Roles.Contains(HrRoles.DepartmentManager)
               && !decider.Roles.Contains(HrRoles.StoreOfficer);
    }

    private async Task<RequestDetailsDto> ToDetailsAsync(Request request, RequestDeciderContext decider)
    {
        var steps = request.Steps.OrderBy(x => x.Order).ToList();
        var timeline = request.Timeline.OrderBy(x => x.OccurredAt).ThenBy(x => x.Id).ToList();
        var userIds = steps
            .SelectMany(x => new[] { x.ApproverUserId, x.DecidedByUserId, x.DelegatedFromUserId })
            .Concat(timeline.Select(x => x.ActorUserId))
            .Where(x => x != null)
            .Select(x => x!.Value);

        var refs = await LoadRefsAsync(new[] { request }, userIds);
        var stock = await _stockReader.GetByRequestIdsAsync(new[] { request.Id });
        var details = CanSeeDetails(request, decider);
        var submitterView = request.OwnerUserId == decider.UserId || request.SubmittedByUserId == decider.UserId;

        return new RequestDetailsDto
        {
            Id = request.Id,
            Type = refs.Type(request),
            Employee = refs.Employees[request.EmployeeId],
            SubmittedBy = refs.Users[request.SubmittedByUserId],
            IsOnBehalf = request.IsOnBehalf,
            Description = details ? request.Description : null,
            DetailsSummary = details ? Summary(request) : null,
            Status = request.Status,
            EmployeeStatus = RequestRoutePlanner.ToEmployeeStatus(request.Status),
            StatusLabel = submitterView ? EmployeeStatusLabel(request) : StatusLabel(request),
            CurrentApproverRole = ToAppRole(request.CurrentApproverRole),
            RejectionReason = details ? request.RejectionReason : null,
            Steps = steps.Select(x => new RequestStepDto
            {
                Order = x.Order,
                ApproverRole = ToAppRole(x.ApproverRole)!.Value,
                Approver = x.ApproverUserId == null ? null : refs.Users[x.ApproverUserId.Value],
                Decision = x.Decision,
                DecidedBy = x.DecidedByUserId == null ? null : refs.Users[x.DecidedByUserId.Value],
                DelegatedFrom = x.DelegatedFromUserId == null ? null : refs.Users[x.DelegatedFromUserId.Value],
                DecidedAt = x.DecidedAt,
                Note = details ? x.Note : null
            }).ToList(),
            Timeline = timeline.Select(x => new TimelineEntryDto
            {
                OccurredAt = x.OccurredAt,
                Action = x.Action,
                Actor = x.ActorUserId == null ? null : refs.Users[x.ActorUserId.Value],
                Note = details ? x.Note : null
            }).ToList(),
            StockIssue = stock.GetValueOrDefault(request.Id),
            SourceMemoId = request.SourceMemoId,
            CanApprove = decider.CanApprove(request),
            CanReject = decider.CanReject(request),
            CanCancel = (request.OwnerUserId == decider.UserId || request.SubmittedByUserId == decider.UserId) && await _workflow.CanCancelAsync(request),
            CreationTime = request.CreationTime,
            LastModificationTime = request.LastModificationTime
        };
    }

    private async Task<RequestRefs> LoadRefsAsync(IReadOnlyCollection<Request> requests, IEnumerable<Guid>? moreUserIds = null)
    {
        var typeIds = requests.Select(x => x.RequestTypeId).Distinct().ToList();
        var types = (await _types.GetListAsync(x => typeIds.Contains(x.Id))).ToDictionary(x => x.Id);
        var users = await _references.GetUserRefsAsync(requests.Select(x => x.SubmittedByUserId).Concat(moreUserIds ?? Array.Empty<Guid>()));
        var employees = await _references.GetRefsAsync(requests.Select(x => x.EmployeeId));
        return new RequestRefs(types, employees, users);
    }

    private static string Summary(Request request) => LocalizedName.Pick(request.DetailsSummaryAr, request.DetailsSummaryEn);

    private static AppRole? ToAppRole(string? role) => role == null ? null : Enum.Parse<AppRole>(role, ignoreCase: true);

    private string StatusLabel(Request request) =>
        request.Status == RequestStatus.PendingManager && request.CurrentApproverRole == HrRoles.SystemAdmin
            ? L["Request:Status:pendingSystemAdmin"]
            : L["Request:Status:" + JsonNamingPolicy.CamelCase.ConvertName(request.Status.ToString())];

    /// <summary>A request at its destination already reads as approved to the person who submitted it.</summary>
    private string EmployeeStatusLabel(Request request) =>
        request.Status == RequestStatus.PendingDestination ? L["Request:Status:approved"] : StatusLabel(request);

    private sealed record RequestRefs(
        Dictionary<Guid, RequestType> Types,
        Dictionary<Guid, EmployeeRefDto> Employees,
        Dictionary<Guid, UserRefDto> Users)
    {
        /// <summary>The destination is the one the request was built with, which may differ from the type's current one.</summary>
        public RequestTypeRefDto Type(Request request)
        {
            var type = Types[request.RequestTypeId];
            return new RequestTypeRefDto
            {
                Id = type.Id,
                Code = type.Code,
                Name = LocalizedName.Pick(type.NameAr, type.NameEn),
                Destination = request.Destination
            };
        }
    }
}

/// <summary>The single definition of "waiting for this user", shared by the inbox list and the summary counts.</summary>
public static class RequestInboxQuery
{
    public static bool IsStockIssue(Request request) =>
        request.Destination == RequestDestination.Store && request.Status == RequestStatus.PendingDestination;

    /// <summary>Includes what reached the user through a delegation, and never their own request, except for carrying it out at the destination.</summary>
    public static IQueryable<Request> Apply(IQueryable<Request> query, RequestDeciderContext decider)
    {
        var me = decider.UserId;
        var myRoles = decider.Roles.ToList();
        var delegatorIds = decider.Delegators.Keys.ToList();
        var delegatorRoles = decider.Delegators.Values.SelectMany(x => x).Distinct().ToList();

        return query.Where(x =>
            x.CurrentStepOrder != null &&
            (
                (x.OwnerUserId != me || x.OwnerUserId == null) &&
                (
                    x.CurrentApproverUserId == me ||
                    (x.CurrentApproverUserId != null && delegatorIds.Contains(x.CurrentApproverUserId.Value)) ||
                    (x.CurrentApproverUserId == null && (myRoles.Contains(x.CurrentApproverRole!) || delegatorRoles.Contains(x.CurrentApproverRole!)))
                )
                ||
                (x.OwnerUserId == me && x.Status == RequestStatus.PendingDestination && myRoles.Contains(x.CurrentApproverRole!))
            ));
    }

    /// <summary>The same list, without what arrived only through a delegation.</summary>
    public static IQueryable<Request> ApplyDirectOnly(IQueryable<Request> query, RequestDeciderContext decider) =>
        Apply(query, new RequestDeciderContext(decider.UserId, decider.Roles, new Dictionary<Guid, IReadOnlyCollection<string>>()));
}

public class RequestSubmitInputDtoValidator : AbstractValidator<RequestSubmitInputDto>
{
    public RequestSubmitInputDtoValidator()
    {
        RuleFor(x => x.RequestTypeId).NotNull();
        RuleFor(x => x.Description).NotNull().Length(1, Request.MaxDescriptionLength);
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
    }
}

public class NoteInputDtoValidator : AbstractValidator<NoteInputDto>
{
    public NoteInputDtoValidator()
    {
        RuleFor(x => x.Note).MaximumLength(Request.MaxNoteLength);
    }
}
