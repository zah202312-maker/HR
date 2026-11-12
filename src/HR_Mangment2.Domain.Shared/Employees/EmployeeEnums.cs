namespace HR_Mangment2.Employees;

public enum EmployeeStatus
{
    Active,
    Suspended,
    Ended
}

public enum AppGender
{
    Male,
    Female
}

public enum EmployeeDocumentType
{
    Identity,
    Passport,
    Contract,
    Certificate,
    Medical,
    Other
}

public enum DocumentSensitivity
{
    Normal,
    Sensitive
}

public enum DocumentScanStatus
{
    Pending,
    Clean,
    Infected
}

public enum AdministrativeTaskType
{
    RequestApproved,
    RequestRejected,
    RequestSubmittedOnBehalf,
    MemoProcessed,
    IncidentForwarded,
    EvaluationSubmitted,
    DepartmentReportSubmitted,
    CircularSent,
    CustodyCountRecorded,
    WarningRecommended,
    DelegationCreated,
    NoticeSent
}
