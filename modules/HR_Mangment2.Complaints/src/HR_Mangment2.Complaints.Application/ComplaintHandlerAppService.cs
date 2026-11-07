using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Identity;
using HR_Mangment2.Notifications;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace HR_Mangment2.Complaints;

/// <summary>Gives a person the screens matching an approved assignment. Access itself is always ComplaintAccessPolicy's decision.</summary>
public class ComplaintHandlerPermissionSync : ITransientDependency
{
    private static readonly string[] Permissions =
    {
        HrPermissions.ComplaintsPermissions.Handle,
        HrPermissions.ComplaintsPermissions.Close,
        HrPermissions.ComplaintsPermissions.RaiseLevel,
        HrPermissions.ComplaintsPermissions.Attachment,
        HrPermissions.ComplaintsPermissions.ViewAccessLog
    };

    private readonly IPermissionManager _permissions;
    private readonly IRepository<ComplaintHandlerAssignment, Guid> _assignments;
    private readonly IClock _clock;

    public ComplaintHandlerPermissionSync(IPermissionManager permissions, IRepository<ComplaintHandlerAssignment, Guid> assignments, IClock clock)
    {
        _permissions = permissions;
        _assignments = assignments;
        _clock = clock;
    }

    public virtual async Task GrantAsync(Guid userId)
    {
        foreach (var permission in Permissions)
        {
            await _permissions.SetForUserAsync(userId, permission, true);
        }
    }

    /// <summary>Takes the screens back unless another assignment still needs them. A role keeps its own grant, which is separate.</summary>
    public virtual async Task ReleaseAsync(Guid userId, Guid endingAssignmentId)
    {
        var now = _clock.Now;
        var stillNeeded = await _assignments.AnyAsync(x =>
            x.UserId == userId &&
            x.Id != endingAssignmentId &&
            x.Status == ComplaintHandlerAssignmentStatus.Approved &&
            (x.To == null || x.To > now));

        if (stillNeeded)
        {
            return;
        }

        foreach (var permission in Permissions)
        {
            await _permissions.SetForUserAsync(userId, permission, false);
        }
    }
}

/// <summary>Holds no complaint data, so unlike ComplaintAppService this one stays in the general audit log.</summary>
[Authorize]
public class ComplaintHandlerAppService : HR_Mangment2AppService, IComplaintHandlerAppService
{
    private const string RequestOperation = "ComplaintHandler_Request";

