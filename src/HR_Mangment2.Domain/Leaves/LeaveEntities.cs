using System;
using System.Collections.Generic;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Leaves;

/// <summary>Sick, maternity and similar leave. A type is never deleted, only switched off, and edits apply to new requests only.</summary>
[Audited]
public class OtherLeaveType : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxNameLength = 128;

    public Guid? TenantId { get; private set; }
    public string NameAr { get; private set; } = null!;
    public string NameEn { get; private set; } = null!;
    public bool IsPaid { get; private set; }
    public decimal? MaxDays { get; private set; }
    public bool RequiresAttachment { get; private set; }
    public OtherLeaveEligibleCategory EligibleCategory { get; private set; }
    public bool DeductsFromAnnualBalance { get; private set; }
    public bool IsActive { get; private set; }

    protected OtherLeaveType()
    {
    }

    public OtherLeaveType(Guid id, Guid? tenantId)
        : base(id)
    {
        TenantId = tenantId;
    }

    public void Set(
        string nameAr,
        string nameEn,
        bool isPaid,
        decimal? maxDays,
        bool requiresAttachment,
        OtherLeaveEligibleCategory eligibleCategory,
        bool deductsFromAnnualBalance,
        bool isActive)
    {
        NameAr = nameAr;
        NameEn = nameEn;
        IsPaid = isPaid;
        MaxDays = maxDays;
        RequiresAttachment = requiresAttachment;
        EligibleCategory = eligibleCategory;
        DeductsFromAnnualBalance = deductsFromAnnualBalance;
        IsActive = isActive;
    }
}

/// <summary>
/// One per employee. Everything that changes a balance touches this row, so its concurrency stamp makes two
/// simultaneous changes collide with a 409 instead of spending the same days twice.
/// </summary>
[Audited]
public class EmployeeLeaveAccount : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }

    /// <summary>
    /// No longer read or written: the next year takes the company default (decision 2026-11-10, manual entitlement removed).
    /// The column stays so old rows keep their history.
    /// </summary>
    public decimal? NextYearEntitlementDays { get; private set; }

    public int Revision { get; private set; }

    protected EmployeeLeaveAccount()
    {
    }

    public EmployeeLeaveAccount(Guid id, Guid? tenantId, Guid employeeId)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
    }

    public void SetNextYearEntitlement(decimal? days) => NextYearEntitlementDays = days;

    public void Touch() => Revision++;
}

/// <summary>One personal leave year, running from the hire anniversary. A future year exists before it is granted when leave reaches into it.</summary>
public class EmployeeLeaveYear : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public DateOnly YearStart { get; private set; }
    public DateOnly YearEnd { get; private set; }
    public decimal EntitlementDays { get; private set; }

    /// <summary>False only for a future year that holds leave but has not started.</summary>
    public bool IsGranted { get; private set; }

    public bool IsClosed { get; private set; }

    protected EmployeeLeaveYear()
    {
    }

    public EmployeeLeaveYear(Guid id, Guid? tenantId, Guid employeeId, DateOnly yearStart, DateOnly yearEnd)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        YearStart = yearStart;
        YearEnd = yearEnd;
    }

    public void Grant(decimal entitlementDays)
    {
        EntitlementDays = entitlementDays;
        IsGranted = true;
    }

    public void Close() => IsClosed = true;
}

/// <summary>Every balance is the sum of these. A row never changes and is never deleted: a correction is another row.</summary>
public class LeaveLedgerEntry : CreationAuditedEntity<Guid>, IMultiTenant
{
    public const int MaxReasonLength = 2000;
    public const int MonthLength = 7;

    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid LeaveYearId { get; private set; }
    public LeaveLedgerType Type { get; private set; }
    public LeaveLedgerBucket Bucket { get; private set; }

    /// <summary>Signed: positive adds to what is available. Null on hour rows.</summary>
    public decimal? Days { get; private set; }

    /// <summary>Signed, on hour rows only.</summary>
    public decimal? Hours { get; private set; }

    /// <summary>The calendar month (yyyy-MM) whose hourly quota an hour row counts against.</summary>
    public string? HourlyMonth { get; private set; }

    public Guid? RequestId { get; private set; }
    public string? Reason { get; private set; }

    protected LeaveLedgerEntry()
    {
    }

    public LeaveLedgerEntry(
        Guid id,
        Guid? tenantId,
        Guid employeeId,
        Guid leaveYearId,
        LeaveLedgerType type,
        LeaveLedgerBucket bucket,
        decimal? days,
        decimal? hours,
        string? hourlyMonth,
        Guid? requestId,
        string? reason)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        LeaveYearId = leaveYearId;
        Type = type;
        Bucket = bucket;
        Days = days;
        Hours = hours;
        HourlyMonth = hourlyMonth;
        RequestId = requestId;
        Reason = reason;
    }
}

/// <summary>Where a leave stands, for overlap checks and attendance. The full status lives on the general request.</summary>
public enum LeaveRequestState
{
    Pending,
    Approved,
    Closed
}

