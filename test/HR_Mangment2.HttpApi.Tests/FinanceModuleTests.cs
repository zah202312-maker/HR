using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HR_Mangment2.Auditing;
using HR_Mangment2.Errors;
using HR_Mangment2.Finance;
using HR_Mangment2.Identity;
using HR_Mangment2.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;
using Xunit;

namespace HR_Mangment2;

[Collection(HostCollection.Name)]
public class FinanceModuleTests
{
    private static readonly byte[] Pdf = Encoding.ASCII.GetBytes("%PDF-1.4 test");

    /// <summary>Each run needs a period of its own, because a period has one run and the database is shared by the tests.</summary>
    private static int _nextPeriod = Random.Shared.Next(0, 5000);

    private static string NextPeriod()
    {
        var n = Interlocked.Increment(ref _nextPeriod);
        return $"{3000 + n / 12:D4}-{n % 12 + 1:D2}";
    }

    private readonly HostFixture _host;

    public FinanceModuleTests(HostFixture host)
    {
        _host = host;
    }

    private static async Task ShouldFailWithAsync(HttpResponseMessage response, string code)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe((HttpStatusCode)422, body);
        JsonDocument.Parse(body).RootElement.GetProperty("error").GetProperty("code").GetString().ShouldBe(code);
    }

    private static async Task<JsonElement> OkAsync(HttpResponseMessage response, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe(expected, body);
        return JsonDocument.Parse(body).RootElement;
    }

    private static Guid IdOf(JsonElement element) => element.GetProperty("id").GetGuid();

    private static string Text(JsonElement element, string property) => element.GetProperty(property).GetString()!;

    private static decimal Number(JsonElement element, string property) => element.GetProperty(property).GetDecimal();

    private async Task RunAsync(Func<IServiceProvider, Task> work)
    {
        using var scope = _host.Factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var tenant = await services.GetRequiredService<ITenantRepository>().FindByNameAsync(
            services.GetRequiredService<ITenantNormalizer>().NormalizeName(HostFixture.TestTenantName)!);
        using var change = services.GetRequiredService<ICurrentTenant>().Change(tenant!.Id);
        using var uow = services.GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: true);
        await work(services);
        await uow.CompleteAsync();
    }

    private record Team(EmployeeTestKit Kit, HttpClient Accountant, CreatedEmployee Manager, HttpClient ManagerClient, CreatedEmployee Staff, HttpClient StaffClient);

    private async Task<Team> TeamAsync()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var accountant = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.Accountant);
        return new Team(kit, accountant, manager, await kit.SignInAsync(manager), staff, await kit.SignInAsync(staff));
    }

    private static Task<JsonElement> SetSalaryAsync(HttpClient accountant, Guid employeeId, string from = "2000-01-01", string? stamp = null) =>
        accountant.PutAsJsonAsync($"/api/app/finance/salary-profile/{employeeId}", new
        {
            baseSalary = 1000m,
            currency = "IQD",
            effectiveFrom = from,
            concurrencyStamp = stamp,
            components = new object[]
            {
                new { name = "Transport", type = "allowance", amount = 100m },
                new { name = "Insurance", type = "fixedDeduction", amount = 50m }
            }
        }).ContinueWith(x => OkAsync(x.Result)).Unwrap();

    private static MultipartFormDataContent AdjustmentForm(Guid employeeId, string period, string type = "addition", decimal amount = 25m, byte[]? file = null)
    {
        var part = new ByteArrayContent(file ?? Pdf);
        part.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        return new MultipartFormDataContent
        {
            { new StringContent(employeeId.ToString()), "employeeId" },
            { new StringContent(period), "period" },
            { new StringContent(type), "type" },
            { new StringContent(amount.ToString(System.Globalization.CultureInfo.InvariantCulture)), "amount" },
            { new StringContent("Reason"), "reason" },
            { part, "attachment", "a.pdf" }
        };
    }

    private static async Task<JsonElement> CreateRunAsync(HttpClient accountant, string period) =>
        await OkAsync(await accountant.PostAsJsonAsync("/api/app/finance/payroll-run", new { period }), HttpStatusCode.Created);

    private static JsonElement ItemFor(JsonElement run, Guid employeeId) =>
        run.GetProperty("items").EnumerateArray().Single(x => IdOf(x.GetProperty("employee")) == employeeId);

    private static object Action(JsonElement run) => new { concurrencyStamp = Text(run, "concurrencyStamp") };

    [Fact]
    public async Task Salary_versions_are_kept_and_the_rules_hold()
    {
        var team = await TeamAsync();
        var url = $"/api/app/finance/salary-profile/{team.Staff.Id}";

        (await team.Accountant.GetAsync(url)).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var first = await SetSalaryAsync(team.Accountant, team.Staff.Id);
        Number(first, "baseSalary").ShouldBe(1000m);
        first.GetProperty("components").GetArrayLength().ShouldBe(2);

        (await team.Accountant.PutAsJsonAsync(url, new { baseSalary = 1m, currency = "IQD", effectiveFrom = "2001-01-01", concurrencyStamp = "stale", components = Array.Empty<object>() }))
            .StatusCode.ShouldBe(HttpStatusCode.Conflict);

        await ShouldFailWithAsync(
            await team.Accountant.PutAsJsonAsync(url, new { baseSalary = 1m, currency = "IQD", effectiveFrom = "2000-01-01", concurrencyStamp = Text(first, "concurrencyStamp"), components = Array.Empty<object>() }),
            HrErrorCodes.Finance.EffectiveDateNotAfterLatest);

        await ShouldFailWithAsync(
            await team.Accountant.PutAsJsonAsync(url, new
            {
                baseSalary = 1m,
                currency = "IQD",
                effectiveFrom = "2001-01-01",
                concurrencyStamp = Text(first, "concurrencyStamp"),
                components = new object[] { new { name = "A", type = "allowance", amount = 1m }, new { name = "a", type = "allowance", amount = 1m } }
            }),
            HrErrorCodes.Finance.DuplicateComponentName);

        (await team.Accountant.PutAsJsonAsync(url, new { baseSalary = 1m, currency = "iqd", effectiveFrom = "2001-01-01", concurrencyStamp = Text(first, "concurrencyStamp"), components = Array.Empty<object>() }))
            .StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        await SetSalaryAsync(team.Accountant, team.Staff.Id, "2001-01-01", Text(first, "concurrencyStamp"));

        var history = await team.Accountant.GetFromJsonAsync<JsonElement>(url + "/history");
        history.GetProperty("totalCount").GetInt64().ShouldBe(2);
        Text(history.GetProperty("items")[0], "effectiveFrom").ShouldBe("2001-01-01");
        Text(await team.Accountant.GetFromJsonAsync<JsonElement>(url + "?asOf=2000-06-01"), "effectiveFrom").ShouldBe("2000-01-01");

        var list = await team.Accountant.GetFromJsonAsync<JsonElement>($"/api/app/finance/salary-profile?filter=Staff&hasProfile=true&maxResultCount=200");
        list.GetProperty("items").EnumerateArray().Any(x => IdOf(x.GetProperty("employee")) == team.Staff.Id).ShouldBeTrue();
    }

    [Fact]
    public async Task Financial_routes_refuse_the_manager_the_store_officer_and_HR()
    {
        var team = await TeamAsync();
        var officer = await team.Kit.SignInAsync(await team.Kit.AddAsync(team.Manager.Id, new[] { HrRoles.Employee, HrRoles.StoreOfficer }));

        foreach (var client in new[] { team.ManagerClient, officer, team.StaffClient, team.Kit.Hr })
        {
            (await client.GetAsync("/api/app/finance/salary-profile")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
            (await client.GetAsync("/api/app/finance/adjustment")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
            (await client.GetAsync("/api/app/finance/expense")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
            (await client.GetAsync("/api/app/finance/accountant-request")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
            (await client.GetAsync("/api/app/finance/report/store-expenses?dateFrom=2000-01-01&dateTo=2000-01-02")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        }

        (await team.ManagerClient.GetAsync("/api/app/finance/payroll-run")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await officer.GetAsync("/api/app/finance/payroll-run")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Payroll_runs_from_draft_to_payslip_with_the_right_numbers()
    {
        var team = await TeamAsync();
        await SetSalaryAsync(team.Accountant, team.Staff.Id);
        var period = NextPeriod();

        await OkAsync(await team.Accountant.PostAsync("/api/app/finance/adjustment", AdjustmentForm(team.Staff.Id, period, "addition", 25m)), HttpStatusCode.Created);
        var deduction = await OkAsync(await team.Accountant.PostAsync("/api/app/finance/adjustment", AdjustmentForm(team.Staff.Id, period, "deduction", 10m)), HttpStatusCode.Created);
        Text(deduction, "currency").ShouldBe("IQD");

        var run = await CreateRunAsync(team.Accountant, period);
        Text(run, "status").ShouldBe("draft");
        var item = ItemFor(run, team.Staff.Id);
        Number(item, "gross").ShouldBe(1100m);
        Number(item, "additions").ShouldBe(25m);
        Number(item, "deductions").ShouldBe(60m);
        Number(item, "net").ShouldBe(1065m);
        item.GetProperty("lines").GetArrayLength().ShouldBe(5);
        Text(item.GetProperty("lines")[0], "label").ShouldBe("Base salary");
        var id = IdOf(run);

        (await team.Accountant.PostAsJsonAsync("/api/app/finance/payroll-run", new { period })).StatusCode.ShouldBe(HttpStatusCode.Conflict);

        // The employee sees nothing before the run closes.
        (await team.StaffClient.GetAsync($"/api/app/finance/payslip/my/{IdOf(item)}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // Approval needs an explicit request first, and a stale run cannot go forward.
        var hr = team.Kit.Hr;
        await ShouldFailWithAsync(await hr.PostAsJsonAsync($"/api/app/finance/payroll-run/{id}/approve", Action(run)), HrErrorCodes.Finance.ApprovalNotRequested);

        await OkAsync(await team.Accountant.PostAsync("/api/app/finance/adjustment", AdjustmentForm(team.Staff.Id, period, "addition", 5m)), HttpStatusCode.Created);
        run = await OkAsync(await team.Accountant.GetAsync($"/api/app/finance/payroll-run/{id}"));
        run.GetProperty("isStale").GetBoolean().ShouldBeTrue();
        await ShouldFailWithAsync(await team.Accountant.PostAsJsonAsync($"/api/app/finance/payroll-run/{id}/request-approval", Action(run)), HrErrorCodes.Finance.PayrollRunStale);

        (await team.Accountant.PostAsJsonAsync($"/api/app/finance/payroll-run/{id}/recalculate", new { concurrencyStamp = "stale" })).StatusCode.ShouldBe(HttpStatusCode.Conflict);
        run = await OkAsync(await team.Accountant.PostAsJsonAsync($"/api/app/finance/payroll-run/{id}/recalculate", Action(run)));
        run.GetProperty("isStale").GetBoolean().ShouldBeFalse();
        Number(ItemFor(run, team.Staff.Id), "net").ShouldBe(1070m);

        run = await OkAsync(await team.Accountant.PostAsJsonAsync($"/api/app/finance/payroll-run/{id}/request-approval", Action(run)));
        run.GetProperty("approvalRequestedAt").ValueKind.ShouldBe(JsonValueKind.String);
        Text(run, "status").ShouldBe("draft");

        var returned = await OkAsync(await hr.PostAsJsonAsync($"/api/app/finance/payroll-run/{id}/return", new { reason = "Check it" }));
        returned.GetProperty("approvalRequestedAt").ValueKind.ShouldBe(JsonValueKind.Null);
        await ShouldFailWithAsync(await hr.PostAsJsonAsync($"/api/app/finance/payroll-run/{id}/return", new { reason = "again" }), HrErrorCodes.Finance.ApprovalNotRequested);

        run = await OkAsync(await team.Accountant.PostAsJsonAsync($"/api/app/finance/payroll-run/{id}/request-approval", Action(returned)));

        var executive = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.ExecutiveDirector);
        (await executive.GetAsync($"/api/app/finance/payroll-run/{id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        await ShouldFailWithAsync(await executive.PostAsJsonAsync($"/api/app/finance/payroll-run/{id}/approve", Action(run)), HrErrorCodes.Finance.NotConfiguredPayrollApprover);

        await ShouldFailWithAsync(await team.Accountant.PostAsJsonAsync($"/api/app/finance/payroll-run/{id}/close", Action(run)), HrErrorCodes.Finance.InvalidPayrollRunStatus);

        run = await OkAsync(await hr.PostAsJsonAsync($"/api/app/finance/payroll-run/{id}/approve", Action(run)));
        Text(run, "status").ShouldBe("approved");
        run.GetProperty("isStale").GetBoolean().ShouldBeFalse();

        // The period is now locked, and its movements are fixed.
        await ShouldFailWithAsync(await team.Accountant.PostAsync("/api/app/finance/adjustment", AdjustmentForm(team.Staff.Id, period)), HrErrorCodes.Finance.PeriodLocked);
        var fixedRows = await team.Accountant.GetFromJsonAsync<JsonElement>($"/api/app/finance/adjustment?period={period}&employeeId={team.Staff.Id}&status=includedInRun");
        fixedRows.GetProperty("totalCount").GetInt64().ShouldBe(3);

        // Salary delivery is marked only on a closed run (decision 2026-11-11).
        await ShouldFailWithAsync(
            await team.Accountant.PostAsJsonAsync($"/api/app/finance/payroll-run/{id}/deliver", new { deliveredOn = DateTime.UtcNow.ToString("yyyy-MM-dd") }),
            HrErrorCodes.Finance.InvalidPayrollRunStatus);

        run = await OkAsync(await team.Accountant.PostAsJsonAsync($"/api/app/finance/payroll-run/{id}/close", Action(run)));
        Text(run, "status").ShouldBe("closed");
        run.GetProperty("deliveredCount").GetInt32().ShouldBe(0);

        await ShouldFailWithAsync(
            await team.Accountant.PostAsJsonAsync($"/api/app/finance/payroll-run/{id}/deliver", new { employeeIds = new[] { team.Staff.Id }, deliveredOn = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-dd") }),
            HrErrorCodes.Finance.DeliveryDateInFuture);
        await ShouldFailWithAsync(
            await team.Accountant.PostAsJsonAsync($"/api/app/finance/payroll-run/{id}/deliver", new { employeeIds = new[] { Guid.NewGuid() }, deliveredOn = DateTime.UtcNow.ToString("yyyy-MM-dd") }),
            HrErrorCodes.Finance.EmployeeNotInPayroll);
        var delivered = await OkAsync(await team.Accountant.PostAsJsonAsync($"/api/app/finance/payroll-run/{id}/deliver", new { employeeIds = new[] { team.Staff.Id }, deliveredOn = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd") }));
        ItemFor(delivered, team.Staff.Id).GetProperty("deliveredOn").GetString().ShouldBe(DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd"));
        ItemFor(delivered, team.Staff.Id).GetProperty("deliveredBy").ValueKind.ShouldBe(JsonValueKind.Object);
        delivered.GetProperty("deliveredCount").GetInt32().ShouldBe(1);
        var mySlips = await team.StaffClient.GetFromJsonAsync<JsonElement>("/api/app/finance/payslip/my");
        mySlips.GetProperty("items").EnumerateArray().Single(x => Text(x, "period") == period).GetProperty("deliveredOn").ValueKind.ShouldBe(JsonValueKind.String);
        var undone = await OkAsync(await team.Accountant.PostAsJsonAsync($"/api/app/finance/payroll-run/{id}/undo-delivery", new { employeeIds = new[] { team.Staff.Id }, reason = "Wrong person" }));
        ItemFor(undone, team.Staff.Id).GetProperty("deliveredOn").ValueKind.ShouldBe(JsonValueKind.Null);
        (await team.StaffClient.PostAsJsonAsync($"/api/app/finance/payroll-run/{id}/deliver", new { deliveredOn = DateTime.UtcNow.ToString("yyyy-MM-dd") })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        run = undone;

        var slipId = IdOf(ItemFor(run, team.Staff.Id));
        var slips = await team.StaffClient.GetFromJsonAsync<JsonElement>("/api/app/finance/payslip/my");
        slips.GetProperty("items").EnumerateArray().Any(x => IdOf(x) == slipId).ShouldBeTrue();
        Number(await OkAsync(await team.StaffClient.GetAsync($"/api/app/finance/payslip/my/{slipId}")), "net").ShouldBe(1070m);
        (await team.ManagerClient.GetAsync($"/api/app/finance/payslip/my/{slipId}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var pdf = await team.StaffClient.GetAsync($"/api/app/finance/payslip/my/{slipId}/pdf");
        pdf.StatusCode.ShouldBe(HttpStatusCode.OK);
        pdf.Content.Headers.ContentType!.MediaType.ShouldBe("application/pdf");

        (await team.Accountant.GetAsync($"/api/app/finance/payroll-run/{id}/export?format=excel")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await hr.GetAsync($"/api/app/finance/payroll-run/{id}/export?format=pdf")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await hr.GetAsync($"/api/app/finance/payroll-run/{id}/export")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // The notice names the payslip and carries no amount.
        var notices = await team.StaffClient.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200");
        var slipNotice = notices.GetProperty("items").EnumerateArray().Single(x => x.GetProperty("resourceId").ValueKind == JsonValueKind.String && x.GetProperty("resourceId").GetGuid() == slipId);
        slipNotice.GetRawText().ShouldNotContain("1070");

        // A closed month is a real closed month for attendance, and a reversal settles in a later period.
        await RunAsync(async services =>
        {
            var closed = services.GetRequiredService<HR_Mangment2.Attendance.IAttendanceClosedMonthReader>();
            (await closed.IsClosedAsync(FinancePeriod.FirstDay(period))).ShouldBeTrue();
        });
    }

    [Fact]
    public async Task Reversing_an_unpaid_movement_cancels_it_and_a_paid_one_settles_later()
    {
        var team = await TeamAsync();
        await SetSalaryAsync(team.Accountant, team.Staff.Id);
        var period = NextPeriod();
        var next = FinancePeriod.Next(period);

        var unpaid = await OkAsync(await team.Accountant.PostAsync("/api/app/finance/adjustment", AdjustmentForm(team.Staff.Id, period, "addition", 40m)), HttpStatusCode.Created);
        var reversal = await OkAsync(await team.Accountant.PostAsJsonAsync($"/api/app/finance/adjustment/{IdOf(unpaid)}/reverse", new { reason = "Mistake", period }));
        Text(reversal, "type").ShouldBe("deduction");
        Text(reversal, "source").ShouldBe("reversal");
        reversal.GetProperty("reversalOfAdjustmentId").GetGuid().ShouldBe(IdOf(unpaid));
        await ShouldFailWithAsync(await team.Accountant.PostAsJsonAsync($"/api/app/finance/adjustment/{IdOf(unpaid)}/reverse", new { reason = "Again", period }), HrErrorCodes.Finance.AdjustmentAlreadyReversed);

        var run = await CreateRunAsync(team.Accountant, period);
        Number(ItemFor(run, team.Staff.Id), "net").ShouldBe(1050m);

        var paid = await OkAsync(await team.Accountant.PostAsync("/api/app/finance/adjustment", AdjustmentForm(team.Staff.Id, period, "addition", 70m)), HttpStatusCode.Created);
        run = await OkAsync(await team.Accountant.GetAsync($"/api/app/finance/payroll-run/{IdOf(run)}"));
        run = await OkAsync(await team.Accountant.PostAsJsonAsync($"/api/app/finance/payroll-run/{IdOf(run)}/recalculate", Action(run)));
        run = await OkAsync(await team.Accountant.PostAsJsonAsync($"/api/app/finance/payroll-run/{IdOf(run)}/request-approval", Action(run)));
        await OkAsync(await team.Kit.Hr.PostAsJsonAsync($"/api/app/finance/payroll-run/{IdOf(run)}/approve", Action(run)));

        await ShouldFailWithAsync(await team.Accountant.PostAsJsonAsync($"/api/app/finance/adjustment/{IdOf(paid)}/reverse", new { reason = "Late", period }), HrErrorCodes.Finance.PeriodLocked);
        await OkAsync(await team.Accountant.PostAsJsonAsync($"/api/app/finance/adjustment/{IdOf(paid)}/reverse", new { reason = "Late", period = next }));

        var nextRun = await CreateRunAsync(team.Accountant, next);
        Number(ItemFor(nextRun, team.Staff.Id), "net").ShouldBe(980m);
    }

    [Fact]
    public async Task Adjustment_rules_and_attachments()
    {
        var team = await TeamAsync();
        var period = NextPeriod();

        await ShouldFailWithAsync(await team.Accountant.PostAsync("/api/app/finance/adjustment", AdjustmentForm(Guid.NewGuid(), period)), HrErrorCodes.Finance.EmployeeNotFound);
        await ShouldFailWithAsync(await team.Accountant.PostAsync("/api/app/finance/adjustment", AdjustmentForm(team.Staff.Id, period)), HrErrorCodes.Finance.SalaryProfileMissing);

        await SetSalaryAsync(team.Accountant, team.Staff.Id);
        await ShouldFailWithAsync(await team.Accountant.PostAsync("/api/app/finance/adjustment", AdjustmentForm(team.Staff.Id, period, file: Encoding.ASCII.GetBytes("nope"))), HrErrorCodes.Document.FileTypeNotAllowed);
        (await team.Accountant.PostAsync("/api/app/finance/adjustment", AdjustmentForm(team.Staff.Id, "2026-13"))).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await team.Accountant.PostAsync("/api/app/finance/adjustment", AdjustmentForm(team.Staff.Id, period, amount: 0m))).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var created = await OkAsync(await team.Accountant.PostAsync("/api/app/finance/adjustment", AdjustmentForm(team.Staff.Id, period)), HttpStatusCode.Created);
        Text(await OkAsync(await team.Accountant.GetAsync($"/api/app/finance/adjustment/{IdOf(created)}/attachment-download-url")), "url").ShouldContain("/files/");

        var form = AdjustmentForm(team.Staff.Id, period);
        form.Add(new StringContent(Guid.NewGuid().ToString()), "sourceRequestId");
        await ShouldFailWithAsync(await team.Accountant.PostAsync("/api/app/finance/adjustment", form), HrErrorCodes.Finance.RequestNotAtAccountantStep);

        var report = await team.Accountant.GetFromJsonAsync<JsonElement>($"/api/app/finance/report/adjustments?periodFrom={period}&periodTo={period}&employeeId={team.Staff.Id}&maxResultCount=1");
        report.GetProperty("totalCount").GetInt64().ShouldBe(1);
        Number(report.GetProperty("totals")[0], "additions").ShouldBe(25m);
    }

    [Fact]
    public async Task Nobody_changes_their_own_financial_records()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var accountant = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.Accountant);
        // This call sets the XSRF cookie, so it uses its own client and the writes below stay free of it.
        var reader = TestAuth.ClientFor(_host, accountant.DefaultRequestHeaders.Authorization!.Parameter!, "en");
        var configuration = await reader.GetFromJsonAsync<JsonElement>("/api/abp/application-configuration");
        var ownId = await kit.AddFileForUserAsync(configuration.GetProperty("currentUser").GetProperty("id").GetGuid(), kit.ExecutiveEmployeeId);

        await ShouldFailWithAsync(
            await accountant.PutAsJsonAsync($"/api/app/finance/salary-profile/{ownId}", new { baseSalary = 9999m, currency = "IQD", effectiveFrom = "2000-01-01", components = Array.Empty<object>() }),
            HrErrorCodes.Finance.SelfChangeNotAllowed);
        await ShouldFailWithAsync(await accountant.PostAsync("/api/app/finance/adjustment", AdjustmentForm(ownId, NextPeriod())), HrErrorCodes.Finance.SelfChangeNotAllowed);
    }

    [Fact]
    public async Task Suggestions_never_touch_a_run_until_approved_and_each_is_reviewed_once()
    {
        var team = await TeamAsync();
        await SetSalaryAsync(team.Accountant, team.Staff.Id);
        var period = NextPeriod();
        Guid approveId = Guid.Empty, rejectId = Guid.Empty;

        await RunAsync(async services =>
        {
            var repository = services.GetRequiredService<IRepository<AdjustmentSuggestion, Guid>>();
            var tenantId = services.GetRequiredService<ICurrentTenant>().Id;
            approveId = Guid.NewGuid();
            rejectId = Guid.NewGuid();
            await repository.InsertAsync(new AdjustmentSuggestion(approveId, tenantId, team.Staff.Id, period, AdjustmentType.Deduction, AdjustmentSuggestionBasis.Lateness, 30, AdjustmentSuggestionQuantityUnit.Minutes, "IQD"));
            await repository.InsertAsync(new AdjustmentSuggestion(rejectId, tenantId, team.Staff.Id, period, AdjustmentType.Deduction, AdjustmentSuggestionBasis.Absence, 1, AdjustmentSuggestionQuantityUnit.Days, "IQD"));
        });

        var run = await CreateRunAsync(team.Accountant, period);
        run.GetProperty("pendingSuggestionCount").GetInt32().ShouldBeGreaterThanOrEqualTo(2);
        Number(ItemFor(run, team.Staff.Id), "deductions").ShouldBe(50m);

        var listed = await team.Accountant.GetFromJsonAsync<JsonElement>($"/api/app/finance/adjustment/suggestions?period={period}&employeeId={team.Staff.Id}&basis=lateness");
        listed.GetProperty("totalCount").GetInt64().ShouldBe(1);
        listed.GetProperty("items")[0].GetProperty("suggestedAmount").ValueKind.ShouldBe(JsonValueKind.Null);

        var url = $"/api/app/finance/adjustment/suggestions/{approveId}";
        await ShouldFailWithAsync(await team.Accountant.PostAsJsonAsync(url + "/approve", new { }), HrErrorCodes.Finance.SuggestionAmountRequired);

        var adjustment = await OkAsync(await team.Accountant.PostAsJsonAsync(url + "/approve", new { amount = 15m }));
        Text(adjustment, "source").ShouldBe("attendanceSuggestion");
        adjustment.GetProperty("attachment").ValueKind.ShouldBe(JsonValueKind.Null);
        Text(adjustment, "reason").ShouldNotBeNullOrWhiteSpace();
        await ShouldFailWithAsync(await team.Accountant.PostAsJsonAsync(url + "/approve", new { amount = 15m }), HrErrorCodes.Finance.SuggestionAlreadyReviewed);

        var rejected = await OkAsync(await team.Accountant.PostAsJsonAsync($"/api/app/finance/adjustment/suggestions/{rejectId}/reject", new { reason = "Excused" }));
        Text(rejected, "status").ShouldBe("rejected");
        Text(rejected, "reviewNote").ShouldBe("Excused");

        run = await OkAsync(await team.Accountant.GetAsync($"/api/app/finance/payroll-run/{IdOf(run)}"));
        run.GetProperty("isStale").GetBoolean().ShouldBeTrue();
        run = await OkAsync(await team.Accountant.PostAsJsonAsync($"/api/app/finance/payroll-run/{IdOf(run)}/recalculate", Action(run)));
        Number(ItemFor(run, team.Staff.Id), "deductions").ShouldBe(65m);
    }

    [Fact]
    public async Task Expenses_reverse_with_a_negative_row_and_store_purchases_arrive_on_their_own()
    {
        var team = await TeamAsync();
        var officer = await team.Kit.SignInAsync(await team.Kit.AddAsync(team.Manager.Id, new[] { HrRoles.Employee, HrRoles.StoreOfficer }));

        var part = new ByteArrayContent(Pdf);
        part.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        var form = new MultipartFormDataContent
        {
            { new StringContent("Fuel"), "category" },
            { new StringContent("120.50"), "amount" },
            { new StringContent("USD"), "currency" },
            { new StringContent("2026-01-15"), "date" },
            { part, "attachment", "bill.pdf" }
        };

        var expense = await OkAsync(await team.Accountant.PostAsync("/api/app/finance/expense", form), HttpStatusCode.Created);
        Text(expense, "source").ShouldBe("general");
        Text(await OkAsync(await team.Accountant.GetAsync($"/api/app/finance/expense/{IdOf(expense)}/attachment-download-url")), "url").ShouldContain("/files/");

        var reversal = await OkAsync(await team.Accountant.PostAsJsonAsync($"/api/app/finance/expense/{IdOf(expense)}/reverse", new { reason = "Wrong bill" }));
        Number(reversal, "amount").ShouldBe(-120.50m);
        reversal.GetProperty("reversalOfExpenseId").GetGuid().ShouldBe(IdOf(expense));
        Text(await OkAsync(await team.Accountant.GetAsync($"/api/app/finance/expense/{IdOf(expense)}")), "status").ShouldBe("reversed");
        await ShouldFailWithAsync(await team.Accountant.PostAsJsonAsync($"/api/app/finance/expense/{IdOf(expense)}/reverse", new { reason = "Again" }), HrErrorCodes.Finance.ExpenseAlreadyReversed);

        (await team.Accountant.GetAsync("/api/app/finance/expense/export?format=excel&category=Fuel")).StatusCode.ShouldBe(HttpStatusCode.OK);

        var suffix = Guid.NewGuid().ToString("N")[..10];
        var item = await OkAsync(await officer.PostAsJsonAsync("/api/app/item", await StoreTestKit.ItemBodyAsync(officer, "صنف " + suffix, "Item " + suffix, "consumable", 1)), HttpStatusCode.Created);
        var movement = await OkAsync(await officer.PostAsJsonAsync("/api/app/stock/receive", new { itemId = IdOf(item), quantity = 5, totalCost = 300m }), HttpStatusCode.Created);

        var list = await team.Accountant.GetFromJsonAsync<JsonElement>("/api/app/finance/expense?source=storePurchase&maxResultCount=200&sorting=date desc");
        var purchase = list.GetProperty("items").EnumerateArray().Single(x => x.GetProperty("stockMovementId").ValueKind == JsonValueKind.String && x.GetProperty("stockMovementId").GetGuid() == IdOf(movement));
        Number(purchase, "amount").ShouldBe(300m);

        var today = DateTime.UtcNow;
        var report = await team.Accountant.GetFromJsonAsync<JsonElement>($"/api/app/finance/report/store-expenses?dateFrom={today.AddDays(-2):yyyy-MM-dd}&dateTo={today.AddDays(2):yyyy-MM-dd}&maxResultCount=1");
        report.GetProperty("items").GetArrayLength().ShouldBe(1);
        report.GetProperty("totals").EnumerateArray().Sum(x => Number(x, "amount")).ShouldBeGreaterThanOrEqualTo(300m);
    }

    [Fact]
    public async Task Reports_export_and_every_financial_read_is_logged_without_values()
    {
        var team = await TeamAsync();
        var profile = await SetSalaryAsync(team.Accountant, team.Staff.Id);
        var period = NextPeriod();

        await ShouldFailWithAsync(await team.Accountant.GetAsync($"/api/app/finance/report/monthly-statement?period={period}"), HrErrorCodes.Finance.PayrollRunNotFoundForPeriod);

        var run = await CreateRunAsync(team.Accountant, period);
        var statement = await OkAsync(await team.Accountant.GetAsync($"/api/app/finance/report/monthly-statement?period={period}"));
        statement.GetProperty("runId").GetGuid().ShouldBe(IdOf(run));
        Text(statement, "runStatus").ShouldBe("draft");
        statement.GetProperty("totals").GetArrayLength().ShouldBeGreaterThan(0);

        await ShouldFailWithAsync(await team.Accountant.GetAsync("/api/app/finance/report/monthlyStatement/export?format=excel"), HrErrorCodes.Finance.ReportFilterRequired);
        await ShouldFailWithAsync(await team.Accountant.GetAsync("/api/app/finance/report/adjustments/export?format=excel"), HrErrorCodes.Finance.ReportFilterRequired);
        await ShouldFailWithAsync(await team.Accountant.GetAsync("/api/app/finance/report/storeExpenses/export?format=pdf"), HrErrorCodes.Finance.ReportFilterRequired);
        (await team.Accountant.GetAsync($"/api/app/finance/report/monthlyStatement/export?format=excel&period={period}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await team.Accountant.GetAsync($"/api/app/finance/report/adjustments/export?format=pdf&periodFrom={period}&periodTo={period}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await team.Accountant.GetAsync("/api/app/finance/report/storeExpenses/export?format=excel&dateFrom=2000-01-01&dateTo=2000-01-02")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await team.Accountant.GetAsync("/api/app/finance/report/unknown/export?format=excel")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        (await team.Accountant.GetAsync($"/api/app/finance/salary-profile/{team.Staff.Id}")).EnsureSuccessStatusCode();

        await RunAsync(async services =>
        {
            var log = services.GetRequiredService<IRepository<HrAuditEntry, Guid>>();
            var id = IdOf(profile).ToString();
            var reads = await log.GetListAsync(x => x.EntityId == id && x.Action == AuditActionType.Read);
            reads.ShouldNotBeEmpty();
            reads[0].EntityType.ShouldBe(nameof(SalaryProfile));
            (await log.CountAsync(x => x.EntityId == period || (x.EntityType == "FinanceReport" && x.Action == AuditActionType.Export))).ShouldBeGreaterThan(0);
        });
    }

    [Fact]
    public void Audit_rows_keep_which_field_changed_but_never_the_amount()
    {
        var change = new Volo.Abp.Auditing.EntityChangeInfo { EntityTypeFullName = typeof(SalaryProfile).FullName };
        change.PropertyChanges = new() { new Volo.Abp.Auditing.EntityPropertyChangeInfo { PropertyName = "BaseSalary", OriginalValue = "1000", NewValue = "2000" } };
        var other = new Volo.Abp.Auditing.EntityChangeInfo { EntityTypeFullName = "HR_Mangment2.Departments.Department" };
        other.PropertyChanges = new() { new Volo.Abp.Auditing.EntityPropertyChangeInfo { PropertyName = "NameEn", NewValue = "Sales" } };

        var info = new Volo.Abp.Auditing.AuditLogInfo { Url = "/api/app/finance/salary-profile/1?x=1" };
        info.EntityChanges.Add(change);
        info.EntityChanges.Add(other);
        info.Actions.Add(new Volo.Abp.Auditing.AuditLogActionInfo { Parameters = "{\"baseSalary\":2000}" });

        HrAuditingStore.RemoveFinancialValues(info);

        change.PropertyChanges[0].PropertyName.ShouldBe("BaseSalary");
        change.PropertyChanges[0].NewValue.ShouldBeNull();
        change.PropertyChanges[0].OriginalValue.ShouldBeNull();
        other.PropertyChanges[0].NewValue.ShouldBe("Sales");
        info.Actions[0].Parameters.ShouldBe("{}");
    }

    [Fact]
    public async Task Accountant_request_appears_only_after_it_reaches_the_accountant()
    {
        var team = await TeamAsync();

        var types = await team.StaffClient.GetFromJsonAsync<JsonElement>("/api/app/request-type");
        var type = types.GetProperty("items").EnumerateArray().Single(x => Text(x, "code") == HR_Mangment2.Requests.RequestTypeCodes.Accountant);

        var created = await OkAsync(await team.StaffClient.PostAsJsonAsync("/api/app/request", new { requestTypeId = IdOf(type), description = "Salary advance" }), HttpStatusCode.Created);
        var id = IdOf(created);
        var url = $"/api/app/finance/accountant-request/{id}";

        (await team.Accountant.GetAsync(url)).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        await OkAsync(await team.ManagerClient.PostAsJsonAsync($"/api/app/request/{id}/approve", new { }));
        (await team.Accountant.GetAsync(url)).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        await OkAsync(await team.Kit.Hr.PostAsJsonAsync($"/api/app/request/{id}/approve", new { }));

        var arrived = await OkAsync(await team.Accountant.GetAsync(url));
        Text(arrived, "status").ShouldBe("pendingAccountant");
        Text(arrived, "description").ShouldBe("Salary advance");
        arrived.GetProperty("timeline").GetArrayLength().ShouldBeGreaterThan(0);

        var list = await team.Accountant.GetFromJsonAsync<JsonElement>($"/api/app/finance/accountant-request?employeeId={team.Staff.Id}&status=pendingAccountant");
        list.GetProperty("totalCount").GetInt64().ShouldBe(1);

        await SetSalaryAsync(team.Accountant, team.Staff.Id);
        var form = AdjustmentForm(team.Staff.Id, NextPeriod());
        form.Add(new StringContent(id.ToString()), "sourceRequestId");
        var linked = await OkAsync(await team.Accountant.PostAsync("/api/app/finance/adjustment", form), HttpStatusCode.Created);

        var after = await OkAsync(await team.Accountant.GetAsync(url));
        after.GetProperty("linkedAdjustmentIds")[0].GetGuid().ShouldBe(IdOf(linked));

        await OkAsync(await team.Accountant.PostAsJsonAsync($"/api/app/request/{id}/approve", new { }));
        Text(await OkAsync(await team.Accountant.GetAsync(url)), "status").ShouldBe("completed");
    }
}
