using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Attendance;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Holidays;
using HR_Mangment2.Identity;
using HR_Mangment2.Requests;
using HR_Mangment2.Settings;
using HR_Mangment2.WorkShifts;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Leaves;

/// <summary>What the schedule says about a date: the same reading attendance uses, where a public holiday beats the shift.</summary>
public record EmployeeScheduleDay(
    DateOnly Date,
    bool HasAssignment,
    LeaveDayClassification Classification,
    WorkShiftDay? ShiftDay,
    string? HolidayNameAr,
    string? HolidayNameEn);

public class EmployeeScheduleReader : ITransientDependency
{
    private readonly IRepository<EmployeeShiftAssignment, Guid> _assignments;
    private readonly IRepository<WorkShift, Guid> _shifts;
    private readonly IRepository<Holiday, Guid> _holidays;

    public EmployeeScheduleReader(
        IRepository<EmployeeShiftAssignment, Guid> assignments,
        IRepository<WorkShift, Guid> shifts,
        IRepository<Holiday, Guid> holidays)
    {
        _assignments = assignments;
        _shifts = shifts;
        _holidays = holidays;
    }

    public virtual async Task<List<EmployeeScheduleDay>> GetAsync(Guid employeeId, DateOnly from, DateOnly to)
    {
        var assignments = await _assignments.GetListAsync(x =>
            x.EmployeeId == employeeId && x.StartDate <= to && (x.EndDate == null || x.EndDate >= from));

        var shifts = new Dictionary<Guid, WorkShift>();
        foreach (var id in assignments.Select(x => x.WorkShiftId).Distinct())
        {
            shifts[id] = await _shifts.GetAsync(id);
        }

        var holidays = (await _holidays.GetListAsync(x => x.Date >= from && x.Date <= to)).ToDictionary(x => x.Date);
        var days = new List<EmployeeScheduleDay>();

        for (var date = from; date <= to; date = date.AddDays(1))
        {
            var assignment = assignments.FirstOrDefault(x => x.StartDate <= date && (x.EndDate == null || x.EndDate >= date));
            var holiday = holidays.GetValueOrDefault(date);
            var shiftDay = assignment == null || holiday != null ? null : AttendanceDayEvaluator.FindDay(shifts[assignment.WorkShiftId], date, assignment.StartDate);

            var classification = holiday != null
                ? LeaveDayClassification.Holiday
                : shiftDay != null ? LeaveDayClassification.WorkingDay : LeaveDayClassification.WeeklyOff;

            days.Add(new EmployeeScheduleDay(date, assignment != null, classification, shiftDay, holiday?.NameAr, holiday?.NameEn));
        }

        return days;
    }
}

/// <summary>
/// Who a leave or correction belongs to. The general on-behalf rules apply, except that these two operations answer 403
/// for anyone outside the rule, and name the executive director with their own code.
/// </summary>
public class DedicatedRequestOwnerResolver : ITransientDependency
{
    private readonly IRepository<Employee, Guid> _employees;
    private readonly ManagerLinkService _managerLinks;
    private readonly RequestWorkflowService _workflow;

    public DedicatedRequestOwnerResolver(IRepository<Employee, Guid> employees, ManagerLinkService managerLinks, RequestWorkflowService workflow)
    {
        _employees = employees;
        _managerLinks = managerLinks;
        _workflow = workflow;
    }

    /// <param name="notActiveCode">The error for an inactive team member, or null to answer 403.</param>
    public virtual async Task<(Employee Owner, bool IsOnBehalf)> ResolveAsync(Guid currentUserId, Guid? employeeId, string executiveCode, string? notActiveCode)
    {
        if (employeeId == null)
        {
            var (me, _) = await _workflow.ResolveOwnerAsync(currentUserId, null);
            await CheckNotExecutiveAsync(me, executiveCode);
            return (me, false);
        }

        var target = await _employees.FindAsync(employeeId.Value);
        if (target == null)
        {
            throw new Volo.Abp.Authorization.AbpAuthorizationException();
        }

        await CheckNotExecutiveAsync(target, executiveCode);

        var mine = await _employees.FirstOrDefaultAsync(x => x.UserId == currentUserId);
        var isMyTeam = mine != null && target.ManagerEmployeeId == mine.Id;

        if (isMyTeam && target.Status != EmployeeStatus.Active && notActiveCode != null)
        {
            throw new BusinessException(notActiveCode);
        }

        var row = RequestRoutePlanner.ResolveRow(await _managerLinks.GetRolesAsync(target));
        if (!isMyTeam || row != RequestOwnerRow.Employee || target.Status != EmployeeStatus.Active)
        {
            throw new Volo.Abp.Authorization.AbpAuthorizationException();
        }

        return await _workflow.ResolveOwnerAsync(currentUserId, employeeId);
    }

    private async Task CheckNotExecutiveAsync(Employee employee, string code)
    {
        if (ManagerLinkService.IsOutsideSchedule(await _managerLinks.GetRolesAsync(employee)))
        {
            throw new BusinessException(code);
        }
    }
}

