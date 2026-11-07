using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Errors;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.PermissionManagement;

namespace HR_Mangment2.Identity;

/// <summary>Reads and changes grants for a role or user, limited to ordinary Hr.* permissions.</summary>
public class PermissionGrantService : ITransientDependency
{
    private readonly IPermissionDefinitionManager _definitions;
    private readonly IPermissionManager _permissionManager;
    private readonly IPermissionGrantRepository _grantRepository;
    private readonly IStringLocalizerFactory _localizerFactory;

    public PermissionGrantService(
        IPermissionDefinitionManager definitions,
        IPermissionManager permissionManager,
        IPermissionGrantRepository grantRepository,
        IStringLocalizerFactory localizerFactory)
    {
        _definitions = definitions;
        _permissionManager = permissionManager;
        _grantRepository = grantRepository;
        _localizerFactory = localizerFactory;
    }

    public virtual async Task<PermissionListResultDto> GetAsync(
        string providerName,
        string providerKey,
        IReadOnlyCollection<string> inheritedFromRoles)
    {
        var result = new PermissionListResultDto();

        foreach (var group in (await _definitions.GetGroupsAsync()).Where(g => g.Name.StartsWith("Hr.", StringComparison.Ordinal)))
        {
            var dto = new PermissionGroupDto { Name = group.Name, DisplayName = group.DisplayName.Localize(_localizerFactory) };

            foreach (var permission in group.GetPermissionsWithChildren().Where(p => !ProtectedPermissions.IsProtected(p.Name)))
            {
                var direct = await _grantRepository.FindAsync(permission.Name, providerName, providerKey) != null;
                var byRoles = new List<string>();

                foreach (var role in inheritedFromRoles)
                {
                    if (await _grantRepository.FindAsync(permission.Name, RolePermissionValueProvider.ProviderName, role) != null)
                    {
                        byRoles.Add(role);
                    }
                }

                dto.Permissions.Add(new PermissionGrantInfoDto
                {
                    Name = permission.Name,
                    DisplayName = permission.DisplayName.Localize(_localizerFactory),
                    ParentName = permission.Parent?.Name,
                    IsDirectlyGranted = direct,
                    IsGranted = direct || byRoles.Count > 0,
                    GrantedByRoleNames = byRoles
                });
            }

            if (dto.Permissions.Count > 0)
            {
                result.Groups.Add(dto);
            }
        }

        return result;
    }

    public virtual async Task UpdateAsync(
        string providerName,
        string providerKey,
        IReadOnlyCollection<string> targetRoleNames,
        IEnumerable<PermissionGrantChangeDto> changes)
    {
        var list = changes.ToList();

        foreach (var change in list)
        {
            if (await _definitions.GetOrNullAsync(change.Name) == null)
            {
                throw new BusinessException(HrErrorCodes.Identity.PermissionNotFound).WithData("name", change.Name);
            }

            ProtectedPermissions.EnsureGrantable(change.Name, targetRoleNames);
        }

        foreach (var change in list)
        {
            await _permissionManager.SetAsync(change.Name, providerName, providerKey, change.IsGranted);
        }
    }
}
