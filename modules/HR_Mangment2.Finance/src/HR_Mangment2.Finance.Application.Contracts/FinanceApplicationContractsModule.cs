using Volo.Abp.Modularity;
using Volo.Abp.Application;

namespace HR_Mangment2.Finance;

[DependsOn(
    typeof(AbpDddApplicationContractsModule),
    typeof(FinanceDomainSharedModule)
)]
public class FinanceApplicationContractsModule : AbpModule
{
}