public record LeaveBalanceSnapshot(
    Employee Employee,
    EmployeeLeaveAccount? Account,
    EmployeeLeaveYear? CurrentYear,
    DateOnly YearStart,
    DateOnly YearEnd,
    DateOnly NextYearStart,
    IReadOnlyList<LeaveLedgerEntry> CurrentEntries,
    IReadOnlyList<LeaveLedgerEntry> FutureEntries,
    decimal NextYearEntitlement)
{
    public decimal Sum(LeaveLedgerBucket bucket) => CurrentEntries.Where(x => x.Bucket == bucket).Sum(x => x.Days ?? 0);

    public decimal Sum(params LeaveLedgerType[] types) => CurrentEntries.Where(x => types.Contains(x.Type)).Sum(x => x.Days ?? 0);

    public LeavePools Pools => new(
        Sum(LeaveLedgerBucket.Carried),
        Sum(LeaveLedgerBucket.Entitlement),
        FutureEntries.Where(x => x.Bucket == LeaveLedgerBucket.Carried).Sum(x => x.Days ?? 0),
        FutureEntries.Where(x => x.Bucket == LeaveLedgerBucket.Entitlement).Sum(x => x.Days ?? 0),
        NextYearEntitlement);
}

/// <summary>The only place that writes the ledger. Every change touches the account, so simultaneous changes collide.</summary>
public class LeaveBalanceManager : ITransientDependency
{
    private readonly IRepository<EmployeeLeaveAccount, Guid> _accounts;
    private readonly IRepository<EmployeeLeaveYear, Guid> _years;
    private readonly IRepository<LeaveLedgerEntry, Guid> _ledger;
    private readonly ICompanyClock _companyClock;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly HrSettingsStore _settings;
    private readonly IIdentityUserRepository _users;

    public LeaveBalanceManager(
        IRepository<EmployeeLeaveAccount, Guid> accounts,
        IRepository<EmployeeLeaveYear, Guid> years,
        IRepository<LeaveLedgerEntry, Guid> ledger,
        ICompanyClock companyClock,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        HrSettingsStore settings,
        IIdentityUserRepository users)
    {
        _accounts = accounts;
        _years = years;
        _ledger = ledger;
        _companyClock = companyClock;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _settings = settings;
        _users = users;
    }

    /// <summary>The company's annual days per leave year, from system settings (decision 2026-11-10).</summary>
    public virtual async Task<decimal> GetDefaultEntitlementAsync() =>
        (await _settings.PeekValueAsync<AnnualLeaveSettings>()).DefaultEntitlementDays;

    /// <summary>
    /// Whether the employee gets a leave year: active, hired by today, and not one of the roles with no shift and no
    /// leave (executive director, HR manager, system admin).
    /// </summary>
    private async Task<bool> HasLeaveAsync(Employee employee, DateOnly today)
    {
        if (employee.Status != EmployeeStatus.Active || employee.HireDate > today)
        {
            return false;
        }

        var roles = employee.UserId == null ? new List<string>() : await _users.GetRoleNamesAsync(employee.UserId.Value);
        return !ManagerLinkService.IsOutsideSchedule(roles);
    }

    public virtual Task<EmployeeLeaveAccount?> FindAccountAsync(Guid employeeId) =>
        _accounts.FirstOrDefaultAsync(x => x.EmployeeId == employeeId);

    public virtual async Task<EmployeeLeaveAccount> GetOrCreateAccountAsync(Guid employeeId) =>
        await FindAccountAsync(employeeId)
        ?? await _accounts.InsertAsync(new EmployeeLeaveAccount(_guidGenerator.Create(), _currentTenant.Id, employeeId), autoSave: true);

    public virtual async Task TouchAsync(EmployeeLeaveAccount account)
    {
        account.Touch();
        await _accounts.UpdateAsync(account);
    }

    /// <summary>
    /// Moves an employee into the year they should be in. Safe to repeat: a year that has rolled is closed and a second
    /// run finds nothing to do. Reads call this too, so nobody sees last year's figures before the worker's next run.
    /// </summary>
    public virtual async Task<EmployeeLeaveYear?> RollOverAsync(Employee employee, DateOnly today)
    {
        var years = (await _years.GetListAsync(x => x.EmployeeId == employee.Id)).OrderBy(x => x.YearStart).ToList();
        var current = years.LastOrDefault(x => x.IsGranted);
        if (current == null)
        {
            // Nobody sets an entitlement by hand any more: the first year opens with the company default (decision 2026-11-10).
            if (!await HasLeaveAsync(employee, today))
            {
                return null;
            }

            return await OpenFirstYearAsync(employee, await GetOrCreateAccountAsync(employee.Id), await GetDefaultEntitlementAsync(), null);
        }

        while (current.YearEnd < today)
        {
            var account = await GetOrCreateAccountAsync(employee.Id);
            var nextStart = current.YearEnd.AddDays(1);
            var next = years.FirstOrDefault(x => x.YearStart == nextStart);
            if (next == null)
            {
                next = await InsertYearAsync(employee, nextStart);
                years.Add(next);
            }

            // Each year takes the default in force when it starts, so a change never touches a running year.
            var entitlement = await GetDefaultEntitlementAsync();
            next.Grant(entitlement);
            await _years.UpdateAsync(next, autoSave: true);

            if (entitlement > 0)
            {
                await WriteAsync(employee.Id, next.Id, LeaveLedgerType.Grant, LeaveLedgerBucket.Entitlement, entitlement, null, null, null);
            }

            var remaining = (await _ledger.GetListAsync(x => x.LeaveYearId == current.Id)).Sum(x => x.Days ?? 0);
            if (remaining > 0)
            {
                await WriteAsync(employee.Id, next.Id, LeaveLedgerType.CarryIn, LeaveLedgerBucket.Carried, remaining, null, null, null);
            }

            current.Close();
            await _years.UpdateAsync(current, autoSave: true);
            await TouchAsync(account);
            current = next;
        }

        return current;
    }

