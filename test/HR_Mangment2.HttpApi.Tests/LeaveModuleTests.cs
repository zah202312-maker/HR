using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HR_Mangment2.Errors;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Infrastructure;
using HR_Mangment2.Leaves;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;
using Xunit;

namespace HR_Mangment2;

/// <summary>
/// The kit's employees were hired on 2025-01-15 with the seeded Sunday to Thursday, 08:00 to 16:00 shift, so their
/// current year runs 2026-01-15 to 2027-01-14. 2026-09-06 is a Sunday. Each employee gets their own dates through
/// the fresh team, so tests do not collide on overlap checks.
/// </summary>
[Collection(HostCollection.Name)]
public class LeaveModuleTests
{
    private readonly HostFixture _host;

    public LeaveModuleTests(HostFixture host)
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

    private static async Task<JsonElement> ShouldFailWithAsync(HttpResponseMessage response, string code)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe((HttpStatusCode)422, body);
        var error = JsonDocument.Parse(body).RootElement.GetProperty("error");
        error.GetProperty("code").GetString().ShouldBe(code);
        return error;
    }

    private static decimal Dec(JsonElement element, string property) => element.GetProperty(property).GetDecimal();

    private static string Stamp(JsonElement balance) => balance.GetProperty("concurrencyStamp").GetString()!;

    private static async Task<JsonElement> BalanceOfAsync(HttpClient hr, Guid employeeId) =>
        await hr.GetFromJsonAsync<JsonElement>($"/api/app/leave-balance/employee/{employeeId}");

    private static async Task<JsonElement> SetEntitlementAsync(HttpClient hr, Guid employeeId, decimal days)
    {
        var balance = await BalanceOfAsync(hr, employeeId);
        return await OkAsync(await hr.PutAsJsonAsync(
            $"/api/app/leave-balance/employee/{employeeId}/entitlement",
            new { entitlementDays = days, concurrencyStamp = Stamp(balance) }));
    }

    private static object Annual(string start, string end, Guid? employeeId = null) => new { kind = "annual", startDate = start, endDate = end, employeeId };

    private static async Task<JsonElement> SubmitAnnualAsync(HttpClient client, string start, string end) =>
        await OkAsync(await client.PostAsJsonAsync("/api/app/leave-request", Annual(start, end)), HttpStatusCode.Created);

    private static Guid IdOf(JsonElement leave) => leave.GetProperty("requestId").GetGuid();

    private static Task<HttpResponseMessage> ApproveAsync(HttpClient client, Guid id) =>
        client.PostAsJsonAsync($"/api/app/request/{id}/approve", new { note = (string?)null });

    private async Task RollOverAsync(Guid employeeId, DateOnly today)
    {
        using var scope = _host.Factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var tenant = await services.GetRequiredService<ITenantRepository>().FindByNameAsync(
            services.GetRequiredService<ITenantNormalizer>().NormalizeName(HostFixture.TestTenantName)!);
        using var change = services.GetRequiredService<ICurrentTenant>().Change(tenant!.Id);
        using var uow = services.GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: true);

        var employee = await services.GetRequiredService<IRepository<Employee, Guid>>().GetAsync(employeeId);
        await services.GetRequiredService<LeaveBalanceManager>().RollOverAsync(employee, today);
        await uow.CompleteAsync();
    }

    private static HttpContent PdfUpload(string text = "%PDF-1.7 test", string contentType = "application/pdf")
    {
        var file = new ByteArrayContent(Encoding.ASCII.GetBytes(text));
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return new MultipartFormDataContent { { file, "file", "note.pdf" } };
    }

    [Fact]
    public async Task Before_an_entitlement_is_set_everything_is_zero_and_leave_is_refused()
    {
        var team = await TeamAsync();

        var balance = await team.StaffClient.GetFromJsonAsync<JsonElement>("/api/app/leave-balance/my");
        balance.GetProperty("isEntitlementSet").GetBoolean().ShouldBeFalse();
        balance.GetProperty("leaveYearId").ValueKind.ShouldBe(JsonValueKind.Null);
        Dec(balance, "remainingDays").ShouldBe(0);
        balance.GetProperty("yearStartDate").GetString().ShouldBe("2026-01-15");
        balance.GetProperty("yearEndDate").GetString().ShouldBe("2027-01-14");
        balance.GetProperty("nextYearStartDate").GetString().ShouldBe("2027-01-15");

        await ShouldFailWithAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", Annual("2026-09-06", "2026-09-07")), HrErrorCodes.Leave.EntitlementNotSet);
    }

    [Fact]
    public async Task The_first_entitlement_opens_the_year_and_later_changes_wait_for_next_year()
    {
        var team = await TeamAsync();
        var url = $"/api/app/leave-balance/employee/{team.Staff.Id}/entitlement";

        (await team.Kit.Hr.PutAsJsonAsync(url, new { entitlementDays = 24, concurrencyStamp = "stale" })).StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var first = await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 24);
        first.GetProperty("isEntitlementSet").GetBoolean().ShouldBeTrue();
        Dec(first, "currentYearEntitlementDays").ShouldBe(24);
        Dec(first, "remainingDays").ShouldBe(24);
        Dec(first, "nextYearEntitlementDays").ShouldBe(24);

        var ledger = await team.StaffClient.GetFromJsonAsync<JsonElement>("/api/app/leave-ledger/my");
        ledger.GetProperty("totalCount").GetInt64().ShouldBe(1);
        ledger.GetProperty("items")[0].GetProperty("type").GetString().ShouldBe("grant");
        ledger.GetProperty("items")[0].GetProperty("bucket").GetString().ShouldBe("entitlement");

        var second = await OkAsync(await team.Kit.Hr.PutAsJsonAsync(url, new { entitlementDays = 21, concurrencyStamp = Stamp(first) }));
        Dec(second, "currentYearEntitlementDays").ShouldBe(24);
        Dec(second, "nextYearEntitlementDays").ShouldBe(21);
        Dec(second, "remainingDays").ShouldBe(24);

        (await team.Kit.Hr.PutAsJsonAsync(url, new { entitlementDays = 30, concurrencyStamp = Stamp(first) })).StatusCode.ShouldBe(HttpStatusCode.Conflict);

        (await team.StaffClient.PutAsJsonAsync(url, new { entitlementDays = 99, concurrencyStamp = Stamp(second) })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task The_document_example_carries_6_and_adds_21_for_27()
    {
        var team = await TeamAsync();
        var set = await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 24);
        await OkAsync(await team.Kit.Hr.PutAsJsonAsync(
            $"/api/app/leave-balance/employee/{team.Staff.Id}/entitlement",
            new { entitlementDays = 21, concurrencyStamp = Stamp(set) }));

        // 18 working days: three and a half weeks of Sunday to Thursday.
        var leave = await SubmitAnnualAsync(team.StaffClient, "2026-06-07", "2026-07-01");
        Dec(leave, "workingDays").ShouldBe(19);

        var adjusted = await OkAsync(await team.Kit.Hr.PostAsJsonAsync($"/api/app/leave-balance/employee/{team.Staff.Id}/adjust", new { days = 1, reason = "Make the example come out" }));
        Dec(adjusted, "adjustmentDays").ShouldBe(1);

        await OkAsync(await ApproveAsync(team.ManagerClient, IdOf(leave)));
        await OkAsync(await ApproveAsync(team.Kit.Hr, IdOf(leave)));

        var before = await BalanceOfAsync(team.Kit.Hr, team.Staff.Id);
        Dec(before, "usedDays").ShouldBe(19);
        Dec(before, "heldDays").ShouldBe(0);
        Dec(before, "remainingDays").ShouldBe(6);

        await RollOverAsync(team.Staff.Id, new DateOnly(2027, 1, 15));
        await RollOverAsync(team.Staff.Id, new DateOnly(2027, 1, 15));

        var after = await BalanceOfAsync(team.Kit.Hr, team.Staff.Id);
        after.GetProperty("yearStartDate").GetString().ShouldBe("2027-01-15");
        Dec(after, "carriedDays").ShouldBe(6);
        Dec(after, "currentYearEntitlementDays").ShouldBe(21);
        Dec(after, "remainingDays").ShouldBe(27);
        Dec(after, "carriedRemainingDays").ShouldBe(6);
        Dec(after, "currentYearRemainingDays").ShouldBe(21);
        Dec(after, "usedDays").ShouldBe(0);
    }

    [Fact]
    public async Task Preview_counts_working_days_and_saves_nothing()
    {
        var team = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 3);

        // Sunday to the next Sunday: 6 working days, 2 weekly days off.
        var preview = await OkAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request/preview", Annual("2026-09-06", "2026-09-13")));

        preview.GetProperty("calendarDays").GetInt32().ShouldBe(8);
        Dec(preview, "workingDays").ShouldBe(6);
        preview.GetProperty("excludedWeeklyOffDays").GetInt32().ShouldBe(2);
        preview.GetProperty("isBalanceSufficient").GetBoolean().ShouldBeFalse();
        Dec(preview, "shortageDays").ShouldBe(3);
        Dec(preview, "remainingDaysAfter").ShouldBe(0);
        preview.GetProperty("canSubmit").GetBoolean().ShouldBeFalse();
        preview.GetProperty("issues")[0].GetProperty("code").GetString().ShouldBe(HrErrorCodes.Leave.InsufficientBalance);
        preview.GetProperty("dayBreakdown").EnumerateArray().Count(x => x.GetProperty("isCounted").GetBoolean()).ShouldBe(6);

        var balance = await BalanceOfAsync(team.Kit.Hr, team.Staff.Id);
        Dec(balance, "heldDays").ShouldBe(0);
    }

    [Fact]
    public async Task The_balance_is_held_at_submission_deducted_at_final_approval_and_released_on_rejection()
    {
        var team = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 10);

        var kept = await SubmitAnnualAsync(team.StaffClient, "2026-09-06", "2026-09-07");
        kept.GetProperty("balanceEffect").GetString().ShouldBe("held");
        kept.GetProperty("status").GetString().ShouldBe("pendingManager");
        Dec(kept, "fromEntitlementDays").ShouldBe(2);

        var held = await BalanceOfAsync(team.Kit.Hr, team.Staff.Id);
        Dec(held, "heldDays").ShouldBe(2);
        Dec(held, "remainingDays").ShouldBe(8);

        var rejected = await SubmitAnnualAsync(team.StaffClient, "2026-09-13", "2026-09-15");
        Dec(await BalanceOfAsync(team.Kit.Hr, team.Staff.Id), "remainingDays").ShouldBe(5);
        await OkAsync(await team.ManagerClient.PostAsJsonAsync($"/api/app/request/{IdOf(rejected)}/reject", new { reason = "Busy month" }));
        Dec(await BalanceOfAsync(team.Kit.Hr, team.Staff.Id), "remainingDays").ShouldBe(8);

        var cancelled = await SubmitAnnualAsync(team.StaffClient, "2026-09-20", "2026-09-21");
        await OkAsync(await team.StaffClient.PostAsJsonAsync($"/api/app/request/{IdOf(cancelled)}/cancel", new { note = "Changed my mind" }));
        var afterCancel = await team.StaffClient.GetFromJsonAsync<JsonElement>($"/api/app/leave-request/{IdOf(cancelled)}");
        afterCancel.GetProperty("balanceEffect").GetString().ShouldBe("released");
        afterCancel.GetProperty("status").GetString().ShouldBe("cancelled");

        await OkAsync(await ApproveAsync(team.ManagerClient, IdOf(kept)));
        Dec(await BalanceOfAsync(team.Kit.Hr, team.Staff.Id), "heldDays").ShouldBe(2);
        await OkAsync(await ApproveAsync(team.Kit.Hr, IdOf(kept)));

        var final = await BalanceOfAsync(team.Kit.Hr, team.Staff.Id);
        Dec(final, "usedDays").ShouldBe(2);
        Dec(final, "heldDays").ShouldBe(0);
        Dec(final, "remainingDays").ShouldBe(8);

        var approved = await team.StaffClient.GetFromJsonAsync<JsonElement>($"/api/app/leave-request/{IdOf(kept)}");
        approved.GetProperty("balanceEffect").GetString().ShouldBe("deducted");
        approved.GetProperty("status").GetString().ShouldBe("approved");

        var my = await team.StaffClient.GetFromJsonAsync<JsonElement>("/api/app/leave-request/my?status=approved");
        my.GetProperty("totalCount").GetInt64().ShouldBe(1);
    }

    [Fact]
    public async Task A_shortage_reports_its_figures_and_overlaps_are_refused()
    {
        var team = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 2);

        var error = await ShouldFailWithAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", Annual("2026-09-06", "2026-09-10")), HrErrorCodes.Leave.InsufficientBalance);
        var data = error.GetProperty("data");
        Dec(data, "availableDays").ShouldBe(2);
        Dec(data, "requestedDays").ShouldBe(5);
        Dec(data, "shortageDays").ShouldBe(3);

        await SubmitAnnualAsync(team.StaffClient, "2026-09-06", "2026-09-06");
        await ShouldFailWithAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", Annual("2026-09-06", "2026-09-07")), HrErrorCodes.Leave.OverlappingLeave);

        await ShouldFailWithAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", Annual("2026-09-11", "2026-09-12")), HrErrorCodes.Leave.NoWorkingDaysInRange);
        await ShouldFailWithAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", Annual("2026-09-10", "2026-09-09")), HrErrorCodes.Leave.InvalidDateRange);
        await ShouldFailWithAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", Annual("2024-01-01", "2024-01-02")), HrErrorCodes.Leave.InvalidDateRange);
        await ShouldFailWithAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", Annual("2028-02-01", "2028-02-02")), HrErrorCodes.Leave.RangeBeyondNextYear);
    }

    [Fact]
    public async Task The_contract_refuses_fields_from_another_kind()
    {
        var team = await TeamAsync();

        (await team.StaffClient.PostAsJsonAsync("/api/app/leave-request/preview", new { kind = "annual", date = "2026-09-06", startDate = "2026-09-06", endDate = "2026-09-07" }))
            .StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await team.StaffClient.PostAsJsonAsync("/api/app/leave-request/preview", new { kind = "hourly", date = "2026-09-06" }))
            .StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await team.StaffClient.PostAsJsonAsync("/api/app/leave-request/preview", new { kind = "other", startDate = "2026-09-06", endDate = "2026-09-07" }))
            .StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task A_leave_across_two_years_is_split_and_the_future_part_is_held_apart()
    {
        var team = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 8);

        // 2027-01-10 (Sunday) to 2027-01-21 (Thursday): 5 days in this year and 5 in the next.
        var preview = await OkAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request/preview", Annual("2027-01-10", "2027-01-21")));
        var segments = preview.GetProperty("segments");
        segments.GetArrayLength().ShouldBe(2);
        segments[0].GetProperty("isFutureYear").GetBoolean().ShouldBeFalse();
        Dec(segments[0], "workingDays").ShouldBe(5);
        Dec(segments[0], "fromEntitlementDays").ShouldBe(5);
        segments[1].GetProperty("isFutureYear").GetBoolean().ShouldBeTrue();
        Dec(segments[1], "workingDays").ShouldBe(5);
        Dec(segments[1], "fromCarriedDays").ShouldBe(3);
        Dec(segments[1], "fromEntitlementDays").ShouldBe(2);
        Dec(preview, "availableDaysBefore").ShouldBe(16);
        Dec(preview, "remainingDaysAfter").ShouldBe(6);

        var leave = await SubmitAnnualAsync(team.StaffClient, "2027-01-10", "2027-01-21");
        leave.GetProperty("segments").GetArrayLength().ShouldBe(2);

        var balance = await BalanceOfAsync(team.Kit.Hr, team.Staff.Id);
        Dec(balance, "heldDays").ShouldBe(5);
        Dec(balance, "remainingDays").ShouldBe(3);
        Dec(balance, "futureYearHeldDays").ShouldBe(5);

        // The three days left are already promised to the coming year, so they cannot be spent now.
        var spend = await OkAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request/preview", Annual("2026-09-06", "2026-09-06")));
        spend.GetProperty("isBalanceSufficient").GetBoolean().ShouldBeFalse();

        // When the year rolls, the held parts land correctly and nothing goes missing.
        await RollOverAsync(team.Staff.Id, new DateOnly(2027, 1, 15));
        var rolled = await BalanceOfAsync(team.Kit.Hr, team.Staff.Id);
        Dec(rolled, "carriedDays").ShouldBe(3);
        Dec(rolled, "currentYearEntitlementDays").ShouldBe(8);
        Dec(rolled, "heldDays").ShouldBe(5);
        Dec(rolled, "remainingDays").ShouldBe(6);
        Dec(rolled, "futureYearHeldDays").ShouldBe(0);
    }

    [Fact]
    public async Task Rejecting_a_leave_after_its_year_closed_returns_the_days_as_added_days()
    {
        var team = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 5);

        var leave = await SubmitAnnualAsync(team.StaffClient, "2026-09-06", "2026-09-07");
        await RollOverAsync(team.Staff.Id, new DateOnly(2027, 1, 15));
        Dec(await BalanceOfAsync(team.Kit.Hr, team.Staff.Id), "carriedDays").ShouldBe(3);

        await OkAsync(await team.ManagerClient.PostAsJsonAsync($"/api/app/request/{IdOf(leave)}/reject", new { reason = "Too late" }));

        var balance = await BalanceOfAsync(team.Kit.Hr, team.Staff.Id);
        Dec(balance, "carriedDays").ShouldBe(5);
        Dec(balance, "remainingDays").ShouldBe(10);
    }

    [Fact]
    public async Task Hourly_leave_uses_the_monthly_quota_then_the_annual_balance()
    {
        var team = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 10);

        // The seeded quota is 8 hours a month, and 8 hours make a day.
        var first = await OkAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", new { kind = "hourly", date = "2026-09-07", fromTime = "08:00", toTime = "14:00" }), HttpStatusCode.Created);
        Dec(first, "requestedHours").ShouldBe(6);
        Dec(first, "hourlyQuotaHoursUsed").ShouldBe(6);
        Dec(first, "hourlyExcessHours").ShouldBe(0);
        Dec(first, "annualDays").ShouldBe(0);
        first.GetProperty("balanceEffect").GetString().ShouldBe("held");

        var second = await OkAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request/preview", new { kind = "hourly", date = "2026-09-08", fromTime = "08:00", toTime = "12:00" }));
        Dec(second, "availableHourlyHoursBefore").ShouldBe(2);
        Dec(second, "hourlyQuotaHoursUsed").ShouldBe(2);
        Dec(second, "hourlyExcessHours").ShouldBe(2);
        Dec(second, "annualDays").ShouldBe(0.25m);

        var created = await OkAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", new { kind = "hourly", date = "2026-09-08", fromTime = "08:00", toTime = "12:00" }), HttpStatusCode.Created);
        Dec(await BalanceOfAsync(team.Kit.Hr, team.Staff.Id), "heldDays").ShouldBe(0.25m);

        // A different month has its own quota.
        var october = await OkAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request/preview", new { kind = "hourly", date = "2026-10-05", fromTime = "08:00", toTime = "12:00" }));
        Dec(october, "hourlyExcessHours").ShouldBe(0);

        await ShouldFailWithAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", new { kind = "hourly", date = "2026-09-07", fromTime = "13:00", toTime = "15:00" }), HrErrorCodes.Leave.OverlappingLeave);
        await ShouldFailWithAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", new { kind = "hourly", date = "2026-09-09", fromTime = "07:00", toTime = "09:00" }), HrErrorCodes.Leave.HourlyOutsideShift);
        await ShouldFailWithAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", new { kind = "hourly", date = "2026-09-11", fromTime = "09:00", toTime = "10:00" }), HrErrorCodes.Leave.HourlyOutsideShift);
        await ShouldFailWithAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", new { kind = "hourly", date = "2026-09-09", fromTime = "10:00", toTime = "09:00" }), HrErrorCodes.Leave.InvalidDateRange);

        await OkAsync(await team.StaffClient.PostAsJsonAsync($"/api/app/request/{IdOf(created)}/cancel", new { note = "No" }));
        var ledger = await team.StaffClient.GetFromJsonAsync<JsonElement>("/api/app/leave-ledger/my?type=release");
        ledger.GetProperty("items").EnumerateArray().Any(x => x.GetProperty("bucket").GetString() == "hourly").ShouldBeTrue();
    }

    [Fact]
    public async Task Other_types_follow_their_settings()
    {
        var team = await TeamAsync();
        var admin = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.SystemAdmin);
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 10);

        (await team.StaffClient.PostAsJsonAsync("/api/app/other-leave-type", new { nameAr = "م", nameEn = "S", isPaid = true, requiresAttachment = false, eligibleCategory = "all", isActive = true }))
            .StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var sick = await OkAsync(await admin.PostAsJsonAsync("/api/app/other-leave-type", new { nameAr = "مرضية", nameEn = "Sick", isPaid = true, maxDays = 3, requiresAttachment = true, eligibleCategory = "all", isActive = true }), HttpStatusCode.Created);
        sick.GetProperty("deductsFromAnnualBalance").GetBoolean().ShouldBeFalse();
        var sickId = sick.GetProperty("id").GetGuid();

        var maternity = await OkAsync(await admin.PostAsJsonAsync("/api/app/other-leave-type", new { nameAr = "ولادة", nameEn = "Maternity", isPaid = true, requiresAttachment = false, eligibleCategory = "female", isActive = true }), HttpStatusCode.Created);

        var available = await team.StaffClient.GetFromJsonAsync<JsonElement>("/api/app/other-leave-type/available");
        var ids = available.GetProperty("items").EnumerateArray().Select(x => x.GetProperty("id").GetGuid()).ToList();
        ids.ShouldContain(sickId);
        ids.ShouldNotContain(maternity.GetProperty("id").GetGuid());

        object Sick(string start, string end, Guid[]? attachments = null) => new { kind = "other", otherLeaveTypeId = sickId, startDate = start, endDate = end, attachmentIds = attachments ?? Array.Empty<Guid>() };

        await ShouldFailWithAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", Sick("2026-09-06", "2026-09-07")), HrErrorCodes.Leave.AttachmentRequired);
        await ShouldFailWithAsync(
            await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", new { kind = "other", otherLeaveTypeId = maternity.GetProperty("id").GetGuid(), startDate = "2026-09-06", endDate = "2026-09-07" }),
            HrErrorCodes.Leave.NotEligibleForLeaveType);

        var upload = await OkAsync(await team.StaffClient.PostAsync("/api/app/leave-request/attachment", PdfUpload()), HttpStatusCode.Created);
        var attachmentId = upload.GetProperty("id").GetGuid();

        await ShouldFailWithAsync(await team.ManagerClient.PostAsJsonAsync("/api/app/leave-request", new { kind = "annual", startDate = "2026-09-06", endDate = "2026-09-06", employeeId = team.Staff.Id, attachmentIds = new[] { attachmentId } }), HrErrorCodes.Leave.InvalidAttachment);

        await ShouldFailWithAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", Sick("2026-09-06", "2026-09-10", new[] { attachmentId })), HrErrorCodes.Leave.OtherLeaveTypeMaxExceeded);

        var leave = await OkAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", Sick("2026-09-06", "2026-09-07", new[] { attachmentId })), HttpStatusCode.Created);
        Dec(leave, "workingDays").ShouldBe(2);
        Dec(leave, "annualDays").ShouldBe(0);
        leave.GetProperty("balanceEffect").GetString().ShouldBe("none");
        leave.GetProperty("attachments").GetArrayLength().ShouldBe(1);
        leave.GetProperty("otherLeaveType").GetProperty("name").GetString().ShouldBe("Sick");

        var balance = await BalanceOfAsync(team.Kit.Hr, team.Staff.Id);
        Dec(balance, "remainingDays").ShouldBe(10);
        var usage = balance.GetProperty("otherLeaveUsages").EnumerateArray().Single(x => x.GetProperty("leaveTypeId").GetGuid() == sickId);
        Dec(usage, "heldDays").ShouldBe(2);
        Dec(usage, "remainingDays").ShouldBe(1);

        await ShouldFailWithAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", Sick("2026-09-13", "2026-09-13", new[] { attachmentId })), HrErrorCodes.Leave.InvalidAttachment);

        var url = await OkAsync(await team.StaffClient.GetAsync($"/api/app/leave-request/{IdOf(leave)}/attachment/{attachmentId}/download-url"));
        url.GetProperty("url").GetString().ShouldContain("/files/");

        var stranger = await team.Kit.AddStaffAsync(team.Manager.Id);
        var strangerClient = await team.Kit.SignInAsync(stranger);
        (await strangerClient.GetAsync($"/api/app/leave-request/{IdOf(leave)}/attachment/{attachmentId}/download-url")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await strangerClient.GetAsync($"/api/app/leave-request/{IdOf(leave)}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await team.StaffClient.GetAsync($"/api/app/leave-request/{IdOf(leave)}/attachment/{Guid.NewGuid()}/download-url")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var edited = await OkAsync(await admin.PutAsJsonAsync($"/api/app/other-leave-type/{sickId}", new { nameAr = "مرضية", nameEn = "Sick", isPaid = true, maxDays = 3, requiresAttachment = false, eligibleCategory = "all", deductsFromAnnualBalance = false, isActive = false, concurrencyStamp = sick.GetProperty("concurrencyStamp").GetString() }));
        edited.GetProperty("isActive").GetBoolean().ShouldBeFalse();
        await ShouldFailWithAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", Sick("2026-09-20", "2026-09-20")), HrErrorCodes.Leave.OtherLeaveTypeInactive);
        (await admin.GetFromJsonAsync<JsonElement>("/api/app/other-leave-type?isActive=false&maxResultCount=200")).GetProperty("items").EnumerateArray()
            .Select(x => x.GetProperty("id").GetGuid()).ShouldContain(sickId);
    }

    [Fact]
    public async Task A_deducting_other_type_holds_the_annual_balance()
    {
        var team = await TeamAsync();
        var admin = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.SystemAdmin);
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 10);

        var type = await OkAsync(await admin.PostAsJsonAsync("/api/app/other-leave-type", new { nameAr = "خاصة", nameEn = "Personal", isPaid = true, requiresAttachment = false, eligibleCategory = "all", deductsFromAnnualBalance = true, isActive = true }), HttpStatusCode.Created);

        var leave = await OkAsync(await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", new { kind = "other", otherLeaveTypeId = type.GetProperty("id").GetGuid(), startDate = "2026-09-06", endDate = "2026-09-08" }), HttpStatusCode.Created);

        Dec(leave, "annualDays").ShouldBe(3);
        leave.GetProperty("balanceEffect").GetString().ShouldBe("held");
        Dec(await BalanceOfAsync(team.Kit.Hr, team.Staff.Id), "heldDays").ShouldBe(3);
    }

    [Fact]
    public async Task Attachments_are_checked_on_upload()
    {
        var team = await TeamAsync();

        await ShouldFailWithAsync(await team.StaffClient.PostAsync("/api/app/leave-request/attachment", PdfUpload("not a pdf")), HrErrorCodes.Leave.AttachmentRejected);
        await ShouldFailWithAsync(await team.StaffClient.PostAsync("/api/app/leave-request/attachment", PdfUpload("%PDF-1.7", "text/plain")), HrErrorCodes.Leave.AttachmentRejected);
        await ShouldFailWithAsync(await team.StaffClient.PostAsync("/api/app/leave-request/attachment", PdfUpload("%PDF-1.7 EICAR-STANDARD-ANTIVIRUS-TEST-FILE")), HrErrorCodes.Leave.AttachmentRejected);
    }

    [Fact]
    public async Task A_manager_submits_for_their_team_only_and_it_starts_at_hr()
    {
        var team = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 10);

        var leave = await OkAsync(await team.ManagerClient.PostAsJsonAsync("/api/app/leave-request", Annual("2026-09-06", "2026-09-07", team.Staff.Id)), HttpStatusCode.Created);
        leave.GetProperty("isOnBehalf").GetBoolean().ShouldBeTrue();
        leave.GetProperty("status").GetString().ShouldBe("pendingHr");
        leave.GetProperty("employee").GetProperty("id").GetGuid().ShouldBe(team.Staff.Id);

        var otherTeam = await TeamAsync();
        (await team.ManagerClient.PostAsJsonAsync("/api/app/leave-request", Annual("2026-09-13", "2026-09-14", otherTeam.Staff.Id))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await team.StaffClient.PostAsJsonAsync("/api/app/leave-request", Annual("2026-09-13", "2026-09-14", team.Manager.Id))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        await ShouldFailWithAsync(await team.ManagerClient.PostAsJsonAsync("/api/app/leave-request", Annual("2026-09-13", "2026-09-14", team.Kit.ExecutiveEmployeeId)), HrErrorCodes.Leave.ExecutiveDirectorNotEligible);
    }

    [Fact]
    public async Task Hr_changes_balances_for_others_only_and_a_deduction_cannot_go_below_zero()
    {
        var team = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 5);
        var url = $"/api/app/leave-balance/employee/{team.Staff.Id}/adjust";

        (await team.Kit.Hr.PostAsJsonAsync(url, new { days = 0, reason = "Nothing" })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await team.Kit.Hr.PostAsJsonAsync(url, new { days = 1 })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        await ShouldFailWithAsync(await team.Kit.Hr.PostAsJsonAsync(url, new { days = -6, reason = "Too many" }), HrErrorCodes.Leave.AdjustmentExceedsBalance);

        var key = Guid.NewGuid().ToString();
        var send = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(new { days = 3, reason = "Extra days" }) };
        send.Headers.Add("Idempotency-Key", key);
        Dec(await OkAsync(await team.Kit.Hr.SendAsync(send)), "remainingDays").ShouldBe(8);

        var again = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(new { days = 3, reason = "Extra days" }) };
        again.Headers.Add("Idempotency-Key", key);
        Dec(await OkAsync(await team.Kit.Hr.SendAsync(again)), "remainingDays").ShouldBe(8);

        var deducted = await OkAsync(await team.Kit.Hr.PostAsJsonAsync(url, new { days = -8, reason = "Correction" }));
        Dec(deducted, "remainingDays").ShouldBe(0);
        Dec(deducted, "adjustmentDays").ShouldBe(-5);

        (await team.StaffClient.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200")).GetProperty("items").EnumerateArray()
            .Select(x => x.GetProperty("templateCode").GetString()).ShouldContain("leaveBalanceAdjusted");

        await ShouldFailWithAsync(await team.Kit.Hr.PostAsJsonAsync($"/api/app/leave-balance/employee/{team.Kit.ExecutiveEmployeeId}/adjust", new { days = 1, reason = "No" }), HrErrorCodes.Leave.ExecutiveDirectorNotEligible);
    }

    [Fact]
    public async Task Nobody_changes_their_own_balance()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var (userId, userName) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee, HrRoles.HrManager);
        var employeeId = await kit.AddFileForUserAsync(userId, kit.ExecutiveEmployeeId);
        var client = TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, userName, TestUsers.Password)).AccessToken, "en");
        var enrollment = await (await client.PostAsync("/api/app/account/mfa/enroll", null)).Content.ReadFromJsonAsync<JsonElement>();
        (await client.PostAsJsonAsync("/api/app/account/mfa/confirm", new { code = Totp.Compute(enrollment.GetProperty("sharedKey").GetString()!) })).EnsureSuccessStatusCode();

        await ShouldFailWithAsync(await client.PostAsJsonAsync($"/api/app/leave-balance/employee/{employeeId}/adjust", new { days = 1, reason = "Mine" }), HrErrorCodes.Leave.SelfAdjustmentNotAllowed);
        var balance = await BalanceOfAsync(client, employeeId);
        await ShouldFailWithAsync(await client.PutAsJsonAsync($"/api/app/leave-balance/employee/{employeeId}/entitlement", new { entitlementDays = 50, concurrencyStamp = Stamp(balance) }), HrErrorCodes.Leave.SelfAdjustmentNotAllowed);
    }

    [Fact]
    public async Task Balance_scope_follows_the_contract()
    {
        var team = await TeamAsync();
        var otherTeam = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 5);

        (await team.ManagerClient.GetAsync($"/api/app/leave-balance/employee/{team.Staff.Id}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await team.ManagerClient.GetAsync($"/api/app/leave-balance/employee/{otherTeam.Staff.Id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await team.ManagerClient.GetAsync($"/api/app/leave-ledger/employee/{otherTeam.Staff.Id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await team.StaffClient.GetAsync($"/api/app/leave-balance/employee/{team.Staff.Id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var admin = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.SystemAdmin);
        (await admin.GetAsync($"/api/app/leave-balance/employee/{team.Staff.Id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await admin.GetAsync($"/api/app/leave-ledger/employee/{team.Staff.Id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        await ShouldFailWithAsync(await team.Kit.Hr.GetAsync($"/api/app/leave-balance/employee/{team.Kit.ExecutiveEmployeeId}"), HrErrorCodes.Leave.ExecutiveDirectorNotEligible);

        (await team.ManagerClient.GetAsync($"/api/app/leave-request/employee/{team.Staff.Id}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await team.ManagerClient.GetAsync($"/api/app/leave-request/employee/{otherTeam.Staff.Id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task A_system_administrator_sees_the_balance_of_someone_whose_request_is_at_their_step()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var (userId, userName) = await TestUsers.CreateAsync(_host, false, HrRoles.Employee, HrRoles.SystemAdmin);
        var adminFileId = await kit.AddFileForUserAsync(userId, kit.ExecutiveEmployeeId);
        var admin = TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, userName, TestUsers.Password)).AccessToken, "en");
        var enrollment = await (await admin.PostAsync("/api/app/account/mfa/enroll", null)).Content.ReadFromJsonAsync<JsonElement>();
        (await admin.PostAsJsonAsync("/api/app/account/mfa/confirm", new { code = Totp.Compute(enrollment.GetProperty("sharedKey").GetString()!) })).EnsureSuccessStatusCode();

        var manager = await kit.AddAsync(adminFileId, new[] { HrRoles.DepartmentManager });
        var managerClient = await kit.SignInAsync(manager);
        await SetEntitlementAsync(kit.Hr, manager.Id, 5);

        (await admin.GetAsync($"/api/app/leave-balance/employee/{manager.Id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var leave = await SubmitAnnualAsync(managerClient, "2026-09-06", "2026-09-07");

        // The manager's request waits for this administrator, as their named manager.
        Dec(await OkAsync(await admin.GetAsync($"/api/app/leave-balance/employee/{manager.Id}")), "heldDays").ShouldBe(2);
        (await admin.GetAsync($"/api/app/leave-ledger/employee/{manager.Id}")).StatusCode.ShouldBe(HttpStatusCode.OK);

        await OkAsync(await ApproveAsync(admin, IdOf(leave)));
        (await admin.GetAsync($"/api/app/leave-balance/employee/{manager.Id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task The_team_calendar_shows_pending_and_approved_only()
    {
        var team = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 10);

        var pending = await SubmitAnnualAsync(team.StaffClient, "2026-09-06", "2026-09-07");
        var rejected = await SubmitAnnualAsync(team.StaffClient, "2026-09-13", "2026-09-14");
        await OkAsync(await team.ManagerClient.PostAsJsonAsync($"/api/app/request/{IdOf(rejected)}/reject", new { reason = "No" }));

        var calendar = await team.ManagerClient.GetFromJsonAsync<JsonElement>("/api/app/leave-calendar/team?fromDate=2026-09-01&toDate=2026-09-30");
        calendar.GetProperty("totalCount").GetInt64().ShouldBe(1);
        var entry = calendar.GetProperty("items")[0];
        entry.GetProperty("requestId").GetGuid().ShouldBe(IdOf(pending));
        entry.GetProperty("status").GetString().ShouldBe("pendingManager");
        Dec(entry, "workingDays").ShouldBe(2);

        (await team.ManagerClient.GetFromJsonAsync<JsonElement>("/api/app/leave-calendar/team?fromDate=2026-09-01&toDate=2026-09-30&includePending=false"))
            .GetProperty("totalCount").GetInt64().ShouldBe(0);

        await ShouldFailWithAsync(await team.ManagerClient.GetAsync("/api/app/leave-calendar/team?fromDate=2026-09-30&toDate=2026-09-01"), HrErrorCodes.Leave.InvalidDateRange);
        await ShouldFailWithAsync(await team.ManagerClient.GetAsync("/api/app/leave-calendar/team?fromDate=2026-01-01&toDate=2027-06-01"), HrErrorCodes.Leave.InvalidDateRange);
        (await team.ManagerClient.GetAsync("/api/app/leave-calendar/team?fromDate=2026-09-01")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await team.StaffClient.GetAsync("/api/app/leave-calendar/team?fromDate=2026-09-01&toDate=2026-09-30")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Ledger_rows_lock_the_hire_date()
    {
        var team = await TeamAsync();
        object Update(string stamp) => new
        {
            fullNameAr = "اسم",
            fullNameEn = "Name",
            email = EmployeeTestKit.UniqueEmail(),
            departmentId = team.Kit.DepartmentId,
            jobTitle = "Lead",
            hireDate = "2025-02-01",
            status = "active",
            concurrencyStamp = stamp
        };

        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 5);

        var file = await team.Kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/employee/{team.Staff.Id}");
        await ShouldFailWithAsync(
            await team.Kit.Hr.PutAsJsonAsync($"/api/app/employee/{team.Staff.Id}", Update(file.GetProperty("concurrencyStamp").GetString()!)),
            HrErrorCodes.Employee.HireDateLocked);
    }

    [Fact]
    public async Task A_manager_cannot_submit_for_a_suspended_team_member()
    {
        var team = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 5);

        var file = await team.Kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/employee/{team.Staff.Id}");
        await OkAsync(await team.Kit.Hr.PutAsJsonAsync($"/api/app/employee/{team.Staff.Id}", new
        {
            fullNameAr = "اسم",
            fullNameEn = "Name",
            email = EmployeeTestKit.UniqueEmail(),
            departmentId = team.Kit.DepartmentId,
            jobTitle = "Lead",
            hireDate = "2025-01-15",
            status = "suspended",
            concurrencyStamp = file.GetProperty("concurrencyStamp").GetString()
        }));

        await ShouldFailWithAsync(await team.ManagerClient.PostAsJsonAsync("/api/app/leave-request/preview", Annual("2026-09-06", "2026-09-07", team.Staff.Id)), HrErrorCodes.Leave.EmployeeNotActive);
        await ShouldFailWithAsync(await team.ManagerClient.PostAsJsonAsync("/api/app/leave-request", Annual("2026-09-06", "2026-09-07", team.Staff.Id)), HrErrorCodes.Leave.EmployeeNotActive);
    }

    [Fact]
    public async Task The_workers_rollover_finds_due_employees_and_is_safe_to_repeat()
    {
        var team = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 5);

        using var scope = _host.Factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var tenant = await services.GetRequiredService<ITenantRepository>().FindByNameAsync(
            services.GetRequiredService<ITenantNormalizer>().NormalizeName(HostFixture.TestTenantName)!);
        using var change = services.GetRequiredService<ICurrentTenant>().Change(tenant!.Id);
        var maintenance = services.GetRequiredService<LeaveMaintenanceService>();

        await maintenance.RollOverDueYearsAsync(new DateOnly(2027, 1, 15));
        await maintenance.RollOverDueYearsAsync(new DateOnly(2027, 1, 15));

        var balance = await BalanceOfAsync(team.Kit.Hr, team.Staff.Id);
        balance.GetProperty("yearStartDate").GetString().ShouldBe("2027-01-15");
        Dec(balance, "carriedDays").ShouldBe(5);
        Dec(balance, "currentYearEntitlementDays").ShouldBe(5);
        Dec(balance, "remainingDays").ShouldBe(10);

        var ledger = await team.StaffClient.GetFromJsonAsync<JsonElement>("/api/app/leave-ledger/my");
        ledger.GetProperty("totalCount").GetInt64().ShouldBe(3);
        ledger.GetProperty("items").EnumerateArray().Count(x => x.GetProperty("type").GetString() == "carryIn").ShouldBe(1);
        ledger.GetProperty("items").EnumerateArray().Single(x => x.GetProperty("type").GetString() == "carryIn").GetProperty("createdBy").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task Two_submissions_at_once_cannot_spend_the_same_days()
    {
        var team = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 5);

        var results = await Task.WhenAll(
            team.StaffClient.PostAsJsonAsync("/api/app/leave-request", Annual("2026-09-06", "2026-09-09")),
            team.StaffClient.PostAsJsonAsync("/api/app/leave-request", Annual("2026-09-13", "2026-09-16")));

        results.Count(x => x.StatusCode == HttpStatusCode.Created).ShouldBeLessThanOrEqualTo(1);
        var balance = await BalanceOfAsync(team.Kit.Hr, team.Staff.Id);
        Dec(balance, "remainingDays").ShouldBeGreaterThanOrEqualTo(0);
        Dec(balance, "heldDays").ShouldBeLessThanOrEqualTo(5);
    }

    [Fact]
    public async Task HR_reverses_an_approved_leave_and_the_days_come_back()
    {
        var team = await TeamAsync();
        await SetEntitlementAsync(team.Kit.Hr, team.Staff.Id, 24);

        var pending = await SubmitAnnualAsync(team.StaffClient, "2026-06-07", "2026-06-11");
        var url = $"/api/app/leave-request/{IdOf(pending)}/reverse";
        await ShouldFailWithAsync(await team.Kit.Hr.PostAsJsonAsync(url, new { reason = "Too early" }), HrErrorCodes.Leave.RequestNotReversible);

        await OkAsync(await ApproveAsync(team.ManagerClient, IdOf(pending)));
        await OkAsync(await ApproveAsync(team.Kit.Hr, IdOf(pending)));
        var used = Dec(await BalanceOfAsync(team.Kit.Hr, team.Staff.Id), "usedDays");
        used.ShouldBeGreaterThan(0);

        (await team.ManagerClient.PostAsJsonAsync(url, new { reason = "Not mine" })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await team.Kit.Hr.PostAsJsonAsync(url, new { reason = "" })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        await OkAsync(await team.Kit.Hr.PostAsJsonAsync(url, new { reason = "Worked those days" }));

        var after = await BalanceOfAsync(team.Kit.Hr, team.Staff.Id);
        Dec(after, "usedDays").ShouldBe(0);
        Dec(after, "heldDays").ShouldBe(0);

        var ledger = await team.Kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/leave-ledger/employee/{team.Staff.Id}?maxResultCount=200");
        ledger.GetProperty("items").EnumerateArray().Count(x => x.GetProperty("type").GetString() == "reverse").ShouldBeGreaterThan(0);

        var request = await team.Kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/request/{IdOf(pending)}");
        request.GetProperty("status").GetString().ShouldBe("cancelled");

        await ShouldFailWithAsync(await team.Kit.Hr.PostAsJsonAsync(url, new { reason = "Again" }), HrErrorCodes.Leave.RequestNotReversible);
    }
}
