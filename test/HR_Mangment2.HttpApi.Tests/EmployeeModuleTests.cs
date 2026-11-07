using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HR_Mangment2.Errors;
using HR_Mangment2.Identity;
using HR_Mangment2.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Account;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Xunit;

namespace HR_Mangment2;

[Collection(HostCollection.Name)]
public class EmployeeModuleTests
{
    private readonly HostFixture _host;

    public EmployeeModuleTests(HostFixture host)
    {
        _host = host;
    }

    private static async Task<JsonElement> ReadAsync(HttpResponseMessage response) => await response.Content.ReadFromJsonAsync<JsonElement>();

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response) =>
        (await ReadAsync(response)).GetProperty("error").GetProperty("code").GetString();

    private static async Task ShouldFailWithAsync(HttpResponseMessage response, string code)
    {
        response.StatusCode.ShouldBe((HttpStatusCode)422, await response.Content.ReadAsStringAsync());
        (await ErrorCodeAsync(response)).ShouldBe(code);
    }

    private static string[] PropertyNames(JsonElement element) => element.EnumerateObject().Select(x => x.Name).ToArray();

    private static MultipartFormDataContent Upload(byte[] content, string contentType, string sensitivity = "normal", string fileName = "file.pdf")
    {
        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return new MultipartFormDataContent
        {
            { file, "file", fileName },
            { new StringContent("certificate"), "type" },
            { new StringContent("Title"), "title" },
            { new StringContent(sensitivity), "sensitivity" }
        };
    }

    private static readonly byte[] Pdf = Encoding.ASCII.GetBytes("%PDF-1.7 test");

    [Fact]
    public async Task Create_returns_file_account_and_one_time_password_with_forced_change()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();

        var response = await kit.Hr.PostAsJsonAsync("/api/app/employee", kit.Body(manager.Id, new[] { HrRoles.StoreOfficer }, "s" + Guid.NewGuid().ToString("N")[..8] + "@company.test"));
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.CacheControl!.NoStore.ShouldBeTrue();

        var created = await ReadAsync(response);
        var employee = created.GetProperty("employee");
        var credentials = created.GetProperty("credentials");
        var password = credentials.GetProperty("temporaryPassword").GetString()!;
        password.Length.ShouldBeGreaterThanOrEqualTo(8);
        credentials.GetProperty("mustChangePassword").GetBoolean().ShouldBeTrue();
        employee.GetProperty("manager").GetProperty("id").GetGuid().ShouldBe(manager.Id);
        employee.GetProperty("currentShift").GetProperty("effectiveFrom").GetString().ShouldBe("2025-01-15");
        employee.GetProperty("status").GetString().ShouldBe("active");
        var account = employee.GetProperty("account");
        account.GetProperty("mustChangePassword").GetBoolean().ShouldBeTrue();
        account.GetProperty("roleNames").EnumerateArray().Select(x => x.GetString()).ShouldBe(new[] { HrRoles.Employee, HrRoles.StoreOfficer }, ignoreOrder: true);

        var token = await TestAuth.SignInAsync(_host, credentials.GetProperty("userName").GetString()!, password);
        var client = TestAuth.ClientFor(_host, token.AccessToken, "en");
        var blocked = await client.GetAsync("/api/app/me/employee-profile");
        blocked.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await ErrorCodeAsync(blocked)).ShouldBe(HrErrorCodes.Identity.PasswordChangeRequired);
    }

    [Fact]
    public async Task Create_enforces_the_contract_rules()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var regular = new[] { HrRoles.Employee };

        await ShouldFailWithAsync(await kit.Hr.PostAsJsonAsync("/api/app/employee", kit.Body(null, regular)), HrErrorCodes.Employee.ManagerRequired);
        await ShouldFailWithAsync(await kit.Hr.PostAsJsonAsync("/api/app/employee", kit.Body(staff.Id, regular)), HrErrorCodes.Employee.InvalidManagerForRole);
        await ShouldFailWithAsync(await kit.Hr.PostAsJsonAsync("/api/app/employee", kit.Body(Guid.NewGuid(), regular)), HrErrorCodes.Employee.ManagerNotFound);
        await ShouldFailWithAsync(await kit.Hr.PostAsJsonAsync("/api/app/employee", kit.Body(manager.Id, regular, shiftId: Guid.NewGuid())), HrErrorCodes.Employee.WorkShiftNotFound);
        await ShouldFailWithAsync(await kit.Hr.PostAsJsonAsync("/api/app/employee", kit.Body(manager.Id, new[] { "noSuchRole" })), HrErrorCodes.Identity.RoleNotFound);
        await ShouldFailWithAsync(
            await kit.Hr.PostAsJsonAsync("/api/app/employee", kit.Body(kit.SystemAdminEmployeeId, new[] { HrRoles.DepartmentManager, HrRoles.Accountant })),
            HrErrorCodes.Identity.ForbiddenRoleCombination);
        await ShouldFailWithAsync(
            await kit.Hr.PostAsJsonAsync("/api/app/employee", kit.Body(kit.ExecutiveEmployeeId, new[] { HrRoles.HrManager })),
            HrErrorCodes.Identity.ProtectedRoleChangeRequiresApproval);
        await ShouldFailWithAsync(
            await kit.Hr.PostAsJsonAsync("/api/app/employee", kit.Body(kit.SystemAdminEmployeeId, new[] { HrRoles.ExecutiveDirector })),
            HrErrorCodes.Employee.ExecutiveCannotHaveManager);

        // A department manager must report to the system admin, and an accountant to a department manager.
        await ShouldFailWithAsync(
            await kit.Hr.PostAsJsonAsync("/api/app/employee", kit.Body(manager.Id, new[] { HrRoles.DepartmentManager })),
            HrErrorCodes.Employee.InvalidManagerForRole);

        var duplicate = kit.Body(manager.Id, regular, userName: staff.UserName);
        (await kit.Hr.PostAsJsonAsync("/api/app/employee", duplicate)).StatusCode.ShouldBe(HttpStatusCode.Conflict);

        (await kit.Hr.PostAsJsonAsync("/api/app/employee", new { fullNameAr = "س" })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task List_get_and_lookup_keep_each_audience_to_its_own_fields()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);

        var list = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/employee?managerEmployeeId={manager.Id}");
        list.GetProperty("totalCount").GetInt64().ShouldBe(1);
        var item = list.GetProperty("items")[0];
        item.GetProperty("id").GetGuid().ShouldBe(staff.Id);
        item.GetProperty("hasAccount").GetBoolean().ShouldBeTrue();

        var byRole = await kit.Hr.GetFromJsonAsync<JsonElement>("/api/app/employee?roleName=departmentManager&maxResultCount=200");
        byRole.GetProperty("items").EnumerateArray().Select(x => x.GetProperty("id").GetGuid()).ShouldContain(manager.Id);

        (await kit.Hr.GetAsync("/api/app/employee?sorting=concurrencyStamp")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await kit.Hr.GetAsync($"/api/app/employee/{Guid.NewGuid()}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var store = TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, (await TestUsers.CreateAsync(_host, false, HrRoles.StoreOfficer)).UserName, TestUsers.Password)).AccessToken, "en");
        var storeLookup = await store.GetFromJsonAsync<JsonElement>($"/api/app/employee/lookup?search={Uri.EscapeDataString("Staff")}&maxResultCount=200");
        PropertyNames(storeLookup.GetProperty("items")[0]).ShouldBe(new[] { "id", "fullName", "departmentName" }, ignoreOrder: true);
        (await store.GetAsync($"/api/app/employee/{staff.Id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var reviewer = TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, (await TestUsers.CreateAsync(_host, false, HrRoles.Reviewer)).UserName, TestUsers.Password)).AccessToken, "en");
        var reviewerIds = (await reviewer.GetFromJsonAsync<JsonElement>("/api/app/employee/lookup?maxResultCount=200"))
            .GetProperty("items").EnumerateArray().Select(x => x.GetProperty("id").GetGuid()).ToList();
        reviewerIds.ShouldContain(manager.Id);
        reviewerIds.ShouldNotContain(staff.Id);
    }

    [Fact]
    public async Task Update_changes_the_account_with_status_and_checks_concurrency_and_reports()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var staffClient = await kit.SignInAsync(staff);

        var staffEmail = EmployeeTestKit.UniqueEmail();

        object Update(string stamp, string status, Guid? department = null) => new
        {
            fullNameAr = "اسم جديد",
            fullNameEn = "New Name",
            email = staffEmail,
            departmentId = department ?? kit.DepartmentId,
            jobTitle = "Lead",
            hireDate = "2025-01-15",
            status,
            concurrencyStamp = stamp
        };

        var staleUpdate = await kit.Hr.PutAsJsonAsync($"/api/app/employee/{staff.Id}", Update("stale", "active"));
        staleUpdate.StatusCode.ShouldBe(HttpStatusCode.Conflict, await staleUpdate.Content.ReadAsStringAsync());

        var managerFile = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/employee/{manager.Id}");
        managerFile.GetProperty("directReportCount").GetInt32().ShouldBe(1);
        await ShouldFailWithAsync(
            await kit.Hr.PutAsJsonAsync($"/api/app/employee/{manager.Id}", Update(managerFile.GetProperty("concurrencyStamp").GetString()!, "suspended")),
            HrErrorCodes.Employee.HasDirectReports);

        var current = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/employee/{staff.Id}");
        var stamp = current.GetProperty("concurrencyStamp").GetString()!;
        await ShouldFailWithAsync(
            await kit.Hr.PutAsJsonAsync($"/api/app/employee/{staff.Id}", Update(stamp, "active", Guid.NewGuid())),
            HrErrorCodes.Department.NotFound);

        var suspended = await kit.Hr.PutAsJsonAsync($"/api/app/employee/{staff.Id}", Update(stamp, "suspended"));
        suspended.StatusCode.ShouldBe(HttpStatusCode.OK, await suspended.Content.ReadAsStringAsync());
        var body = await ReadAsync(suspended);
        body.GetProperty("account").GetProperty("isActive").GetBoolean().ShouldBeFalse();
        body.GetProperty("fullName").GetString().ShouldBe("New Name");
        (await staffClient.GetAsync("/api/app/me/employee-profile")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var back = await kit.Hr.PutAsJsonAsync($"/api/app/employee/{staff.Id}", Update(body.GetProperty("concurrencyStamp").GetString()!, "active"));
        (await ReadAsync(back)).GetProperty("account").GetProperty("isActive").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public async Task Change_manager_writes_history_and_notifies_and_rejects_invalid_links()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var first = await kit.AddDepartmentManagerAsync();
        var second = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(first.Id);
        var other = await kit.AddStaffAsync(first.Id);
        var url = $"/api/app/employee/{staff.Id}/manager";
        var stamp = (await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/employee/{staff.Id}")).GetProperty("concurrencyStamp").GetString();

        await ShouldFailWithAsync(await kit.Hr.PutAsJsonAsync(url, new { managerEmployeeId = first.Id, concurrencyStamp = stamp }), HrErrorCodes.Employee.ManagerUnchanged);
        await ShouldFailWithAsync(await kit.Hr.PutAsJsonAsync(url, new { managerEmployeeId = staff.Id, concurrencyStamp = stamp }), HrErrorCodes.Employee.CannotManageSelf);
        await ShouldFailWithAsync(await kit.Hr.PutAsJsonAsync(url, new { managerEmployeeId = other.Id, concurrencyStamp = stamp }), HrErrorCodes.Employee.InvalidManagerForRole);
        await ShouldFailWithAsync(
            await kit.Hr.PutAsJsonAsync($"/api/app/employee/{kit.ExecutiveEmployeeId}/manager", new { managerEmployeeId = first.Id, concurrencyStamp = "x" }),
            HrErrorCodes.Employee.ExecutiveCannotHaveManager);
        (await kit.Hr.PutAsJsonAsync(url, new { managerEmployeeId = second.Id, concurrencyStamp = "stale" })).StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var changed = await kit.Hr.PutAsJsonAsync(url, new { managerEmployeeId = second.Id, reason = "Moved team", concurrencyStamp = stamp });
        changed.StatusCode.ShouldBe(HttpStatusCode.OK, await changed.Content.ReadAsStringAsync());
        var result = await ReadAsync(changed);
        result.GetProperty("previousManager").GetProperty("id").GetGuid().ShouldBe(first.Id);
        result.GetProperty("newManager").GetProperty("id").GetGuid().ShouldBe(second.Id);
        result.GetProperty("movedPendingItemCount").GetInt32().ShouldBe(0);

        var history = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/employee/{staff.Id}/manager-history");
        history.GetProperty("totalCount").GetInt64().ShouldBe(2);
        var latest = history.GetProperty("items")[0];
        latest.GetProperty("manager").GetProperty("id").GetGuid().ShouldBe(second.Id);
        latest.GetProperty("toDate").ValueKind.ShouldBe(JsonValueKind.Null);
        latest.GetProperty("reason").GetString().ShouldBe("Moved team");
        history.GetProperty("items")[1].GetProperty("toDate").ValueKind.ShouldBe(JsonValueKind.String);

        var staffClient = await kit.SignInAsync(staff);
        var notifications = await staffClient.GetFromJsonAsync<JsonElement>("/api/app/notification");
        notifications.GetProperty("items").EnumerateArray().Select(x => x.GetProperty("templateCode").GetString()).ShouldContain("managerChanged");
    }

    [Fact]
    public async Task Assign_employees_is_all_or_nothing_and_safe_to_repeat()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var first = await kit.AddDepartmentManagerAsync();
        var second = await kit.AddDepartmentManagerAsync();
        var one = await kit.AddStaffAsync(first.Id);
        var two = await kit.AddStaffAsync(first.Id);
        var missing = Guid.NewGuid();
        var url = $"/api/app/manager/{second.Id}/assign-employees";

        var rejected = await kit.Hr.PostAsJsonAsync(url, new { employeeIds = new[] { one.Id, missing } });
        rejected.StatusCode.ShouldBe((HttpStatusCode)422);
        var rejectedError = (await ReadAsync(rejected)).GetProperty("error");
        rejectedError.GetProperty("code").GetString().ShouldBe(HrErrorCodes.Employee.EmployeeNotFound);
        rejectedError.GetProperty("data").GetProperty("invalidEmployeeIds")[0].GetGuid().ShouldBe(missing);
        (await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/employee/{one.Id}")).GetProperty("manager").GetProperty("id").GetGuid().ShouldBe(first.Id);

        var result = await ReadAsync(await kit.Hr.PostAsJsonAsync(url, new { employeeIds = new[] { one.Id, two.Id } }));
        result.GetProperty("assignedCount").GetInt32().ShouldBe(2);
        result.GetProperty("items").GetArrayLength().ShouldBe(2);

        var again = await ReadAsync(await kit.Hr.PostAsJsonAsync(url, new { employeeIds = new[] { one.Id, two.Id } }));
        again.GetProperty("assignedCount").GetInt32().ShouldBe(0);
        again.GetProperty("alreadyAssignedCount").GetInt32().ShouldBe(2);
        (await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/employee/{one.Id}/manager-history")).GetProperty("totalCount").GetInt64().ShouldBe(2);

        (await kit.Hr.PostAsJsonAsync($"/api/app/manager/{Guid.NewGuid()}/assign-employees", new { employeeIds = new[] { one.Id } })).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task My_team_and_profile_show_only_the_fields_of_each_audience()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var outsiderManager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var outsider = await kit.AddStaffAsync(outsiderManager.Id);
        var managerClient = await kit.SignInAsync(manager);

        var team = await managerClient.GetFromJsonAsync<JsonElement>("/api/app/manager/my-team");
        team.GetProperty("totalCount").GetInt64().ShouldBe(1);
        PropertyNames(team.GetProperty("items")[0]).ShouldBe(
            new[] { "id", "fullName", "email", "phoneNumber", "department", "jobTitle", "hireDate", "status", "currentShift" },
            ignoreOrder: true);

        (await managerClient.GetAsync($"/api/app/manager/my-team/{staff.Id}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await managerClient.GetAsync($"/api/app/manager/my-team/{outsider.Id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await managerClient.GetAsync($"/api/app/employee/{staff.Id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var me = await managerClient.GetFromJsonAsync<JsonElement>("/api/app/me");
        me.GetProperty("directReportCount").GetInt32().ShouldBe(1);
        me.GetProperty("employee").GetProperty("id").GetGuid().ShouldBe(manager.Id);

        var staffClient = await kit.SignInAsync(staff);
        var profile = await staffClient.GetFromJsonAsync<JsonElement>("/api/app/me/employee-profile");
        profile.GetProperty("id").GetGuid().ShouldBe(staff.Id);
        profile.GetProperty("manager").GetProperty("id").GetGuid().ShouldBe(manager.Id);
        PropertyNames(profile).ShouldNotContain("account");
        PropertyNames(profile).ShouldNotContain("concurrencyStamp");
        (await staffClient.GetAsync("/api/app/manager/my-team")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Shift_assignment_closes_the_previous_one_and_checks_dates()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var url = $"/api/app/employee/{staff.Id}/shift";

        var newShift = await ReadAsync(await kit.Hr.PostAsJsonAsync("/api/app/work-shift", new
        {
            nameAr = "مسائي",
            nameEn = "Evening " + Guid.NewGuid().ToString("N")[..6],
            type = "fixed",
            graceMinutes = 5,
            overtimePolicy = "none",
            days = new[] { new { dayOfWeek = "sunday", startTime = "16:00", endTime = "22:00", crossesMidnight = false } }
        }));
        var shiftId = newShift.GetProperty("id").GetGuid();

        await ShouldFailWithAsync(await kit.Hr.PutAsJsonAsync(url, new { shiftId, startDate = "2024-12-31" }), HrErrorCodes.Shift.StartDateBeforeHireDate);
        await ShouldFailWithAsync(await kit.Hr.PutAsJsonAsync(url, new { shiftId, startDate = "2025-01-15" }), HrErrorCodes.Shift.StartDateBeforeCurrentAssignment);
        await ShouldFailWithAsync(
            await kit.Hr.PutAsJsonAsync($"/api/app/employee/{kit.ExecutiveEmployeeId}/shift", new { shiftId, startDate = "2030-01-01" }),
            HrErrorCodes.Attendance.ExecutiveDirectorNotCovered);
        (await kit.Hr.PutAsJsonAsync(url, new { shiftId = Guid.NewGuid(), startDate = "2025-03-01" })).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var assigned = await kit.Hr.PutAsJsonAsync(url, new { shiftId, startDate = "2025-03-01" });
        assigned.StatusCode.ShouldBe(HttpStatusCode.OK, await assigned.Content.ReadAsStringAsync());
        (await ReadAsync(assigned)).GetProperty("endDate").ValueKind.ShouldBe(JsonValueKind.Null);

        var history = await kit.Hr.GetFromJsonAsync<JsonElement>(url);
        history.GetProperty("totalCount").GetInt64().ShouldBe(2);
        history.GetProperty("items")[0].GetProperty("shift").GetProperty("id").GetGuid().ShouldBe(shiftId);
        history.GetProperty("items")[1].GetProperty("endDate").GetString().ShouldBe("2025-02-28");

        (await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/work-shift/{shiftId}")).GetProperty("assignedEmployeeCount").GetInt32().ShouldBe(1);

        var inactive = await kit.Hr.PutAsJsonAsync($"/api/app/work-shift/{shiftId}", new
        {
            nameAr = "مسائي",
            nameEn = "Evening",
            type = "fixed",
            graceMinutes = 5,
            overtimePolicy = "none",
            isActive = false,
            days = new[] { new { dayOfWeek = "sunday", startTime = "16:00", endTime = "22:00", crossesMidnight = false } },
            concurrencyStamp = newShift.GetProperty("concurrencyStamp").GetString()
        });
        inactive.EnsureSuccessStatusCode();
        await ShouldFailWithAsync(await kit.Hr.PutAsJsonAsync(url, new { shiftId, startDate = "2025-06-01" }), HrErrorCodes.Shift.ShiftInactive);
    }

    [Fact]
    public async Task Department_with_employees_cannot_be_deleted_and_counts_them()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var department = await ReadAsync(await kit.Hr.PostAsJsonAsync("/api/app/department", new { nameAr = "فرع", nameEn = "Branch" }));
        var id = department.GetProperty("id").GetGuid();

        var staff = await kit.AddStaffAsync(manager.Id);
        var file = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/employee/{staff.Id}");
        var moved = await kit.Hr.PutAsJsonAsync($"/api/app/employee/{staff.Id}", new
        {
            fullNameAr = "س",
            email = EmployeeTestKit.UniqueEmail(),
            departmentId = id,
            jobTitle = "Lead",
            hireDate = "2025-01-15",
            status = "active",
            concurrencyStamp = file.GetProperty("concurrencyStamp").GetString()
        });
        moved.EnsureSuccessStatusCode();

        (await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/department/{id}")).GetProperty("employeeCount").GetInt32().ShouldBe(1);
        await ShouldFailWithAsync(await kit.Hr.DeleteAsync($"/api/app/department/{id}"), HrErrorCodes.Department.HasEmployees);
    }

    [Fact]
    public async Task Accounts_list_get_lookup_and_create_for_a_file_without_account()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);

        var list = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/user-account?search={staff.UserName}");
        list.GetProperty("totalCount").GetInt64().ShouldBe(1);
        var account = list.GetProperty("items")[0];
        account.GetProperty("mustChangePassword").GetBoolean().ShouldBeTrue();
        account.GetProperty("email").ValueKind.ShouldBe(JsonValueKind.String);
        account.GetProperty("employee").GetProperty("id").GetGuid().ShouldBe(staff.Id);
        PropertyNames(account).ShouldNotContain("passwordHash");

        (await kit.Hr.GetFromJsonAsync<JsonElement>("/api/app/user-account?search=admin&maxResultCount=200"))
            .GetProperty("items").EnumerateArray().Select(x => x.GetProperty("userName").GetString()).ShouldNotContain(HrTenantBootstrapper.AdminUserName);

        var lookup = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/user-account/lookup?search={staff.UserName}");
        PropertyNames(lookup.GetProperty("items")[0]).ShouldBe(new[] { "id", "displayName" }, ignoreOrder: true);

        await ShouldFailWithAsync(
            await kit.Hr.PostAsJsonAsync("/api/app/user-account", new { employeeId = staff.Id, userName = "x" + Guid.NewGuid().ToString("N")[..10], roleNames = new[] { HrRoles.Employee } }),
            HrErrorCodes.Employee.EmployeeAlreadyHasAccount);

        // A reviewer reports to the executive director or the HR manager (decision 2026-11-07).
        var fileId = await kit.AddFileWithoutAccountAsync(kit.ExecutiveEmployeeId);
        var created = await kit.Hr.PostAsJsonAsync("/api/app/user-account", new { employeeId = fileId, email = EmployeeTestKit.UniqueEmail(), userName = "y" + Guid.NewGuid().ToString("N")[..10], roleNames = new[] { HrRoles.Reviewer } });
        created.StatusCode.ShouldBe(HttpStatusCode.Created, await created.Content.ReadAsStringAsync());
        var body = await ReadAsync(created);
        body.GetProperty("credentials").GetProperty("temporaryPassword").GetString().ShouldNotBeNullOrEmpty();
        body.GetProperty("account").GetProperty("roleNames").EnumerateArray().Select(x => x.GetString()).ShouldBe(new[] { HrRoles.Employee, HrRoles.Reviewer }, ignoreOrder: true);
        (await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/employee/{fileId}")).GetProperty("account").GetProperty("userName").GetString().ShouldNotBeNull();
    }

    [Fact]
    public async Task Set_roles_follows_the_protected_role_and_manager_rules()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var other = await kit.AddStaffAsync(manager.Id);
        var url = $"/api/app/user-account/{other.UserId}/roles";

        async Task<string> StampAsync(Guid userId) =>
            (await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/user-account/{userId}")).GetProperty("concurrencyStamp").GetString()!;

        (await kit.Hr.PutAsJsonAsync(url, new { roleNames = new[] { HrRoles.Reviewer }, concurrencyStamp = "stale" })).StatusCode.ShouldBe(HttpStatusCode.Conflict);

        // A department manager cannot manage a reviewer, so the new roles need the executive director or the HR manager.
        await ShouldFailWithAsync(
            await kit.Hr.PutAsJsonAsync(url, new { roleNames = new[] { HrRoles.Reviewer }, concurrencyStamp = await StampAsync(other.UserId) }),
            HrErrorCodes.Employee.ManagerRequired);
        var replaced = await kit.Hr.PutAsJsonAsync(url, new { roleNames = new[] { HrRoles.Reviewer }, managerEmployeeId = kit.ExecutiveEmployeeId, concurrencyStamp = await StampAsync(other.UserId) });
        replaced.StatusCode.ShouldBe(HttpStatusCode.OK, await replaced.Content.ReadAsStringAsync());
        (await ReadAsync(replaced)).GetProperty("roleNames").EnumerateArray().Select(x => x.GetString()).ShouldBe(new[] { HrRoles.Employee, HrRoles.Reviewer }, ignoreOrder: true);

        await ShouldFailWithAsync(
            await kit.Hr.PutAsJsonAsync(url, new { roleNames = new[] { HrRoles.ExecutiveDirector }, concurrencyStamp = await StampAsync(other.UserId) }),
            HrErrorCodes.Identity.ProtectedRoleChangeRequiresApproval);
        await ShouldFailWithAsync(
            await kit.Hr.PutAsJsonAsync(url, new { roleNames = new[] { HrRoles.HrManager }, concurrencyStamp = await StampAsync(other.UserId) }),
            HrErrorCodes.Identity.ProtectedRoleChangeRequiresApproval);

        // Granting departmentManager makes the current link invalid, so a manager has to be sent.
        await ShouldFailWithAsync(
            await kit.Hr.PutAsJsonAsync(url, new { roleNames = new[] { HrRoles.DepartmentManager }, concurrencyStamp = await StampAsync(other.UserId) }),
            HrErrorCodes.Employee.ManagerRequired);
        await ShouldFailWithAsync(
            await kit.Hr.PutAsJsonAsync(url, new { roleNames = new[] { HrRoles.DepartmentManager }, managerEmployeeId = manager.Id, concurrencyStamp = await StampAsync(other.UserId) }),
            HrErrorCodes.Employee.InvalidManagerForRole);

        var promoted = await kit.Hr.PutAsJsonAsync(url, new { roleNames = new[] { HrRoles.DepartmentManager }, managerEmployeeId = kit.SystemAdminEmployeeId, concurrencyStamp = await StampAsync(other.UserId) });
        promoted.StatusCode.ShouldBe(HttpStatusCode.OK, await promoted.Content.ReadAsStringAsync());
        (await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/employee/{other.Id}")).GetProperty("manager").GetProperty("id").GetGuid().ShouldBe(kit.SystemAdminEmployeeId);

        // The manager still has staff, so the role that lets them manage staff cannot be taken away.
        await ShouldFailWithAsync(
            await kit.Hr.PutAsJsonAsync($"/api/app/user-account/{manager.UserId}/roles", new { roleNames = new[] { HrRoles.Reviewer }, managerEmployeeId = other.Id, concurrencyStamp = await StampAsync(manager.UserId) }),
            HrErrorCodes.Identity.ManagerRoleHasDirectReports);

        await ShouldFailWithAsync(
            await kit.Hr.PutAsJsonAsync($"/api/app/user-account/{staff.UserId}/roles", new { roleNames = new[] { HrRoles.StoreOfficer, HrRoles.Accountant }, concurrencyStamp = await StampAsync(staff.UserId) }),
            HrErrorCodes.Identity.ForbiddenRoleCombination);

        var executive = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.ExecutiveDirector, HrRoles.HrManager);
        await ShouldFailWithAsync(
            await executive.PutAsJsonAsync($"/api/app/user-account/{staff.UserId}/roles", new { roleNames = new[] { HrRoles.HrManager }, managerEmployeeId = kit.ExecutiveEmployeeId, concurrencyStamp = await StampAsync(staff.UserId) }),
            HrErrorCodes.Identity.SingletonRoleAlreadyAssigned);
    }

    [Fact]
    public async Task New_roles_apply_to_the_next_request_of_a_signed_in_user()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var staffClient = await kit.SignInAsync(staff);

        (await staffClient.GetAsync("/api/app/employee/lookup")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var stamp = (await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/user-account/{staff.UserId}")).GetProperty("concurrencyStamp").GetString();
        (await kit.Hr.PutAsJsonAsync($"/api/app/user-account/{staff.UserId}/roles", new { roleNames = new[] { HrRoles.StoreOfficer }, concurrencyStamp = stamp })).EnsureSuccessStatusCode();

        (await staffClient.GetAsync("/api/app/employee/lookup")).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Own_roles_and_own_password_cannot_be_changed_here()
    {
        var (actorId, actorName) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee, HrRoles.HrManager, HrRoles.ExecutiveDirector);
        var token = await TestAuth.SignInAsync(_host, actorName, TestUsers.Password);
        var client = TestAuth.ClientFor(_host, token.AccessToken, "en");
        var enroll = await ReadAsync(await client.PostAsync("/api/app/account/mfa/enroll", null));
        (await client.PostAsJsonAsync("/api/app/account/mfa/confirm", new { code = Totp.Compute(enroll.GetProperty("sharedKey").GetString()!) })).EnsureSuccessStatusCode();

        await ShouldFailWithAsync(
            await client.PutAsJsonAsync($"/api/app/user-account/{actorId}/roles", new { roleNames = new[] { HrRoles.Employee }, concurrencyStamp = "x" }),
            HrErrorCodes.Identity.CannotChangeOwnRoles);
        await ShouldFailWithAsync(
            await client.PostAsJsonAsync($"/api/app/user-account/{actorId}/reset-password", new { reason = "x" }),
            HrErrorCodes.Identity.CannotResetOwnPassword);
    }

    [Fact]
    public async Task Password_reset_mails_a_one_time_link_and_never_shows_a_password()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var email = "m" + Guid.NewGuid().ToString("N")[..10] + "@company.test";
        var staff = await kit.AddStaffAsync(manager.Id, email);
        var noEmail = await kit.AddStaffAsync(manager.Id);
        await kit.MakeOldAccountWithoutEmailAsync(noEmail.UserId);

        await ShouldFailWithAsync(
            await kit.Hr.PostAsJsonAsync($"/api/app/user-account/{noEmail.UserId}/reset-password", new { reason = "Lost" }),
            HrErrorCodes.Identity.NoVerifiedEmail);

        var hrAccount = (await kit.Hr.GetFromJsonAsync<JsonElement>("/api/app/user-account?roleName=executiveDirector")).GetProperty("items")[0].GetProperty("id").GetGuid();
        await ShouldFailWithAsync(
            await kit.Hr.PostAsJsonAsync($"/api/app/user-account/{hrAccount}/reset-password", new { reason = "x" }),
            HrErrorCodes.Identity.PasswordResetNotAllowedForTarget);

        var response = await kit.Hr.PostAsJsonAsync($"/api/app/user-account/{staff.UserId}/reset-password", new { reason = "Forgot it" });
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        var result = await ReadAsync(response);
        result.GetProperty("deliveredVia").GetString().ShouldBe("email");
        PropertyNames(result).ShouldBe(new[] { "userId", "deliveredVia", "expiresAt" }, ignoreOrder: true);

        var body = CapturingEmailSender.LastBodyTo(email);
        body.ShouldNotBeNull();
        body.ShouldContain("/Account/ResetPassword?userId=" + staff.UserId);

        // The address cannot change while the link that was sent to it still works.
        var file = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/employee/{staff.Id}");
        (await kit.Hr.PutAsJsonAsync($"/api/app/employee/{staff.Id}", new
        {
            fullNameAr = "س",
            email = "other" + email,
            departmentId = kit.DepartmentId,
            jobTitle = "Lead",
            hireDate = "2025-01-15",
            status = "active",
            concurrencyStamp = file.GetProperty("concurrencyStamp").GetString()
        })).StatusCode.ShouldBe(HttpStatusCode.Conflict);

        // The link in the mail opens the server's reset page.
        var linkStart = body.IndexOf("/Account/ResetPassword", StringComparison.Ordinal);
        var page = await _host.CreateClient().GetAsync(body.Substring(linkStart).Trim());
        page.StatusCode.ShouldBe(HttpStatusCode.OK);

        var encodedToken = body.Substring(body.IndexOf("resetToken=", StringComparison.Ordinal) + "resetToken=".Length).Trim();
        using (var scope = _host.Factory.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var tenant = await services.GetRequiredService<ITenantRepository>().FindByNameAsync(
                services.GetRequiredService<ITenantNormalizer>().NormalizeName(HostFixture.TestTenantName)!);
            using (services.GetRequiredService<ICurrentTenant>().Change(tenant!.Id))
            {
                await services.GetRequiredService<IAccountAppService>().ResetPasswordAsync(new ResetPasswordDto
                {
                    UserId = staff.UserId,
                    ResetToken = Uri.UnescapeDataString(encodedToken),
                    Password = "Reset-Test-9z!"
                });
            }
        }

        var account = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/user-account/{staff.UserId}");
        account.GetProperty("mustChangePassword").GetBoolean().ShouldBeFalse();

        var token = await TestAuth.SignInAsync(_host, staff.UserName, "Reset-Test-9z!");
        (await TestAuth.ClientFor(_host, token.AccessToken, "en").GetAsync("/api/app/me/employee-profile")).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task User_permissions_hide_protected_names_and_reject_financial_grants()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var admin = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.SystemAdmin);
        var url = $"/api/app/user-account/{manager.UserId}/permission";

        var read = await admin.GetFromJsonAsync<JsonElement>(url);
        var names = read.GetProperty("groups").EnumerateArray().SelectMany(g => g.GetProperty("permissions").EnumerateArray()).ToList();
        names.Select(x => x.GetProperty("name").GetString()!).ShouldNotContain(x => x.StartsWith("Hr.Complaint", StringComparison.Ordinal));
        names.Single(x => x.GetProperty("name").GetString() == "Hr.Employee.ReadTeam").GetProperty("grantedByRoleNames").EnumerateArray().Select(x => x.GetString()).ShouldContain(HrRoles.DepartmentManager);

        await ShouldFailWithAsync(
            await admin.PutAsJsonAsync(url, new { permissions = new[] { new { name = "Hr.Finance.AccountantRequest.Read", isGranted = true } } }),
            HrErrorCodes.Identity.FinancialPermissionNotGrantable);
        await ShouldFailWithAsync(
            await admin.PutAsJsonAsync(url, new { permissions = new[] { new { name = "Hr.NoSuch", isGranted = true } } }),
            HrErrorCodes.Identity.PermissionNotFound);

        var updated = await admin.PutAsJsonAsync(url, new { permissions = new[] { new { name = "Hr.Employee.Lookup", isGranted = true } } });
        updated.StatusCode.ShouldBe(HttpStatusCode.OK, await updated.Content.ReadAsStringAsync());
        (await ReadAsync(updated)).GetProperty("groups").EnumerateArray().SelectMany(g => g.GetProperty("permissions").EnumerateArray())
            .Single(x => x.GetProperty("name").GetString() == "Hr.Employee.Lookup").GetProperty("isDirectlyGranted").GetBoolean().ShouldBeTrue();

        (await kit.Hr.GetAsync(url)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Documents_follow_the_file_privacy_rules()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var otherManager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var colleague = await kit.AddStaffAsync(manager.Id);
        var url = $"/api/app/employee/{staff.Id}/documents";

        var normal = await kit.Hr.PostAsync(url, Upload(Pdf, "application/pdf"));
        normal.StatusCode.ShouldBe(HttpStatusCode.Created, await normal.Content.ReadAsStringAsync());
        var normalDoc = await ReadAsync(normal);
        normalDoc.GetProperty("scanStatus").GetString().ShouldBe("pending");
        var normalId = normalDoc.GetProperty("id").GetGuid();

        var sensitiveId = (await ReadAsync(await kit.Hr.PostAsync(url, Upload(Pdf, "application/pdf", "sensitive")))).GetProperty("id").GetGuid();

        var hrList = await kit.Hr.GetFromJsonAsync<JsonElement>(url);
        hrList.GetProperty("totalCount").GetInt64().ShouldBe(2);
        hrList.GetProperty("items").EnumerateArray().Select(x => x.GetProperty("scanStatus").GetString()).ShouldAllBe(x => x == "clean");

        // Every access to a sensitive document is recorded. Normal documents are not.
        (await kit.Hr.GetAsync($"/api/app/document/{sensitiveId}/download-url")).EnsureSuccessStatusCode();
        (await kit.Hr.GetAsync($"/api/app/document/{normalId}/download-url")).EnsureSuccessStatusCode();
        (await kit.CountDocumentAccessAsync(sensitiveId)).ShouldBe(1);
        (await kit.CountDocumentAccessAsync(normalId)).ShouldBe(0);

        var staffClient = await kit.SignInAsync(staff);
        (await staffClient.GetFromJsonAsync<JsonElement>(url)).GetProperty("totalCount").GetInt64().ShouldBe(2);

        var managerClient = await kit.SignInAsync(manager);
        var managerList = await managerClient.GetFromJsonAsync<JsonElement>(url);
        managerList.GetProperty("totalCount").GetInt64().ShouldBe(1);
        managerList.GetProperty("items")[0].GetProperty("id").GetGuid().ShouldBe(normalId);
        (await managerClient.GetAsync($"/api/app/document/{sensitiveId}/download-url")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await managerClient.GetAsync($"/api/app/document/{normalId}/download-url")).StatusCode.ShouldBe(HttpStatusCode.OK);

        foreach (var outsider in new[] { await kit.SignInAsync(colleague), await kit.SignInAsync(otherManager) })
        {
            (await outsider.GetAsync(url)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
            (await outsider.GetAsync($"/api/app/document/{normalId}/download-url")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        // The system admin has no way in except as a direct manager.
        var admin = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.SystemAdmin);
        (await admin.GetAsync(url)).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var store = TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, (await TestUsers.CreateAsync(_host, false, HrRoles.StoreOfficer)).UserName, TestUsers.Password)).AccessToken, "en");
        (await store.GetAsync(url)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await staffClient.PostAsync(url, Upload(Pdf, "application/pdf"))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Upload_checks_size_type_signature_and_scan_and_download_link_works_once_deleted_it_stops()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var url = $"/api/app/employee/{staff.Id}/documents";

        await ShouldFailWithAsync(await kit.Hr.PostAsync(url, Upload(Pdf, "application/x-msdownload")), HrErrorCodes.Document.FileTypeNotAllowed);
        await ShouldFailWithAsync(await kit.Hr.PostAsync(url, Upload(Encoding.ASCII.GetBytes("MZ not a pdf"), "application/pdf")), HrErrorCodes.Document.FileTypeNotAllowed);

        var tooBig = new byte[10_485_761];
        Pdf.CopyTo(tooBig, 0);
        await ShouldFailWithAsync(await kit.Hr.PostAsync(url, Upload(tooBig, "application/pdf")), HrErrorCodes.Document.FileTooLarge);

        (await kit.Hr.PostAsync(url, new MultipartFormDataContent { { new StringContent("x"), "title" } })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var infected = Encoding.ASCII.GetBytes("%PDF- EICAR-STANDARD-ANTIVIRUS-TEST-FILE");
        var infectedId = (await ReadAsync(await kit.Hr.PostAsync(url, Upload(infected, "application/pdf")))).GetProperty("id").GetGuid();
        await ShouldFailWithAsync(await kit.Hr.GetAsync($"/api/app/document/{infectedId}/download-url"), HrErrorCodes.Document.NotAvailable);

        var id = (await ReadAsync(await kit.Hr.PostAsync(url, Upload(Pdf, "application/pdf", fileName: "..\\..\\evil.pdf")))).GetProperty("id").GetGuid();
        var link = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/document/{id}/download-url");
        var path = new Uri(link.GetProperty("url").GetString()!).AbsolutePath;

        var anonymous = _host.CreateClient();
        var download = await anonymous.GetAsync(path);
        download.StatusCode.ShouldBe(HttpStatusCode.OK, await download.Content.ReadAsStringAsync());
        (await download.Content.ReadAsByteArrayAsync()).ShouldBe(Pdf);
        download.Content.Headers.ContentDisposition!.DispositionType.ShouldBe("attachment");
        download.Headers.GetValues("X-Content-Type-Options").ShouldContain("nosniff");

        (await anonymous.GetAsync("/files/" + new string('a', 64))).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        (await kit.Hr.DeleteAsync($"/api/app/document/{id}")).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await anonymous.GetAsync(path)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await kit.Hr.GetAsync($"/api/app/document/{id}/download-url")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await kit.Hr.DeleteAsync($"/api/app/document/{id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delegation_lifecycle_rules_and_task_log()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var delegate1 = await kit.AddStaffAsync(manager.Id);
        var managerClient = await kit.SignInAsync(manager);
        var delegateClient = await kit.SignInAsync(delegate1);

        var start = DateTime.UtcNow.AddHours(1);
        object Body(Guid to, string scope, DateTime from, DateTime until) => new
        {
            toUserId = to,
            scope,
            startsAt = from.ToString("O"),
            endsAt = until.ToString("O"),
            reason = "Leave"
        };

        await ShouldFailWithAsync(await managerClient.PostAsJsonAsync("/api/app/delegation", Body(manager.UserId, "requests", start, start.AddDays(1))), HrErrorCodes.Delegation.CannotDelegateToSelf);
        await ShouldFailWithAsync(await managerClient.PostAsJsonAsync("/api/app/delegation", Body(delegate1.UserId, "requests", start, start)), HrErrorCodes.Delegation.InvalidPeriod);
        await ShouldFailWithAsync(await managerClient.PostAsJsonAsync("/api/app/delegation", Body(delegate1.UserId, "requests", start.AddDays(-1), start)), HrErrorCodes.Delegation.InvalidPeriod);
        await ShouldFailWithAsync(await managerClient.PostAsJsonAsync("/api/app/delegation", Body(Guid.NewGuid(), "requests", start, start.AddDays(1))), HrErrorCodes.Delegation.DelegateNotEligible);

        var created = await managerClient.PostAsJsonAsync("/api/app/delegation", Body(delegate1.UserId, "requests", start, start.AddDays(1)));
        created.StatusCode.ShouldBe(HttpStatusCode.Created, await created.Content.ReadAsStringAsync());
        var delegation = await ReadAsync(created);
        var id = delegation.GetProperty("id").GetGuid();
        delegation.GetProperty("status").GetString().ShouldBe("scheduled");
        delegation.GetProperty("toUser").GetProperty("id").GetGuid().ShouldBe(delegate1.UserId);

        await ShouldFailWithAsync(await managerClient.PostAsJsonAsync("/api/app/delegation", Body(delegate1.UserId, "allApprovals", start, start.AddDays(1))), HrErrorCodes.Delegation.OverlappingDelegation);
        (await managerClient.PostAsJsonAsync("/api/app/delegation", Body(delegate1.UserId, "memosAndIncidents", start, start.AddDays(1)))).StatusCode.ShouldBe(HttpStatusCode.Created);

        var received = await delegateClient.GetFromJsonAsync<JsonElement>("/api/app/delegation/my?direction=received&status=scheduled");
        received.GetProperty("totalCount").GetInt64().ShouldBe(2);
        (await delegateClient.GetFromJsonAsync<JsonElement>("/api/app/notification")).GetProperty("items").EnumerateArray()
            .Select(x => x.GetProperty("templateCode").GetString()).ShouldContain("delegationUpdated");
        (await delegateClient.GetAsync("/api/app/delegation/candidate")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await delegateClient.GetAsync("/api/app/delegation")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var candidates = await managerClient.GetFromJsonAsync<JsonElement>("/api/app/delegation/candidate?maxResultCount=200");

        // Every page, because the shared test database can hold more people than one page, depending on the test order.
        var candidateIds = new System.Collections.Generic.List<Guid>();
        for (var skip = 0; skip < candidates.GetProperty("totalCount").GetInt64(); skip += 200)
        {
            var page = await managerClient.GetFromJsonAsync<JsonElement>($"/api/app/delegation/candidate?maxResultCount=200&skipCount={skip}");
            candidateIds.AddRange(page.GetProperty("items").EnumerateArray().Select(x => x.GetProperty("id").GetGuid()));
        }

        candidateIds.ShouldContain(delegate1.UserId);
        candidateIds.ShouldNotContain(manager.UserId);
        PropertyNames(candidates.GetProperty("items")[0]).ShouldBe(new[] { "id", "displayName" }, ignoreOrder: true);

        var hrList = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/delegation?fromUserId={manager.UserId}");
        hrList.GetProperty("totalCount").GetInt64().ShouldBe(2);

        var log = await managerClient.GetFromJsonAsync<JsonElement>("/api/app/me/administrative-task-log?taskType=delegationCreated");
        log.GetProperty("totalCount").GetInt64().ShouldBe(2);
        log.GetProperty("items")[0].GetProperty("title").GetString().ShouldBe("Delegation created");
        (await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/employee/{manager.Id}/administrative-task-log")).GetProperty("totalCount").GetInt64().ShouldBe(2);

        var otherManager = await kit.SignInAsync(await kit.AddDepartmentManagerAsync());
        (await otherManager.DeleteAsync($"/api/app/delegation/{id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        (await managerClient.DeleteAsync($"/api/app/delegation/{id}")).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await ShouldFailWithAsync(await managerClient.DeleteAsync($"/api/app/delegation/{id}"), HrErrorCodes.Delegation.AlreadyEnded);
        var cancelled = await managerClient.GetFromJsonAsync<JsonElement>("/api/app/delegation/my?status=cancelled");
        cancelled.GetProperty("items")[0].GetProperty("cancelledAt").ValueKind.ShouldBe(JsonValueKind.String);
    }

    [Fact]
    public async Task Accountant_can_delegate_the_requests_step_only()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var target = await kit.AddStaffAsync(manager.Id);
        var accountant = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.Accountant);
        var start = DateTime.UtcNow.AddHours(1);

        await ShouldFailWithAsync(
            await accountant.PostAsJsonAsync("/api/app/delegation", new { toUserId = target.UserId, scope = "allApprovals", startsAt = start, endsAt = start.AddDays(1), reason = "x" }),
            HrErrorCodes.Delegation.DelegateNotEligible);
        (await accountant.PostAsJsonAsync("/api/app/delegation", new { toUserId = target.UserId, scope = "requests", startsAt = start, endsAt = start.AddDays(1), reason = "x" }))
            .StatusCode.ShouldBe(HttpStatusCode.Created);
    }
}