    public virtual async Task<LeaveBalanceSnapshot> GetSnapshotAsync(Employee employee)
    {
        var today = await _companyClock.GetTodayAsync();
        var year = await RollOverAsync(employee, today);
        var account = await FindAccountAsync(employee.Id);

        var (start, end) = year != null ? (year.YearStart, year.YearEnd) : LeaveYearMath.YearContaining(employee.HireDate, today);
        var nextStart = end.AddDays(1);

        var entries = year == null ? new List<LeaveLedgerEntry>() : await _ledger.GetListAsync(x => x.LeaveYearId == year.Id);
        var future = await _years.FirstOrDefaultAsync(x => x.EmployeeId == employee.Id && x.YearStart == nextStart);
        var futureEntries = future == null ? new List<LeaveLedgerEntry>() : await _ledger.GetListAsync(x => x.LeaveYearId == future.Id);

        return new LeaveBalanceSnapshot(employee, account, year, start, end, nextStart, entries, futureEntries, await GetDefaultEntitlementAsync());
    }

    /// <summary>Opens the current year with a grant of the given days (the company default).</summary>
    public virtual async Task<EmployeeLeaveYear> OpenFirstYearAsync(Employee employee, EmployeeLeaveAccount account, decimal days, string? reason)
    {
        var (start, _) = LeaveYearMath.YearContaining(employee.HireDate, await _companyClock.GetTodayAsync());
        var year = await _years.FirstOrDefaultAsync(x => x.EmployeeId == employee.Id && x.YearStart == start)
                   ?? await InsertYearAsync(employee, start);

        year.Grant(days);
        await _years.UpdateAsync(year, autoSave: true);
        if (days > 0)
        {
            await WriteAsync(employee.Id, year.Id, LeaveLedgerType.Grant, LeaveLedgerBucket.Entitlement, days, null, null, reason);
        }

        await TouchAsync(account);
        return year;
    }

    /// <summary>A deduction takes added days first, in the same order as leave. The caller has checked the amount is available.</summary>
    public virtual async Task AdjustAsync(LeaveBalanceSnapshot snapshot, EmployeeLeaveAccount account, decimal days, string reason)
    {
        var yearId = snapshot.CurrentYear!.Id;
        var employeeId = snapshot.Employee.Id;

        if (days > 0)
        {
            await WriteAsync(employeeId, yearId, LeaveLedgerType.Adjust, LeaveLedgerBucket.Entitlement, days, null, null, reason);
        }
        else
        {
            var fromCarried = Math.Min(-days, Math.Max(0, snapshot.Sum(LeaveLedgerBucket.Carried)));
            if (fromCarried > 0)
            {
                await WriteAsync(employeeId, yearId, LeaveLedgerType.Adjust, LeaveLedgerBucket.Carried, -fromCarried, null, null, reason);
            }

            if (-days > fromCarried)
            {
                await WriteAsync(employeeId, yearId, LeaveLedgerType.Adjust, LeaveLedgerBucket.Entitlement, days + fromCarried, null, null, reason);
            }
        }

        await TouchAsync(account);
    }

    /// <summary>Holds a submitted leave. A part in the coming year is charged there; everything else to the current year.</summary>
    public virtual async Task HoldAsync(Employee employee, LeavePlan plan, Guid requestId)
    {
        var account = await GetOrCreateAccountAsync(employee.Id);

        if (plan.AnnualDays > 0 || plan.HourlyQuotaHoursUsed > 0)
        {
            var snapshot = await GetSnapshotAsync(employee);
            foreach (var segment in plan.Segments)
            {
                if (segment.FromCarriedDays <= 0 && segment.FromEntitlementDays <= 0)
                {
                    continue;
                }

                var yearId = await ChargedYearIdAsync(employee, snapshot, segment.IsFutureYear);
                await WriteDaysAsync(employee.Id, yearId, LeaveLedgerType.Hold, segment.FromCarriedDays, segment.FromEntitlementDays, requestId);
            }

            if (plan.HourlyQuotaHoursUsed > 0)
            {
                var yearId = await ChargedYearIdAsync(employee, snapshot, plan.Segments[0].IsFutureYear);
                await WriteAsync(employee.Id, yearId, LeaveLedgerType.Hold, LeaveLedgerBucket.Hourly, null, -plan.HourlyQuotaHoursUsed, plan.HourlyMonth, requestId);
            }
        }

        // Without a held balance this still serializes the employee's submissions, for the overlap and limit checks.
        await TouchAsync(account);
    }

    /// <summary>The final approval turns each hold into a deduction: the hold is released and the deduction taken, so the sum stays the balance.</summary>
    public virtual async Task DeductHoldsAsync(Guid employeeId, Guid requestId)
    {
        foreach (var hold in await _ledger.GetListAsync(x => x.RequestId == requestId && x.Type == LeaveLedgerType.Hold))
        {
            await WriteAsync(employeeId, hold.LeaveYearId, LeaveLedgerType.Release, hold.Bucket, -hold.Days, -hold.Hours, hold.HourlyMonth, requestId);
            await WriteAsync(employeeId, hold.LeaveYearId, LeaveLedgerType.Deduct, hold.Bucket, hold.Days, hold.Hours, hold.HourlyMonth, requestId);
        }
    }

