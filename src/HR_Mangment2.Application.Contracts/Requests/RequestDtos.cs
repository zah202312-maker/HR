using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Store;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.Requests;

public class RequestTypeRefDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public RequestDestination Destination { get; set; }
}

public class RequestTypeDto : RequestTypeRefDto
{
    public bool UsesDedicatedEndpoint { get; set; }
    public bool RequiresItem { get; set; }
}

public class RequestTypePagedResultDto
{
    public long TotalCount { get; set; }
    public List<RequestTypeDto> Items { get; set; } = new();
}

public class RequestTypeAdminDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public RequestDestination Destination { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystem { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}

public class RequestTypeAdminPagedResultDto
{
    public long TotalCount { get; set; }
    public List<RequestTypeAdminDto> Items { get; set; } = new();
}

public class RequestTypeCreateInputDto
{
    public string Code { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public RequestDestination? Destination { get; set; }
    public bool? IsActive { get; set; }
}

public class RequestTypeUpdateInputDto
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public RequestDestination? Destination { get; set; }
    public bool? IsActive { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class RequestTypeAdminListInput : PagedAndSortedResultRequestDto
{
    public bool? IsActive { get; set; }
}

public class RequestSubmitInputDto
{
    public Guid? RequestTypeId { get; set; }
    public Guid? OnBehalfOfEmployeeId { get; set; }
    public string Description { get; set; } = null!;
    public Guid? ItemId { get; set; }
    public decimal? Quantity { get; set; }
}

public class NoteInputDto
{
    public string? Note { get; set; }
}

public class TimelineEntryDto
{
    public DateTime OccurredAt { get; set; }
    public string Action { get; set; } = null!;
    public UserRefDto? Actor { get; set; }

    /// <summary>Absent, not null, for an audience that sees only the type of an accountant request.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Note { get; set; }
}

public class RequestStepDto
{
    public int Order { get; set; }
    public AppRole ApproverRole { get; set; }
    public UserRefDto? Approver { get; set; }
    public RequestStepDecision Decision { get; set; }
    public UserRefDto? DecidedBy { get; set; }
    public UserRefDto? DelegatedFrom { get; set; }
    public DateTime? DecidedAt { get; set; }

    /// <summary>Absent, not null, for an audience that sees only the type of an accountant request.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Note { get; set; }
}

public class ItemRefDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public ItemKind Kind { get; set; }
}

public class StockIssueStatusDto
{
    public Guid Id { get; set; }
    public ItemRefDto Item { get; set; } = null!;
    public decimal Quantity { get; set; }
    public StockIssueStatus Status { get; set; }
    public string StatusLabel { get; set; } = null!;
    public DateTime? DeliveredAt { get; set; }
    public Guid? CustodyId { get; set; }
}

public class RequestDetailsDto
{
    public Guid Id { get; set; }
    public RequestTypeRefDto Type { get; set; } = null!;
    public EmployeeRefDto Employee { get; set; } = null!;
    public UserRefDto SubmittedBy { get; set; } = null!;
    public bool IsOnBehalf { get; set; }

    /// <summary>Absent for an audience that sees only the type of an accountant request.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    /// <summary>Absent for an audience that sees only the type of an accountant request.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DetailsSummary { get; set; }

    public RequestStatus Status { get; set; }
    public RequestEmployeeStatus EmployeeStatus { get; set; }
    public string StatusLabel { get; set; } = null!;
    public AppRole? CurrentApproverRole { get; set; }

    /// <summary>The same text as the rejecting step's note, so it is absent for the same audience.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RejectionReason { get; set; }

    public List<RequestStepDto> Steps { get; set; } = new();
    public List<TimelineEntryDto> Timeline { get; set; } = new();
    public StockIssueStatusDto? StockIssue { get; set; }
    public Guid? SourceMemoId { get; set; }
    public bool CanApprove { get; set; }
    public bool CanReject { get; set; }
    public bool CanCancel { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}

public class RequestSummaryDto
{
    public Guid Id { get; set; }
    public RequestTypeRefDto Type { get; set; } = null!;
    public EmployeeRefDto Employee { get; set; } = null!;
    public UserRefDto SubmittedBy { get; set; } = null!;
    public bool IsOnBehalf { get; set; }

    /// <summary>Absent for an audience that sees only the type of an accountant request.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DetailsSummary { get; set; }

