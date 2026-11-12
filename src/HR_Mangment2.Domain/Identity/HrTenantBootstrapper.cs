using System;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Departments;
using HR_Mangment2.Employees;
using HR_Mangment2.Permissions;
using HR_Mangment2.Requests;
using HR_Mangment2.WorkShifts;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;
using IdentityRole = Volo.Abp.Identity.IdentityRole;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace HR_Mangment2.Identity;

/// <summary>
/// First-run data of the company tenant: the eight roles with their contract permissions, the three
/// accounts from QUESTIONS.md item 2, and the framework admin account switched off (document section 7, note 4).
/// It is safe to run on every migration.
/// </summary>
public class HrTenantBootstrapper : ITransientDependency
{
    public const string AdminUserName = "admin";
    private const string DefaultDepartmentNameEn = "General Administration";
    private const string DefaultShiftNameEn = "Default Shift";

    /// <summary>
    /// Permissions taken away from a role after it was first seeded. The data seeder only adds grants,
    /// so these are removed explicitly. Evaluation criteria and grades moved from the system admin to
    /// the HR manager (Zainab, 2026-11-10), then their management was dropped entirely (Zainab, 2026-11-11),
    /// so the old names are kept here as literals to clear grants left in existing databases.
    /// </summary>
    private static readonly (string Permission, string Role)[] RevokedDefaults =
    {
        ("Hr.Evaluations.Criteria.Manage", HrRoles.SystemAdmin),
        ("Hr.Evaluations.Grades.Manage", HrRoles.SystemAdmin),
        ("Hr.Evaluations.Criteria.Manage", HrRoles.HrManager),
        ("Hr.Evaluations.Grades.Manage", HrRoles.HrManager),
        (HrPermissions.EmployeePermissions.ReadTeam, HrRoles.SystemAdmin)
    };

    private static readonly (string UserName, string Role, string EmailSetting)[] FirstAccounts =
    {
        ("executive", HrRoles.ExecutiveDirector, "Seed:ExecutiveEmail"),
        ("hrmanager", HrRoles.HrManager, "Seed:HrManagerEmail"),
        ("sysadmin", HrRoles.SystemAdmin, "Seed:SystemAdminEmail")
    };

    private readonly IdentityRoleManager _roleManager;
    private readonly IdentityUserManager _userManager;
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IPermissionGrantRepository _permissionGrants;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HrTenantBootstrapper> _logger;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IRepository<EmployeeShiftAssignment, Guid> _shiftAssignments;
    private readonly IRepository<Department, Guid> _departments;
    private readonly IRepository<WorkShift, Guid> _shifts;
    private readonly ManagerLinkService _managerLinks;
    private readonly ICompanyClock _companyClock;
    private readonly IRepository<RequestType, Guid> _requestTypes;
    private readonly IRepository<HR_Mangment2.Evaluations.EvaluationCriterion, Guid> _criteria;
    private readonly IRepository<HR_Mangment2.Evaluations.EvaluationGrade, Guid> _grades;

    public HrTenantBootstrapper(
        IRepository<HR_Mangment2.Evaluations.EvaluationCriterion, Guid> criteria,
        IRepository<HR_Mangment2.Evaluations.EvaluationGrade, Guid> grades,
        IRepository<RequestType, Guid> requestTypes,
        IRepository<Employee, Guid> employees,
        IRepository<EmployeeShiftAssignment, Guid> shiftAssignments,
        IRepository<Department, Guid> departments,
        IRepository<WorkShift, Guid> shifts,
        ManagerLinkService managerLinks,
        ICompanyClock companyClock,
        IdentityRoleManager roleManager,
        IdentityUserManager userManager,
        IIdentityRoleRepository roleRepository,
        IPermissionDataSeeder permissionDataSeeder,
        IPermissionGrantRepository permissionGrants,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IConfiguration configuration,
        ILogger<HrTenantBootstrapper> logger)
    {
        _criteria = criteria;
        _grades = grades;
        _requestTypes = requestTypes;
        _employees = employees;
        _shiftAssignments = shiftAssignments;
        _departments = departments;
        _shifts = shifts;
        _managerLinks = managerLinks;
        _companyClock = companyClock;
        _roleManager = roleManager;
        _userManager = userManager;
        _roleRepository = roleRepository;
        _permissionDataSeeder = permissionDataSeeder;
        _permissionGrants = permissionGrants;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _configuration = configuration;
        _logger = logger;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedFirstAccountsAsync();
        await SeedFirstEmployeeFilesAsync();
        await SeedRequestTypesAsync();
        await SeedEvaluationScaleAsync();
        await DisableFrameworkAdminAsync();
    }

