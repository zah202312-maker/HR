using System;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Departments;

/// <summary>A department has no single manager: management comes from each employee's manager link.</summary>
[Audited]
public class Department : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxNameLength = 128;

    public Guid? TenantId { get; private set; }
    public string NameAr { get; private set; } = null!;
    public string NameEn { get; private set; } = null!;

    protected Department()
    {
    }

    public Department(Guid id, Guid? tenantId, string nameAr, string nameEn)
        : base(id)
    {
        TenantId = tenantId;
        Rename(nameAr, nameEn);
    }

    public void Rename(string nameAr, string nameEn)
    {
        NameAr = nameAr;
        NameEn = nameEn;
    }
}
