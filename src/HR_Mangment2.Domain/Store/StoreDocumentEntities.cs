using System;
using System.Collections.Generic;
using System.Linq;
using HR_Mangment2.Errors;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Store;

/// <summary>
/// What a purchase receipt and an issue voucher share (decision 2026-11-11): a draft that can change or be deleted
/// without touching the balance, then an approval that moves stock and freezes it, then an optional reversal.
/// A deleted draft is soft deleted, so its number is free again.
/// </summary>
public abstract class StoreDocument : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public string? Note { get; protected set; }
    public StoreDocumentStatus Status { get; protected set; }
    public DateTime? ApprovedAt { get; protected set; }
    public Guid? ApprovedByUserId { get; protected set; }
    public DateTime? ReversedAt { get; protected set; }
    public Guid? ReversedByUserId { get; protected set; }
    public string? ReverseReason { get; protected set; }

    protected StoreDocument()
    {
    }

    protected StoreDocument(Guid id, Guid? tenantId)
        : base(id)
    {
        TenantId = tenantId;
        Status = StoreDocumentStatus.Draft;
    }

    protected abstract string NotDraftCode { get; }

    protected abstract string NotApprovedCode { get; }

    public void EnsureDraft()
    {
        if (Status != StoreDocumentStatus.Draft)
        {
            throw new BusinessException(NotDraftCode);
        }
    }

    public void EnsureApproved()
    {
        if (Status != StoreDocumentStatus.Approved)
        {
            throw new BusinessException(NotApprovedCode);
        }
    }

    public void MarkApproved(Guid userId, DateTime now)
    {
        EnsureDraft();
        Status = StoreDocumentStatus.Approved;
        ApprovedByUserId = userId;
        ApprovedAt = now;
    }

    public void MarkReversed(Guid userId, DateTime now, string reason)
    {
        EnsureApproved();
        Status = StoreDocumentStatus.Reversed;
        ReversedByUserId = userId;
        ReversedAt = now;
        ReverseReason = reason;
    }
}

[Audited]
public class StorePurchase : StoreDocument
{
    public string ReceiptNumber { get; private set; } = null!;
    public string SupplierName { get; private set; } = null!;
    public DateOnly PurchaseDate { get; private set; }
    public DateOnly ReceivedDate { get; private set; }

    /// <summary>The company currency when the receipt was created (Finance:DefaultCurrency).</summary>
    public string Currency { get; private set; } = null!;

    public ICollection<StorePurchaseLine> Lines { get; private set; } = new List<StorePurchaseLine>();

    protected StorePurchase()
    {
    }

    public StorePurchase(Guid id, Guid? tenantId, string currency)
        : base(id, tenantId)
    {
        Currency = currency;
    }

    protected override string NotDraftCode => HrErrorCodes.StorePurchase.NotDraft;

    protected override string NotApprovedCode => HrErrorCodes.StorePurchase.NotApproved;

    public decimal TotalQuantity => Lines.Sum(x => x.Quantity);

    public decimal TotalAmount => Lines.Sum(x => x.LineTotal);

    public void Set(string receiptNumber, string supplierName, DateOnly purchaseDate, DateOnly receivedDate, string? note, IEnumerable<StorePurchaseLine> lines)
    {
        EnsureDraft();
        ReceiptNumber = receiptNumber;
        SupplierName = supplierName;
        PurchaseDate = purchaseDate;
        ReceivedDate = receivedDate;
        Note = note;
        Lines.Clear();
        foreach (var line in lines)
        {
            Lines.Add(line);
        }
    }
}

/// <summary>The item's SKU, category and unit are copied when the line is saved, so the receipt keeps reading the same later.</summary>
public class StorePurchaseLine : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid PurchaseId { get; private set; }
    public Guid ItemId { get; private set; }
    public string Sku { get; private set; } = null!;
    public Guid? CategoryId { get; private set; }
    public Guid? UnitId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; }
    public Guid? StockMovementId { get; private set; }

    protected StorePurchaseLine()
    {
    }

    public StorePurchaseLine(Guid id, Guid? tenantId, Guid purchaseId, Item item, decimal quantity, decimal unitPrice)
        : base(id)
    {
        TenantId = tenantId;
        PurchaseId = purchaseId;
        ItemId = item.Id;
        Sku = item.Sku;
        CategoryId = item.CategoryId;
        UnitId = item.UnitId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = Math.Round(quantity * unitPrice, StoreConsts.CostScale);
    }

    public void Posted(Guid movementId) => StockMovementId = movementId;
}

