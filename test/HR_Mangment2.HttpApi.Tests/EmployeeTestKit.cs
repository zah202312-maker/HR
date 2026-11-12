using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;

namespace HR_Mangment2;

public record CreatedEmployee(Guid Id, Guid UserId, string UserName, string TemporaryPassword, string Stamp);

/// <summary>Builds the chain the manager rules need, starting from the seeded accounts: executive, system admin, department manager, employee.</summary>
public class EmployeeTestKit
{
    public const string NewPassword = "Changed-Test-1a!";

    private readonly HostFixture _host;

    private EmployeeTestKit(HostFixture host, HttpClient hr, JsonElement seededAdmin, Guid defaultShiftId, Guid executiveId, Guid hrManagerId)
    {
        _host = host;
        Hr = hr;
        SystemAdminEmployeeId = seededAdmin.GetProperty("id").GetGuid();
        DepartmentId = seededAdmin.GetProperty("department").GetProperty("id").GetGuid();
        ShiftId = defaultShiftId;
        ExecutiveEmployeeId = executiveId;
        HrManagerEmployeeId = hrManagerId;
    }

    public HttpClient Hr { get; }
    public Guid SystemAdminEmployeeId { get; }
    public Guid ExecutiveEmployeeId { get; }

    /// <summary>The seeded HR manager's file: department managers report to it (decision 2026-11-10).</summary>
    public Guid HrManagerEmployeeId { get; }
    public Guid DepartmentId { get; }
    public Guid ShiftId { get; }

    public static async Task<EmployeeTestKit> CreateAsync(HostFixture host)
    {
        var hr = await TestUsers.SignedInWithMfaAsync(host, "en", HrRoles.Employee, HrRoles.HrManager);
        // The seeded files are not on the employees list (decision 2026-11-10), so they are found by account.
        var admin = await hr.GetFromJsonAsync<JsonElement>($"/api/app/employee/{await FindSeededEmployeeIdAsync(host, "sysadmin")}");
        var executiveId = await FindSeededEmployeeIdAsync(host, "executive");
        var hrManagerId = await FindSeededEmployeeIdAsync(host, "hrmanager");

        // None of the seeded files has a shift (decisions of 2026-11-10), so the seeded default shift is read directly.
        var defaultShiftId = await FindDefaultShiftIdAsync(host);

        return new EmployeeTestKit(host, hr, admin, defaultShiftId, executiveId, hrManagerId);
    }

    /// <summary>The employee file of a seeded account (executive, hrmanager, sysadmin).</summary>
    public static async Task<Guid> FindSeededEmployeeIdAsync(HostFixture host, string userName)
    {
        using var scope = host.Factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var tenant = await services.GetRequiredService<ITenantRepository>().FindByNameAsync(
            services.GetRequiredService<ITenantNormalizer>().NormalizeName(HostFixture.TestTenantName)!);
        using var change = services.GetRequiredService<ICurrentTenant>().Change(tenant!.Id);
        using var uow = services.GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true);

