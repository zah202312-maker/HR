using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Auditing;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Identity;
using HR_Mangment2.Localization;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace HR_Mangment2.Reports;

public class ReportAppService : HR_Mangment2AppService, IReportAppService
{
    private readonly ReportQueries _queries;
    private readonly EmploymentRecordReportQueries _recordQueries;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly ManagerLinkService _managerLinks;
    private readonly ICompanyClock _companyClock;
    private readonly IRepository<HrAuditEntry, Guid> _auditEntries;
    private readonly IAuditingManager _auditingManager;

    public ReportAppService(
        ReportQueries queries,
        EmploymentRecordReportQueries recordQueries,
        IRepository<Employee, Guid> employees,
        ManagerLinkService managerLinks,
        ICompanyClock companyClock,
        IRepository<HrAuditEntry, Guid> auditEntries,
        IAuditingManager auditingManager)
    {
        _queries = queries;
        _recordQueries = recordQueries;
        _employees = employees;
        _managerLinks = managerLinks;
        _companyClock = companyClock;
        _auditEntries = auditEntries;
        _auditingManager = auditingManager;
    }

    [Authorize(HrPermissions.ReportsPermissions.Team)]
    public virtual Task<TeamAttendanceRowPagedResultDto> GetTeamAttendanceAsync(TeamPeriodReportInput input)
    {
        var (from, to) = RequirePeriod(input.From, input.To);
        return _queries.GetTeamAttendanceAsync(CurrentUser.GetId(), from, to, input.EmployeeId, PageOf(input));
    }

    [Authorize(HrPermissions.ReportsPermissions.Team)]
    public virtual Task<TeamLatenessAbsenceRowPagedResultDto> GetTeamLatenessAbsenceAsync(TeamPeriodReportInput input)
    {
        var (from, to) = RequirePeriod(input.From, input.To);
        return _queries.GetTeamLatenessAbsenceAsync(CurrentUser.GetId(), from, to, input.EmployeeId, PageOf(input));
    }

    [Authorize(HrPermissions.ReportsPermissions.Team)]
    public virtual Task<TeamLeaveRowPagedResultDto> GetTeamLeavesAsync(TeamPeriodReportInput input)
    {
        var (from, to) = RequirePeriod(input.From, input.To);
        return _queries.GetTeamLeavesAsync(CurrentUser.GetId(), from, to, input.EmployeeId, PageOf(input));
    }

    [Authorize(HrPermissions.ReportsPermissions.Team)]
    public virtual Task<TeamPendingRequestRowPagedResultDto> GetTeamPendingAsync(TeamPendingReportInput input) =>
        _queries.GetTeamPendingAsync(CurrentUser.GetId(), input.EmployeeId, PageOf(input));

    [Authorize(HrPermissions.ReportsPermissions.Team)]
    public virtual Task<TeamCustodyRowPagedResultDto> GetTeamCustodyAsync(TeamCustodyReportInput input) =>
        _queries.GetTeamCustodyAsync(CurrentUser.GetId(), input.EmployeeId, input.Kind, PageOf(input));

    [Authorize(HrPermissions.ReportsPermissions.Team)]
    public virtual Task<TeamEvaluationRowPagedResultDto> GetTeamEvaluationsAsync(TeamEvaluationReportInput input) =>
        _queries.GetTeamEvaluationsAsync(CurrentUser.GetId(), input.CycleId, input.Year, input.EmployeeId, PageOf(input));

    [Authorize(HrPermissions.ReportsPermissions.Review)]
    public virtual async Task<ReviewDashboardDto> GetReviewDashboardAsync(ReviewDashboardInput input)
    {
        await RefuseExecutiveDirectorAsync();
        return await _queries.GetReviewDashboardAsync(CurrentUser.GetId(), input.Year, input.Month);
    }

    [Authorize(HrPermissions.ReportsPermissions.Review)]
    public virtual Task<EvaluationConsistencyRowPagedResultDto> GetEvaluationConsistencyAsync(EvaluationConsistencyReportInput input) =>
        _queries.GetEvaluationConsistencyAsync(input.CycleId, input.DepartmentId, PageOf(input));

