namespace HR_Mangment2.Evaluations;

public enum EvaluationCycleStatus
{
    Scheduled,
    Open,
    Closed
}

public enum EvaluationStatus
{
    Draft,
    Submitted,
    Acknowledged
}

public enum EvaluationObjectionStatus
{
    None,
    Pending,
    Accepted,
    Rejected
}

public enum EvaluationObjectionDecision
{
    Accepted,
    Rejected
}

public enum EvaluationResponseType
{
    Comment,
    Objection
}

public enum DepartmentReportStatus
{
    Draft,
    Submitted
}

public enum CircularScope
{
    Team,
    DepartmentManagers,
    Company
}
