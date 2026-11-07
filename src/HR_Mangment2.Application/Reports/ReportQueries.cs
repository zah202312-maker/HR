using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using HR_Mangment2.Attendance;
using HR_Mangment2.Employees;
using HR_Mangment2.Evaluations;
using HR_Mangment2.Identity;
using HR_Mangment2.Leaves;
using HR_Mangment2.Localization;
using HR_Mangment2.Requests;
using Microsoft.Extensions.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Timing;

namespace HR_Mangment2.Reports;

public record ReportPage(int Skip, int Take, string? Sorting)
{
    /// <summary>An export carries the same rows as the report, without the page limit.</summary>
    public static readonly ReportPage All = new(0, int.MaxValue, null);
}

/// <summary>
/// The rows behind both the JSON reports and their exports, so a file can never hold a field or a person the report
/// does not. Callers check permissions; the team reports narrow to the current user's direct reports here.
/// </summary>
public class ReportQueries : ITransientDependency
{
    private static readonly Dictionary<string, string> AttendanceSort = new()
    {
        ["date"] = nameof(AttendanceDay.Date),
        ["lateMinutes"] = nameof(AttendanceDay.LateMinutes)
    };

    private static readonly Dictionary<string, string> LeaveSort = new()
    {
        ["from"] = nameof(LeaveRequest.RangeStart),
        ["to"] = nameof(LeaveRequest.RangeEnd)
    };

    private static readonly Dictionary<string, string> RequestSort = new()
    {
        ["creationTime"] = nameof(Request.CreationTime)
    };

    private static readonly Dictionary<string, string> LatenessSort = new()
    {
        ["fullName"] = "Employee.FullName",
        ["lateCount"] = nameof(TeamLatenessAbsenceRowDto.LateCount),
        ["lateMinutes"] = nameof(TeamLatenessAbsenceRowDto.LateMinutes),
        ["absentDays"] = nameof(TeamLatenessAbsenceRowDto.AbsentDays)
    };

    private static readonly Dictionary<string, string> HrAttendanceSort = new()
    {
        ["fullName"] = "Employee.FullName",
        ["scheduledWorkingDays"] = nameof(HrAttendanceReportRowDto.ScheduledWorkingDays),
        ["presentDays"] = nameof(HrAttendanceReportRowDto.PresentDays),
        ["absentDays"] = nameof(HrAttendanceReportRowDto.AbsentDays),
        ["lateCount"] = nameof(HrAttendanceReportRowDto.LateCount),
        ["lateMinutes"] = nameof(HrAttendanceReportRowDto.LateMinutes),
        ["leaveDays"] = nameof(HrAttendanceReportRowDto.LeaveDays)
    };

    private static readonly Dictionary<string, string> BalanceSort = new()
    {
        ["fullName"] = "Employee.FullName"
    };

    private static readonly Dictionary<string, string> CustodySort = new()
    {
        ["assignedAt"] = nameof(HR_Mangment2.Store.Custody.AssignedAt),
        ["acknowledgedAt"] = nameof(HR_Mangment2.Store.Custody.AcknowledgedAt)
    };

    private static readonly Dictionary<string, string> EvaluationSort = new()
    {
        ["submittedAt"] = nameof(Evaluation.SubmittedAt),
        ["status"] = nameof(Evaluation.Status)
    };

    private static readonly Dictionary<string, string> ConsistencySort = new()
    {
        ["displayName"] = "Manager.DisplayName",
        ["expectedCount"] = nameof(EvaluationConsistencyRowDto.ExpectedCount),
        ["submittedCount"] = nameof(EvaluationConsistencyRowDto.SubmittedCount),
        ["completionRatio"] = nameof(EvaluationConsistencyRowDto.CompletionRatio),
        ["distinctGradesUsed"] = nameof(EvaluationConsistencyRowDto.DistinctGradesUsed)
    };

