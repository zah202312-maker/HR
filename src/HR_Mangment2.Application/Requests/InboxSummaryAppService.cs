using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Users;

namespace HR_Mangment2.Requests;

/// <summary>Each module that adds a kind of pending work adds a contributor. Complaints never have one: the summary must not reveal them.</summary>
public interface IInboxSummaryContributor
{
    Task<IReadOnlyCollection<InboxSummaryItemDto>> GetAsync(Guid userId);
}

public class RequestInboxSummaryContributor : IInboxSummaryContributor, ITransientDependency
{
    private readonly IRepository<Request, Guid> _requests;
    private readonly RequestDeciderReader _deciders;
    private readonly IAsyncQueryableExecuter _executer;

    public RequestInboxSummaryContributor(IRepository<Request, Guid> requests, RequestDeciderReader deciders, IAsyncQueryableExecuter executer)
    {
        _requests = requests;
        _deciders = deciders;
        _executer = executer;
    }

    public virtual async Task<IReadOnlyCollection<InboxSummaryItemDto>> GetAsync(Guid userId)
    {
        var decider = await _deciders.GetAsync(userId);
        var requests = await _requests.GetQueryableAsync();

        var total = RequestInboxQuery.Apply(requests, decider);
        var direct = RequestInboxQuery.ApplyDirectOnly(requests, decider);

        var stockTotal = await CountStockAsync(total);
        var stockDirect = await CountStockAsync(direct);
        var requestTotal = await _executer.CountAsync(total) - stockTotal;
        var requestDirect = await _executer.CountAsync(direct) - stockDirect;

        return new[]
        {
            new InboxSummaryItemDto { Category = InboxCategory.Request, Count = requestTotal, DelegatedCount = requestTotal - requestDirect },
            new InboxSummaryItemDto { Category = InboxCategory.StockIssue, Count = stockTotal, DelegatedCount = stockTotal - stockDirect }
        };
    }

    private Task<int> CountStockAsync(IQueryable<Request> query) =>
        _executer.CountAsync(query.Where(x => x.Destination == RequestDestination.Store && x.Status == RequestStatus.PendingDestination));
}

[Authorize]
public class InboxSummaryAppService : HR_Mangment2AppService, IInboxSummaryAppService
{
    private readonly IEnumerable<IInboxSummaryContributor> _contributors;

    public InboxSummaryAppService(IEnumerable<IInboxSummaryContributor> contributors)
    {
        _contributors = contributors;
    }

    public virtual async Task<InboxSummaryDto> GetAsync()
    {
        var items = new List<InboxSummaryItemDto>();
        foreach (var contributor in _contributors)
        {
            items.AddRange((await contributor.GetAsync(CurrentUser.GetId())).Where(x => x.Count > 0));
        }

        return new InboxSummaryDto { TotalCount = items.Sum(x => x.Count), Items = items };
    }
}
