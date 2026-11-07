using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using HR_Mangment2.Errors;
using HR_Mangment2.Infrastructure;
using Shouldly;
using Xunit;

namespace HR_Mangment2;

/// <summary>What leave and corrections do to attendance. All dates are in the past, and the company is on UTC+3.</summary>
[Collection(HostCollection.Name)]
public class LeaveAttendanceEffectTests
{
    private const string Sunday = "2026-10-04";
    private const string Monday = "2026-10-05";
    private const string Tuesday = "2026-10-06";

    private readonly HostFixture _host;

    public LeaveAttendanceEffectTests(HostFixture host)
    {
        _host = host;
    }

    private sealed record Team(EmployeeTestKit Kit, CreatedEmployee Manager, HttpClient ManagerClient, CreatedEmployee Staff, HttpClient StaffClient);

    private async Task<Team> TeamAsync()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        return new Team(kit, manager, await kit.SignInAsync(manager), staff, await kit.SignInAsync(staff));
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

    private static async Task SetEntitlementAsync(HttpClient hr, Guid employeeId, decimal days)
    {
        var balance = await hr.GetFromJsonAsync<JsonElement>($"/api/app/leave-balance/employee/{employeeId}");
        await OkAsync(await hr.PutAsJsonAsync(
            $"/api/app/leave-balance/employee/{employeeId}/entitlement",
            new { entitlementDays = days, concurrencyStamp = balance.GetProperty("concurrencyStamp").GetString() }));
    }

    private static async Task ApproveThroughAsync(Team team, Guid requestId)
    {
        await OkAsync(await team.ManagerClient.PostAsJsonAsync($"/api/app/request/{requestId}/approve", new { note = (string?)null }));
        await OkAsync(await team.Kit.Hr.PostAsJsonAsync($"/api/app/request/{requestId}/approve", new { note = (string?)null }));
    }

    private static async Task<JsonElement> DayAsync(HttpClient client, string date)
    {
        var page = await client.GetFromJsonAsync<JsonElement>($"/api/app/attendance/my?fromDate={date}&toDate={date}");
        page.GetProperty("items").GetArrayLength().ShouldBe(1, page.ToString());
        return page.GetProperty("items")[0];
    }

    private static Task<HttpResponseMessage> CorrectAsync(HttpClient client, string date, string type, string time, Guid? employeeId = null) =>
        client.PostAsJsonAsync("/api/app/attendance-correction", new { date, punchType = type, correctTime = time, reason = "Forgot to punch", employeeId });

    [Fact]
    public async Task Approved_leave_shows_on_the_attendance_days_and_pending_leave_does_not()
    {
        var team = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 10);

        var leave = await OkAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", new { kind = "annual", startDate = Sunday, endDate = Monday }), HttpStatusCode.Created);
        var id = leave.GetProperty("requestId").GetGuid();

        await AttendanceModuleTests.RunJobsAsync(_host);
        (await team.StaffClient.GetFromJsonAsync<JsonElement>($"/api/app/attendance/my?fromDate={Sunday}&toDate={Sunday}"))
            .GetProperty("items").EnumerateArray().ShouldNotContain(x => x.GetProperty("status").GetString() == "onLeave");

        await ApproveThroughAsync(team, id);
        await AttendanceModuleTests.RunJobsAsync(_host);

        var day = await DayAsync(team.StaffClient, Sunday);
        day.GetProperty("status").GetString().ShouldBe("onLeave");
        day.GetProperty("leaveRequestId").GetGuid().ShouldBe(id);
        day.GetProperty("lateMinutes").GetInt32().ShouldBe(0);

