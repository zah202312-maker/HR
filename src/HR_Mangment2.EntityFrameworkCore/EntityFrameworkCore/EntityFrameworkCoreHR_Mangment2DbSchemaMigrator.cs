using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HR_Mangment2.Data;
using Volo.Abp.DependencyInjection;

namespace HR_Mangment2.EntityFrameworkCore;

public class EntityFrameworkCoreHR_Mangment2DbSchemaMigrator
    : IHR_Mangment2DbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreHR_Mangment2DbSchemaMigrator(IServiceProvider serviceProvider)
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
