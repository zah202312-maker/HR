using Volo.Abp.Modularity;
using Volo.Abp.Application;

namespace HR_Mangment2.Complaints;

[DependsOn(
    typeof(AbpDddApplicationContractsModule),
    typeof(ComplaintsDomainSharedModule)
)]
public class ComplaintsApplicationContractsModule : AbpModule
{
}
