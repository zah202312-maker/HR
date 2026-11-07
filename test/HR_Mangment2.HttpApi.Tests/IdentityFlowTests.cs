using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using HR_Mangment2.Errors;
using HR_Mangment2.Identity;
using HR_Mangment2.Infrastructure;
using Shouldly;
using Xunit;

namespace HR_Mangment2;

[Collection(HostCollection.Name)]
public class IdentityFlowTests
{
    private const string NewPassword = "Changed-Pass-2b!";

    private readonly HostFixture _host;

    public IdentityFlowTests(HostFixture host)
    {
        _host = host;
    }

    private async Task<HttpClient> SignedInAsync(string userName, string password = TestUsers.Password) =>
        TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, userName, password)).AccessToken, "en");

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("error").GetProperty("code").GetString();

    [Fact]
    public async Task Password_gate_blocks_everything_except_its_allowed_operations_until_changed()
    {
        var (_, userName) = await TestUsers.CreateAsync(_host, mustChangePassword: true, HrRoles.Employee);
        var client = await SignedInAsync(userName);

        var me = await client.GetFromJsonAsync<JsonElement>("/api/app/me");
        me.GetProperty("accessGate").GetString().ShouldBe("passwordChangeRequired");
        me.GetProperty("mustChangePassword").GetBoolean().ShouldBeTrue();

        var config = await client.GetFromJsonAsync<JsonElement>("/api/abp/application-configuration");
        config.GetProperty("extraProperties").GetProperty("accessGate").GetString().ShouldBe("passwordChangeRequired");

        var blocked = await client.GetAsync("/api/app/role");
        blocked.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await ErrorCodeAsync(blocked)).ShouldBe(HrErrorCodes.Identity.PasswordChangeRequired);

        var patch = await client.PatchAsJsonAsync("/api/app/me", new { preferredLanguage = "en" });
        patch.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var change = await client.PostAsJsonAsync("/api/app/account/change-password", new { currentPassword = TestUsers.Password, newPassword = NewPassword });
        change.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await change.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessGate").GetString().ShouldBe("none");

        (await client.PatchAsJsonAsync("/api/app/me", new { preferredLanguage = "en" })).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await client.GetFromJsonAsync<JsonElement>("/api/app/me")).GetProperty("preferredLanguage").GetString().ShouldBe("en");
    }

    [Fact]
    public async Task Change_password_rules_use_contract_codes()
    {
        var (_, userName) = await TestUsers.CreateAsync(_host, mustChangePassword: false, HrRoles.Employee);
        var client = await SignedInAsync(userName);

        async Task<string?> Try(string current, string next)
        {
            var response = await client.PostAsJsonAsync("/api/app/account/change-password", new { currentPassword = current, newPassword = next });
            response.StatusCode.ShouldBe((HttpStatusCode)422);
            return await ErrorCodeAsync(response);
        }

        (await Try("Wrong-Pass-1a!", NewPassword)).ShouldBe(HrErrorCodes.Identity.CurrentPasswordIncorrect);
        (await Try(TestUsers.Password, TestUsers.Password)).ShouldBe(HrErrorCodes.Identity.NewPasswordSameAsCurrent);
        (await Try(TestUsers.Password, "short")).ShouldBe(HrErrorCodes.Identity.PasswordPolicyViolation);
    }

    [Fact]
    public async Task Mfa_gate_enrollment_and_sign_in_with_code_work_end_to_end()
    {
        var (_, userName) = await TestUsers.CreateAsync(_host, mustChangePassword: false, HrRoles.Employee, HrRoles.Accountant);
        var client = await SignedInAsync(userName);

        var blocked = await client.GetAsync("/api/app/delegation/my");
        blocked.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await ErrorCodeAsync(blocked)).ShouldBe(HrErrorCodes.Identity.MfaEnrollmentRequired);

        var enroll = await client.PostAsync("/api/app/account/mfa/enroll", null);
        enroll.StatusCode.ShouldBe(HttpStatusCode.OK);
        enroll.Headers.CacheControl!.NoStore.ShouldBeTrue();
        var enrollment = await enroll.Content.ReadFromJsonAsync<JsonElement>();
        var secret = enrollment.GetProperty("sharedKey").GetString()!;
        enrollment.GetProperty("authenticatorUri").GetString().ShouldStartWith("otpauth://totp/");

        var wrong = await client.PostAsJsonAsync("/api/app/account/mfa/confirm", new { code = "000000" });
        (await ErrorCodeAsync(wrong)).ShouldBe(HrErrorCodes.Identity.MfaCodeInvalid);

        var confirm = await client.PostAsJsonAsync("/api/app/account/mfa/confirm", new { code = Totp.Compute(secret) });
        confirm.StatusCode.ShouldBe(HttpStatusCode.OK);
        var codes = await confirm.Content.ReadFromJsonAsync<JsonElement>();
        codes.GetProperty("accessGate").GetString().ShouldBe("none");
        codes.GetProperty("recoveryCodes").GetArrayLength().ShouldBe(8);

        var status = await client.GetFromJsonAsync<JsonElement>("/api/app/account/mfa");
        status.GetProperty("isEnabled").GetBoolean().ShouldBeTrue();
        status.GetProperty("recoveryCodesRemaining").GetInt32().ShouldBe(8);

        var token = await TestAuth.SignInAsync(_host, userName, TestUsers.Password, secret);
        var second = TestAuth.ClientFor(_host, token.AccessToken);
        (await second.GetFromJsonAsync<JsonElement>("/api/app/me")).GetProperty("accessGate").GetString().ShouldBe("none");
    }

    [Fact]
    public async Task Refresh_works_and_changing_password_revokes_other_sessions_only()
    {
        var (_, userName) = await TestUsers.CreateAsync(_host, mustChangePassword: false, HrRoles.Employee);
        var other = await TestAuth.SignInAsync(_host, userName, TestUsers.Password);
        var current = await TestAuth.SignInAsync(_host, userName, TestUsers.Password);
        other.RefreshToken.ShouldNotBeNull();

        var client = TestAuth.ClientFor(_host, current.AccessToken);
        (await client.PostAsJsonAsync("/api/app/account/change-password", new { currentPassword = TestUsers.Password, newPassword = NewPassword }))
            .StatusCode.ShouldBe(HttpStatusCode.OK);

        (await RefreshAsync(other.RefreshToken!)).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var kept = await RefreshAsync(current.RefreshToken!);
        kept.StatusCode.ShouldBe(HttpStatusCode.OK, await kept.Content.ReadAsStringAsync());
    }

    private Task<HttpResponseMessage> RefreshAsync(string refreshToken) =>
        _host.CreateClient().PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = TestAuth.ClientId,
            ["refresh_token"] = refreshToken
        }));

    [Fact]
    public async Task Password_grant_is_rejected_in_oauth_format()
    {
        var response = await _host.CreateClient().PostAsync("/connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = TestAuth.ClientId,
            ["username"] = "x",
            ["password"] = "y"
        }));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadFromJsonAsync<JsonElement>()).TryGetProperty("error", out var error).ShouldBeTrue();
        error.ValueKind.ShouldBe(JsonValueKind.String);
    }

    [Theory]
    [InlineData("/Account/Register")]
    [InlineData("/Account/ForgotPassword")]
    [InlineData("/Account/Manage")]
    public async Task Self_service_account_pages_are_gone(string path)
    {
        var response = await _host.CreateClient().GetAsync(path);
        response.Headers.Location?.ToString().ShouldContain("httpStatusCode=404");
    }

    [Fact]
    public async Task Anonymous_calls_get_401()
    {
        (await _host.CreateClient().GetAsync("/api/app/me")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Roles_require_permission_and_protect_built_in_and_sensitive_rules()
    {
        var (_, plainUser) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee);
        var employee = await SignedInAsync(plainUser);
        var denied = await employee.GetAsync("/api/app/role");
        denied.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await denied.Content.ReadFromJsonAsync<JsonElement>()).TryGetProperty("error", out _).ShouldBeTrue();

        var admin = await SignedInSystemAdminAsync();

        var list = await admin.GetFromJsonAsync<JsonElement>("/api/app/role?maxResultCount=200");
        var items = list.GetProperty("items").EnumerateArray().ToList();
        foreach (var name in HrRoles.All)
        {
            items.ShouldContain(r => r.GetProperty("name").GetString() == name && r.GetProperty("isStatic").GetBoolean());
        }

        items.ShouldNotContain(r => r.GetProperty("name").GetString() == "admin");

        var manager = items.Single(r => r.GetProperty("name").GetString() == HrRoles.DepartmentManager).GetProperty("id").GetString();

        var delete = await admin.DeleteAsync("/api/app/role/" + manager);
        delete.StatusCode.ShouldBe((HttpStatusCode)422);
        (await ErrorCodeAsync(delete)).ShouldBe(HrErrorCodes.Identity.StaticRoleCannotBeDeleted);

        var permissions = await admin.GetFromJsonAsync<JsonElement>($"/api/app/role/{manager}/permission");
        var names = permissions.GetProperty("groups").EnumerateArray()
            .SelectMany(g => g.GetProperty("permissions").EnumerateArray())
            .Select(p => p.GetProperty("name").GetString()!)
            .ToList();
        names.ShouldNotBeEmpty();
        names.ShouldNotContain(n => n.StartsWith("Hr.Complaint") || n.StartsWith("Hr.Finance") || n.StartsWith("Hr.Identity."));

        async Task<string?> Grant(string name)
        {
            var response = await admin.PutAsJsonAsync($"/api/app/role/{manager}/permission", new { permissions = new[] { new { name, isGranted = true } } });
            response.StatusCode.ShouldBe((HttpStatusCode)422);
            return await ErrorCodeAsync(response);
        }

        (await Grant("Hr.Complaints.Handle")).ShouldBe(HrErrorCodes.Identity.ProtectedPermissionNotGrantable);
        (await Grant("Hr.Finance.SalaryProfile.Read")).ShouldBe(HrErrorCodes.Identity.FinancialPermissionNotGrantable);
        (await Grant("Hr.Identity.Role.Manage")).ShouldBe(HrErrorCodes.Identity.ProtectedPermission);
        (await Grant("Hr.DoesNotExist")).ShouldBe(HrErrorCodes.Identity.PermissionNotFound);
    }

    [Fact]
    public async Task Custom_role_lifecycle_and_concurrency()
    {
        var admin = await SignedInSystemAdminAsync();
        var roleName = "custom" + System.Guid.NewGuid().ToString("N")[..8];

        var created = await admin.PostAsJsonAsync("/api/app/role", new { name = roleName, nameAr = "دور", nameEn = "Role" });
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        var role = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = role.GetProperty("id").GetString();
        var stamp = role.GetProperty("concurrencyStamp").GetString();
        role.GetProperty("isStatic").GetBoolean().ShouldBeFalse();

        var updated = await admin.PutAsJsonAsync("/api/app/role/" + id, new { nameAr = "دور ٢", nameEn = "Role 2", concurrencyStamp = stamp });
        updated.StatusCode.ShouldBe(HttpStatusCode.OK);

        var stale = await admin.PutAsJsonAsync("/api/app/role/" + id, new { nameAr = "x", nameEn = "y", concurrencyStamp = stamp });
        stale.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        (await admin.DeleteAsync("/api/app/role/" + id)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Mfa_reset_follows_who_may_reset_whom()
    {
        var admin = await SignedInSystemAdminAsync();
        var (_, plain) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee);
        var me = await admin.GetFromJsonAsync<JsonElement>("/api/app/me");

        var own = await admin.PostAsJsonAsync($"/api/app/user-account/{me.GetProperty("userId").GetString()}/mfa/reset", new { reason = "x" });
        (await ErrorCodeAsync(own)).ShouldBe(HrErrorCodes.Identity.CannotResetOwnMfa);

        var (employeeId, _) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee);
        var other = await admin.PostAsJsonAsync($"/api/app/user-account/{employeeId}/mfa/reset", new { reason = "x" });
        (await ErrorCodeAsync(other)).ShouldBe(HrErrorCodes.Identity.MfaResetNotAllowedForTarget);
        plain.ShouldNotBeNull();
    }

    private async Task<HttpClient> SignedInSystemAdminAsync()
    {
        var (_, userName) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee, HrRoles.SystemAdmin);
        var client = await SignedInAsync(userName);
        var enrollment = await (await client.PostAsync("/api/app/account/mfa/enroll", null)).Content.ReadFromJsonAsync<JsonElement>();
        var confirm = await client.PostAsJsonAsync("/api/app/account/mfa/confirm", new { code = Totp.Compute(enrollment.GetProperty("sharedKey").GetString()!) });
        confirm.StatusCode.ShouldBe(HttpStatusCode.OK);
        return client;
    }
}
