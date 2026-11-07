using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using HR_Mangment2.Auditing;
using HR_Mangment2.Identity;
using HR_Mangment2.Infrastructure;
using Shouldly;
using Volo.Abp.Auditing;
using Xunit;

namespace HR_Mangment2;

[Collection(HostCollection.Name)]
public class AuditLogTests
{
    private readonly HostFixture _host;

    public AuditLogTests(HostFixture host)
    {
        _host = host;
    }

    private Task<HttpClient> AdminAsync() => TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.SystemAdmin);

    private static async Task<List<JsonElement>> ListAsync(HttpClient reader, string query)
    {
        var page = await reader.GetFromJsonAsync<JsonElement>("/api/app/audit-log?maxResultCount=200&" + query);
        return page.GetProperty("items").EnumerateArray().ToList();
    }

    private static List<string> Fields(JsonElement row) =>
        row.GetProperty("changedFields").EnumerateArray().Select(x => x.GetString()!).ToList();

    [Fact]
    public async Task Setting_change_lists_field_names_and_never_values()
    {
        var admin = await AdminAsync();
        var current = await admin.GetFromJsonAsync<JsonElement>("/api/app/system-settings/contracts");
        var newValue = current.GetProperty("alertDays").GetInt32() == 73 ? 74 : 73;

        var since = DateTime.UtcNow.AddSeconds(-5).ToString("O");
        (await admin.PutAsJsonAsync("/api/app/system-settings/contracts", new
        {
            alertDays = newValue,
            concurrencyStamp = current.GetProperty("concurrencyStamp").GetString()
        })).EnsureSuccessStatusCode();

        var row = (await ListAsync(admin, "action=settingChanged&entityType=SystemSettingsContracts&from=" + Uri.EscapeDataString(since))).First();
        row.GetProperty("entityId").ValueKind.ShouldBe(JsonValueKind.Null);
        Fields(row).ShouldBe(new[] { "alertDays" });
        row.GetProperty("succeeded").GetBoolean().ShouldBeTrue();
        row.GetProperty("user").GetProperty("id").GetGuid().ShouldNotBe(Guid.Empty);
        row.EnumerateObject().Select(x => x.Name).OrderBy(x => x).ShouldBe(new[] { "action", "changedFields", "entityId", "entityType", "id", "ipAddress", "occurredAt", "succeeded", "user" });
    }

    [Fact]
    public async Task Sign_in_success_and_failure_appear_with_their_type()
    {
        var (userId, userName) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee);
        await TestAuth.SignInAsync(_host, userName, TestUsers.Password);

        var rejected = false;
        try
        {
            await TestAuth.SignInAsync(_host, userName, "Wrong-Password-1a!");
        }
        catch (ShouldAssertException)
        {
            rejected = true;
        }

        rejected.ShouldBeTrue();

        var admin = await AdminAsync();
        (await ListAsync(admin, $"userId={userId}&action=login")).Count.ShouldBe(1);

        var failed = await ListAsync(admin, $"userId={userId}&action=loginFailed");
        failed.Count.ShouldBe(1);
        failed[0].GetProperty("succeeded").GetBoolean().ShouldBeFalse();
    }

