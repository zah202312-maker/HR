using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Notifications;

/// <summary>Holds no free text: title and body are built from the template in the reader's language.</summary>
public class Notification : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public NotificationTemplateCode TemplateCode { get; private set; }
    public NotificationResourceType ResourceType { get; private set; }
    public Guid? ResourceId { get; private set; }
    public int? Count { get; private set; }
    public DateTime? ReadAt { get; private set; }

    protected Notification()
    {
    }

    public Notification(Guid id, Guid? tenantId, Guid userId, NotificationTemplateCode templateCode, NotificationResourceType resourceType, Guid? resourceId, int? count)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        TemplateCode = templateCode;
        ResourceType = resourceType;
        ResourceId = resourceId;
        Count = count;
    }

    public void MarkRead(DateTime now) => ReadAt ??= now;
}
