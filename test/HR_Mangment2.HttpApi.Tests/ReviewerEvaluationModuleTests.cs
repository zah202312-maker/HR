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

/// <summary>
/// The reviewer's individual evaluations (decision 2026-11-11): a grade from weak to excellent and an optional reason per
/// heading, a date the reviewer picks, no numbers and no average, sent to the executive director like the HR ones.
/// </summary>
[Collection(HostCollection.Name)]
public class ReviewerEvaluationModuleTests
{
    private const string Criteria = "/api/app/reviewer-evaluation-criterion";
    private const string Evaluations = "/api/app/reviewer-evaluation";
    private const string Reports = "/api/app/reviewer-evaluation-report";

    private readonly HostFixture _host;

    public ReviewerEvaluationModuleTests(HostFixture host)
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

    private static object Body(Guid employeeId, params (Guid Criterion, string Grade, string? Reason)[] items) => new
    {
        employeeId,
        evaluationDate = "2026-11-10",
        items = items.Select(x => new { criterionId = x.Criterion, grade = x.Grade, reason = x.Reason }).ToArray(),
        notes = "Committed."
    };

    private static async Task DeactivateAllAsync(HttpClient reviewer)
    {
        var active = await reviewer.GetFromJsonAsync<JsonElement>($"{Criteria}?isActive=true&maxResultCount=200");
        foreach (var c in active.GetProperty("items").EnumerateArray())
        {
            await OkAsync(await reviewer.PutAsJsonAsync($"{Criteria}/{IdOf(c)}",
                new { nameAr = Text(c, "nameAr"), nameEn = Text(c, "nameEn"), sortOrder = c.GetProperty("sortOrder").GetInt32(), isActive = false, concurrencyStamp = Text(c, "concurrencyStamp") }));
        }
    }

    [Fact]
    public async Task The_reviewer_grades_with_reasons_and_no_numbers_and_sends_to_the_executive()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var staffClient = await kit.SignInAsync(staff);
        var reviewer = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.Reviewer);
        var executive = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.ExecutiveDirector);

        // Only the reviewer manages headings and evaluates; the HR manager has his own.
        (await kit.Hr.GetAsync(Criteria)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await executive.PostAsJsonAsync(Evaluations, Body(staff.Id, (Guid.NewGuid(), "good", null)))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        await DeactivateAllAsync(reviewer);
        var respect = IdOf(await OkAsync(await reviewer.PostAsJsonAsync(Criteria, new { nameAr = "الاحترام", nameEn = "Respect", sortOrder = 1 }), HttpStatusCode.Created));
        var quality = IdOf(await OkAsync(await reviewer.PostAsJsonAsync(Criteria, new { nameAr = "جودة العمل", nameEn = "Quality", sortOrder = 2 }), HttpStatusCode.Created));

        // The picker lists every employee the reviewer may evaluate, not only department managers; never the executive.
        async Task<bool> ListedAsync(Guid employeeId)
        {
            var name = Text(await OkAsync(await kit.Hr.GetAsync($"/api/app/employee/{employeeId}")), "fullNameAr");
            var page = await OkAsync(await reviewer.GetAsync($"{Evaluations}/candidate?search={Uri.EscapeDataString(name)}&maxResultCount=200"));
            return page.GetProperty("items").EnumerateArray().Any(x => IdOf(x) == employeeId);
        }

        (await ListedAsync(staff.Id)).ShouldBeTrue();
        (await ListedAsync(manager.Id)).ShouldBeTrue();
        (await ListedAsync(kit.ExecutiveEmployeeId)).ShouldBeFalse();
        (await kit.Hr.GetAsync($"{Evaluations}/candidate")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // Every active heading exactly once, a known grade, and an eligible employee.
        await ShouldFailWithAsync(await reviewer.PostAsJsonAsync(Evaluations, Body(staff.Id, (respect, "good", null))), HrErrorCodes.ReviewerEvaluation.ItemsIncomplete);
        await ShouldFailWithAsync(await reviewer.PostAsJsonAsync(Evaluations, Body(staff.Id, (respect, "good", null), (quality, "good", null), (Guid.NewGuid(), "good", null))), HrErrorCodes.ReviewerEvaluation.CriterionNotFound);
        await ShouldFailWithAsync(await reviewer.PostAsJsonAsync(Evaluations, Body(kit.ExecutiveEmployeeId, (respect, "good", null), (quality, "good", null))), HrErrorCodes.ReviewerEvaluation.EmployeeNotAllowed);
        (await reviewer.PostAsJsonAsync(Evaluations, Body(staff.Id, (respect, "superb", null), (quality, "good", null)))).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await reviewer.PostAsJsonAsync(Evaluations, new { employeeId = staff.Id, items = new[] { new { criterionId = respect, grade = "good" } } })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var created = await OkAsync(await reviewer.PostAsJsonAsync(Evaluations, Body(staff.Id, (quality, "veryGood", "Delivers on time"), (respect, "excellent", null))), HttpStatusCode.Created);
        var id = IdOf(created);
        Text(created, "evaluationDate").ShouldBe("2026-11-10");
        created.TryGetProperty("averageScore", out _).ShouldBeFalse();
        var items = created.GetProperty("items").EnumerateArray().ToList();
        items.Select(x => Text(x, "criterionName")).ShouldBe(new[] { "Respect", "Quality" });
        items.Select(x => Text(x, "grade")).ShouldBe(new[] { "excellent", "veryGood" });
        items[0].GetProperty("reason").ValueKind.ShouldBe(JsonValueKind.Null);
        Text(items[1], "reason").ShouldBe("Delivers on time");

        // The employee sees nothing.
        (await staffClient.GetAsync($"{Evaluations}/{id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await staffClient.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200")).GetRawText().ShouldNotContain(id.ToString());

        // One alone, after confirmation; then the rest together.
        var second = IdOf(await OkAsync(await reviewer.PostAsJsonAsync(Evaluations, Body(staff.Id, (respect, "weak", "Late replies"), (quality, "acceptable", null))), HttpStatusCode.Created));
        var single = await OkAsync(await reviewer.PostAsync($"{Evaluations}/{id}/send", null), HttpStatusCode.Created);
        single.GetProperty("evaluations").EnumerateArray().Select(IdOf).ShouldBe(new[] { id });
        Text(single, "firstEvaluationDate").ShouldBe("2026-11-10");
        await ShouldFailWithAsync(await reviewer.PostAsync($"{Evaluations}/{id}/send", null), HrErrorCodes.ReviewerEvaluation.AlreadySent);
        var bulk = await OkAsync(await reviewer.PostAsJsonAsync(Reports, new { note = "This week" }), HttpStatusCode.Created);
        bulk.GetProperty("evaluations").EnumerateArray().Select(IdOf).ShouldBe(new[] { second });
        await ShouldFailWithAsync(await reviewer.PostAsJsonAsync(Reports, new { }), HrErrorCodes.ReviewerEvaluation.NothingToSend);

        // The executive director is notified and reads the grades and reasons.
        (await executive.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200")).GetProperty("items").EnumerateArray()
            .ShouldContain(x => Text(x, "templateCode") == "reviewerEvaluationReportSent" && x.GetProperty("resourceId").GetGuid() == IdOf(bulk));
        var read = await OkAsync(await executive.GetAsync($"{Reports}/{IdOf(bulk)}"));
        Text(read.GetProperty("evaluations")[0].GetProperty("items")[0], "reason").ShouldBe("Late replies");
        (await kit.Hr.GetAsync($"{Reports}/{IdOf(bulk)}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        await DeactivateAllAsync(reviewer);
    }
}
