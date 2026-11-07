using System;
using System.Collections.Generic;
using System.Linq;
using HR_Mangment2.WorkShifts;

namespace HR_Mangment2.Attendance;

/// <summary>What the schedule says about one date for one employee, before looking at any punch.</summary>
public record AttendanceScheduleDay(
    DateOnly Date,
    WorkShift? Shift,
    bool IsHoliday,
    DateTime? ScheduledStart,
    DateTime? ScheduledEnd,
    DateTime StartOfDay);

public record AttendanceDayResult(
    Guid? WorkShiftId,
    DateTime? ScheduledStart,
    DateTime? ScheduledEnd,
    DateTime? OriginalCheckIn,
    DateTime? OriginalCheckOut,
    AttendanceDayStatus Status,
    int LateMinutes,
    int WorkedMinutes,
    int OvertimeMinutes,
    int? HourlyLeaveMinutes,
    Guid? LeaveRequestId,
    decimal LeaveDays,
    DateTime WindowStart,
    DateTime WindowEnd);

/// <summary>
/// The pure attendance rules, with no database, so every case can be tested directly. The document fixes the
/// outcomes (late, absent, worked hours, a night shift belongs to the day it started). The punch-to-day windows and
/// the single-punch rule are assumptions recorded in PROGRESS.md.
/// </summary>
public static class AttendanceDayEvaluator
{
    /// <summary>How long after an off-day-facing shift end a punch still counts as that shift's checkout.</summary>
    public static readonly TimeSpan CheckoutAllowance = TimeSpan.FromHours(6);

    /// <summary>Two punches closer than this are one touch of the device, not a checkin and a checkout.</summary>
    public static readonly TimeSpan SamePunchGap = TimeSpan.FromMinutes(5);

    /// <summary>A rotating cycle counts from the day the assignment began.</summary>
    public static WorkShiftDay? FindDay(WorkShift shift, DateOnly date, DateOnly assignmentStart)
    {
        if (shift.Type == WorkShiftType.Rotating)
        {
            var cycle = shift.RotationCycleDays!.Value;
            var offset = date.DayNumber - assignmentStart.DayNumber;
            var number = ((offset % cycle) + cycle) % cycle + 1;
            return shift.Days.FirstOrDefault(d => d.CycleDayNumber == number);
        }

        var dayOfWeek = (WorkShiftDayOfWeek)(int)date.DayOfWeek;
        return shift.Days.FirstOrDefault(d => d.DayOfWeek == dayOfWeek);
    }

    /// <summary>
    /// The moment punches stop belonging to <paramref name="day"/> and start belonging to <paramref name="next"/>:
    /// between two shifts the midpoint, after a last shift its end plus the allowance (never before midnight),
    /// before a first shift the allowance before it (never after midnight), and otherwise midnight.
    /// </summary>
    public static DateTime Boundary(AttendanceScheduleDay day, AttendanceScheduleDay next)
    {
        if (day.ScheduledEnd is { } end && next.ScheduledStart is { } start)
        {
            return end + TimeSpan.FromTicks((start - end).Ticks / 2);
        }

        if (day.ScheduledEnd is { } lastEnd)
        {
            var afterEnd = lastEnd + CheckoutAllowance;
            return afterEnd > next.StartOfDay ? afterEnd : next.StartOfDay;
        }

        if (next.ScheduledStart is { } firstStart)
        {
            var beforeStart = firstStart - CheckoutAllowance;
            return beforeStart < next.StartOfDay ? beforeStart : next.StartOfDay;
        }

        return next.StartOfDay;
    }

