using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Identity;
using HR_Mangment2.Localization;
using HR_Mangment2.Notifications;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace HR_Mangment2.Delegations;

[Authorize]
public class DelegationAppService : HR_Mangment2AppService, IDelegationAppService
{
    /// <summary>A start time sent as "now" reaches the server slightly in the past.</summary>
    private static readonly TimeSpan StartTolerance = TimeSpan.FromMinutes(5);

    /// <summary>Roles that may delegate more than the accountant's requests step.</summary>
    private static readonly string[] FullScopeRoles =
    {
        HrRoles.DepartmentManager, HrRoles.HrManager, HrRoles.ExecutiveDirector, HrRoles.SystemAdmin
    };

    private readonly IRepository<Delegation, Guid> _delegations;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly EmployeeReferenceReader _references;
    private readonly AdministrativeTaskLogger _taskLog;
    private readonly HrNotificationPublisher _notifications;

    public DelegationAppService(
        IRepository<Delegation, Guid> delegations,
        IRepository<Employee, Guid> employees,
        EmployeeReferenceReader references,
        AdministrativeTaskLogger taskLog,
        HrNotificationPublisher notifications)
    {
        _delegations = delegations;
        _employees = employees;
        _references = references;
        _taskLog = taskLog;
        _notifications = notifications;
    }

    private static Dictionary<string, string> SortFields => new()
    {
        ["startsAt"] = nameof(Delegation.StartsAt),
        ["endsAt"] = nameof(Delegation.EndsAt),
        ["creationTime"] = nameof(Delegation.CreationTime)
    };

    private static string DefaultSorting => nameof(Delegation.CreationTime) + " desc";

    [Authorize(HrPermissions.DelegationPermissions.ReadAll)]
    public virtual async Task<DelegationPagedResultDto> GetListAsync(DelegationListInput input)
    {
        var query = ApplyStatus(await _delegations.GetQueryableAsync(), input.Status);

        if (input.FromUserId != null)
        {
            query = query.Where(x => x.FromUserId == input.FromUserId);
        }

        if (input.ToUserId != null)
        {
            query = query.Where(x => x.ToUserId == input.ToUserId);
        }

        return await PageAsync(query, input.Sorting, input.SkipCount, input.MaxResultCount);
    }

    [Authorize(HrPermissions.DelegationPermissions.Manage)]
    public virtual async Task<DelegationDto> CreateAsync(DelegationCreateDto input)
    {
        var caller = CurrentUser.GetId();
        var me = input.FromUserId ?? caller;
        var toUserId = input.ToUserId!.Value;
        var scope = input.Scope!.Value;
        var startsAt = input.StartsAt!.Value.ToUniversalTime();
        var endsAt = input.EndsAt!.Value.ToUniversalTime();

        if (toUserId == me)
        {
            throw new BusinessException(HrErrorCodes.Delegation.CannotDelegateToSelf);
        }

        if (startsAt < Clock.Now.Subtract(StartTolerance) || endsAt <= startsAt)
        {
            throw new BusinessException(HrErrorCodes.Delegation.InvalidPeriod);
        }

        // D8, QUESTIONS.md item 10: HR may delegate for an absent approver. Nobody else may name another person.
        IReadOnlyCollection<string> delegatorRoles;
        if (me == caller)
        {
            delegatorRoles = CurrentUser.Roles;
        }
        else
        {
            if (!CurrentUser.IsInRole(HrRoles.HrManager))
            {
                throw new Volo.Abp.Authorization.AbpAuthorizationException();
            }

            delegatorRoles = await LazyServiceProvider.LazyGetRequiredService<Volo.Abp.Identity.IIdentityUserRepository>().GetRoleNamesAsync(me);
            if (!await _employees.AnyAsync(x => x.UserId == me && x.Status == EmployeeStatus.Active) || !delegatorRoles.Any(x => FullScopeRoles.Contains(x) || x == HrRoles.Accountant || x == HrRoles.Reviewer))
            {
                throw new BusinessException(HrErrorCodes.Delegation.DelegatorNotEligible);
            }
        }

        // The accountant delegates the requests step only, and the reviewer the follow-up reports only (decision
        // 2026-11-11); nobody else delegates follow-up reports. The contract has no other code that fits these refusals.
        var fullScope = delegatorRoles.Any(x => FullScopeRoles.Contains(x));
        var accountant = delegatorRoles.Contains(HrRoles.Accountant);
        var reviewer = delegatorRoles.Contains(HrRoles.Reviewer);
        var scopeAllowed = scope == DelegationScope.ReviewReports
            ? reviewer
            : fullScope || (accountant && scope == DelegationScope.Requests);
        var eligible = await _employees.AnyAsync(x => x.UserId == toUserId && x.Status == EmployeeStatus.Active);
        if (!eligible || !scopeAllowed)
        {
            throw new BusinessException(HrErrorCodes.Delegation.DelegateNotEligible);
        }

        var overlapping = await _delegations.AnyAsync(x =>
            x.FromUserId == me &&
            x.CancelledAt == null &&
            x.StartsAt < endsAt &&
            x.EndsAt > startsAt &&
            (x.Scope == scope ||
             (scope != DelegationScope.ReviewReports && x.Scope != DelegationScope.ReviewReports &&
              (x.Scope == DelegationScope.AllApprovals || scope == DelegationScope.AllApprovals))));
        if (overlapping)
        {
            throw new BusinessException(HrErrorCodes.Delegation.OverlappingDelegation);
        }

        var delegation = await _delegations.InsertAsync(
            new Delegation(GuidGenerator.Create(), CurrentTenant.Id, me, toUserId, scope, startsAt, endsAt, input.Reason),
            autoSave: true);

        await _taskLog.LogAsync(caller, AdministrativeTaskType.DelegationCreated, delegation.Id);
        await _notifications.PublishAsync(toUserId, NotificationTemplateCode.DelegationUpdated);

        return (await ToDtosAsync(new[] { delegation })).Single();
    }

