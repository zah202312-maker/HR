using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Departments;
using HR_Mangment2.Identity;
using HR_Mangment2.Localization;
using HR_Mangment2.WorkShifts;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Linq;

namespace HR_Mangment2.Employees;

/// <summary>Builds the small reference shapes that many screens show: employee, department, user and current shift.</summary>
public class EmployeeReferenceReader : ITransientDependency
{
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IRepository<Department, Guid> _departments;
    private readonly IRepository<EmployeeShiftAssignment, Guid> _assignments;
    private readonly IRepository<WorkShift, Guid> _shifts;
    private readonly IIdentityUserRepository _users;
    private readonly IAsyncQueryableExecuter _executer;
    private readonly ProfilePhotoReader _photos;

    public EmployeeReferenceReader(
        IRepository<Employee, Guid> employees,
        IRepository<Department, Guid> departments,
        IRepository<EmployeeShiftAssignment, Guid> assignments,
        IRepository<WorkShift, Guid> shifts,
        IIdentityUserRepository users,
        IAsyncQueryableExecuter executer,
        ProfilePhotoReader photos)
    {
        _employees = employees;
        _departments = departments;
        _assignments = assignments;
        _shifts = shifts;
        _users = users;
        _executer = executer;
        _photos = photos;
    }

    public static string FullName(Employee employee) =>
        LocalizedName.Pick(employee.FullNameAr, employee.FullNameEn ?? employee.FullNameAr);

    public virtual async Task<Dictionary<Guid, DepartmentRefDto>> GetDepartmentsAsync(IEnumerable<Guid> ids)
    {
        var distinct = ids.Distinct().ToList();
        var rows = await _departments.GetListAsync(x => distinct.Contains(x.Id));
        return rows.ToDictionary(x => x.Id, x => new DepartmentRefDto { Id = x.Id, Name = LocalizedName.Pick(x.NameAr, x.NameEn) });
    }

    public virtual async Task<Dictionary<Guid, EmployeeRefDto>> GetRefsAsync(IEnumerable<Guid> employeeIds)
    {
        var distinct = employeeIds.Distinct().ToList();
        return await ToRefsAsync(await _employees.GetListAsync(x => distinct.Contains(x.Id)));
    }

    public virtual async Task<Dictionary<Guid, EmployeeRefDto>> ToRefsAsync(IReadOnlyCollection<Employee> employees)
    {
        var departments = await GetDepartmentsAsync(employees.Select(x => x.DepartmentId));
        var photos = await GetPhotoIdsAsync(employees);
        return employees.ToDictionary(x => x.Id, x => new EmployeeRefDto
        {
            Id = x.Id,
            FullName = FullName(x),
            DepartmentName = departments.GetValueOrDefault(x.DepartmentId)?.Name,
            PhotoId = PhotoOf(x, photos)
        });
    }

    public virtual async Task<EmployeeRefDto> ToRefAsync(Employee employee) =>
        (await ToRefsAsync(new[] { employee }))[employee.Id];

    /// <summary>The name on the employee file when there is one, otherwise the account's own name.</summary>
    public virtual async Task<Dictionary<Guid, UserRefDto>> GetUserRefsAsync(IEnumerable<Guid> userIds)
    {
        var distinct = userIds.Distinct().ToList();
        if (distinct.Count == 0)
        {
            return new Dictionary<Guid, UserRefDto>();
        }

        var files = await _employees.GetListAsync(x => x.UserId != null && distinct.Contains(x.UserId.Value));
        var result = files.ToDictionary(x => x.UserId!.Value, x => new UserRefDto { Id = x.UserId!.Value, DisplayName = FullName(x) });

        var remaining = distinct.Where(id => !result.ContainsKey(id)).ToList();
        if (remaining.Count > 0)
        {
            foreach (var user in await _users.GetListByIdsAsync(remaining))
            {
                result[user.Id] = new UserRefDto { Id = user.Id, DisplayName = string.IsNullOrWhiteSpace(user.Name) ? user.UserName : user.Name };
            }
        }

        var photos = await _photos.GetPhotoIdsAsync(result.Keys);
        foreach (var reference in result.Values)
        {
            reference.PhotoId = photos.TryGetValue(reference.Id, out var photoId) ? photoId : null;
        }

        return result;
    }

    /// <summary>The account photo of each employee file, keyed by user id (decision 2026-11-11).</summary>
    public virtual Task<Dictionary<Guid, Guid>> GetPhotoIdsAsync(IEnumerable<Employee> employees) =>
        _photos.GetPhotoIdsAsync(employees.Where(x => x.UserId != null).Select(x => x.UserId!.Value));

    public static Guid? PhotoOf(Employee employee, IReadOnlyDictionary<Guid, Guid> photos) =>
        employee.UserId != null && photos.TryGetValue(employee.UserId.Value, out var id) ? id : null;

    public virtual async Task<Dictionary<Guid, EmployeeShiftSummaryDto>> GetCurrentShiftsAsync(IEnumerable<Guid> employeeIds)
    {
        var distinct = employeeIds.Distinct().ToList();
        var current = await _assignments.GetListAsync(x => distinct.Contains(x.EmployeeId) && x.EndDate == null);
        var shiftIds = current.Select(x => x.WorkShiftId).Distinct().ToList();
        var shifts = (await _executer.ToListAsync((await _shifts.GetQueryableAsync()).Where(x => shiftIds.Contains(x.Id)))).ToDictionary(x => x.Id);

        return current.ToDictionary(x => x.EmployeeId, x => new EmployeeShiftSummaryDto
        {
            WorkShiftId = x.WorkShiftId,
            Name = LocalizedName.Pick(shifts[x.WorkShiftId].NameAr, shifts[x.WorkShiftId].NameEn),
            EffectiveFrom = x.StartDate
        });
    }
}
