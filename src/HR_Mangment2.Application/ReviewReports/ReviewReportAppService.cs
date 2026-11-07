using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Identity;
using HR_Mangment2.Notifications;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace HR_Mangment2.ReviewReports;

/// <summary>
/// The reviewer's follow-up reports (owner's decision 2026-11-07). The reviewer sees only their own reports; the
/// executive director sees submitted ones only, and the first look is recorded. A report never shows financial or
/// complaint data: a team row holds the reviewer's own assessment and a head count.
/// </summary>
[Authorize]
public class ReviewReportAppService : HR_Mangment2AppService, IReviewReportAppService
{
    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["periodFrom"] = nameof(ReviewReport.PeriodFrom),
        ["submittedAt"] = nameof(ReviewReport.SubmittedAt),
        ["creationTime"] = nameof(ReviewReport.CreationTime)
    };

    private readonly IRepository<ReviewReport, Guid> _reports;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IdentityUserManager _userManager;
    private readonly EmployeeReferenceReader _references;
    private readonly HrNotificationPublisher _notifications;

    public ReviewReportAppService(
        IRepository<ReviewReport, Guid> reports,
        IRepository<Employee, Guid> employees,
        IdentityUserManager userManager,
        EmployeeReferenceReader references,
        HrNotificationPublisher notifications)
    {
        _reports = reports;
        _employees = employees;
        _userManager = userManager;
        _references = references;
        _notifications = notifications;
    }

    [Authorize(HrPermissions.ReviewReportsPermissions.Read)]
    public virtual async Task<ReviewReportSummaryPagedResultDto> GetListAsync(ReviewReportListInput input)
    {
        var query = (await _reports.WithDetailsAsync(x => x.Teams))
            .WhereIf(input.PeriodType != null, x => x.PeriodType == input.PeriodType);

        if (await IsAuthorAsync())
        {
            var me = CurrentUser.GetId();
            query = query.Where(x => x.ReviewerUserId == me).WhereIf(input.Status != null, x => x.Status == input.Status);
        }
        else
        {
            // The executive director never sees a draft.
            query = query.Where(x => x.Status == ReviewReportStatus.Submitted);
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(ReviewReport.PeriodFrom) + " desc", SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));
        var reviewers = await _references.GetUserRefsAsync(rows.Select(x => x.ReviewerUserId));

        return new ReviewReportSummaryPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => new ReviewReportSummaryDto
            {
                Id = x.Id,
                Reviewer = reviewers[x.ReviewerUserId],
                PeriodType = x.PeriodType,
                PeriodFrom = x.PeriodFrom,
                PeriodTo = x.PeriodTo,
                Status = x.Status,
                TeamsCount = x.Teams.Count,
                AverageCompletion = Average(x.Teams),
                SubmittedAt = x.SubmittedAt,
                ViewedAt = x.ViewedAt
            }).ToList()
        };
    }

    [Authorize(HrPermissions.ReviewReportsPermissions.Manage)]
    public virtual async Task<ReviewReportDto> CreateAsync(ReviewReportCreateDto input)
    {
        var me = CurrentUser.GetId();
        var report = new ReviewReport(GuidGenerator.Create(), CurrentTenant.Id, me, input.PeriodType!.Value, input.PeriodFrom!.Value, input.PeriodTo!.Value);
        report.SetSummary(input.Summary);

        var teams = await ReadTeamsAsync(me);
        if (teams.Count == 0)
        {
            throw new BusinessException(HrErrorCodes.ReviewReports.NoTeams);
        }

        var order = 1;
        foreach (var team in teams)
        {
            report.Teams.Add(new ReviewReportTeam(GuidGenerator.Create(), CurrentTenant.Id, report.Id, team.ManagerEmployeeId, team.DepartmentId, team.MembersCount, order++));
        }

        await _reports.InsertAsync(report, autoSave: true);
        return await ToDtoAsync(report);
    }

    [Authorize(HrPermissions.ReviewReportsPermissions.Read)]
    public virtual async Task<ReviewReportDto> GetAsync(Guid id)
    {
        var report = await LoadAsync(id);
        if (report.ReviewerUserId == CurrentUser.GetId())
        {
            return await ToDtoAsync(report);
        }

        // Anyone else reading it is the executive director, and only once it is submitted.
        if (!report.IsSubmitted || await IsAuthorAsync())
        {
            throw new EntityNotFoundException(typeof(ReviewReport), id);
        }

        if (report.ViewedAt == null)
        {
            report.MarkViewed(Clock.Now);
            await _reports.UpdateAsync(report, autoSave: true);
        }

        return await ToDtoAsync(report);
    }

    [Authorize(HrPermissions.ReviewReportsPermissions.Manage)]
    public virtual async Task<ReviewReportDto> UpdateAsync(Guid id, ReviewReportUpdateDto input)
    {
        var report = await LoadOwnDraftAsync(id);
        report.ConcurrencyStamp = input.ConcurrencyStamp;
        report.SetPeriod(input.PeriodType!.Value, input.PeriodFrom!.Value, input.PeriodTo!.Value);
        report.SetSummary(input.Summary);

        foreach (var change in input.Teams ?? new List<ReviewReportTeamInputDto>())
        {
            var team = report.Teams.FirstOrDefault(x => x.ManagerEmployeeId == change.ManagerEmployeeId)
                       ?? throw new BusinessException(HrErrorCodes.ReviewReports.UnknownTeam);
            team.Assess(change.CompletionPercent, change.Notes, change.ProjectNotes);
        }

        await _reports.UpdateAsync(report, autoSave: true);
        return await ToDtoAsync(report);
    }

    [Authorize(HrPermissions.ReviewReportsPermissions.Manage)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var report = await LoadOwnDraftAsync(id);
        await _reports.DeleteAsync(report, autoSave: true);
    }

    [Authorize(HrPermissions.ReviewReportsPermissions.Manage)]
    public virtual async Task<ReviewReportDto> SubmitAsync(Guid id, ReviewReportSubmitDto input)
    {
        var report = await LoadOwnDraftAsync(id);
        if (report.Teams.Any(x => x.CompletionPercent == null))
        {
            throw new BusinessException(HrErrorCodes.ReviewReports.IncompleteTeams);
        }

        report.ConcurrencyStamp = input.ConcurrencyStamp;
        report.Submit(Clock.Now);
        await _reports.UpdateAsync(report, autoSave: true);

        foreach (var executive in (await _userManager.GetUsersInRoleAsync(HrRoles.ExecutiveDirector)).Where(x => x.IsActive))
        {
            await _notifications.PublishAsync(executive.Id, NotificationTemplateCode.ReviewReportSubmitted, NotificationResourceType.ReviewReport, report.Id);
        }

        return await ToDtoAsync(report);
    }

    /// <summary>The reviewer writes reports; the executive director only reads them. The two roles never go together.</summary>
    private Task<bool> IsAuthorAsync() => AuthorizationService.IsGrantedAsync(HrPermissions.ReviewReportsPermissions.Manage);

    private async Task<ReviewReport> LoadAsync(Guid id) =>
        await AsyncExecuter.FirstOrDefaultAsync((await _reports.WithDetailsAsync(x => x.Teams)).Where(x => x.Id == id))
        ?? throw new EntityNotFoundException(typeof(ReviewReport), id);

    private async Task<ReviewReport> LoadOwnDraftAsync(Guid id)
    {
        var report = await LoadAsync(id);
        if (report.ReviewerUserId != CurrentUser.GetId())
        {
            throw new EntityNotFoundException(typeof(ReviewReport), id);
        }

        if (report.IsSubmitted)
        {
            throw new BusinessException(HrErrorCodes.ReviewReports.AlreadySubmitted);
        }

        return report;
    }

    private sealed record TeamSeed(Guid ManagerEmployeeId, Guid DepartmentId, int MembersCount);

    /// <summary>Every active department manager with an active file is one team. The reviewer never assesses their own team.</summary>
    private async Task<List<TeamSeed>> ReadTeamsAsync(Guid reviewerUserId)
    {
        var managerUserIds = (await _userManager.GetUsersInRoleAsync(HrRoles.DepartmentManager))
            .Where(x => x.IsActive && x.Id != reviewerUserId)
            .Select(x => x.Id)
            .ToList();
        if (managerUserIds.Count == 0)
        {
            return new List<TeamSeed>();
        }

        var managers = await _employees.GetListAsync(x => x.UserId != null && managerUserIds.Contains(x.UserId.Value) && x.Status == EmployeeStatus.Active);
        var managerIds = managers.Select(x => x.Id).ToList();

        var counts = (await AsyncExecuter.ToListAsync(
                (await _employees.GetQueryableAsync())
                .Where(x => x.ManagerEmployeeId != null && managerIds.Contains(x.ManagerEmployeeId.Value) && x.Status != EmployeeStatus.Ended)
                .GroupBy(x => x.ManagerEmployeeId!.Value)
                .Select(g => new { ManagerId = g.Key, Count = g.Count() })))
            .ToDictionary(x => x.ManagerId, x => x.Count);

        return managers
            .OrderBy(x => x.FullNameAr)
            .ThenBy(x => x.Id)
            .Select(x => new TeamSeed(x.Id, x.DepartmentId, counts.GetValueOrDefault(x.Id)))
            .ToList();
    }

    private static int? Average(IEnumerable<ReviewReportTeam> teams)
    {
        var values = teams.Where(x => x.CompletionPercent != null).Select(x => x.CompletionPercent!.Value).ToList();
        return values.Count == 0 ? null : (int)Math.Round(values.Average(), MidpointRounding.AwayFromZero);
    }

    private async Task<ReviewReportDto> ToDtoAsync(ReviewReport report)
    {
        var teams = report.Teams.OrderBy(x => x.SortOrder).ToList();
        var managers = await _references.GetRefsAsync(teams.Select(x => x.ManagerEmployeeId));
        var departments = await _references.GetDepartmentsAsync(teams.Where(x => x.DepartmentId != null).Select(x => x.DepartmentId!.Value));
        var reviewers = await _references.GetUserRefsAsync(new[] { report.ReviewerUserId });

        return new ReviewReportDto
        {
            Id = report.Id,
            Reviewer = reviewers[report.ReviewerUserId],
            PeriodType = report.PeriodType,
            PeriodFrom = report.PeriodFrom,
            PeriodTo = report.PeriodTo,
            Status = report.Status,
            Summary = report.Summary,
            AverageCompletion = Average(teams),
            Teams = teams
                .Where(x => managers.ContainsKey(x.ManagerEmployeeId))
                .Select(x => new ReviewReportTeamDto
                {
                    Manager = managers[x.ManagerEmployeeId],
                    Department = x.DepartmentId == null ? null : departments.GetValueOrDefault(x.DepartmentId.Value),
                    MembersCount = x.MembersCount,
                    CompletionPercent = x.CompletionPercent,
                    Notes = x.Notes,
                    ProjectNotes = x.ProjectNotes
                }).ToList(),
            SubmittedAt = report.SubmittedAt,
            ViewedAt = report.ViewedAt,
            ConcurrencyStamp = report.ConcurrencyStamp
        };
    }
}