    /// <summary>Only into an empty table, so a criterion or grade the HR manager renamed or stopped is never brought back.</summary>
    private async Task SeedEvaluationScaleAsync()
    {
        if (!await _criteria.AnyAsync())
        {
            var order = 1;
            foreach (var (nameAr, nameEn) in new[]
                     {
                         ("الالتزام بالدوام", "Attendance commitment"),
                         ("جودة العمل", "Quality of work"),
                         ("إنجاز المهام", "Task completion"),
                         ("التعاون والتواصل", "Cooperation and communication"),
                         ("المسؤولية والمبادرة", "Responsibility and initiative")
                     })
            {
                await _criteria.InsertAsync(new HR_Mangment2.Evaluations.EvaluationCriterion(_guidGenerator.Create(), _currentTenant.Id, nameAr, nameEn, order++, true), autoSave: true);
            }
        }

        if (!await _grades.AnyAsync())
        {
            var order = 1;
            foreach (var (code, nameAr, nameEn) in new[]
                     {
                         ("weak", "ضعيف", "Weak"),
                         ("acceptable", "مقبول", "Acceptable"),
                         ("good", "جيد", "Good"),
                         ("veryGood", "جيد جدًا", "Very good"),
                         ("excellent", "ممتاز", "Excellent")
                     })
            {
                await _grades.InsertAsync(new HR_Mangment2.Evaluations.EvaluationGrade(_guidGenerator.Create(), _currentTenant.Id, code, nameAr, nameEn, order++, true), autoSave: true);
            }
        }
    }

    /// <summary>The store type starts off, as the contract requires before release 2. The system admin turns it on from the request types screen.</summary>
    private async Task SeedRequestTypesAsync()
    {
        foreach (var (code, nameAr, nameEn, destination, isActive) in new[]
                 {
                     (RequestTypeCodes.Leave, "إجازة", "Leave", RequestDestination.None, true),
                     (RequestTypeCodes.AttendanceCorrection, "تصحيح بصمة", "Attendance correction", RequestDestination.None, true),
                     (RequestTypeCodes.StoreItem, "قطعة من المخزن", "Store item", RequestDestination.Store, false),
                     (RequestTypeCodes.Accountant, "طلب من المحاسب", "Accountant request", RequestDestination.Accountant, true),
                     (RequestTypeCodes.AdministrativeNeed, "احتياج إداري", "Administrative need", RequestDestination.None, true)
                 })
        {
            if (!await _requestTypes.AnyAsync(x => x.Code == code))
            {
                await _requestTypes.InsertAsync(
                    new RequestType(_guidGenerator.Create(), _currentTenant.Id, code, nameAr, nameEn, destination, isActive, isSystem: true),
                    autoSave: true);
            }
        }
    }

    /// <summary>
    /// Gives the first accounts their files, so the manager chain has a top: new department managers link to the
    /// system admin, and the HR manager to the executive. The executive and the system admin have no manager, and
    /// none of the three has a shift (decisions of 2026-11-10). The default shift is still created for the employees
    /// HR adds. Names and titles are placeholders that HR corrects (decision Q15).
    /// </summary>
    private async Task SeedFirstEmployeeFilesAsync()
    {
        await GetDefaultShiftAsync();
        var executive = await EnsureFileAsync("executive", "المدير التنفيذي", "Executive Director", null, withShift: false);
        await EnsureFileAsync("hrmanager", "مدير الموارد البشرية", "HR Manager", executive, withShift: false);
        await EnsureFileAsync("sysadmin", "مسؤول النظام", "System Administrator", null, withShift: false);
        await DetachRolesOutsideScheduleAsync();
        await RemoveEmployeeRoleFromSystemAdminsAsync();
        await MoveSystemAdminReportsToHrManagerAsync();
        await MoveAccountantsAndStoreOfficersToExecutiveAsync(executive);
    }

