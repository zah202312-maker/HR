using System;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Employees;

/// <summary>Soft deleted only: the row and the file stay under the retention policy. CreatorId is the uploader.</summary>
[Audited]
public class EmployeeDocument : FullAuditedEntity<Guid>, IMultiTenant
{
    public const int MaxTitleLength = 200;
    public const int MaxFileNameLength = 255;
    public const int MaxContentTypeLength = 128;
    public const int MaxBlobNameLength = 128;

    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public EmployeeDocumentType Type { get; private set; }
    public string Title { get; private set; } = null!;
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long SizeBytes { get; private set; }
    public DocumentSensitivity Sensitivity { get; private set; }
    public DocumentScanStatus ScanStatus { get; private set; }
    public string BlobName { get; private set; } = null!;

    protected EmployeeDocument()
    {
    }

    public EmployeeDocument(
        Guid id,
        Guid? tenantId,
        Guid employeeId,
        EmployeeDocumentType type,
        string title,
        string fileName,
        string contentType,
        long sizeBytes,
        DocumentSensitivity sensitivity,
        string blobName)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        Type = type;
        Title = title;
        FileName = fileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        Sensitivity = sensitivity;
        BlobName = blobName;
        ScanStatus = DocumentScanStatus.Pending;
    }

    public void SetScanResult(DocumentScanStatus status) => ScanStatus = status;
}

/// <summary>Every access to a sensitive document: who and when.</summary>
public class EmployeeDocumentAccessLog : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid DocumentId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime AccessedAt { get; private set; }

    protected EmployeeDocumentAccessLog()
    {
    }

    public EmployeeDocumentAccessLog(Guid id, Guid? tenantId, Guid documentId, Guid userId, DateTime accessedAt)
        : base(id)
    {
        TenantId = tenantId;
        DocumentId = documentId;
        UserId = userId;
        AccessedAt = accessedAt;
    }
}
