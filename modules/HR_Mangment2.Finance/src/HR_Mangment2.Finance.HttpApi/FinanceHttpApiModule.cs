using Volo.Abp.Modularity;
using Volo.Abp.AspNetCore.Mvc;

namespace HR_Mangment2.Finance;

[DependsOn(
    typeof(AbpAspNetCoreMvcModule),
    typeof(HR_Mangment2HttpApiModule),
    typeof(FinanceApplicationContractsModule)
)]
public class FinanceHttpApiModule : AbpModule
{
}