    /// <summary>
    /// Decision 2026-11-11: the accountant and the store officer report to the executive director and have no shift.
    /// Whoever holds either role (and not a role with its own manager row first) moves to the executive's file, with
    /// the usual effects of a manager change, and their current shift closes.
    /// </summary>
    private async Task MoveAccountantsAndStoreOfficersToExecutiveAsync(Employee? executive)
    {
        if (executive == null)
        {
            return;
        }

        var seen = new System.Collections.Generic.HashSet<Guid>();
        foreach (var roleName in new[] { HrRoles.Accountant, HrRoles.StoreOfficer })
        {
            foreach (var user in await _userManager.GetUsersInRoleAsync(roleName))
            {
                if (!seen.Add(user.Id))
                {
                    continue;
                }

                var employee = await _employees.FirstOrDefaultAsync(x => x.UserId == user.Id && x.Status != EmployeeStatus.Ended);
                if (employee == null || employee.Id == executive.Id)
                {
                    continue;
                }

                var roles = (await _userManager.GetRolesAsync(user)).ToList();
                var allowed = ManagerLinkService.AllowedManagerRoles(roles);
                if (allowed != null && allowed.Count == 1 && allowed.Contains(HrRoles.ExecutiveDirector) && employee.ManagerEmployeeId != executive.Id)
                {
                    await _managerLinks.ChangeAsync(employee, executive, "The accountant and the store officer report to the executive director");
                }

                await _managerLinks.DetachFromScheduleAsync(employee, roles, await _companyClock.GetTodayAsync());
                await _employees.UpdateAsync(employee, autoSave: true);
            }
        }
    }

    /// <summary>
    /// Decision 2026-11-10: department managers report to the HR manager. Anyone still linked to a system admin moves
    /// to the active HR manager's file, with the usual effects of a manager change.
    /// </summary>
    private async Task MoveSystemAdminReportsToHrManagerAsync()
    {
        var hrUser = (await _userManager.GetUsersInRoleAsync(HrRoles.HrManager)).FirstOrDefault(x => x.IsActive);
        var hrFile = hrUser == null ? null : await _employees.FirstOrDefaultAsync(x => x.UserId == hrUser.Id && x.Status == EmployeeStatus.Active);
        if (hrFile == null)
        {
            return;
        }

        foreach (var admin in await _userManager.GetUsersInRoleAsync(HrRoles.SystemAdmin))
        {
            var adminFile = await _employees.FirstOrDefaultAsync(x => x.UserId == admin.Id);
            if (adminFile == null)
            {
                continue;
            }

            foreach (var report in await _employees.GetListAsync(x => x.ManagerEmployeeId == adminFile.Id && x.Status != EmployeeStatus.Ended))
            {
                if (report.Id == hrFile.Id)
                {
                    continue;
                }

                await _managerLinks.ChangeAsync(report, hrFile, "Department managers report to the HR manager");
                await _employees.UpdateAsync(report, autoSave: true);
            }
        }
    }

