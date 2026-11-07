using Microsoft.Extensions.DependencyInjection;
using HR_Mangment2.Finance.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace HR_Mangment2.Finance;

[DependsOn(
    typeof(AbpEntityFrameworkCoreModule),
    typeof(FinanceDomainModule)
)]
public class FinanceEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<FinanceDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpEntityOptions>(options =>
        {
            options.Entity<PayrollRun>(entity => entity.DefaultWithDetailsFunc = query => query.Include(x => x.Items).ThenInclude(x => x.Lines).AsSplitQuery());
        });
    }
}