    /// <summary>A start time typed now reaches the server a moment later.</summary>
    private static readonly TimeSpan StartGrace = TimeSpan.FromMinutes(5);

    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["requestedAt"] = nameof(ComplaintHandlerAssignment.RequestedAt),
        ["from"] = nameof(ComplaintHandlerAssignment.From),
        ["to"] = nameof(ComplaintHandlerAssignment.To),
        ["status"] = nameof(ComplaintHandlerAssignment.Status),
        ["level"] = nameof(ComplaintHandlerAssignment.Level)
    };

    private readonly IRepository<ComplaintHandlerAssignment, Guid> _assignments;
    private readonly IIdentityUserRepository _users;
    private readonly IdentityUserManager _userManager;
    private readonly ComplaintHandlerPermissionSync _permissionSync;
    private readonly HrNotificationPublisher _notifications;
    private readonly EmployeeReferenceReader _references;
    private readonly IdempotencyStore _idempotency;

    public ComplaintHandlerAppService(
        IRepository<ComplaintHandlerAssignment, Guid> assignments,
        IIdentityUserRepository users,
        IdentityUserManager userManager,
        ComplaintHandlerPermissionSync permissionSync,
        HrNotificationPublisher notifications,
        EmployeeReferenceReader references,
        IdempotencyStore idempotency)
    {
        _assignments = assignments;
        _users = users;
        _userManager = userManager;
        _permissionSync = permissionSync;
        _notifications = notifications;
        _references = references;
        _idempotency = idempotency;
    }

    [Authorize(HrPermissions.ComplaintsPermissions.HandlersRequest)]
    public virtual async Task<ComplaintHandlerAssignmentDto> RequestAsync(ComplaintHandlerAssignmentCreateDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(RequestOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return await ToDtoAsync(await _assignments.GetAsync(earlier.Value));
        }

        var userId = input.UserId!.Value;
        var user = await _userManager.FindByIdAsync(userId.ToString()) ?? throw new EntityNotFoundException(typeof(IdentityUser), userId);
        await CheckEligibleAsync(user.Id, input.Level!.Value);

        var now = Clock.Now;
        var from = input.From!.Value;
        if (from < now - StartGrace || (input.To != null && input.To <= from))
        {
            throw new BusinessException(HrErrorCodes.Complaints.InvalidAssignmentPeriod);
        }

        var assignment = new ComplaintHandlerAssignment(
            GuidGenerator.Create(), CurrentTenant.Id, user.Id, input.Level.Value, from, input.To, input.Reason, CurrentUser.GetId(), now);
        await _assignments.InsertAsync(assignment, autoSave: true);

        foreach (var executive in (await _userManager.GetUsersInRoleAsync(HrRoles.ExecutiveDirector)).Where(x => x.IsActive))
        {
            await _notifications.PublishAsync(executive.Id, NotificationTemplateCode.SecureHandlerAssignmentUpdate);
        }

        await _idempotency.SaveAsync(RequestOperation, idempotencyKey, input, assignment.Id);
        return await ToDtoAsync(assignment);
    }

    [Authorize(HrPermissions.ComplaintsPermissions.HandlersRead)]
    public virtual async Task<ComplaintHandlerAssignmentPagedResultDto> GetListAsync(ComplaintHandlerListInput input)
    {
        var query = await _assignments.GetQueryableAsync();

        if (input.Status != null)
        {
            query = query.Where(x => x.Status == input.Status);
        }

        if (input.Level != null)
        {
            query = query.Where(x => x.Level == input.Level);
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(ComplaintHandlerAssignment.RequestedAt) + " desc", SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new ComplaintHandlerAssignmentPagedResultDto { TotalCount = total, Items = await ToDtosAsync(items) };
    }

    public virtual async Task<ComplaintHandlerAssignmentDto> ApproveAsync(Guid id, ComplaintHandlerAssignmentApproveDto input)
    {
        await CheckExecutiveAsync();

        var assignment = await _assignments.GetAsync(id);
        if (assignment.Status != ComplaintHandlerAssignmentStatus.PendingApproval)
        {
            throw new BusinessException(HrErrorCodes.Complaints.AssignmentNotPending);
        }

        // Roles may have changed since the request.
        await CheckEligibleAsync(assignment.UserId, assignment.Level);

        assignment.ConcurrencyStamp = input.ConcurrencyStamp;
        assignment.Approve(CurrentUser.GetId(), Clock.Now);
        await _assignments.UpdateAsync(assignment, autoSave: true);

        await _permissionSync.GrantAsync(assignment.UserId);
        await _notifications.PublishAsync(assignment.UserId, NotificationTemplateCode.SecureHandlerAssignmentUpdate);
        await NotifyRequesterAsync(assignment);
        return await ToDtoAsync(assignment);
    }

    public virtual async Task<ComplaintHandlerAssignmentDto> RejectAsync(Guid id, ComplaintHandlerAssignmentReasonDto input)
    {
        await CheckExecutiveAsync();

        var assignment = await _assignments.GetAsync(id);
        if (assignment.Status != ComplaintHandlerAssignmentStatus.PendingApproval)
        {
            throw new BusinessException(HrErrorCodes.Complaints.AssignmentNotPending);
        }

        assignment.ConcurrencyStamp = input.ConcurrencyStamp;
        assignment.Reject(CurrentUser.GetId(), input.Reason, Clock.Now);
        await _assignments.UpdateAsync(assignment, autoSave: true);

        await NotifyRequesterAsync(assignment);
        return await ToDtoAsync(assignment);
    }

    public virtual async Task<ComplaintHandlerAssignmentDto> RevokeAsync(Guid id, ComplaintHandlerAssignmentReasonDto input)
    {
        await CheckExecutiveAsync();

        var assignment = await _assignments.GetAsync(id);
        var now = Clock.Now;
        if (assignment.Status != ComplaintHandlerAssignmentStatus.Approved || (assignment.To != null && assignment.To <= now))
        {
            throw new BusinessException(HrErrorCodes.Complaints.AssignmentNotActive);
        }

        assignment.ConcurrencyStamp = input.ConcurrencyStamp;
        assignment.Revoke(CurrentUser.GetId(), input.Reason, now);
        await _assignments.UpdateAsync(assignment, autoSave: true);

        await _permissionSync.ReleaseAsync(assignment.UserId, assignment.Id);
        await _notifications.PublishAsync(assignment.UserId, NotificationTemplateCode.SecureHandlerAssignmentUpdate);
        await NotifyRequesterAsync(assignment);
        return await ToDtoAsync(assignment);
    }

    /// <summary>The permission alone is not enough: approval belongs to the role itself and cannot be granted or delegated.</summary>
    private async Task CheckExecutiveAsync()
    {
        await AuthorizationService.CheckAsync(HrPermissions.ComplaintsPermissions.HandlersApprove);

        if (!(await _users.GetRoleNamesAsync(CurrentUser.GetId())).Contains(HrRoles.ExecutiveDirector))
        {
            throw new AbpAuthorizationException();
        }
    }

    private async Task CheckEligibleAsync(Guid userId, ComplaintLevel level)
    {
        var roles = await _users.GetRoleNamesAsync(userId);

        if (roles.Contains(HrRoles.SystemAdmin))
        {
            throw new BusinessException(HrErrorCodes.Complaints.HandlerCannotBeSystemAdmin);
        }

        if (!ComplaintAccessPolicy.IsAssignable(roles, level))
        {
            throw new BusinessException(HrErrorCodes.Complaints.HandlerRoleNotAllowed);
        }
    }

    private Task NotifyRequesterAsync(ComplaintHandlerAssignment assignment) =>
        _notifications.PublishAsync(assignment.RequestedByUserId, NotificationTemplateCode.SecureHandlerAssignmentUpdate);

    private async Task<ComplaintHandlerAssignmentDto> ToDtoAsync(ComplaintHandlerAssignment assignment) =>
        (await ToDtosAsync(new[] { assignment })).Single();

    private async Task<List<ComplaintHandlerAssignmentDto>> ToDtosAsync(IReadOnlyCollection<ComplaintHandlerAssignment> assignments)
    {
        var userIds = assignments.SelectMany(x => new[] { x.UserId, x.RequestedByUserId })
            .Concat(assignments.Where(x => x.DecidedByUserId != null).Select(x => x.DecidedByUserId!.Value));
        var users = await _references.GetUserRefsAsync(userIds);

        return assignments.Select(x => new ComplaintHandlerAssignmentDto
        {
            Id = x.Id,
            User = users[x.UserId],
            Level = x.Level,
            Status = x.Status,
            From = x.From,
            To = x.To,
            Reason = x.Reason,
            RequestedBy = users[x.RequestedByUserId],
            RequestedAt = x.RequestedAt,
            DecidedBy = x.DecidedByUserId == null ? null : users[x.DecidedByUserId.Value],
            DecidedAt = x.DecidedAt,
            DecisionReason = x.DecisionReason,
            ConcurrencyStamp = x.ConcurrencyStamp
        }).ToList();
    }
}

