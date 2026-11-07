namespace HR_Mangment2.Requests;

public enum RequestDestination
{
    None,
    Store,
    Accountant
}

public enum RequestStatus
{
    PendingManager,
    PendingHr,
    PendingExecutive,
    PendingDestination,
    Approved,
    Rejected,
    Completed,
    Cancelled
}

public enum RequestEmployeeStatus
{
    PendingManager,
    PendingHr,
    PendingExecutive,
    Approved,
    Rejected,
    Completed,
    Cancelled
}

public enum RequestStepDecision
{
    NotReached,
    Pending,
    Approved,
    ApprovedBySubmission,
    Rejected
}

public enum RequestInboxActionKind
{
    Decide,
    StockIssue
}

/// <summary>Who owns a request decides its route. See the approval matrix in the contract's info.description.</summary>
public enum RequestOwnerRow
{
    Employee,
    DepartmentManager,
    SystemAdmin,
    HrManager,
    ExecutiveDirector,

    /// <summary>Linked to the executive director or the HR manager, so the route skips the manager step (decision 2026-11-07).</summary>
    Reviewer
}

/// <summary>The seeded request types.</summary>
public static class RequestTypeCodes
{
    public const string Leave = "leave";
    public const string AttendanceCorrection = "attendanceCorrection";
    public const string StoreItem = "storeItem";
    public const string Accountant = "accountant";
    public const string AdministrativeNeed = "administrativeNeed";

    /// <summary>These two are submitted from their own modules, then decided through the request routes.</summary>
    public static bool UsesDedicatedEndpoint(string code) => code is Leave or AttendanceCorrection;
}

/// <summary>The contract's closed list of TimelineEntryDto.action values.</summary>
public static class RequestTimelineActions
{
    public const string Submitted = "submitted";
    public const string SubmittedOnBehalf = "submittedOnBehalf";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
    public const string Cancelled = "cancelled";
    public const string ApproverReassigned = "approverReassigned";
    public const string StockIssueAcknowledged = "stockIssueAcknowledged";
    public const string StockIssuePurchasing = "stockIssuePurchasing";
    public const string StockIssueDelivered = "stockIssueDelivered";
    public const string Completed = "completed";
}