    /// <summary>Days released from a year that has since closed become added days in the current year (QUESTIONS.md item 17).</summary>
    public virtual async Task ReleaseHoldsAsync(Employee employee, Guid requestId)
    {
        var holds = await _ledger.GetListAsync(x => x.RequestId == requestId && x.Type == LeaveLedgerType.Hold);
        if (holds.Count == 0)
        {
            return;
        }

        var current = await RollOverAsync(employee, await _companyClock.GetTodayAsync());
        var closedYears = (await _years.GetListAsync(x => x.EmployeeId == employee.Id && x.IsClosed)).Select(x => x.Id).ToHashSet();

        foreach (var hold in holds)
        {
            await WriteAsync(employee.Id, hold.LeaveYearId, LeaveLedgerType.Release, hold.Bucket, -hold.Days, -hold.Hours, hold.HourlyMonth, requestId);

            if (hold.Days != null && closedYears.Contains(hold.LeaveYearId) && current != null)
            {
                await WriteAsync(employee.Id, current.Id, LeaveLedgerType.CarryIn, LeaveLedgerBucket.Carried, -hold.Days, null, null, requestId);
            }
        }

        await TouchAsync(await GetOrCreateAccountAsync(employee.Id));
    }

    /// <summary>D16, QUESTIONS.md item 17: gives back what an approved leave used. Days from a year that has closed become added days now.</summary>
    public virtual async Task ReverseDeductionsAsync(Employee employee, Guid requestId, string reason)
    {
        var deductions = await _ledger.GetListAsync(x => x.RequestId == requestId && x.Type == LeaveLedgerType.Deduct);
        var current = await RollOverAsync(employee, await _companyClock.GetTodayAsync());
        var closedYears = (await _years.GetListAsync(x => x.EmployeeId == employee.Id && x.IsClosed)).Select(x => x.Id).ToHashSet();

        foreach (var deduction in deductions)
        {
            await WriteAsync(employee.Id, deduction.LeaveYearId, LeaveLedgerType.Reverse, deduction.Bucket, -deduction.Days, -deduction.Hours, deduction.HourlyMonth, requestId, reason);

            if (deduction.Days != null && closedYears.Contains(deduction.LeaveYearId) && current != null)
            {
                await WriteAsync(employee.Id, current.Id, LeaveLedgerType.CarryIn, LeaveLedgerBucket.Carried, -deduction.Days, null, null, requestId);
            }
        }

        await TouchAsync(await GetOrCreateAccountAsync(employee.Id));
    }

    public virtual async Task<decimal> GetHourlyMonthSumAsync(Guid employeeId, string month) =>
        (await _ledger.GetListAsync(x => x.EmployeeId == employeeId && x.Bucket == LeaveLedgerBucket.Hourly && x.HourlyMonth == month))
        .Sum(x => x.Hours ?? 0);

    public virtual Task<List<LeaveLedgerEntry>> GetHourlyMonthEntriesAsync(Guid employeeId, string month) =>
        _ledger.GetListAsync(x => x.EmployeeId == employeeId && x.Bucket == LeaveLedgerBucket.Hourly && x.HourlyMonth == month);

    private async Task<Guid> ChargedYearIdAsync(Employee employee, LeaveBalanceSnapshot snapshot, bool isFutureYear)
    {
        if (!isFutureYear)
        {
            return snapshot.CurrentYear!.Id;
        }

        var future = await _years.FirstOrDefaultAsync(x => x.EmployeeId == employee.Id && x.YearStart == snapshot.NextYearStart)
                     ?? await InsertYearAsync(employee, snapshot.NextYearStart);
        return future.Id;
    }

    private async Task<EmployeeLeaveYear> InsertYearAsync(Employee employee, DateOnly start) =>
        await _years.InsertAsync(
            new EmployeeLeaveYear(_guidGenerator.Create(), _currentTenant.Id, employee.Id, start, LeaveYearMath.NextStart(employee.HireDate, start).AddDays(-1)),
            autoSave: true);

    private async Task WriteDaysAsync(Guid employeeId, Guid yearId, LeaveLedgerType type, decimal carried, decimal entitlement, Guid requestId)
    {
        if (carried > 0)
        {
            await WriteAsync(employeeId, yearId, type, LeaveLedgerBucket.Carried, -carried, null, null, requestId);
        }

        if (entitlement > 0)
        {
            await WriteAsync(employeeId, yearId, type, LeaveLedgerBucket.Entitlement, -entitlement, null, null, requestId);
        }
    }

    private Task WriteAsync(Guid employeeId, Guid yearId, LeaveLedgerType type, LeaveLedgerBucket bucket, decimal? days, decimal? hours, string? month, string? reason) =>
        _ledger.InsertAsync(
            new LeaveLedgerEntry(_guidGenerator.Create(), _currentTenant.Id, employeeId, yearId, type, bucket, days, hours, month, null, reason),
            autoSave: true);

