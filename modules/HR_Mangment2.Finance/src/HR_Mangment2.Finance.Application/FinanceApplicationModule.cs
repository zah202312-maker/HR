using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Modularity;
using Volo.Abp.Application;
using Volo.Abp.Mapperly;

namespace HR_Mangment2.Finance;

[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(AbpMapperlyModule),
    typeof(HR_Mangment2ApplicationModule),
    typeof(FinanceDomainModule),
    typeof(FinanceApplicationContractsModule)
)]
public class FinanceApplicationModule : AbpModule
{
    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await context.AddBackgroundWorkerAsync<AdjustmentSuggestionWorker>();
    }
}
