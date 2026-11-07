using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;

namespace HR_Mangment2.Files;

/// <summary>Where uploaded files live. The provider is chosen in configuration, so S3 replaces the database without code changes.</summary>
[BlobContainerName("hr-files")]
public class HrFilesContainer
{
}

/// <summary>A short-lived download link. Only a hash of the token is stored, so the table cannot be used to download.</summary>
public class FileDownloadLink : Entity<Guid>, IMultiTenant
{
    public const int TokenHashLength = 64;
    public const int MaxFileNameLength = 255;
    public const int MaxContentTypeLength = 128;
    public const int MaxBlobNameLength = 128;

    public Guid? TenantId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public Guid ResourceId { get; private set; }
    public string BlobName { get; private set; } = null!;
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }

    protected FileDownloadLink()
    {
    }

    public FileDownloadLink(
        Guid id,
        Guid? tenantId,
        string tokenHash,
        Guid resourceId,
        string blobName,
        string fileName,
        string contentType,
        DateTime expiresAt)
        : base(id)
    {
        TenantId = tenantId;
        TokenHash = tokenHash;
        ResourceId = resourceId;
        BlobName = blobName;
        FileName = fileName;
        ContentType = contentType;
        ExpiresAt = expiresAt;
    }
}

public record IssuedDownloadLink(string Url, DateTime ExpiresAt);

public class FileDownloadLinkManager : ITransientDependency
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(15);

    private readonly IRepository<FileDownloadLink, Guid> _links;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly IClock _clock;
    private readonly IConfiguration _configuration;

    public FileDownloadLinkManager(
        IRepository<FileDownloadLink, Guid> links,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IClock clock,
        IConfiguration configuration)
    {
        _links = links;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _clock = clock;
        _configuration = configuration;
    }

    public virtual async Task<IssuedDownloadLink> IssueAsync(Guid resourceId, string blobName, string fileName, string contentType)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var expiresAt = _clock.Now.Add(Lifetime);

        await _links.InsertAsync(new FileDownloadLink(_guidGenerator.Create(), _currentTenant.Id, Hash(token), resourceId, blobName, fileName, contentType, expiresAt));

        var baseUrl = (_configuration["App:SelfUrl"] ?? string.Empty).TrimEnd('/');
        return new IssuedDownloadLink($"{baseUrl}/files/{token}", expiresAt);
    }

    public virtual async Task<FileDownloadLink?> FindValidAsync(string token)
    {
        var hash = Hash(token);
        var link = await _links.FirstOrDefaultAsync(x => x.TokenHash == hash);
        return link == null || link.ExpiresAt <= _clock.Now ? null : link;
    }

    public virtual Task RevokeAsync(Guid resourceId) => _links.DeleteAsync(x => x.ResourceId == resourceId);

    private static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
}

public enum VirusScanResult
{
    Clean,
    Infected
}

/// <summary>The production implementation is ClamAV (document section 2). It is not connected yet: see PROGRESS.md.</summary>
public interface IVirusScanner
{
    Task<VirusScanResult> ScanAsync(byte[] content);
}

/// <summary>
/// Development stand-in. It only recognizes the EICAR test signature, so it is not real protection. Outside
/// development and testing it refuses to answer: documents then stay pending and cannot be downloaded, instead of
/// being marked clean without a real scan.
/// </summary>
public class SignatureOnlyVirusScanner : IVirusScanner, ITransientDependency
{
    private const string EicarSignature = "EICAR-STANDARD-ANTIVIRUS-TEST-FILE";

    private readonly IHostEnvironment _environment;

    public SignatureOnlyVirusScanner(IHostEnvironment environment)
    {
        _environment = environment;
    }

    public Task<VirusScanResult> ScanAsync(byte[] content)
    {
        if (!_environment.IsDevelopment() && !_environment.IsEnvironment("Testing"))
        {
            throw new InvalidOperationException("No virus scanner is configured for this environment.");
        }

        var infected = Encoding.ASCII.GetString(content).Contains(EicarSignature, StringComparison.Ordinal);
        return Task.FromResult(infected ? VirusScanResult.Infected : VirusScanResult.Clean);
    }
}
