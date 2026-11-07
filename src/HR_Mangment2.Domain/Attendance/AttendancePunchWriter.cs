using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Attendance;

public record AttendancePunchInput(int Index, string DeviceUserId, DateTime PunchTime);

/// <summary>Saves raw punches from the device or a file under one unique rule, then queues the affected days.</summary>
public class AttendancePunchWriter : ITransientDependency
{
    private readonly IRepository<AttendancePunch, Guid> _punches;
    private readonly IRepository<DeviceUserMap, Guid> _maps;
    private readonly IAttendanceRecalculator _recalculator;
    private readonly ICompanyClock _companyClock;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;

    public AttendancePunchWriter(
        IRepository<AttendancePunch, Guid> punches,
        IRepository<DeviceUserMap, Guid> maps,
        IAttendanceRecalculator recalculator,
        ICompanyClock companyClock,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant)
    {
        _punches = punches;
        _maps = maps;
        _recalculator = recalculator;
        _companyClock = companyClock;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
    }

    /// <summary>The stored precision, so a repeat of the same punch always compares equal after a round trip.</summary>
    public static DateTime Normalize(DateTime punchTime)
    {
        var utc = DateTime.SpecifyKind(punchTime, DateTimeKind.Utc);
        return utc.AddTicks(-(utc.Ticks % 10));
    }

    /// <returns>The result of each input, by its index.</returns>
    public virtual async Task<Dictionary<int, AttendancePunchResultStatus>> WriteAsync(
        Guid deviceId,
        IReadOnlyCollection<AttendancePunchInput> items,
        AttendancePunchSource source,
        Guid? importId)
    {
        var result = new Dictionary<int, AttendancePunchResultStatus>();
        if (items.Count == 0)
        {
            return result;
        }

        var userIds = items.Select(x => x.DeviceUserId).Distinct().ToList();
        var times = items.Select(x => Normalize(x.PunchTime)).Distinct().ToList();

        var employeeByUser = (await _maps.GetListAsync(x => x.DeviceId == deviceId && userIds.Contains(x.DeviceUserId)))
            .ToDictionary(x => x.DeviceUserId, x => x.EmployeeId);

        var seen = (await _punches.GetListAsync(x => x.DeviceId == deviceId && userIds.Contains(x.DeviceUserId) && times.Contains(x.PunchTime)))
            .Select(x => (x.DeviceUserId, x.PunchTime))
            .ToHashSet();

        var added = new List<AttendancePunch>();
        foreach (var item in items)
        {
            var time = Normalize(item.PunchTime);
            if (!seen.Add((item.DeviceUserId, time)))
            {
                result[item.Index] = AttendancePunchResultStatus.Duplicate;
                continue;
            }

            Guid? employeeId = employeeByUser.TryGetValue(item.DeviceUserId, out var mapped) ? mapped : null;
            added.Add(new AttendancePunch(_guidGenerator.Create(), _currentTenant.Id, deviceId, item.DeviceUserId, time, source, importId, employeeId));
            result[item.Index] = employeeId == null ? AttendancePunchResultStatus.UnmappedDeviceUser : AttendancePunchResultStatus.Accepted;
        }

        if (added.Count > 0)
        {
            await _punches.InsertManyAsync(added, autoSave: true);
            await _recalculator.RecalculateRangesAsync(await ToRangesAsync(added));
        }

        return result;
    }

    /// <summary>A punch can belong to the day before or after its own date, because of night shifts and day windows.</summary>
    public virtual async Task<List<AttendanceRecalculationRange>> ToRangesAsync(IEnumerable<AttendancePunch> punches)
    {
        var zone = await _companyClock.GetTimeZoneAsync();

        return punches
            .Where(x => x.EmployeeId != null)
            .GroupBy(x => x.EmployeeId!.Value)
            .Select(group => new AttendanceRecalculationRange
            {
                EmployeeId = group.Key,
                From = CompanyClock.ToLocalDate(group.Min(x => x.PunchTime), zone).AddDays(-1),
                To = CompanyClock.ToLocalDate(group.Max(x => x.PunchTime), zone).AddDays(1)
            })
            .ToList();
    }
}
