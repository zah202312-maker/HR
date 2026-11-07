using Volo.Abp.Modularity;

namespace HR_Mangment2;

[DependsOn(
    typeof(HR_Mangment2ApplicationModule),
    typeof(HR_Mangment2DomainTestModule)
)]
public class HR_Mangment2ApplicationTestModule : AbpModule
{

}
