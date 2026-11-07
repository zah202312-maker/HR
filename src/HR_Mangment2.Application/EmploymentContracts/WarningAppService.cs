using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Common;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Files;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Identity;
using HR_Mangment2.Leaves;
using HR_Mangment2.Notifications;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace HR_Mangment2.Warnings;

[Authorize]
public class WarningAppService : HR_Mangment2AppService, IWarningAppService
{
    private const string CreateOperation = "Warning_Create";

    private static readonly WarningStatus[] ManagerFileStatuses = { WarningStatus.Issued, WarningStatus.Objected, WarningStatus.ObjectionUpheld };

    private static readonly Dictionary<string, string> Sort = new()
    {
        ["incidentDate"] = nameof(Warning.IncidentDate),
        ["status"] = nameof(Warning.Status),
        ["creationTime"] = nameof(Warning.CreationTime),
        ["decidedAt"] = nameof(Warning.DecidedAt)
    };

    private readonly IRepository<Warning, Guid> _warnings;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IIdentityUserRepository _users;
    private readonly IdentityUserManager _userManager;
    private readonly EmployeeReferenceReader _references;
    private readonly ICompanyClock _companyClock;
    private readonly WorkCalendarService _calendar;
    private readonly HrNotificationPublisher _notifications;
    private readonly AdministrativeTaskLogger _taskLog;
    private readonly IdempotencyStore _idempotency;
    private readonly UploadedFileGuard _fileGuard;
    private readonly IBlobContainer<HrFilesContainer> _files;
    private readonly FileDownloadLinkManager _links;

    public WarningAppService(
        IRepository<Warning, Guid> warnings,
        IRepository<Employee, Guid> employees,
        IIdentityUserRepository users,
        IdentityUserManager userManager,
        EmployeeReferenceReader references,
        ICompanyClock companyClock,
        WorkCalendarService calendar,
        HrNotificationPublisher notifications,
        AdministrativeTaskLogger taskLog,
        IdempotencyStore idempotency,
        UploadedFileGuard fileGuard,
        IBlobContainer<HrFilesContainer> files,
        FileDownloadLinkManager links)
    {
        _warnings = warnings;
        _employees = employees;
        _users = users;
        _userManager = userManager;
        _references = references;
        _companyClock = companyClock;
        _calendar = calendar;
        _notifications = notifications;
        _taskLog = taskLog;
        _idempotency = idempotency;
        _fileGuard = fileGuard;
        _files = files;
        _links = links;
    }

    [Authorize(HrPermissions.WarningPermissions.Recommend)]
    public virtual async Task<WarningRecommendationDto> CreateAsync(WarningCreateInput input, Guid? idempotencyKey)
    {
        var fingerprint = new
        {
            input.EmployeeId,
            input.RecommendedLevel,
            input.IncidentDate,
            input.Description,
            Files = input.Attachments.Select(x => new { x.FileName, x.ContentType, x.Content.Length }).ToList()
        };

        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, fingerprint);
        if (earlier != null)
        {
            return await ToRecommendationDtoAsync(await _warnings.GetAsync(earlier.Value, includeDetails: false));
        }

