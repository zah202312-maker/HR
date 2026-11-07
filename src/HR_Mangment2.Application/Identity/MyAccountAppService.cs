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

    public MyAccountAppService(
        IdentityUserManager userManager,
        AccountAccessEvaluator accessEvaluator,
        UserTokenRevoker tokenRevoker,
        IMyAccountEmployeeReader employeeReader,
        ICurrentAccessTokenAccessor accessTokenAccessor,
        HrNotificationPublisher notifications,
        IRepository<HrAuditEntry, Guid> auditEntries)
    {
        _userManager = userManager;
        _accessEvaluator = accessEvaluator;
        _tokenRevoker = tokenRevoker;
        _employeeReader = employeeReader;
        _accessTokenAccessor = accessTokenAccessor;
        _notifications = notifications;
        _auditEntries = auditEntries;
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
            MfaEnabled = state.MfaEnabled
        };
    }
}