public class ReviewReportListInputValidator : AbstractValidator<ReviewReportListInput>
{
    public ReviewReportListInputValidator()
    {
        RuleFor(x => x.PeriodType).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
    }
}

public class ReviewReportCreateDtoValidator : AbstractValidator<ReviewReportCreateDto>
{
    public ReviewReportCreateDtoValidator()
    {
        RuleFor(x => x.PeriodType).NotNull().IsInEnum();
        RuleFor(x => x.PeriodFrom).NotNull();
        RuleFor(x => x.PeriodTo).NotNull();
        RuleFor(x => x.Summary).MaximumLength(ReviewReportConsts.MaxSummaryLength);
    }
}

public class ReviewReportTeamInputDtoValidator : AbstractValidator<ReviewReportTeamInputDto>
{
    public ReviewReportTeamInputDtoValidator()
    {
        RuleFor(x => x.ManagerEmployeeId).NotNull();
        RuleFor(x => x.CompletionPercent).InclusiveBetween(ReviewReportConsts.MinCompletionPercent, ReviewReportConsts.MaxCompletionPercent);
        RuleFor(x => x.Notes).MaximumLength(ReviewReportConsts.MaxNotesLength);
        RuleFor(x => x.ProjectNotes).MaximumLength(ReviewReportConsts.MaxNotesLength);
    }
}

