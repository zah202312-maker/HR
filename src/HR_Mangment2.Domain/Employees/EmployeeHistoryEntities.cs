using System;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Employees;

/// <summary>One period of an employee's manager. Rows are never deleted. CreatorId is who changed it.</summary>
[Audited]
public class EmployeeManagerLink : CreationAuditedEntity<Guid>, IMultiTenant
{
    public const int MaxReasonLength = 2000;

    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid ManagerEmployeeId { get; private set; }
    public DateOnly FromDate { get; private set; }
    public DateOnly? ToDate { get; private set; }
    public string? Reason { get; private set; }

    protected EmployeeManagerLink()
    {
    }

    public EmployeeManagerLink(Guid id, Guid? tenantId, Guid employeeId, Guid managerEmployeeId, DateOnly fromDate, string? reason)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        ManagerEmployeeId = managerEmployeeId;
        FromDate = fromDate;
        Reason = reason;
    }

    public void Close(DateOnly toDate) => ToDate = toDate;
}

/// <summary>One period of an employee's shift. A null EndDate marks the current one.</summary>
[Audited]
public class EmployeeShiftAssignment : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid WorkShiftId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }

    protected EmployeeShiftAssignment()
    {
    }

    public EmployeeShiftAssignment(Guid id, Guid? tenantId, Guid employeeId, Guid workShiftId, DateOnly startDate)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        WorkShiftId = workShiftId;
        StartDate = startDate;
    }

    public void Close(DateOnly endDate) => EndDate = endDate;
}

/// <summary>The administrative work of a department manager. Other modules add rows through AdministrativeTaskLogger.</summary>
public class AdministrativeTaskLogEntry : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public AdministrativeTaskType TaskType { get; private set; }
    public Guid? ReferenceId { get; private set; }

    protected AdministrativeTaskLogEntry()
    {
    }

    public AdministrativeTaskLogEntry(Guid id, Guid? tenantId, Guid employeeId, DateTime occurredAt, AdministrativeTaskType taskType, Guid? referenceId)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        OccurredAt = occurredAt;
        TaskType = taskType;
        ReferenceId = referenceId;
    }
}
