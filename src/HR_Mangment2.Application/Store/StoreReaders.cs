using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Localization;
using HR_Mangment2.Requests;
using Microsoft.Extensions.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace HR_Mangment2.Store;

public class ItemReferenceReader : ITransientDependency
{
    private readonly IRepository<Item, Guid> _items;

    public ItemReferenceReader(IRepository<Item, Guid> items)
    {
        _items = items;
    }

    public static ItemRefDto ToRef(Item item) => new()
    {
        Id = item.Id,
        Name = LocalizedName.Pick(item.NameAr, item.NameEn),
        Kind = item.Kind
    };

    public virtual async Task<Dictionary<Guid, Item>> GetAsync(IEnumerable<Guid> ids)
    {
        var distinct = ids.Distinct().ToList();
        return distinct.Count == 0
            ? new Dictionary<Guid, Item>()
            : (await _items.GetListAsync(x => distinct.Contains(x.Id))).ToDictionary(x => x.Id);
    }

    public virtual async Task<Dictionary<Guid, ItemRefDto>> GetRefsAsync(IEnumerable<Guid> ids) =>
        (await GetAsync(ids)).ToDictionary(x => x.Key, x => ToRef(x.Value));
}

public class StockIssueStatusReader : IStockIssueStatusReader, ITransientDependency
{
    private readonly IRepository<StockIssue, Guid> _issues;
    private readonly ItemReferenceReader _items;
    private readonly IStringLocalizer<HR_Mangment2Resource> _l;

    public StockIssueStatusReader(IRepository<StockIssue, Guid> issues, ItemReferenceReader items, IStringLocalizer<HR_Mangment2Resource> l)
    {
        _issues = issues;
        _items = items;
        _l = l;
    }

    public virtual async Task<Dictionary<Guid, StockIssueStatusDto>> GetByRequestIdsAsync(IReadOnlyCollection<Guid> requestIds)
    {
        var issues = await _issues.GetListAsync(x => requestIds.Contains(x.RequestId));
        var items = await _items.GetRefsAsync(issues.Select(x => x.ItemId));

        return issues.ToDictionary(x => x.RequestId, x => new StockIssueStatusDto
        {
            Id = x.Id,
            Item = items[x.ItemId],
            Quantity = x.Quantity,
            Status = x.Status,
            StatusLabel = _l[StoreLabels.IssueStatus(x.Status)],
            DeliveredAt = x.DeliveredAt,
            CustodyId = x.CustodyId
        });
    }
}

public static class StoreLabels
{
    public static string IssueStatus(StockIssueStatus status) =>
        "Stock:IssueStatus:" + System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(status.ToString());

    public static string CustodyStatusKey(CustodyStatus status) =>
        "Custody:Status:" + System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(status.ToString());
}
