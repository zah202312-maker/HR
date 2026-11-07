using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Notifications;

public class DeviceToken : AggregateRoot<Guid>, IMultiTenant
{
    public const int MaxTokenLength = 4096;

    public Guid? TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DevicePlatform Platform { get; private set; }

    protected DeviceToken()
    {
    }

    public DeviceToken(Guid id, Guid? tenantId, Guid userId, string token, DevicePlatform platform)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        Token = token;
        Platform = platform;
    }

    public void AssignTo(Guid userId, DevicePlatform platform)
    {
        UserId = userId;
        Platform = platform;
    }
}
