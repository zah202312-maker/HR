using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HR_Mangment2.Complaints;
using HR_Mangment2.Errors;
using HR_Mangment2.Identity;
using HR_Mangment2.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Shouldly;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;
using Xunit;

namespace HR_Mangment2;

/// <summary>Every refusal on a complaint must look like the complaint does not exist, whatever roles the person holds.</summary>
[Collection(HostCollection.Name)]
public class ComplaintModuleTests
{
    private const string Secret = "secret-subject-marker";
    private const string SecretBody = "secret-content-marker";

    private readonly HostFixture _host;

    public ComplaintModuleTests(HostFixture host)
    {
        _host = host;
    }

    private sealed record Team(EmployeeTestKit Kit, CreatedEmployee Manager, HttpClient ManagerClient, CreatedEmployee Staff, HttpClient StaffClient, CreatedEmployee Colleague);

    private async Task<Team> TeamAsync()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var colleague = await kit.AddStaffAsync(manager.Id);
        return new Team(kit, manager, await kit.SignInAsync(manager), staff, await kit.SignInAsync(staff), colleague);
    }

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
        var error = JsonDocument.Parse(body).RootElement.GetProperty("error");
        error.GetProperty("code").GetString().ShouldBe(code);

        // Complaint operations send no error.data. ABP writes an empty object where there is none.
        var data = error.GetProperty("data");
        if (data.ValueKind != JsonValueKind.Null)
        {
            data.EnumerateObject().ShouldBeEmpty();
        }
    }

    private static string Text(JsonElement element, string property) => element.GetProperty(property).GetString()!;

    private static Guid IdOf(JsonElement element) => element.GetProperty("id").GetGuid();

    private static string Stamp(JsonElement element) => Text(element, "concurrencyStamp");

    private static async Task<JsonElement> SubmitAsync(HttpClient client, Guid? respondent = null, string level = "normal", string subject = Secret, string content = SecretBody) =>
        await OkAsync(
            await client.PostAsJsonAsync("/api/app/complaint", new { targetKind = respondent == null ? "general" : "employee", respondentId = respondent, level, subject, content }),
            HttpStatusCode.Created);

    private static Task<HttpResponseMessage> DetailAsync(HttpClient client, Guid id) => client.GetAsync($"/api/app/complaint/{id}");

    private static async Task<string> StampForHandlerAsync(HttpClient client, Guid id) => Stamp(await OkAsync(await DetailAsync(client, id)));

    private static async Task<string> StampForOwnerAsync(HttpClient client, Guid id) => Stamp(await OkAsync(await client.GetAsync($"/api/app/complaint/my/{id}")));

    private static async Task<bool> IsAssignedAsync(HttpClient client, Guid id)
    {
        var list = await OkAsync(await client.GetAsync("/api/app/complaint/assigned?maxResultCount=200"));
        return list.GetProperty("items").EnumerateArray().Any(x => IdOf(x) == id);
    }

    private static HttpContent Pdf(string text = "%PDF-1.7 complaint evidence")
    {
        var file = new ByteArrayContent(Encoding.ASCII.GetBytes(text));
        file.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        return new MultipartFormDataContent { { file, "file", "evidence.pdf" } };
    }

    private async Task WithTenantAsync(Func<IServiceProvider, Task> work)
    {
        using var scope = _host.Factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var tenant = await services.GetRequiredService<ITenantRepository>().FindByNameAsync(
            services.GetRequiredService<ITenantNormalizer>().NormalizeName(HostFixture.TestTenantName)!);
        using var change = services.GetRequiredService<ICurrentTenant>().Change(tenant!.Id);
        using var uow = services.GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: true);
        await work(services);
        await uow.CompleteAsync();
    }

    [Theory]
    [InlineData(new string[0], ComplaintLevel.Normal, false, ComplaintRespondentCategory.Colleague)]
    [InlineData(new[] { HrRoles.Employee }, ComplaintLevel.Normal, false, ComplaintRespondentCategory.Colleague)]
    [InlineData(new[] { HrRoles.StoreOfficer }, ComplaintLevel.Normal, true, ComplaintRespondentCategory.StoreOfficer)]
    [InlineData(new[] { HrRoles.Accountant }, ComplaintLevel.Normal, true, ComplaintRespondentCategory.Accountant)]
    [InlineData(new[] { HrRoles.DepartmentManager }, ComplaintLevel.Confidential, true, ComplaintRespondentCategory.DepartmentManager)]
    [InlineData(new[] { HrRoles.Reviewer }, ComplaintLevel.HighlyConfidential, true, ComplaintRespondentCategory.Reviewer)]
    [InlineData(new[] { HrRoles.HrManager }, ComplaintLevel.HighlyConfidential, true, ComplaintRespondentCategory.HrManager)]
    [InlineData(new[] { HrRoles.StoreOfficer, HrRoles.DepartmentManager }, ComplaintLevel.Confidential, true, ComplaintRespondentCategory.DepartmentManager)]
    [InlineData(new[] { HrRoles.DepartmentManager, HrRoles.Reviewer }, ComplaintLevel.HighlyConfidential, true, ComplaintRespondentCategory.Reviewer)]
    public void The_routing_table_applies_the_strictest_rule(string[] roles, ComplaintLevel minimum, bool skips, ComplaintRespondentCategory category)
    {
        var profile = ComplaintRoutingService.Classify(roles)!;
        profile.MinimumLevel.ShouldBe(minimum);
        profile.SkipsDepartmentManager.ShouldBe(skips);
        profile.Category.ShouldBe(category);
    }

    [Theory]
    [InlineData(HrRoles.ExecutiveDirector)]
    [InlineData(HrRoles.SystemAdmin)]
    public void Some_people_can_never_be_named_even_with_another_role(string role)
    {
        ComplaintRoutingService.Classify(new[] { role }).ShouldBeNull();
        ComplaintRoutingService.Classify(new[] { HrRoles.Employee, HrRoles.DepartmentManager, role }).ShouldBeNull();
    }

    [Fact]
    public void Each_level_has_one_route()
    {
        ComplaintRoutingService.RouteFor(ComplaintLevel.Normal, true).ShouldBe(new[] { ComplaintHandlerRole.DepartmentManager, ComplaintHandlerRole.HrManager });
        ComplaintRoutingService.RouteFor(ComplaintLevel.Normal, false).ShouldBe(new[] { ComplaintHandlerRole.HrManager });
        ComplaintRoutingService.RouteFor(ComplaintLevel.Confidential, true).ShouldBe(new[] { ComplaintHandlerRole.HrManager });
        ComplaintRoutingService.RouteFor(ComplaintLevel.HighlyConfidential, true).ShouldBe(new[] { ComplaintHandlerRole.ExecutiveDirector });
    }

    [Fact]
    public void Content_is_bound_to_its_place()
    {
        var key = ComplaintCipher.NewKey();
        var sealedValue = ComplaintCipher.Seal(key, Encoding.UTF8.GetBytes("hello"), "a");

        Encoding.UTF8.GetString(ComplaintCipher.Open(key, sealedValue, "a")).ShouldBe("hello");
        Should.Throw<Exception>(() => ComplaintCipher.Open(key, sealedValue, "b"));
        Should.Throw<Exception>(() => ComplaintCipher.Open(ComplaintCipher.NewKey(), sealedValue, "a"));
    }

    [Fact]
    public async Task A_normal_complaint_walks_through_the_manager_to_hr_and_closes()
    {
        var team = await TeamAsync();

        var submitted = await SubmitAsync(team.StaffClient, team.Colleague.Id);
        var id = IdOf(submitted);
        Text(submitted, "status").ShouldBe("pendingDepartmentManager");
        Text(submitted, "level").ShouldBe("normal");
        submitted.GetProperty("levelRaisedBySystem").GetBoolean().ShouldBeFalse();
        Text(submitted, "referenceNumber").ShouldStartWith("C-");

        // The handler is told, with nothing but the generic text and an opaque link.
        var notifications = await team.ManagerClient.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200");
        var notice = notifications.GetProperty("items").EnumerateArray().First(x => Text(x, "templateCode") == "secureItemUpdateForHandler");
        notice.GetRawText().ShouldNotContain(Secret);
        Text(notice, "resourceType").ShouldBe("secureItem");

        (await IsAssignedAsync(team.ManagerClient, id)).ShouldBeTrue();
        (await IsAssignedAsync(team.Kit.Hr, id)).ShouldBeFalse();
        (await DetailAsync(team.Kit.Hr, id)).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var detail = await OkAsync(await DetailAsync(team.ManagerClient, id));
        Text(detail, "subject").ShouldBe(Secret);
        Text(detail, "content").ShouldBe(SecretBody);
        detail.GetProperty("complainant").GetProperty("id").GetGuid().ShouldBe(team.Staff.Id);
        detail.GetProperty("respondent").GetProperty("id").GetGuid().ShouldBe(team.Colleague.Id);
        Text(detail, "respondentCategory").ShouldBe("colleague");
        Text(detail, "minimumLevel").ShouldBe("normal");
        detail.GetProperty("handler").GetProperty("id").GetGuid().ShouldBe(team.Manager.UserId);
        detail.GetProperty("isLate").GetBoolean().ShouldBeFalse();
        detail.GetProperty("dueAt").ValueKind.ShouldBe(JsonValueKind.String);
        detail.GetProperty("actions").EnumerateArray().Select(x => Text(x, "type")).ShouldBe(new[] { "submitted" });

        var action = await OkAsync(
            await team.ManagerClient.PostAsJsonAsync($"/api/app/complaint/{id}/action", new { type = "meeting", note = "Met both sides" }),
            HttpStatusCode.Created);
        Text(action, "note").ShouldBe("Met both sides");
        (await team.ManagerClient.PostAsJsonAsync($"/api/app/complaint/{id}/action", new { type = "closed", note = "x" })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // A department manager does not close it.
        (await team.ManagerClient.PostAsJsonAsync($"/api/app/complaint/{id}/close", new { decision = "x", concurrencyStamp = Stamp(detail) })).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        (await team.ManagerClient.PostAsJsonAsync($"/api/app/complaint/{id}/forward", new { concurrencyStamp = "stale" })).StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var forwarded = await OkAsync(await team.ManagerClient.PostAsJsonAsync($"/api/app/complaint/{id}/forward", new { note = "For HR", concurrencyStamp = Stamp(detail) }));
        Text(forwarded, "status").ShouldBe("pendingHrManager");
        forwarded.GetProperty("callerRetainsAccess").GetBoolean().ShouldBeFalse();

        // Access ends at once, including for a retry.
        (await DetailAsync(team.ManagerClient, id)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await IsAssignedAsync(team.ManagerClient, id)).ShouldBeFalse();
        (await team.ManagerClient.PostAsJsonAsync($"/api/app/complaint/{id}/forward", new { concurrencyStamp = Stamp(forwarded) })).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        (await IsAssignedAsync(team.Kit.Hr, id)).ShouldBeTrue();
        var atHr = await OkAsync(await DetailAsync(team.Kit.Hr, id));
        atHr.GetProperty("actions").EnumerateArray().Select(x => Text(x, "type")).ShouldBe(new[] { "submitted", "meeting", "forwarded" });
        await ShouldFailWithAsync(
            await team.Kit.Hr.PostAsJsonAsync($"/api/app/complaint/{id}/forward", new { concurrencyStamp = Stamp(atHr) }),
            HrErrorCodes.Complaints.InvalidTransition);

        var closed = await OkAsync(await team.Kit.Hr.PostAsJsonAsync($"/api/app/complaint/{id}/close", new { decision = "Warned verbally", concurrencyStamp = Stamp(atHr) }));

        // D22: the complainant sees the final decision.
        Text(await OkAsync(await team.StaffClient.GetAsync($"/api/app/complaint/my/{id}")), "closureDecision").ShouldBe("Warned verbally");

        Text(closed, "status").ShouldBe("closed");
        closed.GetProperty("closedAt").ValueKind.ShouldBe(JsonValueKind.String);
        closed.GetProperty("callerRetainsAccess").GetBoolean().ShouldBeTrue();

        Text(await OkAsync(await DetailAsync(team.Kit.Hr, id)), "closureDecision").ShouldBe("Warned verbally");
        (await NotificationsOfAsync(team.StaffClient)).ShouldContain("secureItemUpdateForOwner");

        await ShouldFailWithAsync(
            await team.Kit.Hr.PostAsJsonAsync($"/api/app/complaint/{id}/action", new { type = "note", note = "late" }),
            HrErrorCodes.Complaints.ComplaintClosed);
        await ShouldFailWithAsync(
            await team.Kit.Hr.PostAsJsonAsync($"/api/app/complaint/{id}/close", new { decision = "x", concurrencyStamp = Stamp(closed) }),
            HrErrorCodes.Complaints.ComplaintClosed);
        await ShouldFailWithAsync(
            await team.StaffClient.PostAsync($"/api/app/complaint/{id}/attachment", Pdf()),
            HrErrorCodes.Complaints.ComplaintClosed);
        await ShouldFailWithAsync(
            await team.StaffClient.PostAsJsonAsync($"/api/app/complaint/{id}/raise-level", new { level = "confidential", concurrencyStamp = Stamp(closed) }),
            HrErrorCodes.Complaints.ComplaintClosed);

        var mine = await OkAsync(await team.StaffClient.GetAsync($"/api/app/complaint/my/{id}"));
        Text(mine, "subject").ShouldBe(Secret);

        // The complainant's view is a different shape: handling details are absent, not null.
        mine.TryGetProperty("actions", out _).ShouldBeFalse();
        mine.TryGetProperty("handler", out _).ShouldBeFalse();
        mine.TryGetProperty("minimumLevel", out _).ShouldBeFalse();
        mine.TryGetProperty("complainant", out _).ShouldBeFalse();
    }

    private static async Task<string[]> NotificationsOfAsync(HttpClient client)
    {
        var list = await client.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200");
        return list.GetProperty("items").EnumerateArray().Select(x => Text(x, "templateCode")).ToArray();
    }

    [Fact]
    public async Task The_respondent_and_strangers_get_404_whatever_roles_they_hold()
    {
        var team = await TeamAsync();
        var (admin, _, _) = await OwnerAsync(team.Kit, team.Kit.ExecutiveEmployeeId, HrRoles.Employee, HrRoles.SystemAdmin);
        var (adminWithHr, _, _) = await OwnerAsync(team.Kit, team.Kit.ExecutiveEmployeeId, HrRoles.Employee, HrRoles.SystemAdmin, HrRoles.HrManager);
        var (executive, _, _) = await OwnerAsync(team.Kit, null, HrRoles.ExecutiveDirector);
        var (reviewer, _, _) = await OwnerAsync(team.Kit, team.Kit.ExecutiveEmployeeId, HrRoles.Employee, HrRoles.Reviewer);
        var colleague = await team.Kit.SignInAsync(team.Colleague);
        var outsider = await team.Kit.SignInAsync(await team.Kit.AddDepartmentManagerAsync());

        var submitted = await SubmitAsync(team.StaffClient, team.Colleague.Id, "confidential");
        var id = IdOf(submitted);
        Text(submitted, "status").ShouldBe("pendingHrManager");

        foreach (var client in new[] { colleague, outsider, team.ManagerClient, admin, adminWithHr, executive, reviewer })
        {
            (await DetailAsync(client, id)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
            (await client.GetAsync($"/api/app/complaint/my/{id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
            (await client.GetAsync($"/api/app/complaint/{id}/access-log")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
            (await client.PostAsJsonAsync($"/api/app/complaint/{id}/action", new { type = "note", note = "x" })).StatusCode.ShouldBe(HttpStatusCode.NotFound);
            (await client.PostAsJsonAsync($"/api/app/complaint/{id}/raise-level", new { level = "highlyConfidential", concurrencyStamp = "x" })).StatusCode.ShouldBe(HttpStatusCode.NotFound);
            (await client.PostAsJsonAsync($"/api/app/complaint/{id}/forward", new { concurrencyStamp = "x" })).StatusCode.ShouldBe(HttpStatusCode.NotFound);
            (await client.PostAsJsonAsync($"/api/app/complaint/{id}/close", new { decision = "x", concurrencyStamp = "x" })).StatusCode.ShouldBe(HttpStatusCode.NotFound);
            (await client.PostAsync($"/api/app/complaint/{id}/attachment", Pdf())).StatusCode.ShouldBe(HttpStatusCode.NotFound);
            (await client.GetAsync($"/api/app/complaint/{id}/attachment/{Guid.NewGuid()}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

            // An empty list, not 403, and never this complaint.
            (await IsAssignedAsync(client, id)).ShouldBeFalse();
            (await OkAsync(await client.GetAsync("/api/app/complaint/my"))).GetProperty("items").EnumerateArray().ShouldNotContain(x => IdOf(x) == id);
        }

        // The complainant uses their own path, even if they hold a handler role.
        var (hrComplainant, _, _) = await OwnerAsync(team.Kit, team.Kit.ExecutiveEmployeeId, HrRoles.Employee, HrRoles.HrManager);
        var own = IdOf(await SubmitAsync(hrComplainant));
        (await DetailAsync(hrComplainant, own)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        await OkAsync(await hrComplainant.GetAsync($"/api/app/complaint/my/{own}"));
        (await IsAssignedAsync(hrComplainant, own)).ShouldBeFalse();
    }

    [Fact]
    public async Task The_level_rises_to_the_respondents_minimum_and_the_route_follows()
    {
        var team = await TeamAsync();
        var (hrRespondentClient, _, hrRespondent) = await OwnerAsync(team.Kit, team.Kit.ExecutiveEmployeeId, HrRoles.Employee, HrRoles.HrManager);
        var (_, _, reviewerRespondent) = await OwnerAsync(team.Kit, team.Kit.ExecutiveEmployeeId, HrRoles.Employee, HrRoles.Reviewer);
        var (_, _, storeRespondent) = await OwnerAsync(team.Kit, team.Manager.Id, HrRoles.Employee, HrRoles.StoreOfficer);
        var (_, _, accountantRespondent) = await OwnerAsync(team.Kit, team.Manager.Id, HrRoles.Employee, HrRoles.Accountant);
        var (executive, _, _) = await OwnerAsync(team.Kit, null, HrRoles.ExecutiveDirector);

        // Against the department manager: at least confidential, and straight to HR.
        var againstManager = await SubmitAsync(team.StaffClient, team.Manager.Id);
        Text(againstManager, "level").ShouldBe("confidential");
        againstManager.GetProperty("levelRaisedBySystem").GetBoolean().ShouldBeTrue();
        Text(againstManager, "status").ShouldBe("pendingHrManager");
        (await DetailAsync(team.ManagerClient, IdOf(againstManager))).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await IsAssignedAsync(team.ManagerClient, IdOf(againstManager))).ShouldBeFalse();

        // Against HR: forced to the highest level, and the HR manager named cannot see it, nor can any HR holder.
        var againstHr = await SubmitAsync(team.StaffClient, hrRespondent);
        Text(againstHr, "level").ShouldBe("highlyConfidential");
        Text(againstHr, "status").ShouldBe("pendingExecutiveDirector");
        (await DetailAsync(hrRespondentClient, IdOf(againstHr))).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await DetailAsync(team.Kit.Hr, IdOf(againstHr))).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        Text(await OkAsync(await DetailAsync(executive, IdOf(againstHr))), "respondentCategory").ShouldBe("hrManager");

        var againstReviewer = await SubmitAsync(team.StaffClient, reviewerRespondent);
        Text(againstReviewer, "level").ShouldBe("highlyConfidential");

        // Store officer and accountant: by choice, but past the department manager.
        var againstStore = await SubmitAsync(team.StaffClient, storeRespondent);
        Text(againstStore, "level").ShouldBe("normal");
        Text(againstStore, "status").ShouldBe("pendingHrManager");
        Text(await SubmitAsync(team.StaffClient, accountantRespondent), "status").ShouldBe("pendingHrManager");
        Text(await SubmitAsync(team.StaffClient, storeRespondent, "highlyConfidential"), "status").ShouldBe("pendingExecutiveDirector");

        // A department manager's complaint passes no manager.
        Text(await SubmitAsync(team.ManagerClient, team.Staff.Id), "status").ShouldBe("pendingHrManager");

        // The executive only ever sees the highest level, not even as a count.
        var assigned = await OkAsync(await executive.GetAsync("/api/app/complaint/assigned?maxResultCount=200&level=normal"));
        assigned.GetProperty("totalCount").GetInt64().ShouldBe(0);
        (await DetailAsync(executive, IdOf(againstStore))).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // The server decides the level on preview too, with no data created.
        var preview = await OkAsync(await team.StaffClient.PostAsJsonAsync("/api/app/complaint/preview-routing", new { targetKind = "employee", respondentId = team.Manager.Id }));
        Text(preview, "minimumLevel").ShouldBe("confidential");
        preview.GetProperty("selectableLevels").EnumerateArray().Select(x => x.GetString()).ShouldBe(new[] { "confidential", "highlyConfidential" });
        preview.GetProperty("routes")[0].GetProperty("handlerPath").EnumerateArray().Select(x => x.GetString()).ShouldBe(new[] { "hrManager" });
        preview.GetProperty("routes")[1].GetProperty("handlerPath").EnumerateArray().Select(x => x.GetString()).ShouldBe(new[] { "executiveDirector" });

        var general = await OkAsync(await team.StaffClient.PostAsJsonAsync("/api/app/complaint/preview-routing", new { targetKind = "general" }));
        general.GetProperty("routes")[0].GetProperty("handlerPath").EnumerateArray().Select(x => x.GetString()).ShouldBe(new[] { "departmentManager", "hrManager" });

        var fromManager = await OkAsync(await team.ManagerClient.PostAsJsonAsync("/api/app/complaint/preview-routing", new { targetKind = "general" }));
        fromManager.GetProperty("routes")[0].GetProperty("handlerPath").EnumerateArray().Select(x => x.GetString()).ShouldBe(new[] { "hrManager" });
    }

    [Fact]
    public async Task Forbidden_targets_are_refused_and_never_listed()
    {
        var team = await TeamAsync();
        var (_, _, adminEmployee) = await OwnerAsync(team.Kit, team.Kit.ExecutiveEmployeeId, HrRoles.Employee, HrRoles.SystemAdmin);
        var (_, _, adminAndManager) = await OwnerAsync(team.Kit, team.Kit.ExecutiveEmployeeId, HrRoles.DepartmentManager, HrRoles.SystemAdmin);

        var body = (object respondent) => new { targetKind = "employee", respondentId = respondent, level = "normal", subject = "s", content = "c" };

        foreach (var target in new[] { team.Kit.ExecutiveEmployeeId, adminEmployee, adminAndManager, Guid.NewGuid() })
        {
            await ShouldFailWithAsync(await team.StaffClient.PostAsJsonAsync("/api/app/complaint", body(target)), HrErrorCodes.Complaints.RespondentNotAllowed);
            await ShouldFailWithAsync(await team.StaffClient.PostAsJsonAsync("/api/app/complaint/preview-routing", new { targetKind = "employee", respondentId = target }), HrErrorCodes.Complaints.RespondentNotAllowed);
        }

        await ShouldFailWithAsync(await team.StaffClient.PostAsJsonAsync("/api/app/complaint", body(team.Staff.Id)), HrErrorCodes.Complaints.SelfComplaintNotAllowed);
        await ShouldFailWithAsync(
            await team.StaffClient.PostAsJsonAsync("/api/app/complaint", new { targetKind = "employee", level = "normal", subject = "s", content = "c" }),
            HrErrorCodes.Complaints.RespondentRequired);
        (await team.StaffClient.PostAsJsonAsync("/api/app/complaint", new { targetKind = "general", respondentId = team.Colleague.Id, level = "normal", subject = "s", content = "c" }))
            .StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await team.StaffClient.PostAsJsonAsync("/api/app/complaint", new { targetKind = "general", level = "normal", subject = new string('s', 201), content = "c" }))
            .StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await team.StaffClient.PostAsJsonAsync("/api/app/complaint", new { targetKind = "general", level = "normal", subject = "s", content = "" }))
            .StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Every page, because the shared test database can hold more people than one page, depending on the test order.
        var list = await OkAsync(await team.StaffClient.GetAsync("/api/app/complaint/respondents?maxResultCount=200&filter=Staff"));
        var ids = new System.Collections.Generic.List<Guid>();
        for (var skip = 0; skip < list.GetProperty("totalCount").GetInt64(); skip += 200)
        {
            var page = await OkAsync(await team.StaffClient.GetAsync($"/api/app/complaint/respondents?maxResultCount=200&filter=Staff&skipCount={skip}"));
            ids.AddRange(page.GetProperty("items").EnumerateArray().Select(IdOf));
        }

        ids.ShouldContain(team.Colleague.Id);
        ids.ShouldNotContain(team.Staff.Id);
        ids.ShouldNotContain(team.Kit.ExecutiveEmployeeId);
        ids.ShouldNotContain(adminEmployee);

        var all = await OkAsync(await team.StaffClient.GetAsync("/api/app/complaint/respondents?maxResultCount=200"));
        all.GetProperty("items").EnumerateArray().Select(IdOf).ShouldNotContain(team.Kit.SystemAdminEmployeeId);

        // The list shows a name, a department and the account photo (decision 2026-11-11), with no role or category.
        var first = list.GetProperty("items")[0];
        first.EnumerateObject().Select(x => x.Name).ShouldBeSubsetOf(new[] { "id", "fullName", "departmentName", "photoId" });

        var (executive, _, _) = await OwnerAsync(team.Kit, null, HrRoles.Employee, HrRoles.ExecutiveDirector);
        (await executive.PostAsJsonAsync("/api/app/complaint", new { targetKind = "general", level = "normal", subject = "s", content = "c" })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task The_hr_managers_own_complaint_goes_to_the_executive()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var (hr, _, _) = await OwnerAsync(kit, kit.ExecutiveEmployeeId, HrRoles.Employee, HrRoles.HrManager);
        var (executive, _, _) = await OwnerAsync(kit, null, HrRoles.ExecutiveDirector);

        var submitted = await SubmitAsync(hr);
        Text(submitted, "level").ShouldBe("highlyConfidential");
        Text(submitted, "status").ShouldBe("pendingExecutiveDirector");
        await OkAsync(await DetailAsync(executive, IdOf(submitted)));
    }

    [Fact]
    public async Task Raising_the_level_moves_it_and_ends_the_old_handlers_access()
    {
        var team = await TeamAsync();
        var (executive, _, _) = await OwnerAsync(team.Kit, null, HrRoles.ExecutiveDirector);

        var id = IdOf(await SubmitAsync(team.StaffClient));
        var stamp = await StampForHandlerAsync(team.ManagerClient, id);

        await ShouldFailWithAsync(
            await team.ManagerClient.PostAsJsonAsync($"/api/app/complaint/{id}/raise-level", new { level = "normal", concurrencyStamp = stamp }),
            HrErrorCodes.Complaints.LevelNotHigher);
        (await team.ManagerClient.PostAsJsonAsync($"/api/app/complaint/{id}/raise-level", new { level = "confidential", concurrencyStamp = "stale" })).StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var raised = await OkAsync(await team.ManagerClient.PostAsJsonAsync($"/api/app/complaint/{id}/raise-level", new { level = "confidential", reason = "Sensitive", concurrencyStamp = stamp }));
        Text(raised, "level").ShouldBe("confidential");
        Text(raised, "status").ShouldBe("pendingHrManager");
        raised.GetProperty("callerRetainsAccess").GetBoolean().ShouldBeFalse();
        (await DetailAsync(team.ManagerClient, id)).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var atHr = await OkAsync(await DetailAsync(team.Kit.Hr, id));
        var levelRaised = atHr.GetProperty("actions").EnumerateArray().Single(x => Text(x, "type") == "levelRaised");
        Text(levelRaised, "note").ShouldBe("Sensitive");

        // The complainant raises too, and keeps their access while HR loses it.
        var byOwner = await OkAsync(await team.StaffClient.PostAsJsonAsync($"/api/app/complaint/{id}/raise-level", new { level = "highlyConfidential", concurrencyStamp = Stamp(atHr) }));
        Text(byOwner, "status").ShouldBe("pendingExecutiveDirector");
        byOwner.GetProperty("callerRetainsAccess").GetBoolean().ShouldBeTrue();
        (await DetailAsync(team.Kit.Hr, id)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        await OkAsync(await team.StaffClient.GetAsync($"/api/app/complaint/my/{id}"));

        var detail = await OkAsync(await DetailAsync(executive, id));
        var closed = await OkAsync(await executive.PostAsJsonAsync($"/api/app/complaint/{id}/close", new { decision = "Done", concurrencyStamp = Stamp(detail) }));
        Text(closed, "status").ShouldBe("closed");
    }

    [Fact]
    public async Task Every_read_and_action_is_on_the_access_log_and_only_handlers_see_it()
    {
        var team = await TeamAsync();
        var id = IdOf(await SubmitAsync(team.StaffClient));

        await OkAsync(await team.StaffClient.GetAsync($"/api/app/complaint/my/{id}"));
        await OkAsync(await DetailAsync(team.ManagerClient, id));
        await OkAsync(await team.ManagerClient.PostAsJsonAsync($"/api/app/complaint/{id}/action", new { type = "note", note = "n" }), HttpStatusCode.Created);

        var log = await OkAsync(await team.ManagerClient.GetAsync($"/api/app/complaint/{id}/access-log"));
        var actions = log.GetProperty("items").EnumerateArray().Select(x => Text(x, "action")).ToList();

        // Newest first, and reading the log is recorded as well.
        actions[0].ShouldBe("viewAccessLog");
        actions.ShouldContain("addAction");
        actions.Count(x => x == "viewDetail").ShouldBe(2);
        log.GetRawText().ShouldNotContain(Secret);

        (await team.StaffClient.GetAsync($"/api/app/complaint/{id}/access-log")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Content_reaches_neither_the_tables_in_clear_nor_the_general_audit_log()
    {
        var team = await TeamAsync();
        var marker = "audit-marker-" + Guid.NewGuid().ToString("N");
        var id = IdOf(await SubmitAsync(team.StaffClient, subject: marker, content: marker));
        await OkAsync(await DetailAsync(team.ManagerClient, id));
        await OkAsync(await team.ManagerClient.PostAsJsonAsync($"/api/app/complaint/{id}/action", new { type = "note", note = marker }), HttpStatusCode.Created);

        await using var connection = new NpgsqlConnection(_host.ConnectionString);
        await connection.OpenAsync();

        foreach (var sql in new[]
                 {
                     "SELECT count(*) FROM \"AbpAuditLogs\" WHERE \"Url\" LIKE '%/api/app/complaint/%' AND \"Url\" NOT LIKE '%complaint-handler%'",
                     "SELECT count(*) FROM \"AbpAuditLogActions\" WHERE \"Parameters\" LIKE '%" + marker + "%' OR \"ServiceName\" LIKE '%ComplaintAppService%'",
                     "SELECT count(*) FROM \"AbpEntityChanges\" WHERE \"EntityTypeFullName\" LIKE '%.Complaints.Complaint' OR \"EntityTypeFullName\" LIKE '%.Complaints.ComplaintAction'",
                     "SELECT count(*) FROM complaints.\"Complaints\" WHERE encode(\"EncryptedContent\", 'escape') LIKE '%" + marker + "%' OR encode(\"EncryptedSubject\", 'escape') LIKE '%" + marker + "%'",
                     "SELECT count(*) FROM complaints.\"ComplaintActions\" WHERE encode(\"EncryptedNote\", 'escape') LIKE '%" + marker + "%'",
                     "SELECT count(*) FROM \"AppNotifications\" n WHERE n::text LIKE '%" + marker + "%'"
                 })
        {
            await using var command = new NpgsqlCommand(sql, connection);
            ((long)(await command.ExecuteScalarAsync())!).ShouldBe(0, sql);
        }
    }

    [Fact]
    public async Task Attachments_are_sealed_and_a_complainant_reaches_only_their_own()
    {
        var team = await TeamAsync();
        var id = IdOf(await SubmitAsync(team.StaffClient));

        var ownUpload = await OkAsync(await team.StaffClient.PostAsync($"/api/app/complaint/{id}/attachment", Pdf("%PDF-1.7 from the complainant")), HttpStatusCode.Created);
        Text(ownUpload, "fileName").ShouldBe("evidence.pdf");
        var handlerUpload = await OkAsync(await team.ManagerClient.PostAsync($"/api/app/complaint/{id}/attachment", Pdf("%PDF-1.7 from the handler")), HttpStatusCode.Created);

        var mine = await OkAsync(await team.StaffClient.GetAsync($"/api/app/complaint/my/{id}"));
        mine.GetProperty("attachments").EnumerateArray().Select(IdOf).ShouldBe(new[] { IdOf(ownUpload) });

        var detail = await OkAsync(await DetailAsync(team.ManagerClient, id));
        detail.GetProperty("attachments").GetArrayLength().ShouldBe(2);

        var download = await team.StaffClient.GetAsync($"/api/app/complaint/{id}/attachment/{IdOf(ownUpload)}");
        download.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await download.Content.ReadAsStringAsync()).ShouldBe("%PDF-1.7 from the complainant");
        download.Headers.CacheControl!.NoStore.ShouldBeTrue();

        (await team.StaffClient.GetAsync($"/api/app/complaint/{id}/attachment/{IdOf(handlerUpload)}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await (await team.ManagerClient.GetAsync($"/api/app/complaint/{id}/attachment/{IdOf(ownUpload)}")).Content.ReadAsStringAsync()).ShouldBe("%PDF-1.7 from the complainant");

        // An attachment of another complaint is not reachable through this one.
        var other = IdOf(await SubmitAsync(team.StaffClient));
        (await team.StaffClient.GetAsync($"/api/app/complaint/{other}/attachment/{IdOf(ownUpload)}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        await ShouldFailWithAsync(await team.StaffClient.PostAsync($"/api/app/complaint/{id}/attachment", Pdf("not a pdf")), HrErrorCodes.Document.FileTypeNotAllowed);
        await ShouldFailWithAsync(
            await team.StaffClient.PostAsync($"/api/app/complaint/{id}/attachment", Pdf("%PDF-X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*")),
            HrErrorCodes.Complaints.AttachmentRejected);

        var log = await OkAsync(await team.ManagerClient.GetAsync($"/api/app/complaint/{id}/access-log?maxResultCount=200"));
        var actions = log.GetProperty("items").EnumerateArray().Select(x => Text(x, "action")).ToList();
        actions.ShouldContain("uploadAttachment");
        actions.ShouldContain("downloadAttachment");

        await using var connection = new NpgsqlConnection(_host.ConnectionString);
        await connection.OpenAsync();
        foreach (var sql in new[]
                 {
                     "SELECT count(*) FROM complaints.\"ComplaintAttachments\" WHERE encode(\"EncryptedFileName\", 'escape') LIKE '%evidence%'",
                     "SELECT count(*) FROM \"AbpBlobs\" WHERE encode(\"Content\", 'escape') LIKE '%from the complainant%'"
                 })
        {
            await using var command = new NpgsqlCommand(sql, connection);
            ((long)(await command.ExecuteScalarAsync())!).ShouldBe(0, sql);
        }
    }

    [Fact]
    public async Task Changing_the_manager_sends_a_waiting_complaint_to_hr_without_counting_it()
    {
        var team = await TeamAsync();
        var newManager = await team.Kit.AddDepartmentManagerAsync();
        var newManagerClient = await team.Kit.SignInAsync(newManager);

        var id = IdOf(await SubmitAsync(team.StaffClient));

        var stamp = (await team.Kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/employee/{team.Staff.Id}")).GetProperty("concurrencyStamp").GetString();
        var moved = await OkAsync(await team.Kit.Hr.PutAsJsonAsync($"/api/app/employee/{team.Staff.Id}/manager", new { managerEmployeeId = newManager.Id, concurrencyStamp = stamp }));
        moved.GetProperty("movedPendingItemCount").GetInt32().ShouldBe(0);

        (await DetailAsync(team.ManagerClient, id)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await DetailAsync(newManagerClient, id)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        Text(await OkAsync(await DetailAsync(team.Kit.Hr, id)), "status").ShouldBe("pendingHrManager");
    }

    [Fact]
    public async Task A_delegation_gives_no_access_to_complaints()
    {
        var team = await TeamAsync();
        var delegateEmployee = await team.Kit.AddStaffAsync(team.Manager.Id);
        var delegateClient = await team.Kit.SignInAsync(delegateEmployee);
        var id = IdOf(await SubmitAsync(team.StaffClient));

        await OkAsync(
            await team.ManagerClient.PostAsJsonAsync("/api/app/delegation", new { toUserId = delegateEmployee.UserId, scope = "allApprovals", startsAt = DateTime.UtcNow, endsAt = DateTime.UtcNow.AddHours(1), reason = "Leave" }),
            HttpStatusCode.Created);

        (await DetailAsync(delegateClient, id)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await IsAssignedAsync(delegateClient, id)).ShouldBeFalse();
    }

    [Fact]
    public async Task A_handler_assignment_needs_the_executive_and_gives_access_only_for_its_period()
    {
        var team = await TeamAsync();
        var (executive, _, _) = await OwnerAsync(team.Kit, null, HrRoles.ExecutiveDirector);
        var (assignee, assigneeUser, assigneeEmployee) = await OwnerAsync(team.Kit, team.Manager.Id, HrRoles.Employee);
        var (_, adminUser, _) = await OwnerAsync(team.Kit, team.Kit.ExecutiveEmployeeId, HrRoles.Employee, HrRoles.SystemAdmin);
        var (_, reviewerUser, _) = await OwnerAsync(team.Kit, team.Kit.ExecutiveEmployeeId, HrRoles.Employee, HrRoles.Reviewer);

        var body = (Guid user, string level) => new { userId = user, level, from = DateTime.UtcNow, reason = "Cover" };

        (await team.ManagerClient.PostAsJsonAsync("/api/app/complaint-handler", body(assigneeUser, "confidential"))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        await ShouldFailWithAsync(await team.Kit.Hr.PostAsJsonAsync("/api/app/complaint-handler", body(adminUser, "normal")), HrErrorCodes.Complaints.HandlerCannotBeSystemAdmin);
        await ShouldFailWithAsync(await team.Kit.Hr.PostAsJsonAsync("/api/app/complaint-handler", body(reviewerUser, "normal")), HrErrorCodes.Complaints.HandlerRoleNotAllowed);
        await ShouldFailWithAsync(await team.Kit.Hr.PostAsJsonAsync("/api/app/complaint-handler", body(team.Manager.UserId, "confidential")), HrErrorCodes.Complaints.HandlerRoleNotAllowed);
        await ShouldFailWithAsync(
            await team.Kit.Hr.PostAsJsonAsync("/api/app/complaint-handler", new { userId = assigneeUser, level = "confidential", from = DateTime.UtcNow, to = DateTime.UtcNow.AddDays(-1), reason = "Cover" }),
            HrErrorCodes.Complaints.InvalidAssignmentPeriod);

        var complaint = IdOf(await SubmitAsync(team.StaffClient, team.Colleague.Id, "confidential"));

        var requested = await OkAsync(await team.Kit.Hr.PostAsJsonAsync("/api/app/complaint-handler", body(assigneeUser, "confidential")), HttpStatusCode.Created);
        Text(requested, "status").ShouldBe("pendingApproval");
        var url = $"/api/app/complaint-handler/{IdOf(requested)}";

        // Nothing until the executive approves, and only the executive can.
        (await DetailAsync(assignee, complaint)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await team.Kit.Hr.PostAsJsonAsync($"{url}/approve", new { concurrencyStamp = Stamp(requested) })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        await ShouldFailWithAsync(await executive.PostAsJsonAsync($"{url}/revoke", new { reason = "x", concurrencyStamp = Stamp(requested) }), HrErrorCodes.Complaints.AssignmentNotActive);

        (await executive.GetFromJsonAsync<JsonElement>("/api/app/complaint-handler?status=pendingApproval&maxResultCount=200"))
            .GetProperty("items").EnumerateArray().Select(IdOf).ShouldContain(IdOf(requested));
        (await NotificationsOfAsync(executive)).ShouldContain("secureHandlerAssignmentUpdate");

        var approved = await OkAsync(await executive.PostAsJsonAsync($"{url}/approve", new { concurrencyStamp = Stamp(requested) }));
        Text(approved, "status").ShouldBe("approved");
        approved.GetProperty("decidedBy").ValueKind.ShouldBe(JsonValueKind.Object);
        await ShouldFailWithAsync(await executive.PostAsJsonAsync($"{url}/approve", new { concurrencyStamp = Stamp(approved) }), HrErrorCodes.Complaints.AssignmentNotPending);
        await ShouldFailWithAsync(await executive.PostAsJsonAsync($"{url}/reject", new { reason = "x", concurrencyStamp = Stamp(approved) }), HrErrorCodes.Complaints.AssignmentNotPending);

        // Approval makes two-step verification mandatory before anything else works.
        (await assignee.GetAsync("/api/app/complaint/assigned")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var enrollment = await OkAsync(await assignee.PostAsync("/api/app/account/mfa/enroll", null));
        (await assignee.PostAsJsonAsync("/api/app/account/mfa/confirm", new { code = Totp.Compute(Text(enrollment, "sharedKey")) })).EnsureSuccessStatusCode();

        (await IsAssignedAsync(assignee, complaint)).ShouldBeTrue();
        await OkAsync(await DetailAsync(assignee, complaint));
        await OkAsync(await assignee.PostAsJsonAsync($"/api/app/complaint/{complaint}/action", new { type = "note", note = "n" }), HttpStatusCode.Created);

        // This call sets the antiforgery cookie on the test client, so it comes after the client's last POST.
        var config = await assignee.GetFromJsonAsync<JsonElement>("/api/abp/application-configuration");
        config.GetProperty("auth").GetProperty("grantedPolicies").TryGetProperty("Hr.Complaints.Handle", out _).ShouldBeTrue();

        // Conflict of interest still applies: the assignee's own complaint is not theirs to handle.
        var againstAssignee = IdOf(await SubmitAsync(team.StaffClient, assigneeEmployee, "confidential"));
        (await DetailAsync(assignee, againstAssignee)).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // Other levels stay closed.
        var highest = IdOf(await SubmitAsync(team.StaffClient, team.Colleague.Id, "highlyConfidential"));
        (await DetailAsync(assignee, highest)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        await OkAsync(await DetailAsync(executive, highest));

        var revoked = await OkAsync(await executive.PostAsJsonAsync($"{url}/revoke", new { reason = "Done", concurrencyStamp = Stamp(approved) }));
        Text(revoked, "status").ShouldBe("revoked");
        Text(revoked, "decisionReason").ShouldBe("Done");

        (await DetailAsync(assignee, complaint)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await IsAssignedAsync(assignee, complaint)).ShouldBeFalse();
        var configAfter = await assignee.GetFromJsonAsync<JsonElement>("/api/abp/application-configuration");
        configAfter.GetProperty("auth").GetProperty("grantedPolicies").TryGetProperty("Hr.Complaints.Handle", out _).ShouldBeFalse();
        await OkAsync(await DetailAsync(team.Kit.Hr, complaint));
    }

    [Fact]
    public async Task A_rejected_request_stays_without_access()
    {
        var team = await TeamAsync();
        var (executive, _, _) = await OwnerAsync(team.Kit, null, HrRoles.ExecutiveDirector);
        var (_, assigneeUser, _) = await OwnerAsync(team.Kit, team.Manager.Id, HrRoles.Employee);

        var requested = await OkAsync(await team.Kit.Hr.PostAsJsonAsync("/api/app/complaint-handler", new { userId = assigneeUser, level = "normal", from = DateTime.UtcNow, to = DateTime.UtcNow.AddDays(1), reason = "Cover" }), HttpStatusCode.Created);
        var rejected = await OkAsync(await executive.PostAsJsonAsync($"/api/app/complaint-handler/{IdOf(requested)}/reject", new { reason = "No", concurrencyStamp = Stamp(requested) }));

        Text(rejected, "status").ShouldBe("rejected");
        Text(rejected, "decisionReason").ShouldBe("No");
    }

    [Fact]
    public async Task An_expired_assignment_stops_at_once_and_the_worker_takes_the_screens_back()
    {
        var team = await TeamAsync();
        var (executive, _, _) = await OwnerAsync(team.Kit, null, HrRoles.ExecutiveDirector);
        var (assignee, assigneeUser, _) = await OwnerAsync(team.Kit, team.Manager.Id, HrRoles.Employee);

        var requested = await OkAsync(
            await team.Kit.Hr.PostAsJsonAsync("/api/app/complaint-handler", new { userId = assigneeUser, level = "confidential", from = DateTime.UtcNow, to = DateTime.UtcNow.AddSeconds(3), reason = "Cover" }),
            HttpStatusCode.Created);
        await OkAsync(await executive.PostAsJsonAsync($"/api/app/complaint-handler/{IdOf(requested)}/approve", new { concurrencyStamp = Stamp(requested) }));

        await Task.Delay(TimeSpan.FromSeconds(4));

        // The period is over: the two-step gate is gone too, and the screens go on the next worker pass.
        await OkAsync(await assignee.GetAsync("/api/app/complaint/assigned"));
        (await IsAssignedAsync(assignee, Guid.NewGuid())).ShouldBeFalse();

        await WithTenantAsync(services => services.GetRequiredService<ComplaintHandlerExpiryService>().ReleaseExpiredAsync(DateTime.UtcNow));

        var config = await assignee.GetFromJsonAsync<JsonElement>("/api/abp/application-configuration");
        config.GetProperty("auth").GetProperty("grantedPolicies").TryGetProperty("Hr.Complaints.Handle", out _).ShouldBeFalse();

        await ShouldFailWithAsync(
            await executive.PostAsJsonAsync($"/api/app/complaint-handler/{IdOf(requested)}/revoke", new { reason = "x", concurrencyStamp = "x" }),
            HrErrorCodes.Complaints.AssignmentNotActive);
    }

    [Fact]
    public async Task A_late_complaint_is_flagged_and_reminders_stay_generic_and_grant_nothing()
    {
        var team = await TeamAsync();
        var id = IdOf(await SubmitAsync(team.StaffClient));

        await using (var connection = new NpgsqlConnection(_host.ConnectionString))
        {
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand("UPDATE complaints.\"Complaints\" SET \"DueAt\" = now() - interval '1 day' WHERE \"Id\" = @id", connection);
            command.Parameters.AddWithValue("id", id);
            await command.ExecuteNonQueryAsync();
        }

        var late = await OkAsync(await team.ManagerClient.GetAsync("/api/app/complaint/assigned?isLate=true&maxResultCount=200"));
        late.GetProperty("items").EnumerateArray().Single(x => IdOf(x) == id).GetProperty("isLate").GetBoolean().ShouldBeTrue();
        (await OkAsync(await team.ManagerClient.GetAsync("/api/app/complaint/assigned?isLate=false&maxResultCount=200")))
            .GetProperty("items").EnumerateArray().ShouldNotContain(x => IdOf(x) == id);

        var sent = 0;
        await WithTenantAsync(async services => sent = await services.GetRequiredService<ComplaintOverdueReminderService>().SendDueAsync(DateTime.UtcNow));
        sent.ShouldBeGreaterThan(0);

        // HR is reminded, without a link and without gaining access: the complaint still sits with the manager.
        var notices = await team.Kit.Hr.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200");
        notices.GetProperty("items").EnumerateArray().ShouldContain(x => Text(x, "templateCode") == "secureItemUpdateForHandler" && Text(x, "resourceType") == "none");
        (await DetailAsync(team.Kit.Hr, id)).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // With no interval configured, one reminder per complaint is enough.
        var again = 1;
        await WithTenantAsync(async services => again = await services.GetRequiredService<ComplaintOverdueReminderService>().SendDueAsync(DateTime.UtcNow));
        again.ShouldBe(0);
    }

    [Fact]
    public async Task The_restricted_account_cannot_delete_or_rewrite_and_sees_only_its_company()
    {
        var team = await TeamAsync();
        var id = IdOf(await SubmitAsync(team.StaffClient));
        await OkAsync(await DetailAsync(team.ManagerClient, id));

        Guid tenantId = Guid.Empty;
        await WithTenantAsync(services =>
        {
            tenantId = services.GetRequiredService<ICurrentTenant>().Id!.Value;
            return Task.CompletedTask;
        });

        await using var connection = new NpgsqlConnection(_host.ConnectionString);
        await connection.OpenAsync();

        async Task<object?> AsRestrictedAsync(string sql, string tenant)
        {
            await using var transaction = await connection.BeginTransactionAsync();
            await using (var role = new NpgsqlCommand($"SET LOCAL ROLE hr_complaints_app; SET LOCAL hr.tenant_id = '{tenant}';", connection, transaction))
            {
                await role.ExecuteNonQueryAsync();
            }

            await using var command = new NpgsqlCommand(sql, connection, transaction);
            return await command.ExecuteScalarAsync();
        }

        var count = $"SELECT count(*) FROM complaints.\"Complaints\" WHERE \"Id\" = '{id}'";
        ((long)(await AsRestrictedAsync(count, tenantId.ToString()))!).ShouldBe(1);
        ((long)(await AsRestrictedAsync(count, Guid.NewGuid().ToString()))!).ShouldBe(0);
        ((long)(await AsRestrictedAsync(count, string.Empty))!).ShouldBe(0);

        foreach (var sql in new[]
                 {
                     $"DELETE FROM complaints.\"Complaints\" WHERE \"Id\" = '{id}'",
                     $"DELETE FROM complaints.\"ComplaintAccessLogs\" WHERE \"ComplaintId\" = '{id}'",
                     $"UPDATE complaints.\"ComplaintAccessLogs\" SET \"UserId\" = \"UserId\" WHERE \"ComplaintId\" = '{id}'",
                     $"UPDATE complaints.\"ComplaintActions\" SET \"ActorUserId\" = \"ActorUserId\" WHERE \"ComplaintId\" = '{id}'",
                     "SELECT count(*) FROM \"AbpUsers\""
                 })
        {
            var denied = await Should.ThrowAsync<PostgresException>(() => AsRestrictedAsync(sql, tenantId.ToString()));
            denied.SqlState.ShouldBe("42501", sql);
        }
    }
}
