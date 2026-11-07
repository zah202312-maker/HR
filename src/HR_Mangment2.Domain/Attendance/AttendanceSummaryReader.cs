using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace HR_Mangment2.Attendance;

public sealed record AttendanceSum(int ScheduledDays, int PresentDays, int AbsentDays, int LateCount, int LateMinutes, decimal LeaveDays)
{
    public static readonly AttendanceSum Zero = new(0, 0, 0, 0, 0, 0);
}

/// <summary>One definition of an employee's attendance numbers over a period, shared by the reports, the evaluation form and the monthly report.</summary>
public class AttendanceSummaryReader : ITransientDependency
{
    private readonly IRepository<AttendanceDay, Guid> _days;
    private readonly IAsyncQueryableExecuter _executer;

    public AttendanceSummaryReader(IRepository<AttendanceDay, Guid> days, IAsyncQueryableExecuter executer)
    {
        _days = days;
        _executer = executer;
    }

    public virtual async Task<Dictionary<Guid, AttendanceSum>> SumAsync(IReadOnlyCollection<Guid> employeeIds, DateOnly from, DateOnly to)
    {
        var groups = await _executer.ToListAsync((await _days.GetQueryableAsync())
            .Where(x => employeeIds.Contains(x.EmployeeId) && x.Date >= from && x.Date <= to)
            .GroupBy(x => new { x.EmployeeId, x.Status })
            .Select(g => new
            {
                g.Key.EmployeeId,
                g.Key.Status,
                Count = g.Count(),
                LateCount = g.Count(x => x.LateMinutes > 0),
                LateMinutes = g.Sum(x => x.LateMinutes),
                LeaveDays = g.Sum(x => x.LeaveDays)
            }));

        return groups.GroupBy(x => x.EmployeeId).ToDictionary(g => g.Key, g => new AttendanceSum(
            g.Where(x => x.Status is AttendanceDayStatus.Present or AttendanceDayStatus.Late or AttendanceDayStatus.Absent or AttendanceDayStatus.Incomplete or AttendanceDayStatus.OnLeave).Sum(x => x.Count),
            g.Where(x => x.Status is AttendanceDayStatus.Present or AttendanceDayStatus.Late).Sum(x => x.Count),
            g.Where(x => x.Status == AttendanceDayStatus.Absent).Sum(x => x.Count),
            g.Sum(x => x.LateCount),
            g.Sum(x => x.LateMinutes),
            g.Sum(x => x.LeaveDays)));
    }
}
