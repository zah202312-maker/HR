using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Attendance;
using HR_Mangment2.Errors;
using HR_Mangment2.Identity;
using HR_Mangment2.Localization;
using HR_Mangment2.Permissions;
using HR_Mangment2.WorkShifts;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace HR_Mangment2.Employees;

[Authorize]
public class EmployeeShiftAppService : HR_Mangment2AppService, IEmployeeShiftAppService
{
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IRepository<EmployeeShiftAssignment, Guid> _assignments;
    private readonly IRepository<WorkShift, Guid> _shifts;
    private readonly ManagerLinkService _managerLinks;
    private readonly EmployeeReferenceReader _references;
    private readonly IAttendanceRecalculator _attendance;
    private readonly ICompanyClock _companyClock;

    public EmployeeShiftAppService(
        IRepository<Employee, Guid> employees,
        IRepository<EmployeeShiftAssignment, Guid> assignments,
        IRepository<WorkShift, Guid> shifts,
        ManagerLinkService managerLinks,
        EmployeeReferenceReader references,
        IAttendanceRecalculator attendance,
        ICompanyClock companyClock)
    {
        _employees = employees;
        _assignments = assignments;
        _shifts = shifts;
        _managerLinks = managerLinks;
        _references = references;
        _attendance = attendance;
        _companyClock = companyClock;
    }

    [Authorize(HrPermissions.EmployeeShiftPermissions.Assign)]
    public virtual async Task<EmployeeShiftDto> AssignAsync(Guid employeeId, EmployeeShiftAssignDto input)
    {
        var employee = await _employees.GetAsync(employeeId);

        if ((await _managerLinks.GetRolesAsync(employee)).Contains(HrRoles.ExecutiveDirector))
        {
            throw new BusinessException(HrErrorCodes.Attendance.ExecutiveDirectorNotCovered);
        }

        var shift = await _shifts.GetAsync(input.ShiftId!.Value, includeDetails: false);
        if (!shift.IsActive)
        {
            throw new BusinessException(HrErrorCodes.Shift.ShiftInactive);
        }

        var startDate = input.StartDate!.Value;
        if (startDate < employee.HireDate)
        {
            throw new BusinessException(HrErrorCodes.Shift.StartDateBeforeHireDate);
        }

        var current = await _assignments.FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.EndDate == null);
        if (current != null)
        {
            // Closing the old row on the day before would end it before it began, so the same start date is refused too.
            if (startDate <= current.StartDate)
            {
                throw new BusinessException(HrErrorCodes.Shift.StartDateBeforeCurrentAssignment);
            }

            current.Close(startDate.AddDays(-1));
            await _assignments.UpdateAsync(current, autoSave: true);
        }

        var assignment = await _assignments.InsertAsync(
            new EmployeeShiftAssignment(GuidGenerator.Create(), CurrentTenant.Id, employeeId, shift.Id, startDate),
            autoSave: true);

        if (startDate < await _companyClock.GetTodayAsync())
        {
            await _attendance.RecalculateEmployeeFromAsync(employeeId, startDate);
        }

        return (await ToDtosAsync(employee, new[] { assignment })).Single();
    }

    [Authorize(HrPermissions.EmployeeShiftPermissions.View)]
    public virtual async Task<EmployeeShiftPagedResultDto> GetHistoryAsync(Guid employeeId, PagedAndSortedResultRequestDto input)
    {
        var employee = await _employees.GetAsync(employeeId);

        var query = (await _assignments.GetQueryableAsync()).Where(x => x.EmployeeId == employeeId);
        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(
                    input.Sorting,
                    nameof(EmployeeShiftAssignment.StartDate) + " desc",
                    new Dictionary<string, string>
                    {
                        ["startDate"] = nameof(EmployeeShiftAssignment.StartDate),
                        ["endDate"] = nameof(EmployeeShiftAssignment.EndDate)
                    })
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new EmployeeShiftPagedResultDto { TotalCount = total, Items = await ToDtosAsync(employee, items) };
    }

    private async Task<List<EmployeeShiftDto>> ToDtosAsync(Employee employee, IReadOnlyCollection<EmployeeShiftAssignment> items)
    {
        var employeeRef = await _references.ToRefAsync(employee);
        var shiftIds = items.Select(x => x.WorkShiftId).Distinct().ToList();
        var shifts = (await AsyncExecuter.ToListAsync((await _shifts.GetQueryableAsync()).Where(x => shiftIds.Contains(x.Id)))).ToDictionary(x => x.Id);
        var creators = await _references.GetUserRefsAsync(items.Where(x => x.CreatorId != null).Select(x => x.CreatorId!.Value));

        return items.Select(x => new EmployeeShiftDto
        {
            Id = x.Id,
            Employee = employeeRef,
            Shift = new WorkShiftRefDto
            {
                Id = x.WorkShiftId,
                Name = LocalizedName.Pick(shifts[x.WorkShiftId].NameAr, shifts[x.WorkShiftId].NameEn),
                Type = shifts[x.WorkShiftId].Type
            },
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            CreationTime = x.CreationTime,
            CreatedBy = x.CreatorId == null ? null : creators.GetValueOrDefault(x.CreatorId.Value)
        }).ToList();
    }
}

public class EmployeeShiftAssignDtoValidator : AbstractValidator<EmployeeShiftAssignDto>
{
    public EmployeeShiftAssignDtoValidator()
    {
        RuleFor(x => x.ShiftId).NotNull();
        RuleFor(x => x.StartDate).NotNull();
    }
}
