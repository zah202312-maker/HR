using Microsoft.Extensions.DependencyInjection;
using HR_Mangment2.Complaints.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace HR_Mangment2.Complaints;

[DependsOn(
    typeof(AbpEntityFrameworkCoreModule),
    typeof(ComplaintsDomainModule)
)]
public class ComplaintsEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<ComplaintsDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });
    }
}
