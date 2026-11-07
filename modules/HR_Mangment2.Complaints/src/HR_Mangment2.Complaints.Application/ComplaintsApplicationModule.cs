using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Modularity;
using Volo.Abp.Application;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Mapperly;

namespace HR_Mangment2.Complaints;

[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(AbpMapperlyModule),
    typeof(HR_Mangment2ApplicationModule),
    typeof(ComplaintsDomainModule),
    typeof(ComplaintsApplicationContractsModule)
)]
public class ComplaintsApplicationModule : AbpModule
{
    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await context.AddBackgroundWorkerAsync<ComplaintOverdueReminderWorker>();
        await context.AddBackgroundWorkerAsync<ComplaintHandlerExpiryWorker>();
    }
}
