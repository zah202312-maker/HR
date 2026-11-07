using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR_Mangment2.Departments;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.Evaluations;

public class EvaluationCriterionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class EvaluationCriterionPagedResultDto
{
    public long TotalCount { get; set; }
    public List<EvaluationCriterionDto> Items { get; set; } = new();
}

public class EvaluationCriterionCreateDto
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
}

public class EvaluationCriterionUpdateDto : EvaluationCriterionCreateDto
{
    public string ConcurrencyStamp { get; set; } = null!;
}

public class EvaluationGradeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class EvaluationGradePagedResultDto
{
    public long TotalCount { get; set; }
    public List<EvaluationGradeDto> Items { get; set; } = new();
}

public class EvaluationGradeUpdateDto
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class EvaluationGradeCreateDto
{
    public string Code { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
}

public class EvaluationScaleListInput : PagedAndSortedResultRequestDto
{
    public bool? IsActive { get; set; }
}

public class EvaluationManagerProgressDto
{
    public int ExpectedCount { get; set; }
    public int SubmittedCount { get; set; }
    public int RemainingCount { get; set; }
    public Guid? ReportId { get; set; }
    public DepartmentReportStatus? ReportStatus { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsLate { get; set; }
}

public class EvaluationCycleDto
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public EvaluationCycleStatus Status { get; set; }
    public DateTime OpensAt { get; set; }
    public DateTime DataFreezeAt { get; set; }
    public DateTime DueAt { get; set; }
    public DateTime EscalatesAt { get; set; }
    public EvaluationManagerProgressDto? ManagerProgress { get; set; }
}

public class EvaluationCyclePagedResultDto
{
    public long TotalCount { get; set; }
    public List<EvaluationCycleDto> Items { get; set; } = new();
}

public class EvaluationCycleListInput : PagedAndSortedResultRequestDto
{
    public int? Year { get; set; }
}

public class EvaluationAttendanceSummaryDto
{
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public int ScheduledWorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateCount { get; set; }
    public int LateMinutes { get; set; }
    public decimal LeaveDays { get; set; }
    public bool IsFrozen { get; set; }
    public DateTime? FrozenAt { get; set; }
}

public class EvaluationGradeOptionDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
}

public class EvaluationDraftScoreDto
{
    public Guid CriterionId { get; set; }
    public string CriterionName { get; set; } = null!;
    public string? GradeCode { get; set; }
    public string? GradeName { get; set; }
}

public class EvaluationSubmittedScoreDto
{
    public Guid CriterionId { get; set; }
    public string CriterionName { get; set; } = null!;
    public string GradeCode { get; set; } = null!;
    public string GradeName { get; set; } = null!;
}