    public RequestStatus Status { get; set; }
    public AppRole? CurrentApproverRole { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}

public class RequestSummaryPagedResultDto
{
    public long TotalCount { get; set; }
    public List<RequestSummaryDto> Items { get; set; } = new();
}

public class MyRequestDto
{
    public Guid Id { get; set; }
    public RequestTypeRefDto Type { get; set; } = null!;
    public string DetailsSummary { get; set; } = null!;
    public RequestEmployeeStatus EmployeeStatus { get; set; }
    public string StatusLabel { get; set; } = null!;
    public AppRole? CurrentStageRole { get; set; }
    public string? StockIssueStatusLabel { get; set; }
    public string? RejectionReason { get; set; }
    public bool IsOnBehalf { get; set; }
    public UserRefDto SubmittedBy { get; set; } = null!;
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}

public class MyRequestPagedResultDto
{
    public long TotalCount { get; set; }
    public List<MyRequestDto> Items { get; set; } = new();
}

public class RequestInboxItemDto
{
    public Guid Id { get; set; }
    public RequestTypeRefDto Type { get; set; } = null!;
    public EmployeeRefDto Employee { get; set; } = null!;
    public UserRefDto SubmittedBy { get; set; } = null!;
    public bool IsOnBehalf { get; set; }

    /// <summary>Absent for an audience that sees only the type of an accountant request.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DetailsSummary { get; set; }

    public RequestStatus Status { get; set; }
    public AppRole CurrentApproverRole { get; set; }
    public RequestInboxActionKind ActionKind { get; set; }
    public Guid? StockIssueId { get; set; }
    public UserRefDto? DelegatedFrom { get; set; }
    public DateTime WaitingSince { get; set; }
    public DateTime CreationTime { get; set; }
}

public class RequestInboxItemPagedResultDto
{
    public long TotalCount { get; set; }
    public List<RequestInboxItemDto> Items { get; set; } = new();
}

public class RequestListInput : PagedAndSortedResultRequestDto
{
    public RequestStatus? Status { get; set; }
    public Guid? RequestTypeId { get; set; }
    public Guid? EmployeeId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}

public class MyRequestListInput : PagedAndSortedResultRequestDto
{
    public RequestEmployeeStatus? EmployeeStatus { get; set; }
    public Guid? RequestTypeId { get; set; }
}

public class RequestInboxInput : PagedAndSortedResultRequestDto
{
    public Guid? RequestTypeId { get; set; }
    public RequestStatus? Status { get; set; }
}

public enum InboxCategory
{
    Request,
    Memo,
    Incident,
    StockIssue,
    Evaluation,
    DepartmentReport,
    EvaluationObjection,
    EvaluationAcknowledgement,
    CircularAcknowledgement,
    CustodyAcknowledgement,
    WarningResponse,
    WarningDecision,
    ContractDecision,
    AppreciationLetterAcknowledgement
}

public class InboxSummaryItemDto
{
    public InboxCategory Category { get; set; }
    public int Count { get; set; }
    public int DelegatedCount { get; set; }
}

public class InboxSummaryDto
{
    public int TotalCount { get; set; }
    public List<InboxSummaryItemDto> Items { get; set; } = new();
}

public interface IRequestTypeAppService : IApplicationService
{
    Task<RequestTypePagedResultDto> GetListAsync(PagedAndSortedResultRequestDto input);
    Task<RequestTypeAdminDto> CreateAsync(RequestTypeCreateInputDto input);
    Task<RequestTypeAdminPagedResultDto> GetAdminListAsync(RequestTypeAdminListInput input);
    Task<RequestTypeAdminDto> GetAsync(Guid id);
    Task<RequestTypeAdminDto> UpdateAsync(Guid id, RequestTypeUpdateInputDto input);
}

public interface IRequestAppService : IApplicationService
{
    Task<RequestDetailsDto> SubmitAsync(RequestSubmitInputDto input, Guid? idempotencyKey);
    Task<RequestSummaryPagedResultDto> GetListAsync(RequestListInput input);
    Task<MyRequestPagedResultDto> GetMyAsync(MyRequestListInput input);
    Task<RequestInboxItemPagedResultDto> GetInboxAsync(RequestInboxInput input);
    Task<RequestDetailsDto> GetAsync(Guid id);
    Task<RequestDetailsDto> ApproveAsync(Guid id, NoteInputDto input);
    Task<RequestDetailsDto> RejectAsync(Guid id, ReasonInputDto input);
    Task<RequestDetailsDto> CancelAsync(Guid id, NoteInputDto input);
}

public interface IInboxSummaryAppService : IApplicationService
{
    Task<InboxSummaryDto> GetAsync();
}