    private Task WriteAsync(Guid employeeId, Guid yearId, LeaveLedgerType type, LeaveLedgerBucket bucket, decimal? days, decimal? hours, string? month, Guid requestId) =>
        WriteAsync(employeeId, yearId, type, bucket, days, hours, month, requestId, null);

    private Task WriteAsync(Guid employeeId, Guid yearId, LeaveLedgerType type, LeaveLedgerBucket bucket, decimal? days, decimal? hours, string? month, Guid? requestId, string? reason) =>
        _ledger.InsertAsync(
            new LeaveLedgerEntry(_guidGenerator.Create(), _currentTenant.Id, employeeId, yearId, type, bucket, days, hours, month, requestId, reason),
            autoSave: true);
}

public record LeavePlanInput(
    LeaveKind Kind,
    DateOnly? Date,
    TimeOnly? FromTime,
    TimeOnly? ToTime,
    DateOnly? StartDate,
    DateOnly? EndDate,
    Guid? OtherLeaveTypeId);

public record LeavePlanSegment(
    DateOnly YearStart,
    DateOnly YearEnd,
    bool IsFutureYear,
    DateOnly? StartDate,
    DateOnly? EndDate,
    decimal WorkingDays,
    decimal FromCarriedDays,
    decimal FromEntitlementDays);

/// <summary>A leave worked out without saving anything. Issues are what would stop it being submitted.</summary>
public class LeavePlan
{
    public LeaveKind Kind { get; init; }
    public DateOnly? Date { get; init; }
    public TimeOnly? FromTime { get; init; }
    public TimeOnly? ToTime { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public DateOnly RangeStart { get; init; }
    public DateOnly RangeEnd { get; init; }
    public OtherLeaveType? OtherLeaveType { get; set; }
    public int CalendarDays { get; set; }
    public decimal WorkingDays { get; set; }
    public int ExcludedWeeklyOffDays { get; set; }
    public int ExcludedHolidayDays { get; set; }
    public decimal? RequestedHours { get; set; }
    public decimal? HourlyQuotaHoursUsed { get; set; }
    public decimal? HourlyExcessHours { get; set; }
    public string? HourlyMonth { get; set; }
    public bool DeductsFromAnnualBalance { get; set; }
    public decimal AnnualDays { get; set; }
    public decimal FromCarriedDays { get; set; }
    public decimal FromEntitlementDays { get; set; }
    public List<LeavePlanSegment> Segments { get; set; } = new();
    public List<EmployeeScheduleDay> DayBreakdown { get; set; } = new();
    public decimal AvailableDaysBefore { get; set; }
    public decimal RemainingDaysAfter { get; set; }
    public decimal? AvailableHourlyHoursBefore { get; set; }
    public decimal ShortageDays { get; set; }
    public List<string> Issues { get; } = new();
}

/// <summary>Works out a leave. Preview and submission share this, so what the screen shows is what the server enforces.</summary>
public class LeavePlanner : ITransientDependency
{
    private readonly LeaveBalanceManager _balances;
    private readonly EmployeeScheduleReader _schedule;
    private readonly IRepository<OtherLeaveType, Guid> _otherTypes;
    private readonly IRepository<LeaveRequest, Guid> _leaves;
    private readonly IRepository<LeaveRequestSegment, Guid> _segments;
    private readonly HrSettingsStore _settings;

    public LeavePlanner(
        LeaveBalanceManager balances,
        EmployeeScheduleReader schedule,
        IRepository<OtherLeaveType, Guid> otherTypes,
        IRepository<LeaveRequest, Guid> leaves,
        IRepository<LeaveRequestSegment, Guid> segments,
        HrSettingsStore settings)
    {
        _balances = balances;
        _schedule = schedule;
        _otherTypes = otherTypes;
        _leaves = leaves;
        _segments = segments;
        _settings = settings;
    }

    public static bool IsEligible(OtherLeaveType type, AppGender? gender) => type.EligibleCategory switch
    {
        OtherLeaveEligibleCategory.Male => gender == AppGender.Male,
        OtherLeaveEligibleCategory.Female => gender == AppGender.Female,
        _ => true
    };

    public virtual async Task<LeavePlan> PlanAsync(Employee owner, LeavePlanInput input)
    {
        if (owner.Status != EmployeeStatus.Active)
        {
            throw new BusinessException(HrErrorCodes.Leave.EmployeeNotActive);
        }

        var isHourly = input.Kind == LeaveKind.Hourly;
        var rangeStart = isHourly ? input.Date!.Value : input.StartDate!.Value;
        var rangeEnd = isHourly ? input.Date!.Value : input.EndDate!.Value;

        if (rangeEnd < rangeStart || rangeStart < owner.HireDate || (isHourly && input.FromTime == input.ToTime))
        {
            throw new BusinessException(HrErrorCodes.Leave.InvalidDateRange);
        }

        var plan = new LeavePlan
        {
            Kind = input.Kind,
            Date = input.Date,
            FromTime = input.FromTime,
            ToTime = input.ToTime,
            StartDate = input.StartDate,
            EndDate = input.EndDate,
            RangeStart = rangeStart,
            RangeEnd = rangeEnd,
            CalendarDays = rangeEnd.DayNumber - rangeStart.DayNumber + 1,
            DeductsFromAnnualBalance = input.Kind == LeaveKind.Annual
        };

        var snapshot = await _balances.GetSnapshotAsync(owner);

        // The furthest a leave may reach is the end of the coming year.
        var lastAllowed = LeaveYearMath.NextStart(owner.HireDate, snapshot.NextYearStart).AddDays(-1);
        if (rangeEnd > lastAllowed)
        {
            plan.Issues.Add(HrErrorCodes.Leave.RangeBeyondNextYear);
            return plan;
        }

        plan.DayBreakdown = await _schedule.GetAsync(owner.Id, rangeStart, rangeEnd);
        if (plan.DayBreakdown.Any(x => !x.HasAssignment))
        {
            throw new BusinessException(HrErrorCodes.Leave.NoShiftAssigned);
        }

        if (isHourly)
        {
            CheckTimesAgainstShift(plan);
        }

        if (input.Kind == LeaveKind.Other)
        {
            await ApplyOtherTypeAsync(plan, owner, input.OtherLeaveTypeId!.Value);
        }

        await CheckOverlapAsync(plan, owner);

        if (isHourly)
        {
            await PlanHourlyAsync(plan, owner, snapshot);
        }
        else
        {
            await PlanDaysAsync(plan, owner, snapshot);
        }

        return plan;
    }

