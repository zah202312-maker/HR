using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR_Mangment2.Departments;
using HR_Mangment2.Employees;
using HR_Mangment2.Evaluations;
using HR_Mangment2.Identity;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.Reports;

public class TeamAttendanceRowDto
{
    public EmployeeRefDto Employee { get; set; } = null!;
    public DateOnly Date { get; set; }
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public int LateMinutes { get; set; }
    public string StatusName { get; set; } = null!;
}

public class TeamAttendanceRowPagedResultDto
{
    public long TotalCount { get; set; }
    public List<TeamAttendanceRowDto> Items { get; set; } = new();
}

public class TeamLatenessAbsenceRowDto
{
    public EmployeeRefDto Employee { get; set; } = null!;
    public int LateCount { get; set; }
    public int LateMinutes { get; set; }
    public int AbsentDays { get; set; }
}

public class TeamLatenessAbsenceRowPagedResultDto
{
    public long TotalCount { get; set; }
    public List<TeamLatenessAbsenceRowDto> Items { get; set; } = new();
}

public class TeamLeaveRowDto
{
    public EmployeeRefDto Employee { get; set; } = null!;
    public string LeaveTypeName { get; set; } = null!;
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public decimal? Days { get; set; }
    public decimal? Hours { get; set; }
    public string StatusName { get; set; } = null!;
}

public class TeamLeaveRowPagedResultDto
{
    public long TotalCount { get; set; }
    public List<TeamLeaveRowDto> Items { get; set; } = new();
}

public class TeamPendingRequestRowDto
{
    public Guid RequestId { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public string RequestTypeName { get; set; } = null!;
    public string StatusName { get; set; } = null!;
    public DateTime CreationTime { get; set; }
}

public class TeamPendingRequestRowPagedResultDto
{
    public long TotalCount { get; set; }
    public List<TeamPendingRequestRowDto> Items { get; set; } = new();
}

public class TeamCustodyRowDto
{
    public Guid CustodyId { get; set; }
    public string ItemName { get; set; } = null!;
    public ReportCustodyKind Kind { get; set; }
    public EmployeeRefDto? Employee { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public string StatusName { get; set; } = null!;
}

public class TeamCustodyRowPagedResultDto
{
    public long TotalCount { get; set; }
    public List<TeamCustodyRowDto> Items { get; set; } = new();
}

public class TeamEvaluationRowDto
{
    public Guid EvaluationId { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public int Year { get; set; }
    public int Month { get; set; }
    public EvaluationStatus Status { get; set; }
    public EvaluationObjectionStatus ObjectionStatus { get; set; }
    public List<EvaluationSubmittedScoreDto> Scores { get; set; } = new();
}

public class TeamEvaluationRowPagedResultDto
{
    public long TotalCount { get; set; }
    public List<TeamEvaluationRowDto> Items { get; set; } = new();
}

public class ReviewComplaintCountsDto
{
    public int ConfidentialSubmittedCount { get; set; }
    public int HighlyConfidentialSubmittedCount { get; set; }
    public int LateNormalCount { get; set; }
    public int LateConfidentialCount { get; set; }
    public int LateHighlyConfidentialCount { get; set; }
    public DateTime AsOf { get; set; }
}

public class ReviewDepartmentCompletionDto
{
    public UserRefDto Manager { get; set; } = null!;
    public DepartmentRefDto? Department { get; set; }
    public DepartmentCompletionDto Completion { get; set; } = null!;
}

public class ReviewDashboardDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public ReviewComplaintCountsDto ComplaintCounts { get; set; } = null!;
    public Guid? CycleId { get; set; }
    public int CompletedDepartmentsCount { get; set; }
    public int LateDepartmentsCount { get; set; }
    public List<ReviewDepartmentCompletionDto> Departments { get; set; } = new();
}

public class EvaluationConsistencyRowDto
{
    public UserRefDto Manager { get; set; } = null!;
    public DepartmentRefDto? Department { get; set; }
    public int ExpectedCount { get; set; }
    public int SubmittedCount { get; set; }
    public decimal CompletionRatio { get; set; }
    public int DistinctGradesUsed { get; set; }
    public bool SameGradeForAll { get; set; }
    public List<EvaluationGradeCountDto> GradeCounts { get; set; } = new();
}

public class EvaluationConsistencyRowPagedResultDto
{
    public long TotalCount { get; set; }
    public List<EvaluationConsistencyRowDto> Items { get; set; } = new();
}

/// <summary>Today's attendance counts for the whole company, for the executive director's dashboard (decision 2026-11-11).</summary>
public class TodayAttendanceSummaryDto
{
    public DateOnly Date { get; set; }
    public int Present { get; set; }
    public int Late { get; set; }
    public int Absent { get; set; }
    public decimal OnLeave { get; set; }
}

public class HrAttendanceReportRowDto
{
    public EmployeeRefDto Employee { get; set; } = null!;
    public int ScheduledWorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateCount { get; set; }
    public int LateMinutes { get; set; }
    public decimal LeaveDays { get; set; }
}

public class HrAttendanceReportRowPagedResultDto
{
    public long TotalCount { get; set; }
    public List<HrAttendanceReportRowDto> Items { get; set; } = new();
}

public class HrLeaveBalanceReportRowDto
{
    public EmployeeRefDto Employee { get; set; } = null!;
    public DateOnly LeaveYearStart { get; set; }
    public decimal EntitlementDays { get; set; }
    public decimal CarriedInDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal HeldDays { get; set; }
    public decimal RemainingDays { get; set; }
    public decimal UsedHours { get; set; }

