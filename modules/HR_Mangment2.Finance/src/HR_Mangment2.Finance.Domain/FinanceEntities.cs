using System;
using System.Collections.Generic;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Finance;

/// <summary>One dated version. A change adds a version and never edits an earlier one.</summary>
[Audited]
public class SalaryProfile : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public decimal BaseSalary { get; private set; }
    public string Currency { get; private set; } = null!;
    public DateOnly EffectiveFrom { get; private set; }
    public ICollection<SalaryComponent> Components { get; private set; } = new List<SalaryComponent>();

    protected SalaryProfile()
    {
    }

    public SalaryProfile(Guid id, Guid? tenantId, Guid employeeId, decimal baseSalary, string currency, DateOnly effectiveFrom)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        BaseSalary = baseSalary;
        Currency = currency;
        EffectiveFrom = effectiveFrom;
    }
}

public class SalaryComponent : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid SalaryProfileId { get; private set; }
    public string Name { get; private set; } = null!;
    public SalaryComponentType Type { get; private set; }
    public decimal Amount { get; private set; }

    protected SalaryComponent()
    {
    }

    public SalaryComponent(Guid id, Guid? tenantId, Guid salaryProfileId, string name, SalaryComponentType type, decimal amount)
        : base(id)
    {
        TenantId = tenantId;
        SalaryProfileId = salaryProfileId;
        Name = name;
        Type = type;
        Amount = amount;
    }
}

/// <summary>Never edited or deleted. Correction is a reversal.</summary>
[Audited]
public class Adjustment : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public string Period { get; private set; } = null!;
    public AdjustmentType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public string Reason { get; private set; } = null!;
    public AdjustmentSource Source { get; private set; }
    public AdjustmentStatus Status { get; private set; }
    public Guid? AttachmentId { get; private set; }
    public string? AttachmentFileName { get; private set; }
    public string? AttachmentContentType { get; private set; }
    public long? AttachmentSizeBytes { get; private set; }
    public string? AttachmentBlobName { get; private set; }
    public Guid? SourceRequestId { get; private set; }
    public Guid? SuggestionId { get; private set; }
    public Guid? ReversalOfAdjustmentId { get; private set; }

    protected Adjustment()
    {
    }

    public Adjustment(
        Guid id,
        Guid? tenantId,
        Guid employeeId,
        string period,
        AdjustmentType type,
        decimal amount,
        string currency,
        string reason,
        AdjustmentSource source,
        AdjustmentStatus status = AdjustmentStatus.Open,
        Guid? sourceRequestId = null,
        Guid? suggestionId = null,
        Guid? reversalOfAdjustmentId = null)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        Period = period;
        Type = type;
        Amount = amount;
        Currency = currency;
        Reason = reason;
        Source = source;
        Status = status;
        SourceRequestId = sourceRequestId;
        SuggestionId = suggestionId;
        ReversalOfAdjustmentId = reversalOfAdjustmentId;
    }

    public void AttachFile(Guid attachmentId, string fileName, string contentType, long sizeBytes, string blobName)
    {
        AttachmentId = attachmentId;
        AttachmentFileName = fileName;
        AttachmentContentType = contentType;
        AttachmentSizeBytes = sizeBytes;
        AttachmentBlobName = blobName;
    }

    public void MarkIncludedInRun() => Status = AdjustmentStatus.IncludedInRun;

    public void MarkReversed() => Status = AdjustmentStatus.Reversed;
}

/// <summary>Has no effect on any run until the accountant approves it.</summary>
[Audited]
public class AdjustmentSuggestion : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public string Period { get; private set; } = null!;
    public AdjustmentType Type { get; private set; }
    public AdjustmentSuggestionBasis Basis { get; private set; }
    public decimal Quantity { get; private set; }
    public AdjustmentSuggestionQuantityUnit QuantityUnit { get; private set; }
    public decimal? SuggestedAmount { get; private set; }
    public string? Currency { get; private set; }
    public string? SettlementForPeriod { get; private set; }
    public Guid? SourceCorrectionId { get; private set; }
    public AdjustmentSuggestionStatus Status { get; private set; }
    public Guid? AdjustmentId { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? ReviewNote { get; private set; }

    protected AdjustmentSuggestion()
    {
    }

    public AdjustmentSuggestion(
        Guid id,
        Guid? tenantId,
        Guid employeeId,
        string period,
        AdjustmentType type,
        AdjustmentSuggestionBasis basis,
        decimal quantity,
        AdjustmentSuggestionQuantityUnit quantityUnit,
        string? currency,
        string? settlementForPeriod = null,
        Guid? sourceCorrectionId = null)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        Period = period;
        Type = type;
        Basis = basis;
        Quantity = quantity;
        QuantityUnit = quantityUnit;
        Currency = currency;
        SettlementForPeriod = settlementForPeriod;
        SourceCorrectionId = sourceCorrectionId;
        Status = AdjustmentSuggestionStatus.PendingReview;
    }

    public void UpdateQuantity(decimal quantity) => Quantity = quantity;

    public void Approve(Guid adjustmentId, Guid userId, DateTime now, string? note)
    {
        Status = AdjustmentSuggestionStatus.Approved;
        AdjustmentId = adjustmentId;
        ReviewedByUserId = userId;
        ReviewedAt = now;
        ReviewNote = note;
    }

    public void Reject(Guid userId, DateTime now, string reason)
    {
        Status = AdjustmentSuggestionStatus.Rejected;
        ReviewedByUserId = userId;
        ReviewedAt = now;
        ReviewNote = reason;
    }
}

