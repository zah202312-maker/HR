using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HR_Mangment2.Errors;
using HR_Mangment2.Notifications;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace HR_Mangment2.Identity;

[Authorize]
public class MfaAppService : HR_Mangment2AppService, IMfaAppService
{
    private const int RecoveryCodeCount = 8;
    private const string Issuer = "HR";

    private readonly IdentityUserManager _userManager;
    private readonly AccountAccessEvaluator _accessEvaluator;
    private readonly UserTokenRevoker _tokenRevoker;
    private readonly IAuditingManager _auditingManager;
    private readonly HrNotificationPublisher _notifications;

    public MfaAppService(
        IdentityUserManager userManager,
        AccountAccessEvaluator accessEvaluator,
        UserTokenRevoker tokenRevoker,
        IAuditingManager auditingManager,
        HrNotificationPublisher notifications)
    {
        _userManager = userManager;
        _accessEvaluator = accessEvaluator;
        _tokenRevoker = tokenRevoker;
        _auditingManager = auditingManager;
        _notifications = notifications;
    }

    public virtual async Task<MfaStatusDto> GetStatusAsync()
    {
        var user = await GetCurrentAsync();

        return new MfaStatusDto
        {
            IsRequired = await _accessEvaluator.IsMfaRequiredAsync(user),
            IsEnabled = user.TwoFactorEnabled,
            EnabledAt = user.GetMfaEnabledAt(),
            RecoveryCodesRemaining = user.TwoFactorEnabled ? await _userManager.CountRecoveryCodesAsync(user) : 0
        };
    }

    public virtual async Task<MfaEnrollmentDto> StartEnrollmentAsync()
    {
        var user = await GetCurrentAsync();
        if (user.TwoFactorEnabled)
        {
            throw new BusinessException(HrErrorCodes.Identity.MfaAlreadyEnabled);
        }

        (await _userManager.ResetAuthenticatorKeyAsync(user)).CheckErrors();
        var key = (await _userManager.GetAuthenticatorKeyAsync(user))!;
        var label = Uri.EscapeDataString(Issuer + ":" + user.UserName);

        return new MfaEnrollmentDto
        {
            SharedKey = key,
            AuthenticatorUri = $"otpauth://totp/{label}?secret={key}&issuer={Issuer}&digits=6&period=30"
        };
    }

    public virtual async Task<MfaRecoveryCodesDto> ConfirmEnrollmentAsync(MfaCodeInputDto input)
    {
        var user = await GetCurrentAsync();
        if (user.TwoFactorEnabled)
        {
            throw new BusinessException(HrErrorCodes.Identity.MfaAlreadyEnabled);
        }

        if (string.IsNullOrEmpty(await _userManager.GetAuthenticatorKeyAsync(user)))
        {
            throw new BusinessException(HrErrorCodes.Identity.MfaEnrollmentNotStarted);
        }

        await EnsureCodeAsync(user, input.Code);

        (await _userManager.SetTwoFactorEnabledAsync(user, true)).CheckErrors();
        user.SetMfaEnabledAt(Clock.Now);
        (await _userManager.UpdateAsync(user)).CheckErrors();
        await _notifications.PublishAsync(user.Id, NotificationTemplateCode.AccountSecurityChanged);

        return await IssueCodesAsync(user);
    }

    public virtual async Task<MfaRecoveryCodesDto> RegenerateRecoveryCodesAsync(MfaCodeInputDto input)
    {
        var user = await GetCurrentAsync();
        if (!user.TwoFactorEnabled)
        {
            throw new BusinessException(HrErrorCodes.Identity.MfaNotEnabled);
        }

        await EnsureCodeAsync(user, input.Code);
        return await IssueCodesAsync(user);
    }

    [Authorize(HrPermissions.IdentityPermissions.UserAccountResetMfa)]
    public virtual async Task ResetForUserAsync(Guid userId, ReasonInputDto input)
    {
        if (userId == CurrentUser.GetId())
        {
            throw new BusinessException(HrErrorCodes.Identity.CannotResetOwnMfa);
        }

        var target = await _userManager.FindByIdAsync(userId.ToString())
                     ?? throw new EntityNotFoundException(typeof(IdentityUser), userId);

        // QUESTIONS.md item 1: the executive for anyone, the system admin only for accountants and other system admins.
        var actorRoles = CurrentUser.Roles;
        if (!actorRoles.Contains(HrRoles.ExecutiveDirector))
        {
            var targetRoles = await _userManager.GetRolesAsync(target);
            var allowedTarget = targetRoles.Contains(HrRoles.Accountant) || targetRoles.Contains(HrRoles.SystemAdmin);
            var protectedTarget = targetRoles.Contains(HrRoles.ExecutiveDirector) || targetRoles.Contains(HrRoles.HrManager);
            if (!allowedTarget || protectedTarget)
            {
                throw new BusinessException(HrErrorCodes.Identity.MfaResetNotAllowedForTarget);
            }
        }

        if (!target.TwoFactorEnabled)
        {
            throw new BusinessException(HrErrorCodes.Identity.MfaNotEnabled);
        }

        (await _userManager.SetTwoFactorEnabledAsync(target, false)).CheckErrors();
        (await _userManager.ResetAuthenticatorKeyAsync(target)).CheckErrors();
        (await _userManager.ReplaceCodesAsync(target)).CheckErrors();
        target.SetMfaEnabledAt(null);
        (await _userManager.UpdateAsync(target)).CheckErrors();
        await _tokenRevoker.RevokeAllAsync(target.Id);
        await _notifications.PublishAsync(target.Id, NotificationTemplateCode.AccountSecurityChanged);

        _auditingManager.Current?.Log.Comments.Add("MFA reset. Reason: " + input.Reason);
    }

    private async Task<IdentityUser> GetCurrentAsync()
    {
        return await _userManager.GetByIdAsync(CurrentUser.GetId());
    }

    private async Task EnsureCodeAsync(IdentityUser user, string code)
    {
        var valid = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, code);
        if (!valid)
        {
            throw new BusinessException(HrErrorCodes.Identity.MfaCodeInvalid);
        }
    }

    private async Task<MfaRecoveryCodesDto> IssueCodesAsync(IdentityUser user)
    {
        var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, RecoveryCodeCount);
        var state = await _accessEvaluator.GetAsync(user.Id);

        return new MfaRecoveryCodesDto { RecoveryCodes = codes!.ToList(), AccessGate = state.Gate };
    }
}

internal static class RecoveryCodeExtensions
{
    /// <summary>Clears recovery codes by storing an empty set.</summary>
    public static Task<IdentityResult> ReplaceCodesAsync(this IdentityUserManager manager, IdentityUser user)
    {
        return manager.ReplaceCodesCoreAsync(user);
    }

    private static async Task<IdentityResult> ReplaceCodesCoreAsync(this IdentityUserManager manager, IdentityUser user)
    {
        await manager.GenerateNewTwoFactorRecoveryCodesAsync(user, 0);
        return IdentityResult.Success;
    }
}
