using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace HR_Mangment2.Data;

/* This is used if database provider does't define
 * IHR_Mangment2DbSchemaMigrator implementation.
 */
public class NullHR_Mangment2DbSchemaMigrator : IHR_Mangment2DbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
