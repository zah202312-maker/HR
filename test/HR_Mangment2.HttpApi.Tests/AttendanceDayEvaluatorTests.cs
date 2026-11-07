using System;
using System.Collections.Generic;
using HR_Mangment2.Attendance;
using HR_Mangment2.WorkShifts;
using Shouldly;
using Xunit;

namespace HR_Mangment2;

/// <summary>The attendance rules alone, with fixed times in UTC, so no database or clock is involved.</summary>
public class AttendanceDayEvaluatorTests
{
    private static readonly DateOnly Date = new(2026, 10, 4);
    private static readonly DateTime Start = new(2026, 10, 4, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime End = new(2026, 10, 4, 16, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Later = new(2026, 10, 5, 0, 0, 0, DateTimeKind.Utc);

    private static WorkShift Shift(WorkShiftOvertimePolicy policy = WorkShiftOvertimePolicy.None, decimal? afterHours = null, int grace = 10)
    {
        var shift = new WorkShift(Guid.NewGuid(), null);
        shift.Set("دوام", "Shift", WorkShiftType.Fixed, grace, policy, afterHours, null,
            new[] { new WorkShiftDay(WorkShiftDayOfWeek.Sunday, null, new TimeOnly(8, 0), new TimeOnly(16, 0), false) });
        return shift;
    }

    private static AttendanceScheduleDay WorkDay(WorkShift? shift = null) =>
        new(Date, shift ?? Shift(), false, Start, End, Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));

    private static AttendanceDayResult? Evaluate(
        AttendanceScheduleDay day,
        IReadOnlyList<DateTime> punches,
        DateTime? now = null,
        AttendanceLeaveCoverage? leave = null,
        DateTime? correctedIn = null,
        DateTime? correctedOut = null) =>
        AttendanceDayEvaluator.Evaluate(day, punches, Start.AddHours(-8), Later, leave, correctedIn, correctedOut, now ?? Later);

    private static DateTime At(int hour, int minute = 0) => new(2026, 10, 4, hour, minute, 0, DateTimeKind.Utc);

    [Fact]
    public void On_time_with_both_punches_is_present_and_counts_worked_minutes()
    {
        var result = Evaluate(WorkDay(), new[] { At(8), At(16) })!;

        result.Status.ShouldBe(AttendanceDayStatus.Present);
        result.LateMinutes.ShouldBe(0);
        result.WorkedMinutes.ShouldBe(480);
        result.OriginalCheckIn.ShouldBe(At(8));
        result.OriginalCheckOut.ShouldBe(At(16));
    }

    [Fact]
    public void Lateness_within_the_grace_period_counts_as_zero()
    {
        var result = Evaluate(WorkDay(), new[] { At(8, 10), At(16) })!;

        result.Status.ShouldBe(AttendanceDayStatus.Present);
        result.LateMinutes.ShouldBe(0);
    }

    [Fact]
    public void Lateness_past_the_grace_period_counts_from_the_shift_start()
    {
        var result = Evaluate(WorkDay(), new[] { At(8, 11), At(16) })!;

        result.Status.ShouldBe(AttendanceDayStatus.Late);
        result.LateMinutes.ShouldBe(11);
    }

    [Fact]
    public void Time_covered_by_hourly_leave_is_not_late()
    {
        var leave = new AttendanceLeaveCoverage(Guid.NewGuid(), false, 0, 60, At(8), At(9));

        var covered = Evaluate(WorkDay(), new[] { At(9), At(16) }, leave: leave)!;
        covered.Status.ShouldBe(AttendanceDayStatus.Present);
        covered.LateMinutes.ShouldBe(0);
        covered.HourlyLeaveMinutes.ShouldBe(60);
        covered.LeaveRequestId.ShouldBe(leave.LeaveRequestId);

        var partly = Evaluate(WorkDay(), new[] { At(9, 20), At(16) }, leave: leave)!;
        partly.Status.ShouldBe(AttendanceDayStatus.Late);
        partly.LateMinutes.ShouldBe(20);
    }

    [Fact]
    public void A_work_day_with_no_punch_is_absent_only_after_the_shift_ends()
    {
        Evaluate(WorkDay(), Array.Empty<DateTime>(), now: At(15)).ShouldBeNull();
        Evaluate(WorkDay(), Array.Empty<DateTime>(), now: End)!.Status.ShouldBe(AttendanceDayStatus.Absent);
    }

    [Fact]
    public void Only_a_checkin_is_present_during_the_shift_and_incomplete_after_it()
    {
        var during = Evaluate(WorkDay(), new[] { At(8) }, now: At(12))!;
        during.Status.ShouldBe(AttendanceDayStatus.Present);
        during.OriginalCheckOut.ShouldBeNull();

        Evaluate(WorkDay(), new[] { At(8, 30) }, now: At(12))!.Status.ShouldBe(AttendanceDayStatus.Late);
        Evaluate(WorkDay(), new[] { At(8) }, now: End)!.Status.ShouldBe(AttendanceDayStatus.Incomplete);
    }

    [Fact]
    public void A_lone_punch_near_the_shift_end_is_a_checkout_not_a_very_late_arrival()
    {
        var result = Evaluate(WorkDay(), new[] { At(15, 55) })!;

        result.Status.ShouldBe(AttendanceDayStatus.Incomplete);
        result.OriginalCheckIn.ShouldBeNull();
        result.OriginalCheckOut.ShouldBe(At(15, 55));
        result.LateMinutes.ShouldBe(0);
    }

    [Fact]
    public void Two_touches_a_moment_apart_are_one_punch()
    {
        var result = Evaluate(WorkDay(), new[] { At(8), At(8, 2) })!;

        result.OriginalCheckIn.ShouldBe(At(8));
        result.OriginalCheckOut.ShouldBeNull();
        result.Status.ShouldBe(AttendanceDayStatus.Incomplete);
    }

    [Fact]
    public void Corrected_times_win_over_the_punches_and_the_originals_are_kept()
    {
        var result = Evaluate(WorkDay(), new[] { At(9) }, correctedIn: At(8), correctedOut: At(16))!;

        result.Status.ShouldBe(AttendanceDayStatus.Present);
        result.WorkedMinutes.ShouldBe(480);
        result.OriginalCheckIn.ShouldBe(At(9));
    }

    [Fact]
    public void A_holiday_an_off_day_a_leave_day_and_no_shift_each_have_their_status()
    {
        var midnight = Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        Evaluate(new AttendanceScheduleDay(Date, Shift(), true, null, null, midnight), Array.Empty<DateTime>())!.Status.ShouldBe(AttendanceDayStatus.Holiday);
        Evaluate(new AttendanceScheduleDay(Date, Shift(), false, null, null, midnight), Array.Empty<DateTime>())!.Status.ShouldBe(AttendanceDayStatus.WeeklyOff);
        Evaluate(new AttendanceScheduleDay(Date, null, false, null, null, midnight), Array.Empty<DateTime>())!.Status.ShouldBe(AttendanceDayStatus.NoShift);

        var leave = Evaluate(WorkDay(), Array.Empty<DateTime>(), leave: new AttendanceLeaveCoverage(Guid.NewGuid(), true, 1, null, null, null))!;
        leave.Status.ShouldBe(AttendanceDayStatus.OnLeave);
        leave.LeaveDays.ShouldBe(1);
    }

    [Fact]
    public void Overtime_follows_the_policy_of_the_shift()
    {
        Evaluate(WorkDay(Shift()), new[] { At(8), At(18) })!.OvertimeMinutes.ShouldBe(0);
        Evaluate(WorkDay(Shift(WorkShiftOvertimePolicy.AfterShiftEnd)), new[] { At(8), At(17, 30) })!.OvertimeMinutes.ShouldBe(90);
        Evaluate(WorkDay(Shift(WorkShiftOvertimePolicy.AfterShiftEnd)), new[] { At(8), At(15) })!.OvertimeMinutes.ShouldBe(0);
        Evaluate(WorkDay(Shift(WorkShiftOvertimePolicy.AfterDailyHours, 6)), new[] { At(9), At(16) })!.OvertimeMinutes.ShouldBe(60);
    }

    [Fact]
    public void A_night_shift_keeps_its_morning_punch_and_a_following_shift_splits_at_the_midpoint()
    {
        var night = new AttendanceScheduleDay(
            Date,
            Shift(),
            false,
            new DateTime(2026, 10, 4, 19, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 10, 5, 3, 0, 0, DateTimeKind.Utc),
            Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));

        var nextDate = Date.AddDays(1);
        var midnight = nextDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var free = new AttendanceScheduleDay(nextDate, Shift(), false, null, null, midnight);
        AttendanceDayEvaluator.Boundary(night, free).ShouldBe(new DateTime(2026, 10, 5, 9, 0, 0, DateTimeKind.Utc));

        var working = new AttendanceScheduleDay(
            nextDate,
            Shift(),
            false,
            new DateTime(2026, 10, 5, 19, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 10, 6, 3, 0, 0, DateTimeKind.Utc),
            midnight);
        AttendanceDayEvaluator.Boundary(night, working).ShouldBe(new DateTime(2026, 10, 5, 11, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void An_ordinary_day_shift_ends_its_window_at_midnight_before_an_off_day()
    {
        var nextDate = Date.AddDays(1);
        var midnight = nextDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        AttendanceDayEvaluator.Boundary(WorkDay(), new AttendanceScheduleDay(nextDate, Shift(), false, null, null, midnight)).ShouldBe(midnight);
    }

    [Fact]
    public void A_rotating_cycle_counts_from_the_assignment_start()
    {
        var shift = new WorkShift(Guid.NewGuid(), null);
        shift.Set("متناوب", "Rotating", WorkShiftType.Rotating, 0, WorkShiftOvertimePolicy.None, null, 3, new[]
        {
            new WorkShiftDay(null, 1, new TimeOnly(8, 0), new TimeOnly(16, 0), false),
            new WorkShiftDay(null, 2, new TimeOnly(16, 0), new TimeOnly(23, 0), false)
        });

        var assigned = new DateOnly(2026, 10, 1);

        AttendanceDayEvaluator.FindDay(shift, assigned, assigned)!.CycleDayNumber.ShouldBe(1);
        AttendanceDayEvaluator.FindDay(shift, assigned.AddDays(1), assigned)!.CycleDayNumber.ShouldBe(2);
        AttendanceDayEvaluator.FindDay(shift, assigned.AddDays(2), assigned).ShouldBeNull();
        AttendanceDayEvaluator.FindDay(shift, assigned.AddDays(3), assigned)!.CycleDayNumber.ShouldBe(1);
    }

    [Fact]
    public void Each_hourly_leave_of_the_day_excuses_lateness_at_its_own_time()
    {
        var first = new AttendanceLeaveCoverage(Guid.NewGuid(), false, 0, 120, At(8), At(9), new[] { new AttendanceCoveredPeriod(At(9), At(10)) });
        Evaluate(WorkDay(), new[] { At(10), At(16) }, leave: first)!.LateMinutes.ShouldBe(0);

        // Only the earliest leave would leave the second hour counted as late.
        var earliestOnly = new AttendanceLeaveCoverage(Guid.NewGuid(), false, 0, 60, At(8), At(9));
        Evaluate(WorkDay(), new[] { At(10), At(16) }, leave: earliestOnly)!.LateMinutes.ShouldBe(60);

        // A later leave does not excuse lateness outside its own time.
        var later = new AttendanceLeaveCoverage(Guid.NewGuid(), false, 0, 120, At(8), At(9), new[] { new AttendanceCoveredPeriod(At(14), At(15)) });
        Evaluate(WorkDay(), new[] { At(10), At(16) }, leave: later)!.LateMinutes.ShouldBe(60);
    }
}
