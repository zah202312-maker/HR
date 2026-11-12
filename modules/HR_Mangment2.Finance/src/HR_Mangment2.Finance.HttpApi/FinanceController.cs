using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using HR_Mangment2.Controllers;
using HR_Mangment2.Controllers.EmploymentContracts;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Reports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Auditing;

namespace HR_Mangment2.Finance;

/// <summary>The multipart form of AdjustmentCreateInputDto.</summary>
public class AdjustmentCreateInputDto
{
    [Required]
    public Guid? EmployeeId { get; set; }

    [Required]
    public string Period { get; set; } = null!;

    [Required]
    public AdjustmentType? Type { get; set; }

    public decimal Amount { get; set; }

    [Required]
    public string Reason { get; set; } = null!;

    [Required]
    public IFormFile Attachment { get; set; } = null!;

    public Guid? SourceRequestId { get; set; }
}

/// <summary>The multipart form of ExpenseCreateInputDto.</summary>
public class ExpenseCreateInputDto
{
    [Required]
    public string Category { get; set; } = null!;

    public decimal Amount { get; set; }

    [Required]
    public string Currency { get; set; } = null!;

    [Required]
    public DateOnly? Date { get; set; }

    public string? Description { get; set; }

    [Required]
    public IFormFile Attachment { get; set; } = null!;

    public Guid? SourceRequestId { get; set; }
}

[Route("api/app/finance")]
public class FinanceController : HR_Mangment2Controller
{
    private readonly ISalaryProfileAppService _profiles;
    private readonly IAdjustmentAppService _adjustments;
    private readonly IPayrollRunAppService _runs;
    private readonly IExpenseAppService _expenses;
    private readonly IAccountantRequestAppService _requests;
    private readonly IFinanceReportAppService _reports;

    public FinanceController(
        ISalaryProfileAppService profiles,
        IAdjustmentAppService adjustments,
        IPayrollRunAppService runs,
        IExpenseAppService expenses,
        IAccountantRequestAppService requests,
        IFinanceReportAppService reports)
    {
        _profiles = profiles;
        _adjustments = adjustments;
        _runs = runs;
        _expenses = expenses;
        _requests = requests;
        _reports = reports;
    }

    [HttpGet("salary-profile/overview", Name = "SalaryProfile_GetOverview")]
    public Task<EmployeeSalaryOverviewPagedResultDto> GetSalaryOverviewAsync([FromQuery] EmployeeOverviewListInput input) => _profiles.GetOverviewAsync(input);

    [HttpGet("salary-profile", Name = "SalaryProfile_List")]
    public Task<SalaryProfileListItemPagedResultDto> GetSalaryProfilesAsync([FromQuery] SalaryProfileListInput input) => _profiles.GetListAsync(input);

    [HttpGet("salary-profile/{employeeId:guid}", Name = "SalaryProfile_Get")]
    public Task<SalaryProfileDto> GetSalaryProfileAsync(Guid employeeId, [FromQuery] SalaryProfileGetInput input) => _profiles.GetAsync(employeeId, input);

    [HttpPut("salary-profile/{employeeId:guid}", Name = "SalaryProfile_Update")]
    public Task<SalaryProfileDto> UpdateSalaryProfileAsync(Guid employeeId, [FromBody] SalaryProfileUpdateInputDto input) => _profiles.UpdateAsync(employeeId, input);

    [HttpGet("salary-profile/{employeeId:guid}/history", Name = "SalaryProfile_GetHistory")]
    public Task<SalaryProfilePagedResultDto> GetSalaryHistoryAsync(Guid employeeId, [FromQuery] PagedAndSortedResultRequestDto input) => _profiles.GetHistoryAsync(employeeId, input);

    [HttpGet("adjustment", Name = "Adjustment_List")]
    public Task<AdjustmentPagedResultDto> GetAdjustmentsAsync([FromQuery] AdjustmentListInput input) => _adjustments.GetListAsync(input);

    [HttpPost("adjustment", Name = "Adjustment_Create")]
    [Consumes("multipart/form-data")]
    [DisableAuditing]
    public async Task<ActionResult<AdjustmentDto>> CreateAdjustmentAsync([FromForm] AdjustmentCreateInputDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _adjustments.CreateAsync(
            new AdjustmentCreateInput
            {
                EmployeeId = input.EmployeeId,
                Period = input.Period,
                Type = input.Type,
                Amount = input.Amount,
                Reason = input.Reason,
                Attachment = await UploadedFiles.ReadAsync(input.Attachment),
                SourceRequestId = input.SourceRequestId
            },
            idempotencyKey));

