using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Attendance;

public interface IAttendanceRecalculator
{
    /// <summary>Recalculates every employee's attendance on a date, after a holiday changes.</summary>
    Task RecalculateDateAsync(DateOnly date);

    /// <summary>Recalculates one employee's attendance from a date, after a shift assignment that starts in the past.</summary>
    Task RecalculateEmployeeFromAsync(Guid employeeId, DateOnly fromDate);

    /// <summary>Recalculates employees after their punches arrive or get linked.</summary>
    Task RecalculateRangesAsync(IReadOnlyCollection<AttendanceRecalculationRange> ranges);
}

public class AttendanceRecalculationRange
{
    public Guid EmployeeId { get; set; }
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
}

/// <summary>Queues the work in the same transaction as the change that caused it, so neither is lost alone.</summary>
public class AttendanceRecalculator : IAttendanceRecalculator, ITransientDependency
{
    private readonly IBackgroundJobManager _jobs;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICompanyClock _companyClock;

    public AttendanceRecalculator(IBackgroundJobManager jobs, ICurrentTenant currentTenant, ICompanyClock companyClock)
    {
        _jobs = jobs;
        _currentTenant = currentTenant;
        _companyClock = companyClock;
    }

    public virtual async Task RecalculateDateAsync(DateOnly date)
    {
        if (date > await _companyClock.GetTodayAsync())
        {
            return;
        }

        await _jobs.EnqueueAsync(new AttendanceRecalculationArgs { TenantId = _currentTenant.Id, AllFrom = date, AllTo = date });
    }

    public virtual async Task RecalculateEmployeeFromAsync(Guid employeeId, DateOnly fromDate)
    {
        var range = new AttendanceRecalculationRange { EmployeeId = employeeId, From = fromDate, To = await _companyClock.GetTodayAsync() };
        await RecalculateRangesAsync(new[] { range });
    }

    public virtual async Task RecalculateRangesAsync(IReadOnlyCollection<AttendanceRecalculationRange> ranges)
    {
        if (ranges.Count > 0)
        {
            await _jobs.EnqueueAsync(new AttendanceRecalculationArgs { TenantId = _currentTenant.Id, Ranges = ranges.ToList() });
        }
    }
}
