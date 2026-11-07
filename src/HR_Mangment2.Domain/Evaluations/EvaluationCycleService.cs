using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HR_Mangment2.Attendance;
using HR_Mangment2.Common;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Notifications;
using HR_Mangment2.Requests;
using HR_Mangment2.Settings;
using HR_Mangment2.Store;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace HR_Mangment2.Evaluations;

/// <summary>What one manager still owes for a cycle. A department counts as done only when every evaluation and the report are in.</summary>
public record MonthlyCompletion(int Expected, int Submitted, bool ReportSubmitted, bool IsCompleted, bool IsLate);

/// <summary>Reads completion the same way for the cycle screen, the report, the reviewer and the reminders.</summary>
public class MonthlyCompletionReader : ITransientDependency
{
    private readonly IRepository<Evaluation, Guid> _evaluations;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IAsyncQueryableExecuter _executer;

    public MonthlyCompletionReader(IRepository<Evaluation, Guid> evaluations, IRepository<Employee, Guid> employees, IAsyncQueryableExecuter executer)
    {
        _evaluations = evaluations;
        _employees = employees;
        _executer = executer;
    }

    /// <summary>The employee files this user manages right now.</summary>
    public virtual async Task<List<Guid>> GetCurrentStaffAsync(Guid managerUserId)
    {
        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == managerUserId);
        if (me == null)
        {
            return new List<Guid>();
        }

        return await _executer.ToListAsync((await _employees.GetQueryableAsync())
            .Where(x => x.ManagerEmployeeId == me.Id && x.Status != EmployeeStatus.Ended)
            .Select(x => x.Id));
    }

    public virtual async Task<MonthlyCompletion> ReadAsync(EvaluationCycle cycle, IReadOnlyCollection<Guid> staff, DepartmentReport? report, DateTime now)
    {
        var forms = await _executer.ToListAsync((await _evaluations.GetQueryableAsync())
            .Where(x => x.CycleId == cycle.Id && staff.Contains(x.EmployeeId))
            .Select(x => new { x.Status, x.SubmittedAt }));

        var expected = forms.Count;
        var submitted = forms.Count(x => x.Status != EvaluationStatus.Draft);
        var reportSubmitted = report is { Status: DepartmentReportStatus.Submitted };
        var completed = reportSubmitted && submitted == expected;

        // Handing in after the deadline is allowed, and stays late. Nothing submitted yet is late once the mark date passes.
        var finishedAt = completed
            ? forms.Select(x => x.SubmittedAt).Append(report!.SubmittedAt).Max()
            : now;

        return new MonthlyCompletion(expected, submitted, reportSubmitted, completed, finishedAt >= cycle.EscalatesAt);
    }
}

/// <summary>
/// The monthly timetable: scheduled, then open on the last working day, then frozen with the report generated on the first
/// working day of the next month, with reminders until the deadline and one escalation. Safe to run again at any time.
/// </summary>
public class EvaluationCycleService : ITransientDependency
{
    private const int FirstReminderHour = 8;

    private readonly IRepository<EvaluationCycle, Guid> _cycles;
    private readonly IRepository<EvaluationCriterion, Guid> _criteria;
    private readonly IRepository<EvaluationGrade, Guid> _grades;
    private readonly IRepository<Evaluation, Guid> _evaluations;
    private readonly IRepository<DepartmentReport, Guid> _reports;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IRepository<Request, Guid> _requests;
    private readonly IRepository<Custody, Guid> _custodies;
    private readonly IRepository<CustodyCount, Guid> _counts;
    private readonly IRepository<Circular, Guid> _circulars;
    private readonly WorkCalendarService _calendar;
    private readonly ICompanyClock _companyClock;
    private readonly HrSettingsStore _settings;
    private readonly AttendanceSummaryReader _attendance;
    private readonly MonthlyCompletionReader _completion;
    private readonly HrNotificationPublisher _notifications;
    private readonly IdentityUserManager _userManager;
    private readonly IIdentityUserRepository _users;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly IAsyncQueryableExecuter _executer;

