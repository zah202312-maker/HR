using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HR_Mangment2.Attendance;

/// <summary>Approved leave on a date.</summary>
/// <param name="IsFullDay">The whole day is on leave, so the status is onLeave.</param>
/// <param name="LeaveDays">What the day counts as in the period's leave days, when it ends up onLeave.</param>
/// <param name="HourlyMinutes">Minutes of approved hourly leave, or null when none.</param>
/// <param name="CoveredFrom">Start of the hourly leave in UTC, which is excluded from lateness.</param>
/// <param name="CoveredTo">End of the hourly leave in UTC.</param>
public record AttendanceLeaveCoverage(
    Guid LeaveRequestId,
    bool IsFullDay,
    decimal LeaveDays,
    int? HourlyMinutes,
    DateTime? CoveredFrom,
    DateTime? CoveredTo,
    IReadOnlyList<AttendanceCoveredPeriod>? OtherCoveredPeriods = null);

/// <summary>A further hourly leave on the same day, in UTC.</summary>
public record AttendanceCoveredPeriod(DateTime From, DateTime To);

/// <summary>The leave module supplies the answer.</summary>
public interface IAttendanceLeaveReader
{
    Task<Dictionary<DateOnly, AttendanceLeaveCoverage>> GetCoverageAsync(Guid employeeId, DateOnly from, DateOnly to);
}