public class ReviewReportUpdateDtoValidator : AbstractValidator<ReviewReportUpdateDto>
{
    public ReviewReportUpdateDtoValidator()
    {
        RuleFor(x => x.PeriodType).NotNull().IsInEnum();
        RuleFor(x => x.PeriodFrom).NotNull();
        RuleFor(x => x.PeriodTo).NotNull();
        RuleFor(x => x.Summary).MaximumLength(ReviewReportConsts.MaxSummaryLength);
        RuleFor(x => x.Teams).NotNull();
        RuleFor(x => x.Teams)
            .Must(teams => teams!.Count <= ReviewReportConsts.MaxTeamsPerUpdate)
            .When(x => x.Teams != null)
            .WithMessage($"At most {ReviewReportConsts.MaxTeamsPerUpdate} teams.");
        RuleFor(x => x.Teams)
            .Must(teams => teams!.Select(t => t.ManagerEmployeeId).Distinct().Count() == teams!.Count)
            .When(x => x.Teams != null)
            .WithMessage("Each team may appear once.");
        RuleForEach(x => x.Teams).SetValidator(new ReviewReportTeamInputDtoValidator());
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public class ReviewReportSubmitDtoValidator : AbstractValidator<ReviewReportSubmitDto>
{
    public ReviewReportSubmitDtoValidator()
    {
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}
