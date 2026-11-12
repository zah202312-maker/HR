using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.HrEvaluations;
using HR_Mangment2.Identity;
using HR_Mangment2.Infrastructure;
using HR_Mangment2.Settings;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;
using Xunit;

namespace HR_Mangment2;

/// <summary>
/// Low average alert of HR evaluations (decision 2026-11-11): per cycle, per employee and heading, sum / count at or
/// below the threshold with at least MinEvaluations evaluations alerts the HR managers.
/// </summary>
[Collection(HostCollection.Name)]
public class HrEvaluationAlertTests
{
    private const string Criteria = "/api/app/hr-evaluation-criterion";
    private const string Evaluations = "/api/app/hr-evaluation";
    private const string Alerts = "/api/app/hr-evaluation-alert";
    private const string Setting = "/api/app/system-settings/hr-evaluation-alert";

    private readonly HostFixture _host;

    public HrEvaluationAlertTests(HostFixture host)
    {
        _host = host;
    }

    private static async Task<JsonElement> OkAsync(HttpResponseMessage response, HttpStatusCode expected = HttpStatusCode.OK)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe(expected, body);
        return JsonDocument.Parse(body).RootElement;
    }

    private static Guid IdOf(JsonElement element) => element.GetProperty("id").GetGuid();

    private static string Text(JsonElement element, string property) => element.GetProperty(property).GetString()!;

    private static async Task DeactivateAllAsync(HttpClient hr)
    {
        var active = await hr.GetFromJsonAsync<JsonElement>($"{Criteria}?isActive=true&maxResultCount=200");
        foreach (var c in active.GetProperty("items").EnumerateArray())
        {
            await OkAsync(await hr.PutAsJsonAsync($"{Criteria}/{IdOf(c)}",
                new { nameAr = Text(c, "nameAr"), nameEn = Text(c, "nameEn"), sortOrder = c.GetProperty("sortOrder").GetInt32(), isActive = false, concurrencyStamp = Text(c, "concurrencyStamp") }));
        }
    }

    private static async Task EvaluateAsync(HttpClient hr, Guid employeeId, Guid a, int scoreA, Guid b, int scoreB) =>
        await OkAsync(await hr.PostAsJsonAsync(Evaluations,
            new { employeeId, scores = new[] { new { criterionId = a, score = scoreA }, new { criterionId = b, score = scoreB } } }), HttpStatusCode.Created);

    private async Task<T> InTenantAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = _host.Factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var tenant = await services.GetRequiredService<ITenantRepository>().FindByNameAsync(
            services.GetRequiredService<ITenantNormalizer>().NormalizeName(HostFixture.TestTenantName)!);
        using var change = services.GetRequiredService<ICurrentTenant>().Change(tenant!.Id);
        using var unitOfWork = services.GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: true);
        var result = await action(services);
        await unitOfWork.CompleteAsync();
        return result;
    }

    [Fact]
    public async Task A_cycle_average_of_seven_or_less_alerts_hr_once_per_heading()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var low = await kit.AddStaffAsync(manager.Id);
        var few = await kit.AddStaffAsync(manager.Id);
        var lowClient = await kit.SignInAsync(low);
        var hr = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.HrManager);
        var executive = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.ExecutiveDirector);

        await DeactivateAllAsync(hr);
        var respect = IdOf(await OkAsync(await hr.PostAsJsonAsync(Criteria, new { nameAr = "الاحترام", nameEn = "Respect", sortOrder = 1 }), HttpStatusCode.Created));
        var quality = IdOf(await OkAsync(await hr.PostAsJsonAsync(Criteria, new { nameAr = "جودة العمل", nameEn = "Quality", sortOrder = 2 }), HttpStatusCode.Created));

        // Respect 10, 9, 6, 7 = 8 (no alert); quality 7, 7, 6, 7 = 6.75 (alert). The example of the decision.
        await EvaluateAsync(hr, low.Id, respect, 10, quality, 7);
        await EvaluateAsync(hr, low.Id, respect, 9, quality, 7);
        await EvaluateAsync(hr, low.Id, respect, 6, quality, 6);
        await EvaluateAsync(hr, low.Id, respect, 7, quality, 7);

        // Three evaluations are fewer than the minimum of four, however low.
        for (var i = 0; i < 3; i++)
        {
            await EvaluateAsync(hr, few.Id, respect, 2, quality, 2);
        }

        var settings = new HrEvaluationAlertSettings { Threshold = 7, WindowWeeks = 4, MinEvaluations = 4 };
        var created = await InTenantAsync(async services =>
        {
            var today = await services.GetRequiredService<ICompanyClock>().GetTodayAsync();
            return await services.GetRequiredService<HrEvaluationAlertService>().ComputeCycleAsync(today.AddDays(-27), today, settings);
        });
        created.Where(x => x.EmployeeId == low.Id || x.EmployeeId == few.Id).Select(x => x.CriterionId).ShouldBe(new[] { quality });

        var list = await OkAsync(await hr.GetAsync($"{Alerts}?employeeId={low.Id}"));
        list.GetProperty("totalCount").GetInt64().ShouldBe(1);
        var alert = list.GetProperty("items")[0];
        var alertId = IdOf(alert);
        Text(alert, "criterionName").ShouldBe("Quality");
        alert.GetProperty("averageScore").GetDecimal().ShouldBe(6.75m);
        alert.GetProperty("evaluationsCount").GetInt32().ShouldBe(4);
        alert.GetProperty("threshold").GetDecimal().ShouldBe(7m);
        alert.GetProperty("employee").GetProperty("id").GetGuid().ShouldBe(low.Id);
        (await OkAsync(await hr.GetAsync($"{Alerts}?employeeId={few.Id}"))).GetProperty("totalCount").GetInt64().ShouldBe(0);

        // The HR manager is notified and opens it; nobody else reads it.
        (await hr.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200")).GetProperty("items").EnumerateArray()
            .ShouldContain(x => Text(x, "templateCode") == "hrEvaluationLowAverage" && Text(x, "resourceType") == "hrEvaluationAlert" && x.GetProperty("resourceId").GetGuid() == alertId);
        Text(await OkAsync(await hr.GetAsync($"{Alerts}/{alertId}")), "criterionName").ShouldBe("Quality");
        (await lowClient.GetAsync($"{Alerts}/{alertId}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await executive.GetAsync(Alerts)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await lowClient.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200")).GetRawText().ShouldNotContain(alertId.ToString());
        (await hr.GetAsync($"{Alerts}/{Guid.NewGuid()}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // Repeating the same cycle adds nothing.
        var again = await InTenantAsync(async services =>
        {
            var today = await services.GetRequiredService<ICompanyClock>().GetTodayAsync();
            return await services.GetRequiredService<HrEvaluationAlertService>().ComputeCycleAsync(today.AddDays(-27), today, settings);
        });
        again.ShouldNotContain(x => x.EmployeeId == low.Id);

        await DeactivateAllAsync(hr);
    }

    [Fact]
    public async Task A_cycle_is_computed_once_when_it_ends()
    {
        var periods = await InTenantAsync(async services =>
        {
            var store = services.GetRequiredService<HrSettingsStore>();
            var alerts = services.GetRequiredService<HrEvaluationAlertService>();
            var last = (await store.GetValueAsync<HrEvaluationAlertState>()).LastPeriodTo;
            var start = (last ?? await services.GetRequiredService<ICompanyClock>().GetTodayAsync()).AddDays(29);

            var seen = new List<DateOnly?>();
            await alerts.CheckDueCycleAsync(start);
            seen.Add((await store.GetValueAsync<HrEvaluationAlertState>()).LastPeriodTo);
            await alerts.CheckDueCycleAsync(start.AddDays(1));
            seen.Add((await store.GetValueAsync<HrEvaluationAlertState>()).LastPeriodTo);
            await alerts.CheckDueCycleAsync(start.AddDays(28));
            seen.Add((await store.GetValueAsync<HrEvaluationAlertState>()).LastPeriodTo);
            seen.Add(start);
            return seen;
        });

        var start = periods[3]!.Value;
        periods[0].ShouldBe(start.AddDays(-1));
        periods[1].ShouldBe(start.AddDays(-1));
        periods[2].ShouldBe(start.AddDays(27));
    }

    [Fact]
    public async Task Only_the_hr_manager_edits_the_alert_setting()
    {
        var hr = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.HrManager);
        var executive = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.ExecutiveDirector);
        (await executive.GetAsync(Setting)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var current = await OkAsync(await hr.GetAsync(Setting));
        var stamp = Text(current, "concurrencyStamp");

        (await hr.PutAsJsonAsync(Setting, new { threshold = 7, windowWeeks = 13, minEvaluations = 4, concurrencyStamp = stamp })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await hr.PutAsJsonAsync(Setting, new { threshold = 0.5, windowWeeks = 4, minEvaluations = 4, concurrencyStamp = stamp })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var changed = await OkAsync(await hr.PutAsJsonAsync(Setting, new { threshold = 6.5, windowWeeks = 2, minEvaluations = 3, concurrencyStamp = stamp }));
        changed.GetProperty("threshold").GetDecimal().ShouldBe(6.5m);
        changed.GetProperty("windowWeeks").GetInt32().ShouldBe(2);
        changed.GetProperty("minEvaluations").GetInt32().ShouldBe(3);
        (await hr.PutAsJsonAsync(Setting, new { threshold = 7, windowWeeks = 4, minEvaluations = 4, concurrencyStamp = stamp })).StatusCode.ShouldBe(HttpStatusCode.Conflict);

        // Back to the decision's values for the other tests.
        await OkAsync(await hr.PutAsJsonAsync(Setting, new { threshold = 7, windowWeeks = 4, minEvaluations = 4, concurrencyStamp = Text(changed, "concurrencyStamp") }));
    }
}
