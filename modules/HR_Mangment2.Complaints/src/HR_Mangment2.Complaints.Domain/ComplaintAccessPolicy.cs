using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Timing;

namespace HR_Mangment2.Complaints;

/// <summary>Who this user is for complaints, read from the database for every operation and never from the token.</summary>
public class ComplaintAccessContext
{
    public ComplaintAccessContext(Guid userId, IReadOnlySet<string> roles, IReadOnlySet<ComplaintLevel> assignedLevels)
    {
        UserId = userId;
        Roles = roles;
        AssignedLevels = assignedLevels;
    }

    public Guid UserId { get; }
    public IReadOnlySet<string> Roles { get; }

    /// <summary>Levels from approved assignments in their period, after the role conflicts are checked again.</summary>
    public IReadOnlySet<ComplaintLevel> AssignedLevels { get; }

    /// <summary>A system administrator handles nothing, whatever other role they give themselves.</summary>
    public bool MayHandleAtAll => !Roles.Contains(HrRoles.SystemAdmin);

    public bool HandlesAsHr => MayHandleAtAll && Roles.Contains(HrRoles.HrManager);

    public bool HandlesAsExecutive => MayHandleAtAll && Roles.Contains(HrRoles.ExecutiveDirector);

    public bool HandlesAsManager => MayHandleAtAll && Roles.Contains(HrRoles.DepartmentManager);

    public bool MayHandleLevel(ComplaintLevel level) => level switch
    {
        ComplaintLevel.HighlyConfidential => HandlesAsExecutive || AssignedLevels.Contains(level),
        _ => HandlesAsHr || AssignedLevels.Contains(level)
    };

    /// <summary>The single definition of a handler. The query form below must say the same thing.</summary>
    public bool CanHandle(Complaint complaint)
    {
        if (!MayHandleAtAll || complaint.ComplainantUserId == UserId || complaint.RespondentUserId == UserId)
        {
            return false;
        }

        return complaint.Status == ComplaintStatus.PendingDepartmentManager
            ? HandlesAsManager && complaint.DepartmentManagerUserId == UserId
            : MayHandleLevel(complaint.Level);
    }

    public IQueryable<Complaint> ApplyHandlerScope(IQueryable<Complaint> query)
    {
        if (!MayHandleAtAll)
        {
            return query.Where(x => false);
        }

        var me = UserId;
        var asManager = HandlesAsManager;
        var normal = MayHandleLevel(ComplaintLevel.Normal);
        var confidential = MayHandleLevel(ComplaintLevel.Confidential);
        var highlyConfidential = MayHandleLevel(ComplaintLevel.HighlyConfidential);

        return query.Where(x =>
            x.ComplainantUserId != me &&
            (x.RespondentUserId == null || x.RespondentUserId != me) &&
            (
                (x.Status == ComplaintStatus.PendingDepartmentManager && asManager && x.DepartmentManagerUserId == me) ||
                (x.Status != ComplaintStatus.PendingDepartmentManager &&
                 (
                     (x.Level == ComplaintLevel.Normal && normal) ||
                     (x.Level == ComplaintLevel.Confidential && confidential) ||
                     (x.Level == ComplaintLevel.HighlyConfidential && highlyConfidential)
                 ))
            ));
    }
}

/// <summary>Checks every complaint operation. A failure always looks like the complaint does not exist.</summary>
public class ComplaintAccessPolicy : ITransientDependency
{
    private readonly IIdentityUserRepository _users;
    private readonly IdentityUserManager _userManager;
    private readonly IRepository<ComplaintHandlerAssignment, Guid> _assignments;
    private readonly IClock _clock;

    public ComplaintAccessPolicy(
        IIdentityUserRepository users,
        IdentityUserManager userManager,
        IRepository<ComplaintHandlerAssignment, Guid> assignments,
        IClock clock)
    {
        _users = users;
        _userManager = userManager;
        _assignments = assignments;
        _clock = clock;
    }

    /// <summary>Reviewers never, a department manager only for normal complaints, the executive never for normal ones.</summary>
    public static bool IsAssignable(IReadOnlyCollection<string> roles, ComplaintLevel level) =>
        !roles.Contains(HrRoles.Reviewer) &&
        !(roles.Contains(HrRoles.DepartmentManager) && level >= ComplaintLevel.Confidential) &&
        !(roles.Contains(HrRoles.ExecutiveDirector) && level == ComplaintLevel.Normal);

    public virtual async Task<ComplaintAccessContext> GetContextAsync(Guid userId)
    {
        var roles = (await _users.GetRoleNamesAsync(userId)).ToHashSet();
        var now = _clock.Now;
        var assignments = await _assignments.GetListAsync(x =>
            x.UserId == userId &&
            x.Status == ComplaintHandlerAssignmentStatus.Approved &&
            x.From <= now &&
            (x.To == null || x.To > now));

        var levels = assignments.Select(x => x.Level).Where(x => IsAssignable(roles, x)).ToHashSet();
        return new ComplaintAccessContext(userId, roles, levels);
    }

    /// <summary>Everyone who can handle this complaint right now, role holders first. Never a party to it.</summary>
    public virtual async Task<List<Guid>> GetHandlerUserIdsAsync(Complaint complaint)
    {
        if (complaint.Status == ComplaintStatus.PendingDepartmentManager)
        {
            return complaint.DepartmentManagerUserId == null ? new List<Guid>() : new List<Guid> { complaint.DepartmentManagerUserId.Value };
        }

        var role = complaint.Level == ComplaintLevel.HighlyConfidential ? HrRoles.ExecutiveDirector : HrRoles.HrManager;
        var candidates = (await _userManager.GetUsersInRoleAsync(role)).Where(x => x.IsActive).Select(x => x.Id).ToList();

        var now = _clock.Now;
        var level = complaint.Level;
        var assigned = await _assignments.GetListAsync(x =>
            x.Level == level &&
            x.Status == ComplaintHandlerAssignmentStatus.Approved &&
            x.From <= now &&
            (x.To == null || x.To > now));
        candidates.AddRange(assigned.Select(x => x.UserId));

        var handlers = new List<Guid>();
        foreach (var candidate in candidates.Distinct())
        {
            if ((await GetContextAsync(candidate)).CanHandle(complaint))
            {
                handlers.Add(candidate);
            }
        }

        return handlers;
    }
}
