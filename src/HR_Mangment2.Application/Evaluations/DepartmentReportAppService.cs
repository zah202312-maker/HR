using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Localization;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace HR_Mangment2.Evaluations;

/// <summary>Turns a report row into its DTO parts, for the monthly report itself and for the reviewer's dashboard.</summary>
public class DepartmentReportReader : Volo.Abp.DependencyInjection.ITransientDependency
{
    private readonly IRepository<Evaluation, Guid> _evaluations;
    private readonly MonthlyCompletionReader _completion;
    private readonly Volo.Abp.Linq.IAsyncQueryableExecuter _executer;

    public DepartmentReportReader(IRepository<Evaluation, Guid> evaluations, MonthlyCompletionReader completion, Volo.Abp.Linq.IAsyncQueryableExecuter executer)
    {
        _evaluations = evaluations;
        _completion = completion;
        _executer = executer;
    }

    public static DepartmentReportSnapshot ReadSnapshot(DepartmentReport report) =>
        JsonSerializer.Deserialize<DepartmentReportSnapshot>(report.SnapshotJson)!;

    public virtual async Task<DepartmentCompletionDto> GetCompletionAsync(EvaluationCycle cycle, DepartmentReport report, DateTime now)
    {
        var staff = ReadSnapshot(report).Employees.Select(x => x.EmployeeId).ToList();
        var completion = await _completion.ReadAsync(cycle, staff, report, now);

        return new DepartmentCompletionDto
        {
            EvaluationsExpected = completion.Expected,
            EvaluationsSubmitted = completion.Submitted,
            ReportSubmitted = completion.ReportSubmitted,
            IsCompleted = completion.IsCompleted,
            IsLate = completion.IsLate
        };
    }

    /// <summary>Counts over the report's frozen staff. It reads grades only, and never an internal note.</summary>
    public virtual async Task<DepartmentReportEvaluationPin> BuildEvaluationPinAsync(Guid cycleId, IReadOnlyCollection<Guid> staff)
    {
        var forms = await _executer.ToListAsync((await _evaluations.WithDetailsAsync(x => x.Scores))
            .Where(x => x.CycleId == cycleId && staff.Contains(x.EmployeeId)));
        var submitted = forms.Where(x => x.Status != EvaluationStatus.Draft).ToList();

        return new DepartmentReportEvaluationPin
        {
            Statuses = forms.ToDictionary(x => x.EmployeeId, x => x.Status),
            ExpectedCount = forms.Count,
            SubmittedCount = submitted.Count,
            AcknowledgedCount = forms.Count(x => x.Status == EvaluationStatus.Acknowledged),
            PendingObjectionsCount = forms.Count(x => x.ObjectionStatus == EvaluationObjectionStatus.Pending),
            GradeCounts = submitted.SelectMany(x => x.Scores).GroupBy(x => x.GradeCode).ToDictionary(g => g.Key, g => g.Count())
        };
    }
}

[Authorize]
public class DepartmentReportAppService : HR_Mangment2AppService, IDepartmentReportAppService
{
    private readonly IRepository<DepartmentReport, Guid> _reports;
    private readonly IRepository<EvaluationCycle, Guid> _cycles;
    private readonly DepartmentReportReader _reader;
    private readonly EmployeeReferenceReader _references;
    private readonly AdministrativeTaskLogger _taskLog;

    public DepartmentReportAppService(
        IRepository<DepartmentReport, Guid> reports,
        IRepository<EvaluationCycle, Guid> cycles,
        DepartmentReportReader reader,
        EmployeeReferenceReader references,
        AdministrativeTaskLogger taskLog)
    {
        _reports = reports;
        _cycles = cycles;
        _reader = reader;
        _references = references;
        _taskLog = taskLog;
    }

