using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using HR_Mangment2.Common;
using HR_Mangment2.Errors;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using Volo.Abp.Validation;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace HR_Mangment2.Identity;

/// <summary>The role rules and account creation shared by Employee_Create, UserAccount_Create and UserAccount_SetRoles.</summary>
public class AccountProvisioner : ITransientDependency
{
    private const int TemporaryPasswordLength = 16;
    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Lower = "abcdefghijkmnopqrstuvwxyz";
    private const string Digits = "23456789";
    private const string Symbols = "!@#$%*-_";
    private const int MaxNameLength = 64;

    private readonly IdentityUserManager _userManager;
    private readonly IIdentityRoleRepository _roles;
    private readonly ICurrentUser _currentUser;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;

    public AccountProvisioner(
        IdentityUserManager userManager,
        IIdentityRoleRepository roles,
        ICurrentUser currentUser,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant)
    {
        _userManager = userManager;
        _roles = roles;
        _currentUser = currentUser;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
    }

    /// <summary>Returns the exact role names, with employee added for anyone who has an employee file.</summary>
    public virtual async Task<List<string>> ResolveRolesAsync(IEnumerable<string> requested, bool hasEmployeeFile)
    {
        var names = new List<string>();
        foreach (var name in requested.Distinct(StringComparer.Ordinal))
        {
            var role = await _roles.FindByNormalizedNameAsync(_userManager.NormalizeName(name));
            if (role == null || role.Name == HrTenantBootstrapper.AdminUserName)
            {
                throw new BusinessException(HrErrorCodes.Identity.RoleNotFound);
            }

            names.Add(role.Name);
        }

        if (hasEmployeeFile && !names.Contains(HrRoles.Employee))
        {
            names.Add(HrRoles.Employee);
        }

        return names;
    }

    /// <summary>The protected role, combination and singleton rules, for a change from one role set to another.</summary>
    public virtual async Task EnsureRoleChangeAllowedAsync(Guid? targetUserId, IReadOnlyCollection<string> currentRoles, IReadOnlyCollection<string> newRoles)
    {
        EnsureAllowedCombination(newRoles);
        EnsureProtectedRolesRespected(currentRoles, newRoles);

        foreach (var singleton in new[] { HrRoles.HrManager, HrRoles.ExecutiveDirector })
        {
            if (newRoles.Contains(singleton) && !currentRoles.Contains(singleton))
            {
                var holders = await _userManager.GetUsersInRoleAsync(singleton);
                if (holders.Any(x => x.IsActive && x.Id != targetUserId))
                {
                    throw new BusinessException(HrErrorCodes.Identity.SingletonRoleAlreadyAssigned);
                }
            }
        }
    }

    /// <summary>Creates the account with a one-time password. Returns the password so the caller can show it once.</summary>
    public virtual async Task<(IdentityUser User, string TemporaryPassword)> CreateAccountAsync(
        string userName,
        string? email,
        string displayName,
        IReadOnlyCollection<string> roleNames)
    {
        if (await _userManager.FindByNameAsync(userName) != null)
        {
            throw new HrConflictException("The user name is already in use.");
        }

        if (email != null && await _userManager.FindByEmailAsync(email) != null)
        {
            throw new HrConflictException("The email is already in use.");
        }

        var user = new IdentityUser(_guidGenerator.Create(), userName, email ?? HrUserProperties.PlaceholderEmail(userName), _currentTenant.Id)
        {
            Name = displayName.Length > MaxNameLength ? displayName[..MaxNameLength] : displayName
        };
        user.SetMustChangePassword(true);

        var password = GenerateTemporaryPassword();
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            // What remains after the checks above is a user name the identity rules do not accept.
            throw new AbpValidationException(
                "Invalid user name.",
                new List<ValidationResult> { new("Invalid user name.", new[] { "userName" }) });
        }

        (await _userManager.AddToRolesAsync(user, roleNames)).CheckErrors();
        return (user, password);
    }

    private static void EnsureAllowedCombination(IReadOnlyCollection<string> roles)
    {
        var forbidden =
            (roles.Contains(HrRoles.ExecutiveDirector) && roles.Contains(HrRoles.Reviewer)) ||
            (roles.Contains(HrRoles.Accountant) && (roles.Contains(HrRoles.DepartmentManager) || roles.Contains(HrRoles.StoreOfficer)));

        if (forbidden)
        {
            throw new BusinessException(HrErrorCodes.Identity.ForbiddenRoleCombination);
        }
    }

    private void EnsureProtectedRolesRespected(IReadOnlyCollection<string> currentRoles, IReadOnlyCollection<string> newRoles)
    {
        // The executive role itself only changes through a documented procedure outside the system.
        var executiveRoleChanges = currentRoles.Contains(HrRoles.ExecutiveDirector) != newRoles.Contains(HrRoles.ExecutiveDirector);

        var touchesProtectedHolder =
            currentRoles.Contains(HrRoles.ExecutiveDirector) ||
            currentRoles.Contains(HrRoles.HrManager) ||
            newRoles.Contains(HrRoles.HrManager);

        if (executiveRoleChanges || (touchesProtectedHolder && !_currentUser.IsInRole(HrRoles.ExecutiveDirector)))
        {
            throw new BusinessException(HrErrorCodes.Identity.ProtectedRoleChangeRequiresApproval);
        }
    }

    private static string GenerateTemporaryPassword()
    {
        var all = Upper + Lower + Digits + Symbols;
        var chars = new List<char> { Pick(Upper), Pick(Lower), Pick(Digits), Pick(Symbols) };
        while (chars.Count < TemporaryPasswordLength)
        {
            chars.Add(Pick(all));
        }

        // Shuffled so the required character kinds do not always sit at the start.
        for (var i = chars.Count - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars.ToArray());
    }

    private static char Pick(string source) => source[RandomNumberGenerator.GetInt32(source.Length)];
}