        var myUserId = CurrentUser.GetId();
        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == myUserId);
        var target = await _employees.FindAsync(input.EmployeeId!.Value);
        if (target == null || target.Status == EmployeeStatus.Ended)
        {
            throw new BusinessException(HrErrorCodes.Warning.EmployeeNotFound);
        }

        if (target.UserId == myUserId)
        {
            throw new BusinessException(HrErrorCodes.Warning.SelfRecommendationNotAllowed);
        }

        var role = await ResolveRecommenderRoleAsync(myUserId, me, target);

        if (input.IncidentDate!.Value > await _companyClock.GetTodayAsync())
        {
            throw new BusinessException(HrErrorCodes.Warning.IncidentDateInFuture);
        }

        foreach (var file in input.Attachments)
        {
            await _fileGuard.EnsureAcceptedAsync(file.Content, file.ContentType, HrErrorCodes.Warning.AttachmentRejected);
        }

        var warning = new Warning(GuidGenerator.Create(), CurrentTenant.Id, target.Id, input.RecommendedLevel!.Value, input.IncidentDate.Value, input.Description.Trim(), myUserId, role);
        foreach (var file in input.Attachments)
        {
            var attachmentId = GuidGenerator.Create();
            var blobName = attachmentId.ToString("N");
            await _files.SaveAsync(blobName, file.Content);
            warning.Attachments.Add(new WarningAttachment(attachmentId, CurrentTenant.Id, warning.Id, Path.GetFileName(file.FileName), file.ContentType, file.Content.LongLength, blobName, Clock.Now));
        }

        await _warnings.InsertAsync(warning, autoSave: true);
        await _taskLog.LogAsync(myUserId, AdministrativeTaskType.WarningRecommended, warning.Id);
        await NotifyHrAsync(NotificationTemplateCode.WarningRecommended, warning.Id);
        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, fingerprint, warning.Id);

        return await ToRecommendationDtoAsync(warning);
    }

    [Authorize(HrPermissions.WarningPermissions.ReadAll)]
    public virtual async Task<WarningPagedResultDto> GetListAsync(WarningListInput input)
    {
        var query = (await _warnings.WithDetailsAsync(x => x.Attachments))
            .WhereIf(input.EmployeeId != null, x => x.EmployeeId == input.EmployeeId)
            .WhereIf(input.Status != null, x => x.Status == input.Status)
            .WhereIf(input.Level != null, x => (x.IssuedLevel ?? x.RecommendedLevel) == input.Level)
            .WhereIf(input.IncidentDateFrom != null, x => x.IncidentDate >= input.IncidentDateFrom)
            .WhereIf(input.IncidentDateTo != null, x => x.IncidentDate <= input.IncidentDateTo);

        if (input.DepartmentId != null)
        {
            var employeeIds = (await _employees.GetQueryableAsync()).Where(x => x.DepartmentId == input.DepartmentId).Select(x => x.Id);
            query = query.Where(x => employeeIds.Contains(x.EmployeeId));
        }

        return await PageAsync(query, input);
    }

    [Authorize(HrPermissions.WarningPermissions.ResolveObjection)]
    public virtual async Task<WarningPagedResultDto> GetObjectionsAsync(WarningObjectionListInput input)
    {
        var query = (await _warnings.WithDetailsAsync(x => x.Attachments))
            .Where(x => x.ObjectionSubmittedAt != null)
            .WhereIf(input.IsPending == true, x => x.Status == WarningStatus.Objected)
            .WhereIf(input.IsPending == false, x => x.Status != WarningStatus.Objected);

        return await PageAsync(query, input);
    }

    [Authorize(HrPermissions.WarningPermissions.Recommend)]
    public virtual async Task<WarningRecommendationPagedResultDto> GetRecommendedByMeAsync(WarningRecommendedListInput input)
    {
        var me = CurrentUser.GetId();
        var query = (await _warnings.GetQueryableAsync())
            .Where(x => x.RecommendedByUserId == me)
            .WhereIf(input.Stage == WarningRecommendationStage.UnderReview, x => x.DecisionOutcome == null)
            .WhereIf(input.Stage == WarningRecommendationStage.Issued, x => x.DecisionOutcome == WarningDecisionOutcome.Issue)
            .WhereIf(input.Stage == WarningRecommendationStage.NotIssued, x => x.DecisionOutcome == WarningDecisionOutcome.Dismiss);

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query
            .ApplySorting(input.Sorting, nameof(Warning.CreationTime) + " desc", Sort)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount));
        var employees = await _references.GetRefsAsync(rows.Select(x => x.EmployeeId));

        return new WarningRecommendationPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => ToRecommendationDto(x, employees[x.EmployeeId])).ToList()
        };
    }

    [Authorize(HrPermissions.WarningPermissions.ReadOwn)]
    public virtual async Task<WarningEmployeePagedResultDto> GetMyListAsync(PagedAndSortedResultRequestDto input)
    {
        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == CurrentUser.GetId());
        if (me == null)
        {
            return new WarningEmployeePagedResultDto();
        }

        var query = (await _warnings.GetQueryableAsync()).Where(x => x.EmployeeId == me.Id && x.NotifiedAt != null);
        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query
            .ApplySorting(input.Sorting, nameof(Warning.CreationTime) + " desc", Sort)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount));
        var today = await _companyClock.GetTodayAsync();

        return new WarningEmployeePagedResultDto { TotalCount = total, Items = rows.Select(x => ToEmployeeDto(x, today)).ToList() };
    }

    [Authorize(HrPermissions.WarningPermissions.ReadOwn)]
    public virtual async Task<WarningEmployeeDto> GetMyAsync(Guid id) =>
        ToEmployeeDto(await LoadOwnNotifiedAsync(id), await _companyClock.GetTodayAsync());

    [Authorize(HrPermissions.WarningPermissions.ReadEmployeeFile)]
    public virtual async Task<WarningFileEntryPagedResultDto> GetEmployeeFileAsync(Guid employeeId, PagedAndSortedResultRequestDto input)
    {
        var employee = await _employees.FindAsync(employeeId) ?? throw new EntityNotFoundException(typeof(Employee), employeeId);
        var query = (await _warnings.GetQueryableAsync()).Where(x => x.EmployeeId == employeeId && x.DecidedAt != null);

        if (!CurrentUser.IsInRole(HrRoles.HrManager))
        {
            var me = await _employees.FirstOrDefaultAsync(x => x.UserId == CurrentUser.GetId());
            if (me == null || employee.ManagerEmployeeId != me.Id)
            {
                throw new EntityNotFoundException(typeof(Employee), employeeId);
            }

            query = query.Where(x => ManagerFileStatuses.Contains(x.Status));
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query
            .ApplySorting(input.Sorting, nameof(Warning.DecidedAt) + " desc", Sort)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount));

        return new WarningFileEntryPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => new WarningFileEntryDto
            {
                Id = x.Id,
                Level = x.IssuedLevel ?? x.RecommendedLevel,
                IncidentDate = x.IncidentDate,
                Description = x.Description,
                Status = x.Status,
                DecisionNote = x.DecisionNote!,
                DecidedAt = x.DecidedAt!.Value,
                EmployeeResponse = ToResponseDto(x),
                ObjectionOutcome = x.ObjectionOutcome
            }).ToList()
        };
    }

    [Authorize(HrPermissions.WarningPermissions.Read)]
    public virtual async Task<WarningDto> GetAsync(Guid id) => (await ToDtosAsync(new[] { await LoadVisibleAsync(id) }))[0];

    [Authorize(HrPermissions.WarningPermissions.Read)]
    public virtual async Task<FileDownloadUrlDto> GetAttachmentDownloadUrlAsync(Guid id, Guid attachmentId)
    {
        var warning = await LoadVisibleAsync(id);
        var attachment = warning.Attachments.FirstOrDefault(x => x.Id == attachmentId)
                         ?? throw new EntityNotFoundException(typeof(WarningAttachment), attachmentId);

        var link = await _links.IssueAsync(attachment.Id, attachment.BlobName, attachment.FileName, attachment.ContentType);
        return new FileDownloadUrlDto { Url = link.Url, ExpiresAt = link.ExpiresAt };
    }

    [Authorize(HrPermissions.WarningPermissions.Notify)]
    public virtual async Task<WarningDto> NotifyAsync(Guid id)
    {
        var warning = await LoadAsync(id);
        var employee = await RefuseOwnCaseAsync(warning);

        if (warning.Status != WarningStatus.Recommended)
        {
            throw new BusinessException(HrErrorCodes.Warning.InvalidStatus);
        }

        var today = await _companyClock.GetTodayAsync();
        warning.Notify(Clock.Now, await _calendar.AddWorkingDaysAsync(today, Warning.ResponseWorkingDays) ?? today);
        await _warnings.UpdateAsync(warning, autoSave: true);

        if (employee.UserId != null)
        {
            await _notifications.PublishAsync(employee.UserId.Value, NotificationTemplateCode.WarningNotified, NotificationResourceType.Warning, warning.Id);
        }

        return (await ToDtosAsync(new[] { warning }))[0];
    }

    [Authorize(HrPermissions.WarningPermissions.Respond)]
    public virtual async Task<WarningEmployeeDto> RespondAsync(Guid id, WarningRespondInputDto input)
    {
        var warning = await LoadOwnNotifiedAsync(id);
        var today = await _companyClock.GetTodayAsync();

        if (warning.ResponseSubmittedAt != null)
        {
            throw new BusinessException(HrErrorCodes.Warning.ResponseAlreadySubmitted);
        }

        if (warning.Status == WarningStatus.ResponseExpired || (warning.Status == WarningStatus.Notified && today > warning.ResponseDueDate))
        {
            throw new BusinessException(HrErrorCodes.Warning.ResponseWindowExpired);
        }

        if (warning.Status != WarningStatus.Notified)
        {
            throw new BusinessException(HrErrorCodes.Warning.InvalidStatus);
        }

        warning.Respond(input.Content.Trim(), Clock.Now);
        await _warnings.UpdateAsync(warning, autoSave: true);
        await NotifyHrAsync(NotificationTemplateCode.WarningReadyForDecision, warning.Id);

        return ToEmployeeDto(warning, today);
    }

    [Authorize(HrPermissions.WarningPermissions.Decide)]
    public virtual async Task<WarningDto> DecideAsync(Guid id, WarningDecideInputDto input)
    {
        var warning = await LoadAsync(id);
        var employee = await RefuseOwnCaseAsync(warning);
        var outcome = input.Decision!.Value;

        // QUESTIONS.md item 52: HR may refuse a recommendation without telling the employee, with a recorded reason. See D35.
        var dismissedUnheard = warning.Status == WarningStatus.Recommended && outcome == WarningDecisionOutcome.Dismiss;

        if (warning.Status == WarningStatus.Notified)
        {
            throw new BusinessException(HrErrorCodes.Warning.ResponseWindowOpen);
        }

        if (warning.Status is not (WarningStatus.Responded or WarningStatus.ResponseExpired) && !dismissedUnheard)
        {
            throw new BusinessException(HrErrorCodes.Warning.InvalidStatus);
        }

        if (outcome == WarningDecisionOutcome.Issue && input.IssuedLevel == null)
        {
            throw new BusinessException(HrErrorCodes.Warning.LevelRequired);
        }

        var today = await _companyClock.GetTodayAsync();
        DateOnly? objectionDue = outcome == WarningDecisionOutcome.Issue
            ? await _calendar.AddWorkingDaysAsync(today, Warning.ObjectionWorkingDays) ?? today
            : null;

        warning.Decide(outcome, input.IssuedLevel, input.Note.Trim(), CurrentUser.GetId(), Clock.Now, objectionDue);
        await _warnings.UpdateAsync(warning, autoSave: true);

        if (!dismissedUnheard && employee.UserId != null)
        {
            await _notifications.PublishAsync(employee.UserId.Value, NotificationTemplateCode.WarningDecided, NotificationResourceType.Warning, warning.Id);
        }

        return (await ToDtosAsync(new[] { warning }))[0];
    }

    [Authorize(HrPermissions.WarningPermissions.Object)]
    public virtual async Task<WarningEmployeeDto> ObjectAsync(Guid id, WarningObjectInputDto input)
    {
        var warning = await LoadOwnNotifiedAsync(id);
        var today = await _companyClock.GetTodayAsync();

        if (warning.ObjectionSubmittedAt != null)
        {
            throw new BusinessException(HrErrorCodes.Warning.ObjectionAlreadySubmitted);
        }

        // QUESTIONS.md item 54 sets the window. The contract has no code for a closed window, so InvalidStatus carries it.
        if (warning.Status != WarningStatus.Issued || today > warning.ObjectionDueDate)
        {
            throw new BusinessException(HrErrorCodes.Warning.InvalidStatus);
        }

        warning.Object(input.Content.Trim(), Clock.Now);
        await _warnings.UpdateAsync(warning, autoSave: true);

        foreach (var user in (await _userManager.GetUsersInRoleAsync(HrRoles.ExecutiveDirector)).Where(x => x.IsActive))
        {
            await _notifications.PublishAsync(user.Id, NotificationTemplateCode.WarningObjectionSubmitted, NotificationResourceType.Warning, warning.Id);
        }

        return ToEmployeeDto(warning, today);
    }

    [Authorize(HrPermissions.WarningPermissions.ResolveObjection)]
    public virtual async Task<WarningDto> ResolveObjectionAsync(Guid id, WarningResolveObjectionInputDto input)
    {
        var warning = await LoadAsync(id);
        if (warning.ObjectionSubmittedAt == null)
        {
            throw new EntityNotFoundException(typeof(Warning), id);
        }

        if (warning.Status != WarningStatus.Objected)
        {
            throw new BusinessException(HrErrorCodes.Warning.InvalidStatus);
        }

        warning.ResolveObjection(input.Outcome!.Value, input.Note.Trim(), CurrentUser.GetId(), Clock.Now);
        await _warnings.UpdateAsync(warning, autoSave: true);

        var employee = await _employees.GetAsync(warning.EmployeeId);
        if (employee.UserId != null)
        {
            await _notifications.PublishAsync(employee.UserId.Value, NotificationTemplateCode.WarningObjectionResolved, NotificationResourceType.Warning, warning.Id);
        }

        await NotifyHrAsync(NotificationTemplateCode.WarningObjectionResolvedForHr, warning.Id);

        return (await ToDtosAsync(new[] { warning }))[0];
    }

    private async Task<WarningRecommenderRole> ResolveRecommenderRoleAsync(Guid myUserId, Employee? me, Employee target)
    {
        var targetIsManager = target.UserId != null && (await _users.GetRoleNamesAsync(target.UserId.Value)).Contains(HrRoles.DepartmentManager);
        var myRoles = await _users.GetRoleNamesAsync(myUserId);

        if (myRoles.Contains(HrRoles.Reviewer) && (targetIsManager || !myRoles.Contains(HrRoles.DepartmentManager)))
        {
            return targetIsManager
                ? WarningRecommenderRole.Reviewer
                : throw new BusinessException(HrErrorCodes.Warning.TargetNotDepartmentManager);
        }

        if (targetIsManager)
        {
            throw new BusinessException(HrErrorCodes.Warning.TargetIsDepartmentManager);
        }

        if (me == null || target.ManagerEmployeeId != me.Id)
        {
            throw new BusinessException(HrErrorCodes.Warning.TargetNotInTeam);
        }

        return WarningRecommenderRole.DepartmentManager;
    }

    private async Task<Employee> RefuseOwnCaseAsync(Warning warning)
    {
        var employee = await _employees.GetAsync(warning.EmployeeId);
        var me = CurrentUser.GetId();

        // QUESTIONS.md item 53 gives this case to the executive director, but the contract's roles do not reach them yet: see D34.
        if (employee.UserId == me || warning.RecommendedByUserId == me)
        {
            throw new BusinessException(HrErrorCodes.Warning.SelfDecisionNotAllowed);
        }

        return employee;
    }

    private async Task NotifyHrAsync(NotificationTemplateCode template, Guid warningId)
    {
        foreach (var user in (await _userManager.GetUsersInRoleAsync(HrRoles.HrManager)).Where(x => x.IsActive))
        {
            await _notifications.PublishAsync(user.Id, template, NotificationResourceType.Warning, warningId);
        }
    }

    private async Task<Warning> LoadAsync(Guid id) =>
        await AsyncExecuter.FirstOrDefaultAsync((await _warnings.WithDetailsAsync(x => x.Attachments)).Where(x => x.Id == id))
        ?? throw new EntityNotFoundException(typeof(Warning), id);

    /// <summary>The executive director reads only warnings that carry an objection. Everything else is a 404 to them.</summary>
    private async Task<Warning> LoadVisibleAsync(Guid id)
    {
        var warning = await LoadAsync(id);
        if (!CurrentUser.IsInRole(HrRoles.HrManager) && warning.ObjectionSubmittedAt == null)
        {
            throw new EntityNotFoundException(typeof(Warning), id);
        }

        return warning;
    }

    private async Task<Warning> LoadOwnNotifiedAsync(Guid id)
    {
        var warning = await _warnings.FindAsync(id, includeDetails: false);
        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == CurrentUser.GetId());
        if (warning == null || me == null || warning.EmployeeId != me.Id || warning.NotifiedAt == null)
        {
            throw new EntityNotFoundException(typeof(Warning), id);
        }

        return warning;
    }

    private async Task<WarningPagedResultDto> PageAsync(IQueryable<Warning> query, PagedAndSortedResultRequestDto input)
    {
        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query
            .ApplySorting(input.Sorting, nameof(Warning.CreationTime) + " desc", Sort)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount));

        return new WarningPagedResultDto { TotalCount = total, Items = await ToDtosAsync(rows) };
    }

    private async Task<List<WarningDto>> ToDtosAsync(IReadOnlyCollection<Warning> warnings)
    {
        var employees = await _references.GetRefsAsync(warnings.Select(x => x.EmployeeId));
        var users = await _references.GetUserRefsAsync(warnings
            .SelectMany(x => new Guid?[] { x.RecommendedByUserId, x.DecidedByUserId, x.ObjectionResolvedByUserId })
            .Where(x => x != null)
            .Select(x => x!.Value));

        return warnings.Select(x => new WarningDto
        {
            Id = x.Id,
            Employee = employees[x.EmployeeId],
            RecommendedLevel = x.RecommendedLevel,
            IncidentDate = x.IncidentDate,
            Description = x.Description,
            Attachments = x.Attachments.OrderBy(a => a.CreationTime).ThenBy(a => a.Id).Select(a => new AttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                SizeBytes = a.SizeBytes,
                CreationTime = a.CreationTime
            }).ToList(),
            RecommendedBy = users[x.RecommendedByUserId],
            RecommenderRole = x.RecommenderRole,
            Status = x.Status,
            NotifiedAt = x.NotifiedAt,
            ResponseDueDate = x.ResponseDueDate,
            EmployeeResponse = ToResponseDto(x),
            Decision = x.DecisionOutcome == null ? null : new WarningDecisionDto
            {
                Outcome = x.DecisionOutcome.Value,
                IssuedLevel = x.IssuedLevel,
                Note = x.DecisionNote!,
                DecidedBy = users[x.DecidedByUserId!.Value],
                DecidedAt = x.DecidedAt!.Value
            },
            Objection = x.ObjectionSubmittedAt == null ? null : new WarningObjectionDto
            {
                Content = x.ObjectionContent!,
                SubmittedAt = x.ObjectionSubmittedAt.Value,
                Outcome = x.ObjectionOutcome,
                ResolutionNote = x.ObjectionResolutionNote,
                ResolvedBy = x.ObjectionResolvedByUserId == null ? null : users[x.ObjectionResolvedByUserId.Value],
                ResolvedAt = x.ObjectionResolvedAt
            },
            CreationTime = x.CreationTime
        }).ToList();
    }

    private async Task<WarningRecommendationDto> ToRecommendationDtoAsync(Warning warning) =>
        ToRecommendationDto(warning, (await _references.GetRefsAsync(new[] { warning.EmployeeId }))[warning.EmployeeId]);

    private static WarningRecommendationDto ToRecommendationDto(Warning warning, EmployeeRefDto employee) => new()
    {
        Id = warning.Id,
        Employee = employee,
        RecommendedLevel = warning.RecommendedLevel,
        IncidentDate = warning.IncidentDate,
        Description = warning.Description,
        Stage = warning.DecisionOutcome switch
        {
            null => WarningRecommendationStage.UnderReview,
            WarningDecisionOutcome.Issue => WarningRecommendationStage.Issued,
            _ => WarningRecommendationStage.NotIssued
        },
        IssuedLevel = warning.IssuedLevel,
        DecidedAt = warning.DecidedAt,
        CreationTime = warning.CreationTime
    };

    private static WarningResponseDto? ToResponseDto(Warning warning) => warning.ResponseSubmittedAt == null
        ? null
        : new WarningResponseDto { Content = warning.ResponseContent!, SubmittedAt = warning.ResponseSubmittedAt.Value };

    private static WarningEmployeeDto ToEmployeeDto(Warning warning, DateOnly today) => new()
    {
        Id = warning.Id,
        Level = warning.IssuedLevel ?? warning.RecommendedLevel,
        IncidentDate = warning.IncidentDate,
        Description = warning.Description,
        Status = warning.Status,
        NotifiedAt = warning.NotifiedAt!.Value,
        ResponseDueDate = warning.ResponseDueDate!.Value,
        CanRespond = warning.Status == WarningStatus.Notified && today <= warning.ResponseDueDate,
        CanObject = warning.Status == WarningStatus.Issued && today <= warning.ObjectionDueDate,
        MyResponse = ToResponseDto(warning),
        DecisionNote = warning.DecisionNote,
        DecidedAt = warning.DecidedAt,
        MyObjection = warning.ObjectionSubmittedAt == null ? null : new WarningEmployeeObjectionDto
        {
            Content = warning.ObjectionContent!,
            SubmittedAt = warning.ObjectionSubmittedAt.Value,
            Outcome = warning.ObjectionOutcome,
            ResolutionNote = warning.ObjectionResolutionNote,
            ResolvedAt = warning.ObjectionResolvedAt
        }
    };
}

