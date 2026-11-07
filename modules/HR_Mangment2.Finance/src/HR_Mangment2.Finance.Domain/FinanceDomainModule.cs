using Volo.Abp.Modularity;
using Volo.Abp.Domain;

namespace HR_Mangment2.Finance;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(HR_Mangment2DomainModule),
    typeof(FinanceDomainSharedModule)
)]
public class FinanceDomainModule : AbpModule
{
}
