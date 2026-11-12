using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Requests;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.Leaves;

public class LeaveHourlyBalanceDto
{
    public bool IsConfigured { get; set; }
    public string Month { get; set; } = null!;
    public decimal QuotaHours { get; set; }
    public decimal UsedHours { get; set; }
    public decimal HeldHours { get; set; }
    public decimal RemainingHours { get; set; }
    public decimal? HoursPerDay { get; set; }
}

public class OtherLeaveUsageDto
{
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = null!;
    public decimal UsedDays { get; set; }
    public decimal HeldDays { get; set; }
    public decimal? MaxDays { get; set; }
    public decimal? RemainingDays { get; set; }
}

public class LeaveBalanceDto
{
    public EmployeeRefDto Employee { get; set; } = null!;
    public bool IsEntitlementSet { get; set; }
    public Guid? LeaveYearId { get; set; }
    public DateOnly YearStartDate { get; set; }
    public DateOnly YearEndDate { get; set; }
    public DateOnly NextYearStartDate { get; set; }
    public decimal CurrentYearEntitlementDays { get; set; }
    public decimal CarriedDays { get; set; }
    public decimal AdjustmentDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal HeldDays { get; set; }
    public decimal RemainingDays { get; set; }
    public decimal CarriedRemainingDays { get; set; }
    public decimal CurrentYearRemainingDays { get; set; }
    public decimal FutureYearHeldDays { get; set; }
    public decimal? NextYearEntitlementDays { get; set; }
    public LeaveHourlyBalanceDto Hourly { get; set; } = null!;
    public List<OtherLeaveUsageDto> OtherLeaveUsages { get; set; } = new();
    public string ConcurrencyStamp { get; set; } = null!;
}


public class LeaveBalanceAdjustDto
{
    public decimal? Days { get; set; }
    public string Reason { get; set; } = null!;
}

public class LeaveLedgerEntryDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid LeaveYearId { get; set; }
    public DateOnly YearStartDate { get; set; }
    public DateOnly YearEndDate { get; set; }
    public LeaveLedgerType Type { get; set; }
    public LeaveLedgerBucket Bucket { get; set; }
    public decimal? Days { get; set; }
    public decimal? Hours { get; set; }
    public Guid? RequestId { get; set; }
    public string? Reason { get; set; }
    public UserRefDto? CreatedBy { get; set; }
    public DateTime CreationTime { get; set; }
}

public class LeaveLedgerEntryPagedResultDto
{
    public long TotalCount { get; set; }
    public List<LeaveLedgerEntryDto> Items { get; set; } = new();
}

public class LeaveLedgerListInput : PagedAndSortedResultRequestDto
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public Guid? LeaveYearId { get; set; }
    public LeaveLedgerType? Type { get; set; }
}

