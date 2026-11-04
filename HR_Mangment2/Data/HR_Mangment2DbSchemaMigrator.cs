using Volo.Abp.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace HR_Mangment2.Data;

public class HR_Mangment2DbSchemaMigrator : ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public HR_Mangment2DbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        
        /* We intentionally resolving the HR_Mangment2DbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<HR_Mangment2DbContext>()
            .Database
            .MigrateAsync();

    }
}
