using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Errors;
using HR_Mangment2.Settings;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace HR_Mangment2.Files;

/// <summary>The document limits release 3 modules share. A scanner that cannot answer counts as a rejection, so an unscanned file never gets in.</summary>
public class UploadedFileGuard : ITransientDependency
{
    private static readonly Dictionary<string, byte[]> Signatures = new(StringComparer.OrdinalIgnoreCase)
    {
        ["application/pdf"] = new byte[] { 0x25, 0x50, 0x44, 0x46 },
        ["image/jpeg"] = new byte[] { 0xFF, 0xD8, 0xFF },
        ["image/png"] = new byte[] { 0x89, 0x50, 0x4E, 0x47 }
    };

    private readonly ISettingProvider _settings;
    private readonly IVirusScanner _scanner;
    private readonly ILogger<UploadedFileGuard> _logger;

    public UploadedFileGuard(ISettingProvider settings, IVirusScanner scanner, ILogger<UploadedFileGuard> logger)
    {
        _settings = settings;
        _scanner = scanner;
        _logger = logger;
    }

    public virtual async Task EnsureAcceptedAsync(byte[] content, string contentType, string rejectedCode)
    {
        var maxSize = long.Parse((await _settings.GetOrNullAsync(HR_Mangment2Settings.DocumentMaxFileSizeBytes))!);
        if (content.LongLength > maxSize)
        {
            throw new BusinessException(HrErrorCodes.Document.FileTooLarge);
        }

        var allowed = ((await _settings.GetOrNullAsync(HR_Mangment2Settings.DocumentAllowedContentTypes)) ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var signatureMatches = !Signatures.TryGetValue(contentType, out var signature) || content.AsSpan().StartsWith(signature);
        if (!allowed.Contains(contentType, StringComparer.OrdinalIgnoreCase) || !signatureMatches)
        {
            throw new BusinessException(HrErrorCodes.Document.FileTypeNotAllowed);
        }

        bool clean;
        try
        {
            clean = await _scanner.ScanAsync(content) == VirusScanResult.Clean;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Virus scan failed for an uploaded file.");
            clean = false;
        }

        if (!clean)
        {
            throw new BusinessException(rejectedCode);
        }
    }
}
