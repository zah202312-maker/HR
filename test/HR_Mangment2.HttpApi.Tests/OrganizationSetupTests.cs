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
public class OrganizationSetupTests
{
    private static int _nextHolidayOffset;

    private readonly HostFixture _host;

    public OrganizationSetupTests(HostFixture host)
    {
        _host = host;
    }

    private Task<HttpClient> HrAsync(string language = "en") => TestUsers.SignedInWithMfaAsync(_host, language, HrRoles.Employee, HrRoles.HrManager);

    private static async Task<JsonElement> ReadAsync(HttpResponseMessage response) => await response.Content.ReadFromJsonAsync<JsonElement>();

    private static async Task<string?> ErrorCodeAsync(HttpResponseMessage response) =>
        (await ReadAsync(response)).GetProperty("error").GetProperty("code").GetString();

    private static object MorningShift(string? name = null) => new
    {
        nameAr = name ?? "صباحي",
        nameEn = name ?? "Morning",
        type = "fixed",
        graceMinutes = 10,
        overtimePolicy = "none",
        days = new[] { new { dayOfWeek = "sunday", startTime = "08:00", endTime = "16:00", crossesMidnight = false } }
    };

    /// <summary>A date no other test run has used in this database.</summary>
    private static string UniqueDate() =>
        DateOnly.FromDateTime(DateTime.UtcNow).AddDays(Random.Shared.Next(1000, 2_500_000) +System.Threading.Interlocked.Increment(ref _nextHolidayOffset)).ToString("yyyy-MM-dd");

    [Fact]
    public async Task Department_crud_with_localized_name_concurrency_and_soft_delete()
    {
        var hr = await HrAsync();

        var created = await hr.PostAsJsonAsync("/api/app/department", new { nameAr = "المبيعات", nameEn = "Sales" });
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        var department = await ReadAsync(created);
        var id = department.GetProperty("id").GetGuid();
        department.GetProperty("name").GetString().ShouldBe("Sales");
        department.GetProperty("employeeCount").GetInt32().ShouldBe(0);
        var stamp = department.GetProperty("concurrencyStamp").GetString();

        var arabic = await HrAsync("ar");
        (await arabic.GetFromJsonAsync<JsonElement>($"/api/app/department/{id}")).GetProperty("name").GetString().ShouldBe("المبيعات");

        var updated = await hr.PutAsJsonAsync($"/api/app/department/{id}", new { nameAr = "المبيعات", nameEn = "Sales 2", concurrencyStamp = stamp });
        updated.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ReadAsync(updated)).GetProperty("lastModificationTime").ValueKind.ShouldBe(JsonValueKind.String);

        (await hr.PutAsJsonAsync($"/api/app/department/{id}", new { nameAr = "س", nameEn = "S", concurrencyStamp = stamp }))
            .StatusCode.ShouldBe(HttpStatusCode.Conflict);

