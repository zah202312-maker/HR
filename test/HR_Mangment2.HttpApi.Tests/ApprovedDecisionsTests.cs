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
using HR_Mangment2.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;
using Shouldly;
using Xunit;

namespace HR_Mangment2;

/// <summary>Step 15: the decisions Zainab approved, applied to the contract first and then to the code.</summary>
[Collection(HostCollection.Name)]
public class ApprovedDecisionsTests
{
    private readonly HostFixture _host;

    public ApprovedDecisionsTests(HostFixture host)
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

    private static string Text(JsonElement element, string property) => element.GetProperty(property).GetString()!;

    [Fact]
    public async Task Every_employee_needs_an_email_and_a_number_of_their_own()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();

        var body = JsonSerializer.SerializeToElement(kit.Body(manager.Id, new[] { HrRoles.Employee }));
        var withoutEmail = body.EnumerateObject().Where(x => x.Name != "email").ToDictionary(x => x.Name, x => x.Value);
        (await kit.Hr.PostAsJsonAsync("/api/app/employee", withoutEmail)).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        await OkAsync(await kit.Hr.PostAsJsonAsync("/api/app/employee", body), HttpStatusCode.Created);

        var again = body.EnumerateObject().ToDictionary(x => x.Name, x => x.Value);
        again["userName"] = JsonSerializer.SerializeToElement("s" + Guid.NewGuid().ToString("N")[..12]);
        again["email"] = JsonSerializer.SerializeToElement(EmployeeTestKit.UniqueEmail());
        (await kit.Hr.PostAsJsonAsync("/api/app/employee", again)).StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task An_inactive_shift_has_its_own_code_when_creating_an_employee()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var shift = await OkAsync(await kit.Hr.PostAsJsonAsync("/api/app/work-shift", new
        {
            nameAr = "دوام " + suffix, nameEn = "Shift " + suffix, type = "fixed", graceMinutes = 10, overtimePolicy = "none", isActive = false,
            days = new[] { new { dayOfWeek = "sunday", startTime = "09:00", endTime = "17:00", crossesMidnight = false } }
        }), HttpStatusCode.Created);

        // A new shift always starts active, so it is turned off separately.
        await OkAsync(await kit.Hr.PutAsJsonAsync($"/api/app/work-shift/{shift.GetProperty("id").GetGuid()}", new
        {
            nameAr = "دوام " + suffix, nameEn = "Shift " + suffix, type = "fixed", graceMinutes = 10, overtimePolicy = "none", isActive = false,
            days = new[] { new { dayOfWeek = "sunday", startTime = "09:00", endTime = "17:00", crossesMidnight = false } },
            concurrencyStamp = shift.GetProperty("concurrencyStamp").GetString()
        }));