    /// <summary>Counts only, for the executive director's dashboard (decision 2026-11-11).</summary>
    [Authorize(HrPermissions.ReportsPermissions.TodaySummary)]
    public virtual async Task<TodayAttendanceSummaryDto> GetTodayAttendanceSummaryAsync() =>
        await _queries.GetTodaySummaryAsync(await _companyClock.GetTodayAsync());

    [Authorize(HrPermissions.ReportsPermissions.Hr)]
    public virtual Task<HrAttendanceReportRowPagedResultDto> GetHrAttendanceAsync(HrPeriodReportInput input)
    {
        var (from, to) = RequirePeriod(input.From, input.To);
        return _queries.GetHrAttendanceAsync(from, to, input.DepartmentId, input.EmployeeId, PageOf(input));
    }

    [Authorize(HrPermissions.ReportsPermissions.Hr)]
    public virtual Task<HrLeaveBalanceReportRowPagedResultDto> GetHrLeaveBalancesAsync(HrLeaveBalanceReportInput input)
    {
        var period = OptionalPeriod(input.From, input.To);
        return _queries.GetHrLeaveBalancesAsync(input.DepartmentId, input.EmployeeId, PageOf(input), period?.From, period?.To);
    }

    [Authorize(HrPermissions.ReportsPermissions.Hr)]
    public virtual Task<HrRequestReportRowPagedResultDto> GetHrRequestsAsync(HrPeriodReportInput input)
    {
        var (from, to) = RequirePeriod(input.From, input.To);
        return _queries.GetHrRequestsAsync(from, to, input.DepartmentId, input.EmployeeId, PageOf(input));
    }

    [Authorize(HrPermissions.ReportsPermissions.Hr)]
    public virtual Task<HrContractReportRowPagedResultDto> GetHrContractsAsync(HrContractReportInput input) =>
        _recordQueries.GetHrContractsAsync(input.EndFrom, input.EndTo, input.DepartmentId, input.EmployeeId, PageOf(input));

    [Authorize(HrPermissions.ReportsPermissions.Hr)]
    public virtual Task<HrWarningReportRowPagedResultDto> GetHrWarningsAsync(HrPeriodReportInput input)
    {
        var (from, to) = RequirePeriod(input.From, input.To);
        return _recordQueries.GetHrWarningsAsync(from, to, input.DepartmentId, input.EmployeeId, PageOf(input));
    }

    [Authorize(HrPermissions.ReportsPermissions.Export)]
    public virtual async Task<ReportFile> ExportAsync(ReportCode code, ReportExportInput input)
    {
        await AuthorizationService.CheckAsync(PermissionFor(code));

        if (code == ReportCode.ReviewDashboard)
        {
            await RefuseExecutiveDirectorAsync();
        }

        var table = await BuildTableAsync(code, input);
        var rightToLeft = !LocalizedName.IsEnglish;
        var isPdf = input.Format == ReportExportFormat.Pdf;

        var content = isPdf ? ReportFileWriter.WritePdf(table, rightToLeft) : ReportFileWriter.WriteExcel(table, rightToLeft);
        var codeName = JsonNamingPolicy.CamelCase.ConvertName(code.ToString());

        // ABP does not audit GET requests, and an export is the one read this log must show.
        await _auditEntries.InsertAsync(
            new HrAuditEntry(
                GuidGenerator.Create(),
                CurrentTenant.Id,
                Clock.Now,
                CurrentUser.GetId(),
                AuditActionType.Export,
                "Report",
                codeName,
                Array.Empty<string>(),
                _auditingManager.Current?.Log.ClientIpAddress,
                true),
            autoSave: true);

        return new ReportFile(
            content,
            isPdf ? ReportFileWriter.PdfContentType : ReportFileWriter.ExcelContentType,
            $"{codeName}-{await _companyClock.GetTodayAsync():yyyyMMdd}.{(isPdf ? "pdf" : "xlsx")}");
    }

