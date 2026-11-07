using System;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Appreciation;

[Audited]
public class AppreciationLetter : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxTitleLength = 200;
    public const int MaxNoteLength = 2000;

    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Note { get; private set; }
    public Guid IssuedByUserId { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public Guid FileId { get; private set; }
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long SizeBytes { get; private set; }
    public string BlobName { get; private set; } = null!;
    public DateTime? AcknowledgedAt { get; private set; }

    protected AppreciationLetter()
    {
    }

    public AppreciationLetter(
        Guid id,
        Guid? tenantId,
        Guid employeeId,
        string title,
        string? note,
        Guid issuedByUserId,
        DateTime issuedAt,
        Guid fileId,
        string fileName,
        string contentType,
        long sizeBytes,
        string blobName)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        Title = title;
        Note = note;
        IssuedByUserId = issuedByUserId;
        IssuedAt = issuedAt;
        FileId = fileId;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        BlobName = blobName;
    }

    /// <summary>Safe to repeat: the first time stays.</summary>
    public void Acknowledge(DateTime now) => AcknowledgedAt ??= now;
}
