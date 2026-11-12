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

namespace HR_Mangment2.ReviewerEvaluations;

/// <summary>The reviewer's own evaluation headings (decision 2026-11-11). Separate from the HR manager's headings.</summary>
[Authorize(HrPermissions.ReviewerEvaluationPermissions.Manage)]
public class ReviewerEvaluationCriterionAppService : HR_Mangment2AppService, IReviewerEvaluationCriterionAppService
{
    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["sortOrder"] = nameof(ReviewerEvaluationCriterion.SortOrder),
        ["nameAr"] = nameof(ReviewerEvaluationCriterion.NameAr),
        ["nameEn"] = nameof(ReviewerEvaluationCriterion.NameEn)
    };

    private readonly IRepository<ReviewerEvaluationCriterion, Guid> _criteria;

    public ReviewerEvaluationCriterionAppService(IRepository<ReviewerEvaluationCriterion, Guid> criteria)
    {
        _criteria = criteria;
    }

    public virtual async Task<ReviewerEvaluationCriterionPagedResultDto> GetListAsync(ReviewerEvaluationCriterionListInput input)
    {
        var query = (await _criteria.GetQueryableAsync()).WhereIf(input.IsActive != null, x => x.IsActive == input.IsActive);
        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query
            .ApplySorting(input.Sorting, nameof(ReviewerEvaluationCriterion.SortOrder) + ", " + nameof(ReviewerEvaluationCriterion.NameAr), SortFields)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount));
        return new ReviewerEvaluationCriterionPagedResultDto { TotalCount = total, Items = rows.Select(ToDto).ToList() };
    }

    public virtual async Task<ReviewerEvaluationCriterionDto> CreateAsync(ReviewerEvaluationCriterionCreateDto input)
    {
        var criterion = new ReviewerEvaluationCriterion(GuidGenerator.Create(), CurrentTenant.Id, input.NameAr.Trim(), input.NameEn.Trim(), input.SortOrder!.Value);
        await _criteria.InsertAsync(criterion, autoSave: true);
        return ToDto(criterion);
    }

    public virtual async Task<ReviewerEvaluationCriterionDto> UpdateAsync(Guid id, ReviewerEvaluationCriterionUpdateDto input)
    {
        var criterion = await _criteria.GetAsync(id);
        criterion.ConcurrencyStamp = input.ConcurrencyStamp;
        criterion.Set(input.NameAr.Trim(), input.NameEn.Trim(), input.SortOrder!.Value, input.IsActive!.Value);
        await _criteria.UpdateAsync(criterion, autoSave: true);
        return ToDto(criterion);
    }

    internal static ReviewerEvaluationCriterionDto ToDto(ReviewerEvaluationCriterion x) => new()
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
/// The reviewer grades any eligible employee for a day they pick: a grade and an optional reason per active heading,
/// with no numbers and no average. Sent to the executive director one at a time or together, like the HR manager's
/// evaluations (decision 2026-11-11). Nothing ever reaches the evaluated employee.
/// </summary>
[Authorize]
public class ReviewerEvaluationAppService : HR_Mangment2AppService, IReviewerEvaluationAppService
{
    private const string CreateOperation = "ReviewerEvaluation_Create";
    private const string SendOneOperation = "ReviewerEvaluation_Send";
    private const string SendOperation = "ReviewerEvaluationReport_Send";

    private readonly IRepository<ReviewerEvaluation, Guid> _evaluations;
    private readonly IRepository<ReviewerEvaluationCriterion, Guid> _criteria;
    private readonly IRepository<ReviewerEvaluationReport, Guid> _reports;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IIdentityUserRepository _users;
    private readonly IdentityUserManager _userManager;
    private readonly EmployeeReferenceReader _references;
    private readonly HrNotificationPublisher _notifications;
    private readonly IdempotencyStore _idempotency;

    public ReviewerEvaluationAppService(
        IRepository<ReviewerEvaluation, Guid> evaluations,
        IRepository<ReviewerEvaluationCriterion, Guid> criteria,
        IRepository<ReviewerEvaluationReport, Guid> reports,
        IRepository<Employee, Guid> employees,
        IIdentityUserRepository users,
        IdentityUserManager userManager,
        EmployeeReferenceReader references,
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
        _notifications = notifications;
        _idempotency = idempotency;
    }

