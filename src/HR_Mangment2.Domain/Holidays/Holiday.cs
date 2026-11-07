using System;
using System.Collections.Generic;
using HR_Mangment2.WorkShifts;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Holidays;

/// <summary>An official holiday of one day. A longer holiday has one row per day.</summary>
[Audited]
public class Holiday : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxNameLength = 128;

    public Guid? TenantId { get; private set; }
    public DateOnly Date { get; private set; }
    public string NameAr { get; private set; } = null!;
    public string NameEn { get; private set; } = null!;

    protected Holiday()
    {
    }

    public Holiday(Guid id, Guid? tenantId, DateOnly date, string nameAr, string nameEn)
        : base(id)
    {
        TenantId = tenantId;
        Set(date, nameAr, nameEn);
    }

    public void Set(DateOnly date, string nameAr, string nameEn)
    {
        Date = date;
        NameAr = nameAr;
        NameEn = nameEn;
    }
}

/// <summary>The company calendar for administrative dates. An employee's attendance follows their shift instead.</summary>
public class WeeklyOffSettings
{
    public List<WorkShiftDayOfWeek> Days { get; set; } = new() { WorkShiftDayOfWeek.Friday, WorkShiftDayOfWeek.Saturday };
}
