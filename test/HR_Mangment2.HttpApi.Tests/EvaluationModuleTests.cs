using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HR_Mangment2.Errors;
using HR_Mangment2.Evaluations;
using HR_Mangment2.Identity;
using HR_Mangment2.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;
using Xunit;

namespace HR_Mangment2;

[Collection(HostCollection.Name)]
public class EvaluationModuleTests
{
    private const string SecretNote = "INTERNAL-NOTE-MUST-NOT-LEAK";

    /// <summary>Months only ever go forward, because each new cycle closes the one before it, and the tests share one database.</summary>
    private static int _monthCounter;

    private readonly HostFixture _host;

    public EvaluationModuleTests(HostFixture host)
    {
        _host = host;
    }

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

    private static Guid IdOf(JsonElement element) => element.GetProperty("id").GetGuid();

    private static string Text(JsonElement element, string property) => element.GetProperty(property).GetString()!;

    private async Task RunAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = _host.Factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var tenant = await services.GetRequiredService<ITenantRepository>().FindByNameAsync(
            services.GetRequiredService<ITenantNormalizer>().NormalizeName(HostFixture.TestTenantName)!);
        using var change = services.GetRequiredService<ICurrentTenant>().Change(tenant!.Id);
        using var uow = services.GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: true);
        await action(services);
        await uow.CompleteAsync();
    }

    private sealed record Moments(int Year, int Month, DateTime OpensAt, DateTime FreezeAt, DateTime DueAt, DateTime EscalatesAt);

    /// <summary>Runs the real timetable service at a chosen moment in a month no other test used.</summary>
    private async Task<Moments> ScheduleNewCycleAsync()
    {
        var offset = Interlocked.Increment(ref _monthCounter);
        var year = 2040 + (offset - 1) / 12;
        var month = (offset - 1) % 12 + 1;
        await ProcessAsync(new DateTime(year, month, 2, 9, 0, 0, DateTimeKind.Utc));

        Moments? moments = null;
        await RunAsync(async services =>
        {
            var cycle = await services.GetRequiredService<IRepository<EvaluationCycle, Guid>>().FirstAsync(x => x.Year == year && x.Month == month);
            moments = new Moments(year, month, cycle.OpensAt, cycle.DataFreezeAt, cycle.DueAt, cycle.EscalatesAt);
        });
        return moments!;
    }

    private Task ProcessAsync(DateTime now) => RunAsync(services => services.GetRequiredService<EvaluationCycleService>().ProcessAsync(now));

    private async Task<(Moments Moments, EmployeeTestKit Kit, CreatedEmployee Manager, HttpClient ManagerClient, CreatedEmployee Staff, HttpClient StaffClient)> OpenCycleWithTeamAsync()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var moments = await ScheduleNewCycleAsync();
        await ProcessAsync(moments.OpensAt);
        return (moments, kit, manager, await kit.SignInAsync(manager), staff, await kit.SignInAsync(staff));
    }

    private static async Task<JsonElement> FormOfAsync(HttpClient manager, Guid employeeId, Moments moments)
    {
        var list = await manager.GetFromJsonAsync<JsonElement>($"/api/app/evaluation/team?employeeId={employeeId}&maxResultCount=200");
        var row = list.GetProperty("items").EnumerateArray().Single(x => x.GetProperty("year").GetInt32() == moments.Year && x.GetProperty("month").GetInt32() == moments.Month);
        return await manager.GetFromJsonAsync<JsonElement>($"/api/app/evaluation/{IdOf(row)}");
    }

    private static object ScoresFor(JsonElement form, string grade = "good") => form.GetProperty("scores").EnumerateArray()
        .Select(x => new { criterionId = x.GetProperty("criterionId").GetGuid(), gradeCode = grade })
        .ToArray();

    /// <summary>Fills and submits the form, leaving an internal note behind so every later read can be checked for it.</summary>
    private static async Task<(Guid Id, string Stamp)> SubmitWithNoteAsync(HttpClient manager, JsonElement form, string grade = "good")
    {
        var id = IdOf(form);
        var saved = await OkAsync(await manager.PutAsJsonAsync($"/api/app/evaluation/{id}", new
        {
            scores = ScoresFor(form, grade),
            employeeNotes = "Keep it up",
            concurrencyStamp = Text(form, "concurrencyStamp")
        }));

        await OkAsync(await manager.PutAsJsonAsync($"/api/app/evaluation/{id}/internal-note/draft", new { content = SecretNote }));
        var submitted = await OkAsync(await manager.PostAsJsonAsync($"/api/app/evaluation/{id}/submit", new { concurrencyStamp = Text(saved, "concurrencyStamp") }));
        return (id, Text(submitted, "concurrencyStamp"));
    }

    [Fact]
    public async Task The_scale_starts_with_five_of_each_and_keeps_its_minimums()
    {
        var admin = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.SystemAdmin);

        var criteria = await admin.GetFromJsonAsync<JsonElement>("/api/app/evaluation-criterion?maxResultCount=200&isActive=true&sorting=sortOrder");
        criteria.GetProperty("items").EnumerateArray().Select(x => Text(x, "nameEn")).ShouldContain("Quality of work");
        var grades = await admin.GetFromJsonAsync<JsonElement>("/api/app/evaluation-grade?maxResultCount=200&isActive=true&sorting=sortOrder");
        grades.GetProperty("items").EnumerateArray().Select(x => Text(x, "code")).ShouldBe(new[] { "weak", "acceptable", "good", "veryGood", "excellent" });

        // The verbal scale has no number anywhere.
        grades.GetProperty("items")[0].EnumerateObject().Select(x => x.Name).ShouldNotContain("value");
        grades.GetProperty("items")[0].EnumerateObject().Select(x => x.Name).ShouldNotContain("percentage");

        var first = criteria.GetProperty("items")[0];
        (await admin.PostAsJsonAsync("/api/app/evaluation-criterion", new { nameAr = Text(first, "nameAr"), nameEn = "Other " + Guid.NewGuid(), sortOrder = 9, isActive = false }))
            .StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await admin.PostAsJsonAsync("/api/app/evaluation-grade", new { code = "good", nameAr = "س", nameEn = "S", sortOrder = 9, isActive = false }))
            .StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await admin.PostAsJsonAsync("/api/app/evaluation-grade", new { code = "Not Valid", nameAr = "س", nameEn = "S", sortOrder = 9, isActive = false }))
            .StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Stopping everything but one leaves the last ones protected, and the scale is put back for the other tests.
        var stoppedCriteria = new System.Collections.Generic.List<(string Url, JsonElement Row)>();
        var stoppedGrades = new System.Collections.Generic.List<(string Url, JsonElement Row)>();
        try
        {
            var active = criteria.GetProperty("items").EnumerateArray().ToList();
            foreach (var row in active.Skip(1))
            {
                var url = $"/api/app/evaluation-criterion/{IdOf(row)}";
                var stopped = await OkAsync(await admin.PutAsJsonAsync(url, new { nameAr = Text(row, "nameAr"), nameEn = Text(row, "nameEn"), sortOrder = row.GetProperty("sortOrder").GetInt32(), isActive = false, concurrencyStamp = Text(row, "concurrencyStamp") }));
                stoppedCriteria.Add((url, stopped));
            }

            await ShouldFailWithAsync(
                await admin.PutAsJsonAsync($"/api/app/evaluation-criterion/{IdOf(first)}", new { nameAr = Text(first, "nameAr"), nameEn = Text(first, "nameEn"), sortOrder = 1, isActive = false, concurrencyStamp = Text(first, "concurrencyStamp") }),
                HrErrorCodes.Evaluations.LastActiveCriterion);

            var activeGrades = grades.GetProperty("items").EnumerateArray().ToList();
            foreach (var row in activeGrades.Skip(2))
            {
                var url = $"/api/app/evaluation-grade/{IdOf(row)}";
                var stopped = await OkAsync(await admin.PutAsJsonAsync(url, new { nameAr = Text(row, "nameAr"), nameEn = Text(row, "nameEn"), sortOrder = row.GetProperty("sortOrder").GetInt32(), isActive = false, concurrencyStamp = Text(row, "concurrencyStamp") }));
                stoppedGrades.Add((url, stopped));
            }

            var firstGrade = activeGrades[0];
            await ShouldFailWithAsync(
                await admin.PutAsJsonAsync($"/api/app/evaluation-grade/{IdOf(firstGrade)}", new { nameAr = Text(firstGrade, "nameAr"), nameEn = Text(firstGrade, "nameEn"), sortOrder = 1, isActive = false, concurrencyStamp = Text(firstGrade, "concurrencyStamp") }),
                HrErrorCodes.Evaluations.MinimumActiveGrades);
        }
        finally
        {
            foreach (var (url, row) in stoppedCriteria.Concat(stoppedGrades))
            {
                await OkAsync(await admin.PutAsJsonAsync(url, new { nameAr = Text(row, "nameAr"), nameEn = Text(row, "nameEn"), sortOrder = row.GetProperty("sortOrder").GetInt32(), isActive = true, concurrencyStamp = Text(row, "concurrencyStamp") }));
            }
        }

        var staff = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee);
        (await staff.GetAsync("/api/app/evaluation-criterion")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Opening_makes_forms_for_staff_only_and_not_for_managers_or_late_joiners()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var moments = await ScheduleNewCycleAsync();

        // A hire too late in the month to reach the minimum worked days.
        var lateBody = JsonSerializer.SerializeToNode(kit.Body(manager.Id, new[] { HrRoles.Employee }), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        lateBody["hireDate"] = new DateOnly(moments.Year, moments.Month, 25).ToString("yyyy-MM-dd");
        var late = await OkAsync(await kit.Hr.PostAsJsonAsync("/api/app/employee", lateBody), HttpStatusCode.Created);
        var lateId = IdOf(late.GetProperty("employee"));

        var managerClient = await kit.SignInAsync(manager);

        // Scheduled: nothing to fill yet.
        (await managerClient.GetFromJsonAsync<JsonElement>($"/api/app/evaluation/team?employeeId={staff.Id}")).GetProperty("totalCount").GetInt64().ShouldBe(0);

        await ProcessAsync(moments.OpensAt);
        await ProcessAsync(moments.OpensAt.AddMinutes(15)); // A second run changes nothing.

        (await managerClient.GetFromJsonAsync<JsonElement>($"/api/app/evaluation/team?employeeId={staff.Id}")).GetProperty("totalCount").GetInt64().ShouldBe(1);
        (await managerClient.GetFromJsonAsync<JsonElement>($"/api/app/evaluation/team?employeeId={lateId}")).GetProperty("totalCount").GetInt64().ShouldBe(0);

        var systemAdmin = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.HrManager);
        (await systemAdmin.GetFromJsonAsync<JsonElement>($"/api/app/evaluation?employeeId={manager.Id}")).GetProperty("totalCount").GetInt64().ShouldBe(0);

        var current = await managerClient.GetFromJsonAsync<JsonElement>("/api/app/evaluation-cycle/current");
        Text(current, "status").ShouldBe("open");
        current.GetProperty("year").GetInt32().ShouldBe(moments.Year);
        var progress = current.GetProperty("managerProgress");
        progress.GetProperty("expectedCount").GetInt32().ShouldBe(1);
        progress.GetProperty("remainingCount").GetInt32().ShouldBe(1);
        progress.GetProperty("isCompleted").GetBoolean().ShouldBeFalse();
        progress.GetProperty("reportId").ValueKind.ShouldBe(JsonValueKind.Null);

        var form = await FormOfAsync(managerClient, staff.Id, moments);
        form.GetProperty("isEditable").GetBoolean().ShouldBeTrue();
        form.GetProperty("scores").GetArrayLength().ShouldBeGreaterThanOrEqualTo(1);
        form.GetProperty("availableGrades").GetArrayLength().ShouldBeGreaterThanOrEqualTo(2);
        form.GetProperty("attendance").GetProperty("isFrozen").GetBoolean().ShouldBeFalse();

        var notifications = await managerClient.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200");
        notifications.GetProperty("items").EnumerateArray().Select(x => Text(x, "templateCode")).ShouldContain("monthEndCycleOpened");

        var inbox = await managerClient.GetFromJsonAsync<JsonElement>("/api/app/me/inbox-summary");
        inbox.GetProperty("items").EnumerateArray().Single(x => Text(x, "category") == "evaluation").GetProperty("count").GetInt32().ShouldBe(1);
    }

    [Fact]
    public async Task The_form_checks_its_rules_and_the_employee_sees_it_only_after_submission()
    {
        var (moments, kit, _, managerClient, staff, staffClient) = await OpenCycleWithTeamAsync();
        var form = await FormOfAsync(managerClient, staff.Id, moments);
        var id = IdOf(form);
        var stamp = Text(form, "concurrencyStamp");
        var firstCriterion = form.GetProperty("scores")[0].GetProperty("criterionId").GetGuid();

        (await staffClient.GetAsync($"/api/app/evaluation/my/{id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await staffClient.PostAsync($"/api/app/evaluation/{id}/acknowledge", null)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await staffClient.GetFromJsonAsync<JsonElement>("/api/app/evaluation/my")).GetProperty("totalCount").GetInt64().ShouldBe(0);

        var otherManager = await kit.SignInAsync(await kit.AddDepartmentManagerAsync());
        (await otherManager.GetAsync($"/api/app/evaluation/{id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        await ShouldFailWithAsync(
            await managerClient.PutAsJsonAsync($"/api/app/evaluation/{id}", new { scores = new[] { new { criterionId = Guid.NewGuid(), gradeCode = "good" } }, concurrencyStamp = stamp }),
            HrErrorCodes.Evaluations.InvalidCriterion);
        await ShouldFailWithAsync(
            await managerClient.PutAsJsonAsync($"/api/app/evaluation/{id}", new { scores = new[] { new { criterionId = firstCriterion, gradeCode = "nope" } }, concurrencyStamp = stamp }),
            HrErrorCodes.Evaluations.InvalidGrade);
        (await managerClient.PutAsJsonAsync($"/api/app/evaluation/{id}", new { scores = Array.Empty<object>(), concurrencyStamp = "stale" }))
            .StatusCode.ShouldBe(HttpStatusCode.Conflict);

        // A partial draft saves, and cannot be submitted.
        var partial = await OkAsync(await managerClient.PutAsJsonAsync($"/api/app/evaluation/{id}", new { scores = new[] { new { criterionId = firstCriterion, gradeCode = "good" } }, concurrencyStamp = stamp }));
        partial.GetProperty("scores").EnumerateArray().Single(x => x.GetProperty("criterionId").GetGuid() == firstCriterion).GetProperty("gradeCode").GetString().ShouldBe("good");
        await ShouldFailWithAsync(
            await managerClient.PostAsJsonAsync($"/api/app/evaluation/{id}/submit", new { concurrencyStamp = Text(partial, "concurrencyStamp") }),
            HrErrorCodes.Evaluations.IncompleteScores);

        // Saving replaces the previous scores instead of adding to them.
        var replaced = await OkAsync(await managerClient.PutAsJsonAsync($"/api/app/evaluation/{id}", new { scores = new[] { new { criterionId = firstCriterion, gradeCode = "weak" } }, concurrencyStamp = Text(partial, "concurrencyStamp") }));
        replaced.GetProperty("scores").EnumerateArray().Count(x => x.GetProperty("gradeCode").ValueKind == JsonValueKind.String).ShouldBe(1);

        await ShouldFailWithAsync(
            await managerClient.PostAsJsonAsync($"/api/app/evaluation/{id}/internal-note", new { content = "Too early" }),
            HrErrorCodes.Evaluations.EvaluationNotSubmitted);

        var (_, _) = await SubmitWithNoteAsync(managerClient, replaced);

        var after = await managerClient.GetFromJsonAsync<JsonElement>($"/api/app/evaluation/{id}");
        after.GetProperty("isEditable").GetBoolean().ShouldBeFalse();
        await ShouldFailWithAsync(
            await managerClient.PutAsJsonAsync($"/api/app/evaluation/{id}", new { scores = Array.Empty<object>(), concurrencyStamp = Text(after, "concurrencyStamp") }),
            HrErrorCodes.Evaluations.NotEditable);
        await ShouldFailWithAsync(
            await managerClient.PutAsJsonAsync($"/api/app/evaluation/{id}/internal-note/draft", new { content = "Late" }),
            HrErrorCodes.Evaluations.NotEditable);

        var mine = await OkAsync(await staffClient.GetAsync($"/api/app/evaluation/my/{id}"));
        Text(mine, "status").ShouldBe("submitted");
        Text(mine, "employeeNotes").ShouldBe("Keep it up");
        mine.GetProperty("scores").GetArrayLength().ShouldBe(form.GetProperty("scores").GetArrayLength());

        var acknowledged = await OkAsync(await staffClient.PostAsync($"/api/app/evaluation/{id}/acknowledge", null));
        Text(acknowledged, "status").ShouldBe("acknowledged");
        var again = await OkAsync(await staffClient.PostAsync($"/api/app/evaluation/{id}/acknowledge", null));
        SameMoment(again, acknowledged, "acknowledgedAt");
    }

    /// <summary>A value read back from the database has microseconds, and the one just written has finer ticks.</summary>
    private static void SameMoment(JsonElement left, JsonElement right, string property) =>
        (left.GetProperty(property).GetDateTime() - right.GetProperty(property).GetDateTime()).Duration().ShouldBeLessThan(TimeSpan.FromMilliseconds(1));

    [Fact]
    public async Task The_internal_note_reaches_only_its_author_and_hr()
    {
        var (moments, kit, _, managerClient, staff, staffClient) = await OpenCycleWithTeamAsync();
        var form = await FormOfAsync(managerClient, staff.Id, moments);
        var (id, _) = await SubmitWithNoteAsync(managerClient, form);
        var hr = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.HrManager);

        var added = await OkAsync(await managerClient.PostAsJsonAsync($"/api/app/evaluation/{id}/internal-note", new { content = SecretNote + "-2" }), HttpStatusCode.Created);
        added.GetProperty("isDraft").GetBoolean().ShouldBeFalse();

        var written = await managerClient.GetFromJsonAsync<JsonElement>($"/api/app/evaluation/{id}/internal-note");
        written.GetProperty("totalCount").GetInt64().ShouldBe(2);
        written.GetProperty("items").EnumerateArray().ShouldAllBe(x => !x.GetProperty("isDraft").GetBoolean());
        (await hr.GetFromJsonAsync<JsonElement>($"/api/app/evaluation/{id}/internal-note")).GetProperty("totalCount").GetInt64().ShouldBe(2);

        // A manager who wrote nothing cannot even tell notes exist.
        var other = await kit.SignInAsync(await kit.AddDepartmentManagerAsync());
        (await other.GetFromJsonAsync<JsonElement>($"/api/app/evaluation/{id}/internal-note")).GetProperty("totalCount").GetInt64().ShouldBe(0);
        (await other.PostAsJsonAsync($"/api/app/evaluation/{id}/internal-note", new { content = "Not mine" })).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // The employee: no permission for the routes, and the note in none of their responses or notifications.
        (await staffClient.GetAsync($"/api/app/evaluation/{id}/internal-note")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        foreach (var url in new[] { $"/api/app/evaluation/my/{id}", "/api/app/evaluation/my", "/api/app/notification?maxResultCount=200", $"/api/app/evaluation-annual-summary/my?year={moments.Year}" })
        {
            (await (await staffClient.GetAsync(url)).Content.ReadAsStringAsync()).ShouldNotContain(SecretNote);
        }

        // And nowhere in what HR or the manager read about the evaluation itself.
        (await (await hr.GetAsync($"/api/app/evaluation/{id}/hr-view")).Content.ReadAsStringAsync()).ShouldNotContain(SecretNote);
        (await (await hr.GetAsync($"/api/app/evaluation?employeeId={staff.Id}")).Content.ReadAsStringAsync()).ShouldNotContain(SecretNote);
        (await (await managerClient.GetAsync($"/api/app/evaluation/{id}")).Content.ReadAsStringAsync()).ShouldNotContain(SecretNote);
        (await (await managerClient.GetAsync("/api/app/report/team-evaluations")).Content.ReadAsStringAsync()).ShouldNotContain(SecretNote);

        var reviewer = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.Reviewer);
        (await (await reviewer.GetAsync("/api/app/report/evaluation-consistency")).Content.ReadAsStringAsync()).ShouldNotContain(SecretNote);

        var audit = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.SystemAdmin);
        (await (await audit.GetAsync("/api/app/audit-log?maxResultCount=200")).Content.ReadAsStringAsync()).ShouldNotContain(SecretNote);
    }

    [Fact]
    public async Task A_new_manager_gets_the_scores_without_the_previous_managers_note_draft()
    {
        var (moments, kit, oldManager, oldClient, staff, _) = await OpenCycleWithTeamAsync();
        var form = await FormOfAsync(oldClient, staff.Id, moments);
        var id = IdOf(form);

        var saved = await OkAsync(await oldClient.PutAsJsonAsync($"/api/app/evaluation/{id}", new { scores = ScoresFor(form), employeeNotes = "Carried over", concurrencyStamp = Text(form, "concurrencyStamp") }));
        var draft = await OkAsync(await oldClient.PutAsJsonAsync($"/api/app/evaluation/{id}/internal-note/draft", new { content = SecretNote }));
        draft.GetProperty("isDraft").GetBoolean().ShouldBeTrue();

        // The draft is its author's alone, even from HR.
        var hr = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.HrManager);
        (await hr.GetFromJsonAsync<JsonElement>($"/api/app/evaluation/{id}/internal-note")).GetProperty("totalCount").GetInt64().ShouldBe(0);
        (await oldClient.GetFromJsonAsync<JsonElement>($"/api/app/evaluation/{id}/internal-note")).GetProperty("totalCount").GetInt64().ShouldBe(1);

        var newManager = await kit.AddDepartmentManagerAsync();
        var newClient = await kit.SignInAsync(newManager);
        var employee = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/employee/{staff.Id}");
        await OkAsync(await kit.Hr.PutAsJsonAsync($"/api/app/employee/{staff.Id}/manager", new { managerEmployeeId = newManager.Id, concurrencyStamp = Text(employee, "concurrencyStamp") }));

        (await oldClient.GetAsync($"/api/app/evaluation/{id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var moved = await OkAsync(await newClient.GetAsync($"/api/app/evaluation/{id}"));
        Text(moved, "employeeNotes").ShouldBe("Carried over");
        moved.GetProperty("scores").EnumerateArray().ShouldAllBe(x => x.GetProperty("gradeCode").GetString() == "good");
        (await newClient.GetFromJsonAsync<JsonElement>($"/api/app/evaluation/{id}/internal-note")).GetProperty("totalCount").GetInt64().ShouldBe(0);

        await OkAsync(await newClient.PostAsJsonAsync($"/api/app/evaluation/{id}/submit", new { concurrencyStamp = Text(saved, "concurrencyStamp") }));

        // Submitting fixed nothing from the old manager's draft.
        (await hr.GetFromJsonAsync<JsonElement>($"/api/app/evaluation/{id}/internal-note")).GetProperty("totalCount").GetInt64().ShouldBe(0);

        var view = await hr.GetFromJsonAsync<JsonElement>($"/api/app/evaluation/{id}/hr-view");
        view.GetProperty("evaluator").GetProperty("id").GetGuid().ShouldBe(newManager.UserId);
    }

    [Fact]
    public async Task An_objection_goes_to_hr_and_is_decided_once()
    {
        var (moments, _, _, managerClient, staff, staffClient) = await OpenCycleWithTeamAsync();
        var form = await FormOfAsync(managerClient, staff.Id, moments);
        var (id, _) = await SubmitWithNoteAsync(managerClient, form);
        var hr = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.HrManager);

        var comment = await OkAsync(await staffClient.PostAsJsonAsync($"/api/app/evaluation/{id}/response", new { type = "comment", content = "Thanks" }), HttpStatusCode.Created);
        var objection = await OkAsync(await staffClient.PostAsJsonAsync($"/api/app/evaluation/{id}/response", new { type = "objection", content = "I disagree" }), HttpStatusCode.Created);
        await ShouldFailWithAsync(
            await staffClient.PostAsJsonAsync($"/api/app/evaluation/{id}/response", new { type = "objection", content = "Again" }),
            HrErrorCodes.Evaluations.ObjectionAlreadyPending);

        var view = await hr.GetFromJsonAsync<JsonElement>($"/api/app/evaluation/{id}/hr-view");
        Text(view, "objectionStatus").ShouldBe("pending");
        view.GetProperty("responses").GetArrayLength().ShouldBe(2);
        var stamp = Text(view, "concurrencyStamp");

        (await hr.GetFromJsonAsync<JsonElement>($"/api/app/evaluation?objectionStatus=pending&employeeId={staff.Id}")).GetProperty("totalCount").GetInt64().ShouldBe(1);

        await ShouldFailWithAsync(
            await hr.PostAsJsonAsync($"/api/app/evaluation/{id}/response/{IdOf(comment)}/decide", new { decision = "accepted", reason = "x", concurrencyStamp = stamp }),
            HrErrorCodes.Evaluations.NotAnObjection);
        (await hr.PostAsJsonAsync($"/api/app/evaluation/{id}/response/{IdOf(objection)}/decide", new { decision = "accepted", reason = "x", concurrencyStamp = "stale" }))
            .StatusCode.ShouldBe(HttpStatusCode.Conflict);

        // D28: scores belong to an accepted objection only.
        (await hr.PostAsJsonAsync($"/api/app/evaluation/{id}/response/{IdOf(objection)}/decide", new { decision = "rejected", reason = "x", concurrencyStamp = stamp, scores = new[] { new { criterionId = Guid.NewGuid(), gradeCode = "x" } } }))
            .StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        await ShouldFailWithAsync(
            await hr.PostAsJsonAsync($"/api/app/evaluation/{id}/response/{IdOf(objection)}/decide", new { decision = "accepted", reason = "x", concurrencyStamp = stamp, scores = new[] { new { criterionId = Guid.NewGuid(), gradeCode = "x" } } }),
            HrErrorCodes.Evaluations.InvalidCriterion);

        var decided = await OkAsync(await hr.PostAsJsonAsync($"/api/app/evaluation/{id}/response/{IdOf(objection)}/decide", new { decision = "rejected", reason = "The facts support the grade", concurrencyStamp = stamp }));
        Text(decided, "objectionStatus").ShouldBe("rejected");
        await ShouldFailWithAsync(
            await hr.PostAsJsonAsync($"/api/app/evaluation/{id}/response/{IdOf(objection)}/decide", new { decision = "accepted", reason = "x", concurrencyStamp = Text(decided, "concurrencyStamp") }),
            HrErrorCodes.Evaluations.ObjectionAlreadyDecided);

        var mine = await staffClient.GetFromJsonAsync<JsonElement>($"/api/app/evaluation/my/{id}");
        var decidedRow = mine.GetProperty("responses").EnumerateArray().Single(x => IdOf(x) == IdOf(objection));
        Text(decidedRow, "decisionReason").ShouldBe("The facts support the grade");

        var notes = await staffClient.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200");
        notes.GetProperty("items").EnumerateArray().Select(x => Text(x, "templateCode")).ShouldContain("evaluationObjectionDecided");

        var summary = await staffClient.GetFromJsonAsync<JsonElement>($"/api/app/evaluation-annual-summary/my?year={moments.Year}");
        summary.GetProperty("objectionsCount").GetInt32().ShouldBe(1);
        summary.GetProperty("evaluatedMonthsCount").GetInt32().ShouldBe(1);
        summary.GetProperty("criteria")[0].GetProperty("gradeCounts")[0].GetProperty("gradeCode").GetString().ShouldBe("good");

        (await managerClient.GetFromJsonAsync<JsonElement>($"/api/app/evaluation-annual-summary/employee/{staff.Id}?year={moments.Year}")).GetProperty("evaluatedMonthsCount").GetInt32().ShouldBe(1);
        var stranger = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.DepartmentManager);
        (await stranger.GetAsync($"/api/app/evaluation-annual-summary/employee/{staff.Id}?year={moments.Year}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await staffClient.GetAsync($"/api/app/evaluation-annual-summary/my")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task An_hr_manager_being_evaluated_cannot_see_or_decide_their_own()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var managerClient = await kit.SignInAsync(manager);

        var (userId, userName) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee, HrRoles.HrManager);
        var ownerId = await kit.AddFileForUserAsync(userId, manager.Id);
        var ownerClient = TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, userName, TestUsers.Password)).AccessToken, "en");
        var enrollment = await (await ownerClient.PostAsync("/api/app/account/mfa/enroll", null)).Content.ReadFromJsonAsync<JsonElement>();
        (await ownerClient.PostAsJsonAsync("/api/app/account/mfa/confirm", new { code = Totp.Compute(Text(enrollment, "sharedKey")) })).EnsureSuccessStatusCode();

        var moments = await ScheduleNewCycleAsync();
        await ProcessAsync(moments.OpensAt);
        var form = await FormOfAsync(managerClient, ownerId, moments);
        var (id, _) = await SubmitWithNoteAsync(managerClient, form);

        (await ownerClient.GetAsync($"/api/app/evaluation/{id}/hr-view")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ownerClient.GetAsync($"/api/app/evaluation/{id}/internal-note")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ownerClient.GetFromJsonAsync<JsonElement>($"/api/app/evaluation?employeeId={ownerId}")).GetProperty("totalCount").GetInt64().ShouldBe(0);

        var objection = await OkAsync(await ownerClient.PostAsJsonAsync($"/api/app/evaluation/{id}/response", new { type = "objection", content = "I disagree" }), HttpStatusCode.Created);
        await ShouldFailWithAsync(
            await ownerClient.PostAsJsonAsync($"/api/app/evaluation/{id}/response/{IdOf(objection)}/decide", new { decision = "accepted", reason = "Mine", concurrencyStamp = "x" }),
            HrErrorCodes.Evaluations.SelfDecisionNotAllowed);
    }

    [Fact]
    public async Task The_month_freezes_into_a_report_and_only_both_submissions_complete_a_department()
    {
        var (moments, kit, manager, managerClient, staff, staffClient) = await OpenCycleWithTeamAsync();
        var hr = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.HrManager);
        var reviewer = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.Reviewer);

        await ProcessAsync(moments.FreezeAt);
        await ProcessAsync(moments.FreezeAt.AddMinutes(15)); // A second run makes no second report.

        var reports = await managerClient.GetFromJsonAsync<JsonElement>("/api/app/department-report");
        var row = reports.GetProperty("items").EnumerateArray().Single(x => x.GetProperty("year").GetInt32() == moments.Year && x.GetProperty("month").GetInt32() == moments.Month);
        var url = $"/api/app/department-report/{IdOf(row)}";

        var report = await managerClient.GetFromJsonAsync<JsonElement>(url);
        Text(report, "status").ShouldBe("draft");
        var snapshot = report.GetProperty("snapshot");
        snapshot.GetProperty("employees").EnumerateArray().Single().GetProperty("employee").GetProperty("id").GetGuid().ShouldBe(staff.Id);
        snapshot.GetProperty("evaluations").GetProperty("expectedCount").GetInt32().ShouldBe(1);
        snapshot.GetProperty("evaluations").GetProperty("submittedCount").GetInt32().ShouldBe(0);
        report.GetProperty("completion").GetProperty("isCompleted").GetBoolean().ShouldBeFalse();

        // Other managers get a 404, HR reads every report, and a manager's list holds only their own.
        var other = await kit.SignInAsync(await kit.AddDepartmentManagerAsync());
        (await other.GetAsync(url)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await other.PatchAsJsonAsync(url, new { notes = "x", concurrencyStamp = "x" })).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await hr.GetAsync(url)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await staffClient.GetAsync(url)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var patched = await OkAsync(await managerClient.PatchAsJsonAsync(url, new { notes = "A calm month", concurrencyStamp = Text(report, "concurrencyStamp") }));
        Text(patched, "notes").ShouldBe("A calm month");

        // The report goes in first, and the evaluation after it: only then is the department done.
        var submitted = await OkAsync(await managerClient.PostAsJsonAsync($"{url}/submit", new { concurrencyStamp = Text(patched, "concurrencyStamp") }));
        Text(submitted, "status").ShouldBe("submitted");
        submitted.GetProperty("completion").GetProperty("reportSubmitted").GetBoolean().ShouldBeTrue();
        submitted.GetProperty("completion").GetProperty("isCompleted").GetBoolean().ShouldBeFalse();
        await ShouldFailWithAsync(await managerClient.PatchAsJsonAsync(url, new { notes = "Late", concurrencyStamp = Text(submitted, "concurrencyStamp") }), HrErrorCodes.DepartmentReports.AlreadySubmitted);
        await ShouldFailWithAsync(await managerClient.PostAsJsonAsync($"{url}/submit", new { concurrencyStamp = Text(submitted, "concurrencyStamp") }), HrErrorCodes.DepartmentReports.AlreadySubmitted);

        var form = await FormOfAsync(managerClient, staff.Id, moments);
        form.GetProperty("attendance").GetProperty("isFrozen").GetBoolean().ShouldBeTrue();
        await SubmitWithNoteAsync(managerClient, form);

        var done = await managerClient.GetFromJsonAsync<JsonElement>(url);
        done.GetProperty("completion").GetProperty("isCompleted").GetBoolean().ShouldBeTrue();

        // The evaluation part of a submitted report stays as it was when it went in.
        done.GetProperty("snapshot").GetProperty("evaluations").GetProperty("submittedCount").GetInt32().ShouldBe(0);

        (await (await managerClient.GetAsync(url)).Content.ReadAsStringAsync()).ShouldNotContain(SecretNote);
        (await (await hr.GetAsync(url)).Content.ReadAsStringAsync()).ShouldNotContain(SecretNote);

        var dashboard = await reviewer.GetFromJsonAsync<JsonElement>($"/api/app/report/review-dashboard?year={moments.Year}&month={moments.Month}");
        dashboard.GetProperty("cycleId").ValueKind.ShouldBe(JsonValueKind.String);
        var mine = dashboard.GetProperty("departments").EnumerateArray().Single(x => x.GetProperty("manager").GetProperty("id").GetGuid() == manager.UserId);
        mine.GetProperty("completion").GetProperty("isCompleted").GetBoolean().ShouldBeTrue();
        mine.GetProperty("completion").GetProperty("isLate").GetBoolean().ShouldBeFalse();
        dashboard.GetRawText().ShouldNotContain(SecretNote);
    }

    [Fact]
    public async Task Reminders_stop_at_the_deadline_and_the_fourth_day_escalates_and_marks_late()
    {
        var (moments, _, _, managerClient, staff, _) = await OpenCycleWithTeamAsync();
        var hr = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.HrManager);

        await ProcessAsync(moments.FreezeAt);

        async Task<int> CountAsync(HttpClient client, string template)
        {
            var list = await client.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200");
            return list.GetProperty("items").EnumerateArray().Count(x => Text(x, "templateCode") == template);
        }

        // Counted from here, because the previous test's cycle froze and reminded when this month's cycle was scheduled.
        var reminders = await CountAsync(managerClient, "monthlyCycleReminder");
        var escalations = await CountAsync(hr, "monthlyCycleEscalated");

        // Morning of the first working day: one reminder, and the same day does not repeat it.
        await ProcessAsync(moments.FreezeAt.AddHours(9));
        (await CountAsync(managerClient, "monthlyCycleReminder")).ShouldBe(reminders + 1);
        await ProcessAsync(moments.FreezeAt.AddHours(10));
        (await CountAsync(managerClient, "monthlyCycleReminder")).ShouldBe(reminders + 1);

        await ProcessAsync(moments.EscalatesAt);
        (await CountAsync(hr, "monthlyCycleEscalated")).ShouldBe(escalations + 1);
        await ProcessAsync(moments.EscalatesAt.AddHours(1));
        (await CountAsync(hr, "monthlyCycleEscalated")).ShouldBe(escalations + 1);

        // Past the deadline there is no reminder, only the late mark.
        await ProcessAsync(moments.DueAt.AddDays(1).AddHours(9));
        (await CountAsync(managerClient, "monthlyCycleReminder")).ShouldBe(reminders + 1);

        // Handing in after the deadline stays allowed.
        var form = await FormOfAsync(managerClient, staff.Id, moments);
        form.GetProperty("isEditable").GetBoolean().ShouldBeTrue();

        // These cycles sit in the future of the real clock, so the late mark is read at a chosen moment.
        await RunAsync(async services =>
        {
            var cycle = await services.GetRequiredService<IRepository<EvaluationCycle, Guid>>().FirstAsync(x => x.Year == moments.Year && x.Month == moments.Month);
            var reader = services.GetRequiredService<MonthlyCompletionReader>();
            var staffIds = new[] { staff.Id };

            (await reader.ReadAsync(cycle, staffIds, null, moments.EscalatesAt.AddMinutes(-1))).IsLate.ShouldBeFalse();
            (await reader.ReadAsync(cycle, staffIds, null, moments.EscalatesAt)).IsLate.ShouldBeTrue();
        });
    }

    [Fact]
    public async Task The_consistency_report_names_no_employee_and_flags_one_grade_for_all()
    {
        var (moments, kit, manager, managerClient, staff, _) = await OpenCycleWithTeamAsync();
        var secondManagerStaff = await kit.AddStaffAsync(manager.Id);
        await ProcessAsync(moments.OpensAt.AddMinutes(1));

        // The second person joined the team after opening, so the check uses a fresh cycle with both.
        var next = await ScheduleNewCycleAsync();
        await ProcessAsync(next.OpensAt);
        var first = await FormOfAsync(managerClient, staff.Id, next);
        var second = await FormOfAsync(managerClient, secondManagerStaff.Id, next);
        await SubmitWithNoteAsync(managerClient, first, "excellent");
        await SubmitWithNoteAsync(managerClient, second, "excellent");

        var reviewer = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.Reviewer);
        var cycleId = first.GetProperty("cycleId").GetGuid();
        var body = await (await reviewer.GetAsync($"/api/app/report/evaluation-consistency?cycleId={cycleId}&maxResultCount=200")).Content.ReadAsStringAsync();
        var row = JsonDocument.Parse(body).RootElement.GetProperty("items").EnumerateArray().Single(x => x.GetProperty("manager").GetProperty("id").GetGuid() == manager.UserId);

        row.GetProperty("expectedCount").GetInt32().ShouldBe(2);
        row.GetProperty("submittedCount").GetInt32().ShouldBe(2);
        row.GetProperty("completionRatio").GetDecimal().ShouldBe(1);
        row.GetProperty("distinctGradesUsed").GetInt32().ShouldBe(1);
        row.GetProperty("sameGradeForAll").GetBoolean().ShouldBeTrue();
        row.GetProperty("gradeCounts")[0].GetProperty("gradeCode").GetString().ShouldBe("excellent");
        row.TryGetProperty("employee", out _).ShouldBeFalse();
        body.ShouldNotContain(staff.Id.ToString());

        var team = await managerClient.GetFromJsonAsync<JsonElement>($"/api/app/report/team-evaluations?cycleId={cycleId}");
        team.GetProperty("totalCount").GetInt64().ShouldBe(2);
        team.GetProperty("items")[0].GetProperty("scores")[0].GetProperty("gradeCode").GetString().ShouldBe("excellent");

        (await managerClient.GetAsync($"/api/app/report/teamEvaluations/export?format=excel&cycleId={cycleId}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await reviewer.GetAsync($"/api/app/report/evaluationConsistency/export?format=excel&cycleId={cycleId}")).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Circulars_fix_their_recipients_and_scope_follows_the_senders_role()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var managerClient = await kit.SignInAsync(manager);
        var staff = await kit.AddStaffAsync(manager.Id);
        var staffClient = await kit.SignInAsync(staff);
        var hr = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.HrManager);

        var lonelyManager = await kit.SignInAsync(await kit.AddDepartmentManagerAsync());
        await ShouldFailWithAsync(await lonelyManager.PostAsJsonAsync("/api/app/circular", new { scope = "team", title = "T", content = "C" }), HrErrorCodes.Circulars.NoRecipients);
        await ShouldFailWithAsync(await managerClient.PostAsJsonAsync("/api/app/circular", new { scope = "company", title = "T", content = "C" }), HrErrorCodes.Circulars.ScopeNotAllowed);
        await ShouldFailWithAsync(await hr.PostAsJsonAsync("/api/app/circular", new { scope = "team", title = "T", content = "C" }), HrErrorCodes.Circulars.ScopeNotAllowed);
        (await staffClient.PostAsJsonAsync("/api/app/circular", new { scope = "team", title = "T", content = "C" })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var key = Guid.NewGuid().ToString();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/app/circular") { Content = JsonContent.Create(new { scope = "team", title = "Team note", content = "Read this" }) };
        request.Headers.Add("Idempotency-Key", key);
        var sent = await OkAsync(await managerClient.SendAsync(request), HttpStatusCode.Created);
        sent.GetProperty("recipientsCount").GetInt32().ShouldBe(1);
        sent.GetProperty("acknowledgedCount").GetInt32().ShouldBe(0);
        var id = IdOf(sent);

        // Someone joining later does not see it.
        var joiner = await kit.AddStaffAsync(manager.Id);
        var joinerClient = await kit.SignInAsync(joiner);
        (await joinerClient.GetAsync($"/api/app/circular/{id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await joinerClient.PostAsync($"/api/app/circular/{id}/acknowledge", null)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await joinerClient.GetFromJsonAsync<JsonElement>("/api/app/circular")).GetProperty("totalCount").GetInt64().ShouldBe(0);

        var received = await OkAsync(await staffClient.GetAsync($"/api/app/circular/{id}"));
        received.GetProperty("acknowledgementRequired").GetBoolean().ShouldBeTrue();
        received.GetProperty("acknowledgedAt").ValueKind.ShouldBe(JsonValueKind.Null);
        (await staffClient.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200")).GetProperty("items").EnumerateArray()
            .Select(x => Text(x, "templateCode")).ShouldContain("circularReceived");

        var inboxBefore = await staffClient.GetFromJsonAsync<JsonElement>("/api/app/me/inbox-summary");
        inboxBefore.GetProperty("items").EnumerateArray().Single(x => Text(x, "category") == "circularAcknowledgement").GetProperty("count").GetInt32().ShouldBe(1);

        var acknowledged = await OkAsync(await staffClient.PostAsync($"/api/app/circular/{id}/acknowledge", null));
        var again = await OkAsync(await staffClient.PostAsync($"/api/app/circular/{id}/acknowledge", null));
        SameMoment(again, acknowledged, "acknowledgedAt");

        (await staffClient.GetFromJsonAsync<JsonElement>("/api/app/circular?isAcknowledged=false")).GetProperty("totalCount").GetInt64().ShouldBe(0);

        var receipts = await managerClient.GetFromJsonAsync<JsonElement>($"/api/app/circular/{id}/receipt");
        receipts.GetProperty("items").EnumerateArray().Single().GetProperty("recipient").GetProperty("id").GetGuid().ShouldBe(staff.Id);
        (await lonelyManager.GetAsync($"/api/app/circular/{id}/receipt")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var list = await managerClient.GetFromJsonAsync<JsonElement>("/api/app/circular/sent");
        list.GetProperty("items").EnumerateArray().Single(x => IdOf(x) == id).GetProperty("acknowledgedCount").GetInt32().ShouldBe(1);

        var toManagers = await OkAsync(await hr.PostAsJsonAsync("/api/app/circular", new { scope = "departmentManagers", title = "For managers", content = "Read" }), HttpStatusCode.Created);
        (await managerClient.GetAsync($"/api/app/circular/{IdOf(toManagers)}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await staffClient.GetAsync($"/api/app/circular/{IdOf(toManagers)}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var company = await OkAsync(await hr.PostAsJsonAsync("/api/app/circular", new { scope = "company", title = "For all", content = "Read" }), HttpStatusCode.Created);
        (await staffClient.GetAsync($"/api/app/circular/{IdOf(company)}")).StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
