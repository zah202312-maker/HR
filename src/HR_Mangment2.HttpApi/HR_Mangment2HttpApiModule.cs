using Localization.Resources.AbpUi;
using HR_Mangment2.Json;
using HR_Mangment2.Localization;
using HR_Mangment2.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Json.SystemTextJson;
using Volo.Abp.Account;
using Volo.Abp.SettingManagement;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.HttpApi;
using Volo.Abp.Localization;
using Volo.Abp.TenantManagement;

namespace HR_Mangment2;

 [DependsOn(
    typeof(HR_Mangment2ApplicationContractsModule),
    typeof(AbpPermissionManagementHttpApiModule),
    typeof(AbpSettingManagementHttpApiModule),
    typeof(AbpAccountHttpApiModule),
    typeof(AbpIdentityHttpApiModule),
    typeof(AbpTenantManagementHttpApiModule),
    typeof(AbpFeatureManagementHttpApiModule)
    )]
public class HR_Mangment2HttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        ConfigureLocalization();
        ConfigureJson(context);

        Configure<MvcOptions>(options =>
        {
            options.Filters.Add<HR_Mangment2.Routing.ApplicationConfigurationContractFilter>();
            options.Conventions.Add(new ContractRouteConvention());
        });
    }

    private static void ConfigureJson(ServiceConfigurationContext context)
    {
        // PostConfigure runs after the framework has added its own converters.
        context.Services.PostConfigure<JsonOptions>(options => HrJsonConfiguration.Apply(options.JsonSerializerOptions));
        context.Services.PostConfigure<AbpSystemTextJsonSerializerOptions>(options => HrJsonConfiguration.Apply(options.JsonSerializerOptions));
    }

    private void ConfigureLocalization()
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<HR_Mangment2Resource>()
                .AddBaseTypes(
                    typeof(AbpUiResource)
                );
        });
    }
}