    public EvaluationCycleService(
        IRepository<EvaluationCycle, Guid> cycles,
        IRepository<EvaluationCriterion, Guid> criteria,
        IRepository<EvaluationGrade, Guid> grades,
        IRepository<Evaluation, Guid> evaluations,
        IRepository<DepartmentReport, Guid> reports,
        IRepository<Employee, Guid> employees,
        IRepository<Request, Guid> requests,
        IRepository<Custody, Guid> custodies,
        IRepository<CustodyCount, Guid> counts,
        IRepository<Circular, Guid> circulars,
        WorkCalendarService calendar,
        ICompanyClock companyClock,
        HrSettingsStore settings,
        AttendanceSummaryReader attendance,
        MonthlyCompletionReader completion,
        HrNotificationPublisher notifications,
        IdentityUserManager userManager,
        IIdentityUserRepository users,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IAsyncQueryableExecuter executer)
    {
        _cycles = cycles;
        _criteria = criteria;
        _grades = grades;
        _evaluations = evaluations;
        _reports = reports;
        _employees = employees;
        _requests = requests;
        _custodies = custodies;
        _counts = counts;
        _circulars = circulars;
        _calendar = calendar;
        _companyClock = companyClock;
        _settings = settings;
        _attendance = attendance;
        _completion = completion;
        _notifications = notifications;
        _userManager = userManager;
        _users = users;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _executer = executer;
    }

    public virtual async Task ProcessAsync(DateTime now)
    {
        var zone = await _companyClock.GetTimeZoneAsync();
        var local = TimeZoneInfo.ConvertTimeFromUtc(now, zone);
        await EnsureCycleExistsAsync(local.Year, local.Month);

        var active = await _executer.ToListAsync((await _cycles.WithDetailsAsync(x => x.Criteria, x => x.Grades))
            .Where(x => x.Status != EvaluationCycleStatus.Closed)
            .OrderBy(x => x.Year).ThenBy(x => x.Month));

        foreach (var cycle in active)
        {
            await RescheduleAsync(cycle);

            if (cycle.Status == EvaluationCycleStatus.Scheduled && now >= cycle.OpensAt)
            {
                await OpenAsync(cycle, active);
            }

            if (cycle.Status != EvaluationCycleStatus.Open)
            {
                continue;
            }

            if (cycle.FrozenAt == null && now >= cycle.DataFreezeAt)
            {
                await FreezeAsync(cycle, now);
            }

            if (cycle.FrozenAt != null)
            {
                await SendRemindersAsync(cycle, now, local);
                await EscalateAsync(cycle, now);
            }
        }
    }

    private async Task EnsureCycleExistsAsync(int year, int month)
    {
        if (await _cycles.AnyAsync(x => x.Year == year && x.Month == month))
        {
            return;
        }

        var (opensAt, freezeAt, dueAt, escalatesAt) = await ComputeScheduleAsync(year, month);
        await _cycles.InsertAsync(
            new EvaluationCycle(_guidGenerator.Create(), _currentTenant.Id, year, month, opensAt, freezeAt, dueAt, escalatesAt),
            autoSave: true);
    }

    /// <summary>Every deadline comes from settings. What already happened stays: a cycle that opened keeps its opening time, and a frozen one its freeze.</summary>
    private async Task RescheduleAsync(EvaluationCycle cycle)
    {
        var (opensAt, freezeAt, dueAt, escalatesAt) = await ComputeScheduleAsync(cycle.Year, cycle.Month);
        opensAt = cycle.Status == EvaluationCycleStatus.Scheduled ? opensAt : cycle.OpensAt;
        freezeAt = cycle.FrozenAt == null ? freezeAt : cycle.DataFreezeAt;

        if (opensAt != cycle.OpensAt || freezeAt != cycle.DataFreezeAt || dueAt != cycle.DueAt || escalatesAt != cycle.EscalatesAt)
        {
            cycle.Reschedule(opensAt, freezeAt, dueAt, escalatesAt);
            await _cycles.UpdateAsync(cycle, autoSave: true);
        }
    }

    private async Task<(DateTime OpensAt, DateTime FreezeAt, DateTime DueAt, DateTime EscalatesAt)> ComputeScheduleAsync(int year, int month)
    {
        var settings = await _settings.PeekValueAsync<ReminderSettings>();
        var lastDay = new DateOnly(year, month, 1).AddMonths(1).AddDays(-1);

        var lastWorkingDay = await _calendar.GetLastWorkingDayOfMonthAsync(year, month);
        var opensAt = (await _companyClock.StartOfDayUtcAsync(lastWorkingDay)).Add(settings.MonthEndOpenTime.ToTimeSpan());

        var freezeDay = await _calendar.AddWorkingDaysAsync(lastDay, 1) ?? lastDay.AddDays(1);
        var dueDay = await _calendar.AddWorkingDaysAsync(lastDay, settings.MonthlySubmissionWorkingDays) ?? freezeDay;
        var escalationDay = await _calendar.AddWorkingDaysAsync(lastDay, settings.MonthlyEscalationWorkingDay) ?? dueDay;

        return (
            opensAt,
            await _companyClock.StartOfDayUtcAsync(freezeDay),
            await _companyClock.StartOfDayUtcAsync(dueDay.AddDays(1)),
            await _companyClock.StartOfDayUtcAsync(escalationDay));
    }