[Audited]
public class PayrollRun : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string Period { get; private set; } = null!;
    public PayrollRunStatus Status { get; private set; }
    public Guid PreparedByUserId { get; private set; }
    public DateTime? ApprovalRequestedAt { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public DateTime LastCalculatedAt { get; private set; }
    public bool IsStale { get; private set; }
    public string? LastReturnReason { get; private set; }
    public string EmployeesWithoutProfile { get; private set; } = string.Empty;
    public ICollection<PayrollItem> Items { get; private set; } = new List<PayrollItem>();

    protected PayrollRun()
    {
    }

    public PayrollRun(Guid id, Guid? tenantId, string period, Guid preparedByUserId, DateTime now)
        : base(id)
    {
        TenantId = tenantId;
        Period = period;
        PreparedByUserId = preparedByUserId;
        LastCalculatedAt = now;
        Status = PayrollRunStatus.Draft;
    }

    public void MarkCalculated(DateTime now, string employeesWithoutProfile)
    {
        LastCalculatedAt = now;
        IsStale = false;
        ApprovalRequestedAt = null;
        EmployeesWithoutProfile = employeesWithoutProfile;
    }

    public void MarkStale() => IsStale = true;

    public void RequestApproval(DateTime now) => ApprovalRequestedAt = now;

    public void Return(string reason)
    {
        ApprovalRequestedAt = null;
        LastReturnReason = reason;
    }

    public void Approve(Guid userId, DateTime now)
    {
        Status = PayrollRunStatus.Approved;
        ApprovedByUserId = userId;
        ApprovedAt = now;
        IsStale = false;
    }

    public void Close(DateTime now)
    {
        Status = PayrollRunStatus.Closed;
        ClosedAt = now;
    }
}

/// <summary>A fixed copy of what was calculated. Its identifier is also the payslip's.</summary>
public class PayrollItem : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid PayrollRunId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public string Currency { get; private set; } = null!;
    public decimal Gross { get; private set; }
    public decimal Additions { get; private set; }
    public decimal Deductions { get; private set; }
    public decimal Net { get; private set; }
    public ICollection<PayrollLine> Lines { get; private set; } = new List<PayrollLine>();

    /// <summary>When the accountant handed this salary over, after the run closed (decision 2026-11-11).</summary>
    public DateOnly? DeliveredOn { get; private set; }

    public Guid? DeliveredByUserId { get; private set; }

    public void MarkDelivered(DateOnly on, Guid userId)
    {
        DeliveredOn = on;
        DeliveredByUserId = userId;
    }

    public void UndoDelivery()
    {
        DeliveredOn = null;
        DeliveredByUserId = null;
    }

    protected PayrollItem()
    {
    }

    public PayrollItem(Guid id, Guid? tenantId, Guid payrollRunId, Guid employeeId, string currency, decimal gross, decimal additions, decimal deductions)
        : base(id)
    {
        TenantId = tenantId;
        PayrollRunId = payrollRunId;
        EmployeeId = employeeId;
        Currency = currency;
        Gross = gross;
        Additions = additions;
        Deductions = deductions;
        Net = gross + additions - deductions;
    }
}

public class PayrollLine : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid PayrollItemId { get; private set; }
    public int Order { get; private set; }
    public PayrollLineKind Kind { get; private set; }
    public string? Label { get; private set; }
    public decimal Amount { get; private set; }
    public Guid? AdjustmentId { get; private set; }

    protected PayrollLine()
    {
    }

    public PayrollLine(Guid id, Guid? tenantId, Guid payrollItemId, int order, PayrollLineKind kind, string? label, decimal amount, Guid? adjustmentId)
        : base(id)
    {
        TenantId = tenantId;
        PayrollItemId = payrollItemId;
        Order = order;
        Kind = kind;
        Label = label;
        Amount = amount;
        AdjustmentId = adjustmentId;
    }
}

/// <summary>Never edited or deleted. Correction is a row with a negative amount, and totals sum both rows.</summary>
[Audited]
public class Expense : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string Category { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public DateOnly Date { get; private set; }
    public string? Description { get; private set; }
    public ExpenseSource Source { get; private set; }
    public ExpenseStatus Status { get; private set; }
    public Guid? AttachmentId { get; private set; }
    public string? AttachmentFileName { get; private set; }
    public string? AttachmentContentType { get; private set; }
    public long? AttachmentSizeBytes { get; private set; }
    public string? AttachmentBlobName { get; private set; }
    public Guid? StockMovementId { get; private set; }
    public Guid? SourceRequestId { get; private set; }
    public Guid? ReversalOfExpenseId { get; private set; }

    protected Expense()
    {
    }

    public Expense(
        Guid id,
        Guid? tenantId,
        string category,
        decimal amount,
        string currency,
        DateOnly date,
        string? description,
        ExpenseSource source,
        Guid? stockMovementId = null,
        Guid? sourceRequestId = null,
        Guid? reversalOfExpenseId = null)
        : base(id)
    {
        TenantId = tenantId;
        Category = category;
        Amount = amount;
        Currency = currency;
        Date = date;
        Description = description;
        Source = source;
        Status = ExpenseStatus.Active;
        StockMovementId = stockMovementId;
        SourceRequestId = sourceRequestId;
        ReversalOfExpenseId = reversalOfExpenseId;
    }

    public void AttachFile(Guid attachmentId, string fileName, string contentType, long sizeBytes, string blobName)
    {
        AttachmentId = attachmentId;
        AttachmentFileName = fileName;
        AttachmentContentType = contentType;
        AttachmentSizeBytes = sizeBytes;
        AttachmentBlobName = blobName;
    }

    public void MarkReversed() => Status = ExpenseStatus.Reversed;
}
