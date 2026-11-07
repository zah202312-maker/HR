using System;
using System.Collections.Generic;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Store;

public static class StoreConsts
{
    public const int MaxNameLength = 128;
    public const int MaxNoteLength = 2000;
    public const int QuantityPrecision = 18;
    public const int QuantityScale = 4;
    public const int CostPrecision = 18;
    public const int CostScale = 4;
}

[Audited]
public class Item : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string NameAr { get; private set; } = null!;
    public string NameEn { get; private set; } = null!;
    public ItemKind Kind { get; private set; }
    public decimal MinQuantity { get; private set; }

    /// <summary>Changes only together with a movement row, through <see cref="StockLedger"/>. Its concurrency stamp stops two movements racing past the balance.</summary>
    public decimal QuantityOnHand { get; private set; }

    public bool IsActive { get; private set; }

    protected Item()
    {
    }

    public Item(Guid id, Guid? tenantId, string nameAr, string nameEn, ItemKind kind, decimal minQuantity)
        : base(id)
    {
        TenantId = tenantId;
        Kind = kind;
        IsActive = true;
        Set(nameAr, nameEn, kind, minQuantity, true);
    }

    public bool IsBelowMinimum => QuantityOnHand < MinQuantity;

    public void Set(string nameAr, string nameEn, ItemKind kind, decimal minQuantity, bool isActive)
    {
        NameAr = nameAr;
        NameEn = nameEn;
        Kind = kind;
        MinQuantity = minQuantity;
        IsActive = isActive;
    }

    internal void ApplyMovement(decimal signedQuantity) => QuantityOnHand += signedQuantity;
}

/// <summary>Never updated or deleted. A correction is another movement. <see cref="TotalCost"/> never leaves in a store DTO: only the accountant sees it, through finance.</summary>
[Audited]
public class StockMovement : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid ItemId { get; private set; }
    public StockMovementType Type { get; private set; }

    /// <summary>Positive for receive and return, negative for issue, either for adjustment.</summary>
    public decimal Quantity { get; private set; }

    public decimal BalanceAfter { get; private set; }
    public Guid? RequestId { get; private set; }
    public Guid? StockIssueId { get; private set; }
    public Guid? CustodyId { get; private set; }
    public Guid? ReversesMovementId { get; private set; }
    public Guid? EmployeeId { get; private set; }
    public Guid PerformedByUserId { get; private set; }
    public string? Note { get; private set; }
    public decimal? TotalCost { get; private set; }

    protected StockMovement()
    {
    }

    public StockMovement(
        Guid id,
        Guid? tenantId,
        Guid itemId,
        StockMovementType type,
        decimal quantity,
        decimal balanceAfter,
        Guid performedByUserId,
        string? note,
        Guid? requestId,
        Guid? stockIssueId,
        Guid? custodyId,
        Guid? reversesMovementId,
        Guid? employeeId,
        decimal? totalCost)
        : base(id)
    {
        TenantId = tenantId;
        ItemId = itemId;
        Type = type;
        Quantity = quantity;
        BalanceAfter = balanceAfter;
        PerformedByUserId = performedByUserId;
        Note = note;
        RequestId = requestId;
        StockIssueId = stockIssueId;
        CustodyId = custodyId;
        ReversesMovementId = reversesMovementId;
        EmployeeId = employeeId;
        TotalCost = totalCost;
    }
}

[Audited]
public class StockIssue : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid RequestId { get; private set; }
    public Guid ItemId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public decimal Quantity { get; private set; }
    public StockIssueStatus Status { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }
    public Guid? AcknowledgedByUserId { get; private set; }
    public DateTime? PurchasingAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public Guid? DeliveredByUserId { get; private set; }
    public Guid? CustodyId { get; private set; }
    public Guid? StockMovementId { get; private set; }

    protected StockIssue()
    {
    }

    public StockIssue(Guid id, Guid? tenantId, Guid requestId, Guid itemId, Guid employeeId, decimal quantity)
        : base(id)
    {
        TenantId = tenantId;
        RequestId = requestId;
        ItemId = itemId;
        EmployeeId = employeeId;
        Quantity = quantity;
        Status = StockIssueStatus.PendingAcknowledgement;
    }

    public void Acknowledge(Guid userId, DateTime now)
    {
        Status = StockIssueStatus.Acknowledged;
        AcknowledgedByUserId = userId;
        AcknowledgedAt = now;
    }

    public void MarkPurchasing(DateTime now)
    {
        Status = StockIssueStatus.Purchasing;
        PurchasingAt = now;
    }

    public void Deliver(Guid userId, DateTime now, Guid movementId, Guid? custodyId)
    {
        Status = StockIssueStatus.Delivered;
        DeliveredByUserId = userId;
        DeliveredAt = now;
        StockMovementId = movementId;
        CustodyId = custodyId;
    }

    public void Cancel() => Status = StockIssueStatus.Cancelled;
}

