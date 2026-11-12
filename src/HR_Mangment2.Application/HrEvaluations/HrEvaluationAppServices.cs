using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Common;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Identity;
using HR_Mangment2.Localization;
using HR_Mangment2.Notifications;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace HR_Mangment2.HrEvaluations;

/// <summary>The HR manager's own evaluation headings (decision 2026-11-10). Separate from the monthly criteria.</summary>
[Authorize(HrPermissions.HrEvaluationPermissions.Manage)]
public class HrEvaluationCriterionAppService : HR_Mangment2AppService, IHrEvaluationCriterionAppService
{
    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["sortOrder"] = nameof(HrEvaluationCriterion.SortOrder),
        ["nameAr"] = nameof(HrEvaluationCriterion.NameAr),
        ["nameEn"] = nameof(HrEvaluationCriterion.NameEn)
    };

    private readonly IRepository<HrEvaluationCriterion, Guid> _criteria;

    public HrEvaluationCriterionAppService(IRepository<HrEvaluationCriterion, Guid> criteria)
    {
        _criteria = criteria;
    }

    public virtual async Task<HrEvaluationCriterionPagedResultDto> GetListAsync(HrEvaluationCriterionListInput input)
    {
        var query = (await _criteria.GetQueryableAsync()).WhereIf(input.IsActive != null, x => x.IsActive == input.IsActive);
        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query
            .ApplySorting(input.Sorting, nameof(HrEvaluationCriterion.SortOrder) + ", " + nameof(HrEvaluationCriterion.NameAr), SortFields)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount));
        return new HrEvaluationCriterionPagedResultDto { TotalCount = total, Items = rows.Select(ToDto).ToList() };
    }

    public virtual async Task<HrEvaluationCriterionDto> CreateAsync(HrEvaluationCriterionCreateDto input)
    {
        var criterion = new HrEvaluationCriterion(GuidGenerator.Create(), CurrentTenant.Id, input.NameAr.Trim(), input.NameEn.Trim(), input.SortOrder!.Value);
        await _criteria.InsertAsync(criterion, autoSave: true);
        return ToDto(criterion);
    }

    public virtual async Task<HrEvaluationCriterionDto> UpdateAsync(Guid id, HrEvaluationCriterionUpdateDto input)
    {
        var criterion = await _criteria.GetAsync(id);
        criterion.ConcurrencyStamp = input.ConcurrencyStamp;
        criterion.Set(input.NameAr.Trim(), input.NameEn.Trim(), input.SortOrder!.Value, input.IsActive!.Value);
        await _criteria.UpdateAsync(criterion, autoSave: true);
        return ToDto(criterion);
    }

    internal static HrEvaluationCriterionDto ToDto(HrEvaluationCriterion x) => new()
    {
        Id = x.Id,
        Name = LocalizedName.Pick(x.NameAr, x.NameEn),
        NameAr = x.NameAr,
        NameEn = x.NameEn,
        SortOrder = x.SortOrder,
        IsActive = x.IsActive,
        ConcurrencyStamp = x.ConcurrencyStamp
    };
}

/// <summary>
/// The HR manager evaluates any eligible employee at any time with a 1-10 score per active heading, and sends the
/// unsent ones to the executive director as a report (decision 2026-11-10). Nothing ever reaches the evaluated employee.
/// </summary>
[Authorize]
public class HrEvaluationAppService : HR_Mangment2AppService, IHrEvaluationAppService
{
    private const string CreateOperation = "HrEvaluation_Create";
    private const string SendOperation = "HrEvaluationReport_Send";
    private const string SendOneOperation = "HrEvaluation_Send";

    private readonly IRepository<HrEvaluation, Guid> _evaluations;
    private readonly IRepository<HrEvaluationCriterion, Guid> _criteria;
    private readonly IRepository<HrEvaluationReport, Guid> _reports;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IIdentityUserRepository _users;
    private readonly IdentityUserManager _userManager;
    private readonly EmployeeReferenceReader _references;
    private readonly ICompanyClock _companyClock;
    private readonly HrNotificationPublisher _notifications;
    private readonly IdempotencyStore _idempotency;

    public HrEvaluationAppService(
        IRepository<HrEvaluation, Guid> evaluations,
        IRepository<HrEvaluationCriterion, Guid> criteria,
        IRepository<HrEvaluationReport, Guid> reports,
        IRepository<Employee, Guid> employees,
        IIdentityUserRepository users,
        IdentityUserManager userManager,
        EmployeeReferenceReader references,
        ICompanyClock companyClock,
        HrNotificationPublisher notifications,
        IdempotencyStore idempotency)
    {
        _evaluations = evaluations;
        _criteria = criteria;
        _reports = reports;
        _employees = employees;
        _users = users;
        _userManager = userManager;
        _references = references;
        _companyClock = companyClock;
        _notifications = notifications;
        _idempotency = idempotency;
    }

