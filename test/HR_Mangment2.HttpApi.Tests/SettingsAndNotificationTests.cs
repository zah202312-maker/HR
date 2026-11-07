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

[Collection(HostCollection.Name)]
public class SettingsAndNotificationTests
{
    private readonly HostFixture _host;

    public SettingsAndNotificationTests(HostFixture host)
    {
        _host = host;
    }

    private Task<HttpClient> AdminAsync() => TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.SystemAdmin);

    [Fact]
    public async Task Sections_list_shows_owners_and_edit_rights()
    {
        var admin = await AdminAsync();
        var sections = await admin.GetFromJsonAsync<JsonElement>("/api/app/system-settings");
        var items = sections.GetProperty("items").EnumerateArray().ToList();

        items.Count.ShouldBe(15);
        var holidays = items.Single(x => x.GetProperty("code").GetString() == "holidays");
        holidays.GetProperty("ownerRole").GetString().ShouldBe("hrManager");
        holidays.GetProperty("isEditableByCaller").GetBoolean().ShouldBeFalse();
        items.Single(x => x.GetProperty("code").GetString() == "finance").GetProperty("isEditableByCaller").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public async Task Approved_initial_values_and_update_with_concurrency_and_validation()
    {
        var admin = await AdminAsync();

        var hourly = await admin.GetFromJsonAsync<JsonElement>("/api/app/system-settings/hourly-leave");
        hourly.GetProperty("hoursPerDay").GetDecimal().ShouldBe(8m);

        var reminders = await admin.GetFromJsonAsync<JsonElement>("/api/app/system-settings/reminders");
        reminders.GetProperty("monthEndOpenTime").GetString().ShouldBe("10:00");
        reminders.GetProperty("monthlyEscalationWorkingDay").GetInt32().ShouldBe(4);
        var stamp = reminders.GetProperty("concurrencyStamp").GetString();

        object Body(int submission, int escalation, string? concurrency) => new
        {
            monthEndOpenTime = "09:30",
            monthlySubmissionWorkingDays = submission,
            monthlyReminderIntervalWorkingDays = 1,
            monthlyEscalationWorkingDay = escalation,
            approvalReminderIntervalWorkingDays = 2,
            evaluationMinWorkedDays = 15,
            concurrencyStamp = concurrency
        };

        var invalid = await admin.PutAsJsonAsync("/api/app/system-settings/reminders", Body(5, 4, stamp));
        invalid.StatusCode.ShouldBe((HttpStatusCode)422);
        (await invalid.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("error").GetProperty("code").GetString()
            .ShouldBe(HrErrorCodes.Settings.InvalidEscalationDay);

        var ok = await admin.PutAsJsonAsync("/api/app/system-settings/reminders", Body(3, 4, stamp));
        ok.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ok.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("monthEndOpenTime").GetString().ShouldBe("09:30");

        (await admin.PutAsJsonAsync("/api/app/system-settings/reminders", Body(3, 4, stamp))).StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Unknown_fields_are_rejected_with_400()
    {
        var admin = await AdminAsync();
        var current = await admin.GetFromJsonAsync<JsonElement>("/api/app/system-settings/contracts");

        var response = await admin.PutAsJsonAsync("/api/app/system-settings/contracts", new
        {
            alertDays = 30,
            concurrencyStamp = current.GetProperty("concurrencyStamp").GetString(),
            complaintHandlerId = Guid.NewGuid()
        });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Employee_cannot_read_system_settings()
    {
        var (_, userName) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee);
        var client = TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, userName, TestUsers.Password)).AccessToken);

        (await client.GetAsync("/api/app/system-settings")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Password_change_creates_a_localized_notification_only_for_its_owner()
    {
        var (_, userName) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee);
        var token = (await TestAuth.SignInAsync(_host, userName, TestUsers.Password)).AccessToken;
        var english = TestAuth.ClientFor(_host, token, "en");
        var arabic = TestAuth.ClientFor(_host, token, "ar");

        (await english.PostAsJsonAsync("/api/app/account/change-password", new { currentPassword = TestUsers.Password, newPassword = "Another-Pass-3c!" }))
            .EnsureSuccessStatusCode();

        (await english.GetFromJsonAsync<JsonElement>("/api/app/notification/unread-count")).GetProperty("count").GetInt32().ShouldBe(1);

        var list = await english.GetFromJsonAsync<JsonElement>("/api/app/notification");
        list.GetProperty("totalCount").GetInt64().ShouldBe(1);
        var item = list.GetProperty("items")[0];
        item.GetProperty("templateCode").GetString().ShouldBe("accountSecurityChanged");
        item.GetProperty("resourceType").GetString().ShouldBe("none");
        item.GetProperty("title").GetString().ShouldBe("Account security change");

        var inArabic = await arabic.GetFromJsonAsync<JsonElement>("/api/app/notification");
        inArabic.GetProperty("items")[0].GetProperty("title").GetString().ShouldBe("تغيير أمني في حسابك");

        var id = item.GetProperty("id").GetString();
        var (_, otherName) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee);
        var other = TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, otherName, TestUsers.Password)).AccessToken);
        (await other.PostAsync($"/api/app/notification/{id}/read", null)).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var read = await english.PostAsync($"/api/app/notification/{id}/read", null);
        read.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await read.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("isRead").GetBoolean().ShouldBeTrue();
        (await english.GetFromJsonAsync<JsonElement>("/api/app/notification?isRead=false")).GetProperty("totalCount").GetInt64().ShouldBe(0);
    }

    [Fact]
    public async Task Device_token_moves_to_the_latest_user_and_unregister_is_idempotent()
    {
        var token = "fcm-" + Guid.NewGuid();
        var (_, first) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee);
        var (_, second) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee);
        var a = TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, first, TestUsers.Password)).AccessToken);
        var b = TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, second, TestUsers.Password)).AccessToken);

        (await a.PostAsJsonAsync("/api/app/device-token", new { token, platform = "android" })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await a.PostAsJsonAsync("/api/app/device-token", new { token, platform = "android" })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await b.PostAsJsonAsync("/api/app/device-token", new { token, platform = "ios" })).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        (await b.PostAsJsonAsync("/api/app/device-token/unregister", new { token })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await b.PostAsJsonAsync("/api/app/device-token/unregister", new { token })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Notification_hub_requires_a_token()
    {
        (await _host.CreateClient().PostAsync("/signalr-hubs/notification/negotiate?negotiateVersion=1", null))
            .StatusCode.ShouldNotBe(HttpStatusCode.OK);

        var (_, userName) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee);
        var token = (await TestAuth.SignInAsync(_host, userName, TestUsers.Password)).AccessToken;
        (await _host.CreateClient().PostAsync("/signalr-hubs/notification/negotiate?negotiateVersion=1&access_token=" + token, null))
            .StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
