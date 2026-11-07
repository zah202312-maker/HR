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
