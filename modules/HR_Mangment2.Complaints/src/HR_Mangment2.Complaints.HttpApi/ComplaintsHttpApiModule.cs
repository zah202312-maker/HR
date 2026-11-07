using Volo.Abp.Modularity;
using Volo.Abp.AspNetCore.Mvc;

namespace HR_Mangment2.Complaints;

[DependsOn(
    typeof(AbpAspNetCoreMvcModule),
    typeof(HR_Mangment2HttpApiModule),
    typeof(ComplaintsApplicationContractsModule)
)]
public class ComplaintsHttpApiModule : AbpModule
{
}