    [HttpPost("adjustment/{id:guid}/reverse", Name = "Adjustment_Reverse")]
    public Task<AdjustmentDto> ReverseAdjustmentAsync(Guid id, [FromBody] AdjustmentReverseInputDto input) => _adjustments.ReverseAsync(id, input);

    [HttpGet("adjustment/{id:guid}/attachment-download-url", Name = "Adjustment_GetAttachmentDownloadUrl")]
    [ResponseCache(NoStore = true)]
    public Task<FileDownloadUrlDto> GetAdjustmentAttachmentAsync(Guid id) => _adjustments.GetAttachmentDownloadUrlAsync(id);

    [HttpGet("adjustment/suggestions", Name = "AdjustmentSuggestion_List")]
    public Task<AdjustmentSuggestionPagedResultDto> GetSuggestionsAsync([FromQuery] AdjustmentSuggestionListInput input) => _adjustments.GetSuggestionsAsync(input);

    [HttpPost("adjustment/suggestions/{id:guid}/approve", Name = "AdjustmentSuggestion_Approve")]
    public Task<AdjustmentDto> ApproveSuggestionAsync(Guid id, [FromBody] AdjustmentSuggestionApproveInputDto input) => _adjustments.ApproveSuggestionAsync(id, input);

    [HttpPost("adjustment/suggestions/{id:guid}/reject", Name = "AdjustmentSuggestion_Reject")]
    public Task<AdjustmentSuggestionDto> RejectSuggestionAsync(Guid id, [FromBody] ReasonInputDto input) => _adjustments.RejectSuggestionAsync(id, input);

    [HttpGet("payroll-run", Name = "PayrollRun_List")]
    public Task<PayrollRunSummaryPagedResultDto> GetRunsAsync([FromQuery] PayrollRunListInput input) => _runs.GetListAsync(input);

    [HttpPost("payroll-run", Name = "PayrollRun_Create")]
    public async Task<ActionResult<PayrollRunDto>> CreateRunAsync([FromBody] PayrollRunCreateInputDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _runs.CreateAsync(input, idempotencyKey));

    [HttpGet("payroll-run/{id:guid}", Name = "PayrollRun_Get")]
    public Task<PayrollRunDto> GetRunAsync(Guid id) => _runs.GetAsync(id);

    [HttpPost("payroll-run/{id:guid}/recalculate", Name = "PayrollRun_Recalculate")]
    public Task<PayrollRunDto> RecalculateAsync(Guid id, [FromBody] PayrollRunActionInputDto input) => _runs.RecalculateAsync(id, input);

    [HttpPost("payroll-run/{id:guid}/request-approval", Name = "PayrollRun_RequestApproval")]
    public Task<PayrollRunDto> RequestApprovalAsync(Guid id, [FromBody] PayrollRunActionInputDto input) => _runs.RequestApprovalAsync(id, input);

    [HttpPost("payroll-run/{id:guid}/approve", Name = "PayrollRun_Approve")]
    public Task<PayrollRunDto> ApproveAsync(Guid id, [FromBody] PayrollRunActionInputDto input) => _runs.ApproveAsync(id, input);

    [HttpPost("payroll-run/{id:guid}/return", Name = "PayrollRun_ReturnForChanges")]
    public Task<PayrollRunDto> ReturnAsync(Guid id, [FromBody] ReasonInputDto input) => _runs.ReturnForChangesAsync(id, input);

    [HttpPost("payroll-run/{id:guid}/close", Name = "PayrollRun_Close")]
    public Task<PayrollRunDto> CloseAsync(Guid id, [FromBody] PayrollRunActionInputDto input) => _runs.CloseAsync(id, input);

    [HttpPost("payroll-run/{id:guid}/deliver", Name = "PayrollRun_Deliver")]
    public Task<PayrollRunDto> DeliverAsync(Guid id, [FromBody] PayrollDeliveryInputDto input) => _runs.DeliverAsync(id, input);

    [HttpPost("payroll-run/{id:guid}/undo-delivery", Name = "PayrollRun_UndoDelivery")]
    public Task<PayrollRunDto> UndoDeliveryAsync(Guid id, [FromBody] PayrollUndoDeliveryInputDto input) => _runs.UndoDeliveryAsync(id, input);

    [HttpGet("payroll-run/{id:guid}/export", Name = "PayrollRun_Export")]
    public async Task<IActionResult> ExportRunAsync(Guid id, [FromQuery] FinanceExportInput input)
    {
        ValidateModel();
        return Send(await _runs.ExportAsync(id, input));
    }

