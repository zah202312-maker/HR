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

/// <summary>Petty cash (decision 2026-11-11): types, a fixed monthly amount per employee, monthly delivery with an expense.</summary>
[Collection(HostCollection.Name)]
public class PettyCashTests
{
    private readonly HostFixture _host;

    public PettyCashTests(HostFixture host)
    {
        _host = host;
    }

    private static async Task<JsonElement> OkAsync(HttpResponseMessage response, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe(expected, body);
        return JsonDocument.Parse(body).RootElement;
    }

    private static async Task FailsWithAsync(HttpResponseMessage response, string code)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe((HttpStatusCode)422, body);
        JsonDocument.Parse(body).RootElement.GetProperty("error").GetProperty("code").GetString().ShouldBe(code);
    }

    private static Guid IdOf(JsonElement e) => e.GetProperty("id").GetGuid();

    private static string Month(int offset) => DateTime.UtcNow.AddMonths(offset).ToString("yyyy-MM");

    [Fact]
    public async Task Monthly_petty_cash_is_listed_changed_delivered_and_undone()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var other = await kit.AddStaffAsync(manager.Id);
        var accountant = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.Accountant);
        var staffClient = await kit.SignInAsync(staff);
        var s = Guid.NewGuid().ToString("N")[..8];

        var type = await OkAsync(await accountant.PostAsJsonAsync("/api/app/finance/petty-cash-type", new { nameAr = "طعام " + s, nameEn = "Food " + s }), HttpStatusCode.Created);
        await FailsWithAsync(await accountant.PostAsJsonAsync("/api/app/finance/petty-cash-type", new { nameAr = "طعام " + s, nameEn = "Other " + s }), HrErrorCodes.Finance.PettyCashTypeNameAlreadyExists);

        // From two months ago, open-ended.
        var assignment = await OkAsync(await accountant.PostAsJsonAsync("/api/app/finance/petty-cash-assignment", new { employeeId = staff.Id, typeId = IdOf(type), amount = 50000m, fromPeriod = Month(-2) }), HttpStatusCode.Created);
        await FailsWithAsync(
            await accountant.PostAsJsonAsync("/api/app/finance/petty-cash-assignment", new { employeeId = staff.Id, typeId = IdOf(type), amount = 1m, fromPeriod = Month(0) }),
            HrErrorCodes.Finance.PettyCashAssignmentOverlap);
        await FailsWithAsync(
            await accountant.PostAsJsonAsync("/api/app/finance/petty-cash-assignment", new { employeeId = other.Id, typeId = IdOf(type), amount = 1m, fromPeriod = Month(0), toPeriod = Month(-1) }),
            HrErrorCodes.Finance.InvalidPeriodRange);

        // The month shows a pending line for it; a future month is refused.
        var month = await OkAsync(await accountant.GetAsync($"/api/app/finance/petty-cash/month?period={Month(0)}&employeeId={staff.Id}"));
        var entry = month.GetProperty("items").EnumerateArray().Single(x => x.GetProperty("assignmentId").GetGuid() == IdOf(assignment));
        entry.GetProperty("status").GetString().ShouldBe("pending");
        entry.GetProperty("amount").GetDecimal().ShouldBe(50000m);
        await FailsWithAsync(await accountant.GetAsync($"/api/app/finance/petty-cash/month?period={Month(1)}"), HrErrorCodes.Finance.FuturePeriod);

        // A new amount reaches the pending month.
        await OkAsync(await accountant.PutAsJsonAsync($"/api/app/finance/petty-cash-assignment/{IdOf(assignment)}", new { amount = 60000m, concurrencyStamp = assignment.GetProperty("concurrencyStamp").GetString() }));
        month = await OkAsync(await accountant.GetAsync($"/api/app/finance/petty-cash/month?period={Month(0)}&employeeId={staff.Id}"));
        entry = month.GetProperty("items")[0];
        entry.GetProperty("amount").GetDecimal().ShouldBe(60000m);

        // One month's amount can change on its own.
        entry = await OkAsync(await accountant.PutAsJsonAsync($"/api/app/finance/petty-cash/entry/{IdOf(entry)}", new { amount = 55000m, concurrencyStamp = entry.GetProperty("concurrencyStamp").GetString() }));
        entry.GetProperty("amount").GetDecimal().ShouldBe(55000m);

        // Delivery creates an expense for the accountant.
        await FailsWithAsync(
            await accountant.PostAsJsonAsync("/api/app/finance/petty-cash/deliver", new { entryIds = new[] { IdOf(entry) }, deliveredOn = DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd") }),
            HrErrorCodes.Finance.DeliveryDateInFuture);
        var delivered = await OkAsync(await accountant.PostAsJsonAsync("/api/app/finance/petty-cash/deliver", new { entryIds = new[] { IdOf(entry) }, deliveredOn = DateTime.UtcNow.ToString("yyyy-MM-dd") }));
        var done = delivered.GetProperty("items")[0];
        done.GetProperty("status").GetString().ShouldBe("delivered");
        done.GetProperty("deliveredBy").ValueKind.ShouldBe(JsonValueKind.Object);
        var expenseId = done.GetProperty("expenseId").GetGuid();
        var expense = await OkAsync(await accountant.GetAsync($"/api/app/finance/expense/{expenseId}"));
        expense.GetProperty("amount").GetDecimal().ShouldBe(55000m);
        expense.GetProperty("source").GetString().ShouldBe("pettyCash");
        await FailsWithAsync(await accountant.PostAsJsonAsync("/api/app/finance/petty-cash/deliver", new { entryIds = new[] { IdOf(entry) }, deliveredOn = DateTime.UtcNow.ToString("yyyy-MM-dd") }), HrErrorCodes.Finance.PettyCashEntryNotPending);

        month = await OkAsync(await accountant.GetAsync($"/api/app/finance/petty-cash/month?period={Month(0)}&employeeId={staff.Id}"));
        month.GetProperty("deliveredAmount").GetDecimal().ShouldBe(55000m);

        // Stopping cannot cut off a delivered month.
        var current = (await OkAsync(await accountant.GetAsync($"/api/app/finance/petty-cash-assignment?employeeId={staff.Id}"))).GetProperty("items")[0];
        await FailsWithAsync(
            await accountant.PutAsJsonAsync($"/api/app/finance/petty-cash-assignment/{IdOf(assignment)}", new { amount = 60000m, toPeriod = Month(-1), concurrencyStamp = current.GetProperty("concurrencyStamp").GetString() }),
            HrErrorCodes.Finance.InvalidPeriodRange);

        // Undo reverses the expense and makes the month pending again.
        var undone = await OkAsync(await accountant.PostAsJsonAsync($"/api/app/finance/petty-cash/entry/{IdOf(entry)}/undo-delivery", new { reason = "Not handed over" }));
        undone.GetProperty("status").GetString().ShouldBe("pending");
        (await OkAsync(await accountant.GetAsync($"/api/app/finance/expense/{expenseId}"))).GetProperty("status").GetString().ShouldBe("reversed");
        await FailsWithAsync(await accountant.PostAsJsonAsync($"/api/app/finance/petty-cash/entry/{IdOf(entry)}/undo-delivery", new { reason = "Again" }), HrErrorCodes.Finance.PettyCashEntryNotDelivered);

        // Only the accountant.
        (await staffClient.GetAsync($"/api/app/finance/petty-cash/month?period={Month(0)}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await kit.Hr.GetAsync("/api/app/finance/petty-cash-type")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
