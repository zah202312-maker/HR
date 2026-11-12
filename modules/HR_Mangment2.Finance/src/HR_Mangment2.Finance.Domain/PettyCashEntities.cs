using System;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Finance;

// Petty cash (decision 2026-11-11): kept apart from the salary, never on a payslip.

/// <summary>A kind of petty cash, such as food or transport. Never deleted, deactivated instead.</summary>
[Audited]
public class PettyCashType : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string NameAr { get; private set; } = null!;
    public string NameEn { get; private set; } = null!;
    public bool IsActive { get; private set; }

    protected PettyCashType()
    {
    }

    public PettyCashType(Guid id, Guid? tenantId, string nameAr, string nameEn)
        : base(id)
    {
        TenantId = tenantId;
        Set(nameAr, nameEn, true);
    }

    public void Set(string nameAr, string nameEn, bool isActive)
    {
        NameAr = nameAr;
        NameEn = nameEn;
        IsActive = isActive;
    }
}

/// <summary>A fixed monthly amount of one type for one employee, from a month until stopped.</summary>
[Audited]
public class PettyCashAssignment : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid TypeId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;

    /// <summary>YYYY-MM, compared as text.</summary>
    public string FromPeriod { get; private set; } = null!;

    public string? ToPeriod { get; private set; }
    public string? Note { get; private set; }

    protected PettyCashAssignment()
    {
    }

    public PettyCashAssignment(Guid id, Guid? tenantId, Guid employeeId, Guid typeId, decimal amount, string currency, string fromPeriod, string? toPeriod, string? note)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        TypeId = typeId;
        Currency = currency;
        FromPeriod = fromPeriod;
        Set(amount, toPeriod, note);
    }

    public bool IsActiveIn(string period) => string.CompareOrdinal(FromPeriod, period) <= 0 && (ToPeriod == null || string.CompareOrdinal(ToPeriod, period) >= 0);

    public void Set(decimal amount, string? toPeriod, string? note)
    {
        Amount = amount;
        ToPeriod = toPeriod;
        Note = note;
    }
}

/// <summary>One employee's petty cash for one month. Created when the month is first read; delivering it creates an expense.</summary>
[Audited]
public class PettyCashEntry : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid AssignmentId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid TypeId { get; private set; }
    public string Period { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public PettyCashEntryStatus Status { get; private set; }
    public DateOnly? DeliveredOn { get; private set; }
    public Guid? DeliveredByUserId { get; private set; }
    public Guid? ExpenseId { get; private set; }

    protected PettyCashEntry()
    {
    }

    public PettyCashEntry(Guid id, Guid? tenantId, PettyCashAssignment assignment, string period)
        : base(id)
    {
        TenantId = tenantId;
        AssignmentId = assignment.Id;
        EmployeeId = assignment.EmployeeId;
        TypeId = assignment.TypeId;
        Period = period;
        Amount = assignment.Amount;
        Currency = assignment.Currency;
        Status = PettyCashEntryStatus.Pending;
    }

    public void ChangeAmount(decimal amount) => Amount = amount;

    public void MarkDelivered(DateOnly on, Guid userId, Guid expenseId)
    {
        Status = PettyCashEntryStatus.Delivered;
        DeliveredOn = on;
        DeliveredByUserId = userId;
        ExpenseId = expenseId;
    }

    public void UndoDelivery()
    {
        Status = PettyCashEntryStatus.Pending;
        DeliveredOn = null;
        DeliveredByUserId = null;
        ExpenseId = null;
    }
}
