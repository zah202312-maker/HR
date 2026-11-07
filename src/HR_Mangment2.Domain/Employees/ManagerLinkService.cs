using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Errors;
using HR_Mangment2.Identity;
using HR_Mangment2.Notifications;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Employees;

public record ManagerChangeOutcome(Guid? PreviousManagerEmployeeId, DateOnly EffectiveFrom, int MovedPendingItemCount);

/// <summary>x-global-rules.manager-link: who may manage whom, and what changing a manager does.</summary>
public class ManagerLinkService : ITransientDependency
{
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IRepository<EmployeeManagerLink, Guid> _links;
    private readonly IIdentityUserRepository _users;
    private readonly IEnumerable<IManagerChangeEffects> _effects;
    private readonly HrNotificationPublisher _notifications;
    private readonly ICompanyClock _companyClock;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;

    public ManagerLinkService(
        IRepository<Employee, Guid> employees,
        IRepository<EmployeeManagerLink, Guid> links,
        IIdentityUserRepository users,
        IEnumerable<IManagerChangeEffects> effects,
        HrNotificationPublisher notifications,
        ICompanyClock companyClock,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant)
    {
        _employees = employees;
        _links = links;
        _users = users;
        _effects = effects;
        _notifications = notifications;
        _companyClock = companyClock;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
    }

    private static readonly string[] ExecutiveOnly = { HrRoles.ExecutiveDirector };
    private static readonly string[] SystemAdminOnly = { HrRoles.SystemAdmin };
    private static readonly string[] DepartmentManagerOnly = { HrRoles.DepartmentManager };

    /// <summary>The reviewer may report to either one (decision 2026-11-07).</summary>
    private static readonly string[] ReviewerManagers = { HrRoles.ExecutiveDirector, HrRoles.HrManager };

    /// <summary>
    /// The roles a manager may hold: holding any one of them is enough. The first rule that matches the employee's
    /// roles applies. Null means no manager at all.
    /// </summary>
    public static IReadOnlyCollection<string>? AllowedManagerRoles(IReadOnlyCollection<string> employeeRoles)
    {
        if (employeeRoles.Contains(HrRoles.ExecutiveDirector))
        {
            return null;
        }

        if (employeeRoles.Contains(HrRoles.HrManager) || employeeRoles.Contains(HrRoles.SystemAdmin))
        {
            return ExecutiveOnly;
        }

        if (employeeRoles.Contains(HrRoles.DepartmentManager))
        {
            return SystemAdminOnly;
        }

        return employeeRoles.Contains(HrRoles.Reviewer) ? ReviewerManagers : DepartmentManagerOnly;
    }

    public virtual async Task<IReadOnlyCollection<string>> GetRolesAsync(Employee employee) =>
        employee.UserId == null
            ? new[] { HrRoles.Employee }
            : await _users.GetRoleNamesAsync(employee.UserId.Value);

    /// <summary>Returns the error code that rejects this manager for the employee, or null if the link is allowed.</summary>
    /// <param name="employeeId">Null while the employee is still being created, which cannot form a cycle.</param>
    public virtual async Task<string?> FindViolationAsync(Guid? employeeId, IReadOnlyCollection<string> employeeRoles, Guid managerEmployeeId)
    {
        var allowedRoles = AllowedManagerRoles(employeeRoles);
        if (allowedRoles == null)
        {
            return HrErrorCodes.Employee.ExecutiveCannotHaveManager;
        }

        if (employeeId == managerEmployeeId)
        {
            return HrErrorCodes.Employee.CannotManageSelf;
        }

        var manager = await _employees.FindAsync(managerEmployeeId);
        if (manager == null)
        {
            return HrErrorCodes.Employee.ManagerNotFound;
        }

        if (manager.Status != EmployeeStatus.Active)
        {
            return HrErrorCodes.Employee.ManagerNotActive;
        }

        var managerRoles = await GetRolesAsync(manager);
        if (!allowedRoles.Any(role => managerRoles.Contains(role)))
        {
            return HrErrorCodes.Employee.InvalidManagerForRole;
        }

        return employeeId != null && await WouldFormCycleAsync(employeeId.Value, manager)
            ? HrErrorCodes.Employee.ManagerCycle
            : null;
    }

    /// <summary>Opens the first link of a new employee.</summary>
    public virtual async Task StartAsync(Employee employee, Guid managerEmployeeId, DateOnly fromDate)
    {
        employee.SetManager(managerEmployeeId);
        await _links.InsertAsync(new EmployeeManagerLink(_guidGenerator.Create(), _currentTenant.Id, employee.Id, managerEmployeeId, fromDate, null));
    }

    /// <summary>Closes the current link, opens a new one from today, moves pending items and notifies both people.</summary>
    public virtual async Task<ManagerChangeOutcome> ChangeAsync(Employee employee, Employee newManager, string? reason)
    {
        var previous = employee.ManagerEmployeeId;
        var today = await _companyClock.GetTodayAsync();

        await CloseCurrentLinkAsync(employee.Id, today);
        await _links.InsertAsync(new EmployeeManagerLink(_guidGenerator.Create(), _currentTenant.Id, employee.Id, newManager.Id, today, reason), autoSave: true);

        employee.SetManager(newManager.Id);
        var moved = 0;
        foreach (var effect in _effects)
        {
            moved += await effect.MovePendingItemsAsync(employee.Id, previous, newManager.Id);
        }

        foreach (var userId in new[] { employee.UserId, newManager.UserId })
        {
            if (userId != null)
            {
                await _notifications.PublishAsync(userId.Value, NotificationTemplateCode.ManagerChanged);
            }
        }

        return new ManagerChangeOutcome(previous, today, moved);
    }

    public virtual async Task CloseCurrentLinkAsync(Guid employeeId, DateOnly toDate)
    {
        var current = await _links.FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.ToDate == null);
        current?.Close(toDate);
        if (current != null)
        {
            await _links.UpdateAsync(current, autoSave: true);
        }
    }

    private async Task<bool> WouldFormCycleAsync(Guid employeeId, Employee manager)
    {
        var seen = new HashSet<Guid>();
        var cursor = manager;

        while (cursor?.ManagerEmployeeId != null && seen.Add(cursor.Id))
        {
            if (cursor.ManagerEmployeeId == employeeId)
            {
                return true;
            }

            cursor = await _employees.FindAsync(cursor.ManagerEmployeeId.Value);
        }

        return false;
    }
}
