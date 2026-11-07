using System;
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
using HR_Mangment2.Requests;
using Shouldly;
using Xunit;

namespace HR_Mangment2;

/// <summary>
/// The reviewer's place in the organization and the follow-up report (owner's decision 2026-11-07). The database is
/// shared with the other tests, so the report holds every department manager they created: rows are found by id.
/// </summary>
[Collection(HostCollection.Name)]
public class ReviewReportModuleTests
{
    private const string Url = "/api/app/review-report";

    private readonly HostFixture _host;

    public ReviewReportModuleTests(HostFixture host)
    {
        _host = host;
    }

    private static async Task<JsonElement> OkAsync(HttpResponseMessage response, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe(expected, body);
        return body.Length == 0 ? default : JsonDocument.Parse(body).RootElement;
    }

    private static async Task ShouldFailWithAsync(HttpResponseMessage response, string code)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe((HttpStatusCode)422, body);
        JsonDocument.Parse(body).RootElement.GetProperty("error").GetProperty("code").GetString().ShouldBe(code);
    }

    private static string Text(JsonElement element, string property) => element.GetProperty(property).GetString()!;

    private static Guid IdOf(JsonElement element) => element.GetProperty("id").GetGuid();

    private async Task<(HttpClient Client, Guid UserId)> ReviewerAsync(EmployeeTestKit kit)
    {
        var (userId, userName) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee, HrRoles.Reviewer);
        await kit.AddFileForUserAsync(userId, kit.ExecutiveEmployeeId);
        return (TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, userName, TestUsers.Password)).AccessToken, "en"), userId);
    }

    private static object Week(string summary = "Weekly follow-up") =>
        new { periodType = "weekly", periodFrom = "2026-11-01", periodTo = "2026-11-07", summary };

    /// <summary>Saves an assessment for every team, a page at a time, then returns the latest report.</summary>
    private static async Task<JsonElement> AssessAllAsync(HttpClient reviewer, JsonElement report, Func<Guid, (int Percent, string? Notes, string? Projects)> assess)
    {
        var id = IdOf(report);
        var managers = report.GetProperty("teams").EnumerateArray().Select(x => x.GetProperty("manager").GetProperty("id").GetGuid()).ToList();
        var current = report;
        foreach (var page in managers.Chunk(200))
        {
            current = await OkAsync(await reviewer.PutAsJsonAsync($"{Url}/{id}", new
            {
                periodType = Text(current, "periodType"),
                periodFrom = Text(current, "periodFrom"),
                periodTo = Text(current, "periodTo"),
                summary = current.GetProperty("summary").GetString(),
                teams = page.Select(managerId =>
                {
                    var (percent, notes, projects) = assess(managerId);
                    return new { managerEmployeeId = managerId, completionPercent = (int?)percent, notes, projectNotes = projects };
                }).ToList(),
                concurrencyStamp = Text(current, "concurrencyStamp")
            }));
        }

        return current;
    }

    [Fact]
    public async Task A_reviewer_reports_to_the_executive_or_the_hr_manager_and_starts_at_hr()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var departmentManager = await kit.AddDepartmentManagerAsync();

        var underManager = await kit.Hr.PostAsJsonAsync("/api/app/employee", kit.Body(departmentManager.Id, new[] { HrRoles.Reviewer }));
        await ShouldFailWithAsync(underManager, HrErrorCodes.Employee.InvalidManagerForRole);

        var (hrUserId, _) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee, HrRoles.HrManager);
        var hrFileId = await kit.AddFileForUserAsync(hrUserId, kit.ExecutiveEmployeeId);
        await kit.AddAsync(hrFileId, new[] { HrRoles.Reviewer });

        var reviewer = await kit.AddAsync(kit.ExecutiveEmployeeId, new[] { HrRoles.Reviewer });
        var client = await kit.SignInAsync(reviewer);

        var types = await client.GetFromJsonAsync<JsonElement>("/api/app/request-type?maxResultCount=200");
        var typeId = types.GetProperty("items").EnumerateArray().Single(x => Text(x, "code") == RequestTypeCodes.AdministrativeNeed).GetProperty("id").GetGuid();
        var request = await OkAsync(await client.PostAsJsonAsync("/api/app/request", new { requestTypeId = typeId, description = "A new desk" }), HttpStatusCode.Created);
        Text(request, "status").ShouldBe("pendingHr");
        Text(request, "currentApproverRole").ShouldBe("hrManager");

        var memo = await OkAsync(await client.PostAsJsonAsync("/api/app/memo", new { kind = "leave", content = "A day off next week" }), HttpStatusCode.Created);
        Text(memo, "status").ShouldBe("pendingHr");
    }

    [Fact]
    public async Task The_reviewer_writes_a_team_report_and_the_executive_reads_it_once_submitted()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        await kit.AddStaffAsync(manager.Id);
        var staffClient = await kit.SignInAsync(staff);

        var (reviewer, _) = await ReviewerAsync(kit);
        var (otherReviewer, _) = await ReviewerAsync(kit);
        var executive = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.ExecutiveDirector);

        // Period rules and access.
        await ShouldFailWithAsync(
            await reviewer.PostAsJsonAsync(Url, new { periodType = "weekly", periodFrom = "2026-11-01", periodTo = "2026-11-06" }),
            HrErrorCodes.ReviewReports.InvalidPeriod);
        await ShouldFailWithAsync(
            await reviewer.PostAsJsonAsync(Url, new { periodType = "monthly", periodFrom = "2026-11-02", periodTo = "2026-11-30" }),
            HrErrorCodes.ReviewReports.InvalidPeriod);
        (await reviewer.PostAsJsonAsync(Url, new { periodFrom = "2026-11-01", periodTo = "2026-11-01" })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await staffClient.GetAsync(Url)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await staffClient.PostAsJsonAsync(Url, Week())).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await executive.PostAsJsonAsync(Url, Week())).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var draft = await OkAsync(await reviewer.PostAsJsonAsync(Url, Week()), HttpStatusCode.Created);
        var id = IdOf(draft);
        Text(draft, "status").ShouldBe("draft");
        Text(draft, "periodType").ShouldBe("weekly");
        draft.GetProperty("averageCompletion").ValueKind.ShouldBe(JsonValueKind.Null);
        draft.GetProperty("viewedAt").ValueKind.ShouldBe(JsonValueKind.Null);

        var team = draft.GetProperty("teams").EnumerateArray().Single(x => x.GetProperty("manager").GetProperty("id").GetGuid() == manager.Id);
        team.GetProperty("membersCount").GetInt32().ShouldBe(2);
        team.GetProperty("completionPercent").ValueKind.ShouldBe(JsonValueKind.Null);
        team.GetProperty("department").GetProperty("id").GetGuid().ShouldBe(kit.DepartmentId);

        // Nobody else sees a draft.
        (await executive.GetAsync($"{Url}/{id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await otherReviewer.GetAsync($"{Url}/{id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await otherReviewer.PutAsJsonAsync($"{Url}/{id}", new { periodType = "weekly", periodFrom = "2026-11-01", periodTo = "2026-11-07", summary = (string?)null, teams = Array.Empty<object>(), concurrencyStamp = Text(draft, "concurrencyStamp") }))
            .StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var executiveList = await OkAsync(await executive.GetAsync($"{Url}?maxResultCount=200"));
        executiveList.GetProperty("items").EnumerateArray().Select(IdOf).ShouldNotContain(id);

        var mine = await OkAsync(await reviewer.GetAsync($"{Url}?status=draft&maxResultCount=200"));
        mine.GetProperty("items").EnumerateArray().Select(IdOf).ShouldContain(id);
        (await (await otherReviewer.GetAsync($"{Url}?maxResultCount=200")).Content.ReadAsStringAsync()).ShouldNotContain(id.ToString());
        (await reviewer.GetAsync($"{Url}?sorting=summary%20asc")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Draft rules.
        await ShouldFailWithAsync(await reviewer.PostAsJsonAsync($"{Url}/{id}/submit", new { concurrencyStamp = Text(draft, "concurrencyStamp") }), HrErrorCodes.ReviewReports.IncompleteTeams);
        await ShouldFailWithAsync(
            await reviewer.PutAsJsonAsync($"{Url}/{id}", new
            {
                periodType = "weekly", periodFrom = "2026-11-01", periodTo = "2026-11-07", summary = "x",
                teams = new[] { new { managerEmployeeId = staff.Id, completionPercent = (int?)50, notes = (string?)null, projectNotes = (string?)null } },
                concurrencyStamp = Text(draft, "concurrencyStamp")
            }),
            HrErrorCodes.ReviewReports.UnknownTeam);
        (await reviewer.PutAsJsonAsync($"{Url}/{id}", new
        {
            periodType = "weekly", periodFrom = "2026-11-01", periodTo = "2026-11-07", summary = "x",
            teams = new[] { new { managerEmployeeId = manager.Id, completionPercent = (int?)101, notes = (string?)null, projectNotes = (string?)null } },
            concurrencyStamp = Text(draft, "concurrencyStamp")
        })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await reviewer.PutAsJsonAsync($"{Url}/{id}", new
        {
            periodType = "weekly", periodFrom = "2026-11-01", periodTo = "2026-11-07", summary = "x",
            teams = Array.Empty<object>(),
            concurrencyStamp = "stale"
        })).StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var assessed = await AssessAllAsync(reviewer, draft, managerId => managerId == manager.Id ? (90, "On time", "Archive project late") : (70, null, null));
        var row = assessed.GetProperty("teams").EnumerateArray().Single(x => x.GetProperty("manager").GetProperty("id").GetGuid() == manager.Id);
        row.GetProperty("completionPercent").GetInt32().ShouldBe(90);
        Text(row, "notes").ShouldBe("On time");
        Text(row, "projectNotes").ShouldBe("Archive project late");
        var values = assessed.GetProperty("teams").EnumerateArray().Select(x => x.GetProperty("completionPercent").GetInt32()).ToList();
        assessed.GetProperty("averageCompletion").GetInt32().ShouldBe((int)Math.Round(values.Average(), MidpointRounding.AwayFromZero));

        // Submit, then the executive reads it and the first look is kept.
        var submitted = await OkAsync(await reviewer.PostAsJsonAsync($"{Url}/{id}/submit", new { concurrencyStamp = Text(assessed, "concurrencyStamp") }));
        Text(submitted, "status").ShouldBe("submitted");
        submitted.GetProperty("submittedAt").ValueKind.ShouldBe(JsonValueKind.String);

        var notices = await executive.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200");
        var notice = notices.GetProperty("items").EnumerateArray().First(x => Text(x, "templateCode") == "reviewReportSubmitted");
        Text(notice, "resourceType").ShouldBe("reviewReport");
        notice.GetProperty("resourceId").GetGuid().ShouldBe(id);

        var listed = await OkAsync(await executive.GetAsync($"{Url}?periodType=weekly&maxResultCount=200"));
        var summary = listed.GetProperty("items").EnumerateArray().Single(x => IdOf(x) == id);
        summary.GetProperty("teamsCount").GetInt32().ShouldBe(values.Count);
        summary.GetProperty("viewedAt").ValueKind.ShouldBe(JsonValueKind.Null);

        var firstLook = await OkAsync(await executive.GetAsync($"{Url}/{id}"));
        var viewedAt = Text(firstLook, "viewedAt");
        Text(await OkAsync(await executive.GetAsync($"{Url}/{id}")), "viewedAt").ShouldBe(viewedAt);
        Text(await OkAsync(await reviewer.GetAsync($"{Url}/{id}")), "viewedAt").ShouldBe(viewedAt);

        // A submitted report is final.
        var latest = await OkAsync(await reviewer.GetAsync($"{Url}/{id}"));
        await ShouldFailWithAsync(
            await reviewer.PutAsJsonAsync($"{Url}/{id}", new { periodType = "weekly", periodFrom = "2026-11-01", periodTo = "2026-11-07", summary = "late edit", teams = Array.Empty<object>(), concurrencyStamp = Text(latest, "concurrencyStamp") }),
            HrErrorCodes.ReviewReports.AlreadySubmitted);
        await ShouldFailWithAsync(await reviewer.DeleteAsync($"{Url}/{id}"), HrErrorCodes.ReviewReports.AlreadySubmitted);
        await ShouldFailWithAsync(await reviewer.PostAsJsonAsync($"{Url}/{id}/submit", new { concurrencyStamp = Text(latest, "concurrencyStamp") }), HrErrorCodes.ReviewReports.AlreadySubmitted);
    }

    [Fact]
    public async Task A_draft_can_be_deleted_and_daily_and_monthly_periods_are_checked()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        await kit.AddDepartmentManagerAsync();
        var (reviewer, _) = await ReviewerAsync(kit);

        var daily = await OkAsync(await reviewer.PostAsJsonAsync(Url, new { periodType = "daily", periodFrom = "2026-11-05", periodTo = "2026-11-05" }), HttpStatusCode.Created);
        var monthly = await OkAsync(await reviewer.PostAsJsonAsync(Url, new { periodType = "monthly", periodFrom = "2026-02-01", periodTo = "2026-02-28" }), HttpStatusCode.Created);
        Text(monthly, "periodTo").ShouldBe("2026-02-28");
        await ShouldFailWithAsync(
            await reviewer.PostAsJsonAsync(Url, new { periodType = "daily", periodFrom = "2026-11-05", periodTo = "2026-11-06" }),
            HrErrorCodes.ReviewReports.InvalidPeriod);

        await OkAsync(await reviewer.DeleteAsync($"{Url}/{IdOf(daily)}"), HttpStatusCode.NoContent);
        (await reviewer.GetAsync($"{Url}/{IdOf(daily)}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await reviewer.GetAsync($"{Url}/{IdOf(monthly)}")).StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
