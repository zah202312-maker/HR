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
/// The reviewer's place in the organization (owner's decision 2026-11-07), the reviewer's rooms and the daily report
/// (decision 2026-11-11). Every test uses fresh reviewers, so their rooms and reports belong to the test alone.
/// </summary>
[Collection(HostCollection.Name)]
public class ReviewReportModuleTests
{
    private const string Url = "/api/app/review-report";
    private const string RoomUrl = "/api/app/review-room";

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

    private static List<Guid> MemberIds(JsonElement element) =>
        element.GetProperty("members").EnumerateArray().Select(IdOf).ToList();

    private async Task<(HttpClient Client, Guid UserId)> ReviewerAsync(EmployeeTestKit kit)
    {
        var (userId, userName) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee, HrRoles.Reviewer);
        await kit.AddFileForUserAsync(userId, kit.ExecutiveEmployeeId);
        return (TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, userName, TestUsers.Password)).AccessToken, "en"), userId);
    }

    private static object Room(string name, Guid managerId, params Guid[] members) =>
        new { name, managerEmployeeId = managerId, memberEmployeeIds = members };

    private static object Edit(JsonElement report, string? content, params object[] teams) => new
    {
        reportDate = Text(report, "reportDate"),
        content,
        teams,
        concurrencyStamp = Text(report, "concurrencyStamp")
    };

    private static object Grade(Guid roomId, string? backend, string? frontend, string? notes = null) => new
    {
        roomId,
        backendGrade = backend,
        backendNotes = backend == null ? null : "Backend " + backend,
        frontendGrade = frontend,
        frontendNotes = frontend == null ? null : "Frontend " + frontend,
        notes
    };

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
    public async Task The_reviewer_builds_rooms_from_a_department_manager_and_the_people_linked_to_them()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var first = await kit.AddStaffAsync(manager.Id);
        var second = await kit.AddStaffAsync(manager.Id);
        var otherManager = await kit.AddDepartmentManagerAsync();
        var stranger = await kit.AddStaffAsync(otherManager.Id);
        var staffClient = await kit.SignInAsync(first);

        var (reviewer, _) = await ReviewerAsync(kit);
        var (otherReviewer, _) = await ReviewerAsync(kit);
        var executive = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.ExecutiveDirector);

        // Only the reviewer works with rooms.
        (await staffClient.GetAsync(RoomUrl)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await executive.GetAsync(RoomUrl)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await executive.GetAsync($"{RoomUrl}/candidates")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await staffClient.PostAsJsonAsync(RoomUrl, Room("Team", manager.Id, first.Id))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // The candidates are the department managers with the people linked to them.
        var candidates = await OkAsync(await reviewer.GetAsync($"{RoomUrl}/candidates"));
        var candidate = candidates.EnumerateArray().Single(x => x.GetProperty("manager").GetProperty("id").GetGuid() == manager.Id);
        MemberIds(candidate).ShouldBe(new[] { first.Id, second.Id }, ignoreOrder: true);
        candidate.GetProperty("department").GetProperty("id").GetGuid().ShouldBe(kit.DepartmentId);

        // The lead must be a department manager, and every member linked to them.
        await ShouldFailWithAsync(await reviewer.PostAsJsonAsync(RoomUrl, Room("Team", first.Id, second.Id)), HrErrorCodes.ReviewRooms.NotADepartmentManager);
        await ShouldFailWithAsync(await reviewer.PostAsJsonAsync(RoomUrl, Room("Team", manager.Id, first.Id, stranger.Id)), HrErrorCodes.ReviewRooms.MemberNotLinked);
        (await reviewer.PostAsJsonAsync(RoomUrl, Room("Team", manager.Id))).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await reviewer.PostAsJsonAsync(RoomUrl, Room("", manager.Id, first.Id))).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var room = await OkAsync(await reviewer.PostAsJsonAsync(RoomUrl, Room("Mobile app", manager.Id, first.Id)), HttpStatusCode.Created);
        var roomId = IdOf(room);
        Text(room, "name").ShouldBe("Mobile app");
        room.GetProperty("manager").GetProperty("id").GetGuid().ShouldBe(manager.Id);
        MemberIds(room).ShouldBe(new[] { first.Id });

        // One room per manager for the same reviewer; another reviewer may have their own.
        await ShouldFailWithAsync(await reviewer.PostAsJsonAsync(RoomUrl, Room("Again", manager.Id, second.Id)), HrErrorCodes.ReviewRooms.ManagerAlreadyHasRoom);
        await OkAsync(await otherReviewer.PostAsJsonAsync(RoomUrl, Room("Theirs", manager.Id, second.Id)), HttpStatusCode.Created);
        (await otherReviewer.GetAsync($"{RoomUrl}/{roomId}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // Edit, with a stale stamp first.
        (await reviewer.PutAsJsonAsync($"{RoomUrl}/{roomId}", new { name = "Mobile", managerEmployeeId = manager.Id, memberEmployeeIds = new[] { first.Id }, concurrencyStamp = "stale" }))
            .StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var edited = await OkAsync(await reviewer.PutAsJsonAsync($"{RoomUrl}/{roomId}", new
        {
            name = "Mobile and web",
            managerEmployeeId = manager.Id,
            memberEmployeeIds = new[] { first.Id, second.Id },
            concurrencyStamp = Text(room, "concurrencyStamp")
        }));
        Text(edited, "name").ShouldBe("Mobile and web");
        MemberIds(edited).ShouldBe(new[] { first.Id, second.Id }, ignoreOrder: true);

        // The list holds the reviewer's own rooms only; a deleted room is gone.
        var other = await OkAsync(await reviewer.PostAsJsonAsync(RoomUrl, Room("Sales", otherManager.Id, stranger.Id)), HttpStatusCode.Created);
        var list = await OkAsync(await reviewer.GetAsync(RoomUrl));
        list.GetProperty("totalCount").GetInt64().ShouldBe(2);
        list.GetProperty("items").EnumerateArray().Select(IdOf).ShouldBe(new[] { roomId, IdOf(other) }, ignoreOrder: true);

        await OkAsync(await reviewer.DeleteAsync($"{RoomUrl}/{IdOf(other)}"), HttpStatusCode.NoContent);
        (await reviewer.GetAsync($"{RoomUrl}/{IdOf(other)}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await OkAsync(await reviewer.GetAsync(RoomUrl))).GetProperty("totalCount").GetInt64().ShouldBe(1);
    }

    [Fact]
    public async Task The_reviewer_writes_a_daily_report_on_each_room_and_the_executive_reads_it_once_submitted()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var staffClient = await kit.SignInAsync(staff);

        var (reviewer, _) = await ReviewerAsync(kit);
        var (otherReviewer, _) = await ReviewerAsync(kit);
        var executive = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.ExecutiveDirector);

        // Rooms come first; date rules and access.
        await ShouldFailWithAsync(await reviewer.PostAsJsonAsync(Url, new { reportDate = "2026-11-02" }), HrErrorCodes.ReviewReports.NoRooms);
        var room = await OkAsync(await reviewer.PostAsJsonAsync(RoomUrl, Room("Mobile app", manager.Id, staff.Id)), HttpStatusCode.Created);
        var roomId = IdOf(room);

        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(2).ToString("yyyy-MM-dd");
        await ShouldFailWithAsync(await reviewer.PostAsJsonAsync(Url, new { reportDate = tomorrow }), HrErrorCodes.ReviewReports.InvalidDate);
        (await reviewer.PostAsJsonAsync(Url, new { content = "No day" })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await staffClient.GetAsync(Url)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await staffClient.PostAsJsonAsync(Url, new { reportDate = "2026-11-02" })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await executive.PostAsJsonAsync(Url, new { reportDate = "2026-11-02" })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var draft = await OkAsync(await reviewer.PostAsJsonAsync(Url, new { reportDate = "2026-11-02" }), HttpStatusCode.Created);
        var id = IdOf(draft);
        Text(draft, "status").ShouldBe("draft");
        Text(draft, "reportDate").ShouldBe("2026-11-02");
        draft.GetProperty("content").ValueKind.ShouldBe(JsonValueKind.Null);
        draft.GetProperty("viewedAt").ValueKind.ShouldBe(JsonValueKind.Null);

        var team = draft.GetProperty("teams").EnumerateArray().Single();
        team.GetProperty("room").GetProperty("id").GetGuid().ShouldBe(roomId);
        Text(team.GetProperty("room"), "name").ShouldBe("Mobile app");
        team.GetProperty("manager").GetProperty("id").GetGuid().ShouldBe(manager.Id);
        team.GetProperty("department").GetProperty("id").GetGuid().ShouldBe(kit.DepartmentId);
        MemberIds(team).ShouldBe(new[] { staff.Id });
        team.GetProperty("backendGrade").ValueKind.ShouldBe(JsonValueKind.Null);
        team.GetProperty("frontendGrade").ValueKind.ShouldBe(JsonValueKind.Null);

        // One report a day.
        await ShouldFailWithAsync(await reviewer.PostAsJsonAsync(Url, new { reportDate = "2026-11-02" }), HrErrorCodes.ReviewReports.DateAlreadyReported);

        // Nobody else sees a draft.
        (await executive.GetAsync($"{Url}/{id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await otherReviewer.GetAsync($"{Url}/{id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await otherReviewer.PutAsJsonAsync($"{Url}/{id}", Edit(draft, null))).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await OkAsync(await executive.GetAsync($"{Url}?maxResultCount=200"))).GetProperty("items").EnumerateArray().Select(IdOf).ShouldNotContain(id);
        (await OkAsync(await reviewer.GetAsync($"{Url}?status=draft"))).GetProperty("items").EnumerateArray().Select(IdOf).ShouldBe(new[] { id });
        (await reviewer.GetAsync($"{Url}?sorting=content%20asc")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Draft rules: the text, then the grades of every room.
        await ShouldFailWithAsync(await reviewer.PostAsJsonAsync($"{Url}/{id}/submit", new { concurrencyStamp = Text(draft, "concurrencyStamp") }), HrErrorCodes.ReviewReports.ContentRequired);
        var written = await OkAsync(await reviewer.PutAsJsonAsync($"{Url}/{id}", Edit(draft, "The mobile team finished the requests screen.", Grade(roomId, "good", null))));
        Text(written, "content").ShouldBe("The mobile team finished the requests screen.");
        await ShouldFailWithAsync(await reviewer.PostAsJsonAsync($"{Url}/{id}/submit", new { concurrencyStamp = Text(written, "concurrencyStamp") }), HrErrorCodes.ReviewReports.IncompleteTeams);
        await ShouldFailWithAsync(await reviewer.PutAsJsonAsync($"{Url}/{id}", Edit(written, "x", Grade(Guid.NewGuid(), "good", "good"))), HrErrorCodes.ReviewReports.UnknownTeam);
        (await reviewer.PutAsJsonAsync($"{Url}/{id}", Edit(written, "x", Grade(roomId, "great", "good")))).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await reviewer.PutAsJsonAsync($"{Url}/{id}", new { reportDate = "2026-11-02", content = "x", teams = Array.Empty<object>(), concurrencyStamp = "stale" }))
            .StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var graded = await OkAsync(await reviewer.PutAsJsonAsync($"{Url}/{id}", Edit(written, "The mobile team finished the requests screen.", Grade(roomId, "veryGood", "average", "On time"))));
        var row = graded.GetProperty("teams").EnumerateArray().Single();
        Text(row, "backendGrade").ShouldBe("veryGood");
        Text(row, "backendNotes").ShouldBe("Backend veryGood");
        Text(row, "frontendGrade").ShouldBe("average");
        Text(row, "frontendNotes").ShouldBe("Frontend average");
        Text(row, "notes").ShouldBe("On time");

        // Changing the room later leaves the report's snapshot alone.
        await OkAsync(await reviewer.PutAsJsonAsync($"{RoomUrl}/{roomId}", new { name = "Renamed", managerEmployeeId = manager.Id, memberEmployeeIds = new[] { staff.Id }, concurrencyStamp = Text(room, "concurrencyStamp") }));
        Text((await OkAsync(await reviewer.GetAsync($"{Url}/{id}"))).GetProperty("teams")[0].GetProperty("room"), "name").ShouldBe("Mobile app");

        // Submit, then the executive reads it and the first look is kept.
        var submitted = await OkAsync(await reviewer.PostAsJsonAsync($"{Url}/{id}/submit", new { concurrencyStamp = Text(graded, "concurrencyStamp") }));
        Text(submitted, "status").ShouldBe("submitted");
        submitted.GetProperty("submittedAt").ValueKind.ShouldBe(JsonValueKind.String);

        var notices = await executive.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200");
        notices.GetProperty("items").EnumerateArray()
            .Any(x => Text(x, "templateCode") == "reviewReportSubmitted" && Text(x, "resourceType") == "reviewReport" && x.GetProperty("resourceId").GetGuid() == id)
            .ShouldBeTrue();

        var listed = await OkAsync(await executive.GetAsync($"{Url}?dateFrom=2026-11-02&dateTo=2026-11-02&maxResultCount=200"));
        var summary = listed.GetProperty("items").EnumerateArray().Single(x => IdOf(x) == id);
        summary.GetProperty("teamsCount").GetInt32().ShouldBe(1);
        Text(summary, "reportDate").ShouldBe("2026-11-02");
        summary.GetProperty("viewedAt").ValueKind.ShouldBe(JsonValueKind.Null);
        (await OkAsync(await executive.GetAsync($"{Url}?dateFrom=2026-11-03&maxResultCount=200"))).GetProperty("items").EnumerateArray().Select(IdOf).ShouldNotContain(id);

        (await OkAsync(await executive.GetAsync($"{Url}/{id}"))).GetProperty("viewedAt").ValueKind.ShouldBe(JsonValueKind.String);
        var viewedAt = Text(await OkAsync(await executive.GetAsync($"{Url}/{id}")), "viewedAt");
        Text(await OkAsync(await executive.GetAsync($"{Url}/{id}")), "viewedAt").ShouldBe(viewedAt);
        Text(await OkAsync(await reviewer.GetAsync($"{Url}/{id}")), "viewedAt").ShouldBe(viewedAt);

        // A submitted report is final.
        var latest = await OkAsync(await reviewer.GetAsync($"{Url}/{id}"));
        await ShouldFailWithAsync(await reviewer.PutAsJsonAsync($"{Url}/{id}", Edit(latest, "late edit")), HrErrorCodes.ReviewReports.AlreadySubmitted);
        await ShouldFailWithAsync(await reviewer.DeleteAsync($"{Url}/{id}"), HrErrorCodes.ReviewReports.AlreadySubmitted);
        await ShouldFailWithAsync(await reviewer.PostAsJsonAsync($"{Url}/{id}/submit", new { concurrencyStamp = Text(latest, "concurrencyStamp") }), HrErrorCodes.ReviewReports.AlreadySubmitted);
    }

    [Fact]
    public async Task Deleting_a_draft_frees_its_day_and_a_report_takes_every_room_in_lead_order()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var firstManager = await kit.AddDepartmentManagerAsync();
        var secondManager = await kit.AddDepartmentManagerAsync();
        var firstStaff = await kit.AddStaffAsync(firstManager.Id);
        var secondStaff = await kit.AddStaffAsync(secondManager.Id);
        var (reviewer, _) = await ReviewerAsync(kit);

        await OkAsync(await reviewer.PostAsJsonAsync(RoomUrl, Room("B", secondManager.Id, secondStaff.Id)), HttpStatusCode.Created);
        await OkAsync(await reviewer.PostAsJsonAsync(RoomUrl, Room("A", firstManager.Id, firstStaff.Id)), HttpStatusCode.Created);

        var draft = await OkAsync(await reviewer.PostAsJsonAsync(Url, new { reportDate = "2026-11-03", content = "Draft" }), HttpStatusCode.Created);
        var leads = draft.GetProperty("teams").EnumerateArray().Select(x => Text(x.GetProperty("manager"), "fullName")).ToList();
        leads.Count.ShouldBe(2);
        leads.ShouldBe(leads.OrderBy(x => x, StringComparer.Ordinal).ToList());

        // Moving a draft onto a taken day is refused; deleting it frees its own day.
        var other = await OkAsync(await reviewer.PostAsJsonAsync(Url, new { reportDate = "2026-11-04" }), HttpStatusCode.Created);
        await ShouldFailWithAsync(await reviewer.PutAsJsonAsync($"{Url}/{IdOf(other)}", new { reportDate = "2026-11-03", content = (string?)null, teams = Array.Empty<object>(), concurrencyStamp = Text(other, "concurrencyStamp") }),
            HrErrorCodes.ReviewReports.DateAlreadyReported);

        await OkAsync(await reviewer.DeleteAsync($"{Url}/{IdOf(draft)}"), HttpStatusCode.NoContent);
        (await reviewer.GetAsync($"{Url}/{IdOf(draft)}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        await OkAsync(await reviewer.PostAsJsonAsync(Url, new { reportDate = "2026-11-03" }), HttpStatusCode.Created);
    }
}
