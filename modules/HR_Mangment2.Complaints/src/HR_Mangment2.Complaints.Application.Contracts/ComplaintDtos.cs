using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Leaves;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.Complaints;

public class ComplaintRespondentPagedResultDto
{
    public long TotalCount { get; set; }
    public List<EmployeeRefDto> Items { get; set; } = new();
}

public class ComplaintRespondentListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}

public class ComplaintRoutingPreviewInputDto
{
    public ComplaintTargetKind? TargetKind { get; set; }
    public Guid? RespondentId { get; set; }
}

public class ComplaintRouteOptionDto
{
    public ComplaintLevel Level { get; set; }
    public List<ComplaintHandlerRole> HandlerPath { get; set; } = new();
}

public class ComplaintRoutingPreviewDto
{
    public ComplaintLevel MinimumLevel { get; set; }
    public List<ComplaintLevel> SelectableLevels { get; set; } = new();
    public List<ComplaintRouteOptionDto> Routes { get; set; } = new();
}

public class ComplaintCreateDto : ComplaintRoutingPreviewInputDto
{
    public ComplaintLevel? Level { get; set; }
    public string Subject { get; set; } = null!;
    public string Content { get; set; } = null!;
}

public class ComplaintSubmitResultDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = null!;
    public ComplaintLevel Level { get; set; }
    public bool LevelRaisedBySystem { get; set; }
    public ComplaintStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public class ComplaintComplainantSummaryDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public ComplaintLevel Level { get; set; }
    public ComplaintStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}

public class ComplaintComplainantSummaryPagedResultDto
{
    public long TotalCount { get; set; }
    public List<ComplaintComplainantSummaryDto> Items { get; set; } = new();
}

public class ComplaintComplainantDetailDto
{
    public string? ClosureDecision { get; set; }

    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = null!;
    public ComplaintTargetKind TargetKind { get; set; }
    public EmployeeRefDto? Respondent { get; set; }
    public ComplaintLevel Level { get; set; }
    public ComplaintStatus Status { get; set; }
    public string Subject { get; set; } = null!;
    public string Content { get; set; } = null!;
    public List<AttachmentDto> Attachments { get; set; } = new();
    public DateTime SubmittedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class ComplaintHandlerSummaryDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = null!;
    public ComplaintLevel Level { get; set; }
    public ComplaintStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? DueAt { get; set; }
    public bool IsLate { get; set; }
    public DateTime? ClosedAt { get; set; }
}

public class ComplaintHandlerSummaryPagedResultDto
{
    public long TotalCount { get; set; }
    public List<ComplaintHandlerSummaryDto> Items { get; set; } = new();
}

public class ComplaintActionDto
{
    public Guid Id { get; set; }
    public ComplaintActionType Type { get; set; }
    public UserRefDto Actor { get; set; } = null!;
    public string? Note { get; set; }
    public DateTime CreationTime { get; set; }
}

public class ComplaintHandlerDetailDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = null!;
    public EmployeeRefDto Complainant { get; set; } = null!;
    public ComplaintTargetKind TargetKind { get; set; }
    public EmployeeRefDto? Respondent { get; set; }
    public ComplaintRespondentCategory? RespondentCategory { get; set; }
    public ComplaintLevel Level { get; set; }
    public ComplaintLevel MinimumLevel { get; set; }
    public ComplaintStatus Status { get; set; }
    public UserRefDto? Handler { get; set; }
    public string Subject { get; set; } = null!;
    public string Content { get; set; } = null!;
    public List<AttachmentDto> Attachments { get; set; } = new();
    public List<ComplaintActionDto> Actions { get; set; } = new();
    public DateTime SubmittedAt { get; set; }
    public DateTime? DueAt { get; set; }
    public bool IsLate { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ClosureDecision { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class ComplaintRaiseLevelInputDto
{
    public ComplaintLevel? Level { get; set; }
    public string? Reason { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class ComplaintForwardInputDto
{
    public string? Note { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class ComplaintCloseInputDto
{
    public string Decision { get; set; } = null!;
    public string ConcurrencyStamp { get; set; } = null!;
}

public class ComplaintActionCreateDto
{
    public ComplaintManualActionType? Type { get; set; }
    public string Note { get; set; } = null!;
}

public class ComplaintTransitionResultDto
{
    public Guid Id { get; set; }
    public ComplaintLevel Level { get; set; }
    public ComplaintStatus Status { get; set; }
    public DateTime? ClosedAt { get; set; }
    public bool CallerRetainsAccess { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class ComplaintAccessLogEntryDto
{
    public Guid Id { get; set; }
    public UserRefDto User { get; set; } = null!;
    public ComplaintAccessAction Action { get; set; }
    public DateTime At { get; set; }
}

public class ComplaintAccessLogEntryPagedResultDto
{
    public long TotalCount { get; set; }
    public List<ComplaintAccessLogEntryDto> Items { get; set; } = new();
}

public class ComplaintMyListInput : PagedAndSortedResultRequestDto
{
    public ComplaintStatus? Status { get; set; }
    public ComplaintLevel? Level { get; set; }
}

public class ComplaintAssignedListInput : ComplaintMyListInput
{
    public bool? IsLate { get; set; }

    /// <summary>Only complaints filed by this employee, still within the handler's own scope (decision 2026-11-10).</summary>
    public Guid? ComplainantEmployeeId { get; set; }
}

public class ComplaintAttachmentUploadInput
{
    public byte[] Content { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}

public class ComplaintAttachmentFile
{
    public byte[] Content { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}

public interface IComplaintAppService : IApplicationService
{
    Task<ComplaintRespondentPagedResultDto> GetRespondentsAsync(ComplaintRespondentListInput input);

    Task<ComplaintRoutingPreviewDto> PreviewRoutingAsync(ComplaintRoutingPreviewInputDto input);

    Task<ComplaintSubmitResultDto> SubmitAsync(ComplaintCreateDto input, Guid? idempotencyKey);

    Task<ComplaintComplainantSummaryPagedResultDto> GetMyListAsync(ComplaintMyListInput input);

    Task<ComplaintComplainantDetailDto> GetMyDetailAsync(Guid id);

    Task<ComplaintHandlerSummaryPagedResultDto> GetAssignedListAsync(ComplaintAssignedListInput input);

    Task<ComplaintHandlerDetailDto> GetDetailAsync(Guid id);

    Task<ComplaintTransitionResultDto> RaiseLevelAsync(Guid id, ComplaintRaiseLevelInputDto input);

    Task<ComplaintActionDto> AddActionAsync(Guid id, ComplaintActionCreateDto input, Guid? idempotencyKey);

    Task<ComplaintTransitionResultDto> ForwardAsync(Guid id, ComplaintForwardInputDto input);

    Task<ComplaintTransitionResultDto> CloseAsync(Guid id, ComplaintCloseInputDto input);

    Task<ComplaintAccessLogEntryPagedResultDto> GetAccessLogAsync(Guid id, PagedAndSortedResultRequestDto input);

    Task<AttachmentDto> UploadAttachmentAsync(Guid id, ComplaintAttachmentUploadInput input, Guid? idempotencyKey);

    Task<ComplaintAttachmentFile> DownloadAttachmentAsync(Guid id, Guid attachmentId);
}

public class ComplaintHandlerAssignmentCreateDto
{
    public Guid? UserId { get; set; }
    public ComplaintLevel? Level { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string Reason { get; set; } = null!;
}

public class ComplaintHandlerAssignmentDto
{
    public Guid Id { get; set; }
    public UserRefDto User { get; set; } = null!;
    public ComplaintLevel Level { get; set; }
    public ComplaintHandlerAssignmentStatus Status { get; set; }
    public DateTime From { get; set; }
    public DateTime? To { get; set; }
    public string Reason { get; set; } = null!;
    public UserRefDto RequestedBy { get; set; } = null!;
    public DateTime RequestedAt { get; set; }
    public UserRefDto? DecidedBy { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? DecisionReason { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class ComplaintHandlerAssignmentPagedResultDto
{
    public long TotalCount { get; set; }
    public List<ComplaintHandlerAssignmentDto> Items { get; set; } = new();
}

public class ComplaintHandlerAssignmentApproveDto
{
    public string ConcurrencyStamp { get; set; } = null!;
}

public class ComplaintHandlerAssignmentReasonDto
{
    public string Reason { get; set; } = null!;
    public string ConcurrencyStamp { get; set; } = null!;
}

public class ComplaintHandlerListInput : PagedAndSortedResultRequestDto
{
    public ComplaintHandlerAssignmentStatus? Status { get; set; }
    public ComplaintLevel? Level { get; set; }
}

public interface IComplaintHandlerAppService : IApplicationService
{
    Task<ComplaintHandlerAssignmentDto> RequestAsync(ComplaintHandlerAssignmentCreateDto input, Guid? idempotencyKey);

    Task<ComplaintHandlerAssignmentPagedResultDto> GetListAsync(ComplaintHandlerListInput input);

    Task<ComplaintHandlerAssignmentDto> ApproveAsync(Guid id, ComplaintHandlerAssignmentApproveDto input);

    Task<ComplaintHandlerAssignmentDto> RejectAsync(Guid id, ComplaintHandlerAssignmentReasonDto input);

    Task<ComplaintHandlerAssignmentDto> RevokeAsync(Guid id, ComplaintHandlerAssignmentReasonDto input);
}
