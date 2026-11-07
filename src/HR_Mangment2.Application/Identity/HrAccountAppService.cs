using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using HR_Mangment2.Notifications;
using Microsoft.Extensions.Options;
using Volo.Abp.Account;
using Volo.Abp.Account.Emailing;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;

namespace HR_Mangment2.Identity;

/// <summary>
/// The reset page calls this. Setting a new password through the link ends the forced change, clears the sign-in
/// lockout and ends every session, because the old password may be why the reset was needed.
/// </summary>
[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IAccountAppService))]
public class HrAccountAppService : AccountAppService
{
    private readonly UserTokenRevoker _tokenRevoker;
    private readonly HrNotificationPublisher _notifications;

    public HrAccountAppService(
        IdentityUserManager userManager,
        IIdentityRoleRepository roleRepository,
        IAccountEmailer accountEmailer,
        IdentitySecurityLogManager identitySecurityLogManager,
        IOptions<Microsoft.AspNetCore.Identity.IdentityOptions> identityOptions,
        UserTokenRevoker tokenRevoker,
        HrNotificationPublisher notifications)
        : base(userManager, roleRepository, accountEmailer, identitySecurityLogManager, identityOptions)
    {
        _tokenRevoker = tokenRevoker;
        _notifications = notifications;
    }

    public override async Task ResetPasswordAsync(ResetPasswordDto input)
    {
        await base.ResetPasswordAsync(input);

        var user = await UserManager.GetByIdAsync(input.UserId);
        user.SetMustChangePassword(false);
        user.SetPasswordResetLinkExpiresAt(null);
        (await UserManager.SetLockoutEndDateAsync(user, null)).CheckErrors();
        (await UserManager.UpdateAsync(user)).CheckErrors();

        await _tokenRevoker.RevokeAllAsync(user.Id);
        await _notifications.PublishAsync(user.Id, NotificationTemplateCode.AccountSecurityChanged);
    }
}
