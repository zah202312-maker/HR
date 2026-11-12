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
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace HR_Mangment2.ReviewReports;

/// <summary>
/// The reviewer's daily report (decision 2026-11-11, replacing the period follow-up reports). One report per reviewer
/// per day, a full text, and one row per room graded for backend and frontend work with notes. The reviewer sees only
/// their own reports; the executive director sees submitted ones only, and the first look is recorded. A report never
/// shows financial or complaint data.
/// </summary>
[Authorize]
public class ReviewReportAppService : HR_Mangment2AppService, IReviewReportAppService
{
    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["reportDate"] = nameof(ReviewReport.ReportDate),
        ["submittedAt"] = nameof(ReviewReport.SubmittedAt),
        ["creationTime"] = nameof(ReviewReport.CreationTime)
    };

    private readonly IRepository<ReviewReport, Guid> _reports;
    private readonly IRepository<ReviewRoom, Guid> _rooms;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IdentityUserManager _userManager;
    private readonly EmployeeReferenceReader _references;
    private readonly HrNotificationPublisher _notifications;
    private readonly ReviewerAccess _access;
    private readonly ICompanyClock _companyClock;

    public ReviewReportAppService(
        IRepository<ReviewReport, Guid> reports,
        IRepository<ReviewRoom, Guid> rooms,
        IRepository<Employee, Guid> employees,
        IdentityUserManager userManager,
        EmployeeReferenceReader references,
        HrNotificationPublisher notifications,
        ReviewerAccess access,
        ICompanyClock companyClock)
    {
        _reports = reports;
        _rooms = rooms;
        _employees = employees;
        _userManager = userManager;
        _references = references;
        _notifications = notifications;
        _access = access;
        _companyClock = companyClock;
    }

    /// <summary>
    /// The reviewer and their stand-in see the reviewer's reports, drafts included; the executive director sees the
    /// submitted ones.
    /// </summary>
    [Authorize]
    public virtual async Task<ReviewReportSummaryPagedResultDto> GetListAsync(ReviewReportListInput input)
    {
        var query = (await _reports.WithDetailsAsync(x => x.Teams))
            .WhereIf(input.DateFrom != null, x => x.ReportDate >= input.DateFrom)
            .WhereIf(input.DateTo != null, x => x.ReportDate <= input.DateTo);

        var reviewers = await _access.ReviewersICanActForAsync();
        if (reviewers.Count > 0)
        {
            query = query
                .Where(x => reviewers.Contains(x.ReviewerUserId))
                .WhereIf(input.Status != null, x => x.Status == input.Status);
        }
        else
        {
            await EnsureGrantedAsync(HrPermissions.ReviewReportsPermissions.Read);

            // The executive director never sees a draft.
            query = query.Where(x => x.Status == ReviewReportStatus.Submitted);
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(ReviewReport.ReportDate) + " desc", SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));
        var users = await _references.GetUserRefsAsync(rows.Select(x => x.ReviewerUserId).Concat(rows.Select(PreparedByOf)));

        return new ReviewReportSummaryPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => new ReviewReportSummaryDto
            {
                Id = x.Id,
                Reviewer = users[x.ReviewerUserId],
                PreparedBy = users[PreparedByOf(x)],
                ReportDate = x.ReportDate,
                Status = x.Status,
                TeamsCount = x.Teams.Count,
                SubmittedAt = x.SubmittedAt,
                ViewedAt = x.ViewedAt
            }).ToList()
        };
    }

    /// <summary>The reviewer writes their own; a stand-in writes for the reviewer named in OnBehalfOfUserId.</summary>
    [Authorize]
    public virtual async Task<ReviewReportDto> CreateAsync(ReviewReportCreateDto input)
    {
        var reviewer = await _access.ResolveReviewerAsync(input.OnBehalfOfUserId);
        var date = input.ReportDate!.Value;
        await EnsureDateIsFreeAsync(reviewer, date, null);

        var rooms = await _rooms.GetListAsync(x => x.ReviewerUserId == reviewer);
        if (rooms.Count == 0)
        {
            throw new BusinessException(HrErrorCodes.ReviewReports.NoRooms);
        }

        var report = new ReviewReport(GuidGenerator.Create(), CurrentTenant.Id, reviewer, date);
        report.SetContent(input.Content);

        var managerIds = rooms.Select(x => x.ManagerEmployeeId).Distinct().ToList();
        var managers = (await _employees.GetListAsync(x => managerIds.Contains(x.Id))).ToDictionary(x => x.Id);

        // Rows follow the room lead's name, then the room id for a stable order.
        var order = 1;
        foreach (var room in rooms
                     .Where(x => managers.ContainsKey(x.ManagerEmployeeId))
                     .OrderBy(x => managers[x.ManagerEmployeeId].FullNameAr)
                     .ThenBy(x => x.Id))
        {
            report.Teams.Add(new ReviewReportTeam(
                GuidGenerator.Create(),
                CurrentTenant.Id,
                report.Id,
                room.Id,
                room.Name,
                room.ManagerEmployeeId,
                managers[room.ManagerEmployeeId].DepartmentId,
                room.MemberEmployeeIds.ToList(),
                order++));
        }

        await _reports.InsertAsync(report, autoSave: true);
        return await ToDtoAsync(report);
    }

    [Authorize]
    public virtual async Task<ReviewReportDto> GetAsync(Guid id)
    {
        var report = await LoadAsync(id);
        if ((await _access.ReviewersICanActForAsync()).Contains(report.ReviewerUserId))
        {
            return await ToDtoAsync(report);
        }

        // Anyone else reading it is the executive director, and only once it is submitted.
        await EnsureGrantedAsync(HrPermissions.ReviewReportsPermissions.Read);
        if (!report.IsSubmitted || await _access.IsAuthorAsync())
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

    [Authorize]
    public virtual async Task<ReviewReportDto> UpdateAsync(Guid id, ReviewReportUpdateDto input)
    {
        var report = await LoadOwnDraftAsync(id);
        report.ConcurrencyStamp = input.ConcurrencyStamp;

        var date = input.ReportDate!.Value;
        if (date != report.ReportDate)
        {
            await EnsureDateIsFreeAsync(report.ReviewerUserId, date, report.Id);
            report.SetReportDate(date);
        }

        report.SetContent(input.Content);

        foreach (var change in input.Teams ?? new List<ReviewReportTeamInputDto>())
        {
            var team = report.Teams.FirstOrDefault(x => x.RoomId == change.RoomId)
                       ?? throw new BusinessException(HrErrorCodes.ReviewReports.UnknownTeam);
            team.Assess(change.BackendGrade, change.BackendNotes, change.FrontendGrade, change.FrontendNotes, change.Notes);
        }

        await _reports.UpdateAsync(report, autoSave: true);
        return await ToDtoAsync(report);
    }

    [Authorize]
    public virtual async Task DeleteAsync(Guid id)
    {
        var report = await LoadOwnDraftAsync(id);
        await _reports.DeleteAsync(report, autoSave: true);
    }

    [Authorize]
    public virtual async Task<ReviewReportDto> SubmitAsync(Guid id, ReviewReportSubmitDto input)
    {
        var report = await LoadOwnDraftAsync(id);
        if (report.Content == null)
        {
            throw new BusinessException(HrErrorCodes.ReviewReports.ContentRequired);
        }

        if (report.Teams.Any(x => !x.IsGraded))
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

    /// <summary>Not after today in the company's time zone, and one report per reviewer per day.</summary>
    private async Task EnsureDateIsFreeAsync(Guid reviewer, DateOnly date, Guid? exceptReportId)
    {
        if (date > await _companyClock.GetTodayAsync())
        {
            throw new BusinessException(HrErrorCodes.ReviewReports.InvalidDate);
        }

        if (await _reports.AnyAsync(x => x.ReviewerUserId == reviewer && x.ReportDate == date && x.Id != exceptReportId))
        {
            throw new BusinessException(HrErrorCodes.ReviewReports.DateAlreadyReported);
        }
    }

    private async Task EnsureGrantedAsync(string permission)
    {
        if (!await AuthorizationService.IsGrantedAsync(permission))
        {
            throw new AbpAuthorizationException();
        }
    }

    /// <summary>Who created the report: the reviewer or the stand-in (FullAudited keeps the creator).</summary>
    private static Guid PreparedByOf(ReviewReport report) => report.CreatorId ?? report.ReviewerUserId;

    private async Task<ReviewReport> LoadAsync(Guid id) =>
        await AsyncExecuter.FirstOrDefaultAsync((await _reports.WithDetailsAsync(x => x.Teams)).Where(x => x.Id == id))
        ?? throw new EntityNotFoundException(typeof(ReviewReport), id);

    /// <summary>A draft of the caller's own (as reviewer) or of a reviewer who delegated to the caller.</summary>
    private async Task<ReviewReport> LoadOwnDraftAsync(Guid id)
    {
        var reviewers = await _access.ReviewersICanActForAsync();
        if (reviewers.Count == 0)
        {
            throw new AbpAuthorizationException();
        }

        var report = await LoadAsync(id);
        if (!reviewers.Contains(report.ReviewerUserId))
        {
            throw new EntityNotFoundException(typeof(ReviewReport), id);
        }

        if (report.IsSubmitted)
        {
            throw new BusinessException(HrErrorCodes.ReviewReports.AlreadySubmitted);
        }

        return report;
    }

    private async Task<ReviewReportDto> ToDtoAsync(ReviewReport report)
    {
        var teams = report.Teams.OrderBy(x => x.SortOrder).ToList();
        var people = await _references.GetRefsAsync(teams.Select(x => x.ManagerEmployeeId).Concat(teams.SelectMany(x => x.MemberEmployeeIds)));
        var departments = await _references.GetDepartmentsAsync(teams.Where(x => x.DepartmentId != null).Select(x => x.DepartmentId!.Value));
        var users = await _references.GetUserRefsAsync(new[] { report.ReviewerUserId, PreparedByOf(report) });

        return new ReviewReportDto
        {
            Id = report.Id,
            Reviewer = users[report.ReviewerUserId],
            PreparedBy = users[PreparedByOf(report)],
            ReportDate = report.ReportDate,
            Status = report.Status,
            Content = report.Content,
            // Rows follow the room lead's name as the reader sees it (decision 2026-11-11).
            Teams = teams
                .Where(x => people.ContainsKey(x.ManagerEmployeeId))
                .OrderBy(x => people[x.ManagerEmployeeId].FullName, StringComparer.CurrentCulture)
                .ThenBy(x => x.SortOrder)
                .Select(x => new ReviewReportTeamDto
                {
                    Room = new ReviewRoomRefDto { Id = x.RoomId, Name = x.RoomName },
                    Manager = people[x.ManagerEmployeeId],
                    Department = x.DepartmentId == null ? null : departments.GetValueOrDefault(x.DepartmentId.Value),
                    Members = x.MemberEmployeeIds
                        .Where(people.ContainsKey)
                        .Select(m => people[m])
                        .OrderBy(m => m.FullName, StringComparer.CurrentCulture)
                        .ToList(),
                    BackendGrade = x.BackendGrade,
                    BackendNotes = x.BackendNotes,
                    FrontendGrade = x.FrontendGrade,
                    FrontendNotes = x.FrontendNotes,
                    Notes = x.Notes
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
        RuleFor(x => x.Status).IsInEnum();
    }
}

public class ReviewReportCreateDtoValidator : AbstractValidator<ReviewReportCreateDto>
{
    public ReviewReportCreateDtoValidator()
    {
        RuleFor(x => x.ReportDate).NotNull();
        RuleFor(x => x.Content).MaximumLength(ReviewReportConsts.MaxContentLength);
    }
}

public class ReviewReportTeamInputDtoValidator : AbstractValidator<ReviewReportTeamInputDto>
{
    public ReviewReportTeamInputDtoValidator()
    {
        RuleFor(x => x.RoomId).NotNull();
        RuleFor(x => x.BackendGrade).IsInEnum();
        RuleFor(x => x.FrontendGrade).IsInEnum();
        RuleFor(x => x.BackendNotes).MaximumLength(ReviewReportConsts.MaxNotesLength);
        RuleFor(x => x.FrontendNotes).MaximumLength(ReviewReportConsts.MaxNotesLength);
        RuleFor(x => x.Notes).MaximumLength(ReviewReportConsts.MaxNotesLength);
    }
}

public class ReviewReportUpdateDtoValidator : AbstractValidator<ReviewReportUpdateDto>
{
    public ReviewReportUpdateDtoValidator()
    {
        RuleFor(x => x.ReportDate).NotNull();
        RuleFor(x => x.Content).MaximumLength(ReviewReportConsts.MaxContentLength);
        RuleFor(x => x.Teams).NotNull();
        RuleFor(x => x.Teams)
            .Must(teams => teams!.Count <= ReviewReportConsts.MaxTeamsPerUpdate)
            .When(x => x.Teams != null)
            .WithMessage($"At most {ReviewReportConsts.MaxTeamsPerUpdate} rooms.");
        RuleFor(x => x.Teams)
            .Must(teams => teams!.Select(t => t.RoomId).Distinct().Count() == teams!.Count)
            .When(x => x.Teams != null)
            .WithMessage("Each room may appear once.");
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
