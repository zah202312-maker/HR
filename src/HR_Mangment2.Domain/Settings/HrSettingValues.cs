using System;
using HR_Mangment2.Attendance;

namespace HR_Mangment2.Settings;

/// <summary>
/// Initial values: document 5.10 and 5.13, and the approved decisions in QUESTIONS.md items 58, 59, 60 and 61.
/// </summary>
public class ReminderSettings
{
    public TimeOnly MonthEndOpenTime { get; set; } = new(10, 0);
    public int MonthlySubmissionWorkingDays { get; set; } = 3;
    public int MonthlyReminderIntervalWorkingDays { get; set; } = 1;
    public int MonthlyEscalationWorkingDay { get; set; } = 4;
    public int? ApprovalReminderIntervalWorkingDays { get; set; } = 2;
    public int? EvaluationMinWorkedDays { get; set; } = 15;
}

public class ContractSettings
{
    public int AlertDays { get; set; } = 30;
}

public class ComplaintSettings
{
    public int? DelayDeadlineWorkingDays { get; set; } = 10;
    public int? OverdueReminderIntervalWorkingDays { get; set; }
}

/// <summary>
/// The company's annual leave days per employee year (decision 2026-11-10). Each leave year is granted with the value
/// in force when it starts, so a change reaches everyone from their next year and never the current one.
/// </summary>
public class AnnualLeaveSettings
{
    public const decimal InitialDays = 24;

    public decimal DefaultEntitlementDays { get; set; } = InitialDays;
}

/// <summary>
/// The low average alert of HR evaluations (decision 2026-11-11): at the end of each cycle of <see cref="WindowWeeks"/>
/// weeks, an employee with at least <see cref="MinEvaluations"/> evaluations whose average on a heading is
/// <see cref="Threshold"/> or lower raises an alert to the HR managers.
/// </summary>
public class HrEvaluationAlertSettings
{
    public decimal Threshold { get; set; } = 7;
    public int WindowWeeks { get; set; } = 4;
    public int MinEvaluations { get; set; } = 4;
}

/// <summary>Internal: the last day of the last computed alert cycle. Not a settings section.</summary>
public class HrEvaluationAlertState
{
    public DateOnly? LastPeriodTo { get; set; }
}

public class HourlyLeaveSettings
{
    public decimal? MonthlyAllowedHours { get; set; } = 8;
    public decimal? HoursPerDay { get; set; } = 8;
}

public class AttendanceSettings
{
    public const string DefaultTimeZone = "Asia/Baghdad";

    public AttendanceCaptureMethod Method { get; set; } = AttendanceCaptureMethod.Device;
    public string TimeZone { get; set; } = DefaultTimeZone;
}

public class FinanceSettings
{
    public PayrollApproverRole? PayrollApproverRole { get; set; } = Settings.PayrollApproverRole.HrManager;
}