    [Authorize(HrPermissions.ReviewerEvaluationPermissions.Manage)]
    public virtual async Task<ReviewerEvaluationPagedResultDto> GetListAsync(ReviewerEvaluationListInput input)
    {
        var query = (await _evaluations.WithDetailsAsync(x => x.Items))
            .WhereIf(input.EmployeeId != null, x => x.EmployeeId == input.EmployeeId)
            .WhereIf(input.IsSent == true, x => x.ReportId != null)
            .WhereIf(input.IsSent == false, x => x.ReportId == null)
            .WhereIf(input.From != null, x => x.EvaluationDate >= input.From)
            .WhereIf(input.To != null, x => x.EvaluationDate <= input.To);

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query
            .OrderByDescending(x => x.EvaluationDate).ThenByDescending(x => x.EvaluatedAt).ThenBy(x => x.Id)
            .Skip(input.SkipCount).Take(input.MaxResultCount));
        return new ReviewerEvaluationPagedResultDto { TotalCount = total, Items = await ToDtosAsync(rows) };
    }

    [Authorize(HrPermissions.ReviewerEvaluationPermissions.Manage)]
    public virtual async Task<ReviewerEvaluationDto> CreateAsync(ReviewerEvaluationCreateDto input, Guid? idempotencyKey)
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
        var given = input.Items.Select(x => x.CriterionId!.Value).ToList();
        if (given.Any(id => active.All(c => c.Id != id)))
        {
            throw new BusinessException(HrErrorCodes.ReviewerEvaluation.CriterionNotFound);
        }

        if (given.Distinct().Count() != given.Count || active.Any(c => !given.Contains(c.Id)))
        {
            throw new BusinessException(HrErrorCodes.ReviewerEvaluation.ItemsIncomplete);
        }

        var notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim();
        var evaluation = new ReviewerEvaluation(GuidGenerator.Create(), CurrentTenant.Id, employee!.Id, me, input.EvaluationDate!.Value, Clock.Now, notes);
        foreach (var item in input.Items)
        {
            var reason = string.IsNullOrWhiteSpace(item.Reason) ? null : item.Reason.Trim();
            evaluation.Items.Add(new ReviewerEvaluationItem(GuidGenerator.Create(), CurrentTenant.Id, evaluation.Id, item.CriterionId!.Value, item.Grade!.Value, reason));
        }

        await _evaluations.InsertAsync(evaluation, autoSave: true);
        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, input, evaluation.Id);
        return (await ToDtosAsync(new[] { evaluation }))[0];
    }

    /// <summary>
    /// Everyone the reviewer may evaluate (decision 2026-11-11): active employees with an account, except the reviewer,
    /// executive directors and system-admin-only accounts. The warning picker keeps department managers only.
    /// </summary>
    [Authorize(HrPermissions.ReviewerEvaluationPermissions.Manage)]
    public virtual async Task<EmployeeLookupPagedResultDto> GetCandidatesAsync(ReviewerEvaluationCandidateInput input)
    {
        var me = CurrentUser.GetId();
        var excluded = new List<Guid> { me };
        excluded.AddRange((await _userManager.GetUsersInRoleAsync(HrRoles.ExecutiveDirector)).Select(x => x.Id));
        foreach (var admin in await _userManager.GetUsersInRoleAsync(HrRoles.SystemAdmin))
        {
            if (AccountProvisioner.IsSystemAdminOnly((await _userManager.GetRolesAsync(admin)).ToList()))
            {
                excluded.Add(admin.Id);
            }
        }

        var search = input.Search?.Trim();
        var query = (await _employees.GetQueryableAsync())
            .Where(x => x.Status == EmployeeStatus.Active && x.UserId != null && !excluded.Contains(x.UserId!.Value))
            .WhereIf(!string.IsNullOrEmpty(search), x =>
                x.FullNameAr.Contains(search!) ||
                (x.FullNameEn != null && x.FullNameEn.Contains(search!)) ||
                (x.EmployeeNumber != null && x.EmployeeNumber.Contains(search!)));

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.FullNameAr).ThenBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount));
        var refs = await _references.ToRefsAsync(rows);
        return new EmployeeLookupPagedResultDto { TotalCount = total, Items = rows.Select(x => refs[x.Id]).ToList() };
    }

    [Authorize(HrPermissions.ReviewerEvaluationPermissions.Manage)]
    public virtual async Task<ReviewerEvaluationDto> GetAsync(Guid id) => (await ToDtosAsync(new[] { await LoadAsync(id) }))[0];

    /// <summary>One evaluation of the caller, in a report of its own, after the client confirmed it.</summary>
    [Authorize(HrPermissions.ReviewerEvaluationPermissions.Manage)]
    public virtual async Task<ReviewerEvaluationReportDto> SendOneAsync(Guid id, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(SendOneOperation, idempotencyKey, id);
        if (earlier != null)
        {
            return await GetReportAsync(earlier.Value);
        }

        var me = CurrentUser.GetId();
        var evaluation = await _evaluations.FirstOrDefaultAsync(x => x.Id == id && x.EvaluatedByUserId == me)
                         ?? throw new EntityNotFoundException(typeof(ReviewerEvaluation), id);
        if (evaluation.ReportId != null)
        {
            throw new BusinessException(HrErrorCodes.ReviewerEvaluation.AlreadySent);
        }

        var report = await SendAsync(new List<ReviewerEvaluation> { evaluation }, null);
        await _idempotency.SaveAsync(SendOneOperation, idempotencyKey, id, report.Id);
        return await GetReportAsync(report.Id);
    }

    [Authorize(HrPermissions.ReviewerEvaluationPermissions.ReadReports)]
    public virtual async Task<ReviewerEvaluationReportPagedResultDto> GetReportListAsync(PagedAndSortedResultRequestDto input)
    {
        var query = await VisibleReportsAsync();
        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.SentAt).ThenBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount));
        var users = await _references.GetUserRefsAsync(rows.Select(x => x.SentByUserId));
        return new ReviewerEvaluationReportPagedResultDto { TotalCount = total, Items = rows.Select(x => Fill(new ReviewerEvaluationReportSummaryDto(), x, users)).ToList() };
    }

    /// <summary>Bundles every unsent evaluation of the caller into one report for the executive director.</summary>
    [Authorize(HrPermissions.ReviewerEvaluationPermissions.Manage)]
    public virtual async Task<ReviewerEvaluationReportDto> SendReportAsync(ReviewerEvaluationReportCreateDto input, Guid? idempotencyKey)
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
            throw new BusinessException(HrErrorCodes.ReviewerEvaluation.NothingToSend);
        }

        var report = await SendAsync(unsent, string.IsNullOrWhiteSpace(input.Note) ? null : input.Note.Trim());
        await _idempotency.SaveAsync(SendOperation, idempotencyKey, input, report.Id);
        return await GetReportAsync(report.Id);
    }

    [Authorize(HrPermissions.ReviewerEvaluationPermissions.ReadReports)]
    public virtual async Task<ReviewerEvaluationReportDto> GetReportAsync(Guid id)
    {
        var report = await AsyncExecuter.FirstOrDefaultAsync((await VisibleReportsAsync()).Where(x => x.Id == id))
                     ?? throw new EntityNotFoundException(typeof(ReviewerEvaluationReport), id);

        var evaluations = await AsyncExecuter.ToListAsync((await _evaluations.WithDetailsAsync(x => x.Items))
            .Where(x => x.ReportId == id)
            .OrderBy(x => x.EvaluationDate).ThenBy(x => x.EvaluatedAt).ThenBy(x => x.Id));
        var users = await _references.GetUserRefsAsync(new[] { report.SentByUserId });

        var dto = Fill(new ReviewerEvaluationReportDto(), report, users);
        dto.Evaluations = await ToDtosAsync(evaluations);
        return dto;
    }

    /// <summary>Puts the evaluations in a new report and notifies the active executive directors.</summary>
    private async Task<ReviewerEvaluationReport> SendAsync(List<ReviewerEvaluation> unsent, string? note)
    {
        var me = CurrentUser.GetId();
        var now = Clock.Now;
        var report = new ReviewerEvaluationReport(GuidGenerator.Create(), CurrentTenant.Id, me, now, note, unsent.Count,
            unsent.Min(x => x.EvaluationDate), unsent.Max(x => x.EvaluationDate));
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
                await _notifications.PublishAsync(executive.Id, NotificationTemplateCode.ReviewerEvaluationReportSent, NotificationResourceType.ReviewerEvaluationReport, report.Id);
            }
        }

        return report;
    }

    /// <summary>The executive director reads every report; the reviewer reads the ones they sent.</summary>
    private async Task<IQueryable<ReviewerEvaluationReport>> VisibleReportsAsync()
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
            throw new BusinessException(HrErrorCodes.ReviewerEvaluation.EmployeeNotAllowed);
        }

        if (employee.UserId != null)
        {
            var roles = await _users.GetRoleNamesAsync(employee.UserId.Value);
            if (roles.Contains(HrRoles.ExecutiveDirector) || AccountProvisioner.IsSystemAdminOnly(roles))
            {
                throw new BusinessException(HrErrorCodes.ReviewerEvaluation.EmployeeNotAllowed);
            }
        }
    }

    private async Task<ReviewerEvaluation> LoadAsync(Guid id) =>
        await AsyncExecuter.FirstOrDefaultAsync((await _evaluations.WithDetailsAsync(x => x.Items)).Where(x => x.Id == id))
        ?? throw new EntityNotFoundException(typeof(ReviewerEvaluation), id);

    private async Task<List<ReviewerEvaluationDto>> ToDtosAsync(IReadOnlyCollection<ReviewerEvaluation> rows)
    {
        var employees = await _references.GetRefsAsync(rows.Select(x => x.EmployeeId));
        var users = await _references.GetUserRefsAsync(rows.Select(x => x.EvaluatedByUserId));
        var criterionIds = rows.SelectMany(x => x.Items).Select(x => x.CriterionId).Distinct().ToList();
        var criteria = (await _criteria.GetListAsync(x => criterionIds.Contains(x.Id))).ToDictionary(x => x.Id);

        return rows.Select(x => new ReviewerEvaluationDto
        {
            Id = x.Id,
            Employee = employees[x.EmployeeId],
            EvaluatedBy = users[x.EvaluatedByUserId],
            EvaluationDate = x.EvaluationDate,
            EvaluatedAt = x.EvaluatedAt,
            Items = x.Items
                .OrderBy(i => criteria[i.CriterionId].SortOrder).ThenBy(i => criteria[i.CriterionId].NameAr)
                .Select(i => new ReviewerEvaluationItemDto
                {
                    CriterionId = i.CriterionId,
                    CriterionName = LocalizedName.Pick(criteria[i.CriterionId].NameAr, criteria[i.CriterionId].NameEn),
                    Grade = i.Grade,
                    Reason = i.Reason
                })
                .ToList(),
            Notes = x.Notes,
            ReportId = x.ReportId,
            SentAt = x.SentAt
        }).ToList();
    }

    private static T Fill<T>(T dto, ReviewerEvaluationReport x, IReadOnlyDictionary<Guid, UserRefDto> users) where T : ReviewerEvaluationReportSummaryDto
    {
        dto.Id = x.Id;
        dto.SentBy = users[x.SentByUserId];
        dto.SentAt = x.SentAt;
        dto.Note = x.Note;
        dto.EvaluationsCount = x.EvaluationsCount;
        dto.FirstEvaluationDate = x.FirstEvaluationDate;
        dto.LastEvaluationDate = x.LastEvaluationDate;
        return dto;
    }
}

