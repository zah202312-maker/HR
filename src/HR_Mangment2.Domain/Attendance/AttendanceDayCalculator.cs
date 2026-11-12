using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Holidays;
using HR_Mangment2.Identity;
using HR_Mangment2.WorkShifts;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;

namespace HR_Mangment2.Attendance;

/// <summary>Loads what the rules need and stores the computed days. Running it again gives the same rows.</summary>
public class AttendanceDayCalculator : ITransientDependency
{
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IRepository<EmployeeShiftAssignment, Guid> _assignments;
    private readonly IRepository<WorkShift, Guid> _shifts;
    private readonly IRepository<Holiday, Guid> _holidays;
    private readonly IRepository<AttendancePunch, Guid> _punches;
    private readonly IRepository<AttendanceDay, Guid> _days;
    private readonly IAttendanceLeaveReader _leave;
    private readonly ManagerLinkService _managerLinks;
    private readonly ICompanyClock _companyClock;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly IClock _clock;

    public AttendanceDayCalculator(
        IRepository<Employee, Guid> employees,
        IRepository<EmployeeShiftAssignment, Guid> assignments,
        IRepository<WorkShift, Guid> shifts,
        IRepository<Holiday, Guid> holidays,
        IRepository<AttendancePunch, Guid> punches,
        IRepository<AttendanceDay, Guid> days,
        IAttendanceLeaveReader leave,
        ManagerLinkService managerLinks,
        ICompanyClock companyClock,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IClock clock)
    {
        _employees = employees;
        _assignments = assignments;
        _shifts = shifts;
        _holidays = holidays;
        _punches = punches;
        _days = days;
        _leave = leave;
        _managerLinks = managerLinks;
        _companyClock = companyClock;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _clock = clock;
    }

    public virtual async Task RecalculateAsync(Guid employeeId, DateOnly from, DateOnly to)
    {
        var employee = await _employees.FindAsync(employeeId);
        if (employee == null || ManagerLinkService.IsOutsideSchedule(await _managerLinks.GetRolesAsync(employee)))
        {
            return;
        }

        var zone = await _companyClock.GetTimeZoneAsync();
        var now = _clock.Now;
        var today = CompanyClock.ToLocalDate(now, zone);

        var first = from > employee.HireDate ? from : employee.HireDate;
        var last = to < today ? to : today;
        if (first > last)
        {
            return;
        }

        // One extra day on each side, because the neighbouring shifts decide where this day's punches end.
        var schedule = await BuildScheduleAsync(employeeId, first.AddDays(-1), last.AddDays(1), zone);
        var existing = (await _days.GetListAsync(x => x.EmployeeId == employeeId && x.Date >= first && x.Date <= last)).ToDictionary(x => x.Date);
        var coverage = await _leave.GetCoverageAsync(employeeId, first, last);

        var windowStart = AttendanceDayEvaluator.Boundary(schedule[first.AddDays(-1)], schedule[first]);
        var windowEnd = AttendanceDayEvaluator.Boundary(schedule[last], schedule[last.AddDays(1)]);
        var punches = (await _punches.GetListAsync(x => x.EmployeeId == employeeId && x.PunchTime >= windowStart && x.PunchTime < windowEnd))
            .Select(x => x.PunchTime)
            .OrderBy(x => x)
            .ToList();

        var canCreateAbsence = employee.Status != EmployeeStatus.Ended;

        for (var date = first; date <= last; date = date.AddDays(1))
        {
            var dayStart = AttendanceDayEvaluator.Boundary(schedule[date.AddDays(-1)], schedule[date]);
            var dayEnd = AttendanceDayEvaluator.Boundary(schedule[date], schedule[date.AddDays(1)]);
            var dayPunches = punches.Where(p => p >= dayStart && p < dayEnd).ToList();
            existing.TryGetValue(date, out var row);

            var result = AttendanceDayEvaluator.Evaluate(
                schedule[date],
                dayPunches,
                dayStart,
                dayEnd,
                coverage.GetValueOrDefault(date),
                row?.CorrectedCheckIn,
                row?.CorrectedCheckOut,
                now);

            // There is no end date on the file, so an ended employee keeps real records but gets no new empty days.
            if (result == null || (row == null && !canCreateAbsence && dayPunches.Count == 0))
            {
                if (row != null)
                {
                    await _days.DeleteAsync(row);
                }

                continue;
            }

            if (row == null)
            {
                row = new AttendanceDay(_guidGenerator.Create(), _currentTenant.Id, employeeId, date);
                row.Apply(result);
                await _days.InsertAsync(row);
            }
            else
            {
                row.Apply(result);
                await _days.UpdateAsync(row);
            }
        }
    }

    private async Task<Dictionary<DateOnly, AttendanceScheduleDay>> BuildScheduleAsync(Guid employeeId, DateOnly from, DateOnly to, TimeZoneInfo zone)
    {
        var assignments = await _assignments.GetListAsync(x =>
            x.EmployeeId == employeeId && x.StartDate <= to && (x.EndDate == null || x.EndDate >= from));

        var shifts = new Dictionary<Guid, WorkShift>();
        foreach (var id in assignments.Select(x => x.WorkShiftId).Distinct())
        {
            shifts[id] = await _shifts.GetAsync(id);
        }

        var holidays = (await _holidays.GetListAsync(x => x.Date >= from && x.Date <= to)).Select(x => x.Date).ToHashSet();
        var result = new Dictionary<DateOnly, AttendanceScheduleDay>();

        for (var date = from; date <= to; date = date.AddDays(1))
        {
            var assignment = assignments.FirstOrDefault(x => x.StartDate <= date && (x.EndDate == null || x.EndDate >= date));
            var shift = assignment == null ? null : shifts[assignment.WorkShiftId];
            var isHoliday = holidays.Contains(date);
            var shiftDay = shift == null || isHoliday ? null : AttendanceDayEvaluator.FindDay(shift, date, assignment!.StartDate);

            result[date] = new AttendanceScheduleDay(
                date,
                shift,
                isHoliday,
                shiftDay == null ? null : CompanyClock.ToUtc(date, shiftDay.StartTime, zone),
                shiftDay == null ? null : CompanyClock.ToUtc(shiftDay.CrossesMidnight ? date.AddDays(1) : date, shiftDay.EndTime, zone),
                CompanyClock.ToUtc(date, TimeOnly.MinValue, zone));
        }

        return result;
    }
}