        var page = await team.StaffClient.GetFromJsonAsync<JsonElement>($"/api/app/attendance/my?fromDate={Sunday}&toDate={Monday}");
        page.GetProperty("summary").GetProperty("leaveDays").GetDecimal().ShouldBe(2);
    }

    [Fact]
    public async Task Approved_hourly_leave_is_recorded_on_the_day()
    {
        var team = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 10);

        var leave = await OkAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", new { kind = "hourly", date = Tuesday, fromTime = "08:00", toTime = "10:00" }), HttpStatusCode.Created);
        await ApproveThroughAsync(team, leave.GetProperty("requestId").GetGuid());
        await AttendanceModuleTests.RunJobsAsync(_host);

        var day = await DayAsync(team.StaffClient, Tuesday);
        day.GetProperty("hourlyLeaveMinutes").GetInt32().ShouldBe(120);
        day.GetProperty("status").GetString().ShouldNotBe("onLeave");

        var balance = await team.StaffClient.GetFromJsonAsync<JsonElement>("/api/app/leave-ledger/my?type=deduct");
        balance.GetProperty("items")[0].GetProperty("hours").GetDecimal().ShouldBe(-2);
    }

    [Fact]
    public async Task An_approved_correction_changes_the_day_and_keeps_the_history()
    {
        var team = await TeamAsync();

        var created = await OkAsync(await CorrectAsync(team.StaffClient, Monday, "checkIn", "08:05"), HttpStatusCode.Created);
        var id = created.GetProperty("requestId").GetGuid();
        created.GetProperty("status").GetString().ShouldBe("pendingManager");
        created.GetProperty("appliedValue").ValueKind.ShouldBe(JsonValueKind.Null);
        created.GetProperty("correctTime").GetString().ShouldBe("08:05");

        await ShouldFailWithAsync(await CorrectAsync(team.StaffClient, Monday, "checkIn", "08:10"), HrErrorCodes.Attendance.CorrectionAlreadyPending);

        await ApproveThroughAsync(team, id);

        var applied = await team.StaffClient.GetFromJsonAsync<JsonElement>($"/api/app/attendance-correction/{id}");
        applied.GetProperty("status").GetString().ShouldBe("approved");
        applied.GetProperty("appliedValue").GetDateTime().ToUniversalTime().ShouldBe(new DateTime(2026, 10, 5, 5, 5, 0, DateTimeKind.Utc));
        applied.GetProperty("affectsClosedMonth").GetBoolean().ShouldBeFalse();

        var day = await DayAsync(team.StaffClient, Monday);
        day.GetProperty("isCorrected").GetBoolean().ShouldBeTrue();
        day.GetProperty("checkIn").GetDateTime().ToUniversalTime().ShouldBe(new DateTime(2026, 10, 5, 5, 5, 0, DateTimeKind.Utc));

        var detail = await team.StaffClient.GetFromJsonAsync<JsonElement>($"/api/app/attendance/day/{day.GetProperty("id").GetGuid()}");
        var history = detail.GetProperty("corrections")[0];
        history.GetProperty("requestId").GetGuid().ShouldBe(id);
        history.GetProperty("oldValue").ValueKind.ShouldBe(JsonValueKind.Null);
        history.GetProperty("punchType").GetString().ShouldBe("checkIn");
        history.GetProperty("approvedBy").ValueKind.ShouldBe(JsonValueKind.Object);
        detail.GetProperty("rawPunches").GetArrayLength().ShouldBe(0);

        // A checkout is compared with the corrected checkin, and a second correction records what it replaced.
        await ShouldFailWithAsync(await CorrectAsync(team.StaffClient, Monday, "checkOut", "07:00"), HrErrorCodes.Attendance.CorrectionTimeInvalid);

        var second = await OkAsync(await CorrectAsync(team.StaffClient, Monday, "checkIn", "08:00"), HttpStatusCode.Created);
        await ApproveThroughAsync(team, second.GetProperty("requestId").GetGuid());
        var replaced = await team.StaffClient.GetFromJsonAsync<JsonElement>($"/api/app/attendance/day/{day.GetProperty("id").GetGuid()}");
        replaced.GetProperty("corrections").GetArrayLength().ShouldBe(2);
        replaced.GetProperty("corrections")[0].GetProperty("oldValue").GetDateTime().ToUniversalTime().ShouldBe(new DateTime(2026, 10, 5, 5, 5, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task A_rejected_correction_changes_nothing_and_frees_the_slot()
    {
        var team = await TeamAsync();

        var created = await OkAsync(await CorrectAsync(team.StaffClient, Tuesday, "checkOut", "16:00"), HttpStatusCode.Created);
        await OkAsync(await team.ManagerClient.PostAsJsonAsync($"/api/app/request/{created.GetProperty("requestId").GetGuid()}/reject", new { reason = "You were absent" }));

        var read = await team.StaffClient.GetFromJsonAsync<JsonElement>($"/api/app/attendance-correction/{created.GetProperty("requestId").GetGuid()}");
        read.GetProperty("status").GetString().ShouldBe("rejected");
        read.GetProperty("appliedValue").ValueKind.ShouldBe(JsonValueKind.Null);

        (await CorrectAsync(team.StaffClient, Tuesday, "checkOut", "16:00")).StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task On_a_night_shift_the_morning_belongs_to_the_day_the_shift_started()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var managerClient = await kit.SignInAsync(manager);

        var shift = await OkAsync(await kit.Hr.PostAsJsonAsync("/api/app/work-shift", new
        {
            nameAr = "ليلي",
            nameEn = "Night",
            type = "overnight",
            graceMinutes = 0,
            overtimePolicy = "afterShiftEnd",
            days = new[] { new { dayOfWeek = "sunday", startTime = "22:00", endTime = "06:00", crossesMidnight = true } }
        }), HttpStatusCode.Created);

        var staff = await kit.AddAsync(manager.Id, new[] { "employee" }, shiftId: shift.GetProperty("id").GetGuid());
        var staffClient = await kit.SignInAsync(staff);
        await SetEntitlementAsync(kit.Hr, staff.Id, 10);

        var correction = await OkAsync(await CorrectAsync(staffClient, Sunday, "checkOut", "06:00"), HttpStatusCode.Created);
        await ApproveThroughAsync(new Team(kit, manager, managerClient, staff, staffClient), correction.GetProperty("requestId").GetGuid());

        var day = await DayAsync(staffClient, Sunday);
        day.GetProperty("checkOut").GetDateTime().ToUniversalTime().ShouldBe(new DateTime(2026, 10, 5, 3, 0, 0, DateTimeKind.Utc));

        var inside = await OkAsync(await staffClient.PostAsJsonAsync("/api/app/leave-request/preview", new { kind = "hourly", date = "2026-09-06", fromTime = "23:00", toTime = "01:00" }));
        inside.GetProperty("requestedHours").GetDecimal().ShouldBe(2);
        inside.GetProperty("issues").GetArrayLength().ShouldBe(0, inside.ToString());

        var before = await OkAsync(await staffClient.PostAsJsonAsync("/api/app/leave-request/preview", new { kind = "hourly", date = "2026-09-06", fromTime = "21:00", toTime = "23:00" }));
        before.GetProperty("issues")[0].GetProperty("code").GetString().ShouldBe(HrErrorCodes.Leave.HourlyOutsideShift);

        var after = await OkAsync(await staffClient.PostAsJsonAsync("/api/app/leave-request/preview", new { kind = "hourly", date = "2026-09-06", fromTime = "05:00", toTime = "07:00" }));
        after.GetProperty("issues")[0].GetProperty("code").GetString().ShouldBe(HrErrorCodes.Leave.HourlyOutsideShift);
    }

    [Fact]
    public async Task Correction_rules_follow_the_contract()
    {
        var team = await TeamAsync();

        await ShouldFailWithAsync(await CorrectAsync(team.StaffClient, "2099-01-01", "checkIn", "08:00"), HrErrorCodes.Attendance.CorrectionDateInFuture);
        await ShouldFailWithAsync(await CorrectAsync(team.StaffClient, "2024-01-01", "checkIn", "08:00"), HrErrorCodes.Attendance.CorrectionTimeInvalid);
        await ShouldFailWithAsync(await CorrectAsync(team.ManagerClient, Monday, "checkIn", "08:00", team.Kit.ExecutiveEmployeeId), HrErrorCodes.Attendance.ExecutiveDirectorNotCovered);

        (await team.StaffClient.PostAsJsonAsync("/api/app/attendance-correction", new { date = Monday, punchType = "checkIn", correctTime = "8am", reason = "x" })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await team.StaffClient.PostAsJsonAsync("/api/app/attendance-correction", new { date = Monday, punchType = "checkIn", correctTime = "08:00", reason = "" })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var onBehalf = await OkAsync(await CorrectAsync(team.ManagerClient, Monday, "checkIn", "08:00", team.Staff.Id), HttpStatusCode.Created);
        onBehalf.GetProperty("isOnBehalf").GetBoolean().ShouldBeTrue();
        onBehalf.GetProperty("status").GetString().ShouldBe("pendingHr");

        var otherTeam = await TeamAsync();
        (await CorrectAsync(team.ManagerClient, Monday, "checkIn", "08:00", otherTeam.Staff.Id)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var id = onBehalf.GetProperty("requestId").GetGuid();
        (await otherTeam.StaffClient.GetAsync($"/api/app/attendance-correction/{id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await team.StaffClient.GetAsync($"/api/app/attendance-correction/{id}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await team.StaffClient.GetAsync($"/api/app/attendance-correction/{Guid.NewGuid()}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
