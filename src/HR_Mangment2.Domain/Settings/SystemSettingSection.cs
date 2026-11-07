using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Settings;

/// <summary>One settings section as a typed document, with its own concurrency stamp.</summary>
public class SystemSettingSection : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string Section { get; private set; } = null!;
    public string ValueJson { get; private set; } = null!;

    protected SystemSettingSection()
    {
    }

    public SystemSettingSection(Guid id, Guid? tenantId, string section, string valueJson)
        : base(id)
    {
        TenantId = tenantId;
        Section = section;
        ValueJson = valueJson;
    }

    public void SetValue(string valueJson) => ValueJson = valueJson;
}
