using System;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Auditing;

/// <summary>
/// One append-only row of the audit log screen. It holds field names only, never values or request parameters,
/// so reading it reveals no financial or personal data. Nothing in the code updates or deletes these rows.
/// </summary>
[DisableAuditing]
public class HrAuditEntry : BasicAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public Guid? UserId { get; private set; }
    public AuditActionType Action { get; private set; }
    public string? EntityType { get; private set; }
    public string? EntityId { get; private set; }
    public string[] ChangedFields { get; private set; } = Array.Empty<string>();
    public string? IpAddress { get; private set; }
    public bool Succeeded { get; private set; }

    protected HrAuditEntry()
    {
    }

    public HrAuditEntry(
        Guid id,
        Guid? tenantId,
        DateTime occurredAt,
        Guid? userId,
        AuditActionType action,
        string? entityType,
        string? entityId,
        string[] changedFields,
        string? ipAddress,
        bool succeeded)
        : base(id)
    {
        TenantId = tenantId;
        OccurredAt = occurredAt;
        UserId = userId;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        ChangedFields = changedFields;
        IpAddress = ipAddress;
        Succeeded = succeeded;
    }
}
