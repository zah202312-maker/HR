using Volo.Abp.Account;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.TenantManagement;

namespace HR_Mangment2;

[DependsOn(
    typeof(HR_Mangment2DomainSharedModule),
    typeof(AbpFeatureManagementApplicationContractsModule),
    typeof(AbpSettingManagementApplicationContractsModule),
    typeof(AbpIdentityApplicationContractsModule),
    typeof(AbpAccountApplicationContractsModule),
    typeof(AbpTenantManagementApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationContractsModule)
)]
public class HR_Mangment2ApplicationContractsModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        HR_Mangment2DtoExtensions.Configure();

        // Contract paging: default 20, maximum 200.
        LimitedResultRequestDto.DefaultMaxResultCount = 20;
        LimitedResultRequestDto.MaxMaxResultCount = 200;
    }
}
