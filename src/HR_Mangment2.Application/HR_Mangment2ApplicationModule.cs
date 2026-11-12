using System.Threading.Tasks;
using HR_Mangment2.Attendance;
using Volo.Abp;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.FluentValidation;
using Volo.Abp.Mapperly;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.TenantManagement;

namespace HR_Mangment2;

[DependsOn(
    typeof(HR_Mangment2DomainModule),
    typeof(HR_Mangment2ApplicationContractsModule),
    typeof(AbpFluentValidationModule),
    typeof(AbpMapperlyModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class HR_Mangment2ApplicationModule : AbpModule
{
    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await context.AddBackgroundWorkerAsync<AttendanceDailyWorker>();
        await context.AddBackgroundWorkerAsync<HR_Mangment2.Requests.RequestReminderWorker>();
        await context.AddBackgroundWorkerAsync<HR_Mangment2.Evaluations.EvaluationCycleWorker>();
        await context.AddBackgroundWorkerAsync<HR_Mangment2.Leaves.LeaveMaintenanceWorker>();
        await context.AddBackgroundWorkerAsync<HR_Mangment2.HrEvaluations.HrEvaluationAlertWorker>();
        await context.AddBackgroundWorkerAsync<HR_Mangment2.EmploymentContracts.EmploymentRecordsWorker>();
    }
}