[Audited]
public class Custody : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid ItemId { get; private set; }
    public CustodyKind Kind { get; private set; }

    /// <summary>Personal custody only.</summary>
    public Guid? EmployeeId { get; private set; }

    /// <summary>Shared custody only. A custody is held by one or the other, never both.</summary>
    public Guid? DepartmentId { get; private set; }

    public decimal Quantity { get; private set; }
    public CustodyStatus Status { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public Guid AssignedByUserId { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }
    public Guid? AcknowledgedByUserId { get; private set; }
    public DateTime? ReturnedAt { get; private set; }
    public Guid? ReturnedByUserId { get; private set; }
    public bool? Restocked { get; private set; }
    public Guid? RequestId { get; private set; }
    public Guid? StockIssueId { get; private set; }
    public string? Note { get; private set; }
    public string? ReturnNote { get; private set; }

    protected Custody()
    {
    }

    public Custody(
        Guid id,
        Guid? tenantId,
        Guid itemId,
        CustodyKind kind,
        Guid? employeeId,
        Guid? departmentId,
        decimal quantity,
        DateTime assignedAt,
        Guid assignedByUserId,
        Guid? requestId,
        Guid? stockIssueId,
        string? note)
        : base(id)
    {
        TenantId = tenantId;
        ItemId = itemId;
        Kind = kind;
        EmployeeId = employeeId;
        DepartmentId = departmentId;
        Quantity = quantity;
        Status = CustodyStatus.PendingAcknowledgement;
        AssignedAt = assignedAt;
        AssignedByUserId = assignedByUserId;
        RequestId = requestId;
        StockIssueId = stockIssueId;
        Note = note;
    }

    public void Acknowledge(Guid userId, DateTime now)
    {
        Status = CustodyStatus.Acknowledged;
        AcknowledgedByUserId = userId;
        AcknowledgedAt = now;
    }

    public void Return(Guid userId, DateTime now, bool restock, string? note)
    {
        Status = CustodyStatus.Returned;
        ReturnedByUserId = userId;
        ReturnedAt = now;
        Restocked = restock;
        ReturnNote = note;
    }
}

/// <summary>A record of what a manager found. It changes neither custody nor stock, and makes nobody personally responsible.</summary>
[Audited]
public class CustodyCount : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid DepartmentId { get; private set; }
    public DateOnly CountDate { get; private set; }
    public CustodyCountResult Result { get; private set; }
    public string? Note { get; private set; }
    public Guid CountedByUserId { get; private set; }
    public ICollection<CustodyCountLine> Lines { get; private set; } = new List<CustodyCountLine>();

    protected CustodyCount()
    {
    }

    public CustodyCount(Guid id, Guid? tenantId, Guid departmentId, DateOnly countDate, CustodyCountResult result, string? note, Guid countedByUserId)
        : base(id)
    {
        TenantId = tenantId;
        DepartmentId = departmentId;
        CountDate = countDate;
        Result = result;
        Note = note;
        CountedByUserId = countedByUserId;
    }
}

public class CustodyCountLine : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid CustodyCountId { get; private set; }
    public Guid CustodyId { get; private set; }
    public Guid ItemId { get; private set; }
    public decimal ExpectedQuantity { get; private set; }
    public decimal CountedQuantity { get; private set; }
    public string? Note { get; private set; }

    protected CustodyCountLine()
    {
    }

    public CustodyCountLine(Guid id, Guid? tenantId, Guid custodyCountId, Guid custodyId, Guid itemId, decimal expectedQuantity, decimal countedQuantity, string? note)
        : base(id)
    {
        TenantId = tenantId;
        CustodyCountId = custodyCountId;
        CustodyId = custodyId;
        ItemId = itemId;
        ExpectedQuantity = expectedQuantity;
        CountedQuantity = countedQuantity;
        Note = note;
    }
}
