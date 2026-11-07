using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ClosedXML.Excel;
using HR_Mangment2.Errors;
using HR_Mangment2.Identity;
using HR_Mangment2.Infrastructure;
using HR_Mangment2.Reports;
using Npgsql;
using Shouldly;
using Xunit;

namespace HR_Mangment2;

/// <summary>
/// Reports read a database the other tests share, so company-wide rows are found by employee and the review counts
/// are compared before and after. 2026-10-04 is a Sunday, and the company is on UTC+3.
/// </summary>
[Collection(HostCollection.Name)]
public class ReportModuleTests
{
    private const string Sunday = "2026-10-04";
    private const string Monday = "2026-10-05";
    private const string Period = "from=2026-10-01&to=2026-10-31";

    private readonly HostFixture _host;

    public ReportModuleTests(HostFixture host)
    {
        _host = host;
    }

    private sealed record Team(EmployeeTestKit Kit, CreatedEmployee Manager, HttpClient ManagerClient, CreatedEmployee Staff, HttpClient StaffClient, CreatedEmployee Quiet);

    private async Task<Team> TeamAsync()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var quiet = await kit.AddStaffAsync(manager.Id);
        return new Team(kit, manager, await kit.SignInAsync(manager), staff, await kit.SignInAsync(staff), quiet);
    }

    private async Task<(HttpClient Client, Guid UserId, Guid EmployeeId)> OwnerAsync(EmployeeTestKit kit, Guid? managerEmployeeId, params string[] roles)
    {
        var (userId, userName) = await TestUsers.CreateAsync(_host, false, roles);
        var employeeId = await kit.AddFileForUserAsync(userId, managerEmployeeId);
        var client = TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, userName, TestUsers.Password)).AccessToken, "en");

        if (roles.Any(HrRoles.MfaRequired.Contains))
        {
            var enrollment = await OkAsync(await client.PostAsync("/api/app/account/mfa/enroll", null));
            (await client.PostAsJsonAsync("/api/app/account/mfa/confirm", new { code = Totp.Compute(enrollment.GetProperty("sharedKey").GetString()!) })).EnsureSuccessStatusCode();
        }

        return (client, userId, employeeId);
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

    private static List<JsonElement> Items(JsonElement page) => page.GetProperty("items").EnumerateArray().ToList();

    private static Guid EmployeeOf(JsonElement row) => row.GetProperty("employee").GetProperty("id").GetGuid();

    private static string[] Fields(JsonElement row) => row.EnumerateObject().Select(x => x.Name).OrderBy(x => x, StringComparer.Ordinal).ToArray();

    private async Task PunchAsync(Guid employeeId, params string[] times)
    {
        var admin = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.SystemAdmin);
        var created = await OkAsync(
            await admin.PostAsJsonAsync("/api/app/attendance-device", new { serialNumber = "SN-" + Guid.NewGuid().ToString("N"), name = "Gate" }),
            HttpStatusCode.Created);
        var deviceId = created.GetProperty("device").GetProperty("id").GetGuid();
        var userNumber = Guid.NewGuid().ToString("N");

        (await admin.PostAsJsonAsync($"/api/app/attendance-device/{deviceId}/user-map", new { deviceUserId = userNumber, employeeId })).EnsureSuccessStatusCode();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/attendance-devices/punches")
        {
            Content = JsonContent.Create(new { punches = times.Select(x => new { deviceUserId = userNumber, punchTime = x }) })
        };
        request.Headers.Add("X-Device-Key", created.GetProperty("deviceKey").GetString());
        (await _host.CreateClient("en").SendAsync(request)).EnsureSuccessStatusCode();

        await AttendanceModuleTests.RunJobsAsync(_host);
    }

    private static async Task SetEntitlementAsync(HttpClient hr, Guid employeeId, decimal days)
    {
        var balance = await hr.GetFromJsonAsync<JsonElement>($"/api/app/leave-balance/employee/{employeeId}");
        (await hr.PutAsJsonAsync(
            $"/api/app/leave-balance/employee/{employeeId}/entitlement",
            new { entitlementDays = days, concurrencyStamp = balance.GetProperty("concurrencyStamp").GetString() })).EnsureSuccessStatusCode();
    }

    private static async Task<Guid> SubmitLeaveAsync(HttpClient client, string start, string end)
    {
        var leave = await OkAsync(await client.PostAsJsonAsync("/api/app/leave-request", new { kind = "annual", startDate = start, endDate = end }), HttpStatusCode.Created);
        return leave.GetProperty("requestId").GetGuid();
    }

    private static Task<HttpResponseMessage> ApproveAsync(HttpClient client, Guid id) =>
        client.PostAsJsonAsync($"/api/app/request/{id}/approve", new { note = (string?)null });

    private static async Task<JsonElement> DashboardAsync(HttpClient reviewer) =>
        (await OkAsync(await reviewer.GetAsync("/api/app/report/review-dashboard"))).GetProperty("complaintCounts");

    private static int Count(JsonElement counts, string name) => counts.GetProperty(name).GetInt32();

    private static async Task<Guid> SubmitComplaintAsync(HttpClient client, Guid? respondent, string level) =>
        (await OkAsync(
            await client.PostAsJsonAsync("/api/app/complaint", new
            {
                targetKind = respondent == null ? "general" : "employee",
                respondentId = respondent,
                level,
                subject = "Report test subject",
                content = "Report test content"
            }),
            HttpStatusCode.Created)).GetProperty("id").GetGuid();

    private async Task MakeLateAsync(Guid complaintId)
    {
        await using var connection = new NpgsqlConnection(_host.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("UPDATE complaints.\"Complaints\" SET \"DueAt\" = now() - interval '1 day' WHERE \"Id\" = @id", connection);
        command.Parameters.AddWithValue("id", complaintId);
        (await command.ExecuteNonQueryAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task Team_attendance_shows_each_day_of_direct_reports_only()
    {
        var team = await TeamAsync();
        var otherManager = await team.Kit.AddDepartmentManagerAsync();
        var outsider = await team.Kit.AddStaffAsync(otherManager.Id);

        await PunchAsync(team.Staff.Id, Sunday + "T05:30:00Z", Sunday + "T13:00:00Z");
        await PunchAsync(outsider.Id, Sunday + "T05:00:00Z", Sunday + "T13:00:00Z");

        // A punch also recalculates the days around it, and the next past working day with no punch is an absence.
        var page = await OkAsync(await team.ManagerClient.GetAsync($"/api/app/report/team-attendance?from={Sunday}&to={Sunday}"));

        var row = Items(page).Single();
        EmployeeOf(row).ShouldBe(team.Staff.Id);
        row.GetProperty("date").GetString().ShouldBe(Sunday);
        row.GetProperty("lateMinutes").GetInt32().ShouldBe(30);
        row.GetProperty("statusName").GetString().ShouldBe("Late");
        row.GetProperty("checkIn").GetDateTime().ToUniversalTime().ShouldBe(new DateTime(2026, 10, 4, 5, 30, 0, DateTimeKind.Utc));
        Fields(row).ShouldBe(new[] { "checkIn", "checkOut", "date", "employee", "lateMinutes", "statusName" });

        // Someone outside the team finds nothing, and does not reveal the person exists.
        var filtered = await OkAsync(await team.ManagerClient.GetAsync($"/api/app/report/team-attendance?{Period}&employeeId={outsider.Id}"));
        filtered.GetProperty("totalCount").GetInt64().ShouldBe(0);
    }

    [Fact]
    public async Task Team_lateness_gives_a_row_to_every_team_member_even_with_no_days()
    {
        var team = await TeamAsync();
        await PunchAsync(team.Staff.Id, Sunday + "T05:30:00Z", Sunday + "T13:00:00Z", Monday + "T05:15:00Z", Monday + "T13:00:00Z");

        var page = await OkAsync(await team.ManagerClient.GetAsync($"/api/app/report/team-lateness-absence?from={Sunday}&to={Monday}&sorting=lateMinutes desc"));
        var rows = Items(page);

        rows.Count.ShouldBe(2);
        EmployeeOf(rows[0]).ShouldBe(team.Staff.Id);
        rows[0].GetProperty("lateCount").GetInt32().ShouldBe(2);
        rows[0].GetProperty("lateMinutes").GetInt32().ShouldBe(45);
        rows[0].GetProperty("absentDays").GetInt32().ShouldBe(0);
        EmployeeOf(rows[1]).ShouldBe(team.Quiet.Id);
        rows[1].GetProperty("lateCount").GetInt32().ShouldBe(0);

        (await team.ManagerClient.GetAsync($"/api/app/report/team-lateness-absence?{Period}&sorting=address")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Team_leaves_lists_open_and_approved_leave_and_leaves_out_refused_leave()
    {
        var team = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 30);

        var approved = await SubmitLeaveAsync(team.StaffClient, "2026-10-11", "2026-10-12");
        (await ApproveAsync(team.ManagerClient, approved)).EnsureSuccessStatusCode();

        var refused = await SubmitLeaveAsync(team.StaffClient, "2026-10-18", "2026-10-18");
        (await team.ManagerClient.PostAsJsonAsync($"/api/app/request/{refused}/reject", new { reason = "Busy month" })).EnsureSuccessStatusCode();

        await SubmitLeaveAsync(team.StaffClient, "2026-10-25", "2026-10-25");

        var rows = Items(await OkAsync(await team.ManagerClient.GetAsync($"/api/app/report/team-leaves?{Period}&sorting=from")));

        rows.Count.ShouldBe(2);
        rows[0].GetProperty("from").GetDateTime().ToUniversalTime().ShouldBe(new DateTime(2026, 10, 11, 0, 0, 0, DateTimeKind.Utc));
        rows[0].GetProperty("to").GetDateTime().ToUniversalTime().ShouldBe(new DateTime(2026, 10, 12, 0, 0, 0, DateTimeKind.Utc));
        rows[0].GetProperty("days").GetDecimal().ShouldBe(2);
        rows[0].GetProperty("hours").ValueKind.ShouldBe(JsonValueKind.Null);
        rows[0].GetProperty("leaveTypeName").GetString().ShouldBe("Annual");
        rows[0].GetProperty("statusName").GetString().ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Team_pending_drops_a_request_once_it_leaves_the_route()
    {
        var team = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 30);
        var leave = await SubmitLeaveAsync(team.StaffClient, "2026-11-08", "2026-11-08");

        var rows = Items(await OkAsync(await team.ManagerClient.GetAsync("/api/app/report/team-pending")));
        var row = rows.Single();
        row.GetProperty("requestId").GetGuid().ShouldBe(leave);
        Fields(row).ShouldBe(new[] { "creationTime", "employee", "requestId", "requestTypeName", "statusName" });

        (await team.StaffClient.PostAsJsonAsync($"/api/app/request/{leave}/cancel", new { note = (string?)null })).EnsureSuccessStatusCode();

        Items(await OkAsync(await team.ManagerClient.GetAsync("/api/app/report/team-pending"))).ShouldBeEmpty();
    }

    [Fact]
    public async Task Custody_and_evaluation_reports_are_empty_for_a_team_with_neither()
    {
        var team = await TeamAsync();
        var (reviewer, _, _) = await OwnerAsync(team.Kit, team.Kit.ExecutiveEmployeeId, HrRoles.Employee, HrRoles.Reviewer);

        foreach (var path in new[] { "team-custody?kind=personal", "team-evaluations?year=2026" })
        {
            var page = await OkAsync(await team.ManagerClient.GetAsync("/api/app/report/" + path));
            page.GetProperty("totalCount").GetInt64().ShouldBe(0);
            Items(page).ShouldBeEmpty();
        }

        // Its rows come from whichever cycles other tests opened in this shared database, so only the route is checked here.
        await OkAsync(await reviewer.GetAsync("/api/app/report/evaluation-consistency"));
    }

    [Fact]
    public async Task A_bad_period_is_refused_and_a_missing_one_is_invalid_input()
    {
        var team = await TeamAsync();

        foreach (var path in new[] { "team-attendance", "team-lateness-absence", "team-leaves" })
        {
            await ShouldFailWithAsync(await team.ManagerClient.GetAsync($"/api/app/report/{path}?from=2026-10-31&to=2026-10-01"), HrErrorCodes.Reports.InvalidPeriod);
            (await team.ManagerClient.GetAsync($"/api/app/report/{path}?from=2026-10-01")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        foreach (var path in new[] { "hr-attendance", "hr-requests" })
        {
            await ShouldFailWithAsync(await team.Kit.Hr.GetAsync($"/api/app/report/{path}?from=2026-10-31&to=2026-10-01"), HrErrorCodes.Reports.InvalidPeriod);
            (await team.Kit.Hr.GetAsync($"/api/app/report/{path}?to=2026-10-01")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task Each_report_group_is_closed_to_the_other_roles()
    {
        var team = await TeamAsync();
        var (reviewer, _, _) = await OwnerAsync(team.Kit, team.Kit.ExecutiveEmployeeId, HrRoles.Employee, HrRoles.Reviewer);

        (await team.Kit.Hr.GetAsync($"/api/app/report/team-attendance?{Period}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await reviewer.GetAsync($"/api/app/report/team-attendance?{Period}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await team.ManagerClient.GetAsync($"/api/app/report/hr-attendance?{Period}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await team.ManagerClient.GetAsync("/api/app/report/review-dashboard")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await team.Kit.Hr.GetAsync("/api/app/report/review-dashboard")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await team.StaffClient.GetAsync("/api/app/report/team-pending")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await team.StaffClient.GetAsync("/api/app/report/teamPending/export?format=pdf")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Hr_attendance_adds_up_the_days_and_filters_by_department()
    {
        var team = await TeamAsync();
        await PunchAsync(team.Staff.Id, Sunday + "T05:30:00Z", Sunday + "T13:00:00Z", Monday + "T05:00:00Z", Monday + "T13:00:00Z");

        var row = Items(await OkAsync(await team.Kit.Hr.GetAsync($"/api/app/report/hr-attendance?from={Sunday}&to={Monday}&employeeId={team.Staff.Id}"))).Single();

        row.GetProperty("scheduledWorkingDays").GetInt32().ShouldBe(2);
        row.GetProperty("presentDays").GetInt32().ShouldBe(2);
        row.GetProperty("absentDays").GetInt32().ShouldBe(0);
        row.GetProperty("lateCount").GetInt32().ShouldBe(1);
        row.GetProperty("lateMinutes").GetInt32().ShouldBe(30);
        row.GetProperty("leaveDays").GetDecimal().ShouldBe(0);

        var inDepartment = await OkAsync(await team.Kit.Hr.GetAsync($"/api/app/report/hr-attendance?{Period}&maxResultCount=200&departmentId={team.Kit.DepartmentId}"));
        Items(inDepartment).ShouldContain(x => EmployeeOf(x) == team.Staff.Id);

        var elsewhere = await OkAsync(await team.Kit.Hr.GetAsync($"/api/app/report/hr-attendance?{Period}&employeeId={team.Staff.Id}&departmentId={Guid.NewGuid()}"));
        elsewhere.GetProperty("totalCount").GetInt64().ShouldBe(0);

        // The executive director is outside attendance and gets no row of zeros.
        var executive = await OkAsync(await team.Kit.Hr.GetAsync($"/api/app/report/hr-attendance?{Period}&employeeId={team.Kit.ExecutiveEmployeeId}"));
        executive.GetProperty("totalCount").GetInt64().ShouldBe(0);
    }

    [Fact]
    public async Task Hr_leave_balances_agree_with_the_employees_own_balance()
    {
        var team = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 30);

        var approved = await SubmitLeaveAsync(team.StaffClient, "2026-09-13", "2026-09-14");
        (await ApproveAsync(team.ManagerClient, approved)).EnsureSuccessStatusCode();
        (await ApproveAsync(team.Kit.Hr, approved)).EnsureSuccessStatusCode();
        await SubmitLeaveAsync(team.StaffClient, "2026-09-20", "2026-09-20");

        var row = Items(await OkAsync(await team.Kit.Hr.GetAsync($"/api/app/report/hr-leave-balances?employeeId={team.Staff.Id}"))).Single();
        var own = await OkAsync(await team.StaffClient.GetAsync("/api/app/leave-balance/my"));

        row.GetProperty("entitlementDays").GetDecimal().ShouldBe(30);
        row.GetProperty("usedDays").GetDecimal().ShouldBe(2);
        row.GetProperty("heldDays").GetDecimal().ShouldBe(1);
        row.GetProperty("remainingDays").GetDecimal().ShouldBe(own.GetProperty("remainingDays").GetDecimal());
        row.GetProperty("carriedInDays").GetDecimal().ShouldBe(0);
        row.GetProperty("usedHours").GetDecimal().ShouldBe(0);
        row.GetProperty("leaveYearStart").GetString().ShouldBe(own.GetProperty("yearStartDate").GetString());
        Fields(row).ShouldBe(new[] { "carriedInDays", "employee", "entitlementDays", "heldDays", "leaveYearStart", "remainingDays", "usedDays", "usedHours" });
    }

    [Fact]
    public async Task Hr_requests_shows_when_a_request_closed()
    {
        var team = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 30);
        var open = await SubmitLeaveAsync(team.StaffClient, "2026-08-09", "2026-08-09");
        var closed = await SubmitLeaveAsync(team.StaffClient, "2026-08-16", "2026-08-16");
        (await team.StaffClient.PostAsJsonAsync($"/api/app/request/{closed}/cancel", new { note = (string?)null })).EnsureSuccessStatusCode();

        var today = DateTime.UtcNow;
        var range = $"from={today.AddDays(-1):yyyy-MM-dd}&to={today.AddDays(1):yyyy-MM-dd}";
        var rows = Items(await OkAsync(await team.Kit.Hr.GetAsync($"/api/app/report/hr-requests?{range}&employeeId={team.Staff.Id}")));

        rows.Count.ShouldBe(2);
        rows.Single(x => x.GetProperty("requestId").GetGuid() == open).GetProperty("closedAt").ValueKind.ShouldBe(JsonValueKind.Null);
        rows.Single(x => x.GetProperty("requestId").GetGuid() == closed).GetProperty("closedAt").ValueKind.ShouldBe(JsonValueKind.String);

        var past = await OkAsync(await team.Kit.Hr.GetAsync($"/api/app/report/hr-requests?from=2020-01-01&to=2020-01-31&employeeId={team.Staff.Id}"));
        past.GetProperty("totalCount").GetInt64().ShouldBe(0);
    }

    [Fact]
    public async Task The_review_dashboard_counts_complaints_and_never_those_the_reviewer_is_party_to()
    {
        var team = await TeamAsync();
        var (reviewer, _, reviewerEmployeeId) = await OwnerAsync(team.Kit, team.Kit.ExecutiveEmployeeId, HrRoles.Employee, HrRoles.Reviewer);

        var before = await DashboardAsync(reviewer);

        var confidential = await SubmitComplaintAsync(team.StaffClient, null, "confidential");
        var highly = await SubmitComplaintAsync(team.StaffClient, null, "highlyConfidential");
        await MakeLateAsync(confidential);
        await MakeLateAsync(highly);

        // A party's own complaints do not move any number, late or not.
        var against = await SubmitComplaintAsync(team.StaffClient, reviewerEmployeeId, "highlyConfidential");
        var byReviewer = await SubmitComplaintAsync(reviewer, null, "confidential");
        await MakeLateAsync(against);
        await MakeLateAsync(byReviewer);

        var after = await DashboardAsync(reviewer);

        Count(after, "confidentialSubmittedCount").ShouldBe(Count(before, "confidentialSubmittedCount") + 1);
        Count(after, "highlyConfidentialSubmittedCount").ShouldBe(Count(before, "highlyConfidentialSubmittedCount") + 1);
        Count(after, "lateConfidentialCount").ShouldBe(Count(before, "lateConfidentialCount") + 1);
        Count(after, "lateHighlyConfidentialCount").ShouldBe(Count(before, "lateHighlyConfidentialCount") + 1);
        Count(after, "lateNormalCount").ShouldBe(Count(before, "lateNormalCount"));
        after.EnumerateObject().Select(x => x.Name).OrderBy(x => x, StringComparer.Ordinal).ShouldBe(new[]
        {
            "asOf", "confidentialSubmittedCount", "highlyConfidentialSubmittedCount", "lateConfidentialCount", "lateHighlyConfidentialCount", "lateNormalCount"
        });

        var raw = await reviewer.GetStringAsync("/api/app/report/review-dashboard");
        raw.ShouldNotContain("Report test");
        raw.ShouldNotContain(confidential.ToString());
    }

    [Fact]
    public async Task The_review_dashboard_filters_submissions_by_month_and_starts_release_one_with_no_departments()
    {
        var team = await TeamAsync();
        var (reviewer, _, _) = await OwnerAsync(team.Kit, team.Kit.ExecutiveEmployeeId, HrRoles.Employee, HrRoles.Reviewer);
        await SubmitComplaintAsync(team.StaffClient, null, "confidential");

        var old = await OkAsync(await reviewer.GetAsync("/api/app/report/review-dashboard?year=2001&month=2"));
        old.GetProperty("year").GetInt32().ShouldBe(2001);
        old.GetProperty("month").GetInt32().ShouldBe(2);
        Count(old.GetProperty("complaintCounts"), "confidentialSubmittedCount").ShouldBe(0);
        old.GetProperty("cycleId").ValueKind.ShouldBe(JsonValueKind.Null);
        old.GetProperty("departments").GetArrayLength().ShouldBe(0);
        old.GetProperty("completedDepartmentsCount").GetInt32().ShouldBe(0);
        old.GetProperty("lateDepartmentsCount").GetInt32().ShouldBe(0);

        (await reviewer.GetAsync("/api/app/report/review-dashboard?year=1999")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await reviewer.GetAsync("/api/app/report/review-dashboard?month=13")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task The_executive_director_is_refused_the_review_dashboard_with_any_other_role()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var (executive, _) = await SignedInUserAsync(HrRoles.ExecutiveDirector, HrRoles.Reviewer);

        (await executive.GetAsync("/api/app/report/review-dashboard")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await executive.GetAsync("/api/app/report/reviewDashboard/export?format=excel")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        kit.ShouldNotBeNull();
    }

    private async Task<(HttpClient Client, Guid UserId)> SignedInUserAsync(params string[] roles)
    {
        var (userId, userName) = await TestUsers.CreateAsync(_host, false, roles);
        var client = TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, userName, TestUsers.Password)).AccessToken, "en");
        var enrollment = await OkAsync(await client.PostAsync("/api/app/account/mfa/enroll", null));
        (await client.PostAsJsonAsync("/api/app/account/mfa/confirm", new { code = Totp.Compute(enrollment.GetProperty("sharedKey").GetString()!) })).EnsureSuccessStatusCode();
        return (client, userId);
    }

    [Fact]
    public async Task Export_gives_the_same_rows_as_the_report_as_an_excel_file()
    {
        var team = await TeamAsync();
        await PunchAsync(team.Staff.Id, Sunday + "T05:30:00Z", Sunday + "T13:00:00Z");

        var response = await team.ManagerClient.GetAsync($"/api/app/report/teamAttendance/export?format=excel&{Period}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        response.Content.Headers.ContentType!.MediaType.ShouldBe(ReportFileWriter.ExcelContentType);

        using var workbook = new XLWorkbook(new MemoryStream(await response.Content.ReadAsByteArrayAsync()));
        var sheet = workbook.Worksheets.Single();
        sheet.RightToLeft.ShouldBeFalse();

        var text = sheet.CellsUsed().Select(x => x.GetString()).ToList();
        text.ShouldContain("Team attendance");
        text.ShouldContain(Sunday);
        text.ShouldContain("2026-10-04 08:30");
        text.ShouldContain("Late");

        var lateMinutes = sheet.CellsUsed().Single(x => x.GetString() == "30");
        lateMinutes.DataType.ShouldBe(XLDataType.Number);
    }

    [Fact]
    public async Task Export_writes_a_pdf_and_arabic_is_the_default_language()
    {
        var team = await TeamAsync();
        var arabic = await team.Kit.SignInAgainAsync(team.Manager, "ar");

        var pdf = await arabic.GetAsync("/api/app/report/teamPending/export?format=pdf");
        pdf.StatusCode.ShouldBe(HttpStatusCode.OK, await pdf.Content.ReadAsStringAsync());
        pdf.Content.Headers.ContentType!.MediaType.ShouldBe(ReportFileWriter.PdfContentType);
        pdf.Content.Headers.ContentDisposition!.FileName!.Trim('"').ShouldStartWith("teamPending-");
        Encoding.ASCII.GetString((await pdf.Content.ReadAsByteArrayAsync())[..5]).ShouldBe("%PDF-");

        var excel = await arabic.GetAsync("/api/app/report/teamPending/export?format=excel");
        using var workbook = new XLWorkbook(new MemoryStream(await excel.Content.ReadAsByteArrayAsync()));
        var sheet = workbook.Worksheets.Single();
        sheet.RightToLeft.ShouldBeTrue();
        sheet.CellsUsed().Select(x => x.GetString()).ShouldContain("الطلبات المعلقة للفريق");
    }

    [Fact]
    public async Task Export_checks_the_code_against_the_callers_group_and_its_filters()
    {
        var team = await TeamAsync();
        var (reviewer, _, _) = await OwnerAsync(team.Kit, team.Kit.ExecutiveEmployeeId, HrRoles.Employee, HrRoles.Reviewer);

        (await team.ManagerClient.GetAsync($"/api/app/report/hrAttendance/export?format=pdf&{Period}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await team.ManagerClient.GetAsync("/api/app/report/reviewDashboard/export?format=pdf")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await team.Kit.Hr.GetAsync($"/api/app/report/teamAttendance/export?format=pdf&{Period}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await reviewer.GetAsync("/api/app/report/hrLeaveBalances/export?format=pdf")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        await ShouldFailWithAsync(await team.ManagerClient.GetAsync("/api/app/report/teamAttendance/export?format=pdf"), HrErrorCodes.Reports.MissingFilter);
        await ShouldFailWithAsync(await team.Kit.Hr.GetAsync("/api/app/report/hrRequests/export?format=pdf&from=2026-10-01"), HrErrorCodes.Reports.MissingFilter);
        await ShouldFailWithAsync(await team.ManagerClient.GetAsync("/api/app/report/teamLeaves/export?format=pdf&from=2026-10-31&to=2026-10-01"), HrErrorCodes.Reports.InvalidPeriod);

        (await team.ManagerClient.GetAsync("/api/app/report/teamPending/export")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await team.ManagerClient.GetAsync("/api/app/report/teamPending/export?format=word")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await team.ManagerClient.GetAsync("/api/app/report/unknownCode/export?format=pdf")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("teamLatenessAbsence", true)]
    [InlineData("teamLeaves", true)]
    [InlineData("teamCustody", false)]
    [InlineData("teamEvaluations", false)]
    public async Task Every_team_code_exports_in_both_formats(string code, bool needsPeriod)
    {
        var team = await TeamAsync();
        var period = needsPeriod ? "&" + Period : string.Empty;

        foreach (var format in new[] { "pdf", "excel" })
        {
            var response = await team.ManagerClient.GetAsync($"/api/app/report/{code}/export?format={format}{period}");
            response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        }
    }

    [Fact]
    public async Task Hr_and_review_codes_export_and_the_review_file_holds_counts_only()
    {
        var team = await TeamAsync();
        var (reviewer, _, _) = await OwnerAsync(team.Kit, team.Kit.ExecutiveEmployeeId, HrRoles.Employee, HrRoles.Reviewer);
        await SubmitComplaintAsync(team.StaffClient, null, "confidential");

        foreach (var path in new[] { $"hrAttendance/export?format=excel&{Period}&employeeId={team.Staff.Id}", $"hrLeaveBalances/export?format=excel&employeeId={team.Staff.Id}", $"hrRequests/export?format=excel&{Period}&employeeId={team.Staff.Id}", $"hrAttendance/export?format=pdf&{Period}&employeeId={team.Staff.Id}" })
        {
            var response = await team.Kit.Hr.GetAsync("/api/app/report/" + path);
            response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        }

        (await reviewer.GetAsync("/api/app/report/evaluationConsistency/export?format=pdf")).StatusCode.ShouldBe(HttpStatusCode.OK);

        var file = await reviewer.GetAsync("/api/app/report/reviewDashboard/export?format=excel");
        file.StatusCode.ShouldBe(HttpStatusCode.OK, await file.Content.ReadAsStringAsync());
        using var workbook = new XLWorkbook(new MemoryStream(await file.Content.ReadAsByteArrayAsync()));
        var text = string.Join("|", workbook.Worksheets.Single().CellsUsed().Select(x => x.GetString()));
        text.ShouldNotContain("Report test");
        text.ShouldNotContain(team.Staff.UserName);
    }

    [Fact]
    public async Task An_export_appears_in_the_audit_log_by_its_code_and_a_plain_report_read_does_not()
    {
        var team = await TeamAsync();
        var manager = team.Manager.UserId;

        (await team.ManagerClient.GetAsync("/api/app/report/team-pending")).EnsureSuccessStatusCode();
        (await team.ManagerClient.GetAsync("/api/app/report/teamPending/export?format=excel")).EnsureSuccessStatusCode();

        var admin = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.SystemAdmin);
        var rows = Items(await OkAsync(await admin.GetAsync($"/api/app/audit-log?userId={manager}&action=export")));

        var row = rows.Single();
        row.GetProperty("entityType").GetString().ShouldBe("Report");
        row.GetProperty("entityId").GetString().ShouldBe("teamPending");
        row.GetProperty("succeeded").GetBoolean().ShouldBeTrue();
        row.GetProperty("changedFields").GetArrayLength().ShouldBe(0);
    }

    [Fact]
    public void The_pdf_writer_handles_arabic_text()
    {
        var table = new ReportTable(
            "حضور الفريق",
            "2026-10-01 - 2026-10-31",
            new[] { "الموظف", "القسم", "التاريخ", "الدخول", "دقائق التأخر", "الحالة" },
            new[]
            {
                new object?[] { "سارة خالد", "قسم المبيعات", "2026-10-04", "2026-10-04 08:30", 30, "متأخر" },
                new object?[] { "علي حسن", "الموارد البشرية", "2026-10-05", null, 0, "غائب" }
            });

        var pdf = ReportFileWriter.WritePdf(table, rightToLeft: true);
        Encoding.ASCII.GetString(pdf[..5]).ShouldBe("%PDF-");
    }

    [Fact]
    public void The_excel_writer_stores_a_formula_looking_name_as_text()
    {
        var table = new ReportTable("=Title:/", null, new[] { "Name", "Days" }, new[] { new object?[] { "=1+1", 1.5m } });

        using var workbook = new XLWorkbook(new MemoryStream(ReportFileWriter.WriteExcel(table, rightToLeft: false)));
        var cell = workbook.Worksheets.Single().CellsUsed().Single(x => x.GetString() == "=1+1");

        cell.HasFormula.ShouldBeFalse();
        cell.DataType.ShouldBe(XLDataType.Text);
    }
}