public class LeaveRequestPreviewInputDto
{
    public Guid? EmployeeId { get; set; }
    public LeaveKind? Kind { get; set; }
    public DateOnly? Date { get; set; }
    public TimeOnly? FromTime { get; set; }
    public TimeOnly? ToTime { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public Guid? OtherLeaveTypeId { get; set; }
}

public class LeaveRequestCreateDto : LeaveRequestPreviewInputDto
{
    public List<Guid> AttachmentIds { get; set; } = new();
    public string? Notes { get; set; }
}

public class LeaveYearSegmentDto
{
    public DateOnly YearStartDate { get; set; }
    public DateOnly YearEndDate { get; set; }
    public bool IsFutureYear { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal WorkingDays { get; set; }
    public decimal FromCarriedDays { get; set; }
    public decimal FromEntitlementDays { get; set; }
}

public class LeaveDayBreakdownDto
{
    public DateOnly Date { get; set; }
    public LeaveDayClassification Classification { get; set; }
    public string? HolidayName { get; set; }
    public bool IsCounted { get; set; }
}

public class LeavePreviewIssueDto
{
    public string Code { get; set; } = null!;
    public string Message { get; set; } = null!;
}

public class LeaveRequestPreviewDto
{
    public LeaveKind Kind { get; set; }
    public int CalendarDays { get; set; }
    public decimal WorkingDays { get; set; }
    public int ExcludedWeeklyOffDays { get; set; }
    public int ExcludedHolidayDays { get; set; }
    public decimal? RequestedHours { get; set; }
    public decimal? HourlyQuotaHoursUsed { get; set; }
    public decimal? HourlyExcessHours { get; set; }
    public bool DeductsFromAnnualBalance { get; set; }
    public decimal AnnualDays { get; set; }
    public decimal FromCarriedDays { get; set; }
    public decimal FromEntitlementDays { get; set; }
    public bool? IsPaid { get; set; }
    public bool RequiresAttachment { get; set; }
    public List<LeaveYearSegmentDto> Segments { get; set; } = new();
    public List<LeaveDayBreakdownDto> DayBreakdown { get; set; } = new();
    public decimal AvailableDaysBefore { get; set; }
    public decimal RemainingDaysAfter { get; set; }
    public decimal? AvailableHourlyHoursBefore { get; set; }
    public bool IsBalanceSufficient { get; set; }
    public decimal ShortageDays { get; set; }
    public bool CanSubmit { get; set; }
    public List<LeavePreviewIssueDto> Issues { get; set; } = new();
}

public class OtherLeaveTypeOptionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsPaid { get; set; }
    public decimal? MaxDays { get; set; }
    public bool RequiresAttachment { get; set; }
    public bool DeductsFromAnnualBalance { get; set; }
}

public class OtherLeaveTypeOptionListDto
{
    public List<OtherLeaveTypeOptionDto> Items { get; set; } = new();
}

public class AttachmentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long SizeBytes { get; set; }
    public DateTime CreationTime { get; set; }
}