/// <summary>The leave details of a general request, sharing its id. The figures are fixed at submission.</summary>
[Audited]
public class LeaveRequest : Entity<Guid>, IMultiTenant
{
    public const int MaxNotesLength = 2000;

    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public LeaveKind Kind { get; private set; }
    public DateOnly? Date { get; private set; }
    public TimeOnly? FromTime { get; private set; }
    public TimeOnly? ToTime { get; private set; }
    public DateOnly? StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }

    /// <summary>The covered dates for every kind, so one query finds overlaps. Equal to the date for hourly leave.</summary>
    public DateOnly RangeStart { get; private set; }

    public DateOnly RangeEnd { get; private set; }
    public Guid? OtherLeaveTypeId { get; private set; }
    public string? Notes { get; private set; }
    public LeaveRequestState State { get; private set; }
    public LeaveBalanceEffect BalanceEffect { get; private set; }
    public decimal WorkingDays { get; private set; }
    public decimal? RequestedHours { get; private set; }
    public decimal? HourlyQuotaHoursUsed { get; private set; }
    public decimal? HourlyExcessHours { get; private set; }
    public decimal AnnualDays { get; private set; }
    public decimal FromCarriedDays { get; private set; }
    public decimal FromEntitlementDays { get; private set; }
    public DateTime SubmittedAt { get; private set; }

    public ICollection<LeaveRequestSegment> Segments { get; private set; } = new List<LeaveRequestSegment>();

    protected LeaveRequest()
    {
    }

    public LeaveRequest(Guid requestId, Guid? tenantId, Guid employeeId, LeavePlan plan, string? notes, DateTime submittedAt)
        : base(requestId)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        Kind = plan.Kind;
        Date = plan.Date;
        FromTime = plan.FromTime;
        ToTime = plan.ToTime;
        StartDate = plan.StartDate;
        EndDate = plan.EndDate;
        RangeStart = plan.RangeStart;
        RangeEnd = plan.RangeEnd;
        OtherLeaveTypeId = plan.OtherLeaveType?.Id;
        Notes = notes;
        State = LeaveRequestState.Pending;
        BalanceEffect = plan.AnnualDays > 0 || plan.HourlyQuotaHoursUsed > 0 ? LeaveBalanceEffect.Held : LeaveBalanceEffect.None;
        WorkingDays = plan.WorkingDays;
        RequestedHours = plan.RequestedHours;
        HourlyQuotaHoursUsed = plan.HourlyQuotaHoursUsed;
        HourlyExcessHours = plan.HourlyExcessHours;
        AnnualDays = plan.AnnualDays;
        FromCarriedDays = plan.FromCarriedDays;
        FromEntitlementDays = plan.FromEntitlementDays;
        SubmittedAt = submittedAt;
    }

    public void MarkApproved()
    {
        State = LeaveRequestState.Approved;
        if (BalanceEffect == LeaveBalanceEffect.Held)
        {
            BalanceEffect = LeaveBalanceEffect.Deducted;
        }
    }

    public void MarkClosed()
    {
        State = LeaveRequestState.Closed;
        if (BalanceEffect == LeaveBalanceEffect.Held)
        {
            BalanceEffect = LeaveBalanceEffect.Released;
        }
    }
}

/// <summary>The part of a leave inside one personal year, with where its days came from.</summary>
public class LeaveRequestSegment : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid LeaveRequestId { get; private set; }

    /// <summary>The year the dates fall in. A past year's dates are charged to the current balance, so this can differ from the ledger's year.</summary>
    public DateOnly YearStart { get; private set; }

    public DateOnly YearEnd { get; private set; }
    public DateOnly? StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public decimal WorkingDays { get; private set; }
    public decimal FromCarriedDays { get; private set; }
    public decimal FromEntitlementDays { get; private set; }

    protected LeaveRequestSegment()
    {
    }

    public LeaveRequestSegment(
        Guid id,
        Guid? tenantId,
        Guid leaveRequestId,
        DateOnly yearStart,
        DateOnly yearEnd,
        DateOnly? startDate,
        DateOnly? endDate,
        decimal workingDays,
        decimal fromCarriedDays,
        decimal fromEntitlementDays)
        : base(id)
    {
        TenantId = tenantId;
        LeaveRequestId = leaveRequestId;
        YearStart = yearStart;
        YearEnd = yearEnd;
        StartDate = startDate;
        EndDate = endDate;
        WorkingDays = workingDays;
        FromCarriedDays = fromCarriedDays;
        FromEntitlementDays = fromEntitlementDays;
    }
}

/// <summary>Stored only after a clean scan. Only its uploader can use it, and an unused one is removed after 24 hours.</summary>
public class LeaveAttachment : CreationAuditedEntity<Guid>, IMultiTenant
{
    public const int MaxFileNameLength = 255;
    public const int MaxContentTypeLength = 128;
    public const int MaxBlobNameLength = 64;
    public const long MaxSizeBytes = 10 * 1024 * 1024;
    public static readonly TimeSpan UnusedLifetime = TimeSpan.FromHours(24);

    public Guid? TenantId { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public Guid? LeaveRequestId { get; private set; }
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long SizeBytes { get; private set; }
    public string BlobName { get; private set; } = null!;

    protected LeaveAttachment()
    {
    }

    public LeaveAttachment(Guid id, Guid? tenantId, Guid uploadedByUserId, string fileName, string contentType, long sizeBytes, string blobName)
        : base(id)
    {
        TenantId = tenantId;
        UploadedByUserId = uploadedByUserId;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        BlobName = blobName;
    }

    public void AttachTo(Guid leaveRequestId) => LeaveRequestId = leaveRequestId;
}
