namespace HR_Mangment2.Delegations;

public enum DelegationScope
{
    AllApprovals,
    Requests,
    MemosAndIncidents,

    /// <summary>The reviewer's follow-up reports (decision 2026-11-11). Not part of AllApprovals.</summary>
    ReviewReports
}

public enum DelegationDirection
{
    Given,
    Received
}

public enum DelegationStatus
{
    Scheduled,
    Active,
    Expired,
    Cancelled
}