    public virtual async Task<DelegationPagedResultDto> GetMyAsync(MyDelegationListInput input)
    {
        var me = CurrentUser.GetId();
        var query = ApplyStatus(await _delegations.GetQueryableAsync(), input.Status);

        query = input.Direction switch
        {
            DelegationDirection.Given => query.Where(x => x.FromUserId == me),
            DelegationDirection.Received => query.Where(x => x.ToUserId == me),
            _ => query.Where(x => x.FromUserId == me || x.ToUserId == me)
        };

        return await PageAsync(query, input.Sorting, input.SkipCount, input.MaxResultCount);
    }

    [Authorize(HrPermissions.DelegationPermissions.Manage)]
    public virtual async Task<DelegationCandidatePagedResultDto> GetCandidatesAsync(DelegationCandidateInput input)
    {
        var me = CurrentUser.GetId();
        var query = (await _employees.GetQueryableAsync())
            .Where(x => x.UserId != null && x.UserId != me && x.Status == EmployeeStatus.Active);

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var search = input.Search.Trim().ToLower();
            query = query.Where(x => x.FullNameAr.ToLower().Contains(search) || (x.FullNameEn != null && x.FullNameEn.ToLower().Contains(search)));
        }

        var nameField = LocalizedName.IsEnglish ? nameof(Employee.FullNameEn) : nameof(Employee.FullNameAr);
        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameField, new Dictionary<string, string> { ["displayName"] = nameField })
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));
        var photos = await _references.GetPhotoIdsAsync(items);

        return new DelegationCandidatePagedResultDto
        {
            TotalCount = total,
            Items = items.Select(x => new UserRefDto { Id = x.UserId!.Value, DisplayName = EmployeeReferenceReader.FullName(x), PhotoId = EmployeeReferenceReader.PhotoOf(x, photos) }).ToList()
        };
    }

    [Authorize(HrPermissions.DelegationPermissions.Manage)]
    public virtual async Task CancelAsync(Guid id)
    {
        var delegation = await _delegations.FindAsync(id);
        if (delegation == null || delegation.FromUserId != CurrentUser.GetId())
        {
            throw new EntityNotFoundException(typeof(Delegation), id);
        }

        if (delegation.GetStatus(Clock.Now) is DelegationStatus.Cancelled or DelegationStatus.Expired)
        {
            throw new BusinessException(HrErrorCodes.Delegation.AlreadyEnded);
        }

        delegation.Cancel(Clock.Now);
        await _delegations.UpdateAsync(delegation);
        await _notifications.PublishAsync(delegation.ToUserId, NotificationTemplateCode.DelegationUpdated);
    }

    private IQueryable<Delegation> ApplyStatus(IQueryable<Delegation> query, DelegationStatus? status)
    {
        var now = Clock.Now;
        return status switch
        {
            DelegationStatus.Cancelled => query.Where(x => x.CancelledAt != null),
            DelegationStatus.Scheduled => query.Where(x => x.CancelledAt == null && now < x.StartsAt),
            DelegationStatus.Active => query.Where(x => x.CancelledAt == null && x.StartsAt <= now && now < x.EndsAt),
            DelegationStatus.Expired => query.Where(x => x.CancelledAt == null && x.EndsAt <= now),
            _ => query
        };
    }

    private async Task<DelegationPagedResultDto> PageAsync(IQueryable<Delegation> query, string? sorting, int skip, int take)
    {
        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.ApplySorting(sorting, DefaultSorting, SortFields).Skip(skip).Take(take));
        return new DelegationPagedResultDto { TotalCount = total, Items = await ToDtosAsync(items) };
    }

    private async Task<List<DelegationDto>> ToDtosAsync(IReadOnlyCollection<Delegation> items)
    {
        var users = await _references.GetUserRefsAsync(items.SelectMany(x => new[] { x.FromUserId, x.ToUserId }));
        var now = Clock.Now;

        return items.Select(x => new DelegationDto
        {
            Id = x.Id,
            FromUser = users[x.FromUserId],
            ToUser = users[x.ToUserId],
            Scope = x.Scope,
            StartsAt = x.StartsAt,
            EndsAt = x.EndsAt,
            Reason = x.Reason,
            Status = x.GetStatus(now),
            CancelledAt = x.CancelledAt,
            CreationTime = x.CreationTime
        }).ToList();
    }
}

public class DelegationCreateDtoValidator : AbstractValidator<DelegationCreateDto>
{
    public DelegationCreateDtoValidator()
    {
        RuleFor(x => x.ToUserId).NotNull();
        RuleFor(x => x.Scope).NotNull();
        RuleFor(x => x.StartsAt).NotNull();
        RuleFor(x => x.EndsAt).NotNull();
        RuleFor(x => x.Reason).NotNull().Length(1, Delegation.MaxReasonLength);
    }
}