    private static void CheckTimesAgainstShift(LeavePlan plan)
    {
        var shiftDay = plan.DayBreakdown[0].ShiftDay;
        var crosses = shiftDay is { CrossesMidnight: true };

        if (plan.ToTime < plan.FromTime && !crosses)
        {
            throw new BusinessException(HrErrorCodes.Leave.InvalidDateRange);
        }

        plan.RequestedHours = LeaveYearMath.Round((decimal)SpanOf(plan.FromTime!.Value, plan.ToTime!.Value).TotalHours);

        if (shiftDay == null)
        {
            plan.Issues.Add(HrErrorCodes.Leave.HourlyOutsideShift);
            return;
        }

        var start = TimeSpan.Zero;
        var end = SpanOf(shiftDay.StartTime, shiftDay.EndTime);
        var from = crosses && plan.FromTime < shiftDay.StartTime
            ? plan.FromTime.Value.ToTimeSpan() + TimeSpan.FromDays(1) - shiftDay.StartTime.ToTimeSpan()
            : plan.FromTime.Value.ToTimeSpan() - shiftDay.StartTime.ToTimeSpan();
        var to = from + SpanOf(plan.FromTime.Value, plan.ToTime.Value);

        if (from < start || to > end)
        {
            plan.Issues.Add(HrErrorCodes.Leave.HourlyOutsideShift);
        }
    }

    private static TimeSpan SpanOf(TimeOnly from, TimeOnly to) =>
        to > from ? to - from : to.ToTimeSpan() + TimeSpan.FromDays(1) - from.ToTimeSpan();

    private async Task ApplyOtherTypeAsync(LeavePlan plan, Employee owner, Guid typeId)
    {
        var type = await _otherTypes.FindAsync(typeId) ?? throw new EntityNotFoundException(typeof(OtherLeaveType), typeId);
        plan.OtherLeaveType = type;
        plan.DeductsFromAnnualBalance = type.DeductsFromAnnualBalance;

        if (!type.IsActive)
        {
            plan.Issues.Add(HrErrorCodes.Leave.OtherLeaveTypeInactive);
        }

        if (!IsEligible(type, owner.Gender))
        {
            plan.Issues.Add(HrErrorCodes.Leave.NotEligibleForLeaveType);
        }
    }

    private async Task CheckOverlapAsync(LeavePlan plan, Employee owner)
    {
        var others = await _leaves.GetListAsync(x =>
            x.EmployeeId == owner.Id &&
            x.State != LeaveRequestState.Closed &&
            x.RangeStart <= plan.RangeEnd &&
            x.RangeEnd >= plan.RangeStart);

        var overlaps = others.Any(other =>
            other.Kind != LeaveKind.Hourly ||
            plan.Kind != LeaveKind.Hourly ||
            (other.FromTime!.Value < plan.ToTime!.Value && plan.FromTime!.Value < other.ToTime!.Value));

        if (overlaps)
        {
            plan.Issues.Add(HrErrorCodes.Leave.OverlappingLeave);
        }
    }

    private async Task PlanHourlyAsync(LeavePlan plan, Employee owner, LeaveBalanceSnapshot snapshot)
    {
        var settings = await _settings.PeekValueAsync<HourlyLeaveSettings>();
        var requested = plan.RequestedHours!.Value;
        var date = plan.Date!.Value;
        var day = plan.DayBreakdown[0];

        plan.ExcludedWeeklyOffDays = day.Classification == LeaveDayClassification.WeeklyOff ? 1 : 0;
        plan.ExcludedHolidayDays = day.Classification == LeaveDayClassification.Holiday ? 1 : 0;
        plan.DeductsFromAnnualBalance = true;

        if (settings.MonthlyAllowedHours is null or <= 0 || settings.HoursPerDay is null or <= 0)
        {
            plan.Issues.Add(HrErrorCodes.Leave.HourlySettingsNotConfigured);
            return;
        }

        plan.HourlyMonth = date.ToString("yyyy-MM");
        var quotaLeft = Math.Max(0, settings.MonthlyAllowedHours.Value + await _balances.GetHourlyMonthSumAsync(owner.Id, plan.HourlyMonth));
        var fromQuota = Math.Min(requested, quotaLeft);
        var excess = requested - fromQuota;
        var annualDays = LeaveYearMath.Round(excess / settings.HoursPerDay.Value);

        plan.AvailableHourlyHoursBefore = LeaveYearMath.Round(quotaLeft);
        plan.HourlyQuotaHoursUsed = LeaveYearMath.Round(fromQuota);
        plan.HourlyExcessHours = LeaveYearMath.Round(excess);

        var (start, end) = LeaveYearMath.YearContaining(owner.HireDate, date);
        var isFuture = start > snapshot.YearEnd;

        ApplyAllocation(plan, snapshot, new[] { new YearGroup(start, end, isFuture, null, null, annualDays) }, annualDays, true);
    }