        (await hr.DeleteAsync($"/api/app/department/{id}")).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await hr.GetAsync($"/api/app/department/{id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Department_search_sorting_and_lookup_shape()
    {
        var hr = await HrAsync();
        var marker = "Dept" + Guid.NewGuid().ToString("N")[..8];
        (await hr.PostAsJsonAsync("/api/app/department", new { nameAr = marker + "ب", nameEn = marker + "B" })).EnsureSuccessStatusCode();
        (await hr.PostAsJsonAsync("/api/app/department", new { nameAr = marker + "أ", nameEn = marker + "A" })).EnsureSuccessStatusCode();

        var list = await hr.GetFromJsonAsync<JsonElement>($"/api/app/department?search={marker.ToLowerInvariant()}&sorting=nameEn%20desc");
        list.GetProperty("totalCount").GetInt64().ShouldBe(2);
        list.GetProperty("items")[0].GetProperty("nameEn").GetString().ShouldBe(marker + "B");

        (await hr.GetAsync("/api/app/department?sorting=concurrencyStamp")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await hr.GetAsync("/api/app/department?sorting=nameEn%3Bdrop")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Every role can look up, and the lookup holds the id and the name only.
        var (_, userName) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee);
        var employee = TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, userName, TestUsers.Password)).AccessToken, "en");
        var lookup = await employee.GetFromJsonAsync<JsonElement>($"/api/app/department/lookup?search={marker}");
        lookup.GetProperty("items")[0].EnumerateObject().Select(x => x.Name).OrderBy(x => x).ShouldBe(new[] { "id", "name" });

        (await employee.GetAsync("/api/app/department")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await employee.PostAsJsonAsync("/api/app/department", new { nameAr = "س", nameEn = "S" })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Department_input_limits_return_400_with_json_member_paths()
    {
        var hr = await HrAsync();

        var response = await hr.PostAsJsonAsync("/api/app/department", new { nameAr = "", nameEn = new string('x', 129) });
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var members = (await ReadAsync(response)).GetProperty("error").GetProperty("validationErrors").EnumerateArray()
            .SelectMany(x => x.GetProperty("members").EnumerateArray()).Select(x => x.GetString()).ToList();
        members.ShouldContain("nameAr");
        members.ShouldContain("nameEn");
    }

    [Fact]
    public async Task Unknown_id_is_404_and_a_non_guid_does_not_match_the_route()
    {
        var hr = await HrAsync();
        (await hr.GetAsync($"/api/app/department/{Guid.NewGuid()}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await hr.GetAsync("/api/app/department/not-a-guid")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("fixed", null, "sunday", null, "08:00", "16:00", false, null)]
    [InlineData("overnight", null, "monday", null, "22:00", "06:00", true, null)]
    [InlineData("rotating", 4, null, 2, "08:00", "20:00", false, null)]
    [InlineData("fixed", null, "sunday", null, "22:00", "06:00", false, HrErrorCodes.Shift.CrossesMidnightMismatch)]
    [InlineData("fixed", null, "sunday", null, "08:00", "16:00", true, HrErrorCodes.Shift.CrossesMidnightMismatch)]
    [InlineData("overnight", null, "monday", null, "08:00", "16:00", false, HrErrorCodes.Shift.InvalidSchedule)]
    [InlineData("rotating", null, null, 1, "08:00", "16:00", false, HrErrorCodes.Shift.InvalidSchedule)]
    [InlineData("rotating", 4, null, 5, "08:00", "16:00", false, HrErrorCodes.Shift.InvalidSchedule)]
    [InlineData("rotating", 4, "sunday", 1, "08:00", "16:00", false, HrErrorCodes.Shift.InvalidSchedule)]
    [InlineData("fixed", 4, "sunday", null, "08:00", "16:00", false, HrErrorCodes.Shift.InvalidSchedule)]
    [InlineData("fixed", null, null, null, "08:00", "16:00", false, HrErrorCodes.Shift.InvalidSchedule)]
    public async Task WorkShift_schedule_rules(string type, int? cycle, string? dayOfWeek, int? cycleDay, string start, string end, bool crosses, string? expectedCode)
    {
        var hr = await HrAsync();
        var response = await hr.PostAsJsonAsync("/api/app/work-shift", new
        {
            nameAr = "دوام",
            nameEn = "Shift",
            type,
            graceMinutes = 5,
            overtimePolicy = "none",
            rotationCycleDays = cycle,
            days = new[] { new { dayOfWeek, cycleDayNumber = cycleDay, startTime = start, endTime = end, crossesMidnight = crosses } }
        });

        if (expectedCode == null)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.Created);
            return;
        }

        response.StatusCode.ShouldBe((HttpStatusCode)422);
        (await ErrorCodeAsync(response)).ShouldBe(expectedCode);
    }

    [Fact]
    public async Task WorkShift_rejects_duplicate_days_and_a_wrong_overtime_combination()
    {
        var hr = await HrAsync();
        var day = new { dayOfWeek = "sunday", startTime = "08:00", endTime = "16:00", crossesMidnight = false };

        var duplicate = await hr.PostAsJsonAsync("/api/app/work-shift", new
        {
            nameAr = "د", nameEn = "S", type = "fixed", graceMinutes = 0, overtimePolicy = "none", days = new[] { day, day }
        });
        (await ErrorCodeAsync(duplicate)).ShouldBe(HrErrorCodes.Shift.InvalidSchedule);

        var missingHours = await hr.PostAsJsonAsync("/api/app/work-shift", new
        {
            nameAr = "د", nameEn = "S", type = "fixed", graceMinutes = 0, overtimePolicy = "afterDailyHours", days = new[] { day }
        });
        (await ErrorCodeAsync(missingHours)).ShouldBe(HrErrorCodes.Shift.InvalidSchedule);

        var extraHours = await hr.PostAsJsonAsync("/api/app/work-shift", new
        {
            nameAr = "د", nameEn = "S", type = "fixed", graceMinutes = 0, overtimePolicy = "none", overtimeAfterHours = 8, days = new[] { day }
        });
        (await ErrorCodeAsync(extraHours)).ShouldBe(HrErrorCodes.Shift.InvalidSchedule);

        var tooMuchGrace = await hr.PostAsJsonAsync("/api/app/work-shift", new
        {
            nameAr = "د", nameEn = "S", type = "fixed", graceMinutes = 241, overtimePolicy = "none", days = new[] { day }
        });
        tooMuchGrace.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var noDays = await hr.PostAsJsonAsync("/api/app/work-shift", new
        {
            nameAr = "د", nameEn = "S", type = "fixed", graceMinutes = 0, overtimePolicy = "none", days = Array.Empty<object>()
        });
        noDays.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task WorkShift_update_replaces_days_and_a_days_only_change_still_rolls_the_stamp()
    {
        var hr = await HrAsync();
        var shift = await ReadAsync(await hr.PostAsJsonAsync("/api/app/work-shift", MorningShift()));
        var id = shift.GetProperty("id").GetGuid();
        var stamp = shift.GetProperty("concurrencyStamp").GetString();
        shift.GetProperty("isActive").GetBoolean().ShouldBeTrue();
        shift.GetProperty("assignedEmployeeCount").GetInt32().ShouldBe(0);

        object Update(string? concurrency) => new
        {
            nameAr = "صباحي",
            nameEn = "Morning",
            type = "fixed",
            graceMinutes = 10,
            overtimePolicy = "none",
            isActive = false,
            days = new[]
            {
                new { dayOfWeek = "tuesday", startTime = "09:00", endTime = "17:00", crossesMidnight = false },
                new { dayOfWeek = "monday", startTime = "09:00", endTime = "17:00", crossesMidnight = false }
            },
            concurrencyStamp = concurrency
        };

        var updated = await hr.PutAsJsonAsync($"/api/app/work-shift/{id}", Update(stamp));
        updated.StatusCode.ShouldBe(HttpStatusCode.OK);

        var read = await hr.GetFromJsonAsync<JsonElement>($"/api/app/work-shift/{id}");
        read.GetProperty("isActive").GetBoolean().ShouldBeFalse();
        read.GetProperty("days").GetArrayLength().ShouldBe(2);
        read.GetProperty("days")[0].GetProperty("dayOfWeek").GetString().ShouldBe("monday");
        read.GetProperty("days")[0].GetProperty("startTime").GetString().ShouldBe("09:00");
        read.GetProperty("concurrencyStamp").GetString().ShouldNotBe(stamp);

        (await hr.PutAsJsonAsync($"/api/app/work-shift/{id}", Update(stamp))).StatusCode.ShouldBe(HttpStatusCode.Conflict);

        // Only the days change now, and the old stamp must still be refused after it.
        var current = read.GetProperty("concurrencyStamp").GetString();
        var daysOnly = new
        {
            nameAr = "صباحي", nameEn = "Morning", type = "fixed", graceMinutes = 10, overtimePolicy = "none", isActive = false,
            days = new[] { new { dayOfWeek = "friday", startTime = "09:00", endTime = "17:00", crossesMidnight = false } },
            concurrencyStamp = current
        };
        (await hr.PutAsJsonAsync($"/api/app/work-shift/{id}", daysOnly)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await hr.PutAsJsonAsync($"/api/app/work-shift/{id}", daysOnly)).StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var filtered = await hr.GetFromJsonAsync<JsonElement>("/api/app/work-shift?isActive=false&type=fixed&maxResultCount=200");
        filtered.GetProperty("items").EnumerateArray().ShouldContain(x => x.GetProperty("id").GetGuid() == id);
    }

    [Fact]
    public async Task WorkShift_is_open_to_the_system_admin_for_manage_and_closed_to_employees()
    {
        var admin = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.SystemAdmin);
        (await admin.PostAsJsonAsync("/api/app/work-shift", MorningShift())).StatusCode.ShouldBe(HttpStatusCode.Created);

        var (_, userName) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee);
        var employee = TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, userName, TestUsers.Password)).AccessToken, "en");
        (await employee.GetAsync("/api/app/work-shift")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Idempotency_key_replays_and_rejects_other_content()
    {
        var hr = await HrAsync();
        var key = Guid.NewGuid().ToString();

        async Task<HttpResponseMessage> PostAsync(object body, string? idempotencyKey)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/app/work-shift") { Content = JsonContent.Create(body) };
            if (idempotencyKey != null)
            {
                request.Headers.Add("Idempotency-Key", idempotencyKey);
            }

            return await hr.SendAsync(request);
        }

        var first = await PostAsync(MorningShift(), key);
        first.StatusCode.ShouldBe(HttpStatusCode.Created);
        var firstId = (await ReadAsync(first)).GetProperty("id").GetGuid();
        var second = await PostAsync(MorningShift(), key);
        second.StatusCode.ShouldBe(HttpStatusCode.Created);
        (await ReadAsync(second)).GetProperty("id").GetGuid().ShouldBe(firstId);

        var reused = await PostAsync(MorningShift("Other"), key);
        reused.StatusCode.ShouldBe((HttpStatusCode)422);
        (await ErrorCodeAsync(reused)).ShouldBe(HrErrorCodes.Common.IdempotencyKeyReused);

        (await PostAsync(MorningShift(), "not-a-uuid")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // The key belongs to one user: another user with the same key gets a new resource.
        var otherHr = await HrAsync();
        using var other = new HttpRequestMessage(HttpMethod.Post, "/api/app/work-shift") { Content = JsonContent.Create(MorningShift()) };
        other.Headers.Add("Idempotency-Key", key);
        var otherResponse = await otherHr.SendAsync(other);
        otherResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        (await ReadAsync(otherResponse)).GetProperty("id").GetGuid().ShouldNotBe(firstId);
    }

    [Fact]
    public async Task Holiday_rejects_a_repeated_date_and_replays_its_key()
    {
        var hr = await HrAsync();
        var date = UniqueDate();
        var key = Guid.NewGuid().ToString();
        var body = new { date, nameAr = "عطلة", nameEn = "Holiday" };

        using var first = new HttpRequestMessage(HttpMethod.Post, "/api/app/holiday") { Content = JsonContent.Create(body) };
        first.Headers.Add("Idempotency-Key", key);
        var created = await hr.SendAsync(first);
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        var holiday = await ReadAsync(created);
        holiday.GetProperty("date").GetString().ShouldBe(date);

        using var replay = new HttpRequestMessage(HttpMethod.Post, "/api/app/holiday") { Content = JsonContent.Create(body) };
        replay.Headers.Add("Idempotency-Key", key);
        var replayed = await hr.SendAsync(replay);
        replayed.StatusCode.ShouldBe(HttpStatusCode.Created);
        (await ReadAsync(replayed)).GetProperty("id").GetGuid().ShouldBe(holiday.GetProperty("id").GetGuid());

        var duplicate = await hr.PostAsJsonAsync("/api/app/holiday", body);
        duplicate.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await ReadAsync(duplicate)).GetProperty("error").GetProperty("message").GetString().ShouldBe("A holiday already exists on this date.");
    }

    [Fact]
    public async Task Holiday_update_list_filter_and_delete()
    {
        var hr = await HrAsync();
        var date = UniqueDate();
        var otherDate = UniqueDate();
        var movedDate = UniqueDate();

        var holiday = await ReadAsync(await hr.PostAsJsonAsync("/api/app/holiday", new { date, nameAr = "أ", nameEn = "A" }));
        (await hr.PostAsJsonAsync("/api/app/holiday", new { date = otherDate, nameAr = "ب", nameEn = "B" })).EnsureSuccessStatusCode();
        var id = holiday.GetProperty("id").GetGuid();
        var stamp = holiday.GetProperty("concurrencyStamp").GetString();

        var clash = await hr.PutAsJsonAsync($"/api/app/holiday/{id}", new { date = otherDate, nameAr = "أ", nameEn = "A", concurrencyStamp = stamp });
        clash.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var moved = await hr.PutAsJsonAsync($"/api/app/holiday/{id}", new { date = movedDate, nameAr = "أ", nameEn = "A2", concurrencyStamp = stamp });
        moved.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ReadAsync(moved)).GetProperty("name").GetString().ShouldBe("A2");

        (await hr.PutAsJsonAsync($"/api/app/holiday/{id}", new { date = movedDate, nameAr = "أ", nameEn = "A3", concurrencyStamp = stamp }))
            .StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var list = await hr.GetFromJsonAsync<JsonElement>($"/api/app/holiday?fromDate={movedDate}&toDate={movedDate}");
        list.GetProperty("totalCount").GetInt64().ShouldBe(1);
        list.GetProperty("items")[0].GetProperty("id").GetGuid().ShouldBe(id);

        var (_, userName) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee);
        var employee = TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, userName, TestUsers.Password)).AccessToken, "en");
        (await employee.GetAsync("/api/app/holiday")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await employee.DeleteAsync($"/api/app/holiday/{id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        (await hr.DeleteAsync($"/api/app/holiday/{id}")).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await hr.DeleteAsync($"/api/app/holiday/{id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // The date is free again after the hard delete.
        (await hr.PostAsJsonAsync("/api/app/holiday", new { date = movedDate, nameAr = "ج", nameEn = "C" })).StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task WeeklyOff_reads_updates_and_validates()
    {
        var hr = await HrAsync();
        var current = await hr.GetFromJsonAsync<JsonElement>("/api/app/weekly-off-setting");
        var stamp = current.GetProperty("concurrencyStamp").GetString();

        (await hr.PutAsJsonAsync("/api/app/weekly-off-setting", new { days = new[] { "friday", "friday" }, concurrencyStamp = stamp }))
            .StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var updated = await hr.PutAsJsonAsync("/api/app/weekly-off-setting", new { days = new[] { "friday", "saturday" }, concurrencyStamp = stamp });
        updated.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ReadAsync(updated)).GetProperty("days").EnumerateArray().Select(x => x.GetString()).ShouldBe(new[] { "friday", "saturday" });

        (await hr.PutAsJsonAsync("/api/app/weekly-off-setting", new { days = new[] { "friday" }, concurrencyStamp = stamp }))
            .StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var (_, userName) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee);
        var employee = TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, userName, TestUsers.Password)).AccessToken, "en");
        (await employee.GetAsync("/api/app/weekly-off-setting")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await employee.PutAsJsonAsync("/api/app/weekly-off-setting", new { days = new[] { "friday" }, concurrencyStamp = stamp }))
            .StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
