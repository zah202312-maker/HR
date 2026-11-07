using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Common;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Notifications;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace HR_Mangment2.Identity;

[Authorize]
public class UserAccountAppService : HR_Mangment2AppService, IUserAccountAppService
{
    private readonly IdentityUserManager _userManager;
    private readonly IIdentityUserRepository _users;
    private readonly IIdentityRoleRepository _roles;
    private readonly IUserAccountQuery _query;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly AccountProvisioner _provisioner;
    private readonly AccountAccessEvaluator _accessEvaluator;
    private readonly ManagerLinkService _managerLinks;
    private readonly EmployeeReferenceReader _references;
    private readonly PermissionGrantService _grantService;
    private readonly HrNotificationPublisher _notifications;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ICompanyClock _companyClock;

    public UserAccountAppService(
        IdentityUserManager userManager,
        IIdentityUserRepository users,
        IIdentityRoleRepository roles,
        IUserAccountQuery query,
        IRepository<Employee, Guid> employees,
        AccountProvisioner provisioner,
        AccountAccessEvaluator accessEvaluator,
        ManagerLinkService managerLinks,
        EmployeeReferenceReader references,
        PermissionGrantService grantService,
        HrNotificationPublisher notifications,
        IEmailSender emailSender,
        IConfiguration configuration,
        ICompanyClock companyClock)
    {
        _userManager = userManager;
        _users = users;
        _roles = roles;
        _query = query;
        _employees = employees;
        _provisioner = provisioner;
        _accessEvaluator = accessEvaluator;
        _managerLinks = managerLinks;
        _references = references;
        _grantService = grantService;
        _notifications = notifications;
        _emailSender = emailSender;
        _configuration = configuration;
        _companyClock = companyClock;
    }

    [Authorize(HrPermissions.IdentityPermissions.UserAccountRead)]
    public virtual async Task<UserAccountPagedResultDto> GetListAsync(UserAccountListInput input)
    {
        Guid? roleId = null;
        if (!string.IsNullOrWhiteSpace(input.RoleName))
        {
            var role = await _roles.FindByNormalizedNameAsync(_userManager.NormalizeName(input.RoleName));
            if (role == null)
            {
                return new UserAccountPagedResultDto();
            }

            roleId = role.Id;
        }

        var query = await _query.GetQueryableAsync(new UserAccountFilter(input.Search, roleId, input.IsActive, input.MustChangePassword));
        var total = await AsyncExecuter.LongCountAsync(query);
        var page = await AsyncExecuter.ToListAsync(
            query.ApplySorting(
                    input.Sorting,
                    nameof(IdentityUser.UserName),
                    new Dictionary<string, string>
                    {
                        ["userName"] = nameof(IdentityUser.UserName),
                        ["email"] = nameof(IdentityUser.Email),
                        ["isActive"] = nameof(IdentityUser.IsActive),
                        ["creationTime"] = nameof(IdentityUser.CreationTime)
                    })
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        var items = new List<UserAccountDto>();
        foreach (var user in page)
        {
            items.Add(await ToDtoAsync(user));
        }

        return new UserAccountPagedResultDto { TotalCount = total, Items = items };
    }

    [Authorize(HrPermissions.IdentityPermissions.UserAccountCreate)]
    public virtual async Task<UserAccountCreatedDto> CreateAsync(UserAccountCreateDto input)
    {
        var employee = await _employees.GetAsync(input.EmployeeId!.Value);

        if (employee.UserId != null)
        {
            throw new BusinessException(HrErrorCodes.Employee.EmployeeAlreadyHasAccount);
        }

        if (employee.Status == EmployeeStatus.Ended)
        {
            throw new BusinessException(HrErrorCodes.Employee.EmployeeEnded);
        }

        var roleNames = await _provisioner.ResolveRolesAsync(input.RoleNames, hasEmployeeFile: true);
        await _provisioner.EnsureRoleChangeAllowedAsync(null, Array.Empty<string>(), roleNames);

        var (user, temporaryPassword) = await _provisioner.CreateAccountAsync(
            input.UserName,
            input.Email ?? employee.Email ?? throw new BusinessException(HrErrorCodes.Identity.NoVerifiedEmail),
            employee.FullNameAr,
            roleNames);

        employee.LinkAccount(user.Id);
        await _employees.UpdateAsync(employee, autoSave: true);

        return new UserAccountCreatedDto
        {
            Account = await ToDtoAsync(user),
            Credentials = new TemporaryCredentialsDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                TemporaryPassword = temporaryPassword,
                MustChangePassword = true
            }
        };
    }