public class EvaluationResponseDto
{
    public Guid Id { get; set; }
    public EvaluationResponseType Type { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreationTime { get; set; }
    public EvaluationObjectionDecision? Decision { get; set; }
    public string? DecisionReason { get; set; }
    public UserRefDto? DecidedBy { get; set; }
    public DateTime? DecidedAt { get; set; }
}

public class EvaluationManagerDto
{
    public Guid Id { get; set; }
    public Guid CycleId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public EvaluationStatus Status { get; set; }
    public bool IsEditable { get; set; }
    public EvaluationAttendanceSummaryDto Attendance { get; set; } = null!;
    public List<EvaluationGradeOptionDto> AvailableGrades { get; set; } = new();
    public List<EvaluationDraftScoreDto> Scores { get; set; } = new();
    public string? EmployeeNotes { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public EvaluationObjectionStatus ObjectionStatus { get; set; }
    public List<EvaluationResponseDto> Responses { get; set; } = new();
    public string ConcurrencyStamp { get; set; } = null!;
}

public class EvaluationManagerSummaryDto
{
    public Guid Id { get; set; }
    public Guid CycleId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public EvaluationStatus Status { get; set; }
    public EvaluationObjectionStatus ObjectionStatus { get; set; }
    public DateTime? SubmittedAt { get; set; }
}

public class EvaluationManagerSummaryPagedResultDto
{
    public long TotalCount { get; set; }
    public List<EvaluationManagerSummaryDto> Items { get; set; } = new();
}

public class EvaluationEmployeeDto
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public UserRefDto Evaluator { get; set; } = null!;
    public EvaluationStatus Status { get; set; }
    public List<EvaluationSubmittedScoreDto> Scores { get; set; } = new();
    public string? EmployeeNotes { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public EvaluationObjectionStatus ObjectionStatus { get; set; }
    public List<EvaluationResponseDto> Responses { get; set; } = new();
}

public class EvaluationEmployeeSummaryDto
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public EvaluationStatus Status { get; set; }
    public EvaluationObjectionStatus ObjectionStatus { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
}

public class EvaluationEmployeeSummaryPagedResultDto
{
    public long TotalCount { get; set; }
    public List<EvaluationEmployeeSummaryDto> Items { get; set; } = new();
}

public class EvaluationHrDto
{
    public Guid Id { get; set; }
    public Guid CycleId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public UserRefDto Evaluator { get; set; } = null!;
    public EvaluationStatus Status { get; set; }
    public EvaluationAttendanceSummaryDto Attendance { get; set; } = null!;
    public List<EvaluationSubmittedScoreDto> Scores { get; set; } = new();
    public string? EmployeeNotes { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public EvaluationObjectionStatus ObjectionStatus { get; set; }
    public List<EvaluationResponseDto> Responses { get; set; } = new();
    public string ConcurrencyStamp { get; set; } = null!;
}

public class EvaluationHrSummaryDto
{
    public Guid Id { get; set; }
    public Guid CycleId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public UserRefDto Evaluator { get; set; } = null!;
    public EvaluationStatus Status { get; set; }
    public EvaluationObjectionStatus ObjectionStatus { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public bool IsLate { get; set; }
}

public class EvaluationHrSummaryPagedResultDto
{
    public long TotalCount { get; set; }
    public List<EvaluationHrSummaryDto> Items { get; set; } = new();
}

public class EvaluationScoreInputDto
{
    public Guid? CriterionId { get; set; }
    public string GradeCode { get; set; } = null!;
}

public class EvaluationSaveDraftDto
{
    public List<EvaluationScoreInputDto> Scores { get; set; } = null!;
    public string? EmployeeNotes { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class EvaluationSubmitInputDto
{
    public string ConcurrencyStamp { get; set; } = null!;
}

public class EvaluationResponseCreateDto
{
    public EvaluationResponseType? Type { get; set; }
    public string Content { get; set; } = null!;
}

public class EvaluationObjectionDecideDto
{
    public List<EvaluationScoreInputDto>? Scores { get; set; }

    public EvaluationObjectionDecision? Decision { get; set; }
    public string Reason { get; set; } = null!;
    public string ConcurrencyStamp { get; set; } = null!;
}

public class EvaluationTeamListInput : PagedAndSortedResultRequestDto
{
    public Guid? CycleId { get; set; }
    public EvaluationStatus? Status { get; set; }
    public Guid? EmployeeId { get; set; }
}

public class EvaluationMyListInput : PagedAndSortedResultRequestDto
{
    public int? Year { get; set; }
}

public class EvaluationHrListInput : PagedAndSortedResultRequestDto
{
    public Guid? CycleId { get; set; }
    public EvaluationStatus? Status { get; set; }
    public EvaluationObjectionStatus? ObjectionStatus { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? EmployeeId { get; set; }
    public bool? IsLate { get; set; }
}

public class EvaluationInternalNoteDto
{
    public Guid Id { get; set; }
    public UserRefDto Author { get; set; } = null!;
    public string Content { get; set; } = null!;
    public bool IsDraft { get; set; }
    public DateTime CreationTime { get; set; }
}

public class EvaluationInternalNotePagedResultDto
{
    public long TotalCount { get; set; }
    public List<EvaluationInternalNoteDto> Items { get; set; } = new();
}

public class EvaluationInternalNoteCreateDto
{
    public string Content { get; set; } = null!;
}

public class EvaluationInternalNoteDraftDto
{
    public string? Content { get; set; }
}

public class EvaluationGradeCountDto
{
    public string GradeCode { get; set; } = null!;
    public string GradeName { get; set; } = null!;
    public int Count { get; set; }
}

public class EvaluationAnnualCriterionSummaryDto
{
    public Guid CriterionId { get; set; }
    public string CriterionName { get; set; } = null!;
    public List<EvaluationGradeCountDto> GradeCounts { get; set; } = new();
}

public class EvaluationAnnualSummaryDto
{
    public EmployeeRefDto Employee { get; set; } = null!;
    public int Year { get; set; }
    public int EvaluatedMonthsCount { get; set; }
    public int AcknowledgedCount { get; set; }
    public int ObjectionsCount { get; set; }
    public List<EvaluationAnnualCriterionSummaryDto> Criteria { get; set; } = new();
}

public class DepartmentCompletionDto
{
    public int EvaluationsExpected { get; set; }
    public int EvaluationsSubmitted { get; set; }
    public bool ReportSubmitted { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsLate { get; set; }
}

public class DepartmentReportEmployeeRowDto
{
    public EmployeeRefDto Employee { get; set; } = null!;
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateCount { get; set; }
    public int LateMinutes { get; set; }
    public decimal LeaveDays { get; set; }
    public int PendingRequestsCount { get; set; }
    public EvaluationStatus? EvaluationStatus { get; set; }
}

public class DepartmentReportEvaluationSummaryDto
{
    public int ExpectedCount { get; set; }
    public int SubmittedCount { get; set; }
    public int AcknowledgedCount { get; set; }
    public int PendingObjectionsCount { get; set; }
    public List<EvaluationGradeCountDto> GradeCounts { get; set; } = new();
}

public class DepartmentReportCustodySummaryDto
{
    public int SharedItemsCount { get; set; }
    public int PersonalItemsCount { get; set; }
    public int UnacknowledgedItemsCount { get; set; }
    public DateOnly? LastCountDate { get; set; }
}

public class DepartmentReportCircularSummaryDto
{
    public int CircularsSentCount { get; set; }
    public int RecipientsCount { get; set; }
    public int AcknowledgedCount { get; set; }
}

public class DepartmentReportSnapshotDto
{
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public DateTime FrozenAt { get; set; }
    public List<DepartmentReportEmployeeRowDto> Employees { get; set; } = new();
    public DepartmentReportEvaluationSummaryDto Evaluations { get; set; } = null!;
    public DepartmentReportCustodySummaryDto Custody { get; set; } = null!;
    public DepartmentReportCircularSummaryDto Circulars { get; set; } = null!;
}

public class DepartmentReportDto
{
    public Guid Id { get; set; }
    public Guid CycleId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public UserRefDto Manager { get; set; } = null!;
    public DepartmentRefDto? Department { get; set; }
    public DepartmentReportStatus Status { get; set; }
    public DepartmentReportSnapshotDto Snapshot { get; set; } = null!;
    public string? Notes { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DepartmentCompletionDto Completion { get; set; } = null!;
    public string ConcurrencyStamp { get; set; } = null!;
}

public class DepartmentReportSummaryDto
{
    public Guid Id { get; set; }
    public Guid CycleId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public UserRefDto Manager { get; set; } = null!;
    public DepartmentRefDto? Department { get; set; }
    public DepartmentReportStatus Status { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DepartmentCompletionDto Completion { get; set; } = null!;
}

public class DepartmentReportSummaryPagedResultDto
{
    public long TotalCount { get; set; }
    public List<DepartmentReportSummaryDto> Items { get; set; } = new();
}

public class DepartmentReportListInput : PagedAndSortedResultRequestDto
{
    public Guid? CycleId { get; set; }
    public DepartmentReportStatus? Status { get; set; }
    public Guid? DepartmentId { get; set; }
    public bool? IsLate { get; set; }
}

public class DepartmentReportNotesPatchDto
{
    public string? Notes { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class DepartmentReportSubmitDto
{
    public string ConcurrencyStamp { get; set; } = null!;
}

public class CircularCreateDto
{
    public CircularScope? Scope { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
}

public class CircularReceivedDto
{
    public Guid Id { get; set; }
    public CircularScope Scope { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public UserRefDto Sender { get; set; } = null!;
    public DateTime SentAt { get; set; }
    public bool AcknowledgementRequired { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
}

public class CircularReceivedPagedResultDto
{
    public long TotalCount { get; set; }
    public List<CircularReceivedDto> Items { get; set; } = new();
}

public class CircularSentDto
{
    public Guid Id { get; set; }
    public CircularScope Scope { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime SentAt { get; set; }
    public int RecipientsCount { get; set; }
    public int AcknowledgedCount { get; set; }
}

public class CircularSentPagedResultDto
{
    public long TotalCount { get; set; }
    public List<CircularSentDto> Items { get; set; } = new();
}

public class CircularReceiptDto
{
    public EmployeeRefDto Recipient { get; set; } = null!;
    public DateTime? AcknowledgedAt { get; set; }
}

public class CircularReceiptPagedResultDto
{
    public long TotalCount { get; set; }
    public List<CircularReceiptDto> Items { get; set; } = new();
}

public class CircularReceivedListInput : PagedAndSortedResultRequestDto
{
    public bool? IsAcknowledged { get; set; }
}

public class CircularReceiptListInput : PagedAndSortedResultRequestDto
{
    public bool? IsAcknowledged { get; set; }
}

public interface IEvaluationCriterionAppService : IApplicationService
{
    Task<EvaluationCriterionPagedResultDto> GetListAsync(EvaluationScaleListInput input);
    Task<EvaluationCriterionDto> CreateAsync(EvaluationCriterionCreateDto input);
    Task<EvaluationCriterionDto> UpdateAsync(Guid id, EvaluationCriterionUpdateDto input);
}

public interface IEvaluationGradeAppService : IApplicationService
{
    Task<EvaluationGradePagedResultDto> GetListAsync(EvaluationScaleListInput input);
    Task<EvaluationGradeDto> CreateAsync(EvaluationGradeCreateDto input);
    Task<EvaluationGradeDto> UpdateAsync(Guid id, EvaluationGradeUpdateDto input);
}

public interface IEvaluationCycleAppService : IApplicationService
{
    Task<EvaluationCycleDto> GetCurrentAsync();
    Task<EvaluationCyclePagedResultDto> GetListAsync(EvaluationCycleListInput input);
}

public interface IEvaluationAppService : IApplicationService
{
    Task<EvaluationManagerSummaryPagedResultDto> GetTeamListAsync(EvaluationTeamListInput input);
    Task<EvaluationManagerDto> GetManagerFormAsync(Guid id);
    Task<EvaluationManagerDto> SaveDraftAsync(Guid id, EvaluationSaveDraftDto input);
    Task<EvaluationManagerDto> SubmitAsync(Guid id, EvaluationSubmitInputDto input);
    Task<EvaluationEmployeeSummaryPagedResultDto> GetMyListAsync(EvaluationMyListInput input);
    Task<EvaluationEmployeeDto> GetMyDetailAsync(Guid id);
    Task<EvaluationEmployeeDto> AcknowledgeAsync(Guid id);
    Task<EvaluationResponseDto> AddResponseAsync(Guid id, EvaluationResponseCreateDto input, Guid? idempotencyKey);
    Task<EvaluationHrDto> DecideObjectionAsync(Guid id, Guid responseId, EvaluationObjectionDecideDto input);
    Task<EvaluationHrSummaryPagedResultDto> GetHrListAsync(EvaluationHrListInput input);
    Task<EvaluationHrDto> GetHrViewAsync(Guid id);
    Task<EvaluationAnnualSummaryDto> GetMyAnnualSummaryAsync(int year);
    Task<EvaluationAnnualSummaryDto> GetAnnualSummaryAsync(Guid employeeId, int year);
}

public interface IEvaluationInternalNoteAppService : IApplicationService
{
    Task<EvaluationInternalNotePagedResultDto> GetListAsync(Guid evaluationId, PagedAndSortedResultRequestDto input);
    Task<EvaluationInternalNoteDto> AddAsync(Guid evaluationId, EvaluationInternalNoteCreateDto input, Guid? idempotencyKey);
    Task<EvaluationInternalNoteDto> SaveDraftAsync(Guid evaluationId, EvaluationInternalNoteDraftDto input);
}

public interface IDepartmentReportAppService : IApplicationService
{
    Task<DepartmentReportSummaryPagedResultDto> GetListAsync(DepartmentReportListInput input);
    Task<DepartmentReportDto> GetAsync(Guid id);
    Task<DepartmentReportDto> UpdateNotesAsync(Guid id, DepartmentReportNotesPatchDto input);
    Task<DepartmentReportDto> SubmitAsync(Guid id, DepartmentReportSubmitDto input);
}

public interface ICircularAppService : IApplicationService
{
    Task<CircularReceivedPagedResultDto> GetReceivedListAsync(CircularReceivedListInput input);
    Task<CircularSentDto> CreateAsync(CircularCreateDto input, Guid? idempotencyKey);
    Task<CircularSentPagedResultDto> GetSentListAsync(PagedAndSortedResultRequestDto input);
    Task<CircularReceivedDto> GetAsync(Guid id);
    Task<CircularReceiptPagedResultDto> GetReceiptsAsync(Guid id, CircularReceiptListInput input);
    Task<CircularReceivedDto> AcknowledgeAsync(Guid id);
}
