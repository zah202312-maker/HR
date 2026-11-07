namespace HR_Mangment2.Reports;

public enum ReportCode
{
    TeamAttendance,
    TeamLatenessAbsence,
    TeamLeaves,
    TeamPending,
    TeamCustody,
    TeamEvaluations,
    ReviewDashboard,
    EvaluationConsistency,
    HrAttendance,
    HrLeaveBalances,
    HrRequests,
    HrContracts,
    HrWarnings
}

public enum ReportExportFormat
{
    Pdf,
    Excel
}

public enum ReportCustodyKind
{
    Personal,
    Shared
}
