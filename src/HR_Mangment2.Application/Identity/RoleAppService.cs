using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Errors;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Identity;
using Volo.Abp.Localization;
using IdentityRole = Volo.Abp.Identity.IdentityRole;

namespace HR_Mangment2.Identity;

[Authorize]
public class RoleAppService : HR_Mangment2AppService, IRoleAppService
{
    private const string NameArProperty = "NameAr";
    private const string NameEnProperty = "NameEn";
    private const string FrameworkAdminRole = "admin";

    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IdentityRoleManager _roleManager;
    private readonly IIdentityUserRepository _userRepository;
    private readonly PermissionGrantService _grantService;

    public RoleAppService(
        IIdentityRoleRepository roleRepository,
        IdentityRoleManager roleManager,
        IIdentityUserRepository userRepository,
        PermissionGrantService grantService)
    {
        _roleRepository = roleRepository;
        _roleManager = roleManager;
        _userRepository = userRepository;
        _grantService = grantService;
    }

    [Authorize(HrPermissions.IdentityPermissions.RoleRead)]
    public virtual async Task<RolePagedResultDto> GetListAsync(RoleListInput input)
    {
        var roles = (await _roleRepository.GetListAsync(filter: input.Search))
            .Where(r => r.Name != FrameworkAdminRole)
            .OrderBy(r => r.Name)
            .ToList();

        var page = roles.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
        var items = new List<RoleDto>();
        foreach (var role in page)
        {
            items.Add(await ToDtoAsync(role));
        }

        return new RolePagedResultDto { TotalCount = roles.Count, Items = items };
    }

    [Authorize(HrPermissions.IdentityPermissions.RoleManage)]
    public virtual async Task<RoleDto> CreateAsync(RoleCreateDto input)
    {
        var role = new IdentityRole(GuidGenerator.Create(), input.Name, CurrentTenant.Id);
        role.SetProperty(NameArProperty, input.NameAr);
        role.SetProperty(NameEnProperty, input.NameEn);
        (await _roleManager.CreateAsync(role)).CheckErrors();
        await CurrentUnitOfWork!.SaveChangesAsync();

        return await ToDtoAsync(role);
    }

    [Authorize(HrPermissions.IdentityPermissions.RoleReadDetail)]
    public virtual async Task<RoleDto> GetAsync(Guid id)
    {
        return await ToDtoAsync(await GetRoleAsync(id));
    }

    [Authorize(HrPermissions.IdentityPermissions.RoleManage)]
    public virtual async Task<RoleDto> UpdateAsync(Guid id, RoleUpdateDto input)
    {
        var role = await GetRoleAsync(id);
        role.SetProperty(NameArProperty, input.NameAr);
        role.SetProperty(NameEnProperty, input.NameEn);
        role.ConcurrencyStamp = input.ConcurrencyStamp;
        await _roleRepository.UpdateAsync(role, autoSave: true);

        return await ToDtoAsync(role);
    }

    [Authorize(HrPermissions.IdentityPermissions.RoleManage)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var role = await GetRoleAsync(id);
        if (role.IsStatic)
        {
            throw new BusinessException(HrErrorCodes.Identity.StaticRoleCannotBeDeleted);
        }

        if (await _userRepository.GetCountAsync(roleId: id) > 0)
        {
            throw new BusinessException(HrErrorCodes.Identity.RoleInUse);
        }

        (await _roleManager.DeleteAsync(role)).CheckErrors();
    }

    [Authorize(HrPermissions.IdentityPermissions.PermissionManage)]
    public virtual async Task<PermissionListResultDto> GetPermissionsAsync(Guid id)
    {
        var role = await GetRoleAsync(id);
        return await _grantService.GetAsync(RolePermissionValueProvider.ProviderName, role.Name, Array.Empty<string>());
    }

    [Authorize(HrPermissions.IdentityPermissions.PermissionManage)]
    public virtual async Task<PermissionListResultDto> UpdatePermissionsAsync(Guid id, PermissionGrantUpdateDto input)
    {
        var role = await GetRoleAsync(id);
        await _grantService.UpdateAsync(RolePermissionValueProvider.ProviderName, role.Name, new[] { role.Name }, input.Permissions);
        return await GetPermissionsAsync(id);
    }

    private async Task<IdentityRole> GetRoleAsync(Guid id)
    {
        var role = await _roleRepository.FindAsync(id);
        if (role == null || role.Name == FrameworkAdminRole)
        {
            throw new EntityNotFoundException(typeof(IdentityRole), id);
        }

        return role;
    }

    private async Task<RoleDto> ToDtoAsync(IdentityRole role)
    {
        var arabic = role.GetProperty<string>(NameArProperty);
        var english = role.GetProperty<string>(NameEnProperty);

        if (role.IsStatic)
        {
            using (CultureHelper.Use("ar"))
            {
                arabic ??= L["Role:" + role.Name];
            }

            using (CultureHelper.Use("en"))
            {
                english ??= L["Role:" + role.Name];
            }
        }

        arabic ??= role.Name;
        english ??= role.Name;

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            NameAr = arabic,
            NameEn = english,
            DisplayName = CultureHelper.IsRtl ? arabic : english,
            IsStatic = role.IsStatic,
            UserCount = (int)await _userRepository.GetCountAsync(roleId: role.Id),
            ConcurrencyStamp = role.ConcurrencyStamp
        };
    }
}