    private async Task OpenAsync(EvaluationCycle cycle, IEnumerable<EvaluationCycle> active)
    {
        // Late hand-ins stay allowed until the next cycle takes over.
        foreach (var earlier in active.Where(x => x.Id != cycle.Id && x.Status == EvaluationCycleStatus.Open))
        {
            earlier.Close();
            await _cycles.UpdateAsync(earlier, autoSave: true);
        }

        foreach (var criterion in await _criteria.GetListAsync(x => x.IsActive))
        {
            cycle.Criteria.Add(new EvaluationCycleCriterion(_guidGenerator.Create(), _currentTenant.Id, cycle.Id, criterion));
        }

        foreach (var grade in await _grades.GetListAsync(x => x.IsActive))
        {
            cycle.Grades.Add(new EvaluationCycleGrade(_guidGenerator.Create(), _currentTenant.Id, cycle.Id, grade));
        }

        cycle.Open();
        await _cycles.UpdateAsync(cycle, autoSave: true);

        var perManager = new Dictionary<Guid, int>();
        foreach (var employee in await GetEligibleAsync(cycle))
        {
            await _evaluations.InsertAsync(new Evaluation(_guidGenerator.Create(), _currentTenant.Id, cycle.Id, employee.EmployeeId));
            perManager[employee.ManagerUserId] = perManager.GetValueOrDefault(employee.ManagerUserId) + 1;
        }

        foreach (var (managerUserId, count) in perManager)
        {
            await _notifications.PublishAsync(managerUserId, NotificationTemplateCode.MonthEndCycleOpened, NotificationResourceType.Evaluation, cycle.Id, count);
        }
    }

    /// <summary>Everyone with a department manager, who is not one, and who worked long enough in the month. A full month always qualifies.</summary>
    private async Task<List<(Guid EmployeeId, Guid ManagerUserId)>> GetEligibleAsync(EvaluationCycle cycle)
    {
        var minimum = (await _settings.PeekValueAsync<ReminderSettings>()).EvaluationMinWorkedDays;
        var employees = await _employees.GetQueryableAsync();

        var candidates = await _executer.ToListAsync(
            from staff in employees
            join manager in employees on staff.ManagerEmployeeId equals manager.Id
            where staff.Status == EmployeeStatus.Active && manager.UserId != null && staff.HireDate <= cycle.PeriodTo
            select new { Staff = staff, ManagerUserId = manager.UserId!.Value });

        var managerRoles = new Dictionary<Guid, bool>();
        var result = new List<(Guid, Guid)>();
        foreach (var candidate in candidates)
        {
            if (!managerRoles.TryGetValue(candidate.ManagerUserId, out var isManager))
            {
                isManager = (await _users.GetRoleNamesAsync(candidate.ManagerUserId)).Contains(HrRoles.DepartmentManager);
                managerRoles[candidate.ManagerUserId] = isManager;
            }

            if (!isManager)
            {
                continue;
            }

            if (candidate.Staff.UserId != null && (await _users.GetRoleNamesAsync(candidate.Staff.UserId.Value)).Contains(HrRoles.DepartmentManager))
            {
                continue;
            }

            if (minimum != null && candidate.Staff.HireDate > cycle.PeriodFrom)
            {
                var worked = await _calendar.CountWorkingDaysAfterAsync(candidate.Staff.HireDate.AddDays(-1), cycle.PeriodTo);
                if (worked < minimum)
                {
                    continue;
                }
            }

            result.Add((candidate.Staff.Id, candidate.ManagerUserId));
        }

        return result;
    }

