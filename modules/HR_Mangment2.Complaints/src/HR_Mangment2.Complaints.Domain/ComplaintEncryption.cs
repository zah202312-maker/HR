using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Volo.Abp.DependencyInjection;

namespace HR_Mangment2.Complaints;

/// <summary>Where the per-company key lives. Production replaces this with the cloud key vault the design calls for.</summary>
public interface IComplaintKeyVault
{
    Task<byte[]> WrapAsync(byte[] key, Guid? tenantId);

    Task<byte[]> UnwrapAsync(byte[] wrappedKey, Guid? tenantId);
}

/// <summary>
/// Temporary stand-in for the key vault: the company key is derived from a master key kept in configuration.
/// With no master key it refuses to work, so content is never stored unprotected.
/// </summary>
public class ConfigurationComplaintKeyVault : IComplaintKeyVault, ITransientDependency
{
    public const string MasterKeySetting = "Complaints:MasterKey";

    private readonly IConfiguration _configuration;

    public ConfigurationComplaintKeyVault(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<byte[]> WrapAsync(byte[] key, Guid? tenantId) => Task.FromResult(ComplaintCipher.Seal(CompanyKey(tenantId), key, "dek"));

    public Task<byte[]> UnwrapAsync(byte[] wrappedKey, Guid? tenantId) => Task.FromResult(ComplaintCipher.Open(CompanyKey(tenantId), wrappedKey, "dek"));

    private byte[] CompanyKey(Guid? tenantId)
    {
        var master = _configuration[MasterKeySetting];
        if (string.IsNullOrWhiteSpace(master))
        {
            throw new InvalidOperationException($"{MasterKeySetting} is not configured.");
        }

        return HKDF.DeriveKey(
            HashAlgorithmName.SHA256,
            Convert.FromBase64String(master),
            ComplaintCipher.KeySize,
            info: Encoding.UTF8.GetBytes("complaints:" + (tenantId?.ToString("N") ?? "host")));
    }
}

/// <summary>AES-256-GCM. Each value is bound to its place by the context, so a sealed value cannot be moved to another complaint or field.</summary>
public static class ComplaintCipher
{
    public const int KeySize = 32;
    private const int NonceSize = 12;
    private const int TagSize = 16;

    public static byte[] NewKey() => RandomNumberGenerator.GetBytes(KeySize);

    public static byte[] Seal(byte[] key, byte[] plain, string context)
    {
        var sealedValue = new byte[NonceSize + TagSize + plain.Length];
        var nonce = sealedValue.AsSpan(0, NonceSize);
        RandomNumberGenerator.Fill(nonce);

        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plain, sealedValue.AsSpan(NonceSize + TagSize), sealedValue.AsSpan(NonceSize, TagSize), Encoding.UTF8.GetBytes(context));
        return sealedValue;
    }

    public static byte[] Open(byte[] key, byte[] sealedValue, string context)
    {
        var plain = new byte[sealedValue.Length - NonceSize - TagSize];

        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(sealedValue.AsSpan(0, NonceSize), sealedValue.AsSpan(NonceSize + TagSize), sealedValue.AsSpan(NonceSize, TagSize), plain, Encoding.UTF8.GetBytes(context));
        return plain;
    }
}

/// <summary>One complaint's key, opened for the length of one operation.</summary>
public sealed class ComplaintContentKey
{
    private readonly byte[] _key;
    private readonly Guid _complaintId;

    public ComplaintContentKey(byte[] key, Guid complaintId)
    {
        _key = key;
        _complaintId = complaintId;
    }

    public byte[] Seal(string field, byte[] plain) => ComplaintCipher.Seal(_key, plain, $"{_complaintId:N}:{field}");

    public byte[] Open(string field, byte[] sealedValue) => ComplaintCipher.Open(_key, sealedValue, $"{_complaintId:N}:{field}");

    public byte[] SealText(string field, string text) => Seal(field, Encoding.UTF8.GetBytes(text));

    public string OpenText(string field, byte[] sealedValue) => Encoding.UTF8.GetString(Open(field, sealedValue));
}

public static class ComplaintFields
{
    public const string Subject = "subject";
    public const string Content = "content";
    public const string Decision = "decision";

    public static string ActionNote(Guid actionId) => "action:" + actionId.ToString("N");

    public static string AttachmentName(Guid attachmentId) => "attachment-name:" + attachmentId.ToString("N");

    public static string AttachmentFile(Guid attachmentId) => "attachment:" + attachmentId.ToString("N");
}

/// <summary>Creates and opens per-complaint keys.</summary>
public class ComplaintKeyService : ITransientDependency
{
    private readonly IComplaintKeyVault _vault;

    public ComplaintKeyService(IComplaintKeyVault vault)
    {
        _vault = vault;
    }

    public virtual async Task<(ComplaintContentKey Key, byte[] Wrapped)> CreateAsync(Guid complaintId, Guid? tenantId)
    {
        var key = ComplaintCipher.NewKey();
        return (new ComplaintContentKey(key, complaintId), await _vault.WrapAsync(key, tenantId));
    }

    public virtual async Task<ComplaintContentKey> OpenAsync(Complaint complaint) =>
        new(await _vault.UnwrapAsync(complaint.WrappedKey, complaint.TenantId), complaint.Id);
}
