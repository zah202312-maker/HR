using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using HR_Mangment2.Errors;
using HR_Mangment2.Identity;
using HR_Mangment2.Infrastructure;
using Shouldly;
using Xunit;

namespace HR_Mangment2;

/// <summary>Store development, decision 2026-11-11: catalog, purchases, teams, issue vouchers, reports.</summary>
[Collection(HostCollection.Name)]
public class StoreDevelopmentTests
{
    private readonly HostFixture _host;

    public StoreDevelopmentTests(HostFixture host)
    {
        _host = host;
    }

    private record Setup(EmployeeTestKit Kit, HttpClient Officer, CreatedEmployee Manager, CreatedEmployee Staff, HttpClient StaffClient);

    private async Task<Setup> SetupAsync()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var officer = await kit.AddAsync(kit.ExecutiveEmployeeId, new[] { HrRoles.Employee, HrRoles.StoreOfficer });
        var staff = await kit.AddStaffAsync(manager.Id);
        return new Setup(kit, await kit.SignInAsync(officer), manager, staff, await kit.SignInAsync(staff));
    }

    private static async Task<JsonElement> OkAsync(HttpResponseMessage response, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe(expected, body);
        return body.Length == 0 ? default : JsonDocument.Parse(body).RootElement;
    }

    private static async Task FailsWithAsync(HttpResponseMessage response, string code)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe((HttpStatusCode)422, body);
        JsonDocument.Parse(body).RootElement.GetProperty("error").GetProperty("code").GetString().ShouldBe(code);
    }

    private static Guid IdOf(JsonElement e) => e.GetProperty("id").GetGuid();

    private static decimal Num(JsonElement e, string p) => e.GetProperty(p).GetDecimal();

    private static string Text(JsonElement e, string p) => e.GetProperty(p).GetString()!;

    private static string Unique() => Guid.NewGuid().ToString("N")[..10];

    private static async Task<JsonElement> ItemAsync(HttpClient officer, string kind = "consumable", decimal min = 2)
    {
        var s = Unique();
        return await OkAsync(await officer.PostAsJsonAsync("/api/app/item", await StoreTestKit.ItemBodyAsync(officer, "مادة " + s, "Item " + s, kind, min)), HttpStatusCode.Created);
    }

    private static async Task<JsonElement> ItemNowAsync(HttpClient officer, Guid id) => await officer.GetFromJsonAsync<JsonElement>($"/api/app/item/{id}");

    private static async Task<JsonElement> PurchaseAsync(HttpClient officer, params (Guid Item, decimal Qty, decimal Price)[] lines)
    {
        var draft = await OkAsync(await officer.PostAsJsonAsync("/api/app/store/purchase", new
        {
            receiptNumber = "R-" + Unique(),
            supplierName = "Supplier",
            purchaseDate = DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-dd"),
            receivedDate = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd"),
            lines = lines.Select(x => new { itemId = x.Item, quantity = x.Qty, unitPrice = x.Price })
        }), HttpStatusCode.Created);
        return await OkAsync(await officer.PostAsync($"/api/app/store/purchase/{IdOf(draft)}/approve", null));
    }

    private static async Task<JsonElement> TeamAsync(HttpClient officer, Guid? departmentId) =>
        await OkAsync(await officer.PostAsJsonAsync("/api/app/store/team", new { name = "Team " + Unique(), departmentId }), HttpStatusCode.Created);

    private static async Task<JsonElement> VoucherDraftAsync(HttpClient officer, Guid teamId, Guid? employeeId, params (Guid Item, decimal Qty)[] lines) =>
        await OkAsync(await officer.PostAsJsonAsync("/api/app/store/issue-voucher", new
        {
            voucherNumber = "V-" + Unique(),
            issueDate = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd"),
            teamId,
            employeeId,
            lines = lines.Select(x => new { itemId = x.Item, quantity = x.Qty })
        }), HttpStatusCode.Created);

    [Fact]
    public async Task Items_carry_sku_category_unit_and_delete_only_without_history()
    {
        var s = await SetupAsync();
        var item = await ItemAsync(s.Officer);
        Text(item, "sku").ShouldStartWith("SKU-");
        item.GetProperty("category").GetProperty("name").GetString().ShouldNotBeNullOrEmpty();
        Text(item, "stockStatus").ShouldBe("outOfStock");
        Num(item, "averageUnitCost").ShouldBe(0);

        // SKU unique.
        var body = await StoreTestKit.ItemBodyAsync(s.Officer, "x" + Unique(), "x" + Unique(), "consumable", 0);
        var json = JsonSerializer.Serialize(body).Replace((string)body.GetType().GetProperty("sku")!.GetValue(body)!, Text(item, "sku"));
        await FailsWithAsync(await s.Officer.PostAsync("/api/app/item", new StringContent(json, System.Text.Encoding.UTF8, "application/json")), HrErrorCodes.Item.SkuAlreadyExists);

        // An inactive category is refused.
        var category = item.GetProperty("category");
        var categoryRow = (await s.Officer.GetFromJsonAsync<JsonElement>($"/api/app/store/item-category?filter={Uri.EscapeDataString(Text(category, "name"))}")).GetProperty("items")[0];
        await OkAsync(await s.Officer.PutAsJsonAsync($"/api/app/store/item-category/{IdOf(categoryRow)}", new { nameAr = Text(categoryRow, "nameAr"), nameEn = Text(categoryRow, "nameEn"), isActive = false, concurrencyStamp = Text(categoryRow, "concurrencyStamp") }));
        await FailsWithAsync(await s.Officer.PostAsJsonAsync("/api/app/item", new { nameAr = "y" + Unique(), nameEn = "y" + Unique(), sku = "Y" + Unique(), categoryId = IdOf(categoryRow), unitId = item.GetProperty("unit").GetProperty("id").GetGuid(), kind = "consumable", minQuantity = 0 }), HrErrorCodes.Item.CategoryInactive);
        await FailsWithAsync(await s.Officer.PostAsJsonAsync("/api/app/store/item-category", new { nameAr = Text(categoryRow, "nameAr"), nameEn = "other" + Unique() }), HrErrorCodes.ItemCategory.NameAlreadyExists);

        // No history: deleted. With history: refused.
        var fresh = await ItemAsync(s.Officer);
        await OkAsync(await s.Officer.DeleteAsync($"/api/app/item/{IdOf(fresh)}"), HttpStatusCode.NoContent);
        (await s.Officer.GetAsync($"/api/app/item/{IdOf(fresh)}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        await PurchaseAsync(s.Officer, (IdOf(item), 1, 10));
        await FailsWithAsync(await s.Officer.DeleteAsync($"/api/app/item/{IdOf(item)}"), HrErrorCodes.Item.HasMovements);

        (await s.StaffClient.GetAsync("/api/app/store/item-category")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task A_purchase_is_a_draft_until_approved_then_moves_stock_average_cost_and_expenses()
    {
        var s = await SetupAsync();
        var a = IdOf(await ItemAsync(s.Officer));
        var b = IdOf(await ItemAsync(s.Officer));
        var receipt = "R-" + Unique();
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var draft = await OkAsync(await s.Officer.PostAsJsonAsync("/api/app/store/purchase", new
        {
            receiptNumber = receipt, supplierName = "Al-Noor", purchaseDate = today, receivedDate = today, note = "Q4",
            lines = new[] { new { itemId = a, quantity = 4m, unitPrice = 2.5m }, new { itemId = b, quantity = 1m, unitPrice = 100m } }
        }), HttpStatusCode.Created);
        Text(draft, "status").ShouldBe("draft");
        Num(draft, "totalQuantity").ShouldBe(5);
        Num(draft, "totalAmount").ShouldBe(110);
        draft.GetProperty("lines")[0].GetProperty("lineTotal").GetDecimal().ShouldBe(10);
        Num(await ItemNowAsync(s.Officer, a), "quantityOnHand").ShouldBe(0);

        await FailsWithAsync(await s.Officer.PostAsJsonAsync("/api/app/store/purchase", new { receiptNumber = receipt, supplierName = "X", purchaseDate = today, receivedDate = today, lines = new[] { new { itemId = a, quantity = 1m, unitPrice = 1m } } }), HrErrorCodes.StorePurchase.ReceiptNumberAlreadyExists);
        await FailsWithAsync(await s.Officer.PostAsJsonAsync("/api/app/store/purchase", new { receiptNumber = "R-" + Unique(), supplierName = "X", purchaseDate = today, receivedDate = DateTime.UtcNow.AddDays(-3).ToString("yyyy-MM-dd"), lines = new[] { new { itemId = a, quantity = 1m, unitPrice = 1m } } }), HrErrorCodes.StorePurchase.InvalidDates);
        await FailsWithAsync(await s.Officer.PostAsJsonAsync("/api/app/store/purchase", new { receiptNumber = "R-" + Unique(), supplierName = "X", purchaseDate = today, receivedDate = today, lines = new[] { new { itemId = a, quantity = 1m, unitPrice = 1m }, new { itemId = a, quantity = 2m, unitPrice = 1m } } }), HrErrorCodes.StorePurchase.DuplicateItem);
        (await s.Officer.PostAsJsonAsync("/api/app/store/purchase", new { receiptNumber = "R-" + Unique(), supplierName = "X", purchaseDate = today, receivedDate = today, lines = new[] { new { itemId = a, quantity = -1m, unitPrice = 1m } } })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // Update replaces the lines.
        var updated = await OkAsync(await s.Officer.PutAsJsonAsync($"/api/app/store/purchase/{IdOf(draft)}", new
        {
            receiptNumber = receipt, supplierName = "Al-Noor", purchaseDate = today, receivedDate = today, note = "Q4",
            lines = new[] { new { itemId = a, quantity = 4m, unitPrice = 2.5m } },
            concurrencyStamp = Text(draft, "concurrencyStamp")
        }));
        updated.GetProperty("lines").GetArrayLength().ShouldBe(1);

        var approved = await OkAsync(await s.Officer.PostAsync($"/api/app/store/purchase/{IdOf(draft)}/approve", null));
        Text(approved, "status").ShouldBe("approved");
        await FailsWithAsync(await s.Officer.PostAsync($"/api/app/store/purchase/{IdOf(draft)}/approve", null), HrErrorCodes.StorePurchase.NotDraft);
        await FailsWithAsync(await s.Officer.DeleteAsync($"/api/app/store/purchase/{IdOf(draft)}"), HrErrorCodes.StorePurchase.NotDraft);

        var item = await ItemNowAsync(s.Officer, a);
        Num(item, "quantityOnHand").ShouldBe(4);
        Num(item, "averageUnitCost").ShouldBe(2.5m);

        // A second purchase at another price moves the weighted average: (4 × 2.5 + 4 × 5) / 8.
        await PurchaseAsync(s.Officer, (a, 4, 5));
        item = await ItemNowAsync(s.Officer, a);
        Num(item, "averageUnitCost").ShouldBe(3.75m);
        Num(item, "stockValue").ShouldBe(30);

        var movements = await s.Officer.GetFromJsonAsync<JsonElement>($"/api/app/stock/movements?itemId={a}&type=purchase&filter={receipt}");
        var movement = movements.GetProperty("items")[0];
        Num(movement, "unitCost").ShouldBe(2.5m);
        Num(movement, "totalCost").ShouldBe(10);
        Text(movement, "documentNumber").ShouldBe(receipt);

        // The accountant gets an expense per line, linked to its movement.
        var accountant = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.Accountant);
        var expenses = await accountant.GetFromJsonAsync<JsonElement>("/api/app/finance/expense?source=storePurchase&maxResultCount=200&sorting=date desc");
        expenses.GetProperty("items").EnumerateArray().Any(x => x.GetProperty("stockMovementId").ValueKind == JsonValueKind.String && x.GetProperty("stockMovementId").GetGuid() == IdOf(movement) && x.GetProperty("amount").GetDecimal() == 10).ShouldBeTrue();

        // Reversal takes it back out at its price, and the average returns to the second purchase.
        var reversed = await OkAsync(await s.Officer.PostAsJsonAsync($"/api/app/store/purchase/{IdOf(draft)}/reverse", new { reason = "Entered twice" }));
        Text(reversed, "status").ShouldBe("reversed");
        item = await ItemNowAsync(s.Officer, a);
        Num(item, "quantityOnHand").ShouldBe(4);
        Num(item, "averageUnitCost").ShouldBe(5);
        await FailsWithAsync(await s.Officer.PostAsJsonAsync($"/api/app/store/purchase/{IdOf(draft)}/reverse", new { reason = "Again" }), HrErrorCodes.StorePurchase.NotApproved);

        // A purchase line is never reversed on its own.
        (await s.Officer.PostAsJsonAsync("/api/app/stock/adjust", new { itemId = a, quantity = -4, reason = "x", reversesMovementId = IdOf(movement) })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var list = await s.Officer.GetFromJsonAsync<JsonElement>($"/api/app/store/purchase?filter={receipt}");
        list.GetProperty("totalCount").GetInt64().ShouldBe(1);
        (await s.StaffClient.GetAsync("/api/app/store/purchase")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task A_purchase_takes_one_attachment_that_replaces_the_previous()
    {
        var s = await SetupAsync();
        var draft = await OkAsync(await s.Officer.PostAsJsonAsync("/api/app/store/purchase", new
        {
            receiptNumber = "R-" + Unique(), supplierName = "X", purchaseDate = DateTime.UtcNow.ToString("yyyy-MM-dd"), receivedDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            lines = new[] { new { itemId = IdOf(await ItemAsync(s.Officer)), quantity = 1m, unitPrice = 1m } }
        }), HttpStatusCode.Created);
        var url = $"/api/app/store/purchase/{IdOf(draft)}";
        (await s.Officer.GetAsync(url + "/attachment-download-url")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        async Task<HttpResponseMessage> UploadAsync(string name)
        {
            var form = new MultipartFormDataContent();
            var file = new ByteArrayContent(System.Text.Encoding.ASCII.GetBytes("%PDF-1.4 test"));
            file.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            form.Add(file, "file", name);
            return await s.Officer.PostAsync(url + "/attachment", form);
        }

        Text(await OkAsync(await UploadAsync("a.pdf")), "fileName").ShouldBe("a.pdf");
        Text(await OkAsync(await UploadAsync("b.pdf")), "fileName").ShouldBe("b.pdf");
        Text((await s.Officer.GetFromJsonAsync<JsonElement>(url)).GetProperty("attachment"), "fileName").ShouldBe("b.pdf");
        (await OkAsync(await s.Officer.GetAsync(url + "/attachment-download-url"))).GetProperty("url").GetString().ShouldNotBeNullOrEmpty();
        await OkAsync(await s.Officer.DeleteAsync(url + "/attachment"), HttpStatusCode.NoContent);
        (await s.Officer.GetFromJsonAsync<JsonElement>(url)).GetProperty("attachment").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task An_issue_voucher_deducts_on_approval_makes_custody_and_reverses()
    {
        var s = await SetupAsync();
        var consumable = IdOf(await ItemAsync(s.Officer));
        var returnable = IdOf(await ItemAsync(s.Officer, "returnable"));
        await PurchaseAsync(s.Officer, (consumable, 10, 2), (returnable, 3, 100));

        var noDepartment = await TeamAsync(s.Officer, null);
        var withDepartment = await TeamAsync(s.Officer, s.Kit.DepartmentId);
        await FailsWithAsync(await s.Officer.PostAsJsonAsync("/api/app/store/team", new { name = Text(noDepartment, "name") }), HrErrorCodes.StoreTeam.NameAlreadyExists);

        // More than on hand: refused at approval, nothing moves.
        var tooMuch = await VoucherDraftAsync(s.Officer, IdOf(withDepartment), null, (consumable, 11));
        await FailsWithAsync(await s.Officer.PostAsync($"/api/app/store/issue-voucher/{IdOf(tooMuch)}/approve", null), HrErrorCodes.Stock.InsufficientStock);
        Num(await ItemNowAsync(s.Officer, consumable), "quantityOnHand").ShouldBe(10);

        // A returnable item to a team without a department and without an employee is refused.
        var orphan = await VoucherDraftAsync(s.Officer, IdOf(noDepartment), null, (returnable, 1));
        await FailsWithAsync(await s.Officer.PostAsync($"/api/app/store/issue-voucher/{IdOf(orphan)}/approve", null), HrErrorCodes.StoreIssueVoucher.TeamHasNoDepartment);

        // To the team's department: shared custody. Value at the average cost.
        var shared = await VoucherDraftAsync(s.Officer, IdOf(withDepartment), null, (consumable, 4), (returnable, 1));
        Text(shared, "status").ShouldBe("draft");
        shared.GetProperty("totalValue").ValueKind.ShouldBe(JsonValueKind.Null);
        var approved = await OkAsync(await s.Officer.PostAsync($"/api/app/store/issue-voucher/{IdOf(shared)}/approve", null));
        Num(approved, "totalValue").ShouldBe(108);
        var custodyLine = approved.GetProperty("lines").EnumerateArray().Single(x => x.GetProperty("item").GetProperty("id").GetGuid() == returnable);
        var custodyId = custodyLine.GetProperty("custodyId").GetGuid();
        var custody = await s.Officer.GetFromJsonAsync<JsonElement>($"/api/app/custody/{custodyId}");
        Text(custody, "kind").ShouldBe("shared");
        Num(await ItemNowAsync(s.Officer, consumable), "quantityOnHand").ShouldBe(6);

        // To a named employee: personal custody.
        var personal = await VoucherDraftAsync(s.Officer, IdOf(noDepartment), s.Staff.Id, (returnable, 1));
        var personalApproved = await OkAsync(await s.Officer.PostAsync($"/api/app/store/issue-voucher/{IdOf(personal)}/approve", null));
        var personalCustody = await s.Officer.GetFromJsonAsync<JsonElement>($"/api/app/custody/{personalApproved.GetProperty("lines")[0].GetProperty("custodyId").GetGuid()}");
        Text(personalCustody, "kind").ShouldBe("personal");

        // The team's record of what it received.
        var teamMovements = await s.Officer.GetFromJsonAsync<JsonElement>($"/api/app/stock/movements?teamId={IdOf(withDepartment)}");
        teamMovements.GetProperty("totalCount").GetInt64().ShouldBe(2);

        // Reversal returns the stock and closes the custody.
        var reversed = await OkAsync(await s.Officer.PostAsJsonAsync($"/api/app/store/issue-voucher/{IdOf(shared)}/reverse", new { reason = "Wrong team" }));
        Text(reversed, "status").ShouldBe("reversed");
        Num(await ItemNowAsync(s.Officer, consumable), "quantityOnHand").ShouldBe(10);
        Text(await s.Officer.GetFromJsonAsync<JsonElement>($"/api/app/custody/{custodyId}"), "status").ShouldBe("returned");

        // Once its custody is back, a voucher cannot be reversed.
        var personalCustodyId = personalApproved.GetProperty("lines")[0].GetProperty("custodyId").GetGuid();
        await OkAsync(await s.Officer.PostAsJsonAsync($"/api/app/custody/{personalCustodyId}/return", new { restock = true }));
        await FailsWithAsync(await s.Officer.PostAsJsonAsync($"/api/app/store/issue-voucher/{IdOf(personal)}/reverse", new { reason = "x" }), HrErrorCodes.StoreIssueVoucher.CustodyAlreadyReturned);

        (await s.StaffClient.GetAsync("/api/app/store/issue-voucher")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Dashboard_reports_and_export_follow_the_approved_documents()
    {
        var s = await SetupAsync();
        var item = IdOf(await ItemAsync(s.Officer, "consumable", 5));
        await PurchaseAsync(s.Officer, (item, 6, 10));
        var team = await TeamAsync(s.Officer, s.Kit.DepartmentId);
        var voucher = await VoucherDraftAsync(s.Officer, IdOf(team), null, (item, 2));
        await OkAsync(await s.Officer.PostAsync($"/api/app/store/issue-voucher/{IdOf(voucher)}/approve", null));

        var dashboard = await OkAsync(await s.Officer.GetAsync("/api/app/store/dashboard"));
        dashboard.GetProperty("purchasesCount").GetInt32().ShouldBeGreaterThanOrEqualTo(1);
        dashboard.GetProperty("lowStockCount").GetInt32().ShouldBeGreaterThanOrEqualTo(1);
        Text(dashboard, "currency").ShouldNotBeNullOrEmpty();

        var from = DateTime.UtcNow.AddDays(-5).ToString("yyyy-MM-dd");
        var to = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var purchases = await OkAsync(await s.Officer.GetAsync($"/api/app/store/report/purchases?from={from}&to={to}&itemId={item}"));
        Num(purchases, "totalAmount").ShouldBe(60);

        var low = await OkAsync(await s.Officer.GetAsync("/api/app/store/report/stock?stockStatus=lowStock&maxResultCount=200"));
        low.GetProperty("items").EnumerateArray().Any(x => x.GetProperty("item").GetProperty("id").GetGuid() == item).ShouldBeTrue();

        var byTeam = await OkAsync(await s.Officer.GetAsync($"/api/app/store/report/issues-by-team?from={from}&to={to}&teamId={IdOf(team)}"));
        Num(byTeam, "totalQuantity").ShouldBe(2);
        Num(byTeam, "totalValue").ShouldBe(20);

        var movement = await OkAsync(await s.Officer.GetAsync($"/api/app/store/report/item-movement?itemId={item}&from={from}&to={to}"));
        Num(movement, "openingBalance").ShouldBe(0);
        Num(movement, "totalIn").ShouldBe(6);
        Num(movement, "totalOut").ShouldBe(2);
        Num(movement, "closingBalance").ShouldBe(4);

        await FailsWithAsync(await s.Officer.GetAsync($"/api/app/store/report/purchases?from={to}&to={from}"), HrErrorCodes.Reports.InvalidPeriod);
        (await s.Officer.GetAsync("/api/app/store/report/purchases")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var excel = await s.Officer.GetAsync($"/api/app/store/report/issuesByTeam/export?format=excel&from={from}&to={to}");
        excel.StatusCode.ShouldBe(HttpStatusCode.OK);
        excel.Content.Headers.ContentType!.MediaType.ShouldBe("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        (await s.Officer.GetAsync("/api/app/store/report/lowStock/export?format=pdf")).StatusCode.ShouldBe(HttpStatusCode.OK);
        await FailsWithAsync(await s.Officer.GetAsync("/api/app/store/report/itemMovement/export?format=pdf"), HrErrorCodes.Reports.MissingFilter);

        (await s.StaffClient.GetAsync("/api/app/store/dashboard")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await s.StaffClient.GetAsync($"/api/app/store/report/stock")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
