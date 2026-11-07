namespace HR_Mangment2.Leaves;

public enum LeaveKind
{
    Hourly,
    Annual,
    Other
}

public enum LeaveLedgerType
{
    Grant,
    CarryIn,
    Hold,
    Release,
    Deduct,
    Adjust,
    Reverse
}

public enum LeaveLedgerBucket
{
    Carried,
    Entitlement,
    Hourly
}

public enum LeaveBalanceEffect
{
    None,
    Held,
    Deducted,
    Released
}

public enum LeaveDayClassification
{
    WorkingDay,
    WeeklyOff,
    Holiday
}

public enum OtherLeaveEligibleCategory
{
    All,
    Male,
    Female
}
