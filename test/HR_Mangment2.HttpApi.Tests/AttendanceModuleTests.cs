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
using HR_Mangment2.Attendance;
using HR_Mangment2.Errors;
using HR_Mangment2.Identity;
using HR_Mangment2.Infrastructure;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Uow;
using Xunit;

namespace HR_Mangment2;

/// <summary>
/// The test host does not run background jobs, so each test runs the queued attendance jobs itself, through the real
/// serializer and executer. All dates are in the past: 2026-10-04 is a Sunday, and the company is on UTC+3.
/// </summary>
[Collection(HostCollection.Name)]
public class AttendanceModuleTests
{
    private const string Sunday = "2026-10-04";
    private const string CheckIn = "2026-10-04T05:00:00Z";
    private const string LateCheckIn = "2026-10-04T05:30:00Z";
    private const string CheckOut = "2026-10-04T13:00:00Z";

    private readonly HostFixture _host;

    public AttendanceModuleTests(HostFixture host)
    {
        _host = host;
    }

    private static async Task<JsonElement> ReadAsync(HttpResponseMessage response) => await response.Content.ReadFromJsonAsync<JsonElement>();

    private static async Task ShouldFailWithAsync(HttpResponseMessage response, string code)
    {
        response.StatusCode.ShouldBe((HttpStatusCode)422, await response.Content.ReadAsStringAsync());
        (await ReadAsync(response)).GetProperty("error").GetProperty("code").GetString().ShouldBe(code);
    }

