using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HR_Mangment2.Errors;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Users;

namespace HR_Mangment2.Idempotency;

/// <summary>
/// Idempotency-Key support, scoped per user and operation for 24 hours. A repeat returns the id of the resource,
/// and the caller reads it again through its normal permission check.
/// </summary>
public class IdempotencyStore : ITransientDependency
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromHours(24);

    private readonly IRepository<IdempotencyRecord, Guid> _repository;
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;

    public IdempotencyStore(
        IRepository<IdempotencyRecord, Guid> repository,
        ICurrentUser currentUser,
        ICurrentTenant currentTenant,
        IGuidGenerator guidGenerator,
        IClock clock)
    {
        _repository = repository;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
        _guidGenerator = guidGenerator;
        _clock = clock;
    }

    /// <summary>Returns the earlier resource id, or null if this key is new. The same key with other content is rejected.</summary>
    public virtual async Task<Guid?> FindResourceAsync(string operation, Guid? key, object request)
    {
        if (key == null)
        {
            return null;
        }

        var userId = _currentUser.GetId();
        var now = _clock.Now;
        var record = await _repository.FirstOrDefaultAsync(x =>
            x.UserId == userId && x.Operation == operation && x.Key == key.Value && x.ExpiresAt > now);

        if (record == null)
        {
            return null;
        }

        if (record.RequestHash != Fingerprint(request))
        {
            throw new BusinessException(HrErrorCodes.Common.IdempotencyKeyReused);
        }

        return record.ResourceId;
    }

    public virtual async Task SaveAsync(string operation, Guid? key, object request, Guid resourceId)
    {
        if (key == null)
        {
            return;
        }

        var userId = _currentUser.GetId();
        var now = _clock.Now;

        // An expired row with this key would otherwise block the new one on the unique index.
        await _repository.DeleteDirectAsync(x => x.ExpiresAt <= now);

        await _repository.InsertAsync(
            new IdempotencyRecord(_guidGenerator.Create(), _currentTenant.Id, userId, operation, key.Value, Fingerprint(request), resourceId, now.Add(Lifetime)),
            autoSave: true);
    }

    private static string Fingerprint(object request) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request, request.GetType()))));
}