    private readonly IRepository<EvaluationCycle, Guid> _cycles;
    private readonly IRepository<Evaluation, Guid> _evaluations;
    private readonly IRepository<DepartmentReport, Guid> _departmentReports;
    private readonly DepartmentReportReader _reportReader;
    private readonly AttendanceSummaryReader _attendanceSums;
    private readonly IRepository<HR_Mangment2.Store.Custody, Guid> _custodies;
    private readonly HR_Mangment2.Store.CustodyDepartmentScope _custodyScope;
    private readonly HR_Mangment2.Store.ItemReferenceReader _itemRefs;
    private readonly IRepository<AttendanceDay, Guid> _days;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IRepository<LeaveRequest, Guid> _leaves;
    private readonly IRepository<OtherLeaveType, Guid> _otherTypes;
    private readonly IRepository<Request, Guid> _requests;
    private readonly IRepository<RequestType, Guid> _requestTypes;
    private readonly LeaveBalanceManager _balances;
    private readonly EmployeeReferenceReader _references;
    private readonly ICompanyClock _companyClock;
    private readonly IReviewComplaintCounter _complaintCounter;
    private readonly IAsyncQueryableExecuter _executer;
    private readonly IStringLocalizer<HR_Mangment2Resource> _l;
    private readonly IClock _clock;

    public ReportQueries(
        IRepository<AttendanceDay, Guid> days,
        IRepository<Employee, Guid> employees,
        IRepository<LeaveRequest, Guid> leaves,
        IRepository<OtherLeaveType, Guid> otherTypes,
        IRepository<Request, Guid> requests,
        IRepository<RequestType, Guid> requestTypes,
        LeaveBalanceManager balances,
        EmployeeReferenceReader references,
        ICompanyClock companyClock,
        IReviewComplaintCounter complaintCounter,
        IAsyncQueryableExecuter executer,
        IStringLocalizer<HR_Mangment2Resource> l,
        IClock clock,
        IRepository<HR_Mangment2.Store.Custody, Guid> custodies,
        HR_Mangment2.Store.CustodyDepartmentScope custodyScope,
        HR_Mangment2.Store.ItemReferenceReader itemRefs,
        AttendanceSummaryReader attendanceSums,
        IRepository<EvaluationCycle, Guid> cycles,
        IRepository<Evaluation, Guid> evaluations,
        IRepository<DepartmentReport, Guid> departmentReports,
        DepartmentReportReader reportReader)
    {
        _cycles = cycles;
        _evaluations = evaluations;
        _departmentReports = departmentReports;
        _reportReader = reportReader;
        _attendanceSums = attendanceSums;
        _custodies = custodies;
        _custodyScope = custodyScope;
        _itemRefs = itemRefs;
        _days = days;
        _employees = employees;
        _leaves = leaves;
        _otherTypes = otherTypes;
        _requests = requests;
        _requestTypes = requestTypes;
        _balances = balances;
        _references = references;
        _companyClock = companyClock;
        _complaintCounter = complaintCounter;
        _executer = executer;
        _l = l;
        _clock = clock;
    }

