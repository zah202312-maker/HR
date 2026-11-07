using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using HR_Mangment2.Identity;
using HR_Mangment2.MultiTenancy;
using Volo.Abp.TenantManagement;

namespace HR_Mangment2.Data;

public class HR_Mangment2DbMigrationService : ITransientDependency
{
    public ILogger<HR_Mangment2DbMigrationService> Logger { get; set; }

    private readonly IDataSeeder _dataSeeder;
    private readonly IEnumerable<IHR_Mangment2DbSchemaMigrator> _dbSchemaMigrators;
    private readonly ITenantRepository _tenantRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly ITenantManager _tenantManager;
    private readonly ITenantNormalizer _tenantNormalizer;
    private readonly IConfiguration _configuration;
    private readonly HrTenantBootstrapper _tenantBootstrapper;

    public HR_Mangment2DbMigrationService(
        IDataSeeder dataSeeder,
        ITenantRepository tenantRepository,
        ICurrentTenant currentTenant,
        ITenantManager tenantManager,
        ITenantNormalizer tenantNormalizer,
        IConfiguration configuration,
        HrTenantBootstrapper tenantBootstrapper,
        IEnumerable<IHR_Mangment2DbSchemaMigrator> dbSchemaMigrators)
    {
        _dataSeeder = dataSeeder;
        _tenantRepository = tenantRepository;
        _currentTenant = currentTenant;
        _tenantManager = tenantManager;
        _tenantNormalizer = tenantNormalizer;
        _configuration = configuration;
        _tenantBootstrapper = tenantBootstrapper;
        _dbSchemaMigrators = dbSchemaMigrators;

        Logger = NullLogger<HR_Mangment2DbMigrationService>.Instance;
    }

    public async Task MigrateAsync()
    {
        var initialMigrationAdded = AddInitialMigrationIfNotExist();

        if (initialMigrationAdded)
        {
            return;
        }

        Logger.LogInformation("Started database migrations...");

        await MigrateDatabaseSchemaAsync();
        await SeedDataAsync();

        Logger.LogInformation($"Successfully completed host database migrations.");

        if (MultiTenancyConsts.IsEnabled)
        {
            await EnsureCompanyTenantAsync();

            
            var tenants = await _tenantRepository.GetListAsync(includeDetails: true);

            var migratedDatabaseSchemas = new HashSet<string>();
            foreach (var tenant in tenants)
            {
                using (_currentTenant.Change(tenant.Id))
                {
                    if (tenant.ConnectionStrings.Any())
                    {
                        var tenantConnectionStrings = tenant.ConnectionStrings
                            .Select(x => x.Value)
                            .ToList();

                        if (!migratedDatabaseSchemas.IsSupersetOf(tenantConnectionStrings))
                        {
                            await MigrateDatabaseSchemaAsync(tenant);

                            migratedDatabaseSchemas.AddIfNotContains(tenantConnectionStrings);
                        }
                    }

                    await SeedDataAsync(tenant);
                    await _tenantBootstrapper.SeedAsync();
                }

                Logger.LogInformation($"Successfully completed {tenant.Name} tenant database migrations.");
            }

            Logger.LogInformation("Successfully completed all database migrations.");
        }
        Logger.LogInformation("You can safely end this process...");
    }

    /// <summary>
    /// The company runs as one tenant chosen by the server (App:TenantName), so it must exist before the first request.
    /// </summary>
    private async Task EnsureCompanyTenantAsync()
    {
        var tenantName = _configuration["App:TenantName"];
        if (string.IsNullOrWhiteSpace(tenantName))
        {
            throw new InvalidOperationException("App:TenantName is not configured.");
        }

        var existing = await _tenantRepository.FindByNameAsync(_tenantNormalizer.NormalizeName(tenantName)!);
        if (existing == null)
        {
            await _tenantRepository.InsertAsync(await _tenantManager.CreateAsync(tenantName), autoSave: true);
            Logger.LogInformation("Created company tenant {TenantName}.", tenantName);
        }
    }

    private async Task MigrateDatabaseSchemaAsync(Tenant? tenant = null)
    {
        Logger.LogInformation(
            $"Migrating schema for {(tenant == null ? "host" : tenant.Name + " tenant")} database...");
        
        foreach (var migrator in _dbSchemaMigrators)
        {
            await migrator.MigrateAsync();
        }
    }

    private async Task SeedDataAsync(Tenant? tenant = null)
    {
        Logger.LogInformation($"Executing {(tenant == null ? "host" : tenant.Name + " tenant")} database seed...");
        
        var adminPassword = _configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException("Seed:AdminPassword is not configured. Set it in appsettings.secrets.json.");
        }

        await _dataSeeder.SeedAsync(new DataSeedContext(tenant?.Id)
            .WithProperty(IdentityDataSeedContributor.AdminEmailPropertyName,
                HR_Mangment2Consts.AdminEmailDefaultValue)
            .WithProperty(IdentityDataSeedContributor.AdminPasswordPropertyName,
                adminPassword)
        );
    }

    private bool AddInitialMigrationIfNotExist()
    {
        try
        {
            if (!DbMigrationsProjectExists())
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }

        try
        {
            if (!MigrationsFolderExists())
            {
                AddInitialMigration();
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception e)
        {
            Logger.LogWarning("Couldn't determinate if any migrations exist : " + e.Message);
            return false;
        }
    }

    private bool DbMigrationsProjectExists()
    {
        var dbMigrationsProjectFolder = GetEntityFrameworkCoreProjectFolderPath();

        return dbMigrationsProjectFolder != null;
    }

    private bool MigrationsFolderExists()
    {
        var dbMigrationsProjectFolder = GetEntityFrameworkCoreProjectFolderPath();

        return dbMigrationsProjectFolder != null && Directory.Exists(Path.Combine(dbMigrationsProjectFolder, "Migrations"));
    }

    private void AddInitialMigration()
    {
        Logger.LogInformation("Creating initial migration...");

        var procStartInfo = new ProcessStartInfo
        {
            FileName = "abp",
            ArgumentList =
            {
                "create-migration-and-run-migrator",
                GetEntityFrameworkCoreProjectFolderPath()!
            }
        };

        try
        {
            Process.Start(procStartInfo);
        }
        catch (Exception)
        {
            throw new Exception("Couldn't run ABP CLI...");
        }
    }

    private string? GetEntityFrameworkCoreProjectFolderPath()
    {
        var slnDirectoryPath = GetSolutionDirectoryPath();

        if (slnDirectoryPath == null)
        {
            throw new Exception("Solution folder not found!");
        }

        var srcDirectoryPath = Path.Combine(slnDirectoryPath, "src");

        return Directory.GetDirectories(srcDirectoryPath)
            .FirstOrDefault(d => d.EndsWith(".EntityFrameworkCore"));
    }

    private string? GetSolutionDirectoryPath()
    {
        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (currentDirectory != null && Directory.GetParent(currentDirectory.FullName) != null)
        {
            currentDirectory = Directory.GetParent(currentDirectory.FullName);

            if (currentDirectory != null && Directory.GetFiles(currentDirectory.FullName).FirstOrDefault(f => f.EndsWith(".sln") || f.EndsWith(".slnx")) != null)
            {
                return currentDirectory.FullName;
            }
        }

        return null;
    }
}
