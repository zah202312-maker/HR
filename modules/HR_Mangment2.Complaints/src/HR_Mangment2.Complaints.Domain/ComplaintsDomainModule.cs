using Volo.Abp.Modularity;
using Volo.Abp.Domain;

namespace HR_Mangment2.Complaints;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(HR_Mangment2DomainModule),
    typeof(ComplaintsDomainSharedModule)
)]
public class ComplaintsDomainModule : AbpModule
{
}
