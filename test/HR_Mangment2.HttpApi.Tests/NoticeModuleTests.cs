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

/// <summary>Notice_* (decision 2026-11-10): HR manager or executive to employees chosen by name.</summary>
[Collection(HostCollection.Name)]
public class NoticeModuleTests
{
    private readonly HostFixture _host;

    public NoticeModuleTests(HostFixture host)
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

    private static object Body(params Guid[] recipients) => new { recipientEmployeeIds = recipients, title = "Visit HR", content = "Please visit HR on Sunday at 10:00." };

    private static async Task<long> SentCountAsync(HttpClient client) =>
        (await client.GetFromJsonAsync<JsonElement>("/api/app/notice/sent?maxResultCount=1")).GetProperty("totalCount").GetInt64();

    [Fact]
    public async Task Only_hr_and_the_executive_send_and_only_chosen_people_receive()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var managerClient = await kit.SignInAsync(manager);
        var staff = await kit.AddStaffAsync(manager.Id);
        var staffClient = await kit.SignInAsync(staff);
        var other = await kit.AddStaffAsync(manager.Id);
        var otherClient = await kit.SignInAsync(other);
        var hr = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.HrManager);
        var executive = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.ExecutiveDirector);

        (await staffClient.PostAsJsonAsync("/api/app/notice", Body(other.Id))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await managerClient.PostAsJsonAsync("/api/app/notice", Body(staff.Id))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await staffClient.GetAsync("/api/app/notice/sent")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // Decision 2026-11-11: the reviewer sends notices like HR, to any fit employee, and follows the receipts.
        var reviewer = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.Reviewer);
        var third = await kit.AddStaffAsync(manager.Id);
        var thirdClient = await kit.SignInAsync(third);
        var fromReviewer = await OkAsync(await reviewer.PostAsJsonAsync("/api/app/notice", Body(third.Id)), HttpStatusCode.Created);
        (await reviewer.GetFromJsonAsync<JsonElement>("/api/app/notice/sent")).GetProperty("items").EnumerateArray().Select(IdOf).ShouldContain(IdOf(fromReviewer));
        await OkAsync(await reviewer.GetAsync($"/api/app/notice/{IdOf(fromReviewer)}/receipt"));
        (await thirdClient.GetFromJsonAsync<JsonElement>("/api/app/notice")).GetProperty("items").EnumerateArray().Select(IdOf).ShouldContain(IdOf(fromReviewer));

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/app/notice") { Content = JsonContent.Create(Body(staff.Id, manager.Id)) };
        var key = Guid.NewGuid().ToString();
        request.Headers.Add("Idempotency-Key", key);
        var sent = await OkAsync(await hr.SendAsync(request), HttpStatusCode.Created);
        sent.GetProperty("recipientsCount").GetInt32().ShouldBe(2);
        sent.GetProperty("acknowledgedCount").GetInt32().ShouldBe(0);
        Text(sent, "title").ShouldBe("Visit HR");
        var id = IdOf(sent);

        // The same key returns the same notice and sends nothing new.
        var repeat = new HttpRequestMessage(HttpMethod.Post, "/api/app/notice") { Content = JsonContent.Create(Body(staff.Id, manager.Id)) };
        repeat.Headers.Add("Idempotency-Key", key);
        IdOf(await OkAsync(await hr.SendAsync(repeat), HttpStatusCode.Created)).ShouldBe(id);

        // Someone not chosen sees nothing, and neither does the sender as a reader.
        (await otherClient.GetAsync($"/api/app/notice/{id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await otherClient.PostAsync($"/api/app/notice/{id}/acknowledge", null)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await otherClient.GetFromJsonAsync<JsonElement>("/api/app/notice")).GetProperty("totalCount").GetInt64().ShouldBe(0);
        (await hr.GetAsync($"/api/app/notice/{id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // The recipient: notification, inbox item, details.
        var received = await OkAsync(await staffClient.GetAsync($"/api/app/notice/{id}"));
        Text(received, "content").ShouldBe("Please visit HR on Sunday at 10:00.");
        received.GetProperty("acknowledgedAt").ValueKind.ShouldBe(JsonValueKind.Null);
        received.TryGetProperty("sender", out var sender).ShouldBeTrue();
        sender.GetProperty("id").GetGuid().ShouldNotBe(Guid.Empty);

        var notifications = (await staffClient.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200")).GetProperty("items").EnumerateArray()
            .Where(x => Text(x, "templateCode") == "noticeReceived").ToList();
        notifications.ShouldContain(x => x.GetProperty("resourceId").GetGuid() == id && Text(x, "resourceType") == "notice");

        var inbox = await staffClient.GetFromJsonAsync<JsonElement>("/api/app/me/inbox-summary");
        inbox.GetProperty("items").EnumerateArray().Single(x => Text(x, "category") == "noticeAcknowledgement").GetProperty("count").GetInt32().ShouldBe(1);

        // Acknowledge is safe to repeat and keeps the first time.
        var acknowledged = await OkAsync(await staffClient.PostAsync($"/api/app/notice/{id}/acknowledge", null));
        var again = await OkAsync(await staffClient.PostAsync($"/api/app/notice/{id}/acknowledge", null));
        (again.GetProperty("acknowledgedAt").GetDateTime() - acknowledged.GetProperty("acknowledgedAt").GetDateTime()).Duration().ShouldBeLessThan(TimeSpan.FromMilliseconds(1));
        (await staffClient.GetFromJsonAsync<JsonElement>("/api/app/notice?isAcknowledged=false")).GetProperty("totalCount").GetInt64().ShouldBe(0);
        (await staffClient.GetFromJsonAsync<JsonElement>("/api/app/me/inbox-summary")).GetProperty("items").EnumerateArray()
            .Any(x => Text(x, "category") == "noticeAcknowledgement").ShouldBeFalse();

        // Receipts and the sent list are the sender's only.
        var receipts = await hr.GetFromJsonAsync<JsonElement>($"/api/app/notice/{id}/receipt");
        receipts.GetProperty("totalCount").GetInt64().ShouldBe(2);
        receipts.GetProperty("items")[0].GetProperty("recipient").GetProperty("id").GetGuid().ShouldBe(staff.Id);
        receipts.GetProperty("items")[0].GetProperty("acknowledgedAt").ValueKind.ShouldBe(JsonValueKind.String);
        (await hr.GetFromJsonAsync<JsonElement>($"/api/app/notice/{id}/receipt?isAcknowledged=false")).GetProperty("items").EnumerateArray()
            .Single().GetProperty("recipient").GetProperty("id").GetGuid().ShouldBe(manager.Id);
        (await executive.GetAsync($"/api/app/notice/{id}/receipt")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await hr.GetAsync($"/api/app/notice/{Guid.NewGuid()}/receipt")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var list = await hr.GetFromJsonAsync<JsonElement>("/api/app/notice/sent?maxResultCount=200");
        list.GetProperty("items").EnumerateArray().Single(x => IdOf(x) == id).GetProperty("acknowledgedCount").GetInt32().ShouldBe(1);

        // The executive director can send too.
        var fromExecutive = await OkAsync(await executive.PostAsJsonAsync("/api/app/notice", Body(other.Id)), HttpStatusCode.Created);
        (await otherClient.GetAsync($"/api/app/notice/{IdOf(fromExecutive)}")).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task One_unfit_recipient_refuses_the_whole_notice()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var staffClient = await kit.SignInAsync(staff);
        var withoutAccount = await kit.AddFileWithoutAccountAsync(manager.Id);
        var hr = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.HrManager);
        var before = await SentCountAsync(hr);

        // The system admin with no business role is not a company employee.
        await ShouldFailWithAsync(await hr.PostAsJsonAsync("/api/app/notice", Body(staff.Id, kit.SystemAdminEmployeeId)), HrErrorCodes.Notices.RecipientNotAllowed);

        // Decision 2026-11-11: an accountant or store officer with no other business role receives no notice.
        var accountant = await kit.AddAsync(manager.Id, new[] { HrRoles.Employee, HrRoles.Accountant });
        var storeOfficer = await kit.AddAsync(manager.Id, new[] { HrRoles.Employee, HrRoles.StoreOfficer });
        await ShouldFailWithAsync(await hr.PostAsJsonAsync("/api/app/notice", Body(staff.Id, accountant.Id)), HrErrorCodes.Notices.RecipientNotAllowed);
        await ShouldFailWithAsync(await hr.PostAsJsonAsync("/api/app/notice", Body(storeOfficer.Id)), HrErrorCodes.Notices.RecipientNotAllowed);
        await ShouldFailWithAsync(await hr.PostAsJsonAsync("/api/app/notice", Body(staff.Id, Guid.NewGuid())), HrErrorCodes.Notices.RecipientNotAllowed);
        await ShouldFailWithAsync(await hr.PostAsJsonAsync("/api/app/notice", Body(staff.Id, withoutAccount)), HrErrorCodes.Notices.RecipientNotAllowed);

        (await SentCountAsync(hr)).ShouldBe(before);
        (await staffClient.GetFromJsonAsync<JsonElement>("/api/app/notice")).GetProperty("totalCount").GetInt64().ShouldBe(0);

        // Shape errors are 400.
        (await hr.PostAsJsonAsync("/api/app/notice", Body())).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await hr.PostAsJsonAsync("/api/app/notice", Body(staff.Id, staff.Id))).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await hr.PostAsJsonAsync("/api/app/notice", new { recipientEmployeeIds = new[] { staff.Id }, title = "", content = "C" })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await hr.PostAsJsonAsync("/api/app/notice", new { recipientEmployeeIds = new[] { staff.Id }, title = "T", content = new string('x', 8001) })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
