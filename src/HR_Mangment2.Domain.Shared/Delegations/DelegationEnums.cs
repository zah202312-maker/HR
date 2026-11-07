namespace HR_Mangment2.Delegations;

public enum DelegationScope
{
    AllApprovals,
    Requests,
    MemosAndIncidents
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