        var user = await services.GetRequiredService<Volo.Abp.Identity.IdentityUserManager>().FindByNameAsync(userName)
                   ?? throw new InvalidOperationException($"The seeded account {userName} is missing.");
        var employee = await services.GetRequiredService<IRepository<Employee, Guid>>().FirstAsync(x => x.UserId == user.Id);
        return employee.Id;
    }

    private static async Task<Guid> FindDefaultShiftIdAsync(HostFixture host)
    {
        using var scope = host.Factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var tenant = await services.GetRequiredService<ITenantRepository>().FindByNameAsync(
            services.GetRequiredService<ITenantNormalizer>().NormalizeName(HostFixture.TestTenantName)!);
        using var change = services.GetRequiredService<ICurrentTenant>().Change(tenant!.Id);
        using var uow = services.GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true);

        var shift = await services.GetRequiredService<IRepository<HR_Mangment2.WorkShifts.WorkShift, Guid>>().FirstAsync(x => x.NameEn == "Default Shift");
        return shift.Id;
    }

    /// <summary>Every employee needs an address of their own.</summary>
    public static string UniqueEmail() => "s" + Guid.NewGuid().ToString("N")[..12] + "@test.example";

    public object Body(Guid? managerId, string[] roles, string? email = null, string? userName = null, Guid? shiftId = null) => new
    {
        fullNameAr = "موظف " + Guid.NewGuid().ToString("N")[..6],
        fullNameEn = "Staff " + Guid.NewGuid().ToString("N")[..6],
        email = email ?? UniqueEmail(),
        departmentId = DepartmentId,
        jobTitle = "Analyst",
        hireDate = "2025-01-15",
        workShiftId = shiftId ?? ShiftId,
        managerEmployeeId = managerId,
        userName = userName ?? "s" + Guid.NewGuid().ToString("N")[..12],
        roleNames = roles,
        employeeNumber = "E-" + Guid.NewGuid().ToString("N")[..12]
    };

    public async Task<CreatedEmployee> AddAsync(Guid managerId, string[] roles, string? email = null, Guid? shiftId = null)
    {
        // Decision 2026-11-11: the accountant and the store officer report to the executive director (unless a
        // department manager row applies first), whatever manager the test had in mind.
        if ((roles.Contains(HrRoles.Accountant) || roles.Contains(HrRoles.StoreOfficer)) && !roles.Contains(HrRoles.DepartmentManager) && !roles.Contains(HrRoles.Reviewer))
        {
            managerId = ExecutiveEmployeeId;
        }

        var response = await Hr.PostAsJsonAsync("/api/app/employee", Body(managerId, roles, email, shiftId: shiftId));
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<JsonElement>();
        var employee = created.GetProperty("employee");
        var credentials = created.GetProperty("credentials");

        return new CreatedEmployee(
            employee.GetProperty("id").GetGuid(),
            credentials.GetProperty("userId").GetGuid(),
            credentials.GetProperty("userName").GetString()!,
            credentials.GetProperty("temporaryPassword").GetString()!,
            employee.GetProperty("concurrencyStamp").GetString()!);
    }

    public Task<CreatedEmployee> AddDepartmentManagerAsync() => AddAsync(HrManagerEmployeeId, new[] { HrRoles.DepartmentManager });

    public Task<CreatedEmployee> AddStaffAsync(Guid managerId, string? email = null) => AddAsync(managerId, new[] { HrRoles.Employee }, email);

    /// <summary>Signs in with the temporary password and completes the forced change, as a new employee would.</summary>
    public async Task<HttpClient> SignInAsync(CreatedEmployee employee, string language = "en")
    {
        var token = await TestAuth.SignInAsync(_host, employee.UserName, employee.TemporaryPassword);
        var client = TestAuth.ClientFor(_host, token.AccessToken, language);
        var change = await client.PostAsJsonAsync("/api/app/account/change-password", new { currentPassword = employee.TemporaryPassword, newPassword = NewPassword });
        change.EnsureSuccessStatusCode();
        return client;
    }

    /// <summary>A new session for someone who already went through <see cref="SignInAsync"/>.</summary>
    public async Task<HttpClient> SignInAgainAsync(CreatedEmployee employee, string language)
    {
        var token = await TestAuth.SignInAsync(_host, employee.UserName, NewPassword);
        return TestAuth.ClientFor(_host, token.AccessToken, language);
    }

    public async Task<long> CountDocumentAccessAsync(Guid documentId)
    {
        using var scope = _host.Factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var tenant = await services.GetRequiredService<ITenantRepository>().FindByNameAsync(
            services.GetRequiredService<ITenantNormalizer>().NormalizeName(HostFixture.TestTenantName)!);
        using var change = services.GetRequiredService<ICurrentTenant>().Change(tenant!.Id);
        using var uow = services.GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true);

        return await services.GetRequiredService<IRepository<EmployeeDocumentAccessLog, Guid>>().CountAsync(x => x.DocumentId == documentId);
    }

    /// <summary>Accounts from before email became required still exist, and only they can have no address.</summary>
    public async Task MakeOldAccountWithoutEmailAsync(Guid userId)
    {
        using var scope = _host.Factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var tenant = await services.GetRequiredService<ITenantRepository>().FindByNameAsync(
            services.GetRequiredService<ITenantNormalizer>().NormalizeName(HostFixture.TestTenantName)!);
        using var change = services.GetRequiredService<ICurrentTenant>().Change(tenant!.Id);
        using var uow = services.GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true);
        var users = services.GetRequiredService<Volo.Abp.Identity.IdentityUserManager>();
        var user = await users.GetByIdAsync(userId);
        if (!(await users.SetEmailAsync(user, HrUserProperties.PlaceholderEmail(user.UserName))).Succeeded)
        {
            throw new InvalidOperationException("Could not clear the test account's email.");
        }

        await uow.CompleteAsync();
    }

    /// <summary>Gives a directly created test user a file, for owners the HTTP rules would not let HR create, such as a second executive.</summary>
    public async Task<Guid> AddFileForUserAsync(Guid userId, Guid? managerId)
    {
        using var scope = _host.Factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var tenant = await services.GetRequiredService<ITenantRepository>().FindByNameAsync(
            services.GetRequiredService<ITenantNormalizer>().NormalizeName(HostFixture.TestTenantName)!);
        using var change = services.GetRequiredService<ICurrentTenant>().Change(tenant!.Id);
        using var uow = services.GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true);

        var employee = new Employee(Guid.NewGuid(), tenant.Id, userId);
        employee.SetProfile("ملف اختبار", "Test File", null, null, null, null, DepartmentId, "Analyst", new DateOnly(2025, 1, 15), null, null);
        if (managerId != null)
        {
            employee.SetManager(managerId.Value);
        }

        await services.GetRequiredService<IRepository<Employee, Guid>>().InsertAsync(employee);
        await uow.CompleteAsync();
        return employee.Id;
    }

    /// <summary>A file with no account, which only data from before this module or a direct insert can produce.</summary>
    public async Task<Guid> AddFileWithoutAccountAsync(Guid managerId, string? email = null)
    {
        using var scope = _host.Factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var tenant = await services.GetRequiredService<ITenantRepository>().FindByNameAsync(
            services.GetRequiredService<ITenantNormalizer>().NormalizeName(HostFixture.TestTenantName)!);
        using var change = services.GetRequiredService<ICurrentTenant>().Change(tenant!.Id);
        using var uow = services.GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true);

        var employee = new Employee(Guid.NewGuid(), tenant.Id, null);
        employee.SetProfile("بدون حساب", "No Account", email, null, null, null, DepartmentId, "Analyst", new DateOnly(2025, 1, 15), null, null);
        employee.SetManager(managerId);
        await services.GetRequiredService<IRepository<Employee, Guid>>().InsertAsync(employee);

        await uow.CompleteAsync();
        return employee.Id;
    }
}