    /// <summary>Returns null when no row should exist yet: a work day still in progress with no punch and no leave.</summary>
    public static AttendanceDayResult? Evaluate(
        AttendanceScheduleDay day,
        IReadOnlyList<DateTime> punches,
        DateTime windowStart,
        DateTime windowEnd,
        AttendanceLeaveCoverage? leave,
        DateTime? correctedCheckIn,
        DateTime? correctedCheckOut,
        DateTime now)
    {
        var (originalIn, originalOut) = PairPunches(day, punches);
        var checkIn = correctedCheckIn ?? originalIn;
        var checkOut = correctedCheckOut ?? originalOut;
        var isWorkDay = day.ScheduledStart != null;

        var late = isWorkDay && checkIn != null ? LateMinutes(day, checkIn.Value, leave) : 0;
        var worked = checkIn != null && checkOut != null && checkOut > checkIn ? WholeMinutes(checkOut.Value - checkIn.Value) : 0;

        AttendanceDayStatus status;
        if (day.IsHoliday)
        {
            status = AttendanceDayStatus.Holiday;
        }
        else if (day.Shift == null)
        {
            status = AttendanceDayStatus.NoShift;
        }
        else if (!isWorkDay)
        {
            status = AttendanceDayStatus.WeeklyOff;
        }
        else if (leave is { IsFullDay: true })
        {
            status = AttendanceDayStatus.OnLeave;
        }
        else if (checkIn == null && checkOut == null)
        {
            if (now < day.ScheduledEnd)
            {
                return null;
            }

            status = AttendanceDayStatus.Absent;
        }
        else if (checkIn != null && (checkOut != null || now < day.ScheduledEnd))
        {
            status = late > 0 ? AttendanceDayStatus.Late : AttendanceDayStatus.Present;
        }
        else
        {
            status = AttendanceDayStatus.Incomplete;
        }

        return new AttendanceDayResult(
            day.Shift?.Id,
            day.ScheduledStart,
            day.ScheduledEnd,
            originalIn,
            originalOut,
            status,
            status == AttendanceDayStatus.OnLeave ? 0 : late,
            worked,
            OvertimeMinutes(day, checkOut, worked),
            leave?.HourlyMinutes,
            leave?.LeaveRequestId,
            status == AttendanceDayStatus.OnLeave ? leave!.LeaveDays : 0,
            windowStart,
            windowEnd);
    }

    /// <summary>
    /// Devices do not say in or out, so the first punch is the checkin and the last the checkout. A lone touch is
    /// whichever scheduled edge it is nearer to, so a forgotten checkin is not reported as a very late arrival.
    /// </summary>
    private static (DateTime? In, DateTime? Out) PairPunches(AttendanceScheduleDay day, IReadOnlyList<DateTime> punches)
    {
        if (punches.Count == 0)
        {
            return (null, null);
        }

        var first = punches[0];
        var last = punches[^1];
        if (last - first >= SamePunchGap)
        {
            return (first, last);
        }

        var nearerEnd = day.ScheduledStart is { } start && day.ScheduledEnd is { } end &&
                        (end - first).Duration() < (first - start).Duration();

        return nearerEnd ? (null, first) : (first, null);
    }

    /// <summary>Time covered by approved hourly leave is not late. What remains is compared with the grace period.</summary>
    private static int LateMinutes(AttendanceScheduleDay day, DateTime checkIn, AttendanceLeaveCoverage? leave)
    {
        var start = day.ScheduledStart!.Value;
        if (checkIn <= start)
        {
            return 0;
        }

        var lateSpan = checkIn - start;
        void Excuse(DateTime from, DateTime to)
        {
            var overlapStart = from > start ? from : start;
            var overlapEnd = to < checkIn ? to : checkIn;
            if (overlapEnd > overlapStart)
            {
                lateSpan -= overlapEnd - overlapStart;
            }
        }

        if (leave is { CoveredFrom: { } coveredFrom, CoveredTo: { } coveredTo })
        {
            Excuse(coveredFrom, coveredTo);
        }

        foreach (var period in leave?.OtherCoveredPeriods ?? Array.Empty<AttendanceCoveredPeriod>())
        {
            Excuse(period.From, period.To);
        }

        var minutes = WholeMinutes(lateSpan);
        return minutes > day.Shift!.GraceMinutes ? minutes : 0;
    }

    private static int OvertimeMinutes(AttendanceScheduleDay day, DateTime? checkOut, int worked)
    {
        var shift = day.Shift;
        if (shift == null || worked == 0)
        {
            return 0;
        }

        switch (shift.OvertimePolicy)
        {
            case WorkShiftOvertimePolicy.AfterShiftEnd:
                return day.ScheduledEnd is { } end && checkOut > end ? WholeMinutes(checkOut.Value - end) : 0;
            case WorkShiftOvertimePolicy.AfterDailyHours:
                var limit = (int)(shift.OvertimeAfterHours!.Value * 60);
                return worked > limit ? worked - limit : 0;
            default:
                return 0;
        }
    }

    private static int WholeMinutes(TimeSpan span) => (int)span.TotalMinutes;
}