public class LeaveRequestDto
{
    public Guid RequestId { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public UserRefDto SubmittedBy { get; set; } = null!;
    public bool IsOnBehalf { get; set; }
    public LeaveKind Kind { get; set; }
    public DateOnly? Date { get; set; }
    public TimeOnly? FromTime { get; set; }
    public TimeOnly? ToTime { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public OtherLeaveTypeOptionDto? OtherLeaveType { get; set; }
    public string? Notes { get; set; }
    public RequestStatus Status { get; set; }
    public LeaveBalanceEffect BalanceEffect { get; set; }
    public decimal WorkingDays { get; set; }
    public decimal? RequestedHours { get; set; }
    public decimal? HourlyQuotaHoursUsed { get; set; }
    public decimal? HourlyExcessHours { get; set; }
    public decimal AnnualDays { get; set; }
    public decimal FromCarriedDays { get; set; }
    public decimal FromEntitlementDays { get; set; }
    public List<LeaveYearSegmentDto> Segments { get; set; } = new();
    public List<AttachmentDto> Attachments { get; set; } = new();
    public DateTime CreationTime { get; set; }
}

public class LeaveRequestPagedResultDto
{
    public long TotalCount { get; set; }
    public List<LeaveRequestDto> Items { get; set; } = new();
}

public class LeaveRequestListInput : PagedAndSortedResultRequestDto
{
    public RequestStatus? Status { get; set; }
    public LeaveKind? Kind { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}

public class LeaveCalendarEntryDto
{
    public Guid RequestId { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public LeaveKind Kind { get; set; }
    public string? OtherLeaveTypeName { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly? FromTime { get; set; }
    public TimeOnly? ToTime { get; set; }
    public RequestStatus Status { get; set; }
    public decimal? WorkingDays { get; set; }
    public decimal? RequestedHours { get; set; }
}

public class LeaveCalendarEntryPagedResultDto
{
    public long TotalCount { get; set; }
    public List<LeaveCalendarEntryDto> Items { get; set; } = new();
}

public class LeaveCalendarListInput : PagedAndSortedResultRequestDto
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public Guid? EmployeeId { get; set; }
    public bool? IncludePending { get; set; }
}

public class OtherLeaveTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public bool IsPaid { get; set; }
    public decimal? MaxDays { get; set; }
    public bool RequiresAttachment { get; set; }
    public OtherLeaveEligibleCategory EligibleCategory { get; set; }
    public bool DeductsFromAnnualBalance { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreationTime { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class OtherLeaveTypePagedResultDto
{
    public long TotalCount { get; set; }
    public List<OtherLeaveTypeDto> Items { get; set; } = new();
}

public class OtherLeaveTypeListInput : PagedAndSortedResultRequestDto
{
    public bool? IsActive { get; set; }
}

public class OtherLeaveTypeCreateDto
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public bool? IsPaid { get; set; }
    public decimal? MaxDays { get; set; }
    public bool? RequiresAttachment { get; set; }
    public OtherLeaveEligibleCategory? EligibleCategory { get; set; }
    public bool DeductsFromAnnualBalance { get; set; }
    public bool? IsActive { get; set; }
}

public class OtherLeaveTypeUpdateDto
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public bool? IsPaid { get; set; }
    public decimal? MaxDays { get; set; }
    public bool? RequiresAttachment { get; set; }
    public OtherLeaveEligibleCategory? EligibleCategory { get; set; }
    public bool? DeductsFromAnnualBalance { get; set; }
    public bool? IsActive { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

/// <summary>The upload as the service sees it. The controller builds it from the multipart form.</summary>
public class LeaveAttachmentUploadInput
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}

public interface ILeaveBalanceAppService : IApplicationService
{
    Task<LeaveBalanceDto> GetMyAsync();
    Task<LeaveBalanceDto> GetByEmployeeAsync(Guid employeeId);
    Task<LeaveBalanceDto> AdjustAsync(Guid employeeId, LeaveBalanceAdjustDto input, Guid? idempotencyKey);
    Task<LeaveLedgerEntryPagedResultDto> GetLedgerByEmployeeAsync(Guid employeeId, LeaveLedgerListInput input);
    Task<LeaveLedgerEntryPagedResultDto> GetMyLedgerAsync(LeaveLedgerListInput input);
}

public interface ILeaveRequestAppService : IApplicationService
{
    Task<LeaveRequestDto> ReverseAsync(Guid id, HR_Mangment2.Identity.ReasonInputDto input);
    Task<LeaveRequestPreviewDto> PreviewAsync(LeaveRequestPreviewInputDto input);
    Task<LeaveRequestDto> CreateAsync(LeaveRequestCreateDto input, Guid? idempotencyKey);
    Task<LeaveRequestPagedResultDto> GetMyListAsync(LeaveRequestListInput input);
    Task<LeaveRequestPagedResultDto> GetListByEmployeeAsync(Guid employeeId, LeaveRequestListInput input);
    Task<LeaveRequestDto> GetAsync(Guid id);
    Task<AttachmentDto> UploadAttachmentAsync(LeaveAttachmentUploadInput input);
    Task<FileDownloadUrlDto> GetAttachmentDownloadUrlAsync(Guid id, Guid attachmentId);
    Task<LeaveCalendarEntryPagedResultDto> GetTeamCalendarAsync(LeaveCalendarListInput input);
}

public interface IOtherLeaveTypeAppService : IApplicationService
{
    Task<OtherLeaveTypePagedResultDto> GetListAsync(OtherLeaveTypeListInput input);
    Task<OtherLeaveTypeDto> CreateAsync(OtherLeaveTypeCreateDto input, Guid? idempotencyKey);
    Task<OtherLeaveTypeDto> UpdateAsync(Guid id, OtherLeaveTypeUpdateDto input);
    Task<OtherLeaveTypeOptionListDto> GetAvailableAsync(Guid? employeeId);
}
