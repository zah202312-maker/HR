using System;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Delegations;

/// <summary>Cancelling keeps the row, so the record of who could decide and when stays complete.</summary>
[Audited]
public class Delegation : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxReasonLength = 2000;

    public Guid? TenantId { get; private set; }
    public Guid FromUserId { get; private set; }
    public Guid ToUserId { get; private set; }
    public DelegationScope Scope { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }
    public string Reason { get; private set; } = null!;
    public DateTime? CancelledAt { get; private set; }

    protected Delegation()
    {
    }

    public Delegation(Guid id, Guid? tenantId, Guid fromUserId, Guid toUserId, DelegationScope scope, DateTime startsAt, DateTime endsAt, string reason)
        : base(id)
    {
        TenantId = tenantId;
        FromUserId = fromUserId;
        ToUserId = toUserId;
        Scope = scope;
        StartsAt = startsAt;
        EndsAt = endsAt;
        Reason = reason;
    }

    public DelegationStatus GetStatus(DateTime now) =>
        CancelledAt != null ? DelegationStatus.Cancelled
        : now < StartsAt ? DelegationStatus.Scheduled
        : now < EndsAt ? DelegationStatus.Active
        : DelegationStatus.Expired;

    public void Cancel(DateTime now) => CancelledAt = now;
}
