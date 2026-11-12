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
/// The reviewer's stand-in (decision 2026-11-11): the reviewer delegates the daily report only (scope
/// reviewReports), and the stand-in manages the rooms and writes, edits and submits reports for the reviewer during the
/// delegation.
/// </summary>
[Collection(HostCollection.Name)]
public class ReviewerStandInTests
{
    private const string Url = "/api/app/review-report";

    private readonly HostFixture _host;

    public ReviewerStandInTests(HostFixture host)
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

    private static object Delegation(Guid toUserId, string scope) =>
        new { toUserId, scope, startsAt = DateTime.UtcNow, endsAt = DateTime.UtcNow.AddDays(2), reason = "Leave" };

    [Fact]
    public async Task The_stand_in_writes_and_submits_follow_up_reports_for_the_reviewer_during_the_delegation()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var standIn = await kit.AddStaffAsync(manager.Id);
        var standInClient = await kit.SignInAsync(standIn);
        var outsider = await kit.SignInAsync(await kit.AddStaffAsync(manager.Id));
        var reviewer = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.Reviewer);
        var reviewerId = (await OkAsync(await reviewer.GetAsync("/api/app/me"))).GetProperty("userId").GetGuid();

        // Before any delegation the stand-in has no access.
        (await standInClient.GetAsync(Url)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await standInClient.PostAsJsonAsync(Url, new { reportDate = "2026-11-05", onBehalfOfUserId = reviewerId }))
            .StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await standInClient.GetAsync("/api/app/review-room")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // The reviewer delegates the follow-up reports only; nobody else uses that scope.
        await ShouldFailWithAsync(await reviewer.PostAsJsonAsync("/api/app/delegation", Delegation(standIn.UserId, "requests")), HrErrorCodes.Delegation.DelegateNotEligible);
        await ShouldFailWithAsync(await kit.Hr.PostAsJsonAsync("/api/app/delegation", Delegation(standIn.UserId, "reviewReports")), HrErrorCodes.Delegation.DelegateNotEligible);
        var delegation = await OkAsync(await reviewer.PostAsJsonAsync("/api/app/delegation", Delegation(standIn.UserId, "reviewReports")), HttpStatusCode.Created);

        // The stand-in builds a room for the reviewer, then writes the report: the reviewer's, prepared by the stand-in.
        var room = await OkAsync(await standInClient.PostAsJsonAsync("/api/app/review-room", new
        {
            onBehalfOfUserId = reviewerId, name = "Team", managerEmployeeId = manager.Id, memberEmployeeIds = new[] { standIn.Id }
        }), HttpStatusCode.Created);
        room.GetProperty("reviewer").GetProperty("id").GetGuid().ShouldBe(reviewerId);
        (await reviewer.GetFromJsonAsync<JsonElement>("/api/app/review-room")).GetProperty("items").EnumerateArray().Select(IdOf).ShouldContain(IdOf(room));
        (await outsider.GetAsync("/api/app/review-room")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var draft = await OkAsync(await standInClient.PostAsJsonAsync(Url, new { reportDate = "2026-11-05", onBehalfOfUserId = reviewerId }), HttpStatusCode.Created);
        draft.GetProperty("reviewer").GetProperty("id").GetGuid().ShouldBe(reviewerId);
        draft.GetProperty("preparedBy").GetProperty("id").GetGuid().ShouldBe(standIn.UserId);
        var id = IdOf(draft);
        (await reviewer.GetFromJsonAsync<JsonElement>($"{Url}?maxResultCount=200")).GetProperty("items").EnumerateArray().Select(IdOf).ShouldContain(id);
        (await standInClient.GetFromJsonAsync<JsonElement>($"{Url}?maxResultCount=200")).GetProperty("items").EnumerateArray().Select(IdOf).ShouldContain(id);
        (await outsider.GetAsync($"{Url}/{id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // The stand-in grades the room and submits it.
        var current = await OkAsync(await standInClient.PutAsJsonAsync($"{Url}/{id}", new
        {
            reportDate = "2026-11-05",
            content = "Prepared by the stand-in",
            teams = new[] { new { roomId = IdOf(room), backendGrade = "good", backendNotes = (string?)null, frontendGrade = "excellent", frontendNotes = (string?)null, notes = (string?)null } },
            concurrencyStamp = Text(draft, "concurrencyStamp")
        }));

        var submitted = await OkAsync(await standInClient.PostAsJsonAsync($"{Url}/{id}/submit", new { concurrencyStamp = Text(current, "concurrencyStamp") }));
        Text(submitted, "status").ShouldBe("submitted");
        submitted.GetProperty("preparedBy").GetProperty("id").GetGuid().ShouldBe(standIn.UserId);

        // Once the delegation is cancelled the stand-in loses access again.
        (await reviewer.DeleteAsync($"/api/app/delegation/{IdOf(delegation)}")).IsSuccessStatusCode.ShouldBeTrue();
        (await standInClient.GetAsync(Url)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await standInClient.GetAsync($"{Url}/{id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await standInClient.GetAsync($"/api/app/review-room/{IdOf(room)}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // The reviewer's own reports show the reviewer as the preparer.
        var own = await OkAsync(await reviewer.PostAsJsonAsync(Url, new { reportDate = "2026-11-06" }), HttpStatusCode.Created);
        own.GetProperty("preparedBy").GetProperty("id").GetGuid().ShouldBe(reviewerId);
        (await reviewer.DeleteAsync($"{Url}/{IdOf(own)}")).IsSuccessStatusCode.ShouldBeTrue();
    }
}