[Fact]
    public async Task Two_step_sign_in_counts_once_after_the_code()
    {
        var (userId, userName) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee, HrRoles.SystemAdmin);
        var client = TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, userName, TestUsers.Password)).AccessToken, "en");
        var enrollment = await (await client.PostAsync("/api/app/account/mfa/enroll", null)).Content.ReadFromJsonAsync<JsonElement>();
        var secret = enrollment.GetProperty("sharedKey").GetString()!;
        (await client.PostAsJsonAsync("/api/app/account/mfa/confirm", new { code = Totp.Compute(secret) })).EnsureSuccessStatusCode();

        await TestAuth.SignInAsync(_host, userName, TestUsers.Password, secret);

        (await ListAsync(client, $"userId={userId}&action=login")).Count.ShouldBe(2);
        (await ListAsync(client, $"userId={userId}&action=loginFailed")).ShouldBeEmpty();
    }

    [Fact]
    public async Task Tracked_entity_writes_give_rows_with_changed_fields()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var created = await kit.Hr.PostAsJsonAsync("/api/app/department", new { nameAr = "قسم تدقيق", nameEn = "Audit Dept" });
        created.EnsureSuccessStatusCode();
        var department = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = department.GetProperty("id").GetString()!;

        (await kit.Hr.PutAsJsonAsync($"/api/app/department/{id}", new
        {
            nameAr = "قسم تدقيق",
            nameEn = "Audit Dept 2",
            concurrencyStamp = department.GetProperty("concurrencyStamp").GetString()
        })).EnsureSuccessStatusCode();

        var admin = await AdminAsync();
        var rows = await ListAsync(admin, $"entityType=Department&entityId={id}&sorting=occurredAt");
        rows.Select(x => x.GetProperty("action").GetString()).ShouldBe(new[] { "create", "update" });
        Fields(rows[1]).ShouldBe(new[] { "nameEn" });
        rows[1].GetRawText().ShouldNotContain("Audit Dept 2");
    }

    [Fact]
    public async Task Failed_write_is_recorded_as_failed_with_no_fields()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var missing = Guid.NewGuid();
        (await kit.Hr.PutAsJsonAsync($"/api/app/department/{missing}", new
        {
            nameAr = "x",
            nameEn = "x",
            concurrencyStamp = "stale"
        })).IsSuccessStatusCode.ShouldBeFalse();

        var admin = await AdminAsync();
        var row = (await ListAsync(admin, $"entityId={missing}&succeeded=false")).Single();
        row.GetProperty("action").GetString().ShouldBe("update");
        row.GetProperty("entityType").GetString().ShouldBe("Department");
        Fields(row).ShouldBeEmpty();
    }

    [Fact]
    public async Task Readers_are_limited_to_the_contract_roles_and_sorting_is_checked()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        (await kit.Hr.GetAsync("/api/app/audit-log")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var admin = await AdminAsync();
        (await admin.GetAsync("/api/app/audit-log?sorting=ipAddress")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Second_sign_in_on_a_temporary_password_raises_an_extra_alert()
    {
        var (_, once) = await TestUsers.CreateAsync(_host, true, HrRoles.Employee);
        var (_, twice) = await TestUsers.CreateAsync(_host, true, HrRoles.Employee);

        await TestAuth.SignInAsync(_host, twice, TestUsers.Password);

        foreach (var (name, expected) in new[] { (once, 1), (twice, 2) })
        {
            var client = TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, name, TestUsers.Password)).AccessToken, "en");
            (await client.PostAsJsonAsync("/api/app/account/change-password", new
            {
                currentPassword = TestUsers.Password,
                newPassword = "Changed-Test-2b!"
            })).EnsureSuccessStatusCode();

            var page = await client.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=50");
            page.GetProperty("items").EnumerateArray()
                .Count(x => x.GetProperty("templateCode").GetString() == "accountSecurityChanged")
                .ShouldBe(expected);
        }
    }

    private static AuditLogInfo Log(string verb, string url, Guid? user = null, int? status = 200, params EntityChangeInfo[] changes)
    {
        var log = new AuditLogInfo { HttpMethod = verb, Url = url, UserId = user, HttpStatusCode = status };
        log.EntityChanges.AddRange(changes);
        return log;
    }

    [Theory]
    [InlineData("PUT", "/api/app/role/8f6d2f6e-2b84-4d19-8f60-3e7a1c9d5b42/permission", AuditActionType.PermissionChanged)]
    [InlineData("PUT", "/api/app/role/8f6d2f6e-2b84-4d19-8f60-3e7a1c9d5b42", AuditActionType.RoleChanged)]
    [InlineData("POST", "/api/app/role", AuditActionType.RoleChanged)]
    [InlineData("PUT", "/api/app/user-account/8f6d2f6e-2b84-4d19-8f60-3e7a1c9d5b42/roles", AuditActionType.RoleChanged)]
    [InlineData("PUT", "/api/app/user-account/8f6d2f6e-2b84-4d19-8f60-3e7a1c9d5b42/permission", AuditActionType.PermissionChanged)]
    [InlineData("POST", "/api/app/user-account/8f6d2f6e-2b84-4d19-8f60-3e7a1c9d5b42/reset-password", AuditActionType.PasswordReset)]
    [InlineData("POST", "/api/app/complaint-handler", AuditActionType.ComplaintHandlerAssignment)]
    [InlineData("POST", "/api/app/complaint-handler/8f6d2f6e-2b84-4d19-8f60-3e7a1c9d5b42/approve", AuditActionType.ComplaintHandlerAssignment)]
    [InlineData("PUT", "/api/app/attendance-settings", AuditActionType.SettingChanged)]
    [InlineData("PUT", "/api/app/weekly-off-setting", AuditActionType.SettingChanged)]
    public void Known_routes_map_to_their_type(string verb, string url, AuditActionType expected)
    {
        var draft = HrAuditEntryBuilder.Build(Log(verb, url, Guid.NewGuid())).Single();
        draft.Action.ShouldBe(expected);
    }

    [Fact]
    public void Reads_and_ignored_writes_give_no_rows_and_complaint_entities_never_appear()
    {
        var user = Guid.NewGuid();
        HrAuditEntryBuilder.Build(Log("GET", "/api/app/department", user)).ShouldBeEmpty();
        HrAuditEntryBuilder.Build(Log("POST", "/api/app/notification/8f6d2f6e-2b84-4d19-8f60-3e7a1c9d5b42/read", user)).ShouldBeEmpty();
        HrAuditEntryBuilder.Build(Log("POST", "/connect/token")).ShouldBeEmpty();

        var complaintChange = new EntityChangeInfo
        {
            EntityTypeFullName = "HR_Mangment2.Complaints.Complaint",
            EntityId = "1",
            ChangeType = EntityChangeType.Updated,
            PropertyChanges = new List<EntityPropertyChangeInfo> { new() { PropertyName = "EncryptedContent" } }
        };

        var rows = HrAuditEntryBuilder.Build(Log("PUT", "/api/app/employee/8f6d2f6e-2b84-4d19-8f60-3e7a1c9d5b42/manager", user, 200, complaintChange));
        rows.Single().EntityType.ShouldBe("Employee");
        rows.Single().ChangedFields.ShouldBeEmpty();
    }

    [Fact]
    public void Bookkeeping_fields_are_hidden_and_names_are_camel_case()
    {
        var change = new EntityChangeInfo
        {
            EntityTypeFullName = "HR_Mangment2.Departments.Department",
            EntityId = "9",
            ChangeType = EntityChangeType.Updated,
            PropertyChanges = new List<EntityPropertyChangeInfo>
            {
                new() { PropertyName = "NameAr" },
                new() { PropertyName = "ConcurrencyStamp" },
                new() { PropertyName = "LastModificationTime" }
            }
        };

        HrAuditEntryBuilder.Build(Log("PUT", "/api/app/department/9", Guid.NewGuid(), 200, change)).Single()
            .ChangedFields.ShouldBe(new[] { "nameAr" });
    }

    [Theory]
    [InlineData("LoginSucceeded", AuditActionType.Login)]
    [InlineData("LoginFailed", AuditActionType.LoginFailed)]
    [InlineData("LoginLockedout", AuditActionType.LoginFailed)]
    [InlineData("LoginNotAllowed", AuditActionType.LoginFailed)]
    [InlineData("LoginRequiresTwoFactor", null)]
    [InlineData("Logout", null)]
    public void Security_log_actions_map_to_contract_types(string action, AuditActionType? expected)
    {
        HrSecurityLogStore.MapAction(action).ShouldBe(expected);
    }
}
