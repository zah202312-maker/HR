using HR_Mangment2.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace HR_Mangment2.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(HR_Mangment2EntityFrameworkCoreModule),
    typeof(HR_Mangment2ApplicationContractsModule)
)]
public class HR_Mangment2DbMigratorModule : AbpModule
{
}
