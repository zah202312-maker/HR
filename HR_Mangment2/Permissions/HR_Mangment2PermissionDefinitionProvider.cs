using HR_Mangment2.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Permissions;

public class HR_Mangment2PermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(HR_Mangment2Permissions.GroupName);


        var booksPermission = myGroup.AddPermission(HR_Mangment2Permissions.Books.Default, L("Permission:Books"));
        booksPermission.AddChild(HR_Mangment2Permissions.Books.Create, L("Permission:Books.Create"));
        booksPermission.AddChild(HR_Mangment2Permissions.Books.Edit, L("Permission:Books.Edit"));
        booksPermission.AddChild(HR_Mangment2Permissions.Books.Delete, L("Permission:Books.Delete"));

        var authorsPermission = myGroup.AddPermission(HR_Mangment2Permissions.Authors.Default, L("Permission:Authors"));
        authorsPermission.AddChild(HR_Mangment2Permissions.Authors.Create, L("Permission:Authors.Create"));
        authorsPermission.AddChild(HR_Mangment2Permissions.Authors.Edit, L("Permission:Authors.Edit"));
        authorsPermission.AddChild(HR_Mangment2Permissions.Authors.Delete, L("Permission:Authors.Delete"));

        //Define your own permissions here. Example:
        //myGroup.AddPermission(HR_Mangment2Permissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<HR_Mangment2Resource>(name);
    }
}