    [Authorize(HrPermissions.HrEvaluationPermissions.Manage)]
    public virtual async Task<HrEvaluationPagedResultDto> GetListAsync(HrEvaluationListInput input)
    {
        var query = (await _evaluations.WithDetailsAsync(x => x.Scores))
            .WhereIf(input.EmployeeId != null, x => x.EmployeeId == input.EmployeeId)
            .WhereIf(input.IsSent == true, x => x.ReportId != null)
            .WhereIf(input.IsSent == false, x => x.ReportId == null);

        if (input.From != null)
        {
            var start = await _companyClock.StartOfDayUtcAsync(input.From.Value);
            query = query.Where(x => x.EvaluatedAt >= start);
        }

        if (input.To != null)
        {
            var end = await _companyClock.StartOfDayUtcAsync(input.To.Value.AddDays(1));
            query = query.Where(x => x.EvaluatedAt < end);
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.EvaluatedAt).ThenBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount));
        return new HrEvaluationPagedResultDto { TotalCount = total, Items = await ToDtosAsync(rows) };
    }

    [Authorize(HrPermissions.HrEvaluationPermissions.Manage)]
    public virtual async Task<HrEvaluationDto> CreateAsync(HrEvaluationCreateDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return (await ToDtosAsync(new[] { await LoadAsync(earlier.Value) }))[0];
        }

        var me = CurrentUser.GetId();
        var employee = await _employees.FindAsync(input.EmployeeId!.Value);
        await EnsureEvaluableAsync(employee, me);

        var active = await _criteria.GetListAsync(x => x.IsActive);
        var given = input.Scores.Select(x => x.CriterionId!.Value).ToList();
        if (given.Any(id => active.All(c => c.Id != id)))
        {
            throw new BusinessException(HrErrorCodes.HrEvaluation.CriterionNotFound);
        }

        if (given.Distinct().Count() != given.Count || active.Any(c => !given.Contains(c.Id)))
        {
            throw new BusinessException(HrErrorCodes.HrEvaluation.ScoresIncomplete);
        }

        var notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim();
        var evaluation = new HrEvaluation(GuidGenerator.Create(), CurrentTenant.Id, employee!.Id, me, Clock.Now, notes);
        foreach (var score in input.Scores)
        {
            evaluation.Scores.Add(new HrEvaluationScore(GuidGenerator.Create(), CurrentTenant.Id, evaluation.Id, score.CriterionId!.Value, score.Score!.Value));
        }

        await _evaluations.InsertAsync(evaluation, autoSave: true);
        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, input, evaluation.Id);
        return (await ToDtosAsync(new[] { evaluation }))[0];
    }

    [Authorize(HrPermissions.HrEvaluationPermissions.Manage)]
    public virtual async Task<HrEvaluationDto> GetAsync(Guid id) => (await ToDtosAsync(new[] { await LoadAsync(id) }))[0];

    [Authorize(HrPermissions.HrEvaluationPermissions.ReadReports)]
    public virtual async Task<HrEvaluationReportPagedResultDto> GetReportListAsync(PagedAndSortedResultRequestDto input)
    {
        var query = await VisibleReportsAsync();
        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.SentAt).ThenBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount));
        var users = await _references.GetUserRefsAsync(rows.Select(x => x.SentByUserId));
        return new HrEvaluationReportPagedResultDto { TotalCount = total, Items = rows.Select(x => Fill(new HrEvaluationReportSummaryDto(), x, users)).ToList() };
    }

    /// <summary>Bundles every unsent evaluation of the caller into one report for the executive director.</summary>
    [Authorize(HrPermissions.HrEvaluationPermissions.Manage)]
    public virtual async Task<HrEvaluationReportDto> SendReportAsync(HrEvaluationReportCreateDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(SendOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return await GetReportAsync(earlier.Value);
        }

        var me = CurrentUser.GetId();
        var unsent = await _evaluations.GetListAsync(x => x.EvaluatedByUserId == me && x.ReportId == null);
        if (unsent.Count == 0)
        {
            throw new BusinessException(HrErrorCodes.HrEvaluation.NothingToSend);
        }

        var report = await SendAsync(unsent, string.IsNullOrWhiteSpace(input.Note) ? null : input.Note.Trim());
        await _idempotency.SaveAsync(SendOperation, idempotencyKey, input, report.Id);
        return await GetReportAsync(report.Id);
    }

    /// <summary>
    /// One evaluation of the caller, in a report of its own, after the client confirmed it (decision 2026-11-11).
    /// Another HR manager's evaluation is 404; a sent one is AlreadySent.
    /// </summary>
    [Authorize(HrPermissions.HrEvaluationPermissions.Manage)]
    public virtual async Task<HrEvaluationReportDto> SendOneAsync(Guid id, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(SendOneOperation, idempotencyKey, id);
        if (earlier != null)
        {
            return await GetReportAsync(earlier.Value);
        }

        var me = CurrentUser.GetId();
        var evaluation = await _evaluations.FirstOrDefaultAsync(x => x.Id == id && x.EvaluatedByUserId == me)
                         ?? throw new EntityNotFoundException(typeof(HrEvaluation), id);
        if (evaluation.ReportId != null)
        {
            throw new BusinessException(HrErrorCodes.HrEvaluation.AlreadySent);
        }

        var report = await SendAsync(new List<HrEvaluation> { evaluation }, null);
        await _idempotency.SaveAsync(SendOneOperation, idempotencyKey, id, report.Id);
        return await GetReportAsync(report.Id);
    }

    /// <summary>Puts the evaluations in a new report and notifies the active executive directors.</summary>
    private async Task<HrEvaluationReport> SendAsync(List<HrEvaluation> unsent, string? note)
    {
        var me = CurrentUser.GetId();
        var now = Clock.Now;
        var report = new HrEvaluationReport(GuidGenerator.Create(), CurrentTenant.Id, me, now, note, unsent.Count, unsent.Min(x => x.EvaluatedAt), unsent.Max(x => x.EvaluatedAt));
        await _reports.InsertAsync(report, autoSave: true);

        foreach (var evaluation in unsent)
        {
            evaluation.MarkSent(report.Id, now);
        }

        await _evaluations.UpdateManyAsync(unsent, autoSave: true);

        foreach (var executive in await _userManager.GetUsersInRoleAsync(HrRoles.ExecutiveDirector))
        {
            if (executive.IsActive && executive.Id != me)
            {
                await _notifications.PublishAsync(executive.Id, NotificationTemplateCode.HrEvaluationReportSent, NotificationResourceType.HrEvaluationReport, report.Id);
            }
        }

        return report;
    }

    [Authorize(HrPermissions.HrEvaluationPermissions.ReadReports)]
    public virtual async Task<HrEvaluationReportDto> GetReportAsync(Guid id)
    {
        var report = await AsyncExecuter.FirstOrDefaultAsync((await VisibleReportsAsync()).Where(x => x.Id == id))
                     ?? throw new EntityNotFoundException(typeof(HrEvaluationReport), id);

        var evaluations = await AsyncExecuter.ToListAsync((await _evaluations.WithDetailsAsync(x => x.Scores))
            .Where(x => x.ReportId == id)
            .OrderBy(x => x.EvaluatedAt).ThenBy(x => x.Id));
        var users = await _references.GetUserRefsAsync(new[] { report.SentByUserId });

        var dto = Fill(new HrEvaluationReportDto(), report, users);
        dto.Evaluations = await ToDtosAsync(evaluations);
        return dto;
    }

    /// <summary>The executive director reads every report; the HR manager reads the ones they sent.</summary>
    private async Task<IQueryable<HrEvaluationReport>> VisibleReportsAsync()
    {
        var me = CurrentUser.GetId();
        var query = await _reports.GetQueryableAsync();
        var roles = await _users.GetRoleNamesAsync(me);
        return roles.Contains(HrRoles.ExecutiveDirector) ? query : query.Where(x => x.SentByUserId == me);
    }

    /// <summary>An active employee who is not the evaluator, not the executive director and not a system-admin-only account.</summary>
    private async Task EnsureEvaluableAsync(Employee? employee, Guid me)
    {
        if (employee == null || employee.Status != EmployeeStatus.Active || employee.UserId == me)
        {
            throw new BusinessException(HrErrorCodes.HrEvaluation.EmployeeNotAllowed);
        }

        if (employee.UserId != null)
        {
            var roles = await _users.GetRoleNamesAsync(employee.UserId.Value);
            if (roles.Contains(HrRoles.ExecutiveDirector) || AccountProvisioner.IsSystemAdminOnly(roles))
            {
                throw new BusinessException(HrErrorCodes.HrEvaluation.EmployeeNotAllowed);
            }
        }
    }

    private async Task<HrEvaluation> LoadAsync(Guid id) =>
        await AsyncExecuter.FirstOrDefaultAsync((await _evaluations.WithDetailsAsync(x => x.Scores)).Where(x => x.Id == id))
        ?? throw new EntityNotFoundException(typeof(HrEvaluation), id);

    private async Task<List<HrEvaluationDto>> ToDtosAsync(IReadOnlyCollection<HrEvaluation> rows)
    {
        var employees = await _references.GetRefsAsync(rows.Select(x => x.EmployeeId));
        var users = await _references.GetUserRefsAsync(rows.Select(x => x.EvaluatedByUserId));
        var criterionIds = rows.SelectMany(x => x.Scores).Select(x => x.CriterionId).Distinct().ToList();
        var criteria = (await _criteria.GetListAsync(x => criterionIds.Contains(x.Id))).ToDictionary(x => x.Id);

        return rows.Select(x => new HrEvaluationDto
        {
            Id = x.Id,
            Employee = employees[x.EmployeeId],
            EvaluatedBy = users[x.EvaluatedByUserId],
            EvaluatedAt = x.EvaluatedAt,
            Scores = x.Scores
                .OrderBy(s => criteria[s.CriterionId].SortOrder).ThenBy(s => criteria[s.CriterionId].NameAr)
                .Select(s => new HrEvaluationScoreDto
                {
                    CriterionId = s.CriterionId,
                    CriterionName = LocalizedName.Pick(criteria[s.CriterionId].NameAr, criteria[s.CriterionId].NameEn),
                    Score = s.Score
                })
                .ToList(),
            AverageScore = x.Scores.Count == 0 ? 0 : Math.Round((decimal)x.Scores.Sum(s => s.Score) / x.Scores.Count, 2, MidpointRounding.AwayFromZero),
            Notes = x.Notes,
            ReportId = x.ReportId,
            SentAt = x.SentAt
        }).ToList();
    }

    private static T Fill<T>(T dto, HrEvaluationReport x, IReadOnlyDictionary<Guid, UserRefDto> users) where T : HrEvaluationReportSummaryDto
    {
        dto.Id = x.Id;
        dto.SentBy = users[x.SentByUserId];
        dto.SentAt = x.SentAt;
        dto.Note = x.Note;
        dto.EvaluationsCount = x.EvaluationsCount;
        dto.FirstEvaluatedAt = x.FirstEvaluatedAt;
        dto.LastEvaluatedAt = x.LastEvaluatedAt;
        return dto;
    }
}

