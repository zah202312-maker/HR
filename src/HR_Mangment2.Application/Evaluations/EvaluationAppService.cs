using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Attendance;
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
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using Volo.Abp.Validation;

namespace HR_Mangment2.Evaluations;

/// <summary>
/// One evaluation seen by three audiences, each with its own DTO. None of those DTOs has a place for the internal note,
/// which lives only behind <see cref="EvaluationInternalNoteAppService"/>.
/// </summary>
[Authorize]
public class EvaluationAppService : HR_Mangment2AppService, IEvaluationAppService
{
    private const string AddResponseOperation = "Evaluation_AddResponse";

    /// <summary>QUESTIONS.md item 41.</summary>
    private const int ObjectionWindowWorkingDays = 5;

    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["submittedAt"] = nameof(Evaluation.SubmittedAt),
        ["status"] = nameof(Evaluation.Status),
        ["objectionStatus"] = nameof(Evaluation.ObjectionStatus),
        ["creationTime"] = nameof(Evaluation.CreationTime)
    };

    private readonly IRepository<Evaluation, Guid> _evaluations;
    private readonly IRepository<EvaluationCycle, Guid> _cycles;
    private readonly IRepository<EvaluationInternalNote, Guid> _notes;
    private readonly IRepository<EvaluationInternalNoteDraft, Guid> _drafts;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly AttendanceSummaryReader _attendance;
    private readonly EmployeeReferenceReader _references;
    private readonly HrNotificationPublisher _notifications;
    private readonly AdministrativeTaskLogger _taskLog;
    private readonly IdempotencyStore _idempotency;
    private readonly IdentityUserManager _userManager;
    private readonly WorkCalendarService _calendar;
    private readonly ICompanyClock _companyClock;

    public EvaluationAppService(
        IRepository<Evaluation, Guid> evaluations,
        IRepository<EvaluationCycle, Guid> cycles,
        IRepository<EvaluationInternalNote, Guid> notes,
        IRepository<EvaluationInternalNoteDraft, Guid> drafts,
        IRepository<Employee, Guid> employees,
        AttendanceSummaryReader attendance,
        EmployeeReferenceReader references,
        HrNotificationPublisher notifications,
        AdministrativeTaskLogger taskLog,
        IdempotencyStore idempotency,
        IdentityUserManager userManager,
        WorkCalendarService calendar,
        ICompanyClock companyClock)
    {
        _evaluations = evaluations;
        _cycles = cycles;
        _notes = notes;
        _drafts = drafts;
        _employees = employees;
        _attendance = attendance;
        _references = references;
        _notifications = notifications;
        _taskLog = taskLog;
        _idempotency = idempotency;
        _userManager = userManager;
        _calendar = calendar;
        _companyClock = companyClock;
    }

    [Authorize(HrPermissions.EvaluationsPermissions.Manage)]
    public virtual async Task<EvaluationManagerSummaryPagedResultDto> GetTeamListAsync(EvaluationTeamListInput input)
    {
        var team = await GetTeamIdsAsync();
        var query = (await _evaluations.GetQueryableAsync())
            .Where(x => team.Contains(x.EmployeeId))
            .WhereIf(input.CycleId != null, x => x.CycleId == input.CycleId)
            .WhereIf(input.Status != null, x => x.Status == input.Status)
            .WhereIf(input.EmployeeId != null, x => x.EmployeeId == input.EmployeeId);

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(Evaluation.CreationTime) + " desc", SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));
        var cycles = await LoadCyclesAsync(rows.Select(x => x.CycleId));
        var employees = await _references.GetRefsAsync(rows.Select(x => x.EmployeeId));

        return new EvaluationManagerSummaryPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => new EvaluationManagerSummaryDto
            {
                Id = x.Id,
                CycleId = x.CycleId,
                Year = cycles[x.CycleId].Year,
                Month = cycles[x.CycleId].Month,
                Employee = employees[x.EmployeeId],
                Status = x.Status,
                ObjectionStatus = x.ObjectionStatus,
                SubmittedAt = x.SubmittedAt
            }).ToList()
        };
    }

    [Authorize(HrPermissions.EvaluationsPermissions.Manage)]
    public virtual async Task<EvaluationManagerDto> GetManagerFormAsync(Guid id) =>
        await ToManagerDtoAsync(await LoadForCurrentManagerAsync(id));

    [Authorize(HrPermissions.EvaluationsPermissions.Manage)]
    public virtual async Task<EvaluationManagerDto> SaveDraftAsync(Guid id, EvaluationSaveDraftDto input)
    {
        var evaluation = await LoadForCurrentManagerAsync(id);
        var cycle = await LoadCycleAsync(evaluation.CycleId);
        CheckEditable(evaluation, cycle);

        var seen = new HashSet<Guid>();
        foreach (var score in input.Scores)
        {
            if (!seen.Add(score.CriterionId!.Value) || cycle.Criteria.All(x => x.CriterionId != score.CriterionId))
            {
                throw new BusinessException(HrErrorCodes.Evaluations.InvalidCriterion);
            }

            if (cycle.Grades.All(x => x.Code != score.GradeCode))
            {
                throw new BusinessException(HrErrorCodes.Evaluations.InvalidGrade);
            }
        }

        evaluation.ConcurrencyStamp = input.ConcurrencyStamp;
        evaluation.Scores.Clear();
        foreach (var score in input.Scores)
        {
            evaluation.Scores.Add(new EvaluationScore(GuidGenerator.Create(), CurrentTenant.Id, evaluation.Id, score.CriterionId!.Value, score.GradeCode));
        }

        evaluation.SetEmployeeNotes(input.EmployeeNotes);
        await _evaluations.UpdateAsync(evaluation, autoSave: true);
        return await ToManagerDtoAsync(evaluation);
    }

    [Authorize(HrPermissions.EvaluationsPermissions.Manage)]
    public virtual async Task<EvaluationManagerDto> SubmitAsync(Guid id, EvaluationSubmitInputDto input)
    {
        var evaluation = await LoadForCurrentManagerAsync(id);
        var cycle = await LoadCycleAsync(evaluation.CycleId);
        CheckEditable(evaluation, cycle);

        if (cycle.Criteria.Any(c => evaluation.Scores.All(s => s.CriterionId != c.CriterionId)))
        {
            throw new BusinessException(HrErrorCodes.Evaluations.IncompleteScores);
        }

        var me = CurrentUser.GetId();
        evaluation.ConcurrencyStamp = input.ConcurrencyStamp;
        evaluation.Submit(me, Clock.Now);
        await _evaluations.UpdateAsync(evaluation, autoSave: true);

        // Only the submitter's own draft becomes fixed. A previous manager's draft does not travel with the form.
        var draft = await _drafts.FirstOrDefaultAsync(x => x.EvaluationId == id && x.AuthorUserId == me);
        if (draft != null)
        {
            await _notes.InsertAsync(new EvaluationInternalNote(GuidGenerator.Create(), CurrentTenant.Id, id, me, draft.Content), autoSave: true);
            await _drafts.DeleteAsync(draft, autoSave: true);
        }

        await _taskLog.LogAsync(me, AdministrativeTaskType.EvaluationSubmitted, id);

        var owner = await _employees.GetAsync(evaluation.EmployeeId);
        if (owner.UserId != null)
        {
            await _notifications.PublishAsync(owner.UserId.Value, NotificationTemplateCode.EvaluationPublished, NotificationResourceType.Evaluation, id);
        }

        return await ToManagerDtoAsync(evaluation);
    }

    [Authorize(HrPermissions.EvaluationsPermissions.ViewOwn)]
    public virtual async Task<EvaluationEmployeeSummaryPagedResultDto> GetMyListAsync(EvaluationMyListInput input)
    {
        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == CurrentUser.GetId());
        if (me == null)
        {
            return new EvaluationEmployeeSummaryPagedResultDto();
        }

        var query = (await _evaluations.GetQueryableAsync()).Where(x => x.EmployeeId == me.Id && x.Status != EvaluationStatus.Draft);
        if (input.Year != null)
        {
            var cycleIds = await AsyncExecuter.ToListAsync((await _cycles.GetQueryableAsync()).Where(x => x.Year == input.Year).Select(x => x.Id));
            query = query.Where(x => cycleIds.Contains(x.CycleId));
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(Evaluation.SubmittedAt) + " desc", SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));
        var cycles = await LoadCyclesAsync(rows.Select(x => x.CycleId));

        return new EvaluationEmployeeSummaryPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => new EvaluationEmployeeSummaryDto
            {
                Id = x.Id,
                Year = cycles[x.CycleId].Year,
                Month = cycles[x.CycleId].Month,
                Status = x.Status,
                ObjectionStatus = x.ObjectionStatus,
                SubmittedAt = x.SubmittedAt!.Value,
                AcknowledgedAt = x.AcknowledgedAt
            }).ToList()
        };
    }

    [Authorize(HrPermissions.EvaluationsPermissions.ViewOwn)]
    public virtual async Task<EvaluationEmployeeDto> GetMyDetailAsync(Guid id) => await ToEmployeeDtoAsync(await LoadMineAsync(id));

    [Authorize(HrPermissions.EvaluationsPermissions.ViewOwn)]
    public virtual async Task<EvaluationEmployeeDto> AcknowledgeAsync(Guid id)
    {
        var evaluation = await LoadMineAsync(id);
        if (evaluation.Status == EvaluationStatus.Submitted)
        {
            evaluation.Acknowledge(Clock.Now);
            await _evaluations.UpdateAsync(evaluation, autoSave: true);
        }

        return await ToEmployeeDtoAsync(evaluation);
    }

    [Authorize(HrPermissions.EvaluationsPermissions.ViewOwn)]
    public virtual async Task<EvaluationResponseDto> AddResponseAsync(Guid id, EvaluationResponseCreateDto input, Guid? idempotencyKey)
    {
        var fingerprint = new { id, input.Type, input.Content };
        var evaluation = await LoadMineAsync(id);

        var earlier = await _idempotency.FindResourceAsync(AddResponseOperation, idempotencyKey, fingerprint);
        if (earlier != null)
        {
            return (await ToResponseDtosAsync(evaluation.Responses.Where(x => x.Id == earlier.Value))).Single();
        }

        if (input.Type == EvaluationResponseType.Objection)
        {
            if (evaluation.ObjectionStatus == EvaluationObjectionStatus.Pending)
            {
                throw new BusinessException(HrErrorCodes.Evaluations.ObjectionAlreadyPending);
            }

            await CheckObjectionWindowAsync(evaluation);
        }

        var response = new EvaluationResponse(GuidGenerator.Create(), CurrentTenant.Id, id, input.Type!.Value, input.Content, Clock.Now);
        evaluation.Responses.Add(response);
        if (input.Type == EvaluationResponseType.Objection)
        {
            evaluation.SetObjectionStatus(EvaluationObjectionStatus.Pending);
        }

        await _evaluations.UpdateAsync(evaluation, autoSave: true);
        await _idempotency.SaveAsync(AddResponseOperation, idempotencyKey, fingerprint, response.Id);

        await NotifyOfResponseAsync(evaluation, input.Type.Value);
        return (await ToResponseDtosAsync(new[] { response })).Single();
    }

    [Authorize(HrPermissions.EvaluationsPermissions.DecideObjection)]
    public virtual async Task<EvaluationHrDto> DecideObjectionAsync(Guid id, Guid responseId, EvaluationObjectionDecideDto input)
    {
        var evaluation = await LoadAsync(id);
        if (evaluation.Status == EvaluationStatus.Draft)
        {
            throw new EntityNotFoundException(typeof(Evaluation), id);
        }

        var me = CurrentUser.GetId();
        var owner = await _employees.GetAsync(evaluation.EmployeeId);
        if (owner.UserId == me)
        {
            throw new BusinessException(HrErrorCodes.Evaluations.SelfDecisionNotAllowed);
        }

        // D29: the executive director decides the HR manager's objection only.
        if (!CurrentUser.IsInRole(HrRoles.HrManager) &&
            (owner.UserId == null || !(await LazyServiceProvider.LazyGetRequiredService<Volo.Abp.Identity.IIdentityUserRepository>().GetRoleNamesAsync(owner.UserId.Value)).Contains(HrRoles.HrManager)))
        {
            throw new Volo.Abp.Authorization.AbpAuthorizationException();
        }

        var response = evaluation.Responses.FirstOrDefault(x => x.Id == responseId)
                       ?? throw new EntityNotFoundException(typeof(EvaluationResponse), responseId);

        if (response.Type != EvaluationResponseType.Objection)
        {
            throw new BusinessException(HrErrorCodes.Evaluations.NotAnObjection);
        }

        if (response.Decision != null)
        {
            throw new BusinessException(HrErrorCodes.Evaluations.ObjectionAlreadyDecided);
        }

        evaluation.ConcurrencyStamp = input.ConcurrencyStamp;
        await ApplyCorrectedScoresAsync(evaluation, input);
        response.Decide(input.Decision!.Value, input.Reason, me, Clock.Now);
        evaluation.SetObjectionStatus(input.Decision == EvaluationObjectionDecision.Accepted ? EvaluationObjectionStatus.Accepted : EvaluationObjectionStatus.Rejected);
        await _evaluations.UpdateAsync(evaluation, autoSave: true);

        if (owner.UserId != null)
        {
            await _notifications.PublishAsync(owner.UserId.Value, NotificationTemplateCode.EvaluationObjectionDecided, NotificationResourceType.Evaluation, id);
        }

        var managerUserId = await GetCurrentManagerUserIdAsync(owner);
        if (managerUserId != null)
        {
            await _notifications.PublishAsync(managerUserId.Value, NotificationTemplateCode.EvaluationObjectionDecided, NotificationResourceType.Evaluation, id);
        }

        return await ToHrDtoAsync(evaluation);
    }

    [Authorize(HrPermissions.EvaluationsPermissions.HrRead)]
    public virtual async Task<EvaluationHrSummaryPagedResultDto> GetHrListAsync(EvaluationHrListInput input)
    {
        var me = CurrentUser.GetId();
        var now = Clock.Now;
        var employees = await _employees.GetQueryableAsync();
        var cycles = await _cycles.GetQueryableAsync();

        var query =
            from evaluation in (await _evaluations.GetQueryableAsync())
            join employee in employees on evaluation.EmployeeId equals employee.Id
            join cycle in cycles on evaluation.CycleId equals cycle.Id
            where employee.UserId != me
            select new HrRow { Evaluation = evaluation, DepartmentId = employee.DepartmentId, EscalatesAt = cycle.EscalatesAt, Year = cycle.Year, Month = cycle.Month };

        query = query
            .WhereIf(input.CycleId != null, x => x.Evaluation.CycleId == input.CycleId)
            .WhereIf(input.Status != null, x => x.Evaluation.Status == input.Status)
            .WhereIf(input.ObjectionStatus != null, x => x.Evaluation.ObjectionStatus == input.ObjectionStatus)
            .WhereIf(input.DepartmentId != null, x => x.DepartmentId == input.DepartmentId)
            .WhereIf(input.EmployeeId != null, x => x.Evaluation.EmployeeId == input.EmployeeId)
            .WhereIf(input.IsLate == true, x => (x.Evaluation.SubmittedAt ?? now) >= x.EscalatesAt)
            .WhereIf(input.IsLate == false, x => (x.Evaluation.SubmittedAt ?? now) < x.EscalatesAt);

        var total = await AsyncExecuter.LongCountAsync(query);
        var hrSort = SortFields.ToDictionary(x => x.Key, x => "Evaluation." + x.Value);
        var rows = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, "Evaluation.CreationTime desc", hrSort, "Evaluation.Id").Skip(input.SkipCount).Take(input.MaxResultCount));

        var refs = await _references.GetRefsAsync(rows.Select(x => x.Evaluation.EmployeeId));
        var evaluators = await GetEvaluatorsAsync(rows.Select(x => x.Evaluation).ToList());

        return new EvaluationHrSummaryPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => new EvaluationHrSummaryDto
            {
                Id = x.Evaluation.Id,
                CycleId = x.Evaluation.CycleId,
                Year = x.Year,
                Month = x.Month,
                Employee = refs[x.Evaluation.EmployeeId],
                Evaluator = evaluators[x.Evaluation.Id],
                Status = x.Evaluation.Status,
                ObjectionStatus = x.Evaluation.ObjectionStatus,
                SubmittedAt = x.Evaluation.SubmittedAt,
                IsLate = (x.Evaluation.SubmittedAt ?? now) >= x.EscalatesAt
            }).ToList()
        };
    }

    [Authorize(HrPermissions.EvaluationsPermissions.HrRead)]
    public virtual async Task<EvaluationHrDto> GetHrViewAsync(Guid id)
    {
        var evaluation = await LoadAsync(id);
        var owner = await _employees.GetAsync(evaluation.EmployeeId);
        if (evaluation.Status == EvaluationStatus.Draft || owner.UserId == CurrentUser.GetId())
        {
            throw new EntityNotFoundException(typeof(Evaluation), id);
        }

        return await ToHrDtoAsync(evaluation);
    }

    [Authorize(HrPermissions.EvaluationsPermissions.ViewOwn)]
    public virtual async Task<EvaluationAnnualSummaryDto> GetMyAnnualSummaryAsync(int year)
    {
        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == CurrentUser.GetId())
                 ?? throw new EntityNotFoundException(typeof(Employee));
        return await BuildAnnualSummaryAsync(me, year);
    }

    [Authorize(HrPermissions.EvaluationsPermissions.AnnualSummaryRead)]
    public virtual async Task<EvaluationAnnualSummaryDto> GetAnnualSummaryAsync(Guid employeeId, int year)
    {
        var employee = await _employees.GetAsync(employeeId);
        var userId = CurrentUser.GetId();
        var isHr = await AuthorizationService.IsGrantedAsync(HrPermissions.EvaluationsPermissions.HrRead);

        if (!isHr && !await IsCurrentManagerAsync(employee, userId))
        {
            throw new EntityNotFoundException(typeof(Employee), employeeId);
        }

        return await BuildAnnualSummaryAsync(employee, year);
    }

    private async Task<EvaluationAnnualSummaryDto> BuildAnnualSummaryAsync(Employee employee, int year)
    {
        var cycles = (await AsyncExecuter.ToListAsync((await _cycles.WithDetailsAsync(x => x.Criteria, x => x.Grades)).Where(x => x.Year == year))).ToDictionary(x => x.Id);
        var cycleIds = cycles.Keys.ToList();
        var forms = await AsyncExecuter.ToListAsync((await _evaluations.WithDetailsAsync(x => x.Scores, x => x.Responses))
            .Where(x => x.EmployeeId == employee.Id && cycleIds.Contains(x.CycleId) && x.Status != EvaluationStatus.Draft));

        var criteria = new Dictionary<Guid, EvaluationAnnualCriterionSummaryDto>();
        foreach (var form in forms)
        {
            var cycle = cycles[form.CycleId];
            foreach (var score in form.Scores)
            {
                var criterion = cycle.Criteria.FirstOrDefault(x => x.CriterionId == score.CriterionId);
                var grade = cycle.Grades.FirstOrDefault(x => x.Code == score.GradeCode);
                if (criterion == null || grade == null)
                {
                    continue;
                }

                if (!criteria.TryGetValue(score.CriterionId, out var row))
                {
                    row = new EvaluationAnnualCriterionSummaryDto { CriterionId = score.CriterionId, CriterionName = LocalizedName.Pick(criterion.NameAr, criterion.NameEn) };
                    criteria[score.CriterionId] = row;
                }

                var count = row.GradeCounts.FirstOrDefault(x => x.GradeCode == score.GradeCode);
                if (count == null)
                {
                    count = new EvaluationGradeCountDto { GradeCode = score.GradeCode, GradeName = LocalizedName.Pick(grade.NameAr, grade.NameEn) };
                    row.GradeCounts.Add(count);
                }

                count.Count++;
            }
        }

        return new EvaluationAnnualSummaryDto
        {
            Employee = await _references.ToRefAsync(employee),
            Year = year,
            EvaluatedMonthsCount = forms.Count,
            AcknowledgedCount = forms.Count(x => x.Status == EvaluationStatus.Acknowledged),
            ObjectionsCount = forms.Sum(x => x.Responses.Count(r => r.Type == EvaluationResponseType.Objection)),
            Criteria = criteria.Values.ToList()
        };
    }

    private static void CheckEditable(Evaluation evaluation, EvaluationCycle cycle)
    {
        if (evaluation.Status != EvaluationStatus.Draft)
        {
            throw new BusinessException(HrErrorCodes.Evaluations.NotEditable);
        }

        if (cycle.Status != EvaluationCycleStatus.Open)
        {
            throw new BusinessException(HrErrorCodes.Evaluations.CycleNotOpen);
        }
    }

    /// <summary>The contract has no code for a late objection, so it is rejected as invalid input on the type. See D30.</summary>
    /// <summary>D28: an accepted objection may correct grades. Criteria not listed keep theirs.</summary>
    private async Task ApplyCorrectedScoresAsync(Evaluation evaluation, EvaluationObjectionDecideDto input)
    {
        if (input.Scores == null || input.Scores.Count == 0)
        {
            return;
        }

        var cycle = await AsyncExecuter.FirstAsync((await _cycles.WithDetailsAsync(x => x.Criteria, x => x.Grades)).Where(x => x.Id == evaluation.CycleId));
        var seen = new HashSet<Guid>();
        foreach (var score in input.Scores)
        {
            if (!seen.Add(score.CriterionId!.Value) || cycle.Criteria.All(x => x.CriterionId != score.CriterionId))
            {
                throw new BusinessException(HrErrorCodes.Evaluations.InvalidCriterion);
            }

            if (cycle.Grades.All(x => x.Code != score.GradeCode))
            {
                throw new BusinessException(HrErrorCodes.Evaluations.InvalidGrade);
            }
        }

        foreach (var score in input.Scores)
        {
            var old = evaluation.Scores.FirstOrDefault(x => x.CriterionId == score.CriterionId);
            if (old != null)
            {
                evaluation.Scores.Remove(old);
            }

            evaluation.Scores.Add(new EvaluationScore(GuidGenerator.Create(), CurrentTenant.Id, evaluation.Id, score.CriterionId!.Value, score.GradeCode));
        }
    }

    private async Task CheckObjectionWindowAsync(Evaluation evaluation)
    {
        var zone = await _companyClock.GetTimeZoneAsync();
        var submittedOn = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(evaluation.SubmittedAt!.Value, zone));
        if (await _calendar.CountWorkingDaysAfterAsync(submittedOn, await _companyClock.GetTodayAsync()) > ObjectionWindowWorkingDays)
        {
            throw new BusinessException(HrErrorCodes.Evaluations.ObjectionWindowClosed);
        }
    }

    private async Task NotifyOfResponseAsync(Evaluation evaluation, EvaluationResponseType type)
    {
        var owner = await _employees.GetAsync(evaluation.EmployeeId);
        if (type == EvaluationResponseType.Objection)
        {
            foreach (var hr in (await _userManager.GetUsersInRoleAsync(HrRoles.HrManager)).Where(x => x.IsActive && x.Id != owner.UserId))
            {
                await _notifications.PublishAsync(hr.Id, NotificationTemplateCode.EvaluationResponseReceived, NotificationResourceType.Evaluation, evaluation.Id);
            }

            return;
        }

        var managerUserId = await GetCurrentManagerUserIdAsync(owner);
        if (managerUserId != null)
        {
            await _notifications.PublishAsync(managerUserId.Value, NotificationTemplateCode.EvaluationResponseReceived, NotificationResourceType.Evaluation, evaluation.Id);
        }
    }

    private async Task<Guid?> GetCurrentManagerUserIdAsync(Employee employee)
    {
        if (employee.ManagerEmployeeId == null)
        {
            return null;
        }

        return (await _employees.FindAsync(employee.ManagerEmployeeId.Value))?.UserId;
    }

    private async Task<bool> IsCurrentManagerAsync(Employee employee, Guid userId) =>
        await GetCurrentManagerUserIdAsync(employee) == userId;

    private async Task<List<Guid>> GetTeamIdsAsync()
    {
        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == CurrentUser.GetId());
        if (me == null)
        {
            return new List<Guid>();
        }

        return await AsyncExecuter.ToListAsync((await _employees.GetQueryableAsync()).Where(x => x.ManagerEmployeeId == me.Id).Select(x => x.Id));
    }

    private async Task<Evaluation> LoadAsync(Guid id)
    {
        var query = (await _evaluations.WithDetailsAsync(x => x.Scores, x => x.Responses)).Where(x => x.Id == id);
        return await AsyncExecuter.FirstOrDefaultAsync(query) ?? throw new EntityNotFoundException(typeof(Evaluation), id);
    }

    private async Task<Evaluation> LoadForCurrentManagerAsync(Guid id)
    {
        var evaluation = await LoadAsync(id);
        var owner = await _employees.GetAsync(evaluation.EmployeeId);
        if (!await IsCurrentManagerAsync(owner, CurrentUser.GetId()))
        {
            throw new EntityNotFoundException(typeof(Evaluation), id);
        }

        return evaluation;
    }

    private async Task<Evaluation> LoadMineAsync(Guid id)
    {
        var evaluation = await LoadAsync(id);
        var owner = await _employees.GetAsync(evaluation.EmployeeId);
        if (owner.UserId != CurrentUser.GetId() || evaluation.Status == EvaluationStatus.Draft)
        {
            throw new EntityNotFoundException(typeof(Evaluation), id);
        }

        return evaluation;
    }

    private async Task<EvaluationCycle> LoadCycleAsync(Guid id)
    {
        var query = (await _cycles.WithDetailsAsync(x => x.Criteria, x => x.Grades)).Where(x => x.Id == id);
        return await AsyncExecuter.FirstAsync(query);
    }

    private async Task<Dictionary<Guid, EvaluationCycle>> LoadCyclesAsync(IEnumerable<Guid> ids)
    {
        var distinct = ids.Distinct().ToList();
        return (await _cycles.GetListAsync(x => distinct.Contains(x.Id))).ToDictionary(x => x.Id);
    }

    private async Task<EvaluationManagerDto> ToManagerDtoAsync(Evaluation evaluation)
    {
        var cycle = await LoadCycleAsync(evaluation.CycleId);
        var owner = await _employees.GetAsync(evaluation.EmployeeId);
        var scores = evaluation.Scores.ToDictionary(x => x.CriterionId);

        return new EvaluationManagerDto
        {
            Id = evaluation.Id,
            CycleId = cycle.Id,
            Year = cycle.Year,
            Month = cycle.Month,
            Employee = await _references.ToRefAsync(owner),
            Status = evaluation.Status,
            IsEditable = evaluation.Status == EvaluationStatus.Draft && cycle.Status == EvaluationCycleStatus.Open,
            Attendance = await ToAttendanceAsync(evaluation, cycle),
            AvailableGrades = cycle.Grades.OrderBy(x => x.SortOrder).Select(x => new EvaluationGradeOptionDto { Code = x.Code, Name = LocalizedName.Pick(x.NameAr, x.NameEn) }).ToList(),
            Scores = cycle.Criteria.OrderBy(x => x.SortOrder).Select(c =>
            {
                var score = scores.GetValueOrDefault(c.CriterionId);
                var grade = score == null ? null : cycle.Grades.FirstOrDefault(x => x.Code == score.GradeCode);
                return new EvaluationDraftScoreDto
                {
                    CriterionId = c.CriterionId,
                    CriterionName = LocalizedName.Pick(c.NameAr, c.NameEn),
                    GradeCode = score?.GradeCode,
                    GradeName = grade == null ? null : LocalizedName.Pick(grade.NameAr, grade.NameEn)
                };
            }).ToList(),
            EmployeeNotes = evaluation.EmployeeNotes,
            SubmittedAt = evaluation.SubmittedAt,
            AcknowledgedAt = evaluation.AcknowledgedAt,
            ObjectionStatus = evaluation.ObjectionStatus,
            Responses = await ToResponseDtosAsync(evaluation.Responses),
            ConcurrencyStamp = evaluation.ConcurrencyStamp
        };
    }

    private async Task<EvaluationEmployeeDto> ToEmployeeDtoAsync(Evaluation evaluation)
    {
        var cycle = await LoadCycleAsync(evaluation.CycleId);
        var evaluators = await _references.GetUserRefsAsync(new[] { evaluation.SubmittedByUserId!.Value });

        return new EvaluationEmployeeDto
        {
            Id = evaluation.Id,
            Year = cycle.Year,
            Month = cycle.Month,
            Evaluator = evaluators[evaluation.SubmittedByUserId.Value],
            Status = evaluation.Status,
            Scores = ToSubmittedScores(evaluation, cycle),
            EmployeeNotes = evaluation.EmployeeNotes,
            SubmittedAt = evaluation.SubmittedAt!.Value,
            AcknowledgedAt = evaluation.AcknowledgedAt,
            ObjectionStatus = evaluation.ObjectionStatus,
            Responses = await ToResponseDtosAsync(evaluation.Responses)
        };
    }

    private async Task<EvaluationHrDto> ToHrDtoAsync(Evaluation evaluation)
    {
        var cycle = await LoadCycleAsync(evaluation.CycleId);
        var owner = await _employees.GetAsync(evaluation.EmployeeId);
        var evaluators = await _references.GetUserRefsAsync(new[] { evaluation.SubmittedByUserId!.Value });

        return new EvaluationHrDto
        {
            Id = evaluation.Id,
            CycleId = cycle.Id,
            Year = cycle.Year,
            Month = cycle.Month,
            Employee = await _references.ToRefAsync(owner),
            Evaluator = evaluators[evaluation.SubmittedByUserId.Value],
            Status = evaluation.Status,
            Attendance = await ToAttendanceAsync(evaluation, cycle),
            Scores = ToSubmittedScores(evaluation, cycle),
            EmployeeNotes = evaluation.EmployeeNotes,
            SubmittedAt = evaluation.SubmittedAt!.Value,
            AcknowledgedAt = evaluation.AcknowledgedAt,
            ObjectionStatus = evaluation.ObjectionStatus,
            Responses = await ToResponseDtosAsync(evaluation.Responses),
            ConcurrencyStamp = evaluation.ConcurrencyStamp
        };
    }

    public static List<EvaluationSubmittedScoreDto> ToSubmittedScores(Evaluation evaluation, EvaluationCycle cycle) =>
        cycle.Criteria.OrderBy(x => x.SortOrder)
            .Select(c => new { Criterion = c, Score = evaluation.Scores.FirstOrDefault(s => s.CriterionId == c.CriterionId) })
            .Where(x => x.Score != null)
            .Select(x =>
            {
                var grade = cycle.Grades.First(g => g.Code == x.Score!.GradeCode);
                return new EvaluationSubmittedScoreDto
                {
                    CriterionId = x.Criterion.CriterionId,
                    CriterionName = LocalizedName.Pick(x.Criterion.NameAr, x.Criterion.NameEn),
                    GradeCode = grade.Code,
                    GradeName = LocalizedName.Pick(grade.NameAr, grade.NameEn)
                };
            }).ToList();

    private async Task<EvaluationAttendanceSummaryDto> ToAttendanceAsync(Evaluation evaluation, EvaluationCycle cycle)
    {
        var dto = new EvaluationAttendanceSummaryDto
        {
            PeriodFrom = cycle.PeriodFrom,
            PeriodTo = cycle.PeriodTo,
            IsFrozen = evaluation.AttendanceFrozenAt != null,
            FrozenAt = evaluation.AttendanceFrozenAt
        };

        if (evaluation.AttendanceFrozenAt != null)
        {
            dto.ScheduledWorkingDays = evaluation.ScheduledWorkingDays;
            dto.PresentDays = evaluation.PresentDays;
            dto.AbsentDays = evaluation.AbsentDays;
            dto.LateCount = evaluation.LateCount;
            dto.LateMinutes = evaluation.LateMinutes;
            dto.LeaveDays = evaluation.LeaveDays;
            return dto;
        }

        var sum = (await _attendance.SumAsync(new[] { evaluation.EmployeeId }, cycle.PeriodFrom, cycle.PeriodTo)).GetValueOrDefault(evaluation.EmployeeId) ?? AttendanceSum.Zero;
        dto.ScheduledWorkingDays = sum.ScheduledDays;
        dto.PresentDays = sum.PresentDays;
        dto.AbsentDays = sum.AbsentDays;
        dto.LateCount = sum.LateCount;
        dto.LateMinutes = sum.LateMinutes;
        dto.LeaveDays = sum.LeaveDays;
        return dto;
    }

    private async Task<List<EvaluationResponseDto>> ToResponseDtosAsync(IEnumerable<EvaluationResponse> responses)
    {
        var rows = responses.OrderBy(x => x.CreationTime).ToList();
        var users = await _references.GetUserRefsAsync(rows.Where(x => x.DecidedByUserId != null).Select(x => x.DecidedByUserId!.Value));

        return rows.Select(x => new EvaluationResponseDto
        {
            Id = x.Id,
            Type = x.Type,
            Content = x.Content,
            CreationTime = x.CreationTime,
            Decision = x.Decision,
            DecisionReason = x.DecisionReason,
            DecidedBy = x.DecidedByUserId == null ? null : users[x.DecidedByUserId.Value],
            DecidedAt = x.DecidedAt
        }).ToList();
    }

    /// <summary>Who submitted it, or for a draft whoever manages the employee now.</summary>
    private async Task<Dictionary<Guid, UserRefDto>> GetEvaluatorsAsync(IReadOnlyCollection<Evaluation> rows)
    {
        var owners = (await _employees.GetListAsync(x => rows.Select(r => r.EmployeeId).Contains(x.Id))).ToDictionary(x => x.Id);
        var managerIds = owners.Values.Where(x => x.ManagerEmployeeId != null).Select(x => x.ManagerEmployeeId!.Value).Distinct().ToList();
        var managers = (await _employees.GetListAsync(x => managerIds.Contains(x.Id))).ToDictionary(x => x.Id);

        var userIds = new Dictionary<Guid, Guid?>();
        foreach (var row in rows)
        {
            var managerId = owners[row.EmployeeId].ManagerEmployeeId;
            userIds[row.Id] = row.SubmittedByUserId ?? (managerId == null ? null : managers.GetValueOrDefault(managerId.Value)?.UserId);
        }

        var users = await _references.GetUserRefsAsync(userIds.Values.Where(x => x != null).Select(x => x!.Value));
        return userIds.ToDictionary(
            x => x.Key,
            x => x.Value != null && users.TryGetValue(x.Value.Value, out var user) ? user : new UserRefDto { Id = Guid.Empty, DisplayName = string.Empty });
    }

    private sealed class HrRow
    {
        public Evaluation Evaluation { get; set; } = null!;
        public Guid DepartmentId { get; set; }
        public DateTime EscalatesAt { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
}

public class EvaluationSaveDraftDtoValidator : AbstractValidator<EvaluationSaveDraftDto>
{
    public EvaluationSaveDraftDtoValidator()
    {
        RuleFor(x => x.Scores).NotNull();
        RuleForEach(x => x.Scores).ChildRules(score =>
        {
            score.RuleFor(x => x.CriterionId).NotNull();
            score.RuleFor(x => x.GradeCode).NotEmpty();
        });
        RuleFor(x => x.EmployeeNotes).MaximumLength(EvaluationConsts.MaxNotesLength);
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public class EvaluationSubmitInputDtoValidator : AbstractValidator<EvaluationSubmitInputDto>
{
    public EvaluationSubmitInputDtoValidator()
    {
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public class EvaluationResponseCreateDtoValidator : AbstractValidator<EvaluationResponseCreateDto>
{
    public EvaluationResponseCreateDtoValidator()
    {
        RuleFor(x => x.Type).NotNull();
        RuleFor(x => x.Content).NotNull().Length(1, EvaluationConsts.MaxResponseLength);
    }
}

public class EvaluationObjectionDecideDtoValidator : AbstractValidator<EvaluationObjectionDecideDto>
{
    public EvaluationObjectionDecideDtoValidator()
    {
        RuleFor(x => x.Decision).NotNull();
        RuleFor(x => x.Reason).NotNull().Length(1, EvaluationConsts.MaxReasonLength);
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
        RuleFor(x => x.Scores).Null().When(x => x.Decision != EvaluationObjectionDecision.Accepted);
    }
}