    /// <summary>Approved annual leave days inside from..to; null without a period (decision 2026-11-10).</summary>
    public decimal? PeriodUsedDays { get; set; }

    /// <summary>Approved hourly leave hours dated inside from..to; null without a period (decision 2026-11-10).</summary>
    public decimal? PeriodUsedHours { get; set; }
}

public class HrLeaveBalanceReportRowPagedResultDto
{
    public long TotalCount { get; set; }
    public List<HrLeaveBalanceReportRowDto> Items { get; set; } = new();
}

public class HrRequestReportRowDto
{
    public Guid RequestId { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public string RequestTypeName { get; set; } = null!;
    public string StatusName { get; set; } = null!;
    public DateTime CreationTime { get; set; }
    public DateTime? ClosedAt { get; set; }
}

public class HrRequestReportRowPagedResultDto
{
    public long TotalCount { get; set; }
    public List<HrRequestReportRowDto> Items { get; set; } = new();
}

public class TeamPeriodReportInput : PagedAndSortedResultRequestDto
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public Guid? EmployeeId { get; set; }
}

public class TeamPendingReportInput : PagedAndSortedResultRequestDto
{
    public Guid? EmployeeId { get; set; }
}

public class TeamCustodyReportInput : PagedAndSortedResultRequestDto
{
    public Guid? EmployeeId { get; set; }
    public ReportCustodyKind? Kind { get; set; }
}

public class TeamEvaluationReportInput : PagedAndSortedResultRequestDto
{
    public Guid? CycleId { get; set; }
    public int? Year { get; set; }
    public Guid? EmployeeId { get; set; }
}

public class ReviewDashboardInput
{
    public int? Year { get; set; }
    public int? Month { get; set; }
}

public class EvaluationConsistencyReportInput : PagedAndSortedResultRequestDto
{
    public Guid? CycleId { get; set; }
    public Guid? DepartmentId { get; set; }
}

public class HrPeriodReportInput : PagedAndSortedResultRequestDto
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? EmployeeId { get; set; }
}

public class HrContractReportRowDto
{
    public Guid ContractId { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public string TypeName { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string StatusName { get; set; } = null!;
}

public class HrContractReportRowPagedResultDto
{
    public long TotalCount { get; set; }
    public List<HrContractReportRowDto> Items { get; set; } = new();
}

/// <summary>The incident description is absent by design.</summary>
public class HrWarningReportRowDto
{
    public Guid WarningId { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public string LevelName { get; set; } = null!;
    public DateOnly IncidentDate { get; set; }
    public string StatusName { get; set; } = null!;
}

public class HrWarningReportRowPagedResultDto
{
    public long TotalCount { get; set; }
    public List<HrWarningReportRowDto> Items { get; set; } = new();
}

public class HrContractReportInput : PagedAndSortedResultRequestDto
{
    public DateOnly? EndFrom { get; set; }
    public DateOnly? EndTo { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? EmployeeId { get; set; }
}

public class HrLeaveBalanceReportInput : PagedAndSortedResultRequestDto
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? EmployeeId { get; set; }
}

public class ReportExportInput
{
    public ReportExportFormat? Format { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? CycleId { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }
    public ReportCustodyKind? Kind { get; set; }
    public DateOnly? EndFrom { get; set; }
    public DateOnly? EndTo { get; set; }
}

/// <summary>The generated file. It never reaches JSON: the controller writes it as the response body.</summary>
public record ReportFile(byte[] Content, string ContentType, string FileName);

public interface IReportAppService : IApplicationService
{
    Task<TeamAttendanceRowPagedResultDto> GetTeamAttendanceAsync(TeamPeriodReportInput input);

    Task<TeamLatenessAbsenceRowPagedResultDto> GetTeamLatenessAbsenceAsync(TeamPeriodReportInput input);

    Task<TeamLeaveRowPagedResultDto> GetTeamLeavesAsync(TeamPeriodReportInput input);

    Task<TeamPendingRequestRowPagedResultDto> GetTeamPendingAsync(TeamPendingReportInput input);

    Task<TeamCustodyRowPagedResultDto> GetTeamCustodyAsync(TeamCustodyReportInput input);

    Task<TeamEvaluationRowPagedResultDto> GetTeamEvaluationsAsync(TeamEvaluationReportInput input);

    Task<ReviewDashboardDto> GetReviewDashboardAsync(ReviewDashboardInput input);

    Task<EvaluationConsistencyRowPagedResultDto> GetEvaluationConsistencyAsync(EvaluationConsistencyReportInput input);

    Task<HrAttendanceReportRowPagedResultDto> GetHrAttendanceAsync(HrPeriodReportInput input);
    Task<TodayAttendanceSummaryDto> GetTodayAttendanceSummaryAsync();

    Task<HrLeaveBalanceReportRowPagedResultDto> GetHrLeaveBalancesAsync(HrLeaveBalanceReportInput input);

    Task<HrRequestReportRowPagedResultDto> GetHrRequestsAsync(HrPeriodReportInput input);
    Task<HrContractReportRowPagedResultDto> GetHrContractsAsync(HrContractReportInput input);
    Task<HrWarningReportRowPagedResultDto> GetHrWarningsAsync(HrPeriodReportInput input);

    Task<ReportFile> ExportAsync(ReportCode code, ReportExportInput input);
}
