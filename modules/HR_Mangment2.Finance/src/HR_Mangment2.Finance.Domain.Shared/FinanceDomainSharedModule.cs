using Volo.Abp.Modularity;
using Volo.Abp.Validation;

namespace HR_Mangment2.Finance;

[DependsOn(
    typeof(AbpValidationModule)
)]
public class FinanceDomainSharedModule : AbpModule
{
}