    private async Task FreezeAsync(EvaluationCycle cycle, DateTime now)
    {
        var forms = await _evaluations.GetListAsync(x => x.CycleId == cycle.Id);
        var sums = await _attendance.SumAsync(forms.Select(x => x.EmployeeId).ToList(), cycle.PeriodFrom, cycle.PeriodTo);
        foreach (var form in forms)
        {
            var sum = sums.GetValueOrDefault(form.EmployeeId) ?? AttendanceSum.Zero;
            form.FreezeAttendance(now, sum.ScheduledDays, sum.PresentDays, sum.AbsentDays, sum.LateCount, sum.LateMinutes, sum.LeaveDays);
        }

        await _evaluations.UpdateManyAsync(forms, autoSave: true);

        foreach (var manager in await _userManager.GetUsersInRoleAsync(HrRoles.DepartmentManager))
        {
            await GenerateReportAsync(cycle, manager.Id, now);
        }

        cycle.MarkFrozen(now);
        await _cycles.UpdateAsync(cycle, autoSave: true);
    }

    private async Task GenerateReportAsync(EvaluationCycle cycle, Guid managerUserId, DateTime now)
    {
        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == managerUserId);
        if (me == null || await _reports.AnyAsync(x => x.CycleId == cycle.Id && x.ManagerUserId == managerUserId))
        {
            return;
        }

        var staff = await _employees.GetListAsync(x => x.ManagerEmployeeId == me.Id && x.Status != EmployeeStatus.Ended);
        if (staff.Count == 0)
        {
            return;
        }

        var staffIds = staff.Select(x => x.Id).ToList();
        var departmentIds = staff.Select(x => x.DepartmentId).Distinct().ToList();
        var sums = await _attendance.SumAsync(staffIds, cycle.PeriodFrom, cycle.PeriodTo);

        var pending = (await _executer.ToListAsync((await _requests.GetQueryableAsync())
                .Where(x => staffIds.Contains(x.EmployeeId) && x.CurrentStepOrder != null)
                .GroupBy(x => x.EmployeeId)
                .Select(g => new { g.Key, Count = g.Count() })))
            .ToDictionary(x => x.Key, x => x.Count);

        var openCustody = await _custodies.GetListAsync(x =>
            x.Status != CustodyStatus.Returned &&
            ((x.EmployeeId != null && staffIds.Contains(x.EmployeeId.Value)) || (x.DepartmentId != null && departmentIds.Contains(x.DepartmentId.Value))));

        var lastCount = await _executer.ToListAsync((await _counts.GetQueryableAsync())
            .Where(x => departmentIds.Contains(x.DepartmentId) && x.CountDate <= cycle.PeriodTo)
            .OrderByDescending(x => x.CountDate)
            .Select(x => x.CountDate)
            .Take(1));

        var from = await _companyClock.StartOfDayUtcAsync(cycle.PeriodFrom);
        var before = await _companyClock.StartOfDayUtcAsync(cycle.PeriodTo.AddDays(1));
        var sent = await _executer.ToListAsync((await _circulars.WithDetailsAsync(x => x.Recipients))
            .Where(x => x.SenderUserId == managerUserId && x.SentAt >= from && x.SentAt < before));

        var snapshot = new DepartmentReportSnapshot
        {
            PeriodFrom = cycle.PeriodFrom,
            PeriodTo = cycle.PeriodTo,
            FrozenAt = now,
            Employees = staff.Select(x =>
            {
                var sum = sums.GetValueOrDefault(x.Id) ?? AttendanceSum.Zero;
                return new DepartmentReportEmployeeRow
                {
                    EmployeeId = x.Id,
                    PresentDays = sum.PresentDays,
                    AbsentDays = sum.AbsentDays,
                    LateCount = sum.LateCount,
                    LateMinutes = sum.LateMinutes,
                    LeaveDays = sum.LeaveDays,
                    PendingRequestsCount = pending.GetValueOrDefault(x.Id)
                };
            }).ToList(),
            SharedItemsCount = openCustody.Count(x => x.Kind == CustodyKind.Shared),
            PersonalItemsCount = openCustody.Count(x => x.Kind == CustodyKind.Personal),
            UnacknowledgedItemsCount = openCustody.Count(x => x.Status == CustodyStatus.PendingAcknowledgement),
            LastCountDate = lastCount.Count == 0 ? null : lastCount[0],
            CircularsSentCount = sent.Count,
            CircularRecipientsCount = sent.Sum(x => x.Recipients.Count),
            CircularAcknowledgedCount = sent.Sum(x => x.Recipients.Count(r => r.AcknowledgedAt != null))
        };