    private async Task PlanDaysAsync(LeavePlan plan, Employee owner, LeaveBalanceSnapshot snapshot)
    {
        var counted = plan.DayBreakdown.Where(x => x.Classification == LeaveDayClassification.WorkingDay).ToList();
        plan.WorkingDays = counted.Count;
        plan.ExcludedWeeklyOffDays = plan.DayBreakdown.Count(x => x.Classification == LeaveDayClassification.WeeklyOff);
        plan.ExcludedHolidayDays = plan.DayBreakdown.Count(x => x.Classification == LeaveDayClassification.Holiday);

        if (counted.Count == 0)
        {
            plan.Issues.Add(HrErrorCodes.Leave.NoWorkingDaysInRange);
        }

        var groups = plan.DayBreakdown
            .GroupBy(x => LeaveYearMath.YearContaining(owner.HireDate, x.Date).Start)
            .OrderBy(x => x.Key)
            .Select(group =>
            {
                var (start, end) = LeaveYearMath.YearContaining(owner.HireDate, group.Key);
                return new YearGroup(
                    start,
                    end,
                    start > snapshot.YearEnd,
                    group.Min(x => x.Date),
                    group.Max(x => x.Date),
                    group.Count(x => x.Classification == LeaveDayClassification.WorkingDay));
            })
            .ToList();

        if (plan.OtherLeaveType?.MaxDays is { } maxDays)
        {
            foreach (var group in groups)
            {
                if (await GetOtherTypeDaysInYearAsync(owner.Id, plan.OtherLeaveType.Id, group.Start) + group.WorkingDays > maxDays)
                {
                    plan.Issues.Add(HrErrorCodes.Leave.OtherLeaveTypeMaxExceeded);
                    break;
                }
            }
        }

        var annualDays = plan.DeductsFromAnnualBalance ? plan.WorkingDays : 0;
        ApplyAllocation(plan, snapshot, groups, annualDays, plan.DeductsFromAnnualBalance);
    }

    /// <summary>Days of this type already pending or approved in a personal year.</summary>
    public virtual async Task<decimal> GetOtherTypeDaysInYearAsync(Guid employeeId, Guid typeId, DateOnly yearStart)
    {
        var leaves = await _leaves.GetQueryableAsync();
        var query = from segment in await _segments.GetQueryableAsync()
                    join leave in leaves on segment.LeaveRequestId equals leave.Id
                    where leave.EmployeeId == employeeId &&
                          leave.OtherLeaveTypeId == typeId &&
                          leave.State != LeaveRequestState.Closed &&
                          segment.YearStart == yearStart
                    select segment.WorkingDays;

        return await _leaves.AsyncExecuter.SumAsync(query);
    }

    private static void ApplyAllocation(LeavePlan plan, LeaveBalanceSnapshot snapshot, IReadOnlyList<YearGroup> groups, decimal annualDays, bool usesBalance)
    {
        plan.AnnualDays = annualDays;

        var isSet = snapshot.CurrentYear != null;
        if (usesBalance && !isSet)
        {
            plan.Issues.Add(HrErrorCodes.Leave.EntitlementNotSet);
        }

        var demands = groups.Select(x => new LeaveDemand(x.IsFuture, usesBalance ? x.WorkingDays : 0)).ToList();
        var result = LeaveAllocator.Allocate(snapshot.Pools, demands);

        plan.AvailableDaysBefore = result.AvailableBefore;
        plan.RemainingDaysAfter = result.RemainingAfter;
        plan.ShortageDays = result.Shortage;
        plan.FromCarriedDays = result.FromCarried;
        plan.FromEntitlementDays = result.FromEntitlement;
        plan.Segments = groups.Select((x, i) => new LeavePlanSegment(
            x.Start,
            x.End,
            x.IsFuture,
            x.First,
            x.Last,
            x.WorkingDays,
            result.Allocations[i].FromCarried,
            result.Allocations[i].FromEntitlement)).ToList();

        if (usesBalance && isSet && result.Shortage > 0)
        {
            plan.Issues.Add(HrErrorCodes.Leave.InsufficientBalance);
        }
    }

    private sealed record YearGroup(DateOnly Start, DateOnly End, bool IsFuture, DateOnly? First, DateOnly? Last, decimal WorkingDays);
}

/// <summary>The balance side of a leave's decisions on the general request.</summary>
[ExposeServices(typeof(IRequestLifecycleHandler))]
public class LeaveLifecycleHandler : IRequestLifecycleHandler, ITransientDependency
{
    private readonly IRepository<LeaveRequest, Guid> _leaves;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly LeaveBalanceManager _balances;
    private readonly IAttendanceRecalculator _attendance;
    private readonly ICompanyClock _companyClock;