    /// <summary>
    /// Databases seeded before 2026-11-10 gave the system admin the employee role. A system admin with no other
    /// role loses it, as account changes now do.
    /// </summary>
    private async Task RemoveEmployeeRoleFromSystemAdminsAsync()
    {
        foreach (var user in await _userManager.GetUsersInRoleAsync(HrRoles.SystemAdmin))
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(HrRoles.Employee) && roles.All(x => x == HrRoles.SystemAdmin || x == HrRoles.Employee))
            {
                CheckResult(await _userManager.RemoveFromRoleAsync(user, HrRoles.Employee));
            }
        }
    }

    /// <summary>
    /// Databases seeded before 2026-11-10 linked the system admin to the executive, and gave the system admin and the
    /// HR manager a shift. What these roles may no longer have is closed today.
    /// </summary>
    private async Task DetachRolesOutsideScheduleAsync()
    {
        var today = await _companyClock.GetTodayAsync();
        var seen = new System.Collections.Generic.HashSet<Guid>();
        foreach (var roleName in new[] { HrRoles.SystemAdmin, HrRoles.HrManager })
        {
            foreach (var user in await _userManager.GetUsersInRoleAsync(roleName))
            {
                if (!seen.Add(user.Id))
                {
                    continue;
                }

                var employee = await _employees.FirstOrDefaultAsync(x => x.UserId == user.Id);
                if (employee != null &&
                    await _managerLinks.DetachFromScheduleAsync(employee, (await _userManager.GetRolesAsync(user)).ToList(), today))
                {
                    await _employees.UpdateAsync(employee, autoSave: true);
                }
            }
        }
    }

    private async Task<Employee?> EnsureFileAsync(string userName, string nameAr, string nameEn, Employee? manager, bool withShift)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return null;
        }

        var existing = await _employees.FirstOrDefaultAsync(x => x.UserId == user.Id);
        if (existing != null)
        {
            return existing;
        }

        var today = await _companyClock.GetTodayAsync();
        var department = await GetDefaultDepartmentAsync();
        var employee = new Employee(_guidGenerator.Create(), _currentTenant.Id, user.Id);
        employee.SetProfile(nameAr, nameEn, user.GetRealEmail(), null, null, null, department.Id, nameAr, today, null, null);

        if (manager != null)
        {
            await _managerLinks.StartAsync(employee, manager.Id, today);
        }

        if (withShift)
        {
            var shift = await GetDefaultShiftAsync();
            await _shiftAssignments.InsertAsync(new EmployeeShiftAssignment(_guidGenerator.Create(), _currentTenant.Id, employee.Id, shift.Id, today));
        }

        return await _employees.InsertAsync(employee, autoSave: true);
    }

    private async Task<Department> GetDefaultDepartmentAsync() =>
        await _departments.FirstOrDefaultAsync(x => x.NameEn == DefaultDepartmentNameEn)
        ?? await _departments.InsertAsync(new Department(_guidGenerator.Create(), _currentTenant.Id, "الإدارة العامة", DefaultDepartmentNameEn), autoSave: true);

    private async Task<WorkShift> GetDefaultShiftAsync()
    {
        var existing = await _shifts.FirstOrDefaultAsync(x => x.NameEn == DefaultShiftNameEn);
        if (existing != null)
        {
            return existing;
        }

        var start = new TimeOnly(8, 0);
        var end = new TimeOnly(16, 0);
        var days = new[]
            {
                WorkShiftDayOfWeek.Sunday, WorkShiftDayOfWeek.Monday, WorkShiftDayOfWeek.Tuesday,
                WorkShiftDayOfWeek.Wednesday, WorkShiftDayOfWeek.Thursday
            }
            .Select(day => new WorkShiftDay(day, null, start, end, false))
            .ToList();

        var shift = new WorkShift(_guidGenerator.Create(), _currentTenant.Id);
        shift.Set("الدوام الافتراضي", DefaultShiftNameEn, WorkShiftType.Fixed, 10, WorkShiftOvertimePolicy.None, null, null, days);
        return await _shifts.InsertAsync(shift, autoSave: true);
    }

    private async Task SeedRolesAsync()
    {
        foreach (var roleName in HrRoles.All)
        {
            if (await _roleRepository.FindByNormalizedNameAsync(_roleManager.NormalizeKey(roleName)) == null)
            {
                var role = new IdentityRole(_guidGenerator.Create(), roleName, _currentTenant.Id) { IsStatic = true, IsPublic = true };
                CheckResult(await _roleManager.CreateAsync(role));
            }

            var permissions = HrPermissions.DefaultRoles
                .Where(entry => entry.Value.Contains(roleName))
                .Select(entry => entry.Key)
                .ToList();

            await _permissionDataSeeder.SeedAsync(RolePermissionValueProvider.ProviderName, roleName, permissions, _currentTenant.Id);
        }

        foreach (var (permission, roleName) in RevokedDefaults)
        {
            var grant = await _permissionGrants.FindAsync(permission, RolePermissionValueProvider.ProviderName, roleName);
            if (grant != null)
            {
                await _permissionGrants.DeleteAsync(grant, autoSave: true);
            }
        }
    }

    private async Task SeedFirstAccountsAsync()
    {
        var password = _configuration["Seed:InitialPassword"];
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Seed:InitialPassword is not configured. Set it in appsettings.secrets.json.");
        }

        foreach (var (userName, role, emailSetting) in FirstAccounts)
        {
            if (await _userManager.FindByNameAsync(userName) != null)
            {
                continue;
            }

            var email = _configuration[emailSetting];
            var user = new IdentityUser(
                _guidGenerator.Create(),
                userName,
                string.IsNullOrWhiteSpace(email) ? HrUserProperties.PlaceholderEmail(userName) : email,
                _currentTenant.Id);
            user.SetMustChangePassword(true);

            CheckResult(await _userManager.CreateAsync(user, password, validatePassword: false));
            // The system admin is not a company employee (decision 2026-11-10).
            CheckResult(await _userManager.AddToRolesAsync(user, role == HrRoles.SystemAdmin ? new[] { role } : new[] { HrRoles.Employee, role }));
            _logger.LogInformation("Created first account {UserName}.", userName);
        }
    }

    private async Task DisableFrameworkAdminAsync()
    {
        var admin = await _userManager.FindByNameAsync(AdminUserName);
        if (admin is { IsActive: true })
        {
            admin.SetIsActive(false);
            CheckResult(await _userManager.UpdateAsync(admin));
        }
    }

    private static void CheckResult(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Code)));
        }
    }
}
