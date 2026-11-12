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
using Shouldly;
using Xunit;

namespace HR_Mangment2;

/// <summary>HR evaluations (decision 2026-11-10): 1-10 per HR heading, never shown to the employee, sent to the executive.</summary>
[Collection(HostCollection.Name)]
public class HrEvaluationModuleTests
{
    private const string Criteria = "/api/app/hr-evaluation-criterion";
    private const string Evaluations = "/api/app/hr-evaluation";
    private const string Reports = "/api/app/hr-evaluation-report";

    private readonly HostFixture _host;

    public HrEvaluationModuleTests(HostFixture host)
    {
        _host = host;
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

    private static Guid IdOf(JsonElement element) => element.GetProperty("id").GetGuid();

    private static string Text(JsonElement element, string property) => element.GetProperty(property).GetString()!;

    private static object Body(Guid employeeId, params (Guid Criterion, int Score)[] scores) =>
        new { employeeId, scores = scores.Select(x => new { criterionId = x.Criterion, score = x.Score }).ToArray(), notes = "Committed, should communicate more." };

    /// <summary>The tests share one database, so every heading left active by an earlier run is switched off first.</summary>
    private static async Task DeactivateAllAsync(HttpClient hr)
    {
        var active = await hr.GetFromJsonAsync<JsonElement>($"{Criteria}?isActive=true&maxResultCount=200");
        foreach (var c in active.GetProperty("items").EnumerateArray())
        {
            await OkAsync(await hr.PutAsJsonAsync($"{Criteria}/{IdOf(c)}",
                new { nameAr = Text(c, "nameAr"), nameEn = Text(c, "nameEn"), sortOrder = c.GetProperty("sortOrder").GetInt32(), isActive = false, concurrencyStamp = Text(c, "concurrencyStamp") }));
        }
    }

    [Fact]
    public async Task Hr_scores_an_employee_the_employee_sees_nothing_and_the_executive_gets_the_report()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var staffClient = await kit.SignInAsync(staff);
        var hr = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.HrManager);
        var otherHr = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.HrManager);
        var executive = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.ExecutiveDirector);

        // Only the HR manager manages headings and evaluates.
        (await staffClient.GetAsync(Criteria)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await executive.PostAsJsonAsync(Evaluations, Body(staff.Id, (Guid.NewGuid(), 5)))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        await DeactivateAllAsync(hr);
        var conduct = await OkAsync(await hr.PostAsJsonAsync(Criteria, new { nameAr = "السلوك المهني", nameEn = "Professional conduct", sortOrder = 1 }), HttpStatusCode.Created);
        var attendance = await OkAsync(await hr.PostAsJsonAsync(Criteria, new { nameAr = "الالتزام بالدوام", nameEn = "Attendance", sortOrder = 2 }), HttpStatusCode.Created);
        conduct.GetProperty("isActive").GetBoolean().ShouldBeTrue();
        Text(conduct, "name").ShouldBe("Professional conduct");

        // A stale stamp is a conflict.
        var renamed = await OkAsync(await hr.PutAsJsonAsync($"{Criteria}/{IdOf(attendance)}",
            new { nameAr = "الالتزام بالدوام", nameEn = "Punctuality", sortOrder = 2, isActive = true, concurrencyStamp = Text(attendance, "concurrencyStamp") }));
        Text(renamed, "nameEn").ShouldBe("Punctuality");
        (await hr.PutAsJsonAsync($"{Criteria}/{IdOf(attendance)}",
            new { nameAr = "x", nameEn = "x", sortOrder = 2, isActive = true, concurrencyStamp = Text(attendance, "concurrencyStamp") })).StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var a = IdOf(conduct);
        var b = IdOf(attendance);

        // Every active heading exactly once, 1..10, and only an eligible employee.
        await ShouldFailWithAsync(await hr.PostAsJsonAsync(Evaluations, Body(staff.Id, (a, 8))), HrErrorCodes.HrEvaluation.ScoresIncomplete);
        await ShouldFailWithAsync(await hr.PostAsJsonAsync(Evaluations, Body(staff.Id, (a, 8), (a, 7))), HrErrorCodes.HrEvaluation.ScoresIncomplete);
        await ShouldFailWithAsync(await hr.PostAsJsonAsync(Evaluations, Body(staff.Id, (a, 8), (b, 7), (Guid.NewGuid(), 5))), HrErrorCodes.HrEvaluation.CriterionNotFound);
        await ShouldFailWithAsync(await hr.PostAsJsonAsync(Evaluations, Body(kit.ExecutiveEmployeeId, (a, 8), (b, 7))), HrErrorCodes.HrEvaluation.EmployeeNotAllowed);
        await ShouldFailWithAsync(await hr.PostAsJsonAsync(Evaluations, Body(kit.SystemAdminEmployeeId, (a, 8), (b, 7))), HrErrorCodes.HrEvaluation.EmployeeNotAllowed);
        await ShouldFailWithAsync(await hr.PostAsJsonAsync(Evaluations, Body(Guid.NewGuid(), (a, 8), (b, 7))), HrErrorCodes.HrEvaluation.EmployeeNotAllowed);
        (await hr.PostAsJsonAsync(Evaluations, Body(staff.Id, (a, 11), (b, 7)))).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await hr.PostAsJsonAsync(Evaluations, Body(staff.Id, (a, 0), (b, 7)))).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var created = await OkAsync(await hr.PostAsJsonAsync(Evaluations, Body(staff.Id, (b, 7), (a, 8))), HttpStatusCode.Created);
        var id = IdOf(created);
        created.GetProperty("averageScore").GetDecimal().ShouldBe(7.5m);
        created.GetProperty("reportId").ValueKind.ShouldBe(JsonValueKind.Null);
        Text(created, "notes").ShouldBe("Committed, should communicate more.");
        created.GetProperty("scores").EnumerateArray().Select(x => Text(x, "criterionName")).ShouldBe(new[] { "Professional conduct", "Punctuality" });

        // A deactivated heading can no longer be scored, and the saved evaluation keeps its name.
        await OkAsync(await hr.PutAsJsonAsync($"{Criteria}/{b}", new { nameAr = "الالتزام بالدوام", nameEn = "Punctuality", sortOrder = 2, isActive = false, concurrencyStamp = Text(renamed, "concurrencyStamp") }));
        await ShouldFailWithAsync(await hr.PostAsJsonAsync(Evaluations, Body(staff.Id, (a, 6), (b, 6))), HrErrorCodes.HrEvaluation.CriterionNotFound);
        var second = await OkAsync(await hr.PostAsJsonAsync(Evaluations, Body(staff.Id, (a, 6))), HttpStatusCode.Created);
        (await OkAsync(await hr.GetAsync($"{Evaluations}/{id}"))).GetProperty("scores").GetArrayLength().ShouldBe(2);

        // The employee sees nothing: no access, no notification.
        (await staffClient.GetAsync($"{Evaluations}/{id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await staffClient.GetAsync(Reports)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await staffClient.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200")).GetRawText().ShouldNotContain(id.ToString());

        var unsent = await OkAsync(await hr.GetAsync($"{Evaluations}?employeeId={staff.Id}&isSent=false&maxResultCount=200"));
        unsent.GetProperty("items").EnumerateArray().Select(IdOf).ShouldContain(id);

        // Sending bundles this HR manager's unsent evaluations; a second send has nothing left.
        var report = await OkAsync(await hr.PostAsJsonAsync(Reports, new { note = "Week 2" }), HttpStatusCode.Created);
        var reportId = IdOf(report);
        report.GetProperty("evaluationsCount").GetInt32().ShouldBe(2);
        report.GetProperty("evaluations").EnumerateArray().Select(IdOf).ShouldBe(new[] { id, IdOf(second) });
        await ShouldFailWithAsync(await hr.PostAsJsonAsync(Reports, new { }), HrErrorCodes.HrEvaluation.NothingToSend);

        var sent = await OkAsync(await hr.GetAsync($"{Evaluations}/{id}"));
        sent.GetProperty("reportId").GetGuid().ShouldBe(reportId);
        (await OkAsync(await hr.GetAsync($"{Evaluations}?employeeId={staff.Id}&isSent=false&maxResultCount=200"))).GetProperty("items").EnumerateArray().Select(IdOf).ShouldNotContain(id);

        // The executive director is notified and reads everything, notes included.
        (await executive.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200")).GetProperty("items").EnumerateArray()
            .ShouldContain(x => Text(x, "templateCode") == "hrEvaluationReportSent" && x.GetProperty("resourceId").GetGuid() == reportId);
        var read = await OkAsync(await executive.GetAsync($"{Reports}/{reportId}"));
        Text(read, "note").ShouldBe("Week 2");
        Text(read.GetProperty("evaluations")[0], "notes").ShouldBe("Committed, should communicate more.");
        (await executive.GetFromJsonAsync<JsonElement>($"{Reports}?maxResultCount=200")).GetProperty("items").EnumerateArray().Select(IdOf).ShouldContain(reportId);

        // Another HR manager does not read this one.
        (await otherHr.GetAsync($"{Reports}/{reportId}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await otherHr.GetFromJsonAsync<JsonElement>($"{Reports}?maxResultCount=200")).GetProperty("items").EnumerateArray().Select(IdOf).ShouldNotContain(reportId);

        await DeactivateAllAsync(hr);
    }

    [Fact]
    public async Task One_evaluation_is_sent_alone_after_confirmation_and_only_once()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var hr = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.HrManager);
        var otherHr = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.HrManager);
        var executive = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.ExecutiveDirector);

        await DeactivateAllAsync(hr);
        var respect = IdOf(await OkAsync(await hr.PostAsJsonAsync(Criteria, new { nameAr = "الاحترام", nameEn = "Respect", sortOrder = 1 }), HttpStatusCode.Created));
        var first = IdOf(await OkAsync(await hr.PostAsJsonAsync(Evaluations, Body(staff.Id, (respect, 7))), HttpStatusCode.Created));
        var second = IdOf(await OkAsync(await hr.PostAsJsonAsync(Evaluations, Body(staff.Id, (respect, 9))), HttpStatusCode.Created));

        // Another HR manager cannot send it, and nobody but the HR manager may.
        (await otherHr.PostAsync($"{Evaluations}/{first}/send", null)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await executive.PostAsync($"{Evaluations}/{first}/send", null)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // Only that one goes, in a report of its own, and the executive director is notified.
        var report = await OkAsync(await hr.PostAsync($"{Evaluations}/{first}/send", null), HttpStatusCode.Created);
        report.GetProperty("evaluationsCount").GetInt32().ShouldBe(1);
        report.GetProperty("evaluations").EnumerateArray().Select(IdOf).ShouldBe(new[] { first });
        (await OkAsync(await hr.GetAsync($"{Evaluations}/{second}"))).GetProperty("reportId").ValueKind.ShouldBe(JsonValueKind.Null);
        (await executive.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200")).GetProperty("items").EnumerateArray()
            .ShouldContain(x => Text(x, "templateCode") == "hrEvaluationReportSent" && x.GetProperty("resourceId").GetGuid() == IdOf(report));

        // A second send is refused; the bulk send takes the rest.
        await ShouldFailWithAsync(await hr.PostAsync($"{Evaluations}/{first}/send", null), HrErrorCodes.HrEvaluation.AlreadySent);
        (await OkAsync(await hr.PostAsJsonAsync(Reports, new { }), HttpStatusCode.Created)).GetProperty("evaluations").EnumerateArray().Select(IdOf).ShouldBe(new[] { second });

        await DeactivateAllAsync(hr);
    }
}
