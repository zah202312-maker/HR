using System.Threading.Tasks;
using HR_Mangment2.Reports;
using Microsoft.AspNetCore.Mvc;

namespace HR_Mangment2.Controllers.Reports;

[Route("api/app/report")]
public class ReportController : HR_Mangment2Controller
{
    private readonly IReportAppService _service;

    public ReportController(IReportAppService service)
    {
        _service = service;
    }

    [HttpGet("team-attendance", Name = "Report_GetTeamAttendance")]
    public Task<TeamAttendanceRowPagedResultDto> GetTeamAttendanceAsync([FromQuery] TeamPeriodReportInput input) => _service.GetTeamAttendanceAsync(input);

    [HttpGet("team-lateness-absence", Name = "Report_GetTeamLatenessAbsence")]
    public Task<TeamLatenessAbsenceRowPagedResultDto> GetTeamLatenessAbsenceAsync([FromQuery] TeamPeriodReportInput input) => _service.GetTeamLatenessAbsenceAsync(input);

    [HttpGet("team-leaves", Name = "Report_GetTeamLeaves")]
    public Task<TeamLeaveRowPagedResultDto> GetTeamLeavesAsync([FromQuery] TeamPeriodReportInput input) => _service.GetTeamLeavesAsync(input);

    [HttpGet("team-pending", Name = "Report_GetTeamPending")]
    public Task<TeamPendingRequestRowPagedResultDto> GetTeamPendingAsync([FromQuery] TeamPendingReportInput input) => _service.GetTeamPendingAsync(input);

    [HttpGet("team-custody", Name = "Report_GetTeamCustody")]
    public Task<TeamCustodyRowPagedResultDto> GetTeamCustodyAsync([FromQuery] TeamCustodyReportInput input) => _service.GetTeamCustodyAsync(input);

    [HttpGet("team-evaluations", Name = "Report_GetTeamEvaluations")]
    public Task<TeamEvaluationRowPagedResultDto> GetTeamEvaluationsAsync([FromQuery] TeamEvaluationReportInput input) => _service.GetTeamEvaluationsAsync(input);

    [HttpGet("review-dashboard", Name = "Report_GetReviewDashboard")]
    public Task<ReviewDashboardDto> GetReviewDashboardAsync([FromQuery] ReviewDashboardInput input) => _service.GetReviewDashboardAsync(input);

    [HttpGet("evaluation-consistency", Name = "Report_GetEvaluationConsistency")]
    public Task<EvaluationConsistencyRowPagedResultDto> GetEvaluationConsistencyAsync([FromQuery] EvaluationConsistencyReportInput input) => _service.GetEvaluationConsistencyAsync(input);

    [HttpGet("today-attendance-summary", Name = "Report_GetTodayAttendanceSummary")]
    public Task<TodayAttendanceSummaryDto> GetTodayAttendanceSummaryAsync() => _service.GetTodayAttendanceSummaryAsync();

    [HttpGet("hr-attendance", Name = "Report_GetHrAttendance")]
    public Task<HrAttendanceReportRowPagedResultDto> GetHrAttendanceAsync([FromQuery] HrPeriodReportInput input) => _service.GetHrAttendanceAsync(input);

    [HttpGet("hr-leave-balances", Name = "Report_GetHrLeaveBalances")]
    public Task<HrLeaveBalanceReportRowPagedResultDto> GetHrLeaveBalancesAsync([FromQuery] HrLeaveBalanceReportInput input) => _service.GetHrLeaveBalancesAsync(input);

    [HttpGet("hr-requests", Name = "Report_GetHrRequests")]
    public Task<HrRequestReportRowPagedResultDto> GetHrRequestsAsync([FromQuery] HrPeriodReportInput input) => _service.GetHrRequestsAsync(input);

    [HttpGet("hr-contracts", Name = "Report_GetHrContracts")]
    public Task<HrContractReportRowPagedResultDto> GetHrContractsAsync([FromQuery] HrContractReportInput input) => _service.GetHrContractsAsync(input);

    [HttpGet("hr-warnings", Name = "Report_GetHrWarnings")]
    public Task<HrWarningReportRowPagedResultDto> GetHrWarningsAsync([FromQuery] HrPeriodReportInput input) => _service.GetHrWarningsAsync(input);

    [HttpGet("{code}/export", Name = "Report_Export")]
    public async Task<IActionResult> ExportAsync(ReportCode code, [FromQuery] ReportExportInput input)
    {
        // ABP's validation filter skips actions returning IActionResult, and an unknown code would bind as the first report.
        ValidateModel();

        var file = await _service.ExportAsync(code, input);
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(file.Content, file.ContentType, file.FileName);
    }
}
