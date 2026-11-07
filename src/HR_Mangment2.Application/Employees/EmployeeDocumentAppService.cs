using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Errors;
using HR_Mangment2.Files;
using HR_Mangment2.Identity;
using HR_Mangment2.Permissions;
using HR_Mangment2.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Settings;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace HR_Mangment2.Employees;

/// <summary>
/// Scope, from the document decision on file privacy: HR sees everything, the employee their own file, and the
/// current direct manager only normal documents. Everyone else gets a 404, so existence is not revealed.
/// </summary>
[Authorize]
public class EmployeeDocumentAppService : HR_Mangment2AppService, IEmployeeDocumentAppService
{
    private enum DocumentAccess
    {
        None,
        NormalOnly,
        All
    }

    private static readonly Dictionary<string, byte[]> FileSignatures = new(StringComparer.OrdinalIgnoreCase)
    {
        ["application/pdf"] = new byte[] { 0x25, 0x50, 0x44, 0x46 },
        ["image/jpeg"] = new byte[] { 0xFF, 0xD8, 0xFF },
        ["image/png"] = new byte[] { 0x89, 0x50, 0x4E, 0x47 }
    };

    private readonly IRepository<Employee, Guid> _employees;
    private readonly IRepository<EmployeeDocument, Guid> _documents;
    private readonly IRepository<EmployeeDocumentAccessLog, Guid> _accessLog;
    private readonly IBlobContainer<HrFilesContainer> _files;
    private readonly FileDownloadLinkManager _links;
    private readonly IVirusScanner _scanner;
    private readonly EmployeeReferenceReader _references;
    private readonly ISettingProvider _settings;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public EmployeeDocumentAppService(
        IRepository<Employee, Guid> employees,
        IRepository<EmployeeDocument, Guid> documents,
        IRepository<EmployeeDocumentAccessLog, Guid> accessLog,
        IBlobContainer<HrFilesContainer> files,
        FileDownloadLinkManager links,
        IVirusScanner scanner,
        EmployeeReferenceReader references,
        ISettingProvider settings,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _employees = employees;
        _documents = documents;
        _accessLog = accessLog;
        _files = files;
        _links = links;
        _scanner = scanner;
        _references = references;
        _settings = settings;
        _unitOfWorkManager = unitOfWorkManager;
    }