    public virtual async Task<TeamAttendanceRowPagedResultDto> GetTeamAttendanceAsync(Guid userId, DateOnly from, DateOnly to, Guid? employeeId, ReportPage page)
    {
        var team = await TeamIdsAsync(userId, employeeId);
        var query = (await _days.GetQueryableAsync()).Where(x => team.Contains(x.EmployeeId) && x.Date >= from && x.Date <= to);

        var total = await _executer.LongCountAsync(query);
        var rows = await _executer.ToListAsync(query
            .ApplySorting(page.Sorting, nameof(AttendanceDay.Date) + " desc", AttendanceSort)
            .Skip(page.Skip)
            .Take(page.Take));
        var employees = await _references.GetRefsAsync(rows.Select(x => x.EmployeeId));

        return new TeamAttendanceRowPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => new TeamAttendanceRowDto
            {
                Employee = employees[x.EmployeeId],
                Date = x.Date,
                CheckIn = x.CheckIn,
                CheckOut = x.CheckOut,
                LateMinutes = x.LateMinutes,
                StatusName = _l["Attendance:Status:" + JsonNamingPolicy.CamelCase.ConvertName(x.Status.ToString())]
            }).ToList()
        };
    }

    public virtual async Task<TeamLatenessAbsenceRowPagedResultDto> GetTeamLatenessAbsenceAsync(Guid userId, DateOnly from, DateOnly to, Guid? employeeId, ReportPage page)
    {
        var team = await TeamIdsAsync(userId, employeeId);
        var totals = await SumDaysAsync(team, from, to);
        var employees = await _references.GetRefsAsync(team);

        var rows = team.Select(id =>
        {
            var sum = totals.GetValueOrDefault(id) ?? AttendanceSum.Zero;
            return new TeamLatenessAbsenceRowDto { Employee = employees[id], LateCount = sum.LateCount, LateMinutes = sum.LateMinutes, AbsentDays = sum.AbsentDays };
        }).ToList();

        return new TeamLatenessAbsenceRowPagedResultDto
        {
            TotalCount = rows.Count,
            Items = rows.AsQueryable().ApplySorting(page.Sorting, "Employee.FullName", LatenessSort, "Employee.Id").Skip(page.Skip).Take(page.Take).ToList()
        };
    }

    public virtual async Task<TeamLeaveRowPagedResultDto> GetTeamLeavesAsync(Guid userId, DateOnly from, DateOnly to, Guid? employeeId, ReportPage page)
    {
        var team = await TeamIdsAsync(userId, employeeId);
        var query = (await _leaves.GetQueryableAsync())
            .Where(x => team.Contains(x.EmployeeId) && x.State != LeaveRequestState.Closed && x.RangeStart <= to && x.RangeEnd >= from);

        var total = await _executer.LongCountAsync(query);
        var rows = await _executer.ToListAsync(query
            .ApplySorting(page.Sorting, nameof(LeaveRequest.RangeStart) + " desc", LeaveSort)
            .Skip(page.Skip)
            .Take(page.Take));

        var requestIds = rows.Select(x => x.Id).ToList();
        var requests = (await _requests.GetListAsync(x => requestIds.Contains(x.Id))).ToDictionary(x => x.Id);
        var typeIds = rows.Where(x => x.OtherLeaveTypeId != null).Select(x => x.OtherLeaveTypeId!.Value).Distinct().ToList();
        var types = (await _otherTypes.GetListAsync(x => typeIds.Contains(x.Id))).ToDictionary(x => x.Id);
        var employees = await _references.GetRefsAsync(rows.Select(x => x.EmployeeId));
        var zone = await _companyClock.GetTimeZoneAsync();

        return new TeamLeaveRowPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => new TeamLeaveRowDto
            {
                Employee = employees[x.EmployeeId],
                LeaveTypeName = x.OtherLeaveTypeId != null
                    ? LocalizedName.Pick(types[x.OtherLeaveTypeId.Value].NameAr, types[x.OtherLeaveTypeId.Value].NameEn)
                    : _l["Leave:TypeName:" + JsonNamingPolicy.CamelCase.ConvertName(x.Kind.ToString())],
                From = LeaveMoment(x, x.RangeStart, x.FromTime, zone),
                To = LeaveMoment(x, x.RangeEnd, x.ToTime, zone),
                Days = x.Kind == LeaveKind.Hourly ? null : x.WorkingDays,
                Hours = x.RequestedHours,
                StatusName = StatusName(requests[x.Id])
            }).ToList()
        };
    }

    public virtual async Task<TeamPendingRequestRowPagedResultDto> GetTeamPendingAsync(Guid userId, Guid? employeeId, ReportPage page)
    {
        var team = await TeamIdsAsync(userId, employeeId);
        var query = (await _requests.GetQueryableAsync()).Where(x => team.Contains(x.EmployeeId) && x.CurrentStepOrder != null);

        var total = await _executer.LongCountAsync(query);
        var rows = await _executer.ToListAsync(query
            .ApplySorting(page.Sorting, nameof(Request.CreationTime) + " desc", RequestSort)
            .Skip(page.Skip)
            .Take(page.Take));
        var types = await LoadRequestTypesAsync(rows);
        var employees = await _references.GetRefsAsync(rows.Select(x => x.EmployeeId));

        return new TeamPendingRequestRowPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => new TeamPendingRequestRowDto
            {
                RequestId = x.Id,
                Employee = employees[x.EmployeeId],
                RequestTypeName = LocalizedName.Pick(types[x.RequestTypeId].NameAr, types[x.RequestTypeId].NameEn),
                StatusName = StatusName(x),
                CreationTime = x.CreationTime
            }).ToList()
        };
    }

    public virtual async Task<ReviewDashboardDto> GetReviewDashboardAsync(Guid userId, int? year, int? month)
    {
        var today = await _companyClock.GetTodayAsync();
        var selectedYear = year ?? today.Year;
        var selectedMonth = month ?? today.Month;
        var start = new DateOnly(selectedYear, selectedMonth, 1);

        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == userId);
        var now = _clock.Now;
        var counts = await _complaintCounter.CountAsync(
            userId,
            me?.Id,
            await _companyClock.StartOfDayUtcAsync(start),
            await _companyClock.StartOfDayUtcAsync(start.AddMonths(1)),
            now);

        var cycle = await _cycles.FirstOrDefaultAsync(x => x.Year == selectedYear && x.Month == selectedMonth);
        var departments = cycle == null ? new List<ReviewDepartmentCompletionDto>() : await GetDepartmentCompletionsAsync(cycle, now);

        return new ReviewDashboardDto
        {
            Year = selectedYear,
            Month = selectedMonth,
            ComplaintCounts = new ReviewComplaintCountsDto
            {
                ConfidentialSubmittedCount = counts.ConfidentialSubmitted,
                HighlyConfidentialSubmittedCount = counts.HighlyConfidentialSubmitted,
                LateNormalCount = counts.LateNormal,
                LateConfidentialCount = counts.LateConfidential,
                LateHighlyConfidentialCount = counts.LateHighlyConfidential,
                AsOf = now
            },
            CycleId = cycle?.Id,
            CompletedDepartmentsCount = departments.Count(x => x.Completion.IsCompleted),
            LateDepartmentsCount = departments.Count(x => x.Completion.IsLate),
            Departments = departments
        };
    }

    /// <summary>Each row is one manager's report for the cycle. Before the reports are generated there is nothing to show.</summary>
    private async Task<List<ReviewDepartmentCompletionDto>> GetDepartmentCompletionsAsync(EvaluationCycle cycle, DateTime now)
    {
        var reports = await _departmentReports.GetListAsync(x => x.CycleId == cycle.Id);
        var users = await _references.GetUserRefsAsync(reports.Select(x => x.ManagerUserId));
        var departments = await _references.GetDepartmentsAsync(reports.Where(x => x.DepartmentId != null).Select(x => x.DepartmentId!.Value));

        var rows = new List<ReviewDepartmentCompletionDto>();
        foreach (var report in reports)
        {
            rows.Add(new ReviewDepartmentCompletionDto
            {
                Manager = users[report.ManagerUserId],
                Department = report.DepartmentId == null ? null : departments.GetValueOrDefault(report.DepartmentId.Value),
                Completion = await _reportReader.GetCompletionAsync(cycle, report, now)
            });
        }

        return rows;
    }

    /// <summary>Submitted evaluations of the manager's current staff, in the verbal scale. The internal note is not reachable from here.</summary>
    public virtual async Task<TeamEvaluationRowPagedResultDto> GetTeamEvaluationsAsync(Guid userId, Guid? cycleId, int? year, Guid? employeeId, ReportPage page)
    {
        var team = await TeamIdsAsync(userId, employeeId);
        var query = (await _evaluations.WithDetailsAsync(x => x.Scores))
            .Where(x => team.Contains(x.EmployeeId) && x.Status != EvaluationStatus.Draft)
            .WhereIf(cycleId != null, x => x.CycleId == cycleId);

        if (year != null)
        {
            var ids = await _executer.ToListAsync((await _cycles.GetQueryableAsync()).Where(x => x.Year == year).Select(x => x.Id));
            query = query.Where(x => ids.Contains(x.CycleId));
        }

        var total = await _executer.LongCountAsync(query);
        var rows = await _executer.ToListAsync(query
            .ApplySorting(page.Sorting, nameof(Evaluation.SubmittedAt) + " desc", EvaluationSort)
            .Skip(page.Skip)
            .Take(page.Take));

        var usedCycles = rows.Select(x => x.CycleId).Distinct().ToList();
        var cycles = (await _executer.ToListAsync((await _cycles.WithDetailsAsync(x => x.Criteria, x => x.Grades)).Where(x => usedCycles.Contains(x.Id)))).ToDictionary(x => x.Id);
        var employees = await _references.GetRefsAsync(rows.Select(x => x.EmployeeId));

        return new TeamEvaluationRowPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => new TeamEvaluationRowDto
            {
                EvaluationId = x.Id,
                Employee = employees[x.EmployeeId],
                Year = cycles[x.CycleId].Year,
                Month = cycles[x.CycleId].Month,
                Status = x.Status,
                ObjectionStatus = x.ObjectionStatus,
                Scores = EvaluationAppService.ToSubmittedScores(x, cycles[x.CycleId])
            }).ToList()
        };
    }

    /// <summary>Per manager, with no employee named. A draft counts for whoever manages the employee now, a submitted form for who submitted it.</summary>
    public virtual async Task<EvaluationConsistencyRowPagedResultDto> GetEvaluationConsistencyAsync(Guid? cycleId, Guid? departmentId, ReportPage page)
    {
        var query = await _cycles.WithDetailsAsync(x => x.Grades);
        var cycle = cycleId != null
            ? await _executer.FirstOrDefaultAsync(query.Where(x => x.Id == cycleId))
            : await _executer.FirstOrDefaultAsync(query.Where(x => x.Status == EvaluationCycleStatus.Open).OrderByDescending(x => x.Year).ThenByDescending(x => x.Month))
              ?? await _executer.FirstOrDefaultAsync(query.Where(x => x.Status != EvaluationCycleStatus.Scheduled).OrderByDescending(x => x.Year).ThenByDescending(x => x.Month));

        if (cycle == null)
        {
            return new EvaluationConsistencyRowPagedResultDto();
        }

        var forms = await _executer.ToListAsync((await _evaluations.WithDetailsAsync(x => x.Scores)).Where(x => x.CycleId == cycle.Id));
        var employeeIds = forms.Select(x => x.EmployeeId).ToList();
        var owners = (await _employees.GetListAsync(x => employeeIds.Contains(x.Id))).ToDictionary(x => x.Id);
        var managerIds = owners.Values.Where(x => x.ManagerEmployeeId != null).Select(x => x.ManagerEmployeeId!.Value).Distinct().ToList();
        var managers = (await _employees.GetListAsync(x => managerIds.Contains(x.Id))).ToDictionary(x => x.Id);

        Guid? EvaluatorOf(Evaluation form)
        {
            if (form.SubmittedByUserId != null)
            {
                return form.SubmittedByUserId;
            }

            var managerId = owners[form.EmployeeId].ManagerEmployeeId;
            return managerId == null ? null : managers.GetValueOrDefault(managerId.Value)?.UserId;
        }

        var groups = forms
            .Where(x => departmentId == null || owners[x.EmployeeId].DepartmentId == departmentId)
            .Select(x => new { Form = x, Evaluator = EvaluatorOf(x) })
            .Where(x => x.Evaluator != null)
            .GroupBy(x => x.Evaluator!.Value)
            .ToList();

        var users = await _references.GetUserRefsAsync(groups.Select(x => x.Key));
        var departmentRefs = await _references.GetDepartmentsAsync(owners.Values.Select(x => x.DepartmentId));
        var gradeNames = cycle.Grades.OrderBy(x => x.SortOrder).ToList();

        var rows = groups.Select(group =>
        {
            var submitted = group.Where(x => x.Form.Status != EvaluationStatus.Draft).Select(x => x.Form).ToList();
            var scores = submitted.SelectMany(x => x.Scores).ToList();
            var departmentIds = group.Select(x => owners[x.Form.EmployeeId].DepartmentId).Distinct().ToList();

            return new EvaluationConsistencyRowDto
            {
                Manager = users[group.Key],
                Department = departmentIds.Count == 1 ? departmentRefs.GetValueOrDefault(departmentIds[0]) : null,
                ExpectedCount = group.Count(),
                SubmittedCount = submitted.Count,
                CompletionRatio = Math.Round((decimal)submitted.Count / group.Count(), 4),
                DistinctGradesUsed = scores.Select(x => x.GradeCode).Distinct().Count(),
                SameGradeForAll = submitted.Count >= 2 && scores.GroupBy(x => x.CriterionId).All(c => c.Select(x => x.GradeCode).Distinct().Count() == 1),
                GradeCounts = gradeNames
                    .Select(grade => new EvaluationGradeCountDto
                    {
                        GradeCode = grade.Code,
                        GradeName = LocalizedName.Pick(grade.NameAr, grade.NameEn),
                        Count = scores.Count(x => x.GradeCode == grade.Code)
                    })
                    .Where(x => x.Count > 0)
                    .ToList()
            };
        }).ToList();

        return new EvaluationConsistencyRowPagedResultDto
        {
            TotalCount = rows.Count,
            Items = rows.AsQueryable().ApplySorting(page.Sorting, "Manager.DisplayName", ConsistencySort, "Manager.Id").Skip(page.Skip).Take(page.Take).ToList()
        };
    }

    public virtual async Task<HrAttendanceReportRowPagedResultDto> GetHrAttendanceAsync(DateOnly from, DateOnly to, Guid? departmentId, Guid? employeeId, ReportPage page)
    {
        // The executive director is outside attendance, and is the only employee with no manager.
        var candidates = await _executer.ToListAsync(CompanyEmployees(await _employees.GetQueryableAsync(), departmentId, employeeId));
        var totals = await SumDaysAsync(candidates.Select(x => x.Id).ToList(), from, to);

        var included = candidates.Where(x => x.Status != EmployeeStatus.Ended || totals.ContainsKey(x.Id)).ToList();
        var employees = await _references.ToRefsAsync(included);

        var rows = included.Select(x =>
        {
            var sum = totals.GetValueOrDefault(x.Id) ?? AttendanceSum.Zero;
            return new HrAttendanceReportRowDto
            {
                Employee = employees[x.Id],
                ScheduledWorkingDays = sum.ScheduledDays,
                PresentDays = sum.PresentDays,
                AbsentDays = sum.AbsentDays,
                LateCount = sum.LateCount,
                LateMinutes = sum.LateMinutes,
                LeaveDays = sum.LeaveDays
            };
        }).ToList();

        return new HrAttendanceReportRowPagedResultDto
        {
            TotalCount = rows.Count,
            Items = rows.AsQueryable().ApplySorting(page.Sorting, "Employee.FullName", HrAttendanceSort, "Employee.Id").Skip(page.Skip).Take(page.Take).ToList()
        };
    }

    public virtual async Task<HrLeaveBalanceReportRowPagedResultDto> GetHrLeaveBalancesAsync(Guid? departmentId, Guid? employeeId, ReportPage page)
    {
        var employees = (await _executer.ToListAsync(CompanyEmployees(await _employees.GetQueryableAsync(), departmentId, employeeId)
            .Where(x => x.Status != EmployeeStatus.Ended))).ToDictionary(x => x.Id);
        var references = await _references.ToRefsAsync(employees.Values);

        var ordered = references.Values
            .Select(x => new HrLeaveBalanceReportRowDto { Employee = x })
            .AsQueryable()
            .ApplySorting(page.Sorting, "Employee.FullName", BalanceSort, "Employee.Id")
            .Skip(page.Skip)
            .Take(page.Take)
            .ToList();

        // A balance is a sum over the ledger, and reading it applies any year-end carry that is due, so it goes
        // through the one reader rather than a shortcut that could disagree with the employee's own screen.
        foreach (var row in ordered)
        {
            var snapshot = await _balances.GetSnapshotAsync(employees[row.Employee.Id]);
            row.LeaveYearStart = snapshot.YearStart;
            row.EntitlementDays = snapshot.CurrentYear?.EntitlementDays ?? 0;
            row.CarriedInDays = snapshot.Sum(LeaveLedgerType.CarryIn);
            row.UsedDays = -snapshot.Sum(LeaveLedgerType.Deduct, LeaveLedgerType.Reverse);
            row.HeldDays = -snapshot.Sum(LeaveLedgerType.Hold, LeaveLedgerType.Release);
            row.RemainingDays = Math.Max(0, snapshot.Sum(LeaveLedgerBucket.Carried) + snapshot.Sum(LeaveLedgerBucket.Entitlement));
            row.UsedHours = -snapshot.CurrentEntries
                .Where(x => x.Type is LeaveLedgerType.Deduct or LeaveLedgerType.Reverse)
                .Sum(x => x.Hours ?? 0);
        }

        return new HrLeaveBalanceReportRowPagedResultDto { TotalCount = references.Count, Items = ordered };
    }

    public virtual async Task<HrRequestReportRowPagedResultDto> GetHrRequestsAsync(DateOnly from, DateOnly to, Guid? departmentId, Guid? employeeId, ReportPage page)
    {
        var start = await _companyClock.StartOfDayUtcAsync(from);
        var end = await _companyClock.StartOfDayUtcAsync(to.AddDays(1));

        var scope = (await _employees.GetQueryableAsync())
            .Where(x => departmentId == null || x.DepartmentId == departmentId)
            .Where(x => employeeId == null || x.Id == employeeId)
            .Select(x => x.Id);
        var query = (await _requests.GetQueryableAsync())
            .Where(x => x.CreationTime >= start && x.CreationTime < end && scope.Contains(x.EmployeeId));

        var total = await _executer.LongCountAsync(query);
        var rows = await _executer.ToListAsync(query
            .ApplySorting(page.Sorting, nameof(Request.CreationTime) + " desc", RequestSort)
            .Skip(page.Skip)
            .Take(page.Take));
        var types = await LoadRequestTypesAsync(rows);
        var employees = await _references.GetRefsAsync(rows.Select(x => x.EmployeeId));

        // A request has no closing date of its own. Its last timeline entry is the decision that finished it.
        var closedIds = rows.Where(x => !x.IsPending).Select(x => x.Id).ToList();
        var closedAt = (await _executer.ToListAsync((await _requests.GetQueryableAsync())
                .Where(x => closedIds.Contains(x.Id))
                .SelectMany(x => x.Timeline)
                .GroupBy(x => x.RequestId)
                .Select(g => new { RequestId = g.Key, At = g.Max(x => x.OccurredAt) })))
            .ToDictionary(x => x.RequestId, x => x.At);

        return new HrRequestReportRowPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => new HrRequestReportRowDto
            {
                RequestId = x.Id,
                Employee = employees[x.EmployeeId],
                RequestTypeName = LocalizedName.Pick(types[x.RequestTypeId].NameAr, types[x.RequestTypeId].NameEn),
                StatusName = StatusName(x),
                CreationTime = x.CreationTime,
                ClosedAt = closedAt.TryGetValue(x.Id, out var at) ? at : null
            }).ToList()
        };
    }

    private static IQueryable<Employee> CompanyEmployees(IQueryable<Employee> employees, Guid? departmentId, Guid? employeeId) =>
        employees
            .Where(x => x.ManagerEmployeeId != null)
            .Where(x => departmentId == null || x.DepartmentId == departmentId)
            .Where(x => employeeId == null || x.Id == employeeId);

    /// <summary>A filter outside the team finds nothing, the same as the other team lists, and does not reveal the person exists.</summary>
    /// <summary>The personal custody of direct reports, and the shared custody of their departments unless one employee was asked for.</summary>
    public virtual async Task<TeamCustodyRowPagedResultDto> GetTeamCustodyAsync(Guid userId, Guid? employeeId, ReportCustodyKind? kind, ReportPage page)
    {
        var team = await TeamIdsAsync(userId, employeeId);
        var departments = employeeId == null ? await _custodyScope.GetManagedDepartmentIdsAsync(userId) : new List<Guid>();

        var query = (await _custodies.GetQueryableAsync()).Where(x =>
            (x.EmployeeId != null && team.Contains(x.EmployeeId.Value)) ||
            (x.DepartmentId != null && departments.Contains(x.DepartmentId.Value)));

        if (kind != null)
        {
            var wanted = kind == ReportCustodyKind.Shared ? HR_Mangment2.Store.CustodyKind.Shared : HR_Mangment2.Store.CustodyKind.Personal;
            query = query.Where(x => x.Kind == wanted);
        }

        var total = await _executer.LongCountAsync(query);
        var rows = await _executer.ToListAsync(query
            .ApplySorting(page.Sorting, nameof(HR_Mangment2.Store.Custody.AssignedAt) + " desc", CustodySort)
            .Skip(page.Skip)
            .Take(page.Take));
        var items = await _itemRefs.GetRefsAsync(rows.Select(x => x.ItemId));
        var employees = await _references.GetRefsAsync(rows.Where(x => x.EmployeeId != null).Select(x => x.EmployeeId!.Value));

        return new TeamCustodyRowPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => new TeamCustodyRowDto
            {
                CustodyId = x.Id,
                ItemName = items[x.ItemId].Name,
                Kind = x.Kind == HR_Mangment2.Store.CustodyKind.Shared ? ReportCustodyKind.Shared : ReportCustodyKind.Personal,
                Employee = x.EmployeeId == null ? null : employees[x.EmployeeId.Value],
                AssignedAt = x.AssignedAt,
                AcknowledgedAt = x.AcknowledgedAt,
                StatusName = _l[HR_Mangment2.Store.StoreLabels.CustodyStatusKey(x.Status)]
            }).ToList()
        };
    }

    private async Task<List<Guid>> TeamIdsAsync(Guid userId, Guid? employeeId)
    {
        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == userId);
        if (me == null)
        {
            return new List<Guid>();
        }

        return await _executer.ToListAsync((await _employees.GetQueryableAsync())
            .Where(x => x.ManagerEmployeeId == me.Id && (employeeId == null || x.Id == employeeId))
            .Select(x => x.Id));
    }

    private Task<Dictionary<Guid, AttendanceSum>> SumDaysAsync(List<Guid> employeeIds, DateOnly from, DateOnly to) =>
        _attendanceSums.SumAsync(employeeIds, from, to);

    private async Task<Dictionary<Guid, RequestType>> LoadRequestTypesAsync(IEnumerable<Request> requests)
    {
        var ids = requests.Select(x => x.RequestTypeId).Distinct().ToList();
        return (await _requestTypes.GetListAsync(x => ids.Contains(x.Id))).ToDictionary(x => x.Id);
    }

    /// <summary>Hourly leave has real times, in company time. A full day is a date, written as midnight UTC like the contract's example.</summary>
    private static DateTime LeaveMoment(LeaveRequest leave, DateOnly date, TimeOnly? time, TimeZoneInfo zone) =>
        leave.Kind == LeaveKind.Hourly && time != null
            ? CompanyClock.ToUtc(date, time.Value, zone)
            : date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

    private string StatusName(Request request) =>
        request.Status == RequestStatus.PendingManager && request.CurrentApproverRole == HrRoles.SystemAdmin
            ? _l["Request:Status:pendingSystemAdmin"]
            : _l["Request:Status:" + JsonNamingPolicy.CamelCase.ConvertName(request.Status.ToString())];
}
