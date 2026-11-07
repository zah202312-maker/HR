using System;
using System.Threading.Tasks;
using HR_Mangment2.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;

namespace HR_Mangment2.Employees;

/// <summary>Dates in the company time zone, which the system administrator sets in the attendance settings.</summary>
public interface ICompanyClock
{
    Task<TimeZoneInfo> GetTimeZoneAsync();

    Task<DateOnly> GetTodayAsync();

    /// <summary>The UTC moment a company-local date begins.</summary>
    Task<DateTime> StartOfDayUtcAsync(DateOnly date);
}

public class CompanyClock : ICompanyClock, ITransientDependency
{
    private readonly IClock _clock;
    private readonly HrSettingsStore _settings;

    public CompanyClock(IClock clock, HrSettingsStore settings)
    {
        _clock = clock;
        _settings = settings;
    }

    public virtual async Task<TimeZoneInfo> GetTimeZoneAsync() =>
        TimeZoneInfo.FindSystemTimeZoneById((await _settings.PeekValueAsync<AttendanceSettings>()).TimeZone);

    public virtual async Task<DateOnly> GetTodayAsync() => ToLocalDate(_clock.Now, await GetTimeZoneAsync());

    public virtual async Task<DateTime> StartOfDayUtcAsync(DateOnly date) => ToUtc(date, TimeOnly.MinValue, await GetTimeZoneAsync());

    public static DateOnly ToLocalDate(DateTime utc, TimeZoneInfo zone) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone));

    /// <summary>A local time skipped by a clock change is taken as the same time one hour later.</summary>
    public static DateTime ToUtc(DateOnly date, TimeOnly time, TimeZoneInfo zone)
    {
        var local = date.ToDateTime(time, DateTimeKind.Unspecified);
        if (zone.IsInvalidTime(local))
        {
            local = local.AddHours(1);
        }

        return TimeZoneInfo.ConvertTimeToUtc(local, zone);
    }
}

/// <summary>Moves pending items to a new manager. The requests, memos and incidents modules supply the real work.</summary>
public interface IManagerChangeEffects
{
    /// <summary>Each module holding work for the old manager registers its own. All of them run.</summary>
    /// <returns>How many pending items moved. Complaints are never counted.</returns>
    Task<int> MovePendingItemsAsync(Guid employeeId, Guid? previousManagerEmployeeId, Guid newManagerEmployeeId);
}

/// <summary>Whether leave ledger entries lock the hire date. The leave module supplies the answer.</summary>
public interface ILeaveLedgerReader
{
    Task<bool> HasEntriesAsync(Guid employeeId);
}

/// <summary>Writes the administrative task log of the employee holding this account.</summary>
public class AdministrativeTaskLogger : ITransientDependency
{
    private readonly IRepository<AdministrativeTaskLogEntry, Guid> _entries;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly IClock _clock;

    public AdministrativeTaskLogger(
        IRepository<AdministrativeTaskLogEntry, Guid> entries,
        IRepository<Employee, Guid> employees,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IClock clock)
    {
        _entries = entries;
        _employees = employees;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _clock = clock;
    }

    public virtual async Task LogAsync(Guid userId, AdministrativeTaskType taskType, Guid? referenceId)
    {
        var employee = await _employees.FirstOrDefaultAsync(x => x.UserId == userId);
        if (employee != null)
        {
            await _entries.InsertAsync(new AdministrativeTaskLogEntry(_guidGenerator.Create(), _currentTenant.Id, employee.Id, _clock.Now, taskType, referenceId));
        }
    }
}