/// <summary>An assignment past its end date stops working at once through the policy. This takes the screens back too.</summary>
public class ComplaintHandlerExpiryWorker : AsyncPeriodicBackgroundWorkerBase
{
    public ComplaintHandlerExpiryWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = (int)TimeSpan.FromHours(1).TotalMilliseconds;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var services = workerContext.ServiceProvider;
        var unitOfWorkManager = services.GetRequiredService<IUnitOfWorkManager>();
        var currentTenant = services.GetRequiredService<ICurrentTenant>();

        List<Guid> tenantIds;
        using (var read = unitOfWorkManager.Begin(requiresNew: true))
        {
            tenantIds = (await services.GetRequiredService<ITenantRepository>().GetListAsync()).Select(x => x.Id).ToList();
            await read.CompleteAsync();
        }

        foreach (var tenantId in tenantIds)
        {
            using (currentTenant.Change(tenantId))
            using (var unitOfWork = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                await services.GetRequiredService<ComplaintHandlerExpiryService>().ReleaseExpiredAsync(services.GetRequiredService<IClock>().Now);
                await unitOfWork.CompleteAsync();
            }
        }
    }
}

public class ComplaintHandlerExpiryService : ITransientDependency
{
    private readonly IRepository<ComplaintHandlerAssignment, Guid> _assignments;
    private readonly ComplaintHandlerPermissionSync _permissionSync;

    public ComplaintHandlerExpiryService(IRepository<ComplaintHandlerAssignment, Guid> assignments, ComplaintHandlerPermissionSync permissionSync)
    {
        _assignments = assignments;
        _permissionSync = permissionSync;
    }

    public virtual async Task<int> ReleaseExpiredAsync(DateTime now)
    {
        var expired = await _assignments.GetListAsync(x =>
            x.Status == ComplaintHandlerAssignmentStatus.Approved && !x.PermissionsReleased && x.To != null && x.To <= now);

        foreach (var assignment in expired)
        {
            await _permissionSync.ReleaseAsync(assignment.UserId, assignment.Id);
            assignment.MarkPermissionsReleased();
            await _assignments.UpdateAsync(assignment);
        }

        return expired.Count;
    }
}

public class ComplaintHandlerAssignmentCreateDtoValidator : AbstractValidator<ComplaintHandlerAssignmentCreateDto>
{
    public ComplaintHandlerAssignmentCreateDtoValidator()
    {
        RuleFor(x => x.UserId).NotNull();
        RuleFor(x => x.Level).NotNull();
        RuleFor(x => x.From).NotNull();
        RuleFor(x => x.Reason).NotNull().Length(1, ComplaintHandlerAssignment.MaxReasonLength);
    }
}

public class ComplaintHandlerAssignmentApproveDtoValidator : AbstractValidator<ComplaintHandlerAssignmentApproveDto>
{
    public ComplaintHandlerAssignmentApproveDtoValidator()
    {
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public class ComplaintHandlerAssignmentReasonDtoValidator : AbstractValidator<ComplaintHandlerAssignmentReasonDto>
{
    public ComplaintHandlerAssignmentReasonDtoValidator()
    {
        RuleFor(x => x.Reason).NotNull().Length(1, ComplaintHandlerAssignment.MaxReasonLength);
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}
