using System;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;

namespace HR_Mangment2.EntityFrameworkCore;

[DependsOn(
    typeof(HR_Mangment2ApplicationTestModule),
    typeof(HR_Mangment2EntityFrameworkCoreModule)
)]
public class HR_Mangment2EntityFrameworkCoreTestModule : AbpModule
{
    private static readonly Lazy<string> MigratedConnection = new(() =>
    {
        var connection = TestDatabase.ConnectionStringFor("Ef");
        using var context = new HR_Mangment2DbContext(
            new DbContextOptionsBuilder<HR_Mangment2DbContext>().UseNpgsql(connection).Options);
        context.Database.Migrate();
        return connection;
    });

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<FeatureManagementOptions>(options =>
        {
            options.SaveStaticFeaturesToDatabase = false;
            options.IsDynamicFeatureStoreEnabled = false;
        });
        Configure<PermissionManagementOptions>(options =>
        {
            options.SaveStaticPermissionsToDatabase = false;
            options.IsDynamicPermissionStoreEnabled = false;
        });

        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = MigratedConnection.Value;
        });
    }
}
