using Volo.Abp.Modularity;
using Volo.Abp.Validation;

namespace HR_Mangment2.Complaints;

[DependsOn(
    typeof(AbpValidationModule)
)]
public class ComplaintsDomainSharedModule : AbpModule
{
}