    private async Task<ReportTable> BuildTableAsync(ReportCode code, ReportExportInput input)
    {
        var userId = CurrentUser.GetId();
        var zone = await _companyClock.GetTimeZoneAsync();

        string Local(DateTime? utc) => utc == null
            ? string.Empty
            : TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc.Value, DateTimeKind.Utc), zone).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

        static string Day(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        ReportTable Table(string? period, string[] columns, IEnumerable<object?[]> rows) => new(
            L["Report:Title:" + JsonNamingPolicy.CamelCase.ConvertName(code.ToString())],
            period,
            columns.Select(x => (string)L["Report:Column:" + x]).ToList(),
            rows.ToList());

        switch (code)
        {
            case ReportCode.TeamAttendance:
            {
                var (from, to) = RequireFilters(input);
                var page = await _queries.GetTeamAttendanceAsync(userId, from, to, input.EmployeeId, ReportPage.All);
                return Table(
                    PeriodText(from, to),
                    new[] { "employee", "department", "date", "checkIn", "checkOut", "lateMinutes", "status" },
                    page.Items.Select(x => new object?[] { x.Employee.FullName, x.Employee.DepartmentName, Day(x.Date), Local(x.CheckIn), Local(x.CheckOut), x.LateMinutes, x.StatusName }));
            }

            case ReportCode.TeamLatenessAbsence:
            {
                var (from, to) = RequireFilters(input);
                var page = await _queries.GetTeamLatenessAbsenceAsync(userId, from, to, input.EmployeeId, ReportPage.All);
                return Table(
                    PeriodText(from, to),
                    new[] { "employee", "department", "lateCount", "lateMinutes", "absentDays" },
                    page.Items.Select(x => new object?[] { x.Employee.FullName, x.Employee.DepartmentName, x.LateCount, x.LateMinutes, x.AbsentDays }));
            }

            case ReportCode.TeamLeaves:
            {
                var (from, to) = RequireFilters(input);
                var page = await _queries.GetTeamLeavesAsync(userId, from, to, input.EmployeeId, ReportPage.All);
                return Table(
                    PeriodText(from, to),
                    new[] { "employee", "department", "leaveType", "from", "to", "days", "hours", "status" },
                    page.Items.Select(x => new object?[]
                    {
                        x.Employee.FullName,
                        x.Employee.DepartmentName,
                        x.LeaveTypeName,
                        x.Hours != null ? Local(x.From) : Day(DateOnly.FromDateTime(x.From)),
                        x.Hours != null ? Local(x.To) : Day(DateOnly.FromDateTime(x.To)),
                        x.Days,
                        x.Hours,
                        x.StatusName
                    }));
            }

            case ReportCode.TeamPending:
            {
                var page = await _queries.GetTeamPendingAsync(userId, input.EmployeeId, ReportPage.All);
                return Table(
                    null,
                    new[] { "employee", "department", "requestType", "status", "creationTime" },
                    page.Items.Select(x => new object?[] { x.Employee.FullName, x.Employee.DepartmentName, x.RequestTypeName, x.StatusName, Local(x.CreationTime) }));
            }

            case ReportCode.TeamCustody:
            {
                var page = await _queries.GetTeamCustodyAsync(userId, input.EmployeeId, input.Kind, ReportPage.All);
                return Table(
                    null,
                    new[] { "item", "kind", "employee", "assignedAt", "acknowledgedAt", "status" },
                    page.Items.Select(x => new object?[] { x.ItemName, x.Kind.ToString(), x.Employee?.FullName, Local(x.AssignedAt), Local(x.AcknowledgedAt), x.StatusName }));
            }

            case ReportCode.TeamEvaluations:
            {
                var page = await _queries.GetTeamEvaluationsAsync(userId, input.CycleId, input.Year, input.EmployeeId, ReportPage.All);
                return Table(
                    null,
                    new[] { "employee", "year", "month", "status", "objectionStatus", "scores" },
                    page.Items.Select(x => new object?[]
                    {
                        x.Employee.FullName,
                        x.Year,
                        x.Month,
                        x.Status.ToString(),
                        x.ObjectionStatus.ToString(),
                        string.Join("; ", x.Scores.Select(s => s.CriterionName + ": " + s.GradeName))
                    }));
            }

            case ReportCode.EvaluationConsistency:
            {
                var page = await _queries.GetEvaluationConsistencyAsync(input.CycleId, input.DepartmentId, ReportPage.All);
                return Table(
                    null,
                    new[] { "manager", "department", "expectedCount", "submittedCount", "completionRatio", "distinctGradesUsed", "sameGradeForAll" },
                    page.Items.Select(x => new object?[] { x.Manager.DisplayName, x.Department?.Name, x.ExpectedCount, x.SubmittedCount, x.CompletionRatio, x.DistinctGradesUsed, x.SameGradeForAll }));
            }

            case ReportCode.ReviewDashboard:
            {
                var dashboard = await _queries.GetReviewDashboardAsync(userId, input.Year, input.Month);
                var counts = dashboard.ComplaintCounts;
                return Table(
                    $"{dashboard.Year:D4}-{dashboard.Month:D2}",
                    new[] { "item", "count" },
                    new[]
                    {
                        new object?[] { (string)L["Report:Review:confidentialSubmitted"], counts.ConfidentialSubmittedCount },
                        new object?[] { (string)L["Report:Review:highlyConfidentialSubmitted"], counts.HighlyConfidentialSubmittedCount },
                        new object?[] { (string)L["Report:Review:lateNormal"], counts.LateNormalCount },
                        new object?[] { (string)L["Report:Review:lateConfidential"], counts.LateConfidentialCount },
                        new object?[] { (string)L["Report:Review:lateHighlyConfidential"], counts.LateHighlyConfidentialCount },
                        new object?[] { (string)L["Report:Review:completedDepartments"], dashboard.CompletedDepartmentsCount },
                        new object?[] { (string)L["Report:Review:lateDepartments"], dashboard.LateDepartmentsCount }
                    });
            }

            case ReportCode.HrAttendance:
            {
                var (from, to) = RequireFilters(input);
                var page = await _queries.GetHrAttendanceAsync(from, to, input.DepartmentId, input.EmployeeId, ReportPage.All);
                return Table(
                    PeriodText(from, to),
                    new[] { "employee", "department", "scheduledWorkingDays", "presentDays", "absentDays", "lateCount", "lateMinutes", "leaveDays" },
                    page.Items.Select(x => new object?[] { x.Employee.FullName, x.Employee.DepartmentName, x.ScheduledWorkingDays, x.PresentDays, x.AbsentDays, x.LateCount, x.LateMinutes, x.LeaveDays }));
            }

            case ReportCode.HrLeaveBalances:
            {
                var period = OptionalPeriod(input.From, input.To);
                var page = await _queries.GetHrLeaveBalancesAsync(input.DepartmentId, input.EmployeeId, ReportPage.All, period?.From, period?.To);
                if (period == null)
                {
                    return Table(
                        null,
                        new[] { "employee", "department", "leaveYearStart", "entitlementDays", "carriedInDays", "usedDays", "heldDays", "remainingDays", "usedHours" },
                        page.Items.Select(x => new object?[] { x.Employee.FullName, x.Employee.DepartmentName, Day(x.LeaveYearStart), x.EntitlementDays, x.CarriedInDays, x.UsedDays, x.HeldDays, x.RemainingDays, x.UsedHours }));
                }

                return Table(
                    PeriodText(period.Value.From, period.Value.To),
                    new[] { "employee", "department", "leaveYearStart", "entitlementDays", "carriedInDays", "usedDays", "heldDays", "remainingDays", "usedHours", "periodUsedDays", "periodUsedHours" },
                    page.Items.Select(x => new object?[] { x.Employee.FullName, x.Employee.DepartmentName, Day(x.LeaveYearStart), x.EntitlementDays, x.CarriedInDays, x.UsedDays, x.HeldDays, x.RemainingDays, x.UsedHours, x.PeriodUsedDays, x.PeriodUsedHours }));
            }

            case ReportCode.HrRequests:
            {
                var (from, to) = RequireFilters(input);
                var page = await _queries.GetHrRequestsAsync(from, to, input.DepartmentId, input.EmployeeId, ReportPage.All);
                return Table(
                    PeriodText(from, to),
                    new[] { "employee", "department", "requestType", "status", "creationTime", "closedAt" },
                    page.Items.Select(x => new object?[] { x.Employee.FullName, x.Employee.DepartmentName, x.RequestTypeName, x.StatusName, Local(x.CreationTime), Local(x.ClosedAt) }));
            }


            case ReportCode.HrContracts:
            {
                var page = await _recordQueries.GetHrContractsAsync(input.EndFrom, input.EndTo, input.DepartmentId, input.EmployeeId, ReportPage.All);
                return Table(
                    null,
                    new[] { "employee", "department", "contractType", "startDate", "endDate", "status" },
                    page.Items.Select(x => new object?[] { x.Employee.FullName, x.Employee.DepartmentName, x.TypeName, Day(x.StartDate), Day(x.EndDate), x.StatusName }));
            }

            case ReportCode.HrWarnings:
            {
                var (from, to) = RequireFilters(input);
                var page = await _recordQueries.GetHrWarningsAsync(from, to, input.DepartmentId, input.EmployeeId, ReportPage.All);
                return Table(
                    PeriodText(from, to),
                    new[] { "employee", "department", "warningLevel", "incidentDate", "status" },
                    page.Items.Select(x => new object?[] { x.Employee.FullName, x.Employee.DepartmentName, x.LevelName, Day(x.IncidentDate), x.StatusName }));
            }

            default:
                throw new EntityNotFoundException();
        }
    }

    private static string PermissionFor(ReportCode code) => code switch
    {
        ReportCode.TeamAttendance or ReportCode.TeamLatenessAbsence or ReportCode.TeamLeaves or ReportCode.TeamPending or ReportCode.TeamCustody or ReportCode.TeamEvaluations
            => HrPermissions.ReportsPermissions.Team,
        ReportCode.ReviewDashboard or ReportCode.EvaluationConsistency
            => HrPermissions.ReportsPermissions.Review,
        _ => HrPermissions.ReportsPermissions.Hr
    };

    /// <summary>The list routes require the dates, so their absence is a 400 there. Export has no fixed filter set, hence its own code.</summary>
    private static (DateOnly From, DateOnly To) RequireFilters(ReportExportInput input)
    {
        if (input.From == null || input.To == null)
        {
            throw new BusinessException(HrErrorCodes.Reports.MissingFilter);
        }

        return RequirePeriod(input.From, input.To);
    }

    private static (DateOnly From, DateOnly To) RequirePeriod(DateOnly? from, DateOnly? to)
    {
        if (to < from)
        {
            throw new BusinessException(HrErrorCodes.Reports.InvalidPeriod);
        }

        return (from!.Value, to!.Value);
    }

    /// <summary>An optional period: both dates or neither, and to not before from; otherwise InvalidPeriod.</summary>
    private static (DateOnly From, DateOnly To)? OptionalPeriod(DateOnly? from, DateOnly? to)
    {
        if (from == null && to == null)
        {
            return null;
        }

        if (from == null || to == null)
        {
            throw new BusinessException(HrErrorCodes.Reports.InvalidPeriod);
        }

        return RequirePeriod(from, to);
    }

    private static string PeriodText(DateOnly from, DateOnly to) =>
        $"{from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} - {to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";

    private static ReportPage PageOf(Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto input) =>
        new(input.SkipCount, input.MaxResultCount, input.Sorting);

    /// <summary>The executive director does not see ordinary complaints even as counts, whatever other roles are held. Roles are read from the database too, in case the token is older than a role change.</summary>
    private async Task RefuseExecutiveDirectorAsync()
    {
        var userId = CurrentUser.GetId();
        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == userId);
        if (CurrentUser.IsInRole(HrRoles.ExecutiveDirector) ||
            (me != null && (await _managerLinks.GetRolesAsync(me)).Contains(HrRoles.ExecutiveDirector)))
        {
            throw new AbpAuthorizationException();
        }
    }
}

public class TeamPeriodReportInputValidator : AbstractValidator<TeamPeriodReportInput>
{
    public TeamPeriodReportInputValidator()
    {
        RuleFor(x => x.From).NotNull();
        RuleFor(x => x.To).NotNull();
    }
}

public class HrPeriodReportInputValidator : AbstractValidator<HrPeriodReportInput>
{
    public HrPeriodReportInputValidator()
    {
        RuleFor(x => x.From).NotNull();
        RuleFor(x => x.To).NotNull();
    }
}

public class TeamEvaluationReportInputValidator : AbstractValidator<TeamEvaluationReportInput>
{
    public TeamEvaluationReportInputValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
    }
}

public class ReviewDashboardInputValidator : AbstractValidator<ReviewDashboardInput>
{
    public ReviewDashboardInputValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }
}

public class ReportExportInputValidator : AbstractValidator<ReportExportInput>
{
    public ReportExportInputValidator()
    {
        RuleFor(x => x.Format).NotNull();
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }
}