    private Task<HttpClient> SystemAdminAsync() => TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.SystemAdmin);

    private async Task<(Guid Id, string Key, string Stamp)> AddDeviceAsync(HttpClient admin)
    {
        var response = await admin.PostAsJsonAsync("/api/app/attendance-device", new { serialNumber = "SN-" + Guid.NewGuid().ToString("N"), name = "Gate" });
        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        var body = await ReadAsync(response);
        var device = body.GetProperty("device");
        return (device.GetProperty("id").GetGuid(), body.GetProperty("deviceKey").GetString()!, device.GetProperty("concurrencyStamp").GetString()!);
    }

    private Task<HttpResponseMessage> PunchAsync(string? key, params (string User, string Time)[] punches) =>
        PunchAsync(_host.CreateClient("en"), key, punches);

    private static Task<HttpResponseMessage> PunchAsync(HttpClient client, string? key, params (string User, string Time)[] punches)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/attendance-devices/punches")
        {
            Content = JsonContent.Create(new { punches = punches.Select(p => new { deviceUserId = p.User, punchTime = p.Time }) })
        };

        if (key != null)
        {
            request.Headers.Add("X-Device-Key", key);
        }

        return client.SendAsync(request);
    }

    private async Task MapAsync(HttpClient admin, Guid deviceId, string deviceUserId, Guid employeeId)
    {
        var response = await admin.PostAsJsonAsync($"/api/app/attendance-device/{deviceId}/user-map", new { deviceUserId, employeeId });
        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
    }

    /// <summary>Runs what the host's job worker would run, oldest first, until the queue is empty.</summary>
    private Task RunAttendanceJobsAsync() => RunJobsAsync(_host);

    internal static async Task RunJobsAsync(HostFixture host)
    {
        using var scope = host.Factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var repository = services.GetRequiredService<IBackgroundJobRepository>();
        var unitOfWorkManager = services.GetRequiredService<IUnitOfWorkManager>();
        var options = services.GetRequiredService<IOptions<AbpBackgroundJobOptions>>().Value;
        var serializer = services.GetRequiredService<IBackgroundJobSerializer>();
        var executer = services.GetRequiredService<IBackgroundJobExecuter>();

        while (true)
        {
            List<BackgroundJobRecord> jobs;
            using (var read = unitOfWorkManager.Begin(requiresNew: true))
            {
                // The queue also holds every notification sent in this test run. Those have low priority, so they sort after these.
                jobs = (await repository.GetWaitingListAsync(null, 1000))
                    .Where(x => x.JobName.StartsWith("Hr.Attendance", StringComparison.Ordinal))
                    .OrderBy(x => x.CreationTime)
                    .ToList();
                await read.CompleteAsync();
            }

            if (jobs.Count == 0)
            {
                return;
            }

            foreach (var job in jobs)
            {
                var configuration = options.GetJob(job.JobName);
                await executer.ExecuteAsync(new JobExecutionContext(services, configuration.JobType, serializer.Deserialize(job.JobArgs, configuration.ArgsType)));

                using var remove = unitOfWorkManager.Begin(requiresNew: true);
                await repository.DeleteAsync(job.Id);
                await remove.CompleteAsync();
            }
        }
    }

    private async Task EnqueueAllEmployeesAsync(DateOnly date)
    {
        using var scope = _host.Factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var tenant = await services.GetRequiredService<Volo.Abp.TenantManagement.ITenantRepository>().FindByNameAsync(
            services.GetRequiredService<Volo.Abp.MultiTenancy.ITenantNormalizer>().NormalizeName(HostFixture.TestTenantName)!);

        using var unitOfWork = services.GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true);
        await services.GetRequiredService<IBackgroundJobManager>().EnqueueAsync(new AttendanceRecalculationArgs { TenantId = tenant!.Id, AllFrom = date, AllTo = date });
        await unitOfWork.CompleteAsync();
    }

    private static async Task<JsonElement> MyDayAsync(HttpClient client, string date)
    {
        var page = await client.GetFromJsonAsync<JsonElement>($"/api/app/attendance/my?fromDate={date}&toDate={date}");
        page.GetProperty("items").GetArrayLength().ShouldBe(1, page.ToString());
        return page.GetProperty("items")[0];
    }

    [Fact]
    public async Task A_device_key_is_shown_once_and_a_rotation_cancels_the_old_key()
    {
        var admin = await SystemAdminAsync();
        var device = await AddDeviceAsync(admin);

        var list = await admin.GetFromJsonAsync<JsonElement>("/api/app/attendance-device?maxResultCount=200");
        var row = list.GetProperty("items").EnumerateArray().Single(x => x.GetProperty("id").GetGuid() == device.Id);
        row.TryGetProperty("deviceKey", out _).ShouldBeFalse();
        row.TryGetProperty("keyHash", out _).ShouldBeFalse();
        row.GetProperty("lastSeenAt").ValueKind.ShouldBe(JsonValueKind.Null);

        (await PunchAsync(device.Key, ("1", CheckIn))).StatusCode.ShouldBe(HttpStatusCode.OK);

        var rotated = await admin.PostAsync($"/api/app/attendance-device/{device.Id}/rotate-key", null);
        rotated.StatusCode.ShouldBe(HttpStatusCode.OK);
        var newKey = (await ReadAsync(rotated)).GetProperty("deviceKey").GetString();
        newKey.ShouldNotBe(device.Key);

        (await PunchAsync(device.Key, ("1", CheckOut))).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await PunchAsync(newKey, ("1", CheckOut))).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task A_date_time_read_back_from_the_database_is_still_the_same_utc_moment()
    {
        var admin = await SystemAdminAsync();
        var device = await AddDeviceAsync(admin);

        // Regression: a legacy Npgsql switch once moved stored values by the machine's offset on every read.
        var list = await admin.GetFromJsonAsync<JsonElement>("/api/app/attendance-device?maxResultCount=200");
        var row = list.GetProperty("items").EnumerateArray().Single(x => x.GetProperty("id").GetGuid() == device.Id);

        var raw = row.GetProperty("creationTime").GetString()!;
        raw.ShouldEndWith("Z");
        (DateTime.UtcNow - DateTime.Parse(raw).ToUniversalTime()).Duration().ShouldBeLessThan(TimeSpan.FromMinutes(2));
    }

    [Fact]
    public async Task The_real_job_worker_builds_the_day_after_a_punch_arrives()
    {
        var admin = await SystemAdminAsync();
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var client = await kit.SignInAsync(staff);
        var device = await AddDeviceAsync(admin);
        await MapAsync(admin, device.Id, "w", staff.Id);

        await PunchAsync(device.Key, ("w", "2026-08-02T05:00:00Z"), ("w", "2026-08-02T13:00:00Z"));

        // Nothing delivers the notifications queued by the other tests, and this worker should not spend its time on them.
        await _host.ClearNotificationJobsAsync();

        // A second host on the same database, with the worker running as it does in production.
        await using var worker = _host.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.PostConfigure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = true);
            services.PostConfigure<AbpBackgroundJobWorkerOptions>(options => options.JobPollPeriod = 200);
        }));
        _ = worker.Server;

        JsonElement page = default;
        for (var attempt = 0; attempt < 100; attempt++)
        {
            page = await client.GetFromJsonAsync<JsonElement>("/api/app/attendance/my?fromDate=2026-08-02&toDate=2026-08-02");
            if (page.GetProperty("totalCount").GetInt64() == 1)
            {
                break;
            }

            await Task.Delay(200);
        }

        page.GetProperty("totalCount").GetInt64().ShouldBe(1);
        page.GetProperty("items")[0].GetProperty("status").GetString().ShouldBe("present");
    }

    [Fact]
    public async Task A_missing_or_wrong_key_or_a_disabled_device_gets_401_in_the_error_shape()
    {
        var admin = await SystemAdminAsync();
        var device = await AddDeviceAsync(admin);

        (await PunchAsync(null, ("1", CheckIn))).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var wrong = await PunchAsync("not-a-real-key", ("1", CheckIn));
        wrong.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await ReadAsync(wrong)).GetProperty("error").ValueKind.ShouldBe(JsonValueKind.Object);

        var disabled = await admin.PutAsJsonAsync($"/api/app/attendance-device/{device.Id}", new { name = "Gate", isActive = false, concurrencyStamp = device.Stamp });
        disabled.StatusCode.ShouldBe(HttpStatusCode.OK, await disabled.Content.ReadAsStringAsync());

        (await PunchAsync(device.Key, ("1", CheckIn))).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task The_key_is_checked_before_the_body_and_an_empty_batch_is_400()
    {
        var admin = await SystemAdminAsync();
        var device = await AddDeviceAsync(admin);

        (await PunchAsync(null)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await PunchAsync(device.Key)).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Device_writes_need_the_manage_permission_and_serial_numbers_are_unique()
    {
        var admin = await SystemAdminAsync();
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var serial = "SN-" + Guid.NewGuid().ToString("N");

        (await kit.Hr.PostAsJsonAsync("/api/app/attendance-device", new { serialNumber = serial, name = "X" })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await kit.Hr.GetAsync("/api/app/attendance-device")).StatusCode.ShouldBe(HttpStatusCode.OK);

        (await admin.PostAsJsonAsync("/api/app/attendance-device", new { serialNumber = serial, name = "X" })).StatusCode.ShouldBe(HttpStatusCode.Created);
        (await admin.PostAsJsonAsync("/api/app/attendance-device", new { serialNumber = serial, name = "Y" })).StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await admin.PostAsJsonAsync("/api/app/attendance-device", new { serialNumber = "", name = "Y" })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task A_repeated_create_with_the_same_idempotency_key_makes_one_device_and_only_the_latest_key_works()
    {
        var admin = await SystemAdminAsync();
        var idempotencyKey = Guid.NewGuid().ToString();
        var body = new { serialNumber = "SN-" + Guid.NewGuid().ToString("N"), name = "Gate" };

        async Task<JsonElement> CreateAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/app/attendance-device") { Content = JsonContent.Create(body) };
            request.Headers.Add("Idempotency-Key", idempotencyKey);
            var response = await admin.SendAsync(request);
            response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
            return await ReadAsync(response);
        }

        var first = await CreateAsync();
        var second = await CreateAsync();

        second.GetProperty("device").GetProperty("id").GetGuid().ShouldBe(first.GetProperty("device").GetProperty("id").GetGuid());
        (await PunchAsync(first.GetProperty("deviceKey").GetString(), ("1", CheckIn))).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await PunchAsync(second.GetProperty("deviceKey").GetString(), ("1", CheckIn))).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Punches_are_counted_and_resending_a_whole_batch_changes_nothing()
    {
        var admin = await SystemAdminAsync();
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var device = await AddDeviceAsync(admin);
        await MapAsync(admin, device.Id, "1047", staff.Id);

        var future = DateTime.UtcNow.AddHours(2).ToString("yyyy-MM-ddTHH:mm:ssZ");
        var batch = new[] { ("1047", CheckIn), ("1047", CheckIn), ("2210", CheckIn), (" ", CheckIn), ("1047", future) };

        var first = await PunchAsync(device.Key, batch);
        first.StatusCode.ShouldBe(HttpStatusCode.OK, await first.Content.ReadAsStringAsync());
        var result = await ReadAsync(first);

        result.GetProperty("receivedCount").GetInt32().ShouldBe(5);
        result.GetProperty("acceptedCount").GetInt32().ShouldBe(1);
        result.GetProperty("duplicateCount").GetInt32().ShouldBe(1);
        result.GetProperty("unmappedCount").GetInt32().ShouldBe(1);
        result.GetProperty("invalidCount").GetInt32().ShouldBe(2);

        var issues = result.GetProperty("issues").EnumerateArray().ToList();
        issues.Count.ShouldBe(3);
        issues[0].GetProperty("index").GetInt32().ShouldBe(2);
        issues[0].GetProperty("status").GetString().ShouldBe("unmappedDeviceUser");
        issues[1].GetProperty("status").GetString().ShouldBe("invalid");
        issues[2].GetProperty("index").GetInt32().ShouldBe(4);

        var again = await ReadAsync(await PunchAsync(device.Key, batch));
        again.GetProperty("acceptedCount").GetInt32().ShouldBe(0);
        again.GetProperty("unmappedCount").GetInt32().ShouldBe(0);
        again.GetProperty("duplicateCount").GetInt32().ShouldBe(3);
        again.GetProperty("invalidCount").GetInt32().ShouldBe(2);

        await RunAttendanceJobsAsync();
    }

    [Fact]
    public async Task Receiving_punches_updates_last_seen_without_changing_the_devices_stamp()
    {
        var admin = await SystemAdminAsync();
        var device = await AddDeviceAsync(admin);

        (await PunchAsync(device.Key, ("1", CheckIn))).StatusCode.ShouldBe(HttpStatusCode.OK);

        var list = await admin.GetFromJsonAsync<JsonElement>("/api/app/attendance-device?maxResultCount=200");
        var row = list.GetProperty("items").EnumerateArray().Single(x => x.GetProperty("id").GetGuid() == device.Id);
        row.GetProperty("lastSeenAt").ValueKind.ShouldBe(JsonValueKind.String);
        row.GetProperty("concurrencyStamp").GetString().ShouldBe(device.Stamp);
        row.GetProperty("unmappedUserCount").GetInt32().ShouldBe(1);

        var stale = await admin.PutAsJsonAsync($"/api/app/attendance-device/{device.Id}", new { name = "New", isActive = true, concurrencyStamp = "stale" });
        stale.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task A_user_held_by_an_access_gate_does_not_block_the_device_path()
    {
        var admin = await SystemAdminAsync();
        var device = await AddDeviceAsync(admin);
        var (_, userName) = await TestUsers.CreateAsync(_host, true, HrRoles.Employee);
        var gated = TestAuth.ClientFor(_host, (await TestAuth.SignInAsync(_host, userName, TestUsers.Password)).AccessToken, "en");

        (await gated.GetAsync("/api/app/attendance/my")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await PunchAsync(gated, device.Key, ("1", CheckIn))).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Mapping_links_earlier_punches_and_builds_the_day_and_removing_it_keeps_them()
    {
        var admin = await SystemAdminAsync();
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var client = await kit.SignInAsync(staff);
        var device = await AddDeviceAsync(admin);

        await PunchAsync(device.Key, ("77", LateCheckIn), ("77", CheckOut));

        var unmapped = await admin.GetFromJsonAsync<JsonElement>($"/api/app/attendance-device/{device.Id}/unmapped-user");
        unmapped.GetProperty("totalCount").GetInt64().ShouldBe(1);
        var entry = unmapped.GetProperty("items")[0];
        entry.GetProperty("deviceUserId").GetString().ShouldBe("77");
        entry.GetProperty("punchCount").GetInt32().ShouldBe(2);

        var mapResponse = await admin.PostAsJsonAsync($"/api/app/attendance-device/{device.Id}/user-map", new { deviceUserId = "77", employeeId = staff.Id });
        mapResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var map = await ReadAsync(mapResponse);
        map.GetProperty("employee").GetProperty("id").GetGuid().ShouldBe(staff.Id);
        map.GetProperty("employee").TryGetProperty("email", out _).ShouldBeFalse();

        await RunAttendanceJobsAsync();

        var day = await MyDayAsync(client, Sunday);
        day.GetProperty("status").GetString().ShouldBe("late");
        day.GetProperty("lateMinutes").GetInt32().ShouldBe(30);
        day.GetProperty("workedMinutes").GetInt32().ShouldBe(450);
        day.GetProperty("isCorrected").GetBoolean().ShouldBeFalse();
        day.GetProperty("date").GetString().ShouldBe(Sunday);
        day.GetProperty("scheduledStart").GetDateTime().ShouldBe(DateTime.Parse(CheckIn).ToUniversalTime());

        (await admin.GetFromJsonAsync<JsonElement>($"/api/app/attendance-device/{device.Id}/unmapped-user")).GetProperty("totalCount").GetInt64().ShouldBe(0);

        (await admin.PostAsJsonAsync($"/api/app/attendance-device/{device.Id}/user-map", new { deviceUserId = "77", employeeId = manager.Id })).StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await admin.PostAsJsonAsync($"/api/app/attendance-device/{device.Id}/user-map", new { deviceUserId = "78", employeeId = staff.Id })).StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var mapId = map.GetProperty("id").GetGuid();
        (await admin.DeleteAsync($"/api/app/attendance-device/{Guid.NewGuid()}/user-map/{mapId}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await admin.DeleteAsync($"/api/app/attendance-device/{device.Id}/user-map/{mapId}")).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        (await MyDayAsync(client, Sunday)).GetProperty("status").GetString().ShouldBe("late");

        var after = await ReadAsync(await PunchAsync(device.Key, ("77", "2026-10-05T05:00:00Z")));
        after.GetProperty("unmappedCount").GetInt32().ShouldBe(1);

        await RunAttendanceJobsAsync();
    }

    [Fact]
    public async Task The_executive_director_is_not_covered_by_attendance()
    {
        var admin = await SystemAdminAsync();
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var device = await AddDeviceAsync(admin);

        await ShouldFailWithAsync(
            await admin.PostAsJsonAsync($"/api/app/attendance-device/{device.Id}/user-map", new { deviceUserId = "1", employeeId = kit.ExecutiveEmployeeId }),
            HrErrorCodes.Attendance.ExecutiveDirectorNotCovered);

        await ShouldFailWithAsync(
            await kit.Hr.GetAsync($"/api/app/attendance/employee/{kit.ExecutiveEmployeeId}"),
            HrErrorCodes.Attendance.ExecutiveDirectorNotCovered);

        var executive = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.ExecutiveDirector);
        await ShouldFailWithAsync(await executive.GetAsync("/api/app/attendance/my"), HrErrorCodes.Attendance.ExecutiveDirectorNotCovered);

        // The system admin is outside attendance too (decision 2026-11-10).
        await ShouldFailWithAsync(
            await admin.PostAsJsonAsync($"/api/app/attendance-device/{device.Id}/user-map", new { deviceUserId = "2", employeeId = kit.SystemAdminEmployeeId }),
            HrErrorCodes.Attendance.ExecutiveDirectorNotCovered);
        await ShouldFailWithAsync(
            await kit.Hr.GetAsync($"/api/app/attendance/employee/{kit.SystemAdminEmployeeId}"),
            HrErrorCodes.Attendance.ExecutiveDirectorNotCovered);
        await ShouldFailWithAsync(await admin.GetAsync("/api/app/attendance/my"), HrErrorCodes.Attendance.ExecutiveDirectorNotCovered);
    }

    [Fact]
    public async Task Each_audience_sees_only_its_own_scope()
    {
        var admin = await SystemAdminAsync();
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var otherManager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var colleague = await kit.AddStaffAsync(otherManager.Id);

        var device = await AddDeviceAsync(admin);
        await MapAsync(admin, device.Id, "s", staff.Id);
        await MapAsync(admin, device.Id, "c", colleague.Id);
        await PunchAsync(device.Key, ("s", CheckIn), ("s", CheckOut), ("c", LateCheckIn), ("c", CheckOut));
        await RunAttendanceJobsAsync();

        var staffClient = await kit.SignInAsync(staff);
        var colleagueClient = await kit.SignInAsync(colleague);
        var managerClient = await kit.SignInAsync(manager);
        var otherManagerClient = await kit.SignInAsync(otherManager);

        var dayId = (await MyDayAsync(staffClient, Sunday)).GetProperty("id").GetGuid();

        (await staffClient.GetAsync($"/api/app/attendance/day/{dayId}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await colleagueClient.GetAsync($"/api/app/attendance/day/{dayId}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await managerClient.GetAsync($"/api/app/attendance/day/{dayId}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await otherManagerClient.GetAsync($"/api/app/attendance/day/{dayId}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await kit.Hr.GetAsync($"/api/app/attendance/day/{dayId}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await staffClient.GetAsync($"/api/app/attendance/day/{Guid.NewGuid()}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        (await managerClient.GetAsync($"/api/app/attendance/employee/{staff.Id}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await managerClient.GetAsync($"/api/app/attendance/employee/{colleague.Id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await kit.Hr.GetAsync($"/api/app/attendance/employee/{colleague.Id}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await kit.Hr.GetAsync($"/api/app/attendance/employee/{Guid.NewGuid()}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await staffClient.GetAsync($"/api/app/attendance/employee/{staff.Id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await staffClient.GetAsync("/api/app/attendance/team")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        var team = await managerClient.GetFromJsonAsync<JsonElement>($"/api/app/attendance/team?fromDate={Sunday}&toDate={Sunday}");
        team.GetProperty("items").EnumerateArray().Select(x => x.GetProperty("employee").GetProperty("id").GetGuid()).ShouldBe(new[] { staff.Id });

        var foreignFilter = await managerClient.GetFromJsonAsync<JsonElement>($"/api/app/attendance/team?employeeId={colleague.Id}");
        foreignFilter.GetProperty("totalCount").GetInt64().ShouldBe(0);
    }

    [Fact]
    public async Task The_day_detail_shows_the_raw_punches_and_originals()
    {
        var admin = await SystemAdminAsync();
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var device = await AddDeviceAsync(admin);
        await MapAsync(admin, device.Id, "9", staff.Id);

        await PunchAsync(device.Key, ("9", CheckIn), ("9", "2026-10-04T09:00:00Z"), ("9", CheckOut));
        await RunAttendanceJobsAsync();

        var client = await kit.SignInAsync(staff);
        var day = await MyDayAsync(client, Sunday);
        var detail = await client.GetFromJsonAsync<JsonElement>($"/api/app/attendance/day/{day.GetProperty("id").GetGuid()}");

        var raw = detail.GetProperty("rawPunches").EnumerateArray().ToList();
        raw.Count.ShouldBe(3);
        raw[0].GetProperty("source").GetString().ShouldBe("device");
        raw[0].GetProperty("deviceName").GetString().ShouldBe("Gate");
        raw.Select(x => x.GetProperty("punchTime").GetDateTime()).ShouldBeInOrder();

        detail.GetProperty("originalCheckIn").GetDateTime().ShouldBe(DateTime.Parse(CheckIn).ToUniversalTime());
        detail.GetProperty("originalCheckOut").GetDateTime().ShouldBe(DateTime.Parse(CheckOut).ToUniversalTime());
        detail.GetProperty("corrections").GetArrayLength().ShouldBe(0);
        detail.GetProperty("day").GetProperty("status").GetString().ShouldBe("present");
    }

    [Fact]
    public async Task A_work_day_with_no_punch_becomes_absent_and_the_summary_covers_every_matching_row()
    {
        var admin = await SystemAdminAsync();
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var device = await AddDeviceAsync(admin);
        await MapAsync(admin, device.Id, "5", staff.Id);

        await PunchAsync(device.Key, ("5", CheckIn), ("5", CheckOut), ("5", "2026-10-05T05:30:00Z"), ("5", "2026-10-05T13:00:00Z"));
        await RunAttendanceJobsAsync();

        // Tuesday has no punch. Friday is the seeded shift's day off.
        await EnqueueAllEmployeesAsync(new DateOnly(2026, 10, 6));
        await EnqueueAllEmployeesAsync(new DateOnly(2026, 10, 9));
        await RunAttendanceJobsAsync();

        var client = await kit.SignInAsync(staff);
        (await MyDayAsync(client, "2026-10-06")).GetProperty("status").GetString().ShouldBe("absent");
        (await MyDayAsync(client, "2026-10-09")).GetProperty("status").GetString().ShouldBe("weeklyOff");

        var page = await client.GetFromJsonAsync<JsonElement>("/api/app/attendance/my?fromDate=2026-10-04&toDate=2026-10-09&maxResultCount=1");
        page.GetProperty("items").GetArrayLength().ShouldBe(1);
        page.GetProperty("items")[0].GetProperty("date").GetString().ShouldBe("2026-10-09");
        page.GetProperty("totalCount").GetInt64().ShouldBeGreaterThanOrEqualTo(4);

        var summary = page.GetProperty("summary");
        summary.GetProperty("presentDays").GetInt32().ShouldBe(2);
        summary.GetProperty("lateDays").GetInt32().ShouldBe(1);
        summary.GetProperty("totalLateMinutes").GetInt32().ShouldBe(30);
        summary.GetProperty("absentDays").GetInt32().ShouldBe(1);
        summary.GetProperty("workedMinutes").GetInt64().ShouldBe(480 + 450);

        var filtered = await client.GetFromJsonAsync<JsonElement>("/api/app/attendance/my?status=absent&fromDate=2026-10-04&toDate=2026-10-09");
        filtered.GetProperty("totalCount").GetInt64().ShouldBe(1);

        var bySort = await client.GetAsync("/api/app/attendance/my?sorting=bogus");
        bySort.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task A_holiday_changes_the_day_and_removing_it_changes_it_back()
    {
        var admin = await SystemAdminAsync();
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var device = await AddDeviceAsync(admin);
        await MapAsync(admin, device.Id, "h", staff.Id);

        const string date = "2026-09-13";
        await PunchAsync(device.Key, ("h", "2026-09-13T05:00:00Z"), ("h", "2026-09-13T13:00:00Z"));
        await RunAttendanceJobsAsync();

        var client = await kit.SignInAsync(staff);
        (await MyDayAsync(client, date)).GetProperty("status").GetString().ShouldBe("present");

        var created = await kit.Hr.PostAsJsonAsync("/api/app/holiday", new { date, nameAr = "عطلة", nameEn = "Holiday" });
        created.StatusCode.ShouldBe(HttpStatusCode.Created, await created.Content.ReadAsStringAsync());
        await RunAttendanceJobsAsync();

        var holiday = await MyDayAsync(client, date);
        holiday.GetProperty("status").GetString().ShouldBe("holiday");
        holiday.GetProperty("scheduledStart").ValueKind.ShouldBe(JsonValueKind.Null);
        holiday.GetProperty("workedMinutes").GetInt32().ShouldBe(480);

        (await kit.Hr.DeleteAsync($"/api/app/holiday/{(await ReadAsync(created)).GetProperty("id").GetGuid()}")).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await RunAttendanceJobsAsync();

        (await MyDayAsync(client, date)).GetProperty("status").GetString().ShouldBe("present");
    }

    [Fact]
    public async Task A_night_shift_belongs_to_the_day_it_started()
    {
        var admin = await SystemAdminAsync();
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();

        var shift = await kit.Hr.PostAsJsonAsync("/api/app/work-shift", new
        {
            nameAr = "ليلي",
            nameEn = "Night",
            type = "overnight",
            graceMinutes = 0,
            overtimePolicy = "afterShiftEnd",
            days = new[] { new { dayOfWeek = "sunday", startTime = "22:00", endTime = "06:00", crossesMidnight = true } }
        });
        shift.StatusCode.ShouldBe(HttpStatusCode.Created, await shift.Content.ReadAsStringAsync());

        var staff = await kit.AddAsync(manager.Id, new[] { HrRoles.Employee }, shiftId: (await ReadAsync(shift)).GetProperty("id").GetGuid());
        var device = await AddDeviceAsync(admin);
        await MapAsync(admin, device.Id, "n", staff.Id);

        // Sunday 22:00 to Monday 07:00 in company time, which is one hour past the scheduled end.
        await PunchAsync(device.Key, ("n", "2026-10-04T19:00:00Z"), ("n", "2026-10-05T04:00:00Z"));
        await RunAttendanceJobsAsync();

        var client = await kit.SignInAsync(staff);
        var day = await MyDayAsync(client, Sunday);
        day.GetProperty("status").GetString().ShouldBe("present");
        day.GetProperty("workedMinutes").GetInt32().ShouldBe(540);
        day.GetProperty("overtimeMinutes").GetInt32().ShouldBe(60);
        day.GetProperty("scheduledEnd").GetDateTime().ShouldBe(DateTime.Parse("2026-10-05T03:00:00Z").ToUniversalTime());

        var monday = await client.GetFromJsonAsync<JsonElement>("/api/app/attendance/my?fromDate=2026-10-05&toDate=2026-10-05");
        foreach (var item in monday.GetProperty("items").EnumerateArray())
        {
            item.GetProperty("checkIn").ValueKind.ShouldBe(JsonValueKind.Null);
        }
    }

    private static MultipartFormDataContent ImportForm(byte[] content, string fileName, Guid deviceId)
    {
        return new MultipartFormDataContent
        {
            { new ByteArrayContent(content), "file", fileName },
            { new StringContent(deviceId.ToString()), "deviceId" }
        };
    }

    [Fact]
    public async Task A_csv_import_saves_rows_in_company_time_and_reports_each_bad_row()
    {
        var admin = await SystemAdminAsync();
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var device = await AddDeviceAsync(admin);
        await MapAsync(admin, device.Id, "31", staff.Id);

        var csv = new StringBuilder()
            .AppendLine("deviceUserId,punchDateTime")
            .AppendLine("31,2026-10-04 08:00")
            .AppendLine("31,2026-10-04 16:00")
            .AppendLine("31,2026-10-04 16:00")
            .AppendLine("99,2026-10-04 08:00")
            .AppendLine(",2026-10-04 08:00")
            .AppendLine("31,04/10/2026 08:00")
            .AppendLine("31,2099-01-01 08:00")
            .AppendLine()
            .ToString();

        var response = await kit.Hr.PostAsync("/api/app/attendance-import", ImportForm(Encoding.UTF8.GetBytes(csv), "punches.csv", device.Id));
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted, await response.Content.ReadAsStringAsync());
        var queued = await ReadAsync(response);
        queued.GetProperty("status").GetString().ShouldBe("queued");
        queued.GetProperty("uploadedBy").GetProperty("id").ValueKind.ShouldBe(JsonValueKind.String);
        var id = queued.GetProperty("id").GetGuid();

        await RunAttendanceJobsAsync();

        var import = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/attendance-import/{id}");
        import.GetProperty("status").GetString().ShouldBe("completedWithErrors");
        import.GetProperty("totalRows").GetInt32().ShouldBe(7);
        import.GetProperty("acceptedCount").GetInt32().ShouldBe(2);
        import.GetProperty("duplicateCount").GetInt32().ShouldBe(1);
        import.GetProperty("unmappedCount").GetInt32().ShouldBe(1);
        import.GetProperty("errorCount").GetInt32().ShouldBe(3);
        import.GetProperty("completedAt").ValueKind.ShouldBe(JsonValueKind.String);

        var errors = import.GetProperty("errors").EnumerateArray().ToList();
        errors.Select(x => x.GetProperty("code").GetString()).ShouldBe(new[] { "emptyDeviceUserId", "invalidDateTime", "futurePunch" });
        errors.Select(x => x.GetProperty("rowNumber").GetInt32()).ShouldBe(new[] { 6, 7, 8 });
        errors[0].GetProperty("message").GetString().ShouldNotBeNullOrWhiteSpace();
        errors[0].GetProperty("message").GetString().ShouldNotStartWith("AttendanceImportError");

        var day = await MyDayAsync(await kit.SignInAsync(staff), Sunday);
        day.GetProperty("checkIn").GetDateTime().ShouldBe(DateTime.Parse(CheckIn).ToUniversalTime());
        day.GetProperty("status").GetString().ShouldBe("present");

        var detail = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/attendance/day/{day.GetProperty("id").GetGuid()}");
        detail.GetProperty("rawPunches")[0].GetProperty("source").GetString().ShouldBe("file");
        detail.GetProperty("rawPunches")[0].GetProperty("importId").GetGuid().ShouldBe(id);

        var list = await kit.Hr.GetFromJsonAsync<JsonElement>("/api/app/attendance-import?status=completedWithErrors&maxResultCount=200");
        list.GetProperty("items").EnumerateArray().Any(x => x.GetProperty("id").GetGuid() == id).ShouldBeTrue();
    }

    [Fact]
    public async Task An_excel_import_reads_numbers_and_real_date_cells()
    {
        var admin = await SystemAdminAsync();
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var device = await AddDeviceAsync(admin);
        await MapAsync(admin, device.Id, "4500", staff.Id);

        byte[] content;
        using (var workbook = new XLWorkbook())
        {
            var sheet = workbook.AddWorksheet("Punches");
            sheet.Cell(1, 1).Value = "punchDateTime";
            sheet.Cell(1, 2).Value = "deviceUserId";
            sheet.Cell(2, 1).Value = new DateTime(2026, 10, 4, 8, 0, 0);
            sheet.Cell(2, 2).Value = 4500;
            sheet.Cell(3, 1).Value = "2026-10-04 16:00";
            sheet.Cell(3, 2).Value = "4500";
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            content = stream.ToArray();
        }

        var response = await kit.Hr.PostAsync("/api/app/attendance-import", ImportForm(content, "punches.xlsx", device.Id));
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted, await response.Content.ReadAsStringAsync());
        await RunAttendanceJobsAsync();

        var import = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/attendance-import/{(await ReadAsync(response)).GetProperty("id").GetGuid()}");
        import.GetProperty("status").GetString().ShouldBe("completed", import.ToString());
        import.GetProperty("acceptedCount").GetInt32().ShouldBe(2);
    }

    [Fact]
    public async Task A_file_without_the_required_columns_fails_with_a_row_error_and_damage_fails_cleanly()
    {
        var admin = await SystemAdminAsync();
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var device = await AddDeviceAsync(admin);

        var missing = await kit.Hr.PostAsync("/api/app/attendance-import", ImportForm(Encoding.UTF8.GetBytes("deviceUserId,time\n1,2026-10-04 08:00\n"), "a.csv", device.Id));
        missing.StatusCode.ShouldBe(HttpStatusCode.Accepted);

        var damaged = await kit.Hr.PostAsync("/api/app/attendance-import", ImportForm(new byte[] { 0x50, 0x4B, 1, 2, 3, 4 }, "a.xlsx", device.Id));
        damaged.StatusCode.ShouldBe(HttpStatusCode.Accepted);

        await RunAttendanceJobsAsync();

        var first = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/attendance-import/{(await ReadAsync(missing)).GetProperty("id").GetGuid()}");
        first.GetProperty("status").GetString().ShouldBe("failed");
        first.GetProperty("errors")[0].GetProperty("code").GetString().ShouldBe("missingColumn");
        first.GetProperty("errors")[0].GetProperty("columnName").GetString().ShouldBe("punchDateTime");

        var second = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/attendance-import/{(await ReadAsync(damaged)).GetProperty("id").GetGuid()}");
        second.GetProperty("status").GetString().ShouldBe("failed");
    }

    [Fact]
    public async Task An_upload_is_refused_for_a_bad_file_or_an_inactive_device()
    {
        var admin = await SystemAdminAsync();
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var device = await AddDeviceAsync(admin);
        var csv = Encoding.UTF8.GetBytes("deviceUserId,punchDateTime\n");

        await ShouldFailWithAsync(await kit.Hr.PostAsync("/api/app/attendance-import", ImportForm(csv, "a.txt", device.Id)), HrErrorCodes.Attendance.ImportFileInvalid);
        await ShouldFailWithAsync(await kit.Hr.PostAsync("/api/app/attendance-import", ImportForm(csv, "a.xlsx", device.Id)), HrErrorCodes.Attendance.ImportFileInvalid);
        await ShouldFailWithAsync(await kit.Hr.PostAsync("/api/app/attendance-import", ImportForm(new byte[20 * 1024 * 1024 + 1], "a.csv", device.Id)), HrErrorCodes.Attendance.ImportFileInvalid);

        (await kit.Hr.PostAsync("/api/app/attendance-import", ImportForm(csv, "a.csv", Guid.NewGuid()))).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await admin.PostAsync("/api/app/attendance-import", ImportForm(csv, "a.csv", device.Id))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        (await admin.PutAsJsonAsync($"/api/app/attendance-device/{device.Id}", new { name = "Gate", isActive = false, concurrencyStamp = device.Stamp })).EnsureSuccessStatusCode();
        await ShouldFailWithAsync(await kit.Hr.PostAsync("/api/app/attendance-import", ImportForm(csv, "a.csv", device.Id)), HrErrorCodes.Attendance.DeviceInactive);
    }

    [Fact]
    public async Task Attendance_settings_refuse_gps_and_a_bad_zone_and_check_the_stamp()
    {
        var admin = await SystemAdminAsync();
        var kit = await EmployeeTestKit.CreateAsync(_host);

        var settings = await kit.Hr.GetFromJsonAsync<JsonElement>("/api/app/attendance-settings");
        settings.GetProperty("method").GetString().ShouldBe("device");
        settings.GetProperty("gpsAvailable").GetBoolean().ShouldBeFalse();
        settings.GetProperty("timeZone").GetString().ShouldBe("Asia/Baghdad");
        var stamp = settings.GetProperty("concurrencyStamp").GetString();

        (await kit.Hr.PutAsJsonAsync("/api/app/attendance-settings", new { method = "device", timeZone = "Asia/Baghdad", concurrencyStamp = stamp })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        await ShouldFailWithAsync(
            await admin.PutAsJsonAsync("/api/app/attendance-settings", new { method = "gps", timeZone = "Asia/Baghdad", concurrencyStamp = stamp }),
            HrErrorCodes.Attendance.GpsNotAvailable);

        (await admin.PutAsJsonAsync("/api/app/attendance-settings", new { method = "device", timeZone = "Mars/Olympus", concurrencyStamp = stamp })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await admin.PutAsJsonAsync("/api/app/attendance-settings", new { method = "device", timeZone = "Asia/Baghdad", concurrencyStamp = "stale" })).StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var saved = await admin.PutAsJsonAsync("/api/app/attendance-settings", new { method = "device", timeZone = "Asia/Baghdad", concurrencyStamp = stamp });
        saved.StatusCode.ShouldBe(HttpStatusCode.OK, await saved.Content.ReadAsStringAsync());
    }
}