    public LeaveLifecycleHandler(
        IRepository<LeaveRequest, Guid> leaves,
        IRepository<Employee, Guid> employees,
        LeaveBalanceManager balances,
        IAttendanceRecalculator attendance,
        ICompanyClock companyClock)
    {
        _leaves = leaves;
        _employees = employees;
        _balances = balances;
        _attendance = attendance;
        _companyClock = companyClock;
    }

    public virtual async Task OnApprovedAsync(Request request)
    {
        var leave = await _leaves.FindAsync(request.Id);
        if (leave == null)
        {
            return;
        }

        await _balances.DeductHoldsAsync(leave.EmployeeId, leave.Id);
        await _balances.TouchAsync(await _balances.GetOrCreateAccountAsync(leave.EmployeeId));
        leave.MarkApproved();
        await _leaves.UpdateAsync(leave, autoSave: true);

        // Future dates get their row when the day arrives, and the calculation reads this leave then.
        var today = await _companyClock.GetTodayAsync();
        if (leave.RangeStart <= today)
        {
            await _attendance.RecalculateRangesAsync(new[]
            {
                new AttendanceRecalculationRange
                {
                    EmployeeId = leave.EmployeeId,
                    From = leave.RangeStart,
                    To = leave.RangeEnd < today ? leave.RangeEnd : today
                }
            });
        }
    }

    public virtual Task OnRejectedAsync(Request request) => CloseAsync(request);

    public virtual Task OnCancelledAsync(Request request) => CloseAsync(request);

    private async Task CloseAsync(Request request)
    {
        var leave = await _leaves.FindAsync(request.Id);
        if (leave == null)
        {
            return;
        }

        await _balances.ReleaseHoldsAsync(await _employees.GetAsync(leave.EmployeeId), leave.Id);
        leave.MarkClosed();
        await _leaves.UpdateAsync(leave, autoSave: true);
    }
}

/// <summary>Approved leave as attendance sees it.</summary>
public class AttendanceLeaveReader : IAttendanceLeaveReader, ITransientDependency
{
    private readonly IRepository<LeaveRequest, Guid> _leaves;
    private readonly ICompanyClock _companyClock;

    public AttendanceLeaveReader(IRepository<LeaveRequest, Guid> leaves, ICompanyClock companyClock)
    {
        _leaves = leaves;
        _companyClock = companyClock;
    }

    public virtual async Task<Dictionary<DateOnly, AttendanceLeaveCoverage>> GetCoverageAsync(Guid employeeId, DateOnly from, DateOnly to)
    {
        var approved = await _leaves.GetListAsync(x =>
            x.EmployeeId == employeeId &&
            x.State == LeaveRequestState.Approved &&
            x.RangeStart <= to &&
            x.RangeEnd >= from);

        var result = new Dictionary<DateOnly, AttendanceLeaveCoverage>();
        if (approved.Count == 0)
        {
            return result;
        }

        var zone = await _companyClock.GetTimeZoneAsync();

        foreach (var leave in approved.Where(x => x.Kind != LeaveKind.Hourly))
        {
            var first = leave.RangeStart > from ? leave.RangeStart : from;
            var last = leave.RangeEnd < to ? leave.RangeEnd : to;
            for (var date = first; date <= last; date = date.AddDays(1))
            {
                result[date] = new AttendanceLeaveCoverage(leave.Id, true, 1, null, null, null);
            }
        }

        // Several hourly leaves can share a day. The minutes add up, and each one excuses lateness at its own time (Q63).
        foreach (var group in approved.Where(x => x.Kind == LeaveKind.Hourly).GroupBy(x => x.Date!.Value).Where(x => x.Key >= from && x.Key <= to && !result.ContainsKey(x.Key)))
        {
            var ordered = group.OrderBy(x => x.FromTime).ToList();
            var earliest = ordered[0];
            var coveredFrom = CompanyClock.ToUtc(group.Key, earliest.FromTime!.Value, zone);
            var toDate = earliest.ToTime < earliest.FromTime ? group.Key.AddDays(1) : group.Key;
            var others = ordered.Skip(1).Select(x => new AttendanceCoveredPeriod(
                CompanyClock.ToUtc(group.Key, x.FromTime!.Value, zone),
                CompanyClock.ToUtc(x.ToTime < x.FromTime ? group.Key.AddDays(1) : group.Key, x.ToTime!.Value, zone))).ToList();

            result[group.Key] = new AttendanceLeaveCoverage(
                earliest.Id,
                false,
                0,
                group.Sum(x => (int)Math.Round((x.RequestedHours ?? 0) * 60)),
                coveredFrom,
                CompanyClock.ToUtc(toDate, earliest.ToTime!.Value, zone),
                others);
        }

        return result;
    }
}

/// <summary>Ledger entries lock the hire date, because every personal year is counted from it.</summary>
public class LeaveLedgerReader : ILeaveLedgerReader, ITransientDependency
{
    private readonly IRepository<LeaveLedgerEntry, Guid> _ledger;

    public LeaveLedgerReader(IRepository<LeaveLedgerEntry, Guid> ledger)
    {
        _ledger = ledger;
    }

    public virtual Task<bool> HasEntriesAsync(Guid employeeId) => _ledger.AnyAsync(x => x.EmployeeId == employeeId);
}