        await _reports.InsertAsync(
            new DepartmentReport(
                _guidGenerator.Create(),
                _currentTenant.Id,
                cycle.Id,
                managerUserId,
                departmentIds.Count == 1 ? departmentIds[0] : null,
                JsonSerializer.Serialize(snapshot)),
            autoSave: true);
    }

    /// <summary>Through the deadline, each manager still owing something, at the configured interval of working days.</summary>
    private async Task SendRemindersAsync(EvaluationCycle cycle, DateTime now, DateTime local)
    {
        if (now >= cycle.DueAt || local.Hour < FirstReminderHour)
        {
            return;
        }

        var settings = await _settings.PeekValueAsync<ReminderSettings>();
        var today = DateOnly.FromDateTime(local);
        if (cycle.LastReminderAt != null)
        {
            var zone = await _companyClock.GetTimeZoneAsync();
            var lastDay = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(cycle.LastReminderAt.Value, zone));
            if (await _calendar.CountWorkingDaysAfterAsync(lastDay, today) < settings.MonthlyReminderIntervalWorkingDays)
            {
                return;
            }
        }

        foreach (var (managerUserId, remaining) in await GetOutstandingAsync(cycle, now))
        {
            await _notifications.PublishAsync(managerUserId, NotificationTemplateCode.MonthlyCycleReminder, NotificationResourceType.Evaluation, cycle.Id, remaining);
        }

        cycle.MarkReminded(now);
        await _cycles.UpdateAsync(cycle, autoSave: true);
    }

    /// <summary>Once per cycle, to the HR manager. It changes nothing by itself: the reviewer's late mark comes from the same date.</summary>
    private async Task EscalateAsync(EvaluationCycle cycle, DateTime now)
    {
        if (cycle.EscalatedAt != null || now < cycle.EscalatesAt)
        {
            return;
        }

        var outstanding = await GetOutstandingAsync(cycle, now);
        if (outstanding.Count > 0)
        {
            foreach (var hr in (await _userManager.GetUsersInRoleAsync(HrRoles.HrManager)).Where(x => x.IsActive))
            {
                await _notifications.PublishAsync(hr.Id, NotificationTemplateCode.MonthlyCycleEscalated, NotificationResourceType.Evaluation, cycle.Id, outstanding.Count);
            }
        }

        cycle.MarkEscalated(now);
        await _cycles.UpdateAsync(cycle, autoSave: true);
    }

    /// <summary>Managers with a report for the cycle who have not finished, and how many items each still owes.</summary>
    private async Task<List<(Guid ManagerUserId, int Remaining)>> GetOutstandingAsync(EvaluationCycle cycle, DateTime now)
    {
        var result = new List<(Guid, int)>();
        foreach (var report in await _reports.GetListAsync(x => x.CycleId == cycle.Id))
        {
            var snapshot = JsonSerializer.Deserialize<DepartmentReportSnapshot>(report.SnapshotJson)!;
            var completion = await _completion.ReadAsync(cycle, snapshot.Employees.Select(x => x.EmployeeId).ToList(), report, now);
            if (!completion.IsCompleted)
            {
                result.Add((report.ManagerUserId, completion.Expected - completion.Submitted + (completion.ReportSubmitted ? 0 : 1)));
            }
        }

        return result;
    }
}

public class EvaluationCycleWorker : AsyncPeriodicBackgroundWorkerBase
{
    public EvaluationCycleWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = (int)TimeSpan.FromMinutes(15).TotalMilliseconds;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var services = workerContext.ServiceProvider;
        var unitOfWorkManager = services.GetRequiredService<IUnitOfWorkManager>();
        var currentTenant = services.GetRequiredService<ICurrentTenant>();
        var now = services.GetRequiredService<IClock>().Now;

        List<Guid> tenantIds;
        using (var read = unitOfWorkManager.Begin(requiresNew: true))
        {
            tenantIds = (await services.GetRequiredService<ITenantRepository>().GetListAsync()).Select(x => x.Id).ToList();
            await read.CompleteAsync();
        }

        foreach (var tenantId in tenantIds)
        {
            using (currentTenant.Change(tenantId))
            using (var unitOfWork = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                await services.GetRequiredService<EvaluationCycleService>().ProcessAsync(now);
                await unitOfWork.CompleteAsync();
            }
        }
    }
}
