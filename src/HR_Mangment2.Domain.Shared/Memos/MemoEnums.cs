namespace HR_Mangment2.Memos;

public enum MemoKind
{
    Leave,
    Problem
}

public enum MemoStatus
{
    PendingManager,
    PendingHr,
    ConvertedToRequest,
    ConvertedToIncident,
    Closed,
    PendingExecutive
}

public enum MemoConvertTarget
{
    LeaveRequest,
    Incident
}

public enum IncidentStatus
{
    PendingManager,
    PendingHr,
    InProgress,
    Resolved
}

public enum IncidentStatusTarget
{
    InProgress,
    Resolved
}

public static class MemoTimelineActions
{
    public const string Submitted = "submitted";
    public const string ForwardedToHr = "forwardedToHr";
    public const string ConvertedToRequest = "convertedToRequest";
    public const string ConvertedToIncident = "convertedToIncident";
    public const string Closed = "closed";
}

public static class IncidentTimelineActions
{
    public const string Submitted = "submitted";
    public const string ForwardedToHr = "forwardedToHr";
    public const string InProgress = "inProgress";
    public const string Resolved = "resolved";
}
