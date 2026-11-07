using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Idempotency;

/// <summary>Holds only the key, the request fingerprint and the resource id, never request content.</summary>
public class IdempotencyRecord : Entity<Guid>, IMultiTenant
{
    public const int MaxOperationLength = 64;
    public const int RequestHashLength = 64;

    public Guid? TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string Operation { get; private set; } = null!;
    public Guid Key { get; private set; }
    public string RequestHash { get; private set; } = null!;
    public Guid ResourceId { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    protected IdempotencyRecord()
    {
    }

    public IdempotencyRecord(Guid id, Guid? tenantId, Guid userId, string operation, Guid key, string requestHash, Guid resourceId, DateTime expiresAt)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        Operation = operation;
        Key = key;
        RequestHash = requestHash;
        ResourceId = resourceId;
        ExpiresAt = expiresAt;
    }
}
