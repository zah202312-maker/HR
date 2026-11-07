using System;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Holidays;
using HR_Mangment2.Settings;
using HR_Mangment2.WorkShifts;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace HR_Mangment2.Common;

/// <summary>The company calendar for administrative deadlines. An employee's own attendance follows their shift instead.</summary>
public class WorkCalendarService : ITransientDependency
{
    private readonly IRepository<Holiday, Guid> _holidays;
    private readonly HrSettingsStore _settings;

    public WorkCalendarService(IRepository<Holiday, Guid> holidays, HrSettingsStore settings)
    {
        _holidays = holidays;
        _settings = settings;
    }

    /// <summary>Counts from the working day after the event, as the contract's shared rules require. The event's own day never counts.</summary>
    public virtual async Task<int> CountWorkingDaysAfterAsync(DateOnly eventDate, DateOnly through)
    {
        if (through <= eventDate)
        {
            return 0;
        }

        var weeklyOff = (await _settings.PeekValueAsync<WeeklyOffSettings>()).Days.ToHashSet();
        var holidays = (await _holidays.GetListAsync(x => x.Date > eventDate && x.Date <= through)).Select(x => x.Date).ToHashSet();

        var count = 0;
        for (var date = eventDate.AddDays(1); date <= through; date = date.AddDays(1))
        {
            if (!holidays.Contains(date) && !weeklyOff.Contains(ToShiftDay(date.DayOfWeek)))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>The month's last working day. Falls back to the last day of the month if the whole month is off.</summary>
    public virtual async Task<DateOnly> GetLastWorkingDayOfMonthAsync(int year, int month)
    {
        var last = new DateOnly(year, month, 1).AddMonths(1).AddDays(-1);
        for (var date = last; date.Month == month; date = date.AddDays(-1))
        {
            if (await CountWorkingDaysAfterAsync(date.AddDays(-1), date) == 1)
            {
                return date;
            }
        }

        return last;
    }

    /// <summary>The date of the given working day after the event, counted the same way. Null if the calendar has no working days at all.</summary>
    public virtual async Task<DateOnly?> AddWorkingDaysAsync(DateOnly eventDate, int workingDays)
    {
        var weeklyOff = (await _settings.PeekValueAsync<WeeklyOffSettings>()).Days.ToHashSet();
        if (weeklyOff.Count >= 7)
        {
            return null;
        }

        // Generous enough for any weekly pattern with a year's holidays on top.
        var horizon = eventDate.AddDays(workingDays * 7 + 366);
        var holidays = (await _holidays.GetListAsync(x => x.Date > eventDate && x.Date <= horizon)).Select(x => x.Date).ToHashSet();

        var date = eventDate;
        for (var left = workingDays; left > 0 && date < horizon;)
        {
            date = date.AddDays(1);
            if (!holidays.Contains(date) && !weeklyOff.Contains(ToShiftDay(date.DayOfWeek)))
            {
                left--;
            }
        }

        return date;
    }

    private static WorkShiftDayOfWeek ToShiftDay(DayOfWeek day) => Enum.Parse<WorkShiftDayOfWeek>(day.ToString());
}