public class WarningCreateInputValidator : AbstractValidator<WarningCreateInput>
{
    public WarningCreateInputValidator()
    {
        RuleFor(x => x.EmployeeId).NotNull();
        RuleFor(x => x.RecommendedLevel).NotNull();
        RuleFor(x => x.IncidentDate).NotNull();
        RuleFor(x => x.Description).NotNull().Length(1, Warning.MaxDescriptionLength);
        RuleFor(x => x.Attachments).Must(x => x.Count <= Warning.MaxAttachments);
    }
}

public class WarningRespondInputDtoValidator : AbstractValidator<WarningRespondInputDto>
{
    public WarningRespondInputDtoValidator()
    {
        RuleFor(x => x.Content).NotNull().Length(1, Warning.MaxDescriptionLength);
    }
}

public class WarningObjectInputDtoValidator : AbstractValidator<WarningObjectInputDto>
{
    public WarningObjectInputDtoValidator()
    {
        RuleFor(x => x.Content).NotNull().Length(1, Warning.MaxDescriptionLength);
    }
}

public class WarningDecideInputDtoValidator : AbstractValidator<WarningDecideInputDto>
{
    public WarningDecideInputDtoValidator()
    {
        RuleFor(x => x.Decision).NotNull();
        RuleFor(x => x.Note).NotNull().Length(1, Warning.MaxNoteLength);
        RuleFor(x => x.IssuedLevel).Null().When(x => x.Decision == WarningDecisionOutcome.Dismiss);
    }
}

public class WarningResolveObjectionInputDtoValidator : AbstractValidator<WarningResolveObjectionInputDto>
{
    public WarningResolveObjectionInputDtoValidator()
    {
        RuleFor(x => x.Outcome).NotNull();
        RuleFor(x => x.Note).NotNull().Length(1, Warning.MaxNoteLength);
    }
}
