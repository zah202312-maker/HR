using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using HR_Mangment2.Identity;
using HR_Mangment2.Infrastructure;
using Shouldly;
using Xunit;

namespace HR_Mangment2;

/// <summary>
/// The executive director's read-only pages (decision 2026-11-11): every employee with the base salary only, and with the
/// current contract's details only.
/// </summary>
[Collection(HostCollection.Name)]
public class ExecutiveOverviewTests
{
    private const string Salaries = "/api/app/finance/salary-profile/overview";
    private const string Contracts = "/api/app/contract/overview";

    private readonly HostFixture _host;

    public ExecutiveOverviewTests(HostFixture host)
    {
        _host = host;
    }

    private static async Task<JsonElement> OkAsync(HttpResponseMessage response, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe(expected, body);
        return JsonDocument.Parse(body).RootElement;
    }

    private static string Day(int offset) => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(offset).ToString("yyyy-MM-dd");

    private static string[] PropertyNames(JsonElement element) => element.EnumerateObject().Select(x => x.Name).ToArray();

    private static JsonElement Row(JsonElement page, Guid employeeId) =>
        page.GetProperty("items").EnumerateArray().Single(x => x.GetProperty("employee").GetProperty("id").GetGuid() == employeeId);

    [Fact]
    public async Task The_executive_sees_each_employee_with_base_salary_and_current_contract_only()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var paid = await kit.AddStaffAsync(manager.Id);
        var unpaid = await kit.AddStaffAsync(manager.Id);
        var managerClient = await kit.SignInAsync(manager);
        var accountant = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.Accountant);
        var executive = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.ExecutiveDirector);

        await OkAsync(await accountant.PutAsJsonAsync($"/api/app/finance/salary-profile/{paid.Id}", new
        {
            baseSalary = 1500m,
            currency = "IQD",
            effectiveFrom = "2000-01-01",
            components = new object[] { new { name = "Transport", type = "allowance", amount = 100m } }
        }));

        var older = new MultipartFormDataContent
        {
            { new StringContent(paid.Id.ToString()), "employeeId" },
            { new StringContent("Probation"), "contractType" },
            { new StringContent(Day(-400)), "startDate" },
            { new StringContent(Day(-40)), "endDate" }
        };
        await OkAsync(await kit.Hr.PostAsync("/api/app/contract", older), HttpStatusCode.Created);
        var current = new MultipartFormDataContent
        {
            { new StringContent(paid.Id.ToString()), "employeeId" },
            { new StringContent("Annual"), "contractType" },
            { new StringContent(Day(-30)), "startDate" },
            { new StringContent(Day(335)), "endDate" }
        };
        await OkAsync(await kit.Hr.PostAsync("/api/app/contract", current), HttpStatusCode.Created);

        // Salaries: the base salary only, nulls when there is no profile.
        var name = (await OkAsync(await kit.Hr.GetAsync($"/api/app/employee/{paid.Id}"))).GetProperty("fullNameAr").GetString()!;
        var salaries = await OkAsync(await executive.GetAsync($"{Salaries}?search={Uri.EscapeDataString(name)}"));
        var salary = Row(salaries, paid.Id);
        PropertyNames(salary).ShouldBe(new[] { "employee", "baseSalary", "currency", "effectiveFrom" }, ignoreOrder: true);
        salary.GetProperty("baseSalary").GetDecimal().ShouldBe(1500m);
        salary.GetProperty("currency").GetString().ShouldBe("IQD");
        var unpaidName = (await OkAsync(await kit.Hr.GetAsync($"/api/app/employee/{unpaid.Id}"))).GetProperty("fullNameAr").GetString()!;
        var none = Row(await OkAsync(await executive.GetAsync($"{Salaries}?search={Uri.EscapeDataString(unpaidName)}")), unpaid.Id);
        none.GetProperty("baseSalary").ValueKind.ShouldBe(JsonValueKind.Null);

        // Contracts: the latest contract's details only, nulls when there is none.
        var contract = Row(await OkAsync(await executive.GetAsync($"{Contracts}?search={Uri.EscapeDataString(name)}")), paid.Id);
        PropertyNames(contract).ShouldBe(new[] { "employee", "contractType", "startDate", "endDate", "status" }, ignoreOrder: true);
        contract.GetProperty("contractType").GetString().ShouldBe("Annual");
        contract.GetProperty("startDate").GetString().ShouldBe(Day(-30));
        Row(await OkAsync(await executive.GetAsync($"{Contracts}?search={Uri.EscapeDataString(unpaidName)}")), unpaid.Id)
            .GetProperty("contractType").ValueKind.ShouldBe(JsonValueKind.Null);

        // Only the executive director; the others keep their own pages.
        foreach (var other in new[] { kit.Hr, accountant, managerClient })
        {
            (await other.GetAsync(Salaries)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
            (await other.GetAsync(Contracts)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        }

        // Today's attendance counts: executive only, and the same sums as the HR attendance report for today.
        var summary = await OkAsync(await executive.GetAsync("/api/app/report/today-attendance-summary"));
        PropertyNames(summary).ShouldBe(new[] { "date", "present", "late", "absent", "onLeave" }, ignoreOrder: true);
        var today = summary.GetProperty("date").GetString()!;
        int present = 0, late = 0, absent = 0;
        decimal onLeave = 0;
        for (var skip = 0; ; skip += 200)
        {
            var page = await OkAsync(await kit.Hr.GetAsync($"/api/app/report/hr-attendance?from={today}&to={today}&skipCount={skip}&maxResultCount=200"));
            foreach (var r in page.GetProperty("items").EnumerateArray())
            {
                present += r.GetProperty("presentDays").GetInt32();
                late += r.GetProperty("lateCount").GetInt32();
                absent += r.GetProperty("absentDays").GetInt32();
                onLeave += r.GetProperty("leaveDays").GetDecimal();
            }

            if (skip + 200 >= page.GetProperty("totalCount").GetInt64()) break;
        }

        summary.GetProperty("present").GetInt32().ShouldBe(present);
        summary.GetProperty("late").GetInt32().ShouldBe(late);
        summary.GetProperty("absent").GetInt32().ShouldBe(absent);
        summary.GetProperty("onLeave").GetDecimal().ShouldBe(onLeave);
        (await kit.Hr.GetAsync("/api/app/report/today-attendance-summary")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await managerClient.GetAsync("/api/app/report/today-attendance-summary")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // Still read only: the executive cannot reach the full profile, the contract or its attachments.
        (await executive.GetAsync($"/api/app/finance/salary-profile/{paid.Id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await executive.GetAsync("/api/app/contract")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
