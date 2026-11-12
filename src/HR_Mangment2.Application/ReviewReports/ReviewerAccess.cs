using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Delegations;
using HR_Mangment2.Permissions;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;
using Volo.Abp.Users;

namespace HR_Mangment2.ReviewReports;

/// <summary>
/// Who may work as a reviewer for the daily report and its rooms: the reviewer (Hr.ReviewReports.Manage) and, during
/// an active reviewReports delegation, their stand-in even without the permission (decision 2026-11-11).
/// </summary>
public class ReviewerAccess : ITransientDependency
{
    private readonly IPermissionChecker _permissions;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IRepository<Delegation, Guid> _delegations;

    public ReviewerAccess(IPermissionChecker permissions, ICurrentUser currentUser, IClock clock, IRepository<Delegation, Guid> delegations)
    {
        _permissions = permissions;
        _currentUser = currentUser;
        _clock = clock;
        _delegations = delegations;
    }

    /// <summary>The reviewer writes; the executive director only reads. The two roles never go together.</summary>
    public virtual Task<bool> IsAuthorAsync() => _permissions.IsGrantedAsync(HrPermissions.ReviewReportsPermissions.Manage);

    /// <summary>Reviewers who delegated their daily report to the caller, right now.</summary>
    public virtual async Task<List<Guid>> DelegatingReviewersAsync()
    {
        var me = _currentUser.GetId();
        var now = _clock.Now;
        return (await _delegations.GetListAsync(x =>
                x.ToUserId == me && x.Scope == DelegationScope.ReviewReports && x.CancelledAt == null && x.StartsAt <= now && x.EndsAt > now))
            .Select(x => x.FromUserId)
            .Distinct()
            .ToList();
    }

    /// <summary>The reviewers whose work the caller may do: themself when they are one, and those who delegated to them.</summary>
    public virtual async Task<List<Guid>> ReviewersICanActForAsync()
    {
        var result = await DelegatingReviewersAsync();
        if (await IsAuthorAsync())
        {
            result.Insert(0, _currentUser.GetId());
        }

        return result.Distinct().ToList();
    }

    /// <summary>The reviewer a new record belongs to: the caller, or the reviewer named in onBehalfOfUserId.</summary>
    public virtual async Task<Guid> ResolveReviewerAsync(Guid? onBehalfOfUserId)
    {
        var me = _currentUser.GetId();
        if (onBehalfOfUserId == null || onBehalfOfUserId == me)
        {
            if (!await IsAuthorAsync())
            {
                throw new AbpAuthorizationException();
            }

            return me;
        }

        if (!(await DelegatingReviewersAsync()).Contains(onBehalfOfUserId.Value))
        {
            throw new AbpAuthorizationException();
        }

        return onBehalfOfUserId.Value;
    }
}