        await ShouldFailWithAsync(
            await kit.Hr.PostAsJsonAsync("/api/app/employee", kit.Body(manager.Id, new[] { HrRoles.Employee }, shiftId: shift.GetProperty("id").GetGuid())),
            HrErrorCodes.Shift.ShiftInactive);
    }

    [Fact]
    public async Task Hire_date_correction_works_with_a_recorded_reason()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var url = $"/api/app/employee/{staff.Id}/correct-hire-date";

        var file = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/employee/{staff.Id}");
        var stamp = Text(file, "concurrencyStamp");

        await ShouldFailWithAsync(
            await kit.Hr.PostAsJsonAsync(url, new { hireDate = Text(file, "hireDate"), reason = "Same", concurrencyStamp = stamp }),
            HrErrorCodes.Employee.HireDateUnchanged);
        (await kit.Hr.PostAsJsonAsync(url, new { hireDate = "2024-06-01", reason = "", concurrencyStamp = stamp })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await (await kit.SignInAsync(manager)).PostAsJsonAsync(url, new { hireDate = "2024-06-01", reason = "x", concurrencyStamp = stamp })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var corrected = await OkAsync(await kit.Hr.PostAsJsonAsync(url, new { hireDate = "2024-06-01", reason = "Typed wrongly", concurrencyStamp = stamp }));
        Text(corrected, "hireDate").ShouldBe("2024-06-01");

        (await kit.Hr.PostAsJsonAsync(url, new { hireDate = "2024-06-02", reason = "Stale", concurrencyStamp = stamp })).StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task A_person_holding_custody_cannot_be_ended()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var officer = await kit.SignInAsync(await kit.AddAsync(manager.Id, new[] { HrRoles.Employee, HrRoles.StoreOfficer }));

        var suffix = Guid.NewGuid().ToString("N")[..10];
        var item = await OkAsync(await officer.PostAsJsonAsync("/api/app/item", new { nameAr = "صنف " + suffix, nameEn = "Item " + suffix, kind = "returnable", minQuantity = 1 }), HttpStatusCode.Created);
        var itemId = item.GetProperty("id").GetGuid();
        await OkAsync(await officer.PostAsJsonAsync("/api/app/stock/receive", new { itemId, quantity = 5 }), HttpStatusCode.Created);
        await OkAsync(await officer.PostAsJsonAsync("/api/app/custody", new { itemId, kind = "personal", employeeId = staff.Id, quantity = 1, deductFromStock = true }), HttpStatusCode.Created);

        var file = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/employee/{staff.Id}");
        var update = new
        {
            fullNameAr = Text(file, "fullNameAr"),
            email = Text(file, "email"),
            departmentId = kit.DepartmentId,
            jobTitle = Text(file, "jobTitle"),
            hireDate = Text(file, "hireDate"),
            status = "ended",
            concurrencyStamp = Text(file, "concurrencyStamp")
        };

        await ShouldFailWithAsync(await kit.Hr.PutAsJsonAsync($"/api/app/employee/{staff.Id}", update), HrErrorCodes.Employee.HasOutstandingCustody);
    }

    [Fact]
    public async Task Executive_reaches_sent_circulars_and_role_setting_as_approved()
    {
        var executive = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.ExecutiveDirector);
        (await executive.GetAsync("/api/app/circular/sent")).StatusCode.ShouldBe(HttpStatusCode.OK);

        var kit = await EmployeeTestKit.CreateAsync(_host);
        var staff = await kit.AddStaffAsync((await kit.AddDepartmentManagerAsync()).Id);

        // The executive acts on the HR manager's balance only.
        (await executive.PostAsJsonAsync($"/api/app/leave-balance/employee/{staff.Id}/adjust", new { days = 1, reason = "x" })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Permissions_split_follow_the_contract_roles()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.SignInAsync(await kit.AddDepartmentManagerAsync());
        var executive = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.ExecutiveDirector);

        // HR reads the role list and not a single role, and the executive reads one warning and not the list.
        (await kit.Hr.GetAsync($"/api/app/role/{Guid.NewGuid()}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await executive.GetAsync("/api/app/warning")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await executive.GetAsync($"/api/app/warning/{Guid.NewGuid()}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await manager.GetAsync("/api/app/appreciation-letter")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await executive.GetAsync("/api/app/appreciation-letter")).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Application_configuration_follows_the_contract_defaults()
    {
        var anonymous = _host.CreateClient();
        var defaults = await anonymous.GetFromJsonAsync<JsonElement>("/api/abp/application-configuration");
        defaults.GetProperty("localization").GetProperty("values").EnumerateObject().Count().ShouldBe(0);
        defaults.GetProperty("extraProperties").ValueKind.ShouldBe(JsonValueKind.Null);

        var asked = await _host.CreateClient().GetFromJsonAsync<JsonElement>("/api/abp/application-configuration?includeLocalizationResources=true");
        asked.GetProperty("localization").GetProperty("values").EnumerateObject().Count().ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task HR_can_delegate_for_an_absent_approver_and_nobody_else_can()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var absent = await kit.AddDepartmentManagerAsync();
        var delegate1 = await kit.AddDepartmentManagerAsync();
        var other = await kit.SignInAsync(await kit.AddDepartmentManagerAsync());
        var start = DateTime.UtcNow.AddMinutes(5);
        var body = new { fromUserId = absent.UserId, toUserId = delegate1.UserId, scope = "requests", startsAt = start, endsAt = start.AddDays(1), reason = "Absent" };

        (await other.PostAsJsonAsync("/api/app/delegation", body)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var created = await OkAsync(await kit.Hr.PostAsJsonAsync("/api/app/delegation", body), HttpStatusCode.Created);
        created.GetRawText().ShouldContain(absent.UserId.ToString());

        var plain = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.HrManager);
        await ShouldFailWithAsync(
            await plain.PostAsJsonAsync("/api/app/delegation", new { fromUserId = Guid.NewGuid(), toUserId = delegate1.UserId, scope = "requests", startsAt = start, endsAt = start.AddDays(1), reason = "Absent" }),
            HrErrorCodes.Delegation.DelegatorNotEligible);
    }

    [Fact]
    public async Task Email_is_queued_with_the_change_and_the_job_delivers_it()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var email = EmployeeTestKit.UniqueEmail();
        var staff = await kit.AddStaffAsync((await kit.AddDepartmentManagerAsync()).Id, email);
        await _host.ClearNotificationJobsAsync();

        using var scope = _host.Factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var tenant = await services.GetRequiredService<ITenantRepository>().FindByNameAsync(
            services.GetRequiredService<ITenantNormalizer>().NormalizeName(HostFixture.TestTenantName)!);
        using var change = services.GetRequiredService<ICurrentTenant>().Change(tenant!.Id);
        var unitOfWorkManager = services.GetRequiredService<IUnitOfWorkManager>();

        // A change that does not commit leaves neither the notification nor its delivery behind.
        using (unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
        {
            await services.GetRequiredService<HrNotificationPublisher>().PublishAsync(staff.UserId, NotificationTemplateCode.CircularReceived);
        }

        var repository = services.GetRequiredService<IBackgroundJobRepository>();
        using (var read = unitOfWorkManager.Begin(requiresNew: true))
        {
            (await repository.GetWaitingListAsync(null, 1000)).Count(x => x.JobName == "HrNotificationDelivery").ShouldBe(0);
            await read.CompleteAsync();
        }

        CapturingEmailSender.LastBodyTo(email).ShouldBeNull();

        using (var commit = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
        {
            await services.GetRequiredService<HrNotificationPublisher>().PublishAsync(staff.UserId, NotificationTemplateCode.CircularReceived);
            await commit.CompleteAsync();
        }

        // Committing queues the mail and sends nothing yet.
        CapturingEmailSender.LastBodyTo(email).ShouldBeNull();

        BackgroundJobRecord job;
        using (var read = unitOfWorkManager.Begin(requiresNew: true))
        {
            job = (await repository.GetWaitingListAsync(null, 1000)).Single(x => x.JobName == "HrNotificationDelivery");
            await read.CompleteAsync();
        }

        var options = services.GetRequiredService<IOptions<AbpBackgroundJobOptions>>().Value;
        var configuration = options.GetJob(job.JobName);
        await services.GetRequiredService<IBackgroundJobExecuter>().ExecuteAsync(new JobExecutionContext(
            services,
            configuration.JobType,
            services.GetRequiredService<IBackgroundJobSerializer>().Deserialize(job.JobArgs, configuration.ArgsType)));

        CapturingEmailSender.LastBodyTo(email).ShouldNotBeNull();
        await _host.ClearNotificationJobsAsync();
    }
}
