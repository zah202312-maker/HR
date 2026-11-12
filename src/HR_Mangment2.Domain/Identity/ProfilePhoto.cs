using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Identity;

/// <summary>
/// The account's own photo, shown next to the name everywhere (decision 2026-11-11). Only the owner changes it. A new
/// upload is a new row with a new id, so a client may cache a photo by id forever; the old row and file are deleted.
/// </summary>
public class ProfilePhoto : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public const long MaxSizeBytes = 1_048_576;
    public const int MaxContentTypeLength = 32;
    public const int MaxBlobNameLength = 128;

    public Guid? TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string BlobName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long SizeBytes { get; private set; }

    protected ProfilePhoto()
    {
    }

    public ProfilePhoto(Guid id, Guid? tenantId, Guid userId, string contentType, long sizeBytes)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        BlobName = "profile-photos/" + id.ToString("N");
        ContentType = contentType;
        SizeBytes = sizeBytes;
    }
}

/// <summary>The photo ids of many accounts at once, for the name references.</summary>
public class ProfilePhotoReader : ITransientDependency
{
    private readonly IRepository<ProfilePhoto, Guid> _photos;

    public ProfilePhotoReader(IRepository<ProfilePhoto, Guid> photos)
    {
        _photos = photos;
    }

    public virtual async Task<Dictionary<Guid, Guid>> GetPhotoIdsAsync(IEnumerable<Guid> userIds)
    {
        var distinct = userIds.Distinct().ToList();
        if (distinct.Count == 0)
        {
            return new Dictionary<Guid, Guid>();
        }

        var rows = await _photos.GetListAsync(x => distinct.Contains(x.UserId));
        return rows.GroupBy(x => x.UserId).ToDictionary(x => x.Key, x => x.OrderByDescending(p => p.CreationTime).First().Id);
    }
}
