using System;
using System.Threading.Tasks;
using HR_Mangment2.Identity;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;

namespace HR_Mangment2.Infrastructure;

public static class TestUsers
{
    public const string Password = "Known-Test-1a!";

    /// <summary>Signs in a holder of an MFA-required role and completes enrollment.</summary>
    public static async Task<System.Net.Http.HttpClient> SignedInWithMfaAsync(HostFixture host, string language, params string[] roles)
    {
        var (_, userName) = await CreateAsync(host, false, roles);
        var client = TestAuth.ClientFor(host, (await TestAuth.SignInAsync(host, userName, Password)).AccessToken, language);
        var enroll = await client.PostAsync("/api/app/account/mfa/enroll", null);
        var enrollment = await System.Net.Http.Json.HttpContentJsonExtensions.ReadFromJsonAsync<System.Text.Json.JsonElement>(enroll.Content);
        var confirm = await System.Net.Http.Json.HttpClientJsonExtensions.PostAsJsonAsync(client, "/api/app/account/mfa/confirm", new { code = Totp.Compute(enrollment.GetProperty("sharedKey").GetString()!) });
        confirm.EnsureSuccessStatusCode();
        return client;
    }

    /// <summary>Creates a user in the company tenant, with no pending gate unless asked for.</summary>
    public static async Task<(Guid Id, string UserName)> CreateAsync(HostFixture host, bool mustChangePassword, params string[] roles)
    {
        using var scope = host.Factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var tenant = await services.GetRequiredService<ITenantRepository>().FindByNameAsync(
            services.GetRequiredService<ITenantNormalizer>().NormalizeName(HostFixture.TestTenantName)!);
        using var change = services.GetRequiredService<ICurrentTenant>().Change(tenant!.Id);
        using var uow = services.GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true);

        var manager = services.GetRequiredService<IdentityUserManager>();
        var userName = "u" + Guid.NewGuid().ToString("N")[..12];
        var user = new IdentityUser(Guid.NewGuid(), userName, userName + "@test.invalid", tenant.Id);
        user.SetMustChangePassword(mustChangePassword);
        (await manager.CreateAsync(user, Password)).CheckSucceeded();
        (await manager.AddToRolesAsync(user, roles)).CheckSucceeded();

        await uow.CompleteAsync();
        return (user.Id, userName);
    }

    private static void CheckSucceeded(this Microsoft.AspNetCore.Identity.IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors));
        }
    }
}
