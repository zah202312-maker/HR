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
using Shouldly;
using Xunit;

namespace HR_Mangment2;

[Collection(HostCollection.Name)]
public class MemoIncidentModuleTests
{
    private readonly HostFixture _host;

    public MemoIncidentModuleTests(HostFixture host)
    {
        _host = host;
    }

    private sealed record Team(EmployeeTestKit Kit, CreatedEmployee Manager, HttpClient ManagerClient, CreatedEmployee Staff, HttpClient StaffClient);

    private async Task<Team> TeamAsync()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        return new Team(kit, manager, await kit.SignInAsync(manager), staff, await kit.SignInAsync(staff));
    }

    /// <summary>A directly created user with a file and a client, for rows the HTTP rules would limit to one holder.</summary>
    private async Task<(HttpClient Client, Guid UserId, Guid EmployeeId)> OwnerAsync(EmployeeTestKit kit, Guid? managerEmployeeId, params string[] roles)
    {
        var (userId, userName) = await TestUsers.CreateAsync(_host, false, roles);
        var employeeId = await kit.AddFileForUserAsync(userId, managerEmployeeId);
        var client = TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, userName, TestUsers.Password)).AccessToken, "en");

        if (roles.Any(HrRoles.MfaRequired.Contains))
        {
            var enrollment = await OkAsync(await client.PostAsync("/api/app/account/mfa/enroll", null));
            (await client.PostAsJsonAsync("/api/app/account/mfa/confirm", new { code = Totp.Compute(Text(enrollment, "sharedKey")) })).EnsureSuccessStatusCode();
        }

        return (client, userId, employeeId);
    }

    private static async Task<JsonElement> OkAsync(HttpResponseMessage response, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe(expected, body);
        return JsonDocument.Parse(body).RootElement;
    }

    private static async Task ShouldFailWithAsync(HttpResponseMessage response, string code)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe((HttpStatusCode)422, body);
        JsonDocument.Parse(body).RootElement.GetProperty("error").GetProperty("code").GetString().ShouldBe(code);
    }

    private static string Text(JsonElement element, string property) => element.GetProperty(property).GetString()!;

    private static Guid IdOf(JsonElement element) => element.GetProperty("id").GetGuid();

    private static string[] Actions(JsonElement item) => item.GetProperty("timeline").EnumerateArray().Select(x => Text(x, "action")).ToArray();

    private static async Task<JsonElement> WriteMemoAsync(HttpClient client, string kind = "problem", string content = "The printer is broken") =>
        await OkAsync(await client.PostAsJsonAsync("/api/app/memo", new { kind, content }), HttpStatusCode.Created);

    private static async Task<JsonElement> ReportAsync(HttpClient client, string description = "The AC is broken") =>
        await OkAsync(await client.PostAsJsonAsync("/api/app/incident", new { description }), HttpStatusCode.Created);

    private static Task<HttpResponseMessage> ForwardAsync(HttpClient client, Guid id, string area = "memo", string? note = null) =>
        client.PostAsJsonAsync($"/api/app/{area}/{id}/forward", new { note });

    private static Task<HttpResponseMessage> ToIncidentAsync(HttpClient client, Guid id) =>
        client.PostAsJsonAsync($"/api/app/memo/{id}/convert", new { target = "incident", note = "Handling it" });

    private static Task<HttpResponseMessage> CloseAsync(HttpClient client, Guid id) =>
        client.PostAsJsonAsync($"/api/app/memo/{id}/close", new { reason = "Nothing to do" });

    private static (string Start, string End) NextWorkingDays()
    {
        var day = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(14);
        while (day.DayOfWeek != DayOfWeek.Sunday)
        {
            day = day.AddDays(1);
        }

        return (day.ToString("yyyy-MM-dd"), day.AddDays(1).ToString("yyyy-MM-dd"));
    }

    private static Task SetEntitlementAsync(HttpClient hr, Guid employeeId, decimal days) => AnnualLeaveDefault.OpenWithAsync(hr, employeeId, days);

    private static Task<HttpResponseMessage> ToLeaveAsync(HttpClient client, Guid id)
    {
        var (start, end) = NextWorkingDays();
        return client.PostAsJsonAsync($"/api/app/memo/{id}/convert", new { target = "leaveRequest", leave = new { kind = "annual", startDate = start, endDate = end } });
    }

    private static async Task<int> CountInInboxAsync(HttpClient client, string area, Guid id, string query = "")
    {
        var inbox = await client.GetFromJsonAsync<JsonElement>($"/api/app/{area}/inbox?maxResultCount=200{query}");
        return inbox.GetProperty("items").EnumerateArray().Count(x => IdOf(x) == id);
    }

    private static async Task<string[]> NotificationsAsync(HttpClient client)
    {
        var list = await client.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200");
        return list.GetProperty("items").EnumerateArray().Select(x => Text(x, "templateCode")).ToArray();
    }

    [Fact]
    public async Task A_memo_walks_from_the_employee_through_the_manager_to_hr_and_closes()
    {
        var team = await TeamAsync();

        var memo = await WriteMemoAsync(team.StaffClient);
        var id = IdOf(memo);
        Text(memo, "status").ShouldBe("pendingManager");
        Text(memo, "statusLabel").ShouldBe("Waiting for the department manager");
        Text(memo, "currentHandlerRole").ShouldBe("departmentManager");
        memo.GetProperty("employee").GetProperty("id").GetGuid().ShouldBe(team.Staff.Id);
        Actions(memo).ShouldBe(new[] { "submitted" });
        (await NotificationsAsync(team.ManagerClient)).ShouldContain("memoUpdate");

        (await CountInInboxAsync(team.ManagerClient, "memo", id)).ShouldBe(1);
        (await CountInInboxAsync(team.Kit.Hr, "memo", id)).ShouldBe(0);

        // HR has not received it yet, so it is neither theirs to act on nor to read.
        (await team.Kit.Hr.GetAsync($"/api/app/memo/{id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await CloseAsync(team.Kit.Hr, id)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // A manager may not close: the permission belongs to the last stage.
        (await CloseAsync(team.ManagerClient, id)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // The body is optional on this operation.
        var forwarded = await OkAsync(await team.ManagerClient.PostAsync($"/api/app/memo/{id}/forward", null));
        Text(forwarded, "status").ShouldBe("pendingHr");
        Text(forwarded, "currentHandlerRole").ShouldBe("hrManager");
        Actions(forwarded).ShouldBe(new[] { "submitted", "forwardedToHr" });
        (await NotificationsAsync(team.StaffClient)).ShouldContain("memoUpdate");

        (await CountInInboxAsync(team.ManagerClient, "memo", id)).ShouldBe(0);
        (await CountInInboxAsync(team.ManagerClient, "memo", id, "&status=pendingHr")).ShouldBe(1);
        (await CountInInboxAsync(team.Kit.Hr, "memo", id)).ShouldBe(1);

        // It passed through the manager, so they still read it, but it has moved on.
        await OkAsync(await team.ManagerClient.GetAsync($"/api/app/memo/{id}"));
        await ShouldFailWithAsync(await ForwardAsync(team.ManagerClient, id), HrErrorCodes.Memo.InvalidState);
        // D2: forwarding is not an HR operation.
        (await ForwardAsync(team.Kit.Hr, id)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var closed = await OkAsync(await CloseAsync(team.Kit.Hr, id));
        Text(closed, "status").ShouldBe("closed");
        Text(closed, "closeReason").ShouldBe("Nothing to do");
        closed.GetProperty("currentHandlerRole").ValueKind.ShouldBe(JsonValueKind.Null);

        // The writer sees where it ended and why.
        var mine = await OkAsync(await team.StaffClient.GetAsync($"/api/app/memo/{id}"));
        Text(mine, "closeReason").ShouldBe("Nothing to do");
        var list = await team.StaffClient.GetFromJsonAsync<JsonElement>("/api/app/memo/my?status=closed");
        list.GetProperty("items").EnumerateArray().Single(x => IdOf(x) == id);

        await ShouldFailWithAsync(await CloseAsync(team.Kit.Hr, id), HrErrorCodes.Memo.InvalidState);
    }

    [Fact]
    public async Task Only_the_people_on_a_memo_can_open_it_and_nobody_handles_their_own()
    {
        var team = await TeamAsync();
        var id = IdOf(await WriteMemoAsync(team.StaffClient));

        var colleague = await team.Kit.SignInAsync(await team.Kit.AddStaffAsync(team.Manager.Id));
        var otherManager = await team.Kit.SignInAsync(await team.Kit.AddDepartmentManagerAsync());

        (await colleague.GetAsync($"/api/app/memo/{id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await otherManager.GetAsync($"/api/app/memo/{id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await ForwardAsync(otherManager, id)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await ForwardAsync(colleague, id)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await ToIncidentAsync(otherManager, id)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await team.ManagerClient.GetAsync($"/api/app/memo/{Guid.NewGuid()}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // The manager's own memo goes straight to HR (decision 2026-11-10), and the manager cannot act on it.
        var own = await WriteMemoAsync(team.ManagerClient);
        Text(own, "status").ShouldBe("pendingHr");
        Text(own, "currentHandlerRole").ShouldBe("hrManager");
        await ShouldFailWithAsync(await ForwardAsync(team.ManagerClient, IdOf(own)), HrErrorCodes.Memo.SelfActionNotAllowed);
        await ShouldFailWithAsync(await ToIncidentAsync(team.ManagerClient, IdOf(own)), HrErrorCodes.Memo.SelfActionNotAllowed);
    }

    [Fact]
    public async Task A_memo_needs_a_manager_and_the_executive_does_not_write_one()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var (noManager, _, _) = await OwnerAsync(kit, null, HrRoles.Employee);

        await ShouldFailWithAsync(await noManager.PostAsJsonAsync("/api/app/memo", new { kind = "leave", content = "x" }), HrErrorCodes.Memo.NoManagerLinked);
        await ShouldFailWithAsync(await noManager.PostAsJsonAsync("/api/app/incident", new { description = "x" }), HrErrorCodes.Incident.NoManagerLinked);

        (await noManager.PostAsJsonAsync("/api/app/memo", new { kind = "other", content = "x" })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await noManager.PostAsJsonAsync("/api/app/memo", new { kind = "leave", content = "" })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await noManager.PostAsJsonAsync("/api/app/memo", new { kind = "leave", content = new string('x', 4001) })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var (executive, _, _) = await OwnerAsync(kit, null, HrRoles.ExecutiveDirector);
        (await executive.PostAsJsonAsync("/api/app/memo", new { kind = "leave", content = "x" })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await executive.GetAsync("/api/app/memo/my")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task The_hr_managers_memo_stops_at_the_executive_and_the_system_admins_may_go_on_to_hr()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var (executive, _, _) = await OwnerAsync(kit, null, HrRoles.ExecutiveDirector);
        var (hrWriter, _, _) = await OwnerAsync(kit, kit.ExecutiveEmployeeId, HrRoles.Employee, HrRoles.HrManager);
        var (adminWriter, _, _) = await OwnerAsync(kit, null, HrRoles.Employee, HrRoles.SystemAdmin);

        var fromHr = await WriteMemoAsync(hrWriter);
        Text(fromHr, "status").ShouldBe("pendingExecutive");
        Text(fromHr, "currentHandlerRole").ShouldBe("executiveDirector");

        await ShouldFailWithAsync(await ForwardAsync(executive, IdOf(fromHr)), HrErrorCodes.Memo.InvalidState);
        await ShouldFailWithAsync(await CloseAsync(hrWriter, IdOf(fromHr)), HrErrorCodes.Memo.SelfActionNotAllowed);
        Text(await OkAsync(await CloseAsync(executive, IdOf(fromHr))), "status").ShouldBe("closed");

        var fromAdmin = await WriteMemoAsync(adminWriter);
        Text(fromAdmin, "status").ShouldBe("pendingExecutive");

        // HR is the last stage for this one, so the executive cannot close it.
        await ShouldFailWithAsync(await CloseAsync(executive, IdOf(fromAdmin)), HrErrorCodes.Memo.InvalidState);
        Text(await OkAsync(await ForwardAsync(executive, IdOf(fromAdmin), note: "For HR")), "status").ShouldBe("pendingHr");
        (await CountInInboxAsync(kit.Hr, "memo", IdOf(fromAdmin))).ShouldBe(1);
    }

    [Fact]
    public async Task Converting_a_leave_memo_makes_a_leave_request_that_starts_at_hr_with_the_balance_held()
    {
        var team = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 24);
        var id = IdOf(await WriteMemoAsync(team.StaffClient, "leave", "Family matter"));

        await ShouldFailWithAsync(await ToIncidentAsync(team.ManagerClient, id), HrErrorCodes.Memo.ConvertTargetMismatch);
        await ShouldFailWithAsync(
            await team.ManagerClient.PostAsJsonAsync($"/api/app/memo/{id}/convert", new { target = "leaveRequest" }),
            HrErrorCodes.Memo.LeaveDetailsInvalid);
        await ShouldFailWithAsync(
            await team.ManagerClient.PostAsJsonAsync($"/api/app/memo/{id}/convert", new { target = "leaveRequest", leave = new { kind = "annual", startDate = "2026-12-01" } }),
            HrErrorCodes.Memo.LeaveDetailsInvalid);
        await ShouldFailWithAsync(
            await team.ManagerClient.PostAsJsonAsync($"/api/app/memo/{id}/convert", new { target = "leaveRequest", leave = new { kind = "annual", startDate = "2026-12-10", endDate = "2026-12-01" } }),
            HrErrorCodes.Leave.InvalidDateRange);

        var converted = await OkAsync(await ToLeaveAsync(team.ManagerClient, id));
        Text(converted, "status").ShouldBe("convertedToRequest");
        Text(converted, "statusLabel").ShouldBe("Converted to a request");
        converted.GetProperty("currentHandlerRole").ValueKind.ShouldBe(JsonValueKind.Null);
        Actions(converted).ShouldContain("convertedToRequest");

        var requestId = converted.GetProperty("convertedRequestId").GetGuid();
        var request = await OkAsync(await team.Kit.Hr.GetAsync($"/api/app/request/{requestId}"));
        Text(request, "status").ShouldBe("pendingHr");
        request.GetProperty("sourceMemoId").GetGuid().ShouldBe(id);
        request.GetProperty("isOnBehalf").GetBoolean().ShouldBeFalse();
        request.GetProperty("employee").GetProperty("id").GetGuid().ShouldBe(team.Staff.Id);
        Text(request.GetProperty("steps")[0], "decision").ShouldBe("approvedBySubmission");

        var balance = await team.Kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/leave-balance/employee/{team.Staff.Id}");
        balance.GetProperty("heldDays").GetDecimal().ShouldBe(2);

        await ShouldFailWithAsync(await ToLeaveAsync(team.ManagerClient, id), HrErrorCodes.Memo.InvalidState);

        // The writer finishes it through the normal flow, and the balance follows.
        var approved = await OkAsync(await team.Kit.Hr.PostAsync($"/api/app/request/{requestId}/approve", null));
        Text(approved, "status").ShouldBe("approved");
    }

    [Fact]
    public async Task Converting_after_hr_received_it_still_records_the_manager_step_and_keeps_hrs_approval_separate()
    {
        var team = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 24);
        var id = IdOf(await WriteMemoAsync(team.StaffClient, "leave"));
        await OkAsync(await ForwardAsync(team.ManagerClient, id));

        var converted = await OkAsync(await ToLeaveAsync(team.Kit.Hr, id));
        var request = await OkAsync(await team.Kit.Hr.GetAsync($"/api/app/request/{converted.GetProperty("convertedRequestId").GetGuid()}"));

        Text(request, "status").ShouldBe("pendingHr");
        Text(request.GetProperty("steps")[0], "decision").ShouldBe("approvedBySubmission");
        Text(request.GetProperty("steps")[1], "decision").ShouldBe("pending");
        request.GetProperty("canApprove").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public async Task The_hr_managers_leave_memo_starts_at_the_executive_when_converted()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var (executive, _, _) = await OwnerAsync(kit, null, HrRoles.ExecutiveDirector);
        var (hrWriter, _, hrEmployee) = await OwnerAsync(kit, kit.ExecutiveEmployeeId, HrRoles.Employee, HrRoles.HrManager);
        await SetEntitlementAsync(kit.Hr, hrEmployee, 24);

        // Without a shift the leave cannot be worked out, so this also covers a planner refusal.
        var id = IdOf(await WriteMemoAsync(hrWriter, "leave"));
        await ShouldFailWithAsync(await ToLeaveAsync(executive, id), HrErrorCodes.Leave.NoShiftAssigned);

        await OkAsync(await kit.Hr.PutAsJsonAsync($"/api/app/employee/{hrEmployee}/shift", new { shiftId = kit.ShiftId, startDate = "2025-02-01" }));

        var converted = await OkAsync(await ToLeaveAsync(executive, id));
        var requestId = converted.GetProperty("convertedRequestId").GetGuid();
        var request = await OkAsync(await executive.GetAsync($"/api/app/request/{requestId}"));

        Text(request, "status").ShouldBe("pendingExecutive");
        Text(request.GetProperty("steps")[0], "decision").ShouldBe("pending");
    }

    [Fact]
    public async Task A_leave_memo_cannot_outrun_the_balance_or_skip_a_required_document()
    {
        var team = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 1);
        var id = IdOf(await WriteMemoAsync(team.StaffClient, "leave"));

        await ShouldFailWithAsync(await ToLeaveAsync(team.ManagerClient, id), HrErrorCodes.Leave.InsufficientBalance);

        var admin = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.SystemAdmin);
        var sick = await OkAsync(
            await admin.PostAsJsonAsync("/api/app/other-leave-type", new { nameAr = "مرضية", nameEn = "Sick", isPaid = true, requiresAttachment = true, eligibleCategory = "all", isActive = true }),
            HttpStatusCode.Created);

        var (start, end) = NextWorkingDays();
        await ShouldFailWithAsync(
            await team.ManagerClient.PostAsJsonAsync($"/api/app/memo/{id}/convert", new { target = "leaveRequest", leave = new { kind = "other", otherLeaveTypeId = IdOf(sick), startDate = start, endDate = end } }),
            HrErrorCodes.Memo.LeaveAttachmentRequired);
        await ShouldFailWithAsync(
            await team.ManagerClient.PostAsJsonAsync($"/api/app/memo/{id}/convert", new { target = "leaveRequest", leave = new { kind = "other", otherLeaveTypeId = Guid.NewGuid(), startDate = start, endDate = end } }),
            HrErrorCodes.Memo.LeaveDetailsInvalid);

        // Nothing changed: the failed attempts left the memo where it was, with no request behind it.
        var memo = await OkAsync(await team.ManagerClient.GetAsync($"/api/app/memo/{id}"));
        Text(memo, "status").ShouldBe("pendingManager");
        memo.GetProperty("convertedRequestId").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task Converting_a_problem_memo_makes_an_incident_at_hr()
    {
        var team = await TeamAsync();
        var id = IdOf(await WriteMemoAsync(team.StaffClient, "problem", "The door is broken"));

        var converted = await OkAsync(await ToIncidentAsync(team.ManagerClient, id));
        Text(converted, "status").ShouldBe("convertedToIncident");
        var incidentId = converted.GetProperty("convertedIncidentId").GetGuid();

        var incident = await OkAsync(await team.Kit.Hr.GetAsync($"/api/app/incident/{incidentId}"));
        Text(incident, "status").ShouldBe("pendingHr");
        Text(incident, "description").ShouldBe("The door is broken");
        incident.GetProperty("sourceMemoId").GetGuid().ShouldBe(id);
        incident.GetProperty("employee").GetProperty("id").GetGuid().ShouldBe(team.Staff.Id);

        // It belongs to the writer, who can follow it.
        var mine = await team.StaffClient.GetFromJsonAsync<JsonElement>("/api/app/incident/my");
        mine.GetProperty("items").EnumerateArray().Single(x => IdOf(x) == incidentId);

        var withLeave = IdOf(await WriteMemoAsync(team.StaffClient));
        var (start, end) = NextWorkingDays();
        await ShouldFailWithAsync(
            await team.ManagerClient.PostAsJsonAsync($"/api/app/memo/{withLeave}/convert", new { target = "incident", leave = new { kind = "annual", startDate = start, endDate = end } }),
            HrErrorCodes.Memo.LeaveDetailsInvalid);
        (await team.ManagerClient.PostAsJsonAsync($"/api/app/memo/{withLeave}/convert", new { target = "incident", itemId = Guid.NewGuid() }))
            .StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task An_incident_walks_through_the_manager_and_hr_to_resolved()
    {
        var team = await TeamAsync();

        var incident = await ReportAsync(team.StaffClient);
        var id = IdOf(incident);
        var url = $"/api/app/incident/{id}";
        Text(incident, "status").ShouldBe("pendingManager");
        Text(incident, "currentHandlerRole").ShouldBe("departmentManager");
        (await NotificationsAsync(team.ManagerClient)).ShouldContain("incidentUpdate");

        (await CountInInboxAsync(team.ManagerClient, "incident", id)).ShouldBe(1);
        (await CountInInboxAsync(team.Kit.Hr, "incident", id)).ShouldBe(1);

        var stamp = Text(incident, "concurrencyStamp");
        await ShouldFailWithAsync(
            await team.Kit.Hr.PatchAsJsonAsync(url, new { status = "resolved", concurrencyStamp = stamp }),
            HrErrorCodes.Incident.InvalidTransition);

        var forwarded = await OkAsync(await team.ManagerClient.PostAsync($"{url}/forward", null));
        Text(forwarded, "status").ShouldBe("pendingHr");
        Text(forwarded, "currentHandlerRole").ShouldBe("hrManager");
        await ShouldFailWithAsync(await ForwardAsync(team.ManagerClient, id, "incident"), HrErrorCodes.Incident.InvalidState);

        var tasks = await team.ManagerClient.GetFromJsonAsync<JsonElement>("/api/app/me/administrative-task-log?maxResultCount=200");
        tasks.GetProperty("items").EnumerateArray().Select(x => Text(x, "taskType")).ShouldContain("incidentForwarded");

        (await team.Kit.Hr.PatchAsJsonAsync(url, new { status = "inProgress", concurrencyStamp = stamp })).StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await team.ManagerClient.PatchAsJsonAsync(url, new { status = "inProgress", concurrencyStamp = Text(forwarded, "concurrencyStamp") })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await team.Kit.Hr.PatchAsJsonAsync(url, new { status = "pendingHr", concurrencyStamp = Text(forwarded, "concurrencyStamp") })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var working = await OkAsync(await team.Kit.Hr.PatchAsJsonAsync(url, new { status = "inProgress", note = "Technician booked", concurrencyStamp = Text(forwarded, "concurrencyStamp") }));
        Text(working, "statusLabel").ShouldBe("In progress");

        await ShouldFailWithAsync(
            await team.Kit.Hr.PatchAsJsonAsync(url, new { status = "inProgress", concurrencyStamp = Text(working, "concurrencyStamp") }),
            HrErrorCodes.Incident.InvalidTransition);

        var resolved = await OkAsync(await team.Kit.Hr.PatchAsJsonAsync(url, new { status = "resolved", concurrencyStamp = Text(working, "concurrencyStamp") }));
        Text(resolved, "status").ShouldBe("resolved");
        resolved.GetProperty("currentHandlerRole").ValueKind.ShouldBe(JsonValueKind.Null);
        Actions(resolved).ShouldBe(new[] { "submitted", "forwardedToHr", "inProgress", "resolved" });

        var seen = await OkAsync(await team.StaffClient.GetAsync(url));
        seen.GetProperty("timeline").EnumerateArray().Single(x => Text(x, "action") == "inProgress").GetProperty("note").GetString().ShouldBe("Technician booked");

        // Without a status the inbox shows open incidents only.
        (await CountInInboxAsync(team.Kit.Hr, "incident", id)).ShouldBe(0);
        (await CountInInboxAsync(team.Kit.Hr, "incident", id, "&status=resolved")).ShouldBe(1);
    }

    [Fact]
    public async Task A_managers_incident_goes_straight_to_hr_and_strangers_cannot_read_incidents()
    {
        var team = await TeamAsync();

        var own = await ReportAsync(team.ManagerClient);
        Text(own, "status").ShouldBe("pendingHr");
        await ShouldFailWithAsync(await ForwardAsync(team.ManagerClient, IdOf(own), "incident"), HrErrorCodes.Incident.InvalidState);

        var id = IdOf(await ReportAsync(team.StaffClient));
        var colleague = await team.Kit.SignInAsync(await team.Kit.AddStaffAsync(team.Manager.Id));
        var otherManager = await team.Kit.SignInAsync(await team.Kit.AddDepartmentManagerAsync());

        (await colleague.GetAsync($"/api/app/incident/{id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await colleague.GetAsync("/api/app/incident/inbox")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await otherManager.GetAsync($"/api/app/incident/{id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await ForwardAsync(otherManager, id, "incident")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await CountInInboxAsync(otherManager, "incident", id)).ShouldBe(0);
        (await team.Kit.Hr.GetAsync($"/api/app/incident/{Guid.NewGuid()}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        (await team.StaffClient.PostAsJsonAsync("/api/app/incident", new { description = "x", itemId = Guid.NewGuid() })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await team.StaffClient.PostAsJsonAsync("/api/app/incident", new { description = "" })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Changing_the_manager_moves_waiting_memos_and_incidents_but_not_later_ones()
    {
        var team = await TeamAsync();
        var newManager = await team.Kit.AddDepartmentManagerAsync();
        var newManagerClient = await team.Kit.SignInAsync(newManager);

        var waitingMemo = IdOf(await WriteMemoAsync(team.StaffClient));
        var waitingIncident = IdOf(await ReportAsync(team.StaffClient));
        var movedOn = IdOf(await WriteMemoAsync(team.StaffClient));
        await OkAsync(await ForwardAsync(team.ManagerClient, movedOn));

        var stamp = (await team.Kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/employee/{team.Staff.Id}")).GetProperty("concurrencyStamp").GetString();
        var moved = await OkAsync(await team.Kit.Hr.PutAsJsonAsync($"/api/app/employee/{team.Staff.Id}/manager", new { managerEmployeeId = newManager.Id, concurrencyStamp = stamp }));
        moved.GetProperty("movedPendingItemCount").GetInt32().ShouldBe(2);

        (await CountInInboxAsync(newManagerClient, "memo", waitingMemo)).ShouldBe(1);
        (await CountInInboxAsync(newManagerClient, "memo", movedOn)).ShouldBe(0);
        (await CountInInboxAsync(newManagerClient, "incident", waitingIncident)).ShouldBe(1);

        (await CountInInboxAsync(team.ManagerClient, "memo", waitingMemo)).ShouldBe(0);
        (await CountInInboxAsync(team.ManagerClient, "incident", waitingIncident)).ShouldBe(0);
        (await ForwardAsync(team.ManagerClient, waitingIncident, "incident")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        await ShouldFailWithAsync(await ForwardAsync(team.ManagerClient, waitingMemo), HrErrorCodes.Memo.InvalidState);

        await OkAsync(await ForwardAsync(newManagerClient, waitingMemo));
        await OkAsync(await ForwardAsync(newManagerClient, waitingIncident, "incident"));
    }

    [Fact]
    public async Task A_delegate_acts_for_the_manager_only_under_the_memo_scope()
    {
        var team = await TeamAsync();
        var delegateEmployee = await team.Kit.AddStaffAsync(team.Manager.Id);
        var delegateClient = await team.Kit.SignInAsync(delegateEmployee);

        var memo = IdOf(await WriteMemoAsync(team.StaffClient));
        var incident = IdOf(await ReportAsync(team.StaffClient));
        var ownMemo = IdOf(await WriteMemoAsync(delegateClient));

        (await delegateClient.GetAsync("/api/app/memo/inbox")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        await OkAsync(
            await team.ManagerClient.PostAsJsonAsync("/api/app/delegation", new { toUserId = delegateEmployee.UserId, scope = "requests", startsAt = DateTime.UtcNow, endsAt = DateTime.UtcNow.AddHours(1), reason = "Leave" }),
            HttpStatusCode.Created);

        // A delegation for requests does not reach memos.
        (await delegateClient.GetAsync("/api/app/memo/inbox")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await delegateClient.GetAsync($"/api/app/memo/{memo}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await ForwardAsync(delegateClient, memo)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        await OkAsync(
            await team.ManagerClient.PostAsJsonAsync("/api/app/delegation", new { toUserId = delegateEmployee.UserId, scope = "memosAndIncidents", startsAt = DateTime.UtcNow.AddHours(2), endsAt = DateTime.UtcNow.AddHours(3), reason = "Later" }),
            HttpStatusCode.Created);

        // A delegation that has not started gives nothing yet.
        (await delegateClient.GetAsync($"/api/app/memo/{memo}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var (otherDelegate, otherDelegateUserId, _) = await OwnerAsync(team.Kit, team.Manager.Id, HrRoles.Employee);
        await OkAsync(
            await team.ManagerClient.PostAsJsonAsync("/api/app/delegation", new { toUserId = otherDelegateUserId, scope = "memosAndIncidents", startsAt = DateTime.UtcNow, endsAt = DateTime.UtcNow.AddHours(1), reason = "Leave" }),
            HttpStatusCode.Created);

        (await CountInInboxAsync(otherDelegate, "memo", memo)).ShouldBe(1);
        (await CountInInboxAsync(otherDelegate, "incident", incident)).ShouldBe(1);

        var summary = await otherDelegate.GetFromJsonAsync<JsonElement>("/api/app/me/inbox-summary");
        var memos = summary.GetProperty("items").EnumerateArray().Single(x => Text(x, "category") == "memo");
        memos.GetProperty("count").GetInt32().ShouldBe(memos.GetProperty("delegatedCount").GetInt32());
        var incidents = summary.GetProperty("items").EnumerateArray().Single(x => Text(x, "category") == "incident");
        incidents.GetProperty("count").GetInt32().ShouldBe(incidents.GetProperty("delegatedCount").GetInt32());

        var forwarded = await OkAsync(await ForwardAsync(otherDelegate, memo));
        forwarded.GetProperty("timeline")[1].GetProperty("actor").GetProperty("id").GetGuid().ShouldBe(otherDelegateUserId);
        await OkAsync(await ForwardAsync(otherDelegate, incident, "incident"));

        // The first action stands, and the delegate's own memo stays with the principal.
        await ShouldFailWithAsync(await ForwardAsync(team.ManagerClient, memo), HrErrorCodes.Memo.InvalidState);
        await OkAsync(await ForwardAsync(team.ManagerClient, ownMemo));
    }

    [Fact]
    public async Task The_inbox_summary_counts_what_waits_and_the_lists_filter_and_sort()
    {
        var team = await TeamAsync();
        var memo = IdOf(await WriteMemoAsync(team.StaffClient, "leave"));
        await WriteMemoAsync(team.StaffClient, "problem");
        await ReportAsync(team.StaffClient);

        var summary = await team.ManagerClient.GetFromJsonAsync<JsonElement>("/api/app/me/inbox-summary");
        var items = summary.GetProperty("items").EnumerateArray().ToList();
        items.Single(x => Text(x, "category") == "memo").GetProperty("count").GetInt32().ShouldBe(2);
        items.Single(x => Text(x, "category") == "incident").GetProperty("count").GetInt32().ShouldBe(1);
        items.Single(x => Text(x, "category") == "memo").GetProperty("delegatedCount").GetInt32().ShouldBe(0);

        var leaves = await team.ManagerClient.GetFromJsonAsync<JsonElement>($"/api/app/memo/inbox?kind=leave&employeeId={team.Staff.Id}");
        leaves.GetProperty("totalCount").GetInt64().ShouldBe(1);
        var item = leaves.GetProperty("items")[0];
        IdOf(item).ShouldBe(memo);
        Text(item, "contentPreview").ShouldBe("The printer is broken");

        (await team.ManagerClient.GetAsync("/api/app/memo/inbox?employeeId=not-a-guid")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await team.ManagerClient.GetAsync("/api/app/memo/inbox?sorting=content")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        await OkAsync(await team.ManagerClient.GetAsync("/api/app/memo/inbox?sorting=creationTime desc"));
        await OkAsync(await team.StaffClient.GetAsync("/api/app/incident/my?sorting=status"));

        var longText = new string('م', 500);
        var longMemo = await WriteMemoAsync(team.StaffClient, "problem", longText);
        var mine = await team.StaffClient.GetFromJsonAsync<JsonElement>("/api/app/memo/my?maxResultCount=200");
        Text(mine.GetProperty("items").EnumerateArray().Single(x => IdOf(x) == IdOf(longMemo)), "contentPreview").Length.ShouldBe(200);
    }

    [Fact]
    public async Task Writing_twice_with_one_key_returns_the_same_memo_and_incident()
    {
        var team = await TeamAsync();
        var key = Guid.NewGuid().ToString();

        async Task<JsonElement> SendAsync(string url, object body)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
            request.Headers.Add("Idempotency-Key", key);
            return await OkAsync(await team.StaffClient.SendAsync(request), HttpStatusCode.Created);
        }

        var memoBody = new { kind = "problem", content = "Once" };
        IdOf(await SendAsync("/api/app/memo", memoBody)).ShouldBe(IdOf(await SendAsync("/api/app/memo", memoBody)));

        var incidentBody = new { description = "Once" };
        IdOf(await SendAsync("/api/app/incident", incidentBody)).ShouldBe(IdOf(await SendAsync("/api/app/incident", incidentBody)));
    }

    [Fact]
    public async Task Approving_or_cancelling_a_request_needs_no_body()
    {
        var team = await TeamAsync();
        var types = await team.StaffClient.GetFromJsonAsync<JsonElement>("/api/app/request-type?maxResultCount=200");
        var typeId = IdOf(types.GetProperty("items").EnumerateArray().Single(x => Text(x, "code") == RequestTypeCodes.AdministrativeNeed));

        var approve = IdOf(await OkAsync(await team.StaffClient.PostAsJsonAsync("/api/app/request", new { requestTypeId = typeId, description = "A desk" }), HttpStatusCode.Created));
        var cancel = IdOf(await OkAsync(await team.StaffClient.PostAsJsonAsync("/api/app/request", new { requestTypeId = typeId, description = "A chair" }), HttpStatusCode.Created));

        await OkAsync(await team.ManagerClient.PostAsync($"/api/app/request/{approve}/approve", null));
        Text(await OkAsync(await team.StaffClient.PostAsync($"/api/app/request/{cancel}/cancel", null)), "status").ShouldBe("cancelled");
    }
}
