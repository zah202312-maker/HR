using System.Linq;
using HR_Mangment2.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace HR_Mangment2.Permissions;

public class HrPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        foreach (var group in HrPermissions.DefaultRoles.Keys.GroupBy(GroupName))
        {
            var definition = context.AddGroup(group.Key, Text("PermissionGroup:" + group.Key));
            foreach (var name in group)
            {
                definition.AddPermission(name, Text("Permission:" + name));
            }
        }
    }

    private static string GroupName(string permission) => string.Join('.', permission.Split('.').Take(2));

    private static LocalizableString Text(string key) => LocalizableString.Create<HR_Mangment2Resource>(key);
}
