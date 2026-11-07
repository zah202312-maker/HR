using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HR_Mangment2.EmploymentContracts;
using HR_Mangment2.Errors;
using HR_Mangment2.Identity;
using HR_Mangment2.Infrastructure;
using HR_Mangment2.Warnings;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Xunit;

namespace HR_Mangment2;

[Collection(HostCollection.Name)]
public class EmploymentRecordsModuleTests
{
    private static readonly byte[] Pdf = Encoding.ASCII.GetBytes("%PDF-1.4 test");

    private readonly HostFixture _host;

    public EmploymentRecordsModuleTests(HostFixture host)
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

    private static string Day(int offset) => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(offset).ToString("yyyy-MM-dd");

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

    private static MultipartFormDataContent WarningForm(Guid employeeId, string level = "first", string? incidentDate = null, byte[]? file = null, string contentType = "application/pdf")
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(employeeId.ToString()), "employeeId" },
            { new StringContent(level), "recommendedLevel" },
            { new StringContent(incidentDate ?? Day(-1)), "incidentDate" },
            { new StringContent("Late without notice"), "description" }
        };

        if (file != null)
        {
            var part = new ByteArrayContent(file);
            part.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            form.Add(part, "attachments", "incident.pdf");
        }

        return form;
    }

    private static MultipartFormDataContent LetterForm(Guid employeeId, byte[]? file = null, string contentType = "application/pdf")
    {
        var part = new ByteArrayContent(file ?? Pdf);
        part.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        return new MultipartFormDataContent
        {
            { new StringContent(employeeId.ToString()), "employeeId" },
            { new StringContent("Thank you"), "title" },
            { part, "file", "letter.pdf" }
        };
    }

    private static Task<JsonElement> CreateContractAsync(HttpClient hr, Guid employeeId, int startOffset, int endOffset, Guid? previous = null) =>
        hr.PostAsJsonAsync("/api/app/contract", new { employeeId, contractType = "Annual", startDate = Day(startOffset), endDate = Day(endOffset), previousContractId = previous })
            .ContinueWith(x => OkAsync(x.Result, HttpStatusCode.Created)).Unwrap();

    [Fact]
    public async Task Contract_runs_from_recommendation_to_decision_and_hides_the_steps_from_the_employee()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var staffClient = await kit.SignInAsync(staff);

        var contract = await CreateContractAsync(kit.Hr, staff.Id, -300, 10);
        Text(contract, "status").ShouldBe("pendingRecommendation");
        contract.GetProperty("daysRemaining").GetInt32().ShouldBe(10);
        contract.GetProperty("recommendation").ValueKind.ShouldBe(JsonValueKind.Null);
        var id = IdOf(contract);

        (await staffClient.GetAsync($"/api/app/notification?maxResultCount=200")).EnsureSuccessStatusCode();

        await ShouldFailWithAsync(await kit.Hr.PostAsJsonAsync($"/api/app/contract/{id}/decide", new { decision = "renew" }), HrErrorCodes.Contract.InvalidStatus);

        var recommended = await OkAsync(await kit.Hr.PostAsJsonAsync($"/api/app/contract/{id}/recommend", new { employeePosition = "Wants to stay", recommendation = "renew" }));
        Text(recommended, "status").ShouldBe("pendingDecision");
        recommended.GetProperty("decisionDueDate").ValueKind.ShouldBe(JsonValueKind.String);
        recommended.GetProperty("isDecisionOverdue").GetBoolean().ShouldBeFalse();

        var before = await OkAsync(await staffClient.GetAsync($"/api/app/contract/my/{id}"));
        Text(before, "status").ShouldBe("active");
        before.TryGetProperty("recommendation", out _).ShouldBeFalse();

        var decided = await OkAsync(await kit.Hr.PostAsJsonAsync($"/api/app/contract/{id}/decide", new { decision = "doNotRenew", note = "internal", employeeMessage = "See HR" }));
        Text(decided, "status").ShouldBe("notRenewed");
        Text(decided.GetProperty("recommendation"), "recommendation").ShouldBe("renew");
        decided.GetProperty("employeeNotifiedAt").ValueKind.ShouldBe(JsonValueKind.String);

        var after = await OkAsync(await staffClient.GetAsync($"/api/app/contract/my/{id}"));
        Text(after, "status").ShouldBe("notRenewed");
        Text(after, "employeeMessage").ShouldBe("See HR");
        after.GetRawText().ShouldNotContain("internal");

        var other = await kit.SignInAsync(await kit.AddStaffAsync(manager.Id));
        (await other.GetAsync($"/api/app/contract/my/{id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await staffClient.GetAsync($"/api/app/contract/{id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await (await kit.SignInAsync(manager)).GetAsync("/api/app/contract")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Contract_create_rules_and_the_employee_file_summary()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);

        Task<HttpResponseMessage> Post(object body) => kit.Hr.PostAsJsonAsync("/api/app/contract", body);

        await ShouldFailWithAsync(await Post(new { employeeId = Guid.NewGuid(), contractType = "A", startDate = Day(1), endDate = Day(2) }), HrErrorCodes.Contract.EmployeeNotFound);
        await ShouldFailWithAsync(await Post(new { employeeId = staff.Id, contractType = "A", startDate = Day(5), endDate = Day(5) }), HrErrorCodes.Contract.InvalidDateRange);
        (await Post(new { employeeId = staff.Id, contractType = "", startDate = Day(1), endDate = Day(2) })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var first = await CreateContractAsync(kit.Hr, staff.Id, -10, 400);
        Text(first, "status").ShouldBe("active");

        await ShouldFailWithAsync(await Post(new { employeeId = staff.Id, contractType = "A", startDate = Day(100), endDate = Day(500) }), HrErrorCodes.Contract.OverlappingContract);
        await ShouldFailWithAsync(await Post(new { employeeId = staff.Id, contractType = "A", startDate = Day(401), endDate = Day(500), previousContractId = IdOf(first) }), HrErrorCodes.Contract.InvalidPreviousContract);

        var file = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/employee/{staff.Id}");
        file.GetProperty("currentContract").GetProperty("contractId").GetGuid().ShouldBe(IdOf(first));

        var list = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/contract?employeeId={staff.Id}&status=active");
        list.GetProperty("totalCount").GetInt64().ShouldBe(1);
    }

    [Fact]
    public async Task HR_cannot_act_on_their_own_contract()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var configuration = await kit.Hr.GetFromJsonAsync<JsonElement>("/api/abp/application-configuration");
        var employeeId = await kit.AddFileForUserAsync(configuration.GetProperty("currentUser").GetProperty("id").GetGuid(), kit.ExecutiveEmployeeId);

        var contract = await kit.Hr.PostAsJsonAsync("/api/app/contract", new { employeeId, contractType = "A", startDate = Day(-200), endDate = Day(5) });
        var id = IdOf(await OkAsync(contract, HttpStatusCode.Created));

        await ShouldFailWithAsync(
            await kit.Hr.PostAsJsonAsync($"/api/app/contract/{id}/recommend", new { employeePosition = "p", recommendation = "renew" }),
            HrErrorCodes.Contract.SelfActionNotAllowed);
    }

    [Fact]
    public async Task Daily_task_moves_contracts_into_the_alert_window_and_notifies_once()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);

        var contract = await CreateContractAsync(kit.Hr, staff.Id, -10, 400);
        var id = IdOf(contract);

        await RunAsync(async services =>
        {
            var repository = services.GetRequiredService<Volo.Abp.Domain.Repositories.IRepository<EmploymentContract, Guid>>();
            var row = await repository.GetAsync(id);
            typeof(EmploymentContract).GetProperty(nameof(EmploymentContract.EndDate))!.SetValue(row, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(20));
            await repository.UpdateAsync(row);
        });

        var clock = _host.Factory.Services.GetRequiredService<IClock>();
        await RunAsync(services => services.GetRequiredService<ContractAlertService>().ProcessAsync(clock.Now));
        await RunAsync(services => services.GetRequiredService<ContractAlertService>().ProcessAsync(clock.Now));

        var after = await OkAsync(await kit.Hr.GetAsync($"/api/app/contract/{id}"));
        Text(after, "status").ShouldBe("pendingRecommendation");

        var expiring = await kit.Hr.GetFromJsonAsync<JsonElement>("/api/app/contract/expiring?withinDays=25&maxResultCount=200");
        expiring.GetProperty("alertDays").GetInt32().ShouldBe(25);
        expiring.GetProperty("items").EnumerateArray().Any(x => IdOf(x) == id).ShouldBeTrue();

        var notices = await kit.Hr.GetFromJsonAsync<JsonElement>("/api/app/notification?maxResultCount=200");
        notices.GetProperty("items").EnumerateArray().Count(x => x.TryGetProperty("resourceId", out var r) && r.ValueKind == JsonValueKind.String && r.GetGuid() == id).ShouldBe(1);
    }

    [Fact]
    public async Task Warning_full_path_with_objection_and_each_audience_view()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var managerClient = await kit.SignInAsync(manager);
        var staff = await kit.AddStaffAsync(manager.Id);
        var staffClient = await kit.SignInAsync(staff);

        var created = await OkAsync(await managerClient.PostAsync("/api/app/warning", WarningForm(staff.Id, file: Pdf)), HttpStatusCode.Created);
        Text(created, "stage").ShouldBe("underReview");
        created.TryGetProperty("status", out _).ShouldBeFalse();
        var id = IdOf(created);

        (await staffClient.GetAsync($"/api/app/warning/my/{id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await staffClient.GetFromJsonAsync<JsonElement>("/api/app/warning/my")).GetProperty("totalCount").GetInt64().ShouldBe(0);

        var full = await OkAsync(await kit.Hr.GetAsync($"/api/app/warning/{id}"));
        var attachmentId = IdOf(full.GetProperty("attachments")[0]);
        Text(await OkAsync(await kit.Hr.GetAsync($"/api/app/warning/{id}/attachment/{attachmentId}/download-url")), "url").ShouldContain("/files/");
        (await kit.Hr.GetAsync($"/api/app/warning/{id}/attachment/{Guid.NewGuid()}/download-url")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        await ShouldFailWithAsync(await kit.Hr.PostAsJsonAsync($"/api/app/warning/{id}/decide", new { decision = "issue", issuedLevel = "first", note = "n" }), HrErrorCodes.Warning.InvalidStatus);

        var notified = await OkAsync(await kit.Hr.PostAsync($"/api/app/warning/{id}/notify", null));
        Text(notified, "status").ShouldBe("notified");
        notified.GetProperty("responseDueDate").ValueKind.ShouldBe(JsonValueKind.String);

        await ShouldFailWithAsync(await kit.Hr.PostAsJsonAsync($"/api/app/warning/{id}/decide", new { decision = "issue", issuedLevel = "first", note = "n" }), HrErrorCodes.Warning.ResponseWindowOpen);

        var mine = await OkAsync(await staffClient.GetAsync($"/api/app/warning/my/{id}"));
        mine.GetProperty("canRespond").GetBoolean().ShouldBeTrue();
        mine.TryGetProperty("recommendedBy", out _).ShouldBeFalse();

        var other = await kit.SignInAsync(await kit.AddStaffAsync(manager.Id));
        (await other.PostAsJsonAsync($"/api/app/warning/{id}/respond", new { content = "no" })).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var responded = await OkAsync(await staffClient.PostAsJsonAsync($"/api/app/warning/{id}/respond", new { content = "My reply" }));
        Text(responded, "status").ShouldBe("responded");
        await ShouldFailWithAsync(await staffClient.PostAsJsonAsync($"/api/app/warning/{id}/respond", new { content = "again" }), HrErrorCodes.Warning.ResponseAlreadySubmitted);

        await ShouldFailWithAsync(await kit.Hr.PostAsJsonAsync($"/api/app/warning/{id}/decide", new { decision = "issue", note = "n" }), HrErrorCodes.Warning.LevelRequired);
        (await kit.Hr.PostAsJsonAsync($"/api/app/warning/{id}/decide", new { decision = "dismiss", issuedLevel = "first", note = "n" })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var issued = await OkAsync(await kit.Hr.PostAsJsonAsync($"/api/app/warning/{id}/decide", new { decision = "issue", issuedLevel = "final", note = "Verdict note" }));
        Text(issued, "status").ShouldBe("issued");

        var recommended = await managerClient.GetFromJsonAsync<JsonElement>("/api/app/warning/recommended-by-me?stage=issued&maxResultCount=200");
        var row = recommended.GetProperty("items").EnumerateArray().Single(x => IdOf(x) == id);
        Text(row, "issuedLevel").ShouldBe("final");
        row.GetRawText().ShouldNotContain("My reply");
        row.GetRawText().ShouldNotContain("Verdict note");

        var executive = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.ExecutiveDirector);
        (await executive.GetAsync($"/api/app/warning/{id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        (await OkAsync(await staffClient.GetAsync($"/api/app/warning/my/{id}"))).GetProperty("canObject").GetBoolean().ShouldBeTrue();
        var objected = await OkAsync(await staffClient.PostAsJsonAsync($"/api/app/warning/{id}/object", new { content = "I object" }));
        Text(objected, "status").ShouldBe("objected");
        await ShouldFailWithAsync(await staffClient.PostAsJsonAsync($"/api/app/warning/{id}/object", new { content = "again" }), HrErrorCodes.Warning.ObjectionAlreadySubmitted);

        (await executive.GetAsync($"/api/app/warning/{id}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        var pending = await executive.GetFromJsonAsync<JsonElement>("/api/app/warning/objection?isPending=true&maxResultCount=200");
        pending.GetProperty("items").EnumerateArray().Any(x => IdOf(x) == id).ShouldBeTrue();

        var managerFile = await managerClient.GetFromJsonAsync<JsonElement>($"/api/app/warning/employee/{staff.Id}");
        managerFile.GetProperty("totalCount").GetInt64().ShouldBe(1);

        var resolved = await OkAsync(await executive.PostAsJsonAsync($"/api/app/warning/{id}/resolve-objection", new { outcome = "overturn", note = "Cancelled" }));
        Text(resolved, "status").ShouldBe("objectionOverturned");
        await ShouldFailWithAsync(await executive.PostAsJsonAsync($"/api/app/warning/{id}/resolve-objection", new { outcome = "uphold", note = "x" }), HrErrorCodes.Warning.InvalidStatus);

        // An overturned warning stays on record for HR, and leaves the manager's view of warnings in force.
        (await managerClient.GetFromJsonAsync<JsonElement>($"/api/app/warning/employee/{staff.Id}")).GetProperty("totalCount").GetInt64().ShouldBe(0);
        (await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/warning/employee/{staff.Id}")).GetProperty("totalCount").GetInt64().ShouldBe(1);
    }

    [Fact]
    public async Task Warning_recommendation_targets_follow_the_role()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var managerClient = await kit.SignInAsync(manager);
        var otherManager = await kit.AddDepartmentManagerAsync();
        var staff = await kit.AddStaffAsync(manager.Id);
        var outsider = await kit.AddStaffAsync(otherManager.Id);
        var reviewer = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.Reviewer);

        await ShouldFailWithAsync(await managerClient.PostAsync("/api/app/warning", WarningForm(Guid.NewGuid())), HrErrorCodes.Warning.EmployeeNotFound);
        await ShouldFailWithAsync(await managerClient.PostAsync("/api/app/warning", WarningForm(manager.Id)), HrErrorCodes.Warning.SelfRecommendationNotAllowed);
        await ShouldFailWithAsync(await managerClient.PostAsync("/api/app/warning", WarningForm(outsider.Id)), HrErrorCodes.Warning.TargetNotInTeam);
        await ShouldFailWithAsync(await managerClient.PostAsync("/api/app/warning", WarningForm(otherManager.Id)), HrErrorCodes.Warning.TargetIsDepartmentManager);
        await ShouldFailWithAsync(await managerClient.PostAsync("/api/app/warning", WarningForm(staff.Id, incidentDate: Day(3))), HrErrorCodes.Warning.IncidentDateInFuture);
        await ShouldFailWithAsync(await managerClient.PostAsync("/api/app/warning", WarningForm(staff.Id, file: Encoding.ASCII.GetBytes("not a pdf"))), HrErrorCodes.Document.FileTypeNotAllowed);

        await ShouldFailWithAsync(await reviewer.PostAsync("/api/app/warning", WarningForm(staff.Id)), HrErrorCodes.Warning.TargetNotDepartmentManager);
        var ok = await OkAsync(await reviewer.PostAsync("/api/app/warning", WarningForm(manager.Id)), HttpStatusCode.Created);

        Text(await OkAsync(await kit.Hr.GetAsync($"/api/app/warning/{IdOf(ok)}")), "recommenderRole").ShouldBe("reviewer");
    }

    [Fact]
    public async Task Reply_window_expiry_and_dismissal_without_notifying_the_employee()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var managerClient = await kit.SignInAsync(manager);
        var staff = await kit.AddStaffAsync(manager.Id);
        var staffClient = await kit.SignInAsync(staff);

        var unheard = IdOf(await OkAsync(await managerClient.PostAsync("/api/app/warning", WarningForm(staff.Id)), HttpStatusCode.Created));
        var dismissed = await OkAsync(await kit.Hr.PostAsJsonAsync($"/api/app/warning/{unheard}/decide", new { decision = "dismiss", note = "No case" }));
        Text(dismissed, "status").ShouldBe("dismissed");
        (await staffClient.GetAsync($"/api/app/warning/my/{unheard}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var late = IdOf(await OkAsync(await managerClient.PostAsync("/api/app/warning", WarningForm(staff.Id)), HttpStatusCode.Created));
        await OkAsync(await kit.Hr.PostAsync($"/api/app/warning/{late}/notify", null));

        await RunAsync(async services =>
        {
            var repository = services.GetRequiredService<Volo.Abp.Domain.Repositories.IRepository<Warning, Guid>>();
            var row = await repository.GetAsync(late, includeDetails: false);
            typeof(Warning).GetProperty(nameof(Warning.ResponseDueDate))!.SetValue(row, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-5));
            await repository.UpdateAsync(row);
        });

        await ShouldFailWithAsync(await staffClient.PostAsJsonAsync($"/api/app/warning/{late}/respond", new { content = "late" }), HrErrorCodes.Warning.ResponseWindowExpired);

        await RunAsync(services => services.GetRequiredService<WarningDeadlineService>().ProcessAsync());
        Text(await OkAsync(await kit.Hr.GetAsync($"/api/app/warning/{late}")), "status").ShouldBe("responseExpired");

        var issued = await OkAsync(await kit.Hr.PostAsJsonAsync($"/api/app/warning/{late}/decide", new { decision = "issue", issuedLevel = "first", note = "Decided" }));
        Text(issued, "status").ShouldBe("issued");
    }

    [Fact]
    public async Task Appreciation_letter_scopes_and_acknowledgement()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var managerClient = await kit.SignInAsync(manager);
        var otherManager = await kit.SignInAsync(await kit.AddDepartmentManagerAsync());
        var staff = await kit.AddStaffAsync(manager.Id);
        var staffClient = await kit.SignInAsync(staff);
        var executive = await TestUsers.SignedInWithMfaAsync(_host, "en", HrRoles.Employee, HrRoles.ExecutiveDirector);

        (await managerClient.PostAsync("/api/app/appreciation-letter", LetterForm(staff.Id))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        await ShouldFailWithAsync(await executive.PostAsync("/api/app/appreciation-letter", LetterForm(Guid.NewGuid())), HrErrorCodes.Appreciation.EmployeeNotFound);
        await ShouldFailWithAsync(await executive.PostAsync("/api/app/appreciation-letter", LetterForm(staff.Id, Encoding.ASCII.GetBytes("nope"))), HrErrorCodes.Document.FileTypeNotAllowed);

        var letter = await OkAsync(await executive.PostAsync("/api/app/appreciation-letter", LetterForm(staff.Id)), HttpStatusCode.Created);
        var id = IdOf(letter);
        letter.GetProperty("isAcknowledged").GetBoolean().ShouldBeFalse();
        Text(letter.GetProperty("file"), "fileName").ShouldBe("letter.pdf");

        (await staffClient.GetFromJsonAsync<JsonElement>("/api/app/appreciation-letter/my")).GetProperty("totalCount").GetInt64().ShouldBe(1);
        (await managerClient.GetAsync($"/api/app/appreciation-letter/{id}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await otherManager.GetAsync($"/api/app/appreciation-letter/{id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await otherManager.GetAsync($"/api/app/appreciation-letter/employee/{staff.Id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await managerClient.GetAsync("/api/app/appreciation-letter")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/appreciation-letter?employeeId={staff.Id}&isAcknowledged=false")).GetProperty("totalCount").GetInt64().ShouldBe(1);
        Text(await OkAsync(await staffClient.GetAsync($"/api/app/appreciation-letter/{id}/download-url")), "url").ShouldContain("/files/");

        (await managerClient.PostAsync($"/api/app/appreciation-letter/{id}/acknowledge", null)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        static int Count(JsonElement summary, string category) =>
            summary.GetProperty("items").EnumerateArray().Where(x => Text(x, "category") == category).Sum(x => x.GetProperty("count").GetInt32());

        var inbox = await staffClient.GetFromJsonAsync<JsonElement>("/api/app/me/inbox-summary");
        Count(inbox, "appreciationLetterAcknowledgement").ShouldBe(1);

        var warned = IdOf(await OkAsync(await managerClient.PostAsync("/api/app/warning", WarningForm(staff.Id)), HttpStatusCode.Created));
        Count(await kit.Hr.GetFromJsonAsync<JsonElement>("/api/app/me/inbox-summary"), "warningDecision").ShouldBeGreaterThan(0);
        await OkAsync(await kit.Hr.PostAsync($"/api/app/warning/{warned}/notify", null));
        Count(await staffClient.GetFromJsonAsync<JsonElement>("/api/app/me/inbox-summary"), "warningResponse").ShouldBe(1);

        var first = await OkAsync(await staffClient.PostAsync($"/api/app/appreciation-letter/{id}/acknowledge", null));
        var second = await OkAsync(await staffClient.PostAsync($"/api/app/appreciation-letter/{id}/acknowledge", null));
        first.GetProperty("acknowledgedAt").GetDateTime().ShouldBe(second.GetProperty("acknowledgedAt").GetDateTime(), TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task HR_reports_list_and_export_contracts_and_warnings_without_the_description()
    {
        var kit = await EmployeeTestKit.CreateAsync(_host);
        var manager = await kit.AddDepartmentManagerAsync();
        var managerClient = await kit.SignInAsync(manager);
        var staff = await kit.AddStaffAsync(manager.Id);

        await CreateContractAsync(kit.Hr, staff.Id, -10, 400);
        await OkAsync(await managerClient.PostAsync("/api/app/warning", WarningForm(staff.Id)), HttpStatusCode.Created);

        var contracts = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/report/hr-contracts?employeeId={staff.Id}");
        contracts.GetProperty("totalCount").GetInt64().ShouldBe(1);
        Text(contracts.GetProperty("items")[0], "statusName").ShouldBe("Active");

        var warnings = await kit.Hr.GetFromJsonAsync<JsonElement>($"/api/app/report/hr-warnings?from={Day(-30)}&to={Day(0)}&employeeId={staff.Id}");
        warnings.GetProperty("totalCount").GetInt64().ShouldBe(1);
        Text(warnings.GetProperty("items")[0], "levelName").ShouldBe("First");
        warnings.GetRawText().ShouldNotContain("Late without notice");

        (await kit.Hr.GetAsync("/api/app/report/hr-warnings")).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await managerClient.GetAsync($"/api/app/report/hr-contracts")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        (await kit.Hr.GetAsync($"/api/app/report/hrContracts/export?format=excel&employeeId={staff.Id}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await kit.Hr.GetAsync($"/api/app/report/hrWarnings/export?format=excel&from={Day(-30)}&to={Day(0)}")).StatusCode.ShouldBe(HttpStatusCode.OK);
        await ShouldFailWithAsync(await kit.Hr.GetAsync("/api/app/report/hrWarnings/export?format=pdf"), HrErrorCodes.Reports.MissingFilter);
    }
}
