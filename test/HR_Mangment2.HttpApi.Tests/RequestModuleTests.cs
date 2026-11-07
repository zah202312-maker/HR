using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using HR_Mangment2.Errors;
using HR_Mangment2.Identity;
using HR_Mangment2.Infrastructure;
using HR_Mangment2.Requests;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;
using Xunit;

namespace HR_Mangment2;

[Collection(HostCollection.Name)]
public class RequestModuleTests
{
    private readonly HostFixture _host;

    public RequestModuleTests(HostFixture host)
    {
        _host = host;
    }

    private static async Task<JsonElement> ReadAsync(HttpResponseMessage response) => await response.Content.ReadFromJsonAsync<JsonElement>();

    private static async Task ShouldFailWithAsync(HttpResponseMessage response, string code)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe((HttpStatusCode)422, body);
        JsonDocument.Parse(body).RootElement.GetProperty("error").GetProperty("code").GetString().ShouldBe(code);
    }

    private static async Task<JsonElement> OkAsync(HttpResponseMessage response, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe(expected, body);
        return JsonDocument.Parse(body).RootElement;
    }

    private static async Task<Guid> TypeIdAsync(HttpClient client, string code)
    {
        var types = await client.GetFromJsonAsync<JsonElement>("/api/app/request-type?maxResultCount=200");
        return types.GetProperty("items").EnumerateArray().Single(x => x.GetProperty("code").GetString() == code).GetProperty("id").GetGuid();
    }

    private static async Task<JsonElement> SubmitAsync(HttpClient client, string code = RequestTypeCodes.AdministrativeNeed, string description = "A new desk", Guid? onBehalfOf = null) =>
        await OkAsync(
            await client.PostAsJsonAsync("/api/app/request", new { requestTypeId = await TypeIdAsync(client, code), description, onBehalfOfEmployeeId = onBehalfOf }),
            HttpStatusCode.Created);

    private static Guid IdOf(JsonElement request) => request.GetProperty("id").GetGuid();

    private static string Text(JsonElement element, string property) => element.GetProperty(property).GetString()!;

    private static Task<HttpResponseMessage> ApproveAsync(HttpClient client, Guid id, string? note = null) =>
        client.PostAsJsonAsync($"/api/app/request/{id}/approve", new { note });

    private static Task<HttpResponseMessage> RejectAsync(HttpClient client, Guid id, string reason = "Not needed") =>
        client.PostAsJsonAsync($"/api/app/request/{id}/reject", new { reason });

    private static Task<HttpResponseMessage> CancelAsync(HttpClient client, Guid id) =>
        client.PostAsJsonAsync($"/api/app/request/{id}/cancel", new { note = "No longer needed" });

    private static async Task<JsonElement?> FindInInboxAsync(HttpClient client, Guid id)
    {
        var inbox = await client.GetFromJsonAsync<JsonElement>("/api/app/request/inbox?maxResultCount=200");
        foreach (var item in inbox.GetProperty("items").EnumerateArray())
        {
            if (item.GetProperty("id").GetGuid() == id)
            {
                return item;
            }
        }

        return null;
    }

    private static async Task<string[]> NotificationsAsync(HttpClient client)
    {
        var list = await client.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200");
        return list.GetProperty("items").EnumerateArray().Select(x => Text(x, "templateCode")).ToArray();
    }

    private async Task<HttpClient> ClientForRolesAsync(params string[] roles) =>
        await TestUsers.SignedInWithMfaAsync(_host, "en", roles);

    /// <summary>A directly created user with a file, and a client. Holders of MFA-required roles enroll first.</summary>
    private async Task<(HttpClient Client, Guid UserId, Guid EmployeeId)> OwnerAsync(EmployeeTestKit kit, Guid? managerEmployeeId, params string[] roles)
    {
        var (userId, userName) = await TestUsers.CreateAsync(_host, false, roles);
        var employeeId = await kit.AddFileForUserAsync(userId, managerEmployeeId);
        var client = TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, userName, TestUsers.Password)).AccessToken, "en");

        if (roles.Any(HrRoles.MfaRequired.Contains))
        {
            var enrollment = await ReadAsync(await client.PostAsync("/api/app/account/mfa/enroll", null));
            (await client.PostAsJsonAsync("/api/app/account/mfa/confirm", new { code = Totp.Compute(Text(enrollment, "sharedKey")) })).EnsureSuccessStatusCode();
        }

        return (client, userId, employeeId);
    }

    private async Task<(EmployeeTestKit Kit, CreatedEmployee Manager, HttpClient ManagerClient, CreatedEmployee Staff, HttpClient StaffClient)> TeamAsync()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        return (kit, manager, await kit.SignInAsync(manager), staff, await kit.SignInAsync(staff));
    }

    [Fact]
    public async Task The_new_request_screen_lists_active_types_with_their_flags()
    {
        var (_, _, _, _, staffClient) = await TeamAsync();

        var items = (await staffClient.GetFromJsonAsync<JsonElement>("/api/app/request-type?maxResultCount=200")).GetProperty("items").EnumerateArray().ToList();

        var leave = items.Single(x => Text(x, "code") == RequestTypeCodes.Leave);
        leave.GetProperty("usesDedicatedEndpoint").GetBoolean().ShouldBeTrue();
        leave.GetProperty("destination").GetString().ShouldBe("none");
        items.Single(x => Text(x, "code") == RequestTypeCodes.Accountant).GetProperty("destination").GetString().ShouldBe("accountant");
        items.Single(x => Text(x, "code") == RequestTypeCodes.AdministrativeNeed).GetProperty("usesDedicatedEndpoint").GetBoolean().ShouldBeFalse();
        items.ShouldNotContain(x => Text(x, "code") == RequestTypeCodes.StoreItem);
    }

    [Fact]
    public async Task The_system_administrator_manages_types_and_cannot_move_a_seeded_ones_destination()
    {
        var admin = await ClientForRolesAsync(HrRoles.Employee, HrRoles.SystemAdmin);
        var (_, _, _, _, staffClient) = await TeamAsync();
        var code = "t" + Guid.NewGuid().ToString("N")[..10];
        var body = new { code, nameAr = "نوع", nameEn = "Kind", destination = "none", isActive = false };

        (await staffClient.PostAsJsonAsync("/api/app/request-type", body)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var created = await OkAsync(await admin.PostAsJsonAsync("/api/app/request-type", body), HttpStatusCode.Created);
        created.GetProperty("isSystem").GetBoolean().ShouldBeFalse();
        await ShouldFailWithAsync(await admin.PostAsJsonAsync("/api/app/request-type", body), HrErrorCodes.Request.TypeCodeAlreadyExists);
        (await admin.PostAsJsonAsync("/api/app/request-type", new { code = "Bad Code", nameAr = "ن", nameEn = "K", destination = "none", isActive = true }))
            .StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var url = $"/api/app/request-type/{IdOf(created)}";
        (await admin.PutAsJsonAsync(url, new { nameAr = "نوع", nameEn = "Kind", destination = "accountant", isActive = true, concurrencyStamp = "stale" }))
            .StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var updated = await OkAsync(await admin.PutAsJsonAsync(url, new { nameAr = "نوع", nameEn = "Kind", destination = "accountant", isActive = true, concurrencyStamp = Text(created, "concurrencyStamp") }));
        updated.GetProperty("destination").GetString().ShouldBe("accountant");

        var all = await admin.GetFromJsonAsync<JsonElement>("/api/app/request-type/admin?maxResultCount=200&isActive=false");
        var store = all.GetProperty("items").EnumerateArray().Single(x => Text(x, "code") == RequestTypeCodes.StoreItem);
        await ShouldFailWithAsync(
            await admin.PutAsJsonAsync($"/api/app/request-type/{IdOf(store)}", new { nameAr = "ن", nameEn = "K", destination = "none", isActive = false, concurrencyStamp = Text(store, "concurrencyStamp") }),
            HrErrorCodes.Request.SystemTypeDestinationLocked);
    }

    [Fact]
    public async Task A_request_walks_from_employee_through_manager_and_hr_to_approved()
    {
        var (kit, _, managerClient, _, staffClient) = await TeamAsync();
        var managerName = (await managerClient.GetFromJsonAsync<JsonElement>("/api/app/me")).GetProperty("userId").GetGuid();

        var submitted = await SubmitAsync(staffClient, description: "A new desk");
        var id = IdOf(submitted);

        Text(submitted, "status").ShouldBe("pendingManager");
        Text(submitted, "employeeStatus").ShouldBe("pendingManager");
        Text(submitted, "statusLabel").ShouldBe("Waiting for the department manager");
        Text(submitted, "currentApproverRole").ShouldBe("departmentManager");
        Text(submitted, "detailsSummary").ShouldBe("A new desk");
        submitted.GetProperty("canCancel").GetBoolean().ShouldBeTrue();
        submitted.GetProperty("canApprove").GetBoolean().ShouldBeFalse();
        var steps = submitted.GetProperty("steps").EnumerateArray().ToList();
        steps.Count.ShouldBe(2);
        steps[0].GetProperty("approver").GetProperty("id").GetGuid().ShouldBe(managerName);
        Text(steps[0], "decision").ShouldBe("pending");
        Text(steps[1], "approverRole").ShouldBe("hrManager");
        steps[1].GetProperty("approver").ValueKind.ShouldBe(JsonValueKind.Null);
        Text(steps[1], "decision").ShouldBe("notReached");

        (await NotificationsAsync(managerClient)).ShouldContain("requestPendingAction");
        (await FindInInboxAsync(kit.Hr, id)).ShouldBeNull();

        var item = (await FindInInboxAsync(managerClient, id))!.Value;
        Text(item, "actionKind").ShouldBe("decide");
        item.GetProperty("delegatedFrom").ValueKind.ShouldBe(JsonValueKind.Null);

        (await kit.Hr.PostAsJsonAsync($"/api/app/request/{id}/approve", new { })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var afterManager = await OkAsync(await ApproveAsync(managerClient, id, "Fine by me"));
        Text(afterManager, "status").ShouldBe("pendingHr");
        afterManager.GetProperty("canApprove").GetBoolean().ShouldBeFalse();
        (await FindInInboxAsync(managerClient, id)).ShouldBeNull();
        (await FindInInboxAsync(kit.Hr, id)).ShouldNotBeNull();

        var afterHr = await OkAsync(await ApproveAsync(kit.Hr, id));
        Text(afterHr, "status").ShouldBe("approved");
        afterHr.GetProperty("currentApproverRole").ValueKind.ShouldBe(JsonValueKind.Null);
        afterHr.GetProperty("timeline").EnumerateArray().Select(x => Text(x, "action")).ShouldBe(new[] { "submitted", "approved", "approved" });
        afterHr.GetProperty("steps")[0].GetProperty("note").GetString().ShouldBe("Fine by me");

        await ShouldFailWithAsync(await ApproveAsync(kit.Hr, id), HrErrorCodes.Request.NotPending);

        var mine = await staffClient.GetFromJsonAsync<JsonElement>("/api/app/request/my?employeeStatus=approved");
        var row = mine.GetProperty("items").EnumerateArray().Single(x => x.GetProperty("id").GetGuid() == id);
        Text(row, "statusLabel").ShouldBe("Approved");
        row.GetProperty("currentStageRole").ValueKind.ShouldBe(JsonValueKind.Null);
        row.TryGetProperty("status", out _).ShouldBeFalse();
        (await NotificationsAsync(staffClient)).ShouldContain("requestStatusChanged");

        var log = await managerClient.GetFromJsonAsync<JsonElement>("/api/app/me/administrative-task-log");
        log.GetProperty("items").EnumerateArray().Select(x => Text(x, "taskType")).ShouldContain("requestApproved");
    }

    [Fact]
    public async Task Rejecting_ends_the_request_and_shows_the_reason()
    {
        var (_, _, managerClient, _, staffClient) = await TeamAsync();
        var id = IdOf(await SubmitAsync(staffClient));

        (await managerClient.PostAsJsonAsync($"/api/app/request/{id}/reject", new { reason = "" })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var rejected = await OkAsync(await RejectAsync(managerClient, id, "No budget"));
        Text(rejected, "status").ShouldBe("rejected");
        Text(rejected, "rejectionReason").ShouldBe("No budget");
        rejected.GetProperty("steps")[1].GetProperty("decision").GetString().ShouldBe("notReached");

        var details = await staffClient.GetFromJsonAsync<JsonElement>($"/api/app/request/{id}");
        Text(details, "employeeStatus").ShouldBe("rejected");
        Text(details, "rejectionReason").ShouldBe("No budget");
        await ShouldFailWithAsync(await RejectAsync(managerClient, id), HrErrorCodes.Request.NotPending);
        await ShouldFailWithAsync(await CancelAsync(staffClient, id), HrErrorCodes.Request.CannotCancelAtThisStage);
    }

    [Fact]
    public async Task A_manager_submits_for_their_own_team_only_and_it_starts_at_hr()
    {
        var (kit, _, managerClient, staff, staffClient) = await TeamAsync();
        var otherManager = await kit.AddDepartmentManagerAsync();
        var otherStaff = await kit.AddStaffAsync(otherManager.Id);

        var submitted = await SubmitAsync(managerClient, onBehalfOf: staff.Id);
        var id = IdOf(submitted);

        Text(submitted, "status").ShouldBe("pendingHr");
        submitted.GetProperty("isOnBehalf").GetBoolean().ShouldBeTrue();
        submitted.GetProperty("employee").GetProperty("id").GetGuid().ShouldBe(staff.Id);
        var managerUserId = (await managerClient.GetFromJsonAsync<JsonElement>("/api/app/me")).GetProperty("userId").GetGuid();
        submitted.GetProperty("submittedBy").GetProperty("id").GetGuid().ShouldBe(managerUserId);
        Text(submitted.GetProperty("steps")[0], "decision").ShouldBe("approvedBySubmission");
        Text(submitted.GetProperty("timeline")[0], "action").ShouldBe("submittedOnBehalf");

        (await NotificationsAsync(staffClient)).ShouldContain("requestSubmittedOnBehalf");
        var mine = await staffClient.GetFromJsonAsync<JsonElement>("/api/app/request/my");
        mine.GetProperty("items").EnumerateArray().ShouldContain(x => x.GetProperty("id").GetGuid() == id && x.GetProperty("isOnBehalf").GetBoolean());
        var log = await managerClient.GetFromJsonAsync<JsonElement>("/api/app/me/administrative-task-log");
        log.GetProperty("items").EnumerateArray().Select(x => Text(x, "taskType")).ShouldContain("requestSubmittedOnBehalf");

        var type = await TypeIdAsync(managerClient, RequestTypeCodes.AdministrativeNeed);
        (await managerClient.PostAsJsonAsync("/api/app/request", new { requestTypeId = type, description = "x", onBehalfOfEmployeeId = otherStaff.Id })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await staffClient.PostAsJsonAsync("/api/app/request", new { requestTypeId = type, description = "x", onBehalfOfEmployeeId = staff.Id })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // The submitter is not the owner, so they can still withdraw it.
        submitted.GetProperty("canCancel").GetBoolean().ShouldBeTrue();
        Text(await OkAsync(await CancelAsync(managerClient, id)), "status").ShouldBe("cancelled");
        (await NotificationsAsync(staffClient)).Count(x => x == "requestStatusChanged").ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task A_team_member_outside_the_employee_row_cannot_be_submitted_for()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var (adminClient, _, adminEmployee) = await OwnerAsync(kit, kit.ExecutiveEmployeeId, HrRoles.Employee, HrRoles.SystemAdmin, HrRoles.DepartmentManager);
        var (_, _, reportEmployee) = await OwnerAsync(kit, adminEmployee, HrRoles.Employee, HrRoles.DepartmentManager);

        await ShouldFailWithAsync(
            await adminClient.PostAsJsonAsync("/api/app/request", new
            {
                requestTypeId = await TypeIdAsync(adminClient, RequestTypeCodes.AdministrativeNeed),
                description = "x",
                onBehalfOfEmployeeId = reportEmployee
            }),
            HrErrorCodes.Request.OnBehalfNotAllowed);
    }

    [Fact]
    public async Task Submitting_applies_each_refusal()
    {
        var (kit, _, _, _, staffClient) = await TeamAsync();
        var need = await TypeIdAsync(staffClient, RequestTypeCodes.AdministrativeNeed);

        await ShouldFailWithAsync(
            await staffClient.PostAsJsonAsync("/api/app/request", new { requestTypeId = await TypeIdAsync(staffClient, RequestTypeCodes.Leave), description = "x" }),
            HrErrorCodes.Request.UseDedicatedEndpoint);
        await ShouldFailWithAsync(
            await staffClient.PostAsJsonAsync("/api/app/request", new { requestTypeId = need, description = "x", quantity = 1 }),
            HrErrorCodes.Request.ItemFieldsInvalid);
        (await staffClient.PostAsJsonAsync("/api/app/request", new { requestTypeId = need, description = "" })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await staffClient.PostAsJsonAsync("/api/app/request", new { requestTypeId = Guid.NewGuid(), description = "x" })).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var admin = await ClientForRolesAsync(HrRoles.Employee, HrRoles.SystemAdmin);
        var all = await admin.GetFromJsonAsync<JsonElement>("/api/app/request-type/admin?maxResultCount=200");
        var store = all.GetProperty("items").EnumerateArray().Single(x => Text(x, "code") == RequestTypeCodes.StoreItem);
        await ShouldFailWithAsync(
            await staffClient.PostAsJsonAsync("/api/app/request", new { requestTypeId = IdOf(store), description = "x" }),
            HrErrorCodes.Request.TypeInactive);

        var (executive, _, _) = await OwnerAsync(kit, null, HrRoles.Employee, HrRoles.ExecutiveDirector);
        await ShouldFailWithAsync(
            await executive.PostAsJsonAsync("/api/app/request", new { requestTypeId = need, description = "x" }),
            HrErrorCodes.Request.ExecutiveCannotSubmit);

        var (unlinked, _, _) = await OwnerAsync(kit, null, HrRoles.Employee);
        await ShouldFailWithAsync(
            await unlinked.PostAsJsonAsync("/api/app/request", new { requestTypeId = need, description = "x" }),
            HrErrorCodes.Request.NoApproverAvailable);
    }

    [Fact]
    public async Task The_system_administrator_approves_department_managers_first_and_the_label_says_so()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var (adminClient, _, adminEmployee) = await OwnerAsync(kit, kit.ExecutiveEmployeeId, HrRoles.Employee, HrRoles.SystemAdmin);
        var (managerClient, _, _) = await OwnerAsync(kit, adminEmployee, HrRoles.Employee, HrRoles.DepartmentManager);

        var submitted = await SubmitAsync(managerClient);

        Text(submitted, "status").ShouldBe("pendingManager");
        Text(submitted, "currentApproverRole").ShouldBe("systemAdmin");
        Text(submitted, "statusLabel").ShouldBe("Waiting for the system administrator");
        Text(await OkAsync(await ApproveAsync(adminClient, IdOf(submitted))), "status").ShouldBe("pendingHr");

        // Another holder of the role has no step here: the step is named.
        var otherAdmin = await ClientForRolesAsync(HrRoles.Employee, HrRoles.SystemAdmin);
        (await otherAdmin.GetAsync($"/api/app/request/{IdOf(submitted)}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task The_hr_manager_goes_to_the_executive_and_nobody_approves_their_own_request()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var (hrClient, _, _) = await OwnerAsync(kit, kit.ExecutiveEmployeeId, HrRoles.Employee, HrRoles.HrManager);
        var executive = await ClientForRolesAsync(HrRoles.Employee, HrRoles.ExecutiveDirector);

        var submitted = await SubmitAsync(hrClient);
        var id = IdOf(submitted);
        Text(submitted, "status").ShouldBe("pendingExecutive");
        Text(submitted, "statusLabel").ShouldBe("Waiting for the executive director");

        await ShouldFailWithAsync(await ApproveAsync(hrClient, id), HrErrorCodes.Request.SelfApprovalNotAllowed);
        await ShouldFailWithAsync(await RejectAsync(hrClient, id), HrErrorCodes.Request.SelfApprovalNotAllowed);
        (await FindInInboxAsync(hrClient, id)).ShouldBeNull();

        Text(await OkAsync(await ApproveAsync(executive, id)), "status").ShouldBe("approved");
    }

    [Fact]
    public async Task Details_of_an_accountant_request_are_absent_for_the_manager_and_completing_it_is_the_accountants_approval()
    {
        var (kit, _, managerClient, _, staffClient) = await TeamAsync();
        var accountant = await ClientForRolesAsync(HrRoles.Employee, HrRoles.Accountant);

        var submitted = await SubmitAsync(staffClient, RequestTypeCodes.Accountant, "Salary advance of one month");
        var id = IdOf(submitted);
        Text(submitted, "description").ShouldBe("Salary advance of one month");

        var inboxItem = (await FindInInboxAsync(managerClient, id))!.Value;
        inboxItem.TryGetProperty("detailsSummary", out _).ShouldBeFalse();
        Text(inboxItem.GetProperty("type"), "code").ShouldBe(RequestTypeCodes.Accountant);

        var forManager = await OkAsync(await ApproveAsync(managerClient, id, "Secret note"));
        forManager.TryGetProperty("description", out _).ShouldBeFalse();
        forManager.TryGetProperty("detailsSummary", out _).ShouldBeFalse();
        forManager.GetProperty("steps")[0].TryGetProperty("note", out _).ShouldBeFalse();
        forManager.GetProperty("timeline")[1].TryGetProperty("note", out _).ShouldBeFalse();
        forManager.GetRawText().ShouldNotContain("Salary advance");
        forManager.GetRawText().ShouldNotContain("Secret note");

        var list = await managerClient.GetStringAsync("/api/app/request?maxResultCount=200");
        list.ShouldNotContain("Salary advance");

        var forHr = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/request/{id}");
        Text(forHr, "description").ShouldBe("Salary advance of one month");
        Text(forHr.GetProperty("steps")[0], "note").ShouldBe("Secret note");

        Text(await OkAsync(await ApproveAsync(kit.Hr, id)), "status").ShouldBe("pendingDestination");

        var mine = await staffClient.GetFromJsonAsync<JsonElement>($"/api/app/request/my?maxResultCount=200&employeeStatus=approved");
        var row = mine.GetProperty("items").EnumerateArray().Single(x => x.GetProperty("id").GetGuid() == id);
        Text(row, "currentStageRole").ShouldBe("accountant");
        Text(row, "statusLabel").ShouldBe("Approved");

        var done = await OkAsync(await ApproveAsync(accountant, id));
        Text(done, "status").ShouldBe("completed");
        Text(done, "description").ShouldBe("Salary advance of one month");
        Text(done.GetProperty("timeline").EnumerateArray().Last(), "action").ShouldBe("completed");
    }

    [Fact]
    public async Task The_accountant_may_carry_out_their_own_request_but_never_reject_it()
    {
        var (kit, manager, managerClient, _, _) = await TeamAsync();
        var (accountant, _, _) = await OwnerAsync(kit, manager.Id, HrRoles.Employee, HrRoles.Accountant);

        var submitted = await SubmitAsync(accountant, RequestTypeCodes.Accountant);
        var id = IdOf(submitted);
        await ShouldFailWithAsync(await ApproveAsync(accountant, id), HrErrorCodes.Request.SelfApprovalNotAllowed);
        (await FindInInboxAsync(accountant, id)).ShouldBeNull();

        await OkAsync(await ApproveAsync(managerClient, id));
        var atDestination = await OkAsync(await ApproveAsync(kit.Hr, id));
        Text(atDestination, "status").ShouldBe("pendingDestination");

        (await FindInInboxAsync(accountant, id)).ShouldNotBeNull();
        var own = await accountant.GetFromJsonAsync<JsonElement>($"/api/app/request/{id}");
        own.GetProperty("canApprove").GetBoolean().ShouldBeTrue();
        own.GetProperty("canReject").GetBoolean().ShouldBeFalse();
        await ShouldFailWithAsync(await RejectAsync(accountant, id), HrErrorCodes.Request.SelfApprovalNotAllowed);

        var done = await OkAsync(await ApproveAsync(accountant, id));
        Text(done, "status").ShouldBe("completed");

        // Recorded as execution, not as an approval.
        done.GetProperty("timeline").EnumerateArray().Select(x => Text(x, "action")).ShouldBe(new[] { "submitted", "approved", "approved", "completed" });
    }

    [Fact]
    public async Task A_delegate_decides_within_scope_with_both_names_recorded_and_never_their_own_request()
    {
        var (kit, manager, managerClient, _, staffClient) = await TeamAsync();
        var delegateEmployee = await kit.AddStaffAsync(manager.Id);
        var delegateClient = await kit.SignInAsync(delegateEmployee);
        var delegateUserId = (await delegateClient.GetFromJsonAsync<JsonElement>("/api/app/me")).GetProperty("userId").GetGuid();
        var managerUserId = (await managerClient.GetFromJsonAsync<JsonElement>("/api/app/me")).GetProperty("userId").GetGuid();

        var ownRequest = IdOf(await SubmitAsync(delegateClient));
        var id = IdOf(await SubmitAsync(staffClient));

        // Without a delegation this plain employee has no inbox and no decision at all.
        (await delegateClient.GetAsync("/api/app/request/inbox")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await ApproveAsync(delegateClient, id)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        await OkAsync(
            await managerClient.PostAsJsonAsync("/api/app/delegation", new
            {
                toUserId = delegateUserId,
                scope = "memosAndIncidents",
                startsAt = DateTime.UtcNow,
                endsAt = DateTime.UtcNow.AddDays(1),
                reason = "Leave"
            }),
            HttpStatusCode.Created);
        (await delegateClient.GetAsync($"/api/app/request/{id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var delegation = await managerClient.PostAsJsonAsync("/api/app/delegation", new
        {
            toUserId = delegateUserId,
            scope = "requests",
            startsAt = DateTime.UtcNow,
            endsAt = DateTime.UtcNow.AddDays(1),
            reason = "Leave"
        });
        await OkAsync(delegation, HttpStatusCode.Created);

        var item = (await FindInInboxAsync(delegateClient, id))!.Value;
        item.GetProperty("delegatedFrom").GetProperty("id").GetGuid().ShouldBe(managerUserId);
        (await FindInInboxAsync(delegateClient, ownRequest)).ShouldBeNull();

        var summary = await delegateClient.GetFromJsonAsync<JsonElement>("/api/app/me/inbox-summary");
        var requests = summary.GetProperty("items").EnumerateArray().Single(x => Text(x, "category") == "request");
        requests.GetProperty("count").GetInt32().ShouldBe(requests.GetProperty("delegatedCount").GetInt32());
        requests.GetProperty("count").GetInt32().ShouldBeGreaterThan(0);

        await ShouldFailWithAsync(await ApproveAsync(delegateClient, ownRequest), HrErrorCodes.Request.SelfApprovalNotAllowed);

        var decided = await OkAsync(await ApproveAsync(delegateClient, id, "On behalf of the manager"));
        var step = decided.GetProperty("steps")[0];
        step.GetProperty("decidedBy").GetProperty("id").GetGuid().ShouldBe(delegateUserId);
        step.GetProperty("delegatedFrom").GetProperty("id").GetGuid().ShouldBe(managerUserId);

        // The first decision stands: the step has moved on to HR, so the principal has nothing left to decide on it.
        (await ApproveAsync(managerClient, id)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // The delegate's own request stays with the principal, who can still decide it.
        await OkAsync(await ApproveAsync(managerClient, ownRequest));
    }

    [Fact]
    public async Task Changing_the_manager_moves_pending_requests_and_shows_it_on_the_timeline()
    {
        var (kit, _, managerClient, staff, staffClient) = await TeamAsync();
        var newManager = await kit.AddDepartmentManagerAsync();
        var newManagerClient = await kit.SignInAsync(newManager);

        var waiting = IdOf(await SubmitAsync(staffClient));
        var atHr = IdOf(await SubmitAsync(staffClient));
        await OkAsync(await ApproveAsync(managerClient, atHr));

        var stamp = (await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/employee/{staff.Id}")).GetProperty("concurrencyStamp").GetString();
        var moved = await OkAsync(await kit.Hr.PutAsJsonAsync($"/api/app/employee/{staff.Id}/manager", new { managerEmployeeId = newManager.Id, concurrencyStamp = stamp }));
        moved.GetProperty("movedPendingItemCount").GetInt32().ShouldBe(1);

        (await FindInInboxAsync(newManagerClient, waiting)).ShouldNotBeNull();
        (await FindInInboxAsync(newManagerClient, atHr)).ShouldBeNull();
        (await FindInInboxAsync(managerClient, waiting)).ShouldBeNull();
        (await ApproveAsync(managerClient, waiting)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var approved = await OkAsync(await ApproveAsync(newManagerClient, waiting));
        approved.GetProperty("timeline").EnumerateArray().Select(x => Text(x, "action")).ShouldContain("approverReassigned");
    }

    [Fact]
    public async Task Only_the_people_on_a_request_can_open_it()
    {
        var (kit, manager, managerClient, staff, staffClient) = await TeamAsync();
        var colleague = await kit.SignInAsync(await kit.AddStaffAsync(manager.Id));
        var outsiderManager = await kit.SignInAsync(await kit.AddDepartmentManagerAsync());
        var id = IdOf(await SubmitAsync(staffClient));

        (await colleague.GetAsync($"/api/app/request/{id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await outsiderManager.GetAsync($"/api/app/request/{id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await staffClient.GetAsync($"/api/app/request/{Guid.NewGuid()}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await managerClient.GetAsync($"/api/app/request/{id}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await kit.Hr.GetAsync($"/api/app/request/{id}")).StatusCode.ShouldBe(HttpStatusCode.OK);

        (await staffClient.GetAsync("/api/app/request")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var outsiderList = await outsiderManager.GetFromJsonAsync<JsonElement>("/api/app/request?maxResultCount=200");
        outsiderList.GetProperty("items").EnumerateArray().ShouldNotContain(x => x.GetProperty("id").GetGuid() == id);
        var teamList = await managerClient.GetFromJsonAsync<JsonElement>($"/api/app/request?employeeId={staff.Id}&status=pendingManager");
        teamList.GetProperty("items").EnumerateArray().ShouldContain(x => x.GetProperty("id").GetGuid() == id);
        (await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/request?employeeId={staff.Id}")).GetProperty("totalCount").GetInt64().ShouldBe(1);

        (await colleague.PostAsJsonAsync($"/api/app/request/{id}/cancel", new { })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await kit.Hr.GetAsync("/api/app/request?sorting=bogus")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Cancelling_tells_the_current_approver_and_stops_the_request()
    {
        var (_, _, managerClient, _, staffClient) = await TeamAsync();
        var id = IdOf(await SubmitAsync(staffClient));

        var cancelled = await OkAsync(await CancelAsync(staffClient, id));
        Text(cancelled, "status").ShouldBe("cancelled");
        Text(cancelled, "statusLabel").ShouldBe("Cancelled");
        cancelled.GetProperty("canCancel").GetBoolean().ShouldBeFalse();
        (await FindInInboxAsync(managerClient, id)).ShouldBeNull();
        (await NotificationsAsync(managerClient)).ShouldContain("requestStatusChanged");
    }

    [Fact]
    public async Task Repeating_a_submission_with_the_same_key_returns_the_first_request()
    {
        var (_, _, _, _, staffClient) = await TeamAsync();
        var key = Guid.NewGuid().ToString();
        var body = new { requestTypeId = await TypeIdAsync(staffClient, RequestTypeCodes.AdministrativeNeed), description = "Once" };

        HttpRequestMessage Message() => new(HttpMethod.Post, "/api/app/request")
        {
            Content = JsonContent.Create(body),
            Headers = { { "Idempotency-Key", key } }
        };

        var first = await OkAsync(await staffClient.SendAsync(Message()), HttpStatusCode.Created);
        var second = await OkAsync(await staffClient.SendAsync(Message()), HttpStatusCode.Created);

        IdOf(second).ShouldBe(IdOf(first));
    }

    [Fact]
    public async Task The_summary_counts_pending_work_and_the_status_label_follows_the_language()
    {
        var (_, _, managerClient, _, staffClient) = await TeamAsync();
        (await staffClient.GetFromJsonAsync<JsonElement>("/api/app/me/inbox-summary")).GetProperty("totalCount").GetInt32().ShouldBe(0);

        var id = IdOf(await SubmitAsync(staffClient));

        var summary = await managerClient.GetFromJsonAsync<JsonElement>("/api/app/me/inbox-summary");
        var row = summary.GetProperty("items").EnumerateArray().Single(x => Text(x, "category") == "request");
        row.GetProperty("count").GetInt32().ShouldBe(1);
        row.GetProperty("delegatedCount").GetInt32().ShouldBe(0);
        summary.GetProperty("totalCount").GetInt32().ShouldBe(1);

        var arabic = new HttpRequestMessage(HttpMethod.Get, $"/api/app/request/{id}");
        arabic.Headers.AcceptLanguage.ParseAdd("ar");
        arabic.Headers.Authorization = staffClient.DefaultRequestHeaders.Authorization;
        staffClient.DefaultRequestHeaders.AcceptLanguage.Clear();
        Text(await OkAsync(await staffClient.SendAsync(arabic)), "statusLabel").ShouldBe("بانتظار مدير القسم");
    }

    [Fact]
    public async Task A_reminder_repeats_the_notice_after_the_interval_and_changes_nothing_else()
    {
        var (_, _, managerClient, _, staffClient) = await TeamAsync();
        var id = IdOf(await SubmitAsync(staffClient));
        var before = (await NotificationsAsync(managerClient)).Count(x => x == "requestApprovalReminder");

        using var scope = _host.Factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var tenant = await services.GetRequiredService<ITenantRepository>().FindByNameAsync(
            services.GetRequiredService<ITenantNormalizer>().NormalizeName(HostFixture.TestTenantName)!);
        using var change = services.GetRequiredService<ICurrentTenant>().Change(tenant!.Id);
        var reminders = services.GetRequiredService<RequestApprovalReminderService>();
        var unitOfWorks = services.GetRequiredService<IUnitOfWorkManager>();

        async Task<int> RunAsync(DateTime now)
        {
            using var uow = unitOfWorks.Begin(requiresNew: true, isTransactional: true);
            var sent = await reminders.SendDueAsync(now);
            await uow.CompleteAsync();
            return sent;
        }

        await RunAsync(DateTime.UtcNow);
        (await NotificationsAsync(managerClient)).Count(x => x == "requestApprovalReminder").ShouldBe(before);

        var later = DateTime.UtcNow.AddDays(14);
        (await RunAsync(later)).ShouldBeGreaterThan(0);
        (await NotificationsAsync(managerClient)).Count(x => x == "requestApprovalReminder").ShouldBe(before + 1);

        await RunAsync(later);
        (await NotificationsAsync(managerClient)).Count(x => x == "requestApprovalReminder").ShouldBe(before + 1);

        Text(await staffClient.GetFromJsonAsync<JsonElement>($"/api/app/request/{id}"), "status").ShouldBe("pendingManager");
    }
}
