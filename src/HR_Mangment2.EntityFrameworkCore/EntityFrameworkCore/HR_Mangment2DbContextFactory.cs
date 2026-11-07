using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HR_Mangment2.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class HR_Mangment2DbContextFactory : IDesignTimeDbContextFactory<HR_Mangment2DbContext>
{
    public HR_Mangment2DbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        HR_Mangment2EfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<HR_Mangment2DbContext>()
            .UseNpgsql(configuration.GetConnectionString("Default"));
        
        return new HR_Mangment2DbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../HR_Mangment2.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