    [HttpGet("payslip/my", Name = "Payslip_GetMyList")]
    public Task<PayslipSummaryPagedResultDto> GetMyPayslipsAsync([FromQuery] PagedAndSortedResultRequestDto input) => _runs.GetMyPayslipsAsync(input);

    [HttpGet("payslip/my/{id:guid}", Name = "Payslip_GetMy")]
    public Task<PayslipDto> GetMyPayslipAsync(Guid id) => _runs.GetMyPayslipAsync(id);

    [HttpGet("payslip/my/{id:guid}/pdf", Name = "Payslip_DownloadMyPdf")]
    public async Task<IActionResult> DownloadMyPayslipAsync(Guid id) => Send(await _runs.DownloadMyPayslipPdfAsync(id));

    [HttpGet("expense", Name = "Expense_List")]
    public Task<ExpensePagedResultDto> GetExpensesAsync([FromQuery] ExpenseListInput input) => _expenses.GetListAsync(input);

    [HttpPost("expense", Name = "Expense_Create")]
    [Consumes("multipart/form-data")]
    [DisableAuditing]
    public async Task<ActionResult<ExpenseDto>> CreateExpenseAsync([FromForm] ExpenseCreateInputDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _expenses.CreateAsync(
            new ExpenseCreateInput
            {
                Category = input.Category,
                Amount = input.Amount,
                Currency = input.Currency,
                Date = input.Date,
                Description = input.Description,
                Attachment = await UploadedFiles.ReadAsync(input.Attachment),
                SourceRequestId = input.SourceRequestId
            },
            idempotencyKey));

    [HttpGet("expense/export", Name = "Expense_Export")]
    public async Task<IActionResult> ExportExpensesAsync([FromQuery] ExpenseExportInput input)
    {
        ValidateModel();
        return Send(await _expenses.ExportAsync(input));
    }

    [HttpGet("expense/{id:guid}", Name = "Expense_Get")]
    public Task<ExpenseDto> GetExpenseAsync(Guid id) => _expenses.GetAsync(id);

    [HttpPost("expense/{id:guid}/reverse", Name = "Expense_Reverse")]
    public Task<ExpenseDto> ReverseExpenseAsync(Guid id, [FromBody] ReasonInputDto input) => _expenses.ReverseAsync(id, input);

    [HttpGet("expense/{id:guid}/attachment-download-url", Name = "Expense_GetAttachmentDownloadUrl")]
    [ResponseCache(NoStore = true)]
    public Task<FileDownloadUrlDto> GetExpenseAttachmentAsync(Guid id) => _expenses.GetAttachmentDownloadUrlAsync(id);

    [HttpGet("accountant-request", Name = "AccountantRequest_List")]
    public Task<AccountantRequestSummaryPagedResultDto> GetAccountantRequestsAsync([FromQuery] AccountantRequestListInput input) => _requests.GetListAsync(input);

    [HttpGet("accountant-request/{id:guid}", Name = "AccountantRequest_Get")]
    public Task<AccountantRequestDto> GetAccountantRequestAsync(Guid id) => _requests.GetAsync(id);

    [HttpGet("report/monthly-statement", Name = "FinanceReport_GetMonthlyStatement")]
    public Task<FinanceMonthlyStatementReportDto> GetMonthlyStatementAsync([FromQuery] FinanceMonthlyStatementInput input) => _reports.GetMonthlyStatementAsync(input);

    [HttpGet("report/adjustments", Name = "FinanceReport_GetAdjustments")]
    public Task<FinanceAdjustmentReportPagedResultDto> GetAdjustmentReportAsync([FromQuery] FinanceAdjustmentReportInput input) => _reports.GetAdjustmentsAsync(input);

    [HttpGet("report/store-expenses", Name = "FinanceReport_GetStoreExpenses")]
    public Task<FinanceStoreExpenseReportPagedResultDto> GetStoreExpenseReportAsync([FromQuery] FinanceStoreExpenseReportInput input) => _reports.GetStoreExpensesAsync(input);

    [HttpGet("report/{code}/export", Name = "FinanceReport_Export")]
    public async Task<IActionResult> ExportReportAsync(FinanceReportCode code, [FromQuery] FinanceReportExportInput input)
    {
        ValidateModel();
        return Send(await _reports.ExportAsync(code, input));
    }

    private IActionResult Send(ReportFile file)
    {
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(file.Content, file.ContentType, file.FileName);
    }
}
