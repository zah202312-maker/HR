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
    public const int MaxSkuLength = 32;
    public const int MaxDocumentNumberLength = 64;
    public const int MaxSupplierNameLength = 200;
    public const int MaxDocumentLines = 200;
    public const int MaxCurrencyLength = 3;
}

[Audited]
public class Item : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string NameAr { get; private set; } = null!;
    public string NameEn { get; private set; } = null!;

    /// <summary>Unique in the company. Items older than 2026-11-11 got ITM-0001 and on from the migration.</summary>
    public string Sku { get; private set; } = null!;

    /// <summary>Null only for items older than 2026-11-11 until someone edits them.</summary>
    public Guid? CategoryId { get; private set; }

    public Guid? UnitId { get; private set; }
    public ItemKind Kind { get; private set; }
    public decimal MinQuantity { get; private set; }

    /// <summary>Changes only together with a movement row, through <see cref="StockLedger"/>. Its concurrency stamp stops two movements racing past the balance.</summary>
    public decimal QuantityOnHand { get; private set; }

    /// <summary>Weighted average cost of the stock on hand. Changes only through <see cref="StockLedger"/>.</summary>
    public decimal AverageUnitCost { get; private set; }

    public bool IsActive { get; private set; }

    protected Item()
    {
    }

    public Item(Guid id, Guid? tenantId, string nameAr, string nameEn, string sku, Guid categoryId, Guid unitId, ItemKind kind, decimal minQuantity)
        : base(id)
    {
        TenantId = tenantId;
        Kind = kind;
        IsActive = true;
        Set(nameAr, nameEn, sku, categoryId, unitId, kind, minQuantity, true);
    }

    public bool IsBelowMinimum => QuantityOnHand < MinQuantity;

    public decimal StockValue => Math.Round(QuantityOnHand * AverageUnitCost, StoreConsts.CostScale);

    public StockStatus StockStatus => StatusOf(QuantityOnHand, MinQuantity);

    public static StockStatus StatusOf(decimal quantityOnHand, decimal minQuantity) =>
        quantityOnHand <= 0 ? StockStatus.OutOfStock : quantityOnHand < minQuantity ? StockStatus.LowStock : StockStatus.InStock;

    public void Set(string nameAr, string nameEn, string sku, Guid categoryId, Guid unitId, ItemKind kind, decimal minQuantity, bool isActive)
    {
        NameAr = nameAr;
        NameEn = nameEn;
        Sku = sku;
        CategoryId = categoryId;
        UnitId = unitId;
        Kind = kind;
        MinQuantity = minQuantity;
        IsActive = isActive;
    }

    /// <summary>
    /// The weighted average absorbs a movement at its unit cost: (Q × avg + q × u) / (Q + q). An outflow at the average
    /// leaves it unchanged, and reversing a purchase at its price takes that purchase back out. An empty stock keeps the
    /// last average, so the screens still show a price.
    /// </summary>
    internal void ApplyMovement(decimal signedQuantity, decimal unitCost)
    {
        var after = QuantityOnHand + signedQuantity;
        if (after != 0)
        {
            var average = (QuantityOnHand * AverageUnitCost + signedQuantity * unitCost) / after;
            AverageUnitCost = Math.Max(0, Math.Round(average, StoreConsts.CostScale));
        }
        else if (signedQuantity > 0)
        {
            AverageUnitCost = unitCost;
        }

        QuantityOnHand = after;
    }
}

/// <summary>A category or a unit of measure the store officer keeps. Never deleted, deactivated instead.</summary>
[Audited]
public abstract class StoreCatalogEntry : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string NameAr { get; private set; } = null!;
    public string NameEn { get; private set; } = null!;
    public bool IsActive { get; private set; }

    protected StoreCatalogEntry()
    {
    }

    protected StoreCatalogEntry(Guid id, Guid? tenantId, string nameAr, string nameEn)
        : base(id)
    {
        TenantId = tenantId;
        IsActive = true;
        Set(nameAr, nameEn, true);
    }

    public void Set(string nameAr, string nameEn, bool isActive)
    {
        NameAr = nameAr;
        NameEn = nameEn;
        IsActive = isActive;
    }
}

public class ItemCategory : StoreCatalogEntry
{
    protected ItemCategory()
    {
    }

    public ItemCategory(Guid id, Guid? tenantId, string nameAr, string nameEn)
        : base(id, tenantId, nameAr, nameEn)
    {
    }
}

public class ItemUnit : StoreCatalogEntry
{
    protected ItemUnit()
    {
    }

    public ItemUnit(Guid id, Guid? tenantId, string nameAr, string nameEn)
        : base(id, tenantId, nameAr, nameEn)
    {
    }
}

/// <summary>A team or body that receives stock on an issue voucher. Its department, if any, holds shared custody.</summary>
[Audited]
public class StoreTeam : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string Name { get; private set; } = null!;
    public Guid? DepartmentId { get; private set; }
    public bool IsActive { get; private set; }

    protected StoreTeam()
    {
    }

    public StoreTeam(Guid id, Guid? tenantId, string name, Guid? departmentId)
        : base(id)
    {
        TenantId = tenantId;
        Set(name, departmentId, true);
    }

    public void Set(string name, Guid? departmentId, bool isActive)
    {
        Name = name;
        DepartmentId = departmentId;
        IsActive = isActive;
    }
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

    /// <summary>The value of the movement with its sign: quantity × <see cref="UnitCost"/>. Null on movements older than 2026-11-11 that had no cost.</summary>
    public decimal? TotalCost { get; private set; }

    /// <summary>The purchase price for stock in, the average cost at the time for stock out. Null on movements older than 2026-11-11.</summary>
    public decimal? UnitCost { get; private set; }

    public Guid? PurchaseId { get; private set; }
    public Guid? IssueVoucherId { get; private set; }
    public Guid? TeamId { get; private set; }

    /// <summary>The receipt or voucher number, copied so the log can be searched without a join.</summary>
    public string? DocumentNumber { get; private set; }

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
        decimal? totalCost,
        decimal? unitCost = null,
        Guid? purchaseId = null,
        Guid? issueVoucherId = null,
        Guid? teamId = null,
        string? documentNumber = null)
        : base(id)
    {
        UnitCost = unitCost;
        PurchaseId = purchaseId;
        IssueVoucherId = issueVoucherId;
        TeamId = teamId;
        DocumentNumber = documentNumber;
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
