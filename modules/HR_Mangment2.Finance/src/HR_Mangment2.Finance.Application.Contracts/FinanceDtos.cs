using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Leaves;
using HR_Mangment2.Reports;
using HR_Mangment2.Requests;
using HR_Mangment2.Warnings;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.Finance;

public class SalaryComponentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public SalaryComponentType Type { get; set; }
    public decimal Amount { get; set; }
}

public class SalaryComponentInputDto
{
    public string Name { get; set; } = null!;
    public SalaryComponentType? Type { get; set; }
    public decimal Amount { get; set; }
}

public class SalaryProfileDto
{
    public Guid Id { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public decimal BaseSalary { get; set; }
    public string Currency { get; set; } = null!;
    public DateOnly EffectiveFrom { get; set; }
    public List<SalaryComponentDto> Components { get; set; } = new();
    public string ConcurrencyStamp { get; set; } = null!;
    public DateTime CreationTime { get; set; }
}

public class SalaryProfileListItemDto
{
    public EmployeeRefDto Employee { get; set; } = null!;
    public bool HasProfile { get; set; }
    public decimal? BaseSalary { get; set; }
    public string? Currency { get; set; }
    public DateOnly? EffectiveFrom { get; set; }
}

public class SalaryProfileListItemPagedResultDto
{
    public long TotalCount { get; set; }
    public List<SalaryProfileListItemDto> Items { get; set; } = new();
}

public class SalaryProfilePagedResultDto
{
    public long TotalCount { get; set; }
    public List<SalaryProfileDto> Items { get; set; } = new();
}

public class SalaryProfileUpdateInputDto
{
    public decimal BaseSalary { get; set; }
    public string Currency { get; set; } = null!;
    public DateOnly? EffectiveFrom { get; set; }
    public List<SalaryComponentInputDto> Components { get; set; } = null!;
    public string? ConcurrencyStamp { get; set; }
}

public class SalaryProfileListInput : PagedAndSortedResultRequestDto
{
    public Guid? DepartmentId { get; set; }
    public bool? HasProfile { get; set; }
    public string? Filter { get; set; }
}

/// <summary>Search and paging of SalaryProfile_GetOverview (decision 2026-11-11).</summary>
public class EmployeeOverviewListInput : PagedResultRequestDto
{
    [System.ComponentModel.DataAnnotations.StringLength(100)]
    public string? Search { get; set; }
}

/// <summary>One line of the executive director's employee salaries page: the base salary only.</summary>
public class EmployeeSalaryOverviewDto
{
    public EmployeeRefDto Employee { get; set; } = null!;
    public decimal? BaseSalary { get; set; }
    public string? Currency { get; set; }
    public DateOnly? EffectiveFrom { get; set; }
}

public class EmployeeSalaryOverviewPagedResultDto
{
    public long TotalCount { get; set; }
    public List<EmployeeSalaryOverviewDto> Items { get; set; } = new();
}

public class SalaryProfileGetInput
{
    public DateOnly? AsOf { get; set; }
}

public class AdjustmentDto
{
    public Guid Id { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public string Period { get; set; } = null!;
    public AdjustmentType Type { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public AdjustmentSource Source { get; set; }
    public AdjustmentStatus Status { get; set; }
    public AttachmentDto? Attachment { get; set; }
    public Guid? SourceRequestId { get; set; }
    public Guid? SuggestionId { get; set; }
    public Guid? ReversalOfAdjustmentId { get; set; }
    public UserRefDto CreatedBy { get; set; } = null!;
    public DateTime CreationTime { get; set; }
}

public class AdjustmentPagedResultDto
{
    public long TotalCount { get; set; }
    public List<AdjustmentDto> Items { get; set; } = new();
}

public class AdjustmentCreateInput
{
    public Guid? EmployeeId { get; set; }
    public string Period { get; set; } = null!;
    public AdjustmentType? Type { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = null!;
    public UploadedFileInput Attachment { get; set; } = null!;
    public Guid? SourceRequestId { get; set; }
}

public class AdjustmentReverseInputDto
{
    public string Reason { get; set; } = null!;
    public string Period { get; set; } = null!;
}

public class AdjustmentListInput : PagedAndSortedResultRequestDto
{
    public Guid? EmployeeId { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? Period { get; set; }
    public AdjustmentType? Type { get; set; }
    public AdjustmentSource? Source { get; set; }
    public AdjustmentStatus? Status { get; set; }
}

public class AdjustmentSuggestionDto
{
    public Guid Id { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public string Period { get; set; } = null!;
    public AdjustmentType Type { get; set; }
    public AdjustmentSuggestionBasis Basis { get; set; }
    public decimal Quantity { get; set; }
    public AdjustmentSuggestionQuantityUnit QuantityUnit { get; set; }
    public decimal? SuggestedAmount { get; set; }
    public string? Currency { get; set; }
    public string? SettlementForPeriod { get; set; }
    public AdjustmentSuggestionStatus Status { get; set; }
    public Guid? AdjustmentId { get; set; }
    public UserRefDto? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNote { get; set; }
    public DateTime CreationTime { get; set; }
}

public class AdjustmentSuggestionPagedResultDto
{
    public long TotalCount { get; set; }
    public List<AdjustmentSuggestionDto> Items { get; set; } = new();
}

public class AdjustmentSuggestionApproveInputDto
{
    public decimal? Amount { get; set; }
    public string? Reason { get; set; }
}

public class AdjustmentSuggestionListInput : PagedAndSortedResultRequestDto
{
    public string? Period { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid? DepartmentId { get; set; }
    public AdjustmentSuggestionStatus? Status { get; set; }
    public AdjustmentSuggestionBasis? Basis { get; set; }
}

public class PayrollLineDto
{
    public PayrollLineKind Kind { get; set; }
    public string Label { get; set; } = null!;
    public decimal Amount { get; set; }
    public Guid? AdjustmentId { get; set; }
}

public class PayrollItemDto
{
    public Guid Id { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public string Currency { get; set; } = null!;
    public decimal Gross { get; set; }
    public decimal Additions { get; set; }
    public decimal Deductions { get; set; }
    public decimal Net { get; set; }
    public List<PayrollLineDto> Lines { get; set; } = new();
    public DateOnly? DeliveredOn { get; set; }
    public UserRefDto? DeliveredBy { get; set; }
}

public class PayrollTotalDto
{
    public string Currency { get; set; } = null!;
    public decimal Gross { get; set; }
    public decimal Additions { get; set; }
    public decimal Deductions { get; set; }
    public decimal Net { get; set; }
}

public class PayrollRunSummaryDto
{
    public Guid Id { get; set; }
    public string Period { get; set; } = null!;
    public PayrollRunStatus Status { get; set; }
    public UserRefDto PreparedBy { get; set; } = null!;
    public DateTime? ApprovalRequestedAt { get; set; }
    public UserRefDto? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime LastCalculatedAt { get; set; }
    public bool IsStale { get; set; }
    public int EmployeeCount { get; set; }
    public int DeliveredCount { get; set; }
    public List<PayrollTotalDto> Totals { get; set; } = new();
    public string ConcurrencyStamp { get; set; } = null!;
}

public class PayrollRunDto : PayrollRunSummaryDto
{
    public List<PayrollItemDto> Items { get; set; } = new();
    public List<EmployeeRefDto> EmployeesWithoutProfile { get; set; } = new();
    public int PendingSuggestionCount { get; set; }
}

public class PayrollRunSummaryPagedResultDto
{
    public long TotalCount { get; set; }
    public List<PayrollRunSummaryDto> Items { get; set; } = new();
}

public class PayrollRunCreateInputDto
{
    public string Period { get; set; } = null!;
}

public class PayrollRunActionInputDto
{
    public string ConcurrencyStamp { get; set; } = null!;
    public string? Note { get; set; }
}

public class PayrollRunListInput : PagedAndSortedResultRequestDto
{
    public PayrollRunStatus? Status { get; set; }
    public string? PeriodFrom { get; set; }
    public string? PeriodTo { get; set; }
}

public class FinanceExportInput
{
    public FinanceExportFormat? Format { get; set; }
}

public class PayslipSummaryDto
{
    public Guid Id { get; set; }
    public string Period { get; set; } = null!;
    public string Currency { get; set; } = null!;
    public decimal Gross { get; set; }
    public decimal Additions { get; set; }
    public decimal Deductions { get; set; }
    public decimal Net { get; set; }
    public DateTime ClosedAt { get; set; }
    public DateOnly? DeliveredOn { get; set; }
}

public class PayslipSummaryPagedResultDto
{
    public long TotalCount { get; set; }
    public List<PayslipSummaryDto> Items { get; set; } = new();
}

public class PayslipDto
{
    public Guid Id { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public string Period { get; set; } = null!;
    public string Currency { get; set; } = null!;
    public decimal Gross { get; set; }
    public decimal Additions { get; set; }
    public decimal Deductions { get; set; }
    public decimal Net { get; set; }
    public List<PayrollLineDto> Lines { get; set; } = new();
    public DateTime ClosedAt { get; set; }
    public DateOnly? DeliveredOn { get; set; }
}

public class ExpenseDto
{
    public Guid Id { get; set; }
    public string Category { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public DateOnly Date { get; set; }
    public string? Description { get; set; }
    public ExpenseSource Source { get; set; }
    public ExpenseStatus Status { get; set; }
    public AttachmentDto? Attachment { get; set; }
    public Guid? StockMovementId { get; set; }
    public Guid? SourceRequestId { get; set; }
    public Guid? ReversalOfExpenseId { get; set; }
    public UserRefDto? CreatedBy { get; set; }
    public DateTime CreationTime { get; set; }
}

public class ExpensePagedResultDto
{
    public long TotalCount { get; set; }
    public List<ExpenseDto> Items { get; set; } = new();
}

public class ExpenseCreateInput
{
    public string Category { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public DateOnly? Date { get; set; }
    public string? Description { get; set; }
    public UploadedFileInput Attachment { get; set; } = null!;
    public Guid? SourceRequestId { get; set; }
}

public class ExpenseListInput : PagedAndSortedResultRequestDto
{
    public ExpenseSource? Source { get; set; }
    public string? Category { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public ExpenseStatus? Status { get; set; }
}

public class ExpenseExportInput
{
    public FinanceExportFormat? Format { get; set; }
    public ExpenseSource? Source { get; set; }
    public string? Category { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
}

public class AccountantRequestSummaryDto
{
    public Guid Id { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public string RequestTypeCode { get; set; } = null!;
    public string RequestTypeName { get; set; } = null!;
    public AccountantRequestStatus Status { get; set; }
    public DateTime ArrivedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
}

public class AccountantRequestSummaryPagedResultDto
{
    public long TotalCount { get; set; }
    public List<AccountantRequestSummaryDto> Items { get; set; } = new();
}

public class AccountantRequestDto
{
    public Guid Id { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public UserRefDto SubmittedBy { get; set; } = null!;
    public string RequestTypeCode { get; set; } = null!;
    public string RequestTypeName { get; set; } = null!;
    public string? Description { get; set; }
    public AccountantRequestStatus Status { get; set; }
    public DateTime ArrivedAt { get; set; }
    public DateTime? DecidedAt { get; set; }
    public List<TimelineEntryDto> Timeline { get; set; } = new();
    public List<Guid> LinkedAdjustmentIds { get; set; } = new();
    public List<Guid> LinkedExpenseIds { get; set; } = new();
}

public class AccountantRequestListInput : PagedAndSortedResultRequestDto
{
    public AccountantRequestStatus? Status { get; set; }
    public Guid? EmployeeId { get; set; }
}

public class FinanceMonthlyStatementReportDto
{
    public string Period { get; set; } = null!;
    public Guid RunId { get; set; }
    public PayrollRunStatus RunStatus { get; set; }
    public List<PayrollItemDto> Items { get; set; } = new();
    public List<PayrollTotalDto> Totals { get; set; } = new();
}

public class FinanceAdjustmentTotalDto
{
    public string Currency { get; set; } = null!;
    public decimal Additions { get; set; }
    public decimal Deductions { get; set; }
}

public class FinanceAdjustmentReportPagedResultDto
{
    public long TotalCount { get; set; }
    public List<AdjustmentDto> Items { get; set; } = new();
    public List<FinanceAdjustmentTotalDto> Totals { get; set; } = new();
}

public class FinanceAmountTotalDto
{
    public string Currency { get; set; } = null!;
    public decimal Amount { get; set; }
}

public class FinanceStoreExpenseReportPagedResultDto
{
    public long TotalCount { get; set; }
    public List<ExpenseDto> Items { get; set; } = new();
    public List<FinanceAmountTotalDto> Totals { get; set; } = new();
}

public class FinanceMonthlyStatementInput
{
    public string Period { get; set; } = null!;
    public Guid? DepartmentId { get; set; }
}

public class FinanceAdjustmentReportInput : PagedAndSortedResultRequestDto
{
    public string PeriodFrom { get; set; } = null!;
    public string PeriodTo { get; set; } = null!;
    public Guid? EmployeeId { get; set; }
    public Guid? DepartmentId { get; set; }
    public AdjustmentType? Type { get; set; }
}

public class FinanceStoreExpenseReportInput : PagedAndSortedResultRequestDto
{
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
}

public class FinanceReportExportInput
{
    public FinanceExportFormat? Format { get; set; }
    public string? Period { get; set; }
    public string? PeriodFrom { get; set; }
    public string? PeriodTo { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid? DepartmentId { get; set; }
    public AdjustmentType? Type { get; set; }
}

public interface ISalaryProfileAppService : IApplicationService
{
    Task<SalaryProfileListItemPagedResultDto> GetListAsync(SalaryProfileListInput input);
    Task<EmployeeSalaryOverviewPagedResultDto> GetOverviewAsync(EmployeeOverviewListInput input);
    Task<SalaryProfileDto> GetAsync(Guid employeeId, SalaryProfileGetInput input);
    Task<SalaryProfileDto> UpdateAsync(Guid employeeId, SalaryProfileUpdateInputDto input);
    Task<SalaryProfilePagedResultDto> GetHistoryAsync(Guid employeeId, PagedAndSortedResultRequestDto input);
}

public interface IAdjustmentAppService : IApplicationService
{
    Task<AdjustmentPagedResultDto> GetListAsync(AdjustmentListInput input);
    Task<AdjustmentDto> CreateAsync(AdjustmentCreateInput input, Guid? idempotencyKey);
    Task<AdjustmentDto> ReverseAsync(Guid id, AdjustmentReverseInputDto input);
    Task<FileDownloadUrlDto> GetAttachmentDownloadUrlAsync(Guid id);
    Task<AdjustmentSuggestionPagedResultDto> GetSuggestionsAsync(AdjustmentSuggestionListInput input);
    Task<AdjustmentDto> ApproveSuggestionAsync(Guid id, AdjustmentSuggestionApproveInputDto input);
    Task<AdjustmentSuggestionDto> RejectSuggestionAsync(Guid id, ReasonInputDto input);
}

public interface IPayrollRunAppService : IApplicationService
{
    Task<PayrollRunSummaryPagedResultDto> GetListAsync(PayrollRunListInput input);
    Task<PayrollRunDto> CreateAsync(PayrollRunCreateInputDto input, Guid? idempotencyKey);
    Task<PayrollRunDto> GetAsync(Guid id);
    Task<PayrollRunDto> RecalculateAsync(Guid id, PayrollRunActionInputDto input);
    Task<PayrollRunDto> RequestApprovalAsync(Guid id, PayrollRunActionInputDto input);
    Task<PayrollRunDto> ApproveAsync(Guid id, PayrollRunActionInputDto input);
    Task<PayrollRunDto> ReturnForChangesAsync(Guid id, ReasonInputDto input);
    Task<PayrollRunDto> CloseAsync(Guid id, PayrollRunActionInputDto input);
    Task<PayrollRunDto> DeliverAsync(Guid id, PayrollDeliveryInputDto input);
    Task<PayrollRunDto> UndoDeliveryAsync(Guid id, PayrollUndoDeliveryInputDto input);
    Task<ReportFile> ExportAsync(Guid id, FinanceExportInput input);
    Task<PayslipSummaryPagedResultDto> GetMyPayslipsAsync(PagedAndSortedResultRequestDto input);
    Task<PayslipDto> GetMyPayslipAsync(Guid id);
    Task<ReportFile> DownloadMyPayslipPdfAsync(Guid id);
}

public interface IExpenseAppService : IApplicationService
{
    Task<ExpensePagedResultDto> GetListAsync(ExpenseListInput input);
    Task<ExpenseDto> CreateAsync(ExpenseCreateInput input, Guid? idempotencyKey);
    Task<ReportFile> ExportAsync(ExpenseExportInput input);
    Task<ExpenseDto> GetAsync(Guid id);
    Task<ExpenseDto> ReverseAsync(Guid id, ReasonInputDto input);
    Task<FileDownloadUrlDto> GetAttachmentDownloadUrlAsync(Guid id);
}

public interface IAccountantRequestAppService : IApplicationService
{
    Task<AccountantRequestSummaryPagedResultDto> GetListAsync(AccountantRequestListInput input);
    Task<AccountantRequestDto> GetAsync(Guid id);
}

public interface IFinanceReportAppService : IApplicationService
{
    Task<FinanceMonthlyStatementReportDto> GetMonthlyStatementAsync(FinanceMonthlyStatementInput input);
    Task<FinanceAdjustmentReportPagedResultDto> GetAdjustmentsAsync(FinanceAdjustmentReportInput input);
    Task<FinanceStoreExpenseReportPagedResultDto> GetStoreExpensesAsync(FinanceStoreExpenseReportInput input);
    Task<ReportFile> ExportAsync(FinanceReportCode code, FinanceReportExportInput input);
}
