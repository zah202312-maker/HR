using Microsoft.AspNetCore.Identity;
using IdentityUser = Volo.Abp.Identity.IdentityUser;
using System;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Auditing;
using HR_Mangment2.Errors;
using HR_Mangment2.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Volo.Abp;
using HR_Mangment2.Files;
using Microsoft.Extensions.Logging;
using Volo.Abp.Auditing;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using Volo.Abp.Security.Claims;

namespace HR_Mangment2.Identity;

[Authorize]
public class MyAccountAppService : HR_Mangment2AppService, IMyAccountAppService
{

private readonly IdentityUserManager _userManager;
    private readonly AccountAccessEvaluator _accessEvaluator;
    private readonly UserTokenRevoker _tokenRevoker;
    private readonly IMyAccountEmployeeReader _employeeReader;
    private readonly ICurrentAccessTokenAccessor _accessTokenAccessor;
    private readonly HrNotificationPublisher _notifications;
    private readonly IRepository<HrAuditEntry, Guid> _auditEntries;
    private readonly IRepository<ProfilePhoto, Guid> _photos;
    private readonly IBlobContainer<HrFilesContainer> _files;
    private readonly IVirusScanner _scanner;

    public MyAccountAppService(
        IdentityUserManager userManager,
        AccountAccessEvaluator accessEvaluator,
        UserTokenRevoker tokenRevoker,
        IMyAccountEmployeeReader employeeReader,
        ICurrentAccessTokenAccessor accessTokenAccessor,
        HrNotificationPublisher notifications,
        IRepository<HrAuditEntry, Guid> auditEntries,
        IRepository<ProfilePhoto, Guid> photos,
        IBlobContainer<HrFilesContainer> files,
        IVirusScanner scanner)
    {
        _userManager = userManager;
        _accessEvaluator = accessEvaluator;
        _tokenRevoker = tokenRevoker;
        _employeeReader = employeeReader;
        _accessTokenAccessor = accessTokenAccessor;
        _notifications = notifications;
        _auditEntries = auditEntries;
        _photos = photos;
        _files = files;
        _scanner = scanner;
    }

    public virtual async Task<MyAccountDto> GetAsync()
    {
        return await BuildAsync(await GetUserAsync());
    }

    public virtual async Task<MyAccountDto> UpdateAsync(MyAccountUpdateDto input)
    {
        var user = await GetUserAsync();
        user.SetPreferredLanguage(input.PreferredLanguage!.Value == AppLanguage.En ? "en" : "ar");
        (await _userManager.UpdateAsync(user)).CheckErrors();
        return await BuildAsync(user);
    }

    public virtual async Task<ChangePasswordResultDto> ChangePasswordAsync(ChangePasswordInputDto input)
    {
        var user = await GetUserAsync();

        if (!await _userManager.CheckPasswordAsync(user, input.CurrentPassword))
        {
            throw new BusinessException(HrErrorCodes.Identity.CurrentPasswordIncorrect);
        }

        if (input.NewPassword == input.CurrentPassword)
        {
            throw new BusinessException(HrErrorCodes.Identity.NewPasswordSameAsCurrent);
        }

        // Read before the change moves LastPasswordChangeTime forward.
        var temporaryPasswordUsedBefore = user.GetMustChangePassword() && await HadEarlierSignInAsync(user);

        try
        {
            (await _userManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword)).CheckErrors();
        }
        catch (AbpIdentityResultException)
        {
            throw new BusinessException(HrErrorCodes.Identity.PasswordPolicyViolation);
        }

        user.SetMustChangePassword(false);
        (await _userManager.UpdateAsync(user)).CheckErrors();

        await _tokenRevoker.RevokeOtherSessionsAsync(user.Id, _accessTokenAccessor.GetTokenId());
        await _notifications.PublishAsync(user.Id, NotificationTemplateCode.AccountSecurityChanged);

        if (temporaryPasswordUsedBefore)
        {
            // The owner's own sign-in is one. Any more means the temporary password was used before this change.
            await _notifications.PublishAsync(user.Id, NotificationTemplateCode.AccountSecurityChanged);
        }