public class ReviewerEvaluationCriterionCreateDtoValidator : AbstractValidator<ReviewerEvaluationCriterionCreateDto>
{
    public ReviewerEvaluationCriterionCreateDtoValidator()
    {
        RuleFor(x => x.NameAr).NotNull().Length(1, ReviewerEvaluationCriterion.MaxNameLength);
        RuleFor(x => x.NameEn).NotNull().Length(1, ReviewerEvaluationCriterion.MaxNameLength);
        RuleFor(x => x.SortOrder).NotNull().GreaterThanOrEqualTo(0);
    }
}

public class ReviewerEvaluationCriterionUpdateDtoValidator : AbstractValidator<ReviewerEvaluationCriterionUpdateDto>
{
    public ReviewerEvaluationCriterionUpdateDtoValidator()
    {
        RuleFor(x => x.NameAr).NotNull().Length(1, ReviewerEvaluationCriterion.MaxNameLength);
        RuleFor(x => x.NameEn).NotNull().Length(1, ReviewerEvaluationCriterion.MaxNameLength);
        RuleFor(x => x.SortOrder).NotNull().GreaterThanOrEqualTo(0);
        RuleFor(x => x.IsActive).NotNull();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public class ReviewerEvaluationCreateDtoValidator : AbstractValidator<ReviewerEvaluationCreateDto>
{
    public ReviewerEvaluationCreateDtoValidator()
    {
        RuleFor(x => x.EmployeeId).NotNull();
        RuleFor(x => x.EvaluationDate).NotNull();
        RuleFor(x => x.Items).NotNull();
        RuleFor(x => x.Items).Must(x => x.Count is >= 1 and <= ReviewerEvaluation.MaxItems).When(x => x.Items != null);
        RuleForEach(x => x.Items).ChildRules(i =>
        {
            i.RuleFor(x => x.CriterionId).NotNull();
            i.RuleFor(x => x.Grade).NotNull().IsInEnum();
            i.RuleFor(x => x.Reason).MaximumLength(ReviewerEvaluation.MaxReasonLength);
        }).When(x => x.Items != null);
        RuleFor(x => x.Notes).MaximumLength(ReviewerEvaluation.MaxNotesLength);
    }
}

public class ReviewerEvaluationReportCreateDtoValidator : AbstractValidator<ReviewerEvaluationReportCreateDto>
{
    public ReviewerEvaluationReportCreateDtoValidator()
    {
        RuleFor(x => x.Note).MaximumLength(ReviewerEvaluationReport.MaxNoteLength);
    }
}
