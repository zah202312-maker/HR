using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HR_Mangment2.Data;

public class HR_Mangment2DbContextFactory : IDesignTimeDbContextFactory<HR_Mangment2DbContext>
{
    public HR_Mangment2DbContext CreateDbContext(string[] args)
    {
        HR_Mangment2GlobalFeatureConfigurator.Configure();
        HR_Mangment2ModuleExtensionConfigurator.Configure();

        // https://www.npgsql.org/efcore/release-notes/6.0.html#opting-out-of-the-new-timestamp-mapping-logic
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        
        HR_Mangment2EfCoreEntityExtensionMappings.Configure();
        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<HR_Mangment2DbContext>()
            .UseNpgsql(configuration.GetConnectionString("Default"));

        return new HR_Mangment2DbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}