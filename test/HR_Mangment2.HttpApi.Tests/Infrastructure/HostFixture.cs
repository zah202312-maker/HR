using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HR_Mangment2.Data;
using HR_Mangment2.DbMigrator;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using FluentValidation;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Data;
using Xunit;

namespace HR_Mangment2.Infrastructure;

[CollectionDefinition(Name)]
public class HostCollection : ICollectionFixture<HostFixture>
{
    public const string Name = "Host";
}

/// <summary>
/// Runs the real Host against a dedicated PostgreSQL database, in the Testing environment,
/// which behaves like production for error details and exception pages.
/// </summary>
public sealed class HostFixture : IAsyncLifetime
{
    public const string TestTenantName = "default";
    public const string InitialPassword = "Initial-Test-1a!";

    public string ConnectionString { get; } = TestDatabase.ConnectionStringFor("Api");

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public async Task ClearNotificationJobsAsync()
    {
        await using var connection = new Npgsql.NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new Npgsql.NpgsqlCommand("DELETE FROM \"AbpBackgroundJobs\" WHERE \"JobName\" = 'HrNotificationDelivery'", connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task InitializeAsync()
    {
        await MigrateAsync();
        await ClearNotificationJobsAsync();

        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("RateLimit:PermitPerMinute", "100000");

            // A throwaway key for the test database. It protects nothing real.
            builder.UseSetting("Complaints:MasterKey", "dGVzdC1vbmx5LW1hc3Rlci1rZXktZm9yLWNvbXBsYWludHM=");
            builder.ConfigureTestServices(services =>
            {
                // PostConfigure wins over the Host secrets file, so the development database stays untouched.
                services.PostConfigure<AbpDbConnectionOptions>(options =>
                {
                    options.ConnectionStrings.Default = ConnectionString;
                });
                services.PostConfigure<AbpBackgroundJobOptions>(options =>
                {
                    options.IsJobExecutionEnabled = false;
                });

                services.Replace(ServiceDescriptor.Singleton<Volo.Abp.Emailing.IEmailSender, CapturingEmailSender>());
                services.AddControllers().AddApplicationPart(typeof(ProbeController).Assembly);
                services.AddTransient<ProbeController>();
                services.AddTransient<IValidator<ProbeFluentInput>, ProbeFluentInputValidator>();
            });
        });

        _ = Factory.Server;
    }

    public HttpClient CreateClient(string? language = null)
    {
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        // Same as the generated Flutter client: errors must come back as JSON.
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        if (language != null)
        {
            client.DefaultRequestHeaders.Add("Accept-Language", language);
        }

        return client;
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
    }

    private async Task MigrateAsync()
    {
        // Every run starts from an empty database, so tests do not depend on earlier runs.
        if (!ConnectionString.Contains("_Test_", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to reset a database that is not a test database.");
        }

        await using (var context = new HR_Mangment2.EntityFrameworkCore.HR_Mangment2DbContext(
                         new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<HR_Mangment2.EntityFrameworkCore.HR_Mangment2DbContext>()
                             .UseNpgsql(ConnectionString).Options))
        {
            await context.Database.EnsureDeletedAsync();
        }

        using var application = await AbpApplicationFactory.CreateAsync<HR_Mangment2DbMigratorModule>(options =>
        {
            options.Services.ReplaceConfiguration(new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = ConnectionString,
                    ["App:TenantName"] = TestTenantName,
                    ["Seed:AdminPassword"] = "Test-only-" + Guid.NewGuid().ToString("N") + "aA1!",
                    ["StringEncryption:DefaultPassPhrase"] = "test-only-passphrase",
                    ["Seed:InitialPassword"] = InitialPassword,
                    ["OpenIddict:Applications:HR_Mangment2_Flutter:ClientId"] = TestAuth.ClientId,
                    ["OpenIddict:Applications:HR_Mangment2_Flutter:RedirectUris:0"] = TestAuth.RedirectUri
                })
                .Build());
            options.UseAutofac();
            options.AddDataMigrationEnvironment();
        });

        await application.InitializeAsync();
        await application.ServiceProvider.GetRequiredService<HR_Mangment2DbMigrationService>().MigrateAsync();
        await application.ShutdownAsync();
    }
}