        var state = await _accessEvaluator.GetAsync(user.Id);
        return new ChangePasswordResultDto { AccessGate = state.Gate, ChangedAt = Clock.Now };
    }

    private async Task<bool> HadEarlierSignInAsync(IdentityUser user)
    {
        var since = user.LastPasswordChangeTime?.UtcDateTime ?? user.CreationTime;
        var signIns = await _auditEntries.CountAsync(x =>
            x.UserId == user.Id && x.Action == AuditActionType.Login && x.Succeeded && x.OccurredAt >= since);
        return signIns > 1;
    }


    /// <summary>
    /// The owner's photo only (decision 2026-11-11). JPEG, PNG or WebP up to 1 MB, checked by the file's first bytes.
    /// A new upload gets a new id and removes the old photo, so clients may cache a photo by its id.
    /// </summary>
    [DisableAuditing]
    public virtual async Task<MyPhotoDto> UploadPhotoAsync(MyPhotoUploadInput input)
    {
        if (input.Content.LongLength > ProfilePhoto.MaxSizeBytes)
        {
            throw new BusinessException(HrErrorCodes.Document.FileTooLarge);
        }

        var contentType = DetectImageType(input.Content);
        if (contentType == null || !string.Equals(contentType, input.ContentType, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException(HrErrorCodes.Document.FileTypeNotAllowed);
        }

        bool clean;
        try
        {
            clean = await _scanner.ScanAsync(input.Content) == VirusScanResult.Clean;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Virus scan failed for a profile photo.");
            clean = false;
        }

        if (!clean)
        {
            throw new BusinessException(HrErrorCodes.Document.FileTypeNotAllowed);
        }

        var userId = CurrentUser.GetId();
        await RemovePhotoAsync(userId);

        var photo = new ProfilePhoto(GuidGenerator.Create(), CurrentTenant.Id, userId, contentType, input.Content.LongLength);
        await _files.SaveAsync(photo.BlobName, input.Content, overrideExisting: true);
        await _photos.InsertAsync(photo, autoSave: true);
        return new MyPhotoDto { PhotoId = photo.Id };
    }

    public virtual async Task DeletePhotoAsync() => await RemovePhotoAsync(CurrentUser.GetId());

    /// <summary>Any signed-in user reads any photo, like the name next to it (decision 2026-11-11).</summary>
    public virtual async Task<ProfilePhotoContent> GetPhotoAsync(Guid photoId)
    {
        var photo = await _photos.FindAsync(photoId) ?? throw new EntityNotFoundException(typeof(ProfilePhoto), photoId);
        var content = await _files.GetAllBytesOrNullAsync(photo.BlobName) ?? throw new EntityNotFoundException(typeof(ProfilePhoto), photoId);
        return new ProfilePhotoContent { Content = content, ContentType = photo.ContentType };
    }

    private async Task RemovePhotoAsync(Guid userId)
    {
        foreach (var old in await _photos.GetListAsync(x => x.UserId == userId))
        {
            await _photos.DeleteAsync(old, autoSave: true);
            await _files.DeleteAsync(old.BlobName);
        }
    }

    /// <summary>The image type from the first bytes, or null when it is not JPEG, PNG or WebP.</summary>
    private static string? DetectImageType(byte[] content)
    {
        var span = content.AsSpan();
        if (span.StartsWith(new byte[] { 0xFF, 0xD8, 0xFF }))
        {
            return "image/jpeg";
        }

        if (span.StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
        {
            return "image/png";
        }

        if (span.Length >= 12 && span[..4].SequenceEqual("RIFF"u8) && span.Slice(8, 4).SequenceEqual("WEBP"u8))
        {
            return "image/webp";
        }

        return null;
    }
    private async Task<IdentityUser> GetUserAsync()
    {
        return await _userManager.GetByIdAsync(CurrentUser.GetId());
    }

    private async Task<MyAccountDto> BuildAsync(IdentityUser user)
    {
        var state = await _accessEvaluator.GetAsync(user.Id);
        var employee = await _employeeReader.GetAsync(user.Id);
        var fullName = string.Join(' ', new[] { user.Name, user.Surname }.Where(x => !string.IsNullOrWhiteSpace(x)));

        return new MyAccountDto
        {
            UserId = user.Id,
            UserName = user.UserName,
            DisplayName = employee.Employee?.FullName ?? (fullName.Length > 0 ? fullName : user.UserName),
            Email = user.Email,
            PreferredLanguage = user.GetPreferredLanguage() == "en" ? AppLanguage.En : AppLanguage.Ar,
            RoleNames = (await _userManager.GetRolesAsync(user)).OrderBy(x => x).ToList(),
            Employee = employee.Employee,
            DirectReportCount = employee.DirectReportCount,
            AccessGate = state.Gate,
            MustChangePassword = state.MustChangePassword,
            MfaRequired = state.MfaRequired,
            MfaEnabled = state.MfaEnabled,
            PhotoId = (await _photos.FirstOrDefaultAsync(x => x.UserId == user.Id))?.Id
        };
    }
}
