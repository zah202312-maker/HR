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
using HR_Mangment2.Requests;
using Shouldly;
using Xunit;

namespace HR_Mangment2;

[Collection(HostCollection.Name)]
public class StoreModuleTests
{
    private readonly HostFixture _host;

    public StoreModuleTests(HostFixture host)
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

    private record Team(EmployeeTestKit Kit, HttpClient Officer, CreatedEmployee Manager, HttpClient ManagerClient, CreatedEmployee Staff, HttpClient StaffClient);

    private async Task<Team> TeamAsync()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var officer = await kit.AddAsync(manager.Id, new[] { HrRoles.Employee, HrRoles.StoreOfficer });
        var staff = await kit.AddStaffAsync(manager.Id);

        return new Team(kit, await kit.SignInAsync(officer), manager, await kit.SignInAsync(manager), staff, await kit.SignInAsync(staff));
    }

    private static async Task<JsonElement> AddItemAsync(HttpClient officer, string kind = "returnable", decimal min = 1, decimal stock = 0)
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var item = await OkAsync(
            await officer.PostAsJsonAsync("/api/app/item", new { nameAr = "صنف " + suffix, nameEn = "Item " + suffix, kind, minQuantity = min }),
            HttpStatusCode.Created);

        if (stock > 0)
        {
            await OkAsync(await officer.PostAsJsonAsync("/api/app/stock/receive", new { itemId = IdOf(item), quantity = stock }), HttpStatusCode.Created);
        }

        return await officer.GetFromJsonAsync<JsonElement>($"/api/app/item/{IdOf(item)}");
    }

    /// <summary>The store type stays off for the other tests in this database, which assert it starts inactive.</summary>
    private async Task WithStoreTypeActiveAsync(Func<Guid, Task> test)
    {
        var admin = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.SystemAdmin);
        var all = await admin.GetFromJsonAsync<JsonElement>("/api/app/request-type/admin?maxResultCount=200");
        var type = all.GetProperty("items").EnumerateArray().Single(x => Text(x, "code") == RequestTypeCodes.StoreItem);
        var url = $"/api/app/request-type/{IdOf(type)}";

        var active = await OkAsync(await admin.PutAsJsonAsync(url, new
        {
            nameAr = Text(type, "nameAr"),
            nameEn = Text(type, "nameEn"),
            destination = "store",
            isActive = true,
            concurrencyStamp = Text(type, "concurrencyStamp")
        }));

        try
        {
            await test(IdOf(type));
        }
        finally
        {
            await OkAsync(await admin.PutAsJsonAsync(url, new
            {
                nameAr = Text(type, "nameAr"),
                nameEn = Text(type, "nameEn"),
                destination = "store",
                isActive = false,
                concurrencyStamp = Text(active, "concurrencyStamp")
            }));
        }
    }

    /// <summary>Takes a store request through the manager and HR, until the stock issue exists.</summary>
    private async Task<(Guid RequestId, Guid IssueId)> RequestReachingStoreAsync(Team team, Guid typeId, Guid itemId, decimal quantity)
    {
        var request = await OkAsync(
            await team.StaffClient.PostAsJsonAsync("/api/app/request", new { requestTypeId = typeId, description = "Need one", itemId, quantity }),
            HttpStatusCode.Created);
        var requestId = IdOf(request);

        await OkAsync(await team.ManagerClient.PostAsJsonAsync($"/api/app/request/{requestId}/approve", new { }));
        var hr = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.HrManager);
        var approved = await OkAsync(await hr.PostAsJsonAsync($"/api/app/request/{requestId}/approve", new { }));

        Text(approved, "status").ShouldBe("pendingDestination");
        Text(approved, "currentApproverRole").ShouldBe("storeOfficer");
        return (requestId, approved.GetProperty("stockIssue").GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task Item_rules_kind_lock_duplicate_names_stale_stamp_and_no_quantities_in_the_lookup()
    {
        var team = await TeamAsync();
        var item = await AddItemAsync(team.Officer);
        var url = $"/api/app/item/{IdOf(item)}";

        item.GetProperty("quantityOnHand").GetDecimal().ShouldBe(0);
        item.GetProperty("isBelowMinimum").GetBoolean().ShouldBeTrue();

        await ShouldFailWithAsync(
            await team.Officer.PostAsJsonAsync("/api/app/item", new { nameAr = Text(item, "nameAr"), nameEn = "Other " + Guid.NewGuid(), kind = "returnable", minQuantity = 0 }),
            HrErrorCodes.Item.NameAlreadyExists);

        (await team.Officer.PutAsJsonAsync(url, new { nameAr = "س " + Guid.NewGuid(), nameEn = "S " + Guid.NewGuid(), kind = "returnable", minQuantity = 0, isActive = true, concurrencyStamp = "stale" }))
            .StatusCode.ShouldBe(HttpStatusCode.Conflict);

        // The kind can still change before the first movement.
        var changed = await OkAsync(await team.Officer.PutAsJsonAsync(url, new
        {
            nameAr = Text(item, "nameAr"),
            nameEn = Text(item, "nameEn"),
            kind = "consumable",
            minQuantity = 1,
            isActive = true,
            concurrencyStamp = Text(item, "concurrencyStamp")
        }));
        Text(changed, "kind").ShouldBe("consumable");

        await OkAsync(await team.Officer.PostAsJsonAsync("/api/app/stock/receive", new { itemId = IdOf(item), quantity = 2 }), HttpStatusCode.Created);
        var current = await team.Officer.GetFromJsonAsync<JsonElement>(url);
        await ShouldFailWithAsync(
            await team.Officer.PutAsJsonAsync(url, new
            {
                nameAr = Text(item, "nameAr"),
                nameEn = Text(item, "nameEn"),
                kind = "returnable",
                minQuantity = 1,
                isActive = true,
                concurrencyStamp = Text(current, "concurrencyStamp")
            }),
            HrErrorCodes.Item.KindLocked);

        // Staff reach the lookup only, and the lookup holds no quantity.
        (await team.StaffClient.GetAsync("/api/app/item")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var lookup = await team.StaffClient.GetFromJsonAsync<JsonElement>($"/api/app/item/lookup?filter={Uri.EscapeDataString(Text(item, "nameEn"))}");
        var entry = lookup.GetProperty("items").EnumerateArray().Single();
        entry.TryGetProperty("quantityOnHand", out _).ShouldBeFalse();
        entry.TryGetProperty("minQuantity", out _).ShouldBeFalse();
    }

    [Fact]
    public async Task Movements_keep_a_running_balance_and_never_expose_the_cost()
    {
        var team = await TeamAsync();
        var item = await AddItemAsync(team.Officer);
        var itemId = IdOf(item);

        var receive = await team.Officer.PostAsJsonAsync("/api/app/stock/receive", new { itemId, quantity = 5, totalCost = 123.45m, note = "Bought" });
        var receiveBody = await receive.Content.ReadAsStringAsync();
        receive.StatusCode.ShouldBe(HttpStatusCode.Created, receiveBody);
        receiveBody.ShouldNotContain("123.45");
        receiveBody.ShouldNotContain("cost", Case.Insensitive);
        var received = JsonDocument.Parse(receiveBody).RootElement;
        received.GetProperty("balanceAfter").GetDecimal().ShouldBe(5);

        await ShouldFailWithAsync(
            await team.Officer.PostAsJsonAsync("/api/app/stock/adjust", new { itemId, quantity = 0, reason = "None" }),
            HrErrorCodes.Stock.ZeroQuantity);
        await ShouldFailWithAsync(
            await team.Officer.PostAsJsonAsync("/api/app/stock/adjust", new { itemId, quantity = -6, reason = "Too many" }),
            HrErrorCodes.Stock.InsufficientStock);

        var reversal = await OkAsync(
            await team.Officer.PostAsJsonAsync("/api/app/stock/adjust", new { itemId, quantity = -5, reason = "Wrong delivery", reversesMovementId = IdOf(received) }),
            HttpStatusCode.Created);
        reversal.GetProperty("balanceAfter").GetDecimal().ShouldBe(0);
        reversal.GetProperty("reversesMovementId").GetGuid().ShouldBe(IdOf(received));
        Text(reversal, "type").ShouldBe("adjustment");

        await OkAsync(await team.Officer.PostAsJsonAsync("/api/app/stock/receive", new { itemId, quantity = 5 }), HttpStatusCode.Created);
        await ShouldFailWithAsync(
            await team.Officer.PostAsJsonAsync("/api/app/stock/adjust", new { itemId, quantity = -5, reason = "Again", reversesMovementId = IdOf(received) }),
            HrErrorCodes.Stock.MovementAlreadyReversed);

        // A reversal has to mirror the movement it reverses.
        (await team.Officer.PostAsJsonAsync("/api/app/stock/adjust", new { itemId, quantity = -1, reason = "Part", reversesMovementId = IdOf(reversal) }))
            .StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var history = await team.Officer.GetAsync($"/api/app/stock/movements?itemId={itemId}&sorting=creationTime");
        var historyBody = await history.Content.ReadAsStringAsync();
        historyBody.ShouldNotContain("123.45");
        var rows = JsonDocument.Parse(historyBody).RootElement.GetProperty("items").EnumerateArray().ToList();
        rows.Select(x => x.GetProperty("quantity").GetDecimal()).ShouldBe(new[] { 5m, -5m, 5m });
        rows.Select(x => x.GetProperty("balanceAfter").GetDecimal()).ShouldBe(new[] { 5m, 0m, 5m });

        (await team.StaffClient.GetAsync("/api/app/stock/movements")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var inactive = await AddItemAsync(team.Officer);
        var stamp = Text(inactive, "concurrencyStamp");
        await OkAsync(await team.Officer.PutAsJsonAsync($"/api/app/item/{IdOf(inactive)}", new
        {
            nameAr = Text(inactive, "nameAr"),
            nameEn = Text(inactive, "nameEn"),
            kind = "returnable",
            minQuantity = 1,
            isActive = false,
            concurrencyStamp = stamp
        }));
        await ShouldFailWithAsync(
            await team.Officer.PostAsJsonAsync("/api/app/stock/receive", new { itemId = IdOf(inactive), quantity = 1 }),
            HrErrorCodes.Stock.ItemInactive);
    }

    [Fact]
    public Task A_returnable_request_runs_from_submission_to_delivery_and_deducts_only_on_delivery() =>
        WithStoreTypeActiveAsync(async typeId =>
        {
            var team = await TeamAsync();
            var itemId = IdOf(await AddItemAsync(team.Officer, stock: 3));

            var (requestId, issueId) = await RequestReachingStoreAsync(team, typeId, itemId, 2);
            var issueUrl = $"/api/app/stock-issue/{issueId}";

            var issue = await team.Officer.GetFromJsonAsync<JsonElement>(issueUrl);
            Text(issue, "status").ShouldBe("pendingAcknowledgement");
            issue.GetProperty("isAvailable").GetBoolean().ShouldBeTrue();
            issue.GetProperty("employee").TryGetProperty("fullName", out _).ShouldBeTrue();
            issue.GetProperty("employee").TryGetProperty("hireDate", out _).ShouldBeFalse();

            // Delivery cannot skip the acknowledgement, and approval is not the store's way through.
            await ShouldFailWithAsync(await team.Officer.PostAsJsonAsync($"{issueUrl}/deliver", new { }), HrErrorCodes.Stock.IssueInvalidState);
            // D2: approving is not the store officer's operation at all. They reject here and deliver through the stock issue.
            (await team.Officer.PostAsJsonAsync($"/api/app/request/{requestId}/approve", new { })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

            var seen = await OkAsync(await team.Officer.PostAsync($"{issueUrl}/acknowledge", null));
            Text(seen, "status").ShouldBe("acknowledged");
            seen.GetProperty("acknowledgedBy").GetProperty("id").ValueKind.ShouldBe(JsonValueKind.String);
            seen.GetProperty("quantityOnHand").GetDecimal().ShouldBe(3);
            await ShouldFailWithAsync(await team.Officer.PostAsync($"{issueUrl}/acknowledge", null), HrErrorCodes.Stock.IssueInvalidState);

            // Once the store has seen it, the owner can no longer cancel.
            await ShouldFailWithAsync(
                await team.StaffClient.PostAsJsonAsync($"/api/app/request/{requestId}/cancel", new { }),
                HrErrorCodes.Request.CannotCancelAtThisStage);

            var delivered = await OkAsync(await team.Officer.PostAsJsonAsync($"{issueUrl}/deliver", new { note = "Handed over" }));
            Text(delivered, "status").ShouldBe("delivered");
            delivered.GetProperty("quantityOnHand").GetDecimal().ShouldBe(1);
            var custodyId = delivered.GetProperty("custodyId").GetGuid();

            var request = await team.StaffClient.GetFromJsonAsync<JsonElement>($"/api/app/request/{requestId}");
            Text(request, "status").ShouldBe("completed");
            Text(request.GetProperty("stockIssue"), "status").ShouldBe("delivered");
            var actions = request.GetProperty("timeline").EnumerateArray().Select(x => Text(x, "action")).ToList();
            actions.ShouldContain(RequestTimelineActions.StockIssueAcknowledged);
            actions.ShouldContain(RequestTimelineActions.StockIssueDelivered);
            actions.Last().ShouldBe(RequestTimelineActions.Completed);

            // The custody is the owner's alone, and waits for their acknowledgement.
            var mine = await team.StaffClient.GetFromJsonAsync<JsonElement>("/api/app/custody/my");
            var custody = mine.GetProperty("items").EnumerateArray().Single(x => IdOf(x) == custodyId);
            Text(custody, "status").ShouldBe("pendingAcknowledgement");
            Text(custody, "kind").ShouldBe("personal");
            custody.GetProperty("stockIssueId").GetGuid().ShouldBe(issueId);

            var notifications = await team.StaffClient.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200");
            notifications.GetProperty("items").EnumerateArray().Select(x => Text(x, "templateCode")).ShouldContain("custodyAssigned");

            (await team.Officer.PostAsync($"/api/app/custody/{custodyId}/acknowledge", null)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
            var acknowledged = await OkAsync(await team.StaffClient.PostAsync($"/api/app/custody/{custodyId}/acknowledge", null));
            Text(acknowledged, "status").ShouldBe("acknowledged");
            await ShouldFailWithAsync(await team.StaffClient.PostAsync($"/api/app/custody/{custodyId}/acknowledge", null), HrErrorCodes.Custody.AlreadyAcknowledged);
        });

    [Fact]
    public Task Purchasing_keeps_the_request_open_and_a_failed_delivery_changes_nothing() =>
        WithStoreTypeActiveAsync(async typeId =>
        {
            var team = await TeamAsync();
            var itemId = IdOf(await AddItemAsync(team.Officer, kind: "consumable"));
            var otherItemId = IdOf(await AddItemAsync(team.Officer, kind: "consumable"));

            var (requestId, issueId) = await RequestReachingStoreAsync(team, typeId, itemId, 4);
            var issueUrl = $"/api/app/stock-issue/{issueId}";

            (await team.Officer.GetFromJsonAsync<JsonElement>(issueUrl)).GetProperty("isAvailable").GetBoolean().ShouldBeFalse();
            await ShouldFailWithAsync(await team.Officer.PostAsync($"{issueUrl}/mark-purchasing", null), HrErrorCodes.Stock.IssueInvalidState);
            await ShouldFailWithAsync(
                await team.Officer.PostAsJsonAsync("/api/app/stock/receive", new { itemId, quantity = 4, totalCost = 10, stockIssueId = issueId }),
                HrErrorCodes.Stock.IssueNotPurchasing);

            await OkAsync(await team.Officer.PostAsync($"{issueUrl}/acknowledge", null));

            // No stock yet: the delivery fails, and leaves neither a movement nor a state change behind.
            await ShouldFailWithAsync(await team.Officer.PostAsJsonAsync($"{issueUrl}/deliver", new { }), HrErrorCodes.Stock.InsufficientStock);
            Text(await team.Officer.GetFromJsonAsync<JsonElement>(issueUrl), "status").ShouldBe("acknowledged");
            (await team.Officer.GetFromJsonAsync<JsonElement>($"/api/app/stock/movements?itemId={itemId}")).GetProperty("totalCount").GetInt64().ShouldBe(0);

            var purchasing = await OkAsync(await team.Officer.PostAsync($"{issueUrl}/mark-purchasing", null));
            Text(purchasing, "status").ShouldBe("purchasing");
            Text(await team.StaffClient.GetFromJsonAsync<JsonElement>($"/api/app/request/{requestId}"), "status").ShouldBe("pendingDestination");

            await ShouldFailWithAsync(
                await team.Officer.PostAsJsonAsync("/api/app/stock/receive", new { itemId, quantity = 4, stockIssueId = issueId }),
                HrErrorCodes.Stock.PurchaseCostRequired);
            await ShouldFailWithAsync(
                await team.Officer.PostAsJsonAsync("/api/app/stock/receive", new { itemId = otherItemId, quantity = 4, totalCost = 10, stockIssueId = issueId }),
                HrErrorCodes.Stock.IssueItemMismatch);

            var received = await OkAsync(
                await team.Officer.PostAsJsonAsync("/api/app/stock/receive", new { itemId, quantity = 4, totalCost = 10, stockIssueId = issueId }),
                HttpStatusCode.Created);
            received.GetProperty("stockIssueId").GetGuid().ShouldBe(issueId);
            Text(await team.Officer.GetFromJsonAsync<JsonElement>(issueUrl), "status").ShouldBe("purchasing");

            // A consumable never becomes custody.
            var delivered = await OkAsync(await team.Officer.PostAsync($"{issueUrl}/deliver", null));
            Text(delivered, "status").ShouldBe("delivered");
            delivered.GetProperty("custodyId").ValueKind.ShouldBe(JsonValueKind.Null);
            delivered.GetProperty("quantityOnHand").GetDecimal().ShouldBe(0);
            (await team.StaffClient.GetFromJsonAsync<JsonElement>("/api/app/custody/my")).GetProperty("totalCount").GetInt64().ShouldBe(0);
        });

    [Fact]
    public Task Rejecting_or_cancelling_at_the_store_cancels_the_issue_and_shared_delivery_goes_to_the_department() =>
        WithStoreTypeActiveAsync(async typeId =>
        {
            var team = await TeamAsync();
            var itemId = IdOf(await AddItemAsync(team.Officer, stock: 5));

            var (cancelled, cancelledIssue) = await RequestReachingStoreAsync(team, typeId, itemId, 1);
            await OkAsync(await team.StaffClient.PostAsJsonAsync($"/api/app/request/{cancelled}/cancel", new { note = "Not needed" }));
            Text(await team.Officer.GetFromJsonAsync<JsonElement>($"/api/app/stock-issue/{cancelledIssue}"), "status").ShouldBe("cancelled");

            var (rejected, rejectedIssue) = await RequestReachingStoreAsync(team, typeId, itemId, 1);
            await OkAsync(await team.Officer.PostAsJsonAsync($"/api/app/request/{rejected}/reject", new { reason = "Not allowed" }));
            Text(await team.Officer.GetFromJsonAsync<JsonElement>($"/api/app/stock-issue/{rejectedIssue}"), "status").ShouldBe("cancelled");

            var (_, sharedIssue) = await RequestReachingStoreAsync(team, typeId, itemId, 2);
            await OkAsync(await team.Officer.PostAsync($"/api/app/stock-issue/{sharedIssue}/acknowledge", null));
            var delivered = await OkAsync(await team.Officer.PostAsJsonAsync($"/api/app/stock-issue/{sharedIssue}/deliver", new { custodyKind = "shared" }));

            var custody = await team.Officer.GetFromJsonAsync<JsonElement>($"/api/app/custody/{delivered.GetProperty("custodyId").GetGuid()}");
            Text(custody, "kind").ShouldBe("shared");
            custody.GetProperty("employee").ValueKind.ShouldBe(JsonValueKind.Null);
            custody.GetProperty("department").GetProperty("id").GetGuid().ShouldBe(team.Kit.DepartmentId);

            // Shared custody is not personal custody of the person who asked for it.
            (await team.StaffClient.GetFromJsonAsync<JsonElement>("/api/app/custody/my")).GetProperty("totalCount").GetInt64().ShouldBe(0);
            await OkAsync(await team.ManagerClient.PostAsync($"/api/app/custody/{IdOf(custody)}/acknowledge", null));
        });

    [Fact]
    public async Task Direct_custody_checks_its_holder_and_returning_it_restocks_only_on_request()
    {
        var team = await TeamAsync();
        var itemId = IdOf(await AddItemAsync(team.Officer, stock: 2));
        var consumableId = IdOf(await AddItemAsync(team.Officer, kind: "consumable"));

        await ShouldFailWithAsync(
            await team.Officer.PostAsJsonAsync("/api/app/custody", new { itemId, kind = "personal", employeeId = team.Staff.Id, departmentId = team.Kit.DepartmentId, quantity = 1, deductFromStock = false }),
            HrErrorCodes.Custody.InvalidHolder);
        await ShouldFailWithAsync(
            await team.Officer.PostAsJsonAsync("/api/app/custody", new { itemId, kind = "shared", employeeId = team.Staff.Id, quantity = 1, deductFromStock = false }),
            HrErrorCodes.Custody.InvalidHolder);
        await ShouldFailWithAsync(
            await team.Officer.PostAsJsonAsync("/api/app/custody", new { itemId = consumableId, kind = "personal", employeeId = team.Staff.Id, quantity = 1, deductFromStock = false }),
            HrErrorCodes.Custody.ItemNotReturnable);
        await ShouldFailWithAsync(
            await team.Officer.PostAsJsonAsync("/api/app/custody", new { itemId, kind = "personal", employeeId = team.Staff.Id, quantity = 9, deductFromStock = true }),
            HrErrorCodes.Stock.InsufficientStock);
        (await team.Officer.GetFromJsonAsync<JsonElement>($"/api/app/custody?itemId={itemId}")).GetProperty("totalCount").GetInt64().ShouldBe(0);

        // What the employee already holds does not touch the stock.
        var undeducted = await OkAsync(
            await team.Officer.PostAsJsonAsync("/api/app/custody", new { itemId, kind = "personal", employeeId = team.Staff.Id, quantity = 1, deductFromStock = false }),
            HttpStatusCode.Created);
        (await team.Officer.GetFromJsonAsync<JsonElement>($"/api/app/item/{itemId}")).GetProperty("quantityOnHand").GetDecimal().ShouldBe(2);

        var deducted = await OkAsync(
            await team.Officer.PostAsJsonAsync("/api/app/custody", new { itemId, kind = "personal", employeeId = team.Staff.Id, quantity = 2, deductFromStock = true }),
            HttpStatusCode.Created);
        (await team.Officer.GetFromJsonAsync<JsonElement>($"/api/app/item/{itemId}")).GetProperty("quantityOnHand").GetDecimal().ShouldBe(0);

        // Who reaches one custody: its holder and their manager, the store and HR, but not a colleague.
        (await team.StaffClient.GetAsync($"/api/app/custody/{IdOf(deducted)}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await team.ManagerClient.GetAsync($"/api/app/custody/{IdOf(deducted)}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        var colleague = await team.Kit.AddStaffAsync(team.Manager.Id);
        (await (await team.Kit.SignInAsync(colleague)).GetAsync($"/api/app/custody/{IdOf(deducted)}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var teamList = await team.ManagerClient.GetFromJsonAsync<JsonElement>($"/api/app/custody/team?employeeId={team.Staff.Id}");
        teamList.GetProperty("totalCount").GetInt64().ShouldBe(2);

        var report = await team.ManagerClient.GetFromJsonAsync<JsonElement>($"/api/app/report/team-custody?employeeId={team.Staff.Id}");
        report.GetProperty("totalCount").GetInt64().ShouldBe(2);
        report.GetProperty("items")[0].TryGetProperty("itemName", out _).ShouldBeTrue();

        // Damage: stays out of the balance and needs a note.
        await ShouldFailWithAsync(
            await team.Officer.PostAsJsonAsync($"/api/app/custody/{IdOf(deducted)}/return", new { restock = false }),
            HrErrorCodes.Custody.ReturnNoteRequired);
        var damaged = await OkAsync(await team.Officer.PostAsJsonAsync($"/api/app/custody/{IdOf(undeducted)}/return", new { restock = false, note = "Broken" }));
        Text(damaged, "status").ShouldBe("returned");
        Text(damaged, "note").ShouldBe("Broken");
        (await team.Officer.GetFromJsonAsync<JsonElement>($"/api/app/item/{itemId}")).GetProperty("quantityOnHand").GetDecimal().ShouldBe(0);

        await OkAsync(await team.Officer.PostAsJsonAsync($"/api/app/custody/{IdOf(deducted)}/return", new { restock = true }));
        (await team.Officer.GetFromJsonAsync<JsonElement>($"/api/app/item/{itemId}")).GetProperty("quantityOnHand").GetDecimal().ShouldBe(2);

        await ShouldFailWithAsync(
            await team.Officer.PostAsJsonAsync($"/api/app/custody/{IdOf(deducted)}/return", new { restock = true }),
            HrErrorCodes.Custody.AlreadyReturned);
        await ShouldFailWithAsync(await team.StaffClient.PostAsync($"/api/app/custody/{IdOf(deducted)}/acknowledge", null), HrErrorCodes.Custody.AlreadyReturned);

        var notifications = await team.StaffClient.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200");
        notifications.GetProperty("items").EnumerateArray().Select(x => Text(x, "templateCode")).ShouldContain("custodyReturned");
    }

    [Fact]
    public async Task A_count_covers_every_open_shared_custody_and_tells_the_store_about_a_difference()
    {
        var team = await TeamAsync();
        var kit = team.Kit;

        // A department with no staff is still held by its open shared custody, until that is returned.
        var emptyDepartment = await OkAsync(
            await kit.Hr.PostAsJsonAsync("/api/app/department", new { nameAr = "قسم " + Guid.NewGuid().ToString("N")[..8], nameEn = "Dept " + Guid.NewGuid().ToString("N")[..8] }),
            HttpStatusCode.Created);
        var held = await OkAsync(
            await team.Officer.PostAsJsonAsync("/api/app/custody", new { itemId = IdOf(await AddItemAsync(team.Officer)), kind = "shared", departmentId = IdOf(emptyDepartment), quantity = 1, deductFromStock = false }),
            HttpStatusCode.Created);
        await ShouldFailWithAsync(await kit.Hr.DeleteAsync($"/api/app/department/{IdOf(emptyDepartment)}"), HrErrorCodes.Department.HasSharedCustody);
        await OkAsync(await team.Officer.PostAsJsonAsync($"/api/app/custody/{IdOf(held)}/return", new { restock = false, note = "Moved away" }));
        (await kit.Hr.DeleteAsync($"/api/app/department/{IdOf(emptyDepartment)}")).IsSuccessStatusCode.ShouldBeTrue();

        // Each count needs its own department, because the test database shares open custody between tests.
        var department = await OkAsync(
            await kit.Hr.PostAsJsonAsync("/api/app/department", new { nameAr = "قسم " + Guid.NewGuid().ToString("N")[..8], nameEn = "Dept " + Guid.NewGuid().ToString("N")[..8] }),
            HttpStatusCode.Created);
        var departmentId = IdOf(department);
        var manager = await kit.AddDepartmentManagerAsync();
        var managerClient = await kit.SignInAsync(manager);
        var body = kit.Body(manager.Id, new[] { HrRoles.Employee });
        var json = JsonSerializer.SerializeToNode(body, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        json["departmentId"] = departmentId;
        await OkAsync(await kit.Hr.PostAsJsonAsync("/api/app/employee", json), HttpStatusCode.Created);

        var itemId = IdOf(await AddItemAsync(team.Officer));
        var first = await OkAsync(
            await team.Officer.PostAsJsonAsync("/api/app/custody", new { itemId, kind = "shared", departmentId, quantity = 3, deductFromStock = false }),
            HttpStatusCode.Created);
        var second = await OkAsync(
            await team.Officer.PostAsJsonAsync("/api/app/custody", new { itemId, kind = "shared", departmentId, quantity = 2, deductFromStock = false }),
            HttpStatusCode.Created);

        // A manager with no staff there is out of scope, and the department cannot be deleted while it holds custody.
        await ShouldFailWithAsync(
            await team.ManagerClient.PostAsJsonAsync("/api/app/custody-count", new { departmentId, countDate = "2026-11-01", lines = new[] { new { custodyId = IdOf(first), countedQuantity = 3 } } }),
            HrErrorCodes.Custody.DepartmentNotInScope);
        await ShouldFailWithAsync(await kit.Hr.DeleteAsync($"/api/app/department/{departmentId}"), HrErrorCodes.Department.HasEmployees);

        await ShouldFailWithAsync(
            await managerClient.PostAsJsonAsync("/api/app/custody-count", new { departmentId, countDate = "2026-11-01", lines = new[] { new { custodyId = IdOf(first), countedQuantity = 3 } } }),
            HrErrorCodes.Custody.CountLinesIncomplete);
        await ShouldFailWithAsync(
            await managerClient.PostAsJsonAsync("/api/app/custody-count", new
            {
                departmentId,
                countDate = "2026-11-01",
                lines = new[] { new { custodyId = IdOf(first), countedQuantity = 3 }, new { custodyId = IdOf(first), countedQuantity = 3 } }
            }),
            HrErrorCodes.Custody.CountLineInvalid);

        var matched = await OkAsync(
            await managerClient.PostAsJsonAsync("/api/app/custody-count", new
            {
                departmentId,
                countDate = "2026-11-01",
                lines = new[] { new { custodyId = IdOf(first), countedQuantity = 3 }, new { custodyId = IdOf(second), countedQuantity = 2 } }
            }),
            HttpStatusCode.Created);
        Text(matched, "result").ShouldBe("matched");

        var different = await OkAsync(
            await managerClient.PostAsJsonAsync("/api/app/custody-count", new
            {
                departmentId,
                countDate = "2026-11-02",
                lines = new[] { new { custodyId = IdOf(first), countedQuantity = 2 }, new { custodyId = IdOf(second), countedQuantity = 2 } }
            }),
            HttpStatusCode.Created);
        Text(different, "result").ShouldBe("discrepancy");
        different.GetProperty("lines").EnumerateArray().Single(x => x.GetProperty("custodyId").GetGuid() == IdOf(first)).GetProperty("expectedQuantity").GetDecimal().ShouldBe(3);

        // A count changes nothing on the custody itself.
        (await team.Officer.GetFromJsonAsync<JsonElement>($"/api/app/custody/{IdOf(first)}")).GetProperty("quantity").GetDecimal().ShouldBe(3);

        var officerNotifications = await team.Officer.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200");
        officerNotifications.GetProperty("items").EnumerateArray().Select(x => Text(x, "templateCode")).ShouldContain("custodyCountDifference");

        // The store sees the company, and another manager only their own departments.
        (await team.Officer.GetFromJsonAsync<JsonElement>($"/api/app/custody-count?departmentId={departmentId}")).GetProperty("totalCount").GetInt64().ShouldBe(2);
        (await team.ManagerClient.GetFromJsonAsync<JsonElement>($"/api/app/custody-count?departmentId={departmentId}")).GetProperty("totalCount").GetInt64().ShouldBe(0);
        (await managerClient.GetFromJsonAsync<JsonElement>($"/api/app/custody-count?result=discrepancy&departmentId={departmentId}")).GetProperty("totalCount").GetInt64().ShouldBe(1);

        var teamShared = await managerClient.GetFromJsonAsync<JsonElement>("/api/app/custody/team?kind=shared");
        teamShared.GetProperty("totalCount").GetInt64().ShouldBe(2);
    }

    [Fact]
    public async Task An_incident_shows_the_item_it_names()
    {
        var team = await TeamAsync();
        var item = await AddItemAsync(team.Officer);

        var incident = await OkAsync(
            await team.StaffClient.PostAsJsonAsync("/api/app/incident", new { itemId = IdOf(item), description = "It broke" }),
            HttpStatusCode.Created);

        incident.GetProperty("item").GetProperty("id").GetGuid().ShouldBe(IdOf(item));
        Text(incident.GetProperty("item"), "name").ShouldBe(Text(item, "nameEn"));

        var mine = await team.StaffClient.GetFromJsonAsync<JsonElement>("/api/app/incident/my");
        mine.GetProperty("items").EnumerateArray().Single(x => IdOf(x) == IdOf(incident)).GetProperty("item").GetProperty("id").GetGuid().ShouldBe(IdOf(item));
    }
}
