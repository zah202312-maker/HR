namespace HR_Mangment2.Complaints;

/// <summary>In ascending order: comparing two levels compares their confidentiality.</summary>
public enum ComplaintLevel
{
    Normal,
    Confidential,
    HighlyConfidential
}

public enum ComplaintStatus
{
    PendingDepartmentManager,
    PendingHrManager,
    PendingExecutiveDirector,
    Closed
}

public enum ComplaintTargetKind
{
    Employee,
    General
}

public enum ComplaintRespondentCategory
{
    Colleague,
    DepartmentManager,
    StoreOfficer,
    Accountant,
    HrManager,
    Reviewer
}

public enum ComplaintHandlerRole
{
    DepartmentManager,
    HrManager,
    ExecutiveDirector
}

public enum ComplaintActionType
{
    Note,
    Meeting,
    Investigation,
    FollowUp,
    Submitted,
    LevelRaised,
    Forwarded,
    Closed
}

public enum ComplaintManualActionType
{
    Note,
    Meeting,
    Investigation,
    FollowUp
}

public enum ComplaintAccessAction
{
    ViewDetail,
    DownloadAttachment,
    UploadAttachment,
    ViewAccessLog,
    AddAction,
    RaiseLevel,
    Forward,
    Close
}

public enum ComplaintHandlerAssignmentStatus
{
    PendingApproval,
    Approved,
    Rejected,
    Revoked
}
