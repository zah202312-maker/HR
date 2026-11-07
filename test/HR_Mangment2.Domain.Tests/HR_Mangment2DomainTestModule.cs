using Volo.Abp.Modularity;

namespace HR_Mangment2;

[DependsOn(
    typeof(HR_Mangment2DomainModule),
    typeof(HR_Mangment2TestBaseModule)
)]
public class HR_Mangment2DomainTestModule : AbpModule
{

}
