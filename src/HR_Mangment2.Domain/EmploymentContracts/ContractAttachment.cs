using System;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.EmploymentContracts;

/// <summary>
/// A copy of the paper contract or a related document, uploaded with Contract_Create (decision 2026-11-10).
/// A mistaken file is soft deleted: hidden from every view and download, its row and blob kept.
/// </summary>
[Audited]
public class ContractAttachment : Entity<Guid>, IMultiTenant, ISoftDelete
{
    public const int MaxPerContract = 10;

    public Guid? TenantId { get; private set; }
    public Guid ContractId { get; private set; }
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long SizeBytes { get; private set; }
    public string BlobName { get; private set; } = null!;
    public DateTime CreationTime { get; private set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedByUserId { get; private set; }

    protected ContractAttachment()
    {
    }

    public ContractAttachment(Guid id, Guid? tenantId, Guid contractId, string fileName, string contentType, long sizeBytes, string blobName, DateTime creationTime)
        : base(id)
    {
        TenantId = tenantId;
        ContractId = contractId;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        BlobName = blobName;
        CreationTime = creationTime;
    }

    public void MarkDeleted(Guid userId, DateTime now)
    {
        IsDeleted = true;
        DeletedAt = now;
        DeletedByUserId = userId;
    }
}
