namespace HR_Mangment2.WorkShifts;

public enum WorkShiftType
{
    Fixed,
    Rotating,
    Overnight
}

public enum WorkShiftOvertimePolicy
{
    None,
    AfterShiftEnd,
    AfterDailyHours
}

/// <summary>The values match System.DayOfWeek.</summary>
public enum WorkShiftDayOfWeek
{
    Sunday,
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday
}
