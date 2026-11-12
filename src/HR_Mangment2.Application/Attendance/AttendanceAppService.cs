using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Identity;
using HR_Mangment2.Localization;
using HR_Mangment2.Permissions;
using HR_Mangment2.WorkShifts;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace HR_Mangment2.Attendance;

[Authorize]
public class AttendanceAppService : HR_Mangment2AppService, IAttendanceAppService
{
    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["date"] = nameof(AttendanceDay.Date),
        ["status"] = nameof(AttendanceDay.Status),
        ["lateMinutes"] = nameof(AttendanceDay.LateMinutes),
        ["workedMinutes"] = nameof(AttendanceDay.WorkedMinutes),
        ["overtimeMinutes"] = nameof(AttendanceDay.OvertimeMinutes)
    };

    private readonly IRepository<AttendanceDay, Guid> _days;
    private readonly IRepository<AttendancePunch, Guid> _punches;
    private readonly IRepository<AttendanceDevice, Guid> _devices;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IRepository<WorkShift, Guid> _shifts;
    private readonly IRepository<AttendanceCorrection, Guid> _corrections;
    private readonly IRepository<HR_Mangment2.Requests.Request, Guid> _requests;
    private readonly ManagerLinkService _managerLinks;
    private readonly EmployeeReferenceReader _references;

    public AttendanceAppService(
        IRepository<AttendanceDay, Guid> days,
        IRepository<AttendancePunch, Guid> punches,
        IRepository<AttendanceDevice, Guid> devices,
        IRepository<Employee, Guid> employees,
        IRepository<WorkShift, Guid> shifts,
        IRepository<AttendanceCorrection, Guid> corrections,
        IRepository<HR_Mangment2.Requests.Request, Guid> requests,
        ManagerLinkService managerLinks,
        EmployeeReferenceReader references)
    {
        _corrections = corrections;
        _requests = requests;
        _days = days;
        _punches = punches;
        _devices = devices;
        _employees = employees;
        _shifts = shifts;
        _managerLinks = managerLinks;
        _references = references;
    }

    [Authorize(HrPermissions.AttendancePermissions.ViewOwn)]
    public virtual async Task<AttendanceDayPagedResultDto> GetMyAsync(AttendanceMyListInput input)
    {
        if (ManagerLinkService.IsOutsideSchedule(CurrentUser.Roles))
        {
            throw new BusinessException(HrErrorCodes.Attendance.ExecutiveDirectorNotCovered);
        }

        var me = await FindMyEmployeeAsync();
        if (me == null)
        {
            return new AttendanceDayPagedResultDto();
        }

        return await QueryAsync((await _days.GetQueryableAsync()).Where(x => x.EmployeeId == me.Id), input);
    }

    [Authorize(HrPermissions.AttendancePermissions.ViewTeam)]
    public virtual async Task<AttendanceDayPagedResultDto> GetTeamAsync(AttendanceTeamListInput input)
    {
        var me = await FindMyEmployeeAsync();
        if (me == null)
        {
            return new AttendanceDayPagedResultDto();
        }

        var team = (await _employees.GetQueryableAsync()).Where(x => x.ManagerEmployeeId == me.Id).Select(x => x.Id);
        var query = (await _days.GetQueryableAsync()).Where(x => team.Contains(x.EmployeeId));

        if (input.EmployeeId != null)
        {
            query = query.Where(x => x.EmployeeId == input.EmployeeId);
        }

        return await QueryAsync(query, input);
    }

    [Authorize(HrPermissions.AttendancePermissions.ViewEmployee)]
    public virtual async Task<AttendanceDayPagedResultDto> GetByEmployeeAsync(Guid employeeId, AttendanceMyListInput input)
    {
        var employee = await _employees.GetAsync(employeeId);

        if (!CurrentUser.IsInRole(HrRoles.HrManager))
        {
            var me = await FindMyEmployeeAsync();
            if (me == null || employee.ManagerEmployeeId != me.Id)
            {
                throw new AbpAuthorizationException();
            }
        }

        if (ManagerLinkService.IsOutsideSchedule(await _managerLinks.GetRolesAsync(employee)))
        {
            throw new BusinessException(HrErrorCodes.Attendance.ExecutiveDirectorNotCovered);
        }

        return await QueryAsync((await _days.GetQueryableAsync()).Where(x => x.EmployeeId == employeeId), input);
    }

    [Authorize(HrPermissions.AttendancePermissions.ViewDayDetail)]
    public virtual async Task<AttendanceDayDetailDto> GetDayDetailAsync(Guid id)
    {
        var day = await _days.FindAsync(id) ?? throw new EntityNotFoundException(typeof(AttendanceDay), id);
        await CheckCanSeeDayAsync(day);

        var punches = await AsyncExecuter.ToListAsync(
            (await _punches.GetQueryableAsync())
                .Where(x => x.EmployeeId == day.EmployeeId && x.PunchTime >= day.WindowStart && x.PunchTime < day.WindowEnd)
                .OrderBy(x => x.PunchTime));

        var deviceIds = punches.Select(x => x.DeviceId).Distinct().ToList();
        var deviceNames = (await _devices.GetListAsync(x => deviceIds.Contains(x.Id))).ToDictionary(x => x.Id, x => x.Name);

        var corrections = (await _corrections.GetListAsync(x => x.EmployeeId == day.EmployeeId && x.Date == day.Date && x.AppliedAt != null))
            .OrderByDescending(x => x.AppliedAt)
            .ToList();
        var correctionIds = corrections.Select(x => x.Id).ToList();
        var submitters = (await _requests.GetListAsync(x => correctionIds.Contains(x.Id))).ToDictionary(x => x.Id, x => x.SubmittedByUserId);
        var users = await _references.GetUserRefsAsync(submitters.Values.Concat(corrections.Where(x => x.ApprovedByUserId != null).Select(x => x.ApprovedByUserId!.Value)));

        return new AttendanceDayDetailDto
        {
            Corrections = corrections.Select(x => new AttendanceCorrectionHistoryEntryDto
            {
                Id = x.Id,
                RequestId = x.Id,
                PunchType = x.PunchType,
                OldValue = x.ReplacedValue,
                NewValue = x.AppliedValue!.Value,
                Reason = x.Reason,
                AppliedAt = x.AppliedAt!.Value,
                RequestedBy = users[submitters[x.Id]],
                ApprovedBy = x.ApprovedByUserId == null ? null : users[x.ApprovedByUserId.Value],
                AffectsClosedMonth = x.AffectsClosedMonth ?? false,
                SettlementPeriod = x.SettlementPeriod
            }).ToList(),
            Day = (await ToDtosAsync(new[] { day })).Single(),
            OriginalCheckIn = day.OriginalCheckIn,
            OriginalCheckOut = day.OriginalCheckOut,
            RawPunches = punches.Select(x => new AttendanceRawPunchDto
            {
                Id = x.Id,
                PunchTime = x.PunchTime,
                Source = x.Source,
                DeviceName = deviceNames.GetValueOrDefault(x.DeviceId),
                ImportId = x.ImportId
            }).ToList()
        };
    }

    private async Task CheckCanSeeDayAsync(AttendanceDay day)
    {
        if (CurrentUser.IsInRole(HrRoles.HrManager))
        {
            return;
        }

        var me = await FindMyEmployeeAsync();
        if (me != null && day.EmployeeId == me.Id)
        {
            return;
        }

        if (me != null && CurrentUser.IsInRole(HrRoles.DepartmentManager) &&
            await _employees.AnyAsync(x => x.Id == day.EmployeeId && x.ManagerEmployeeId == me.Id))
        {
            return;
        }

        throw new AbpAuthorizationException();
    }

    private async Task<Employee?> FindMyEmployeeAsync()
    {
        var userId = CurrentUser.GetId();
        return await _employees.FirstOrDefaultAsync(x => x.UserId == userId);
    }

    private async Task<AttendanceDayPagedResultDto> QueryAsync(IQueryable<AttendanceDay> query, AttendanceMyListInput input)
    {
        if (input.FromDate != null)
        {
            query = query.Where(x => x.Date >= input.FromDate);
        }

        if (input.ToDate != null)
        {
            query = query.Where(x => x.Date <= input.ToDate);
        }

        if (input.Status != null)
        {
            query = query.Where(x => x.Status == input.Status);
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(AttendanceDay.Date) + " desc", SortFields)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new AttendanceDayPagedResultDto
        {
            TotalCount = total,
            Items = await ToDtosAsync(items),
            Summary = await SummarizeAsync(query)
        };
    }

    /// <summary>Over every matching row, not only the current page.</summary>
    private async Task<AttendancePeriodSummaryDto> SummarizeAsync(IQueryable<AttendanceDay> query)
    {
        var groups = await AsyncExecuter.ToListAsync(query
            .GroupBy(x => x.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count(),
                Late = g.Sum(x => x.LateMinutes),
                Leave = g.Sum(x => x.LeaveDays),
                Worked = g.Sum(x => (long)x.WorkedMinutes),
                Overtime = g.Sum(x => (long)x.OvertimeMinutes)
            }));

        int CountOf(AttendanceDayStatus status) => groups.Where(x => x.Status == status).Sum(x => x.Count);

        return new AttendancePeriodSummaryDto
        {
            PresentDays = CountOf(AttendanceDayStatus.Present) + CountOf(AttendanceDayStatus.Late),
            LateDays = CountOf(AttendanceDayStatus.Late),
            TotalLateMinutes = groups.Sum(x => x.Late),
            AbsentDays = CountOf(AttendanceDayStatus.Absent),
            IncompleteDays = CountOf(AttendanceDayStatus.Incomplete),
            LeaveDays = groups.Sum(x => x.Leave),
            WorkedMinutes = groups.Sum(x => x.Worked),
            OvertimeMinutes = groups.Sum(x => x.Overtime)
        };
    }

    private async Task<List<AttendanceDayDto>> ToDtosAsync(IReadOnlyCollection<AttendanceDay> days)
    {
        var employees = await _references.GetRefsAsync(days.Select(x => x.EmployeeId));
        var shiftIds = days.Where(x => x.WorkShiftId != null).Select(x => x.WorkShiftId!.Value).Distinct().ToList();
        var shifts = (await AsyncExecuter.ToListAsync((await _shifts.GetQueryableAsync()).Where(x => shiftIds.Contains(x.Id)))).ToDictionary(x => x.Id);

        return days.Select(x => new AttendanceDayDto
        {
            Id = x.Id,
            Employee = employees[x.EmployeeId],
            Date = x.Date,
            Shift = x.WorkShiftId == null
                ? null
                : new WorkShiftRefDto
                {
                    Id = x.WorkShiftId.Value,
                    Name = LocalizedName.Pick(shifts[x.WorkShiftId.Value].NameAr, shifts[x.WorkShiftId.Value].NameEn),
                    Type = shifts[x.WorkShiftId.Value].Type
                },
            ScheduledStart = x.ScheduledStart,
            ScheduledEnd = x.ScheduledEnd,
            CheckIn = x.CheckIn,
            CheckOut = x.CheckOut,
            Status = x.Status,
            LateMinutes = x.LateMinutes,
            WorkedMinutes = x.WorkedMinutes,
            OvertimeMinutes = x.OvertimeMinutes,
            HourlyLeaveMinutes = x.HourlyLeaveMinutes,
            LeaveRequestId = x.LeaveRequestId,
            IsCorrected = x.IsCorrected
        }).ToList();
    }
}