    [Authorize(HrPermissions.EmployeePermissions.DocumentRead)]
    public virtual async Task<EmployeeDocumentPagedResultDto> GetListAsync(Guid employeeId, EmployeeDocumentListInput input)
    {
        var employee = await _employees.FindAsync(employeeId);
        var access = employee == null ? DocumentAccess.None : await GetAccessAsync(employee);
        if (access == DocumentAccess.None)
        {
            throw new EntityNotFoundException(typeof(Employee), employeeId);
        }

        var query = (await _documents.GetQueryableAsync()).Where(x => x.EmployeeId == employeeId);

        if (access == DocumentAccess.NormalOnly)
        {
            query = query.Where(x => x.Sensitivity == DocumentSensitivity.Normal);
        }

        if (input.Type != null)
        {
            query = query.Where(x => x.Type == input.Type);
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(
                    input.Sorting,
                    nameof(EmployeeDocument.CreationTime) + " desc",
                    new Dictionary<string, string>
                    {
                        ["title"] = nameof(EmployeeDocument.Title),
                        ["type"] = nameof(EmployeeDocument.Type),
                        ["creationTime"] = nameof(EmployeeDocument.CreationTime)
                    })
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        var uploaders = await _references.GetUserRefsAsync(items.Where(x => x.CreatorId != null).Select(x => x.CreatorId!.Value));

        return new EmployeeDocumentPagedResultDto
        {
            TotalCount = total,
            Items = items.Select(x => ToDto(x, x.CreatorId == null ? null : uploaders.GetValueOrDefault(x.CreatorId.Value))).ToList()
        };
    }

    [Authorize(HrPermissions.EmployeePermissions.DocumentUpload)]
    public virtual async Task<EmployeeDocumentDto> UploadAsync(Guid employeeId, EmployeeDocumentUploadInput input)
    {
        var employee = await _employees.GetAsync(employeeId);
        if (employee.Status == EmployeeStatus.Ended)
        {
            throw new BusinessException(HrErrorCodes.Employee.EmployeeEnded);
        }

        await EnsureFileAllowedAsync(input);

        var id = GuidGenerator.Create();
        var blobName = id.ToString("N");
        await _files.SaveAsync(blobName, input.Content);

        var document = new EmployeeDocument(
            id,
            CurrentTenant.Id,
            employeeId,
            input.Type,
            input.Title,
            Path.GetFileName(input.FileName),
            input.ContentType,
            input.Content.LongLength,
            input.Sensitivity,
            blobName);
        await _documents.InsertAsync(document, autoSave: true);

        var content = input.Content;
        _unitOfWorkManager.Current!.OnCompleted(() => ScanAsync(id, content));

        var uploader = await _references.GetUserRefsAsync(new[] { CurrentUser.GetId() });
        return ToDto(document, uploader.GetValueOrDefault(CurrentUser.GetId()));
    }

    [Authorize(HrPermissions.EmployeePermissions.DocumentRead)]
    public virtual async Task<FileDownloadUrlDto> GetDownloadUrlAsync(Guid id)
    {
        var document = await FindVisibleAsync(id);

        if (document.ScanStatus != DocumentScanStatus.Clean)
        {
            throw new BusinessException(HrErrorCodes.Document.NotAvailable);
        }

        if (document.Sensitivity == DocumentSensitivity.Sensitive)
        {
            await _accessLog.InsertAsync(new EmployeeDocumentAccessLog(GuidGenerator.Create(), CurrentTenant.Id, id, CurrentUser.GetId(), Clock.Now));
        }

        var link = await _links.IssueAsync(id, document.BlobName, document.FileName, document.ContentType);
        return new FileDownloadUrlDto { Url = link.Url, ExpiresAt = link.ExpiresAt };
    }

    [Authorize(HrPermissions.EmployeePermissions.DocumentDelete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var document = await _documents.GetAsync(id);
        await _links.RevokeAsync(id);
        await _documents.DeleteAsync(document);
    }

    private async Task<EmployeeDocument> FindVisibleAsync(Guid id)
    {
        var document = await _documents.FindAsync(id);
        var employee = document == null ? null : await _employees.FindAsync(document.EmployeeId);
        var access = employee == null ? DocumentAccess.None : await GetAccessAsync(employee);

        var visible = access == DocumentAccess.All ||
                      (access == DocumentAccess.NormalOnly && document!.Sensitivity == DocumentSensitivity.Normal);

        return visible ? document! : throw new EntityNotFoundException(typeof(EmployeeDocument), id);
    }

    private async Task<DocumentAccess> GetAccessAsync(Employee employee)
    {
        var userId = CurrentUser.GetId();

        if (employee.UserId == userId || CurrentUser.IsInRole(HrRoles.HrManager))
        {
            return DocumentAccess.All;
        }

        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == userId);
        return me != null && employee.ManagerEmployeeId == me.Id ? DocumentAccess.NormalOnly : DocumentAccess.None;
    }

    private async Task EnsureFileAllowedAsync(EmployeeDocumentUploadInput input)
    {
        var maxSize = long.Parse((await _settings.GetOrNullAsync(HR_Mangment2Settings.DocumentMaxFileSizeBytes))!);
        if (input.Content.LongLength > maxSize)
        {
            throw new BusinessException(HrErrorCodes.Document.FileTooLarge);
        }

        var allowed = ((await _settings.GetOrNullAsync(HR_Mangment2Settings.DocumentAllowedContentTypes)) ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var signatureMatches = !FileSignatures.TryGetValue(input.ContentType, out var signature) ||
                               input.Content.AsSpan().StartsWith(signature);

        if (!allowed.Contains(input.ContentType, StringComparer.OrdinalIgnoreCase) || !signatureMatches)
        {
            throw new BusinessException(HrErrorCodes.Document.FileTypeNotAllowed);
        }
    }

    /// <summary>Runs after the upload commits. If it fails the document stays pending and cannot be downloaded.</summary>
    private async Task ScanAsync(Guid documentId, byte[] content)
    {
        try
        {
            var result = await _scanner.ScanAsync(content);

            using var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true);
            var document = await _documents.GetAsync(documentId);
            document.SetScanResult(result == VirusScanResult.Clean ? DocumentScanStatus.Clean : DocumentScanStatus.Infected);
            await _documents.UpdateAsync(document);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Virus scan failed for document {DocumentId}.", documentId);
        }
    }

    private static EmployeeDocumentDto ToDto(EmployeeDocument document, UserRefDto? uploadedBy) => new()
    {
        Id = document.Id,
        EmployeeId = document.EmployeeId,
        Type = document.Type,
        Title = document.Title,
        FileName = document.FileName,
        ContentType = document.ContentType,
        SizeBytes = document.SizeBytes,
        Sensitivity = document.Sensitivity,
        ScanStatus = document.ScanStatus,
        UploadedBy = uploadedBy,
        CreationTime = document.CreationTime
    };
}

/// <summary>Anonymous on purpose: the unguessable, short-lived token in the link is the credential.</summary>
[AllowAnonymous]
[DisableAuditing]
public class FileDownloadAppService : HR_Mangment2AppService, IFileDownloadAppService
{
    private readonly FileDownloadLinkManager _links;
    private readonly IBlobContainer<HrFilesContainer> _files;

    public FileDownloadAppService(FileDownloadLinkManager links, IBlobContainer<HrFilesContainer> files)
    {
        _links = links;
        _files = files;
    }

    public virtual async Task<DownloadableFile?> OpenAsync(string token)
    {
        var link = await _links.FindValidAsync(token);
        if (link == null)
        {
            return null;
        }

        var content = await _files.GetOrNullAsync(link.BlobName);
        return content == null ? null : new DownloadableFile(content, link.FileName, link.ContentType);
    }
}
