using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HR_Mangment2.Auditing;
using Volo.Abp.Auditing;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Settings;

public record SettingsSnapshot<T>(T Value, string ConcurrencyStamp, DateTime? LastModificationTime);

/// <summary>Reads and writes a typed settings section. Other modules read their settings from here.</summary>
public class HrSettingsStore : ITransientDependency
{
    private readonly IRepository<SystemSettingSection, Guid> _repository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly IAuditingManager _auditingManager;

    public HrSettingsStore(
        IRepository<SystemSettingSection, Guid> repository,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IAuditingManager auditingManager)
    {
        _repository = repository;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _auditingManager = auditingManager;
    }

    public virtual async Task<T> GetValueAsync<T>() where T : class, new() => (await GetAsync<T>()).Value;

    public virtual async Task<SettingsSnapshot<T>> GetAsync<T>() where T : class, new()
    {
        var row = await GetOrCreateAsync<T>();
        return new SettingsSnapshot<T>(JsonSerializer.Deserialize<T>(row.ValueJson)!, row.ConcurrencyStamp, row.LastModificationTime);
    }

    /// <summary>Reads without creating the row, for code that runs on every request or inside other work.</summary>
    public virtual async Task<T> PeekValueAsync<T>() where T : class, new()
    {
        var section = typeof(T).Name;
        var row = await _repository.FirstOrDefaultAsync(x => x.Section == section);
        return row == null ? new T() : JsonSerializer.Deserialize<T>(row.ValueJson)!;
    }

    /// <summary>A stale stamp makes the save fail with a concurrency error, which the API returns as 409.</summary>
    public virtual async Task<SettingsSnapshot<T>> SetAsync<T>(T value, string concurrencyStamp) where T : class, new()
    {
        var row = await GetOrCreateAsync<T>();
        var newJson = JsonSerializer.Serialize(value);
        HrAuditEntryBuilder.SetChangedFields(_auditingManager, HrAuditEntryBuilder.DiffJson(row.ValueJson, newJson));

        row.ConcurrencyStamp = concurrencyStamp;
        row.SetValue(newJson);
        await _repository.UpdateAsync(row, autoSave: true);

        return new SettingsSnapshot<T>(value, row.ConcurrencyStamp, row.LastModificationTime);
    }

    private async Task<SystemSettingSection> GetOrCreateAsync<T>() where T : class, new()
    {
        var section = typeof(T).Name;
        var row = await _repository.FirstOrDefaultAsync(x => x.Section == section);
        if (row != null)
        {
            return row;
        }

        row = new SystemSettingSection(_guidGenerator.Create(), _currentTenant.Id, section, JsonSerializer.Serialize(new T()));
        return await _repository.InsertAsync(row, autoSave: true);
    }
}
