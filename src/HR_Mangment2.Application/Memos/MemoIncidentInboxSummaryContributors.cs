using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Requests;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace HR_Mangment2.Memos;

public class MemoInboxSummaryContributor : IInboxSummaryContributor, ITransientDependency
{
    private readonly IRepository<Memo, Guid> _memos;
    private readonly RequestDeciderReader _deciders;
    private readonly IAsyncQueryableExecuter _executer;

    public MemoInboxSummaryContributor(IRepository<Memo, Guid> memos, RequestDeciderReader deciders, IAsyncQueryableExecuter executer)
    {
        _memos = memos;
        _deciders = deciders;
        _executer = executer;
    }

    public virtual async Task<IReadOnlyCollection<InboxSummaryItemDto>> GetAsync(Guid userId)
    {
        var decider = await _deciders.GetForMemosAndIncidentsAsync(userId);
        var memos = await _memos.GetQueryableAsync();

        var total = await _executer.CountAsync(MemoInboxQuery.WaitingFor(memos, decider));
        var direct = await _executer.CountAsync(MemoInboxQuery.WaitingFor(
            memos,
            new RequestDeciderContext(decider.UserId, decider.Roles, new Dictionary<Guid, IReadOnlyCollection<string>>())));

        return new[] { new InboxSummaryItemDto { Category = InboxCategory.Memo, Count = total, DelegatedCount = total - direct } };
    }
}

/// <summary>A manager's action is forwarding. HR's work continues until the incident is resolved.</summary>
public class IncidentInboxSummaryContributor : IInboxSummaryContributor, ITransientDependency
{
    private readonly IRepository<Incident, Guid> _incidents;
    private readonly RequestDeciderReader _deciders;
    private readonly IAsyncQueryableExecuter _executer;

    public IncidentInboxSummaryContributor(IRepository<Incident, Guid> incidents, RequestDeciderReader deciders, IAsyncQueryableExecuter executer)
    {
        _incidents = incidents;
        _deciders = deciders;
        _executer = executer;
    }

    public virtual async Task<IReadOnlyCollection<InboxSummaryItemDto>> GetAsync(Guid userId)
    {
        var decider = await _deciders.GetForMemosAndIncidentsAsync(userId);
        var incidents = await _incidents.GetQueryableAsync();

        var direct = await _executer.CountAsync(Waiting(incidents, userId, decider.Roles.Contains(HrRoles.HrManager), new List<Guid>()));
        var total = await _executer.CountAsync(Waiting(
            incidents,
            userId,
            decider.Roles.Contains(HrRoles.HrManager) || decider.Delegators.Values.Any(x => x.Contains(HrRoles.HrManager)),
            decider.Delegators.Keys.ToList()));

        return new[] { new InboxSummaryItemDto { Category = InboxCategory.Incident, Count = total, DelegatedCount = total - direct } };
    }

    private static IQueryable<Incident> Waiting(IQueryable<Incident> query, Guid userId, bool hr, List<Guid> delegatorIds) =>
        query.Where(x =>
            (hr && (x.Status == IncidentStatus.PendingHr || x.Status == IncidentStatus.InProgress)) ||
            (x.Status == IncidentStatus.PendingManager &&
             x.SubmitterUserId != userId &&
             (x.CurrentManagerUserId == userId || (x.CurrentManagerUserId != null && delegatorIds.Contains(x.CurrentManagerUserId.Value)))));
}
