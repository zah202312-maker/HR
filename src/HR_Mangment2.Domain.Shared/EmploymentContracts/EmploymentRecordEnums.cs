namespace HR_Mangment2.EmploymentContracts
{
    public enum ContractStatus
    {
        Active,
        PendingRecommendation,
        PendingDecision,
        Renewed,
        NotRenewed
    }

    public enum ContractEmployeeStatus
    {
        Active,
        Renewed,
        NotRenewed
    }

    public enum ContractRenewalOption
    {
        Renew,
        DoNotRenew
    }
}

namespace HR_Mangment2.Warnings
{
    public enum WarningLevel
    {
        First,
        Final
    }

    public enum WarningStatus
    {
        Recommended,
        Notified,
        Responded,
        ResponseExpired,
        Issued,
        Dismissed,
        Objected,
        ObjectionUpheld,
        ObjectionOverturned
    }

    public enum WarningDecisionOutcome
    {
        Issue,
        Dismiss
    }

    public enum WarningObjectionOutcome
    {
        Uphold,
        Overturn
    }

    public enum WarningRecommendationStage
    {
        UnderReview,
        Issued,
        NotIssued
    }

    public enum WarningRecommenderRole
    {
        DepartmentManager,
        Reviewer
    }
}