    [Authorize(HrPermissions.IdentityPermissions.UserAccountLookup)]
    public virtual async Task<UserRefPagedResultDto> LookupAsync(UserAccountLookupInput input)
    {
        var query = await _query.GetQueryableAsync(new UserAccountFilter(input.Search, null, null, null));
        var total = await AsyncExecuter.LongCountAsync(query);
        var ids = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.UserName).Skip(input.SkipCount).Take(input.MaxResultCount).Select(x => x.Id));
        var refs = await _references.GetUserRefsAsync(ids);

        return new UserRefPagedResultDto { TotalCount = total, Items = ids.Select(id => refs[id]).ToList() };
    }

    [Authorize(HrPermissions.IdentityPermissions.UserAccountRead)]
    public virtual async Task<UserAccountDto> GetAsync(Guid id) => await ToDtoAsync(await GetUserAsync(id));

    [Authorize(HrPermissions.IdentityPermissions.UserAccountResetPassword)]
    public virtual async Task<PasswordResetResultDto> ResetPasswordAsync(Guid id, ReasonInputDto input)
    {
        if (id == CurrentUser.GetId())
        {
            throw new BusinessException(HrErrorCodes.Identity.CannotResetOwnPassword);
        }

        var user = await GetUserAsync(id);
        var targetRoles = await _userManager.GetRolesAsync(user);

        if (!CurrentUser.IsInRole(HrRoles.ExecutiveDirector) &&
            (targetRoles.Contains(HrRoles.HrManager) || targetRoles.Contains(HrRoles.ExecutiveDirector)))
        {
            throw new BusinessException(HrErrorCodes.Identity.PasswordResetNotAllowedForTarget);
        }

        if (!user.IsActive)
        {
            throw new BusinessException(HrErrorCodes.Identity.AccountInactive);
        }

        var email = user.GetRealEmail() ?? throw new BusinessException(HrErrorCodes.Identity.NoVerifiedEmail);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var expiresAt = Clock.Now.Add(HrAccountConsts.PasswordResetLinkLifetime);

        user.SetPasswordResetLinkExpiresAt(expiresAt);
        (await _userManager.SetLockoutEndDateAsync(user, null)).CheckErrors();
        (await _userManager.UpdateAsync(user)).CheckErrors();

        await _notifications.PublishAsync(user.Id, NotificationTemplateCode.AccountPasswordReset);

        var baseUrl = (_configuration["App:SelfUrl"] ?? string.Empty).TrimEnd('/');
        var link = $"{baseUrl}/Account/ResetPassword?userId={user.Id}&tenantId={CurrentTenant.Id}&resetToken={Uri.EscapeDataString(token)}";

        string subject, body;
        using (CultureHelper.Use(user.GetPreferredLanguage()))
        {
            subject = L["Account:PasswordResetEmailSubject"];
            body = string.Format(L["Account:PasswordResetEmailBody"], link);
        }

        CurrentUnitOfWork!.OnCompleted(async () =>
        {
            try
            {
                await _emailSender.SendAsync(email, subject, body, isBodyHtml: false);
            }
            catch (Exception ex)
            {
                // The link is already recorded and the change committed. Only the account id goes to the log, never the link.
                Logger.LogWarning(ex, "Password reset email failed for user {UserId}.", user.Id);
            }
        });

        return new PasswordResetResultDto { UserId = user.Id, DeliveredVia = PasswordResetDelivery.Email, ExpiresAt = expiresAt };
    }

    [Authorize(HrPermissions.IdentityPermissions.UserAccountSetRoles)]
    public virtual async Task<UserAccountDto> SetRolesAsync(Guid id, UserRolesUpdateDto input)
    {
        if (id == CurrentUser.GetId())
        {
            throw new BusinessException(HrErrorCodes.Identity.CannotChangeOwnRoles);
        }

        var user = await GetUserAsync(id);
        if (user.ConcurrencyStamp != input.ConcurrencyStamp)
        {
            throw new HrConflictException("The account changed since it was read.");
        }

        var employee = await _employees.FirstOrDefaultAsync(x => x.UserId == id);
        var currentRoles = (await _userManager.GetRolesAsync(user)).ToList();
        var newRoles = await _provisioner.ResolveRolesAsync(input.RoleNames, employee != null);

        await _provisioner.EnsureRoleChangeAllowedAsync(id, currentRoles, newRoles);
        await EnsureNotLastSystemAdminAsync(id, currentRoles, newRoles);

        if (employee != null)
        {
            await ApplyEmployeeEffectsAsync(employee, newRoles, input.ManagerEmployeeId);
        }

        (await _userManager.SetRolesAsync(user, newRoles)).CheckErrors();
        await _notifications.PublishAsync(id, NotificationTemplateCode.AccountSecurityChanged);

        return await ToDtoAsync(user);
    }

    [Authorize(HrPermissions.IdentityPermissions.PermissionManage)]
    public virtual async Task<PermissionListResultDto> GetPermissionsAsync(Guid id)
    {
        var user = await GetUserAsync(id);
        var roles = await _userManager.GetRolesAsync(user);
        return await _grantService.GetAsync(UserPermissionValueProvider.ProviderName, id.ToString(), roles.ToList());
    }

    [Authorize(HrPermissions.IdentityPermissions.PermissionManage)]
    public virtual async Task<PermissionListResultDto> UpdatePermissionsAsync(Guid id, PermissionGrantUpdateDto input)
    {
        if (id == CurrentUser.GetId())
        {
            throw new BusinessException(HrErrorCodes.Identity.CannotChangeOwnPermissions);
        }

        var user = await GetUserAsync(id);
        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        await _grantService.UpdateAsync(UserPermissionValueProvider.ProviderName, id.ToString(), roles, input.Permissions);

        return await _grantService.GetAsync(UserPermissionValueProvider.ProviderName, id.ToString(), roles);
    }

    private async Task EnsureNotLastSystemAdminAsync(Guid userId, IReadOnlyCollection<string> currentRoles, IReadOnlyCollection<string> newRoles)
    {
        if (!currentRoles.Contains(HrRoles.SystemAdmin) || newRoles.Contains(HrRoles.SystemAdmin))
        {
            return;
        }

        var holders = await _userManager.GetUsersInRoleAsync(HrRoles.SystemAdmin);
        if (!holders.Any(x => x.IsActive && x.Id != userId))
        {
            throw new BusinessException(HrErrorCodes.Identity.LastSystemAdminCannotBeRemoved);
        }
    }

    private async Task ApplyEmployeeEffectsAsync(Employee employee, IReadOnlyCollection<string> newRoles, Guid? requestedManagerId)
    {
        if (employee.Status == EmployeeStatus.Ended)
        {
            throw new BusinessException(HrErrorCodes.Employee.EmployeeEnded);
        }

        // People already linked to this employee must still be manageable by the new set of roles.
        foreach (var report in await _employees.GetListAsync(x => x.ManagerEmployeeId == employee.Id && x.Status != EmployeeStatus.Ended))
        {
            var allowed = ManagerLinkService.AllowedManagerRoles(await _managerLinks.GetRolesAsync(report));
            if (allowed != null && !allowed.Any(role => newRoles.Contains(role)))
            {
                throw new BusinessException(HrErrorCodes.Identity.ManagerRoleHasDirectReports);
            }
        }

        if (ManagerLinkService.AllowedManagerRoles(newRoles) == null)
        {
            if (requestedManagerId != null)
            {
                throw new BusinessException(HrErrorCodes.Employee.ExecutiveCannotHaveManager);
            }

            if (employee.ManagerEmployeeId != null)
            {
                await _managerLinks.CloseCurrentLinkAsync(employee.Id, await _companyClock.GetTodayAsync());
                employee.SetManager(null);
                await _employees.UpdateAsync(employee, autoSave: true);
            }

            return;
        }

        var managerToCheck = requestedManagerId ?? employee.ManagerEmployeeId;
        if (managerToCheck == null)
        {
            throw new BusinessException(HrErrorCodes.Employee.ManagerRequired);
        }

        var violation = await _managerLinks.FindViolationAsync(employee.Id, newRoles, managerToCheck.Value);
        if (violation != null)
        {
            // Without a new manager in the input, a link the new roles invalidate means one has to be sent.
            throw new BusinessException(requestedManagerId == null ? HrErrorCodes.Employee.ManagerRequired : violation);
        }

        if (requestedManagerId != null && requestedManagerId != employee.ManagerEmployeeId)
        {
            await _managerLinks.ChangeAsync(employee, await _employees.GetAsync(requestedManagerId.Value), null);
            await _employees.UpdateAsync(employee, autoSave: true);
        }
    }

    private async Task<IdentityUser> GetUserAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || user.UserName == HrTenantBootstrapper.AdminUserName)
        {
            throw new EntityNotFoundException(typeof(IdentityUser), id);
        }

        return user;
    }

    private async Task<UserAccountDto> ToDtoAsync(IdentityUser user)
    {
        var employee = await _employees.FirstOrDefaultAsync(x => x.UserId == user.Id);
        var state = await _accessEvaluator.GetAsync(user.Id);

        return new UserAccountDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.GetRealEmail(),
            Employee = employee == null ? null : await _references.ToRefAsync(employee),
            IsActive = user.IsActive,
            IsLockedOut = user.LockoutEnabled && user.LockoutEnd > DateTimeOffset.UtcNow,
            MustChangePassword = state.MustChangePassword,
            MfaRequired = state.MfaRequired,
            MfaEnabled = state.MfaEnabled,
            RoleNames = (await _users.GetRoleNamesAsync(user.Id)).OrderBy(x => x).ToList(),
            CreationTime = user.CreationTime,
            LastModificationTime = user.LastModificationTime,
            ConcurrencyStamp = user.ConcurrencyStamp
        };
    }
}

public class UserAccountCreateDtoValidator : AbstractValidator<UserAccountCreateDto>
{
    public UserAccountCreateDtoValidator()
    {
        RuleFor(x => x.EmployeeId).NotNull();
        RuleFor(x => x.UserName).NotNull().Length(3, 64);
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.RoleNames).NotEmpty();
        RuleForEach(x => x.RoleNames).NotEmpty().MaximumLength(64);
    }
}

public class UserRolesUpdateDtoValidator : AbstractValidator<UserRolesUpdateDto>
{
    public UserRolesUpdateDtoValidator()
    {
        RuleFor(x => x.RoleNames).NotEmpty();
        RuleForEach(x => x.RoleNames).NotEmpty().MaximumLength(64);
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}