    [Authorize(HrPermissions.DepartmentReportsPermissions.Read)]
    public virtual async Task<DepartmentReportSummaryPagedResultDto> GetListAsync(DepartmentReportListInput input)
    {
        var query = (await _reports.GetQueryableAsync())
            .WhereIf(input.CycleId != null, x => x.CycleId == input.CycleId)
            .WhereIf(input.Status != null, x => x.Status == input.Status)
            .WhereIf(input.DepartmentId != null, x => x.DepartmentId == input.DepartmentId);

        if (!await IsHrAsync())
        {
            var me = CurrentUser.GetId();
            query = query.Where(x => x.ManagerUserId == me);
        }

        var rows = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.CreationTime).ThenBy(x => x.Id));
        var cycles = (await _cycles.GetListAsync(x => rows.Select(r => r.CycleId).Contains(x.Id))).ToDictionary(x => x.Id);
        var now = Clock.Now;

        // Lateness depends on the evaluations too, so it is worked out here. A company has one report per manager per month.
        var summaries = new List<(DepartmentReport Report, DepartmentCompletionDto Completion)>();
        foreach (var row in rows)
        {
            var completion = await _reader.GetCompletionAsync(cycles[row.CycleId], row, now);
            if (input.IsLate == null || completion.IsLate == input.IsLate)
            {
                summaries.Add((row, completion));
            }
        }

        var page = summaries.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
        var users = await _references.GetUserRefsAsync(page.Select(x => x.Report.ManagerUserId));
        var departments = await _references.GetDepartmentsAsync(page.Where(x => x.Report.DepartmentId != null).Select(x => x.Report.DepartmentId!.Value));

        return new DepartmentReportSummaryPagedResultDto
        {
            TotalCount = summaries.Count,
            Items = page.Select(x => new DepartmentReportSummaryDto
            {
                Id = x.Report.Id,
                CycleId = x.Report.CycleId,
                Year = cycles[x.Report.CycleId].Year,
                Month = cycles[x.Report.CycleId].Month,
                Manager = users[x.Report.ManagerUserId],
                Department = x.Report.DepartmentId == null ? null : departments.GetValueOrDefault(x.Report.DepartmentId.Value),
                Status = x.Report.Status,
                SubmittedAt = x.Report.SubmittedAt,
                Completion = x.Completion
            }).ToList()
        };
    }

    [Authorize(HrPermissions.DepartmentReportsPermissions.Read)]
    public virtual async Task<DepartmentReportDto> GetAsync(Guid id)
    {
        var report = await _reports.GetAsync(id);
        if (report.ManagerUserId != CurrentUser.GetId() && !await IsHrAsync())
        {
            throw new EntityNotFoundException(typeof(DepartmentReport), id);
        }

        return await ToDtoAsync(report);
    }

    [Authorize(HrPermissions.DepartmentReportsPermissions.Manage)]
    public virtual async Task<DepartmentReportDto> UpdateNotesAsync(Guid id, DepartmentReportNotesPatchDto input)
    {
        var report = await LoadOwnDraftAsync(id);
        report.ConcurrencyStamp = input.ConcurrencyStamp;
        report.SetNotes(input.Notes);
        await _reports.UpdateAsync(report, autoSave: true);
        return await ToDtoAsync(report);
    }

    [Authorize(HrPermissions.DepartmentReportsPermissions.Manage)]
    public virtual async Task<DepartmentReportDto> SubmitAsync(Guid id, DepartmentReportSubmitDto input)
    {
        var report = await LoadOwnDraftAsync(id);
        var staff = DepartmentReportReader.ReadSnapshot(report).Employees.Select(x => x.EmployeeId).ToList();
        var pin = await _reader.BuildEvaluationPinAsync(report.CycleId, staff);

        report.ConcurrencyStamp = input.ConcurrencyStamp;
        report.Submit(Clock.Now, JsonSerializer.Serialize(pin));
        await _reports.UpdateAsync(report, autoSave: true);
        await _taskLog.LogAsync(CurrentUser.GetId(), AdministrativeTaskType.DepartmentReportSubmitted, id);

        return await ToDtoAsync(report);
    }

    private Task<bool> IsHrAsync() => AuthorizationService.IsGrantedAsync(HrPermissions.EvaluationsPermissions.HrRead);

    private async Task<DepartmentReport> LoadOwnDraftAsync(Guid id)
    {
        var report = await _reports.GetAsync(id);
        if (report.ManagerUserId != CurrentUser.GetId())
        {
            throw new EntityNotFoundException(typeof(DepartmentReport), id);
        }

        if (report.Status == DepartmentReportStatus.Submitted)
        {
            throw new BusinessException(HrErrorCodes.DepartmentReports.AlreadySubmitted);
        }

        return report;
    }

    private async Task<DepartmentReportDto> ToDtoAsync(DepartmentReport report)
    {
        var cycle = await AsyncExecuter.FirstAsync((await _cycles.WithDetailsAsync(x => x.Grades)).Where(x => x.Id == report.CycleId));
        var snapshot = DepartmentReportReader.ReadSnapshot(report);
        var staff = snapshot.Employees.Select(x => x.EmployeeId).ToList();

        var pin = report.EvaluationPinJson != null
            ? JsonSerializer.Deserialize<DepartmentReportEvaluationPin>(report.EvaluationPinJson)!
            : await _reader.BuildEvaluationPinAsync(report.CycleId, staff);

        var employees = await _references.GetRefsAsync(staff);
        var users = await _references.GetUserRefsAsync(new[] { report.ManagerUserId });
        var departments = await _references.GetDepartmentsAsync(report.DepartmentId == null ? Array.Empty<Guid>() : new[] { report.DepartmentId.Value });

        return new DepartmentReportDto
        {
            Id = report.Id,
            CycleId = report.CycleId,
            Year = cycle.Year,
            Month = cycle.Month,
            Manager = users[report.ManagerUserId],
            Department = report.DepartmentId == null ? null : departments.GetValueOrDefault(report.DepartmentId.Value),
            Status = report.Status,
            Notes = report.Notes,
            SubmittedAt = report.SubmittedAt,
            ConcurrencyStamp = report.ConcurrencyStamp,
            Completion = await _reader.GetCompletionAsync(cycle, report, Clock.Now),
            Snapshot = new DepartmentReportSnapshotDto
            {
                PeriodFrom = snapshot.PeriodFrom,
                PeriodTo = snapshot.PeriodTo,
                FrozenAt = snapshot.FrozenAt,
                Employees = snapshot.Employees.Select(x => new DepartmentReportEmployeeRowDto
                {
                    Employee = employees[x.EmployeeId],
                    PresentDays = x.PresentDays,
                    AbsentDays = x.AbsentDays,
                    LateCount = x.LateCount,
                    LateMinutes = x.LateMinutes,
                    LeaveDays = x.LeaveDays,
                    PendingRequestsCount = x.PendingRequestsCount,
                    EvaluationStatus = pin.Statuses.TryGetValue(x.EmployeeId, out var status) ? status : null
                }).ToList(),
                Evaluations = new DepartmentReportEvaluationSummaryDto
                {
                    ExpectedCount = pin.ExpectedCount,
                    SubmittedCount = pin.SubmittedCount,
                    AcknowledgedCount = pin.AcknowledgedCount,
                    PendingObjectionsCount = pin.PendingObjectionsCount,
                    GradeCounts = cycle.Grades.OrderBy(x => x.SortOrder)
                        .Where(x => pin.GradeCounts.ContainsKey(x.Code))
                        .Select(x => new EvaluationGradeCountDto { GradeCode = x.Code, GradeName = LocalizedName.Pick(x.NameAr, x.NameEn), Count = pin.GradeCounts[x.Code] })
                        .ToList()
                },
                Custody = new DepartmentReportCustodySummaryDto
                {
                    SharedItemsCount = snapshot.SharedItemsCount,
                    PersonalItemsCount = snapshot.PersonalItemsCount,
                    UnacknowledgedItemsCount = snapshot.UnacknowledgedItemsCount,
                    LastCountDate = snapshot.LastCountDate
                },
                Circulars = new DepartmentReportCircularSummaryDto
                {
                    CircularsSentCount = snapshot.CircularsSentCount,
                    RecipientsCount = snapshot.CircularRecipientsCount,
                    AcknowledgedCount = snapshot.CircularAcknowledgedCount
                }
            }
        };
    }
}

public class DepartmentReportNotesPatchDtoValidator : AbstractValidator<DepartmentReportNotesPatchDto>
{
    public DepartmentReportNotesPatchDtoValidator()
    {
        RuleFor(x => x.Notes).MaximumLength(EvaluationConsts.MaxNotesLength);
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public class DepartmentReportSubmitDtoValidator : AbstractValidator<DepartmentReportSubmitDto>
{
    public DepartmentReportSubmitDtoValidator()
    {
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}