[Audited]
public class StoreIssueVoucher : StoreDocument
{
    public string VoucherNumber { get; private set; } = null!;
    public DateOnly IssueDate { get; private set; }
    public Guid TeamId { get; private set; }
    public Guid? EmployeeId { get; private set; }
    public string Currency { get; private set; } = null!;
    public ICollection<StoreIssueVoucherLine> Lines { get; private set; } = new List<StoreIssueVoucherLine>();

    protected StoreIssueVoucher()
    {
    }

    public StoreIssueVoucher(Guid id, Guid? tenantId, string currency)
        : base(id, tenantId)
    {
        Currency = currency;
    }

    protected override string NotDraftCode => HrErrorCodes.StoreIssueVoucher.NotDraft;

    protected override string NotApprovedCode => HrErrorCodes.StoreIssueVoucher.NotApproved;

    public decimal TotalQuantity => Lines.Sum(x => x.Quantity);

    /// <summary>Known only once approved, at the average cost of that moment.</summary>
    public decimal? TotalValue => Status == StoreDocumentStatus.Draft ? null : Lines.Sum(x => x.LineValue ?? 0);

    public void Set(string voucherNumber, DateOnly issueDate, Guid teamId, Guid? employeeId, string? note, IEnumerable<StoreIssueVoucherLine> lines)
    {
        EnsureDraft();
        VoucherNumber = voucherNumber;
        IssueDate = issueDate;
        TeamId = teamId;
        EmployeeId = employeeId;
        Note = note;
        Lines.Clear();
        foreach (var line in lines)
        {
            Lines.Add(line);
        }
    }
}

public class StoreIssueVoucherLine : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid IssueVoucherId { get; private set; }
    public Guid ItemId { get; private set; }
    public string Sku { get; private set; } = null!;
    public Guid? UnitId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal? UnitCost { get; private set; }
    public decimal? LineValue { get; private set; }
    public Guid? CustodyId { get; private set; }
    public Guid? StockMovementId { get; private set; }

    protected StoreIssueVoucherLine()
    {
    }

    public StoreIssueVoucherLine(Guid id, Guid? tenantId, Guid issueVoucherId, Item item, decimal quantity)
        : base(id)
    {
        TenantId = tenantId;
        IssueVoucherId = issueVoucherId;
        ItemId = item.Id;
        Sku = item.Sku;
        UnitId = item.UnitId;
        Quantity = quantity;
    }

    public void Posted(StockMovement movement, Guid? custodyId)
    {
        StockMovementId = movement.Id;
        UnitCost = movement.UnitCost;
        LineValue = Math.Round(Quantity * (movement.UnitCost ?? 0), StoreConsts.CostScale);
        CustodyId = custodyId;
    }
}

/// <summary>The scan of a receipt or voucher. One per document: a new upload soft deletes the previous one, row and blob kept.</summary>
[Audited]
public class StoreDocumentAttachment : Entity<Guid>, IMultiTenant, ISoftDelete
{
    public Guid? TenantId { get; private set; }

    /// <summary>A <see cref="StorePurchase"/> or a <see cref="StoreIssueVoucher"/>. Their ids never collide.</summary>
    public Guid DocumentId { get; private set; }

    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long SizeBytes { get; private set; }
    public string BlobName { get; private set; } = null!;
    public DateTime CreationTime { get; private set; }
    public Guid CreatorId { get; private set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedByUserId { get; private set; }

    protected StoreDocumentAttachment()
    {
    }

    public StoreDocumentAttachment(Guid id, Guid? tenantId, Guid documentId, string fileName, string contentType, long sizeBytes, string blobName, DateTime creationTime, Guid creatorId)
        : base(id)
    {
        TenantId = tenantId;
        DocumentId = documentId;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        BlobName = blobName;
        CreationTime = creationTime;
        CreatorId = creatorId;
    }

    public void MarkDeleted(Guid userId, DateTime now)
    {
        IsDeleted = true;
        DeletedAt = now;
        DeletedByUserId = userId;
    }
}