public class HrEvaluationCriterionCreateDtoValidator : AbstractValidator<HrEvaluationCriterionCreateDto>
{
    public HrEvaluationCriterionCreateDtoValidator()
    {
        RuleFor(x => x.NameAr).NotNull().Length(1, HrEvaluationCriterion.MaxNameLength);
        RuleFor(x => x.NameEn).NotNull().Length(1, HrEvaluationCriterion.MaxNameLength);
        RuleFor(x => x.SortOrder).NotNull().GreaterThanOrEqualTo(0);
    }
}

public class HrEvaluationCriterionUpdateDtoValidator : AbstractValidator<HrEvaluationCriterionUpdateDto>
{
    public HrEvaluationCriterionUpdateDtoValidator()
    {
        RuleFor(x => x.NameAr).NotNull().Length(1, HrEvaluationCriterion.MaxNameLength);
        RuleFor(x => x.NameEn).NotNull().Length(1, HrEvaluationCriterion.MaxNameLength);
        RuleFor(x => x.SortOrder).NotNull().GreaterThanOrEqualTo(0);
        RuleFor(x => x.IsActive).NotNull();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public class HrEvaluationCreateDtoValidator : AbstractValidator<HrEvaluationCreateDto>
{
    public HrEvaluationCreateDtoValidator()
    {
        RuleFor(x => x.EmployeeId).NotNull();
        RuleFor(x => x.Scores).NotNull();
        RuleFor(x => x.Scores).Must(x => x.Count is >= 1 and <= HrEvaluation.MaxScores).When(x => x.Scores != null);
        RuleForEach(x => x.Scores).ChildRules(s =>
        {
            s.RuleFor(x => x.CriterionId).NotNull();
            s.RuleFor(x => x.Score).NotNull().InclusiveBetween(HrEvaluation.MinScore, HrEvaluation.MaxScore);
        }).When(x => x.Scores != null);
        RuleFor(x => x.Notes).MaximumLength(HrEvaluation.MaxNotesLength);
    }
}

public class HrEvaluationReportCreateDtoValidator : AbstractValidator<HrEvaluationReportCreateDto>
{
    public HrEvaluationReportCreateDtoValidator()
    {
        RuleFor(x => x.Note).MaximumLength(HrEvaluationReport.MaxNoteLength);
    }
}
