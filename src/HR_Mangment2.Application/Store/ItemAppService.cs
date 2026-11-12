using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Errors;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Localization;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace HR_Mangment2.Store;

[Authorize]
public class ItemAppService : HR_Mangment2AppService, IItemAppService
{
    private const string CreateOperation = "Item_Create";

    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["nameAr"] = nameof(Item.NameAr),
        ["nameEn"] = nameof(Item.NameEn),
        ["sku"] = nameof(Item.Sku),
        ["kind"] = nameof(Item.Kind),
        ["minQuantity"] = nameof(Item.MinQuantity),
        ["quantityOnHand"] = nameof(Item.QuantityOnHand),
        ["averageUnitCost"] = nameof(Item.AverageUnitCost),
        ["creationTime"] = nameof(Item.CreationTime)
    };

    private readonly IRepository<Item, Guid> _items;
    private readonly IRepository<StockMovement, Guid> _movements;
    private readonly IRepository<StockIssue, Guid> _issues;
    private readonly IRepository<Custody, Guid> _custodies;
    private readonly IRepository<ItemCategory, Guid> _categories;
    private readonly IRepository<ItemUnit, Guid> _units;
    private readonly IRepository<StorePurchaseLine, Guid> _purchaseLines;
    private readonly IRepository<StoreIssueVoucherLine, Guid> _voucherLines;
    private readonly StoreReferenceReader _storeRefs;
    private readonly IdempotencyStore _idempotency;

    public ItemAppService(
        IRepository<Item, Guid> items,
        IRepository<StockMovement, Guid> movements,
        IRepository<StockIssue, Guid> issues,
        IRepository<Custody, Guid> custodies,
        IRepository<ItemCategory, Guid> categories,
        IRepository<ItemUnit, Guid> units,
        IRepository<StorePurchaseLine, Guid> purchaseLines,
        IRepository<StoreIssueVoucherLine, Guid> voucherLines,
        StoreReferenceReader storeRefs,
        IdempotencyStore idempotency)
    {
        _items = items;
        _movements = movements;
        _issues = issues;
        _custodies = custodies;
        _categories = categories;
        _units = units;
        _purchaseLines = purchaseLines;
        _voucherLines = voucherLines;
        _storeRefs = storeRefs;
        _idempotency = idempotency;
    }

    [Authorize(HrPermissions.StorePermissions.ItemView)]
    public virtual async Task<ItemPagedResultDto> GetListAsync(ItemListInput input)
    {
        var query = (await _items.GetQueryableAsync())
            .WhereIf(input.Kind != null, x => x.Kind == input.Kind)
            .WhereIf(input.IsActive != null, x => x.IsActive == input.IsActive)
            .WhereIf(input.BelowMinimum == true, x => x.QuantityOnHand < x.MinQuantity)
            .WhereIf(input.BelowMinimum == false, x => x.QuantityOnHand >= x.MinQuantity)
            .WhereIf(input.CategoryId != null, x => x.CategoryId == input.CategoryId)
            .WhereStatus(input.StockStatus)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.NameAr.Contains(input.Filter!) || x.NameEn.Contains(input.Filter!) || x.Sku.Contains(input.Filter!));

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(Item.NameAr), SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new ItemPagedResultDto { TotalCount = total, Items = await ToDtosAsync(items) };
    }

    [Authorize(HrPermissions.StorePermissions.ItemManage)]
    public virtual async Task<ItemDto> CreateAsync(ItemCreateInputDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return await ToDtoAsync(await _items.GetAsync(earlier.Value));
        }

        var sku = input.Sku.Trim();
        await CheckNamesAreFreeAsync(input.NameAr, input.NameEn, sku, null);
        await CheckCatalogAsync(input.CategoryId!.Value, input.UnitId!.Value, null);

        var item = new Item(GuidGenerator.Create(), CurrentTenant.Id, input.NameAr, input.NameEn, sku, input.CategoryId.Value, input.UnitId.Value, input.Kind!.Value, input.MinQuantity!.Value);
        await _items.InsertAsync(item, autoSave: true);
        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, input, item.Id);

        return await ToDtoAsync(item);
    }

    /// <summary>Active items without quantities, for choosing an item on a request or incident.</summary>
    [Authorize(HrPermissions.StorePermissions.ItemLookup)]
    public virtual async Task<ItemRefPagedResultDto> LookupAsync(ItemLookupInput input)
    {
        var query = (await _items.GetQueryableAsync())
            .Where(x => x.IsActive)
            .WhereIf(input.Kind != null, x => x.Kind == input.Kind)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.NameAr.Contains(input.Filter!) || x.NameEn.Contains(input.Filter!));

        var total = await AsyncExecuter.LongCountAsync(query);
        var defaultSort = LocalizedName.IsEnglish ? nameof(Item.NameEn) : nameof(Item.NameAr);
        var lookupFields = new Dictionary<string, string> { ["name"] = defaultSort };
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, defaultSort, lookupFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new ItemRefPagedResultDto { TotalCount = total, Items = items.Select(ItemReferenceReader.ToRef).ToList() };
    }

    [Authorize(HrPermissions.StorePermissions.ItemView)]
    public virtual async Task<ItemDto> GetAsync(Guid id) => await ToDtoAsync(await _items.GetAsync(id));

    [Authorize(HrPermissions.StorePermissions.ItemManage)]
    public virtual async Task<ItemDto> UpdateAsync(Guid id, ItemUpdateInputDto input)
    {
        var item = await _items.GetAsync(id);
        if (item.Kind != input.Kind && await HasHistoryAsync(id))
        {
            throw new BusinessException(HrErrorCodes.Item.KindLocked);
        }

        var sku = input.Sku.Trim();
        await CheckNamesAreFreeAsync(input.NameAr, input.NameEn, sku, id);
        await CheckCatalogAsync(input.CategoryId!.Value, input.UnitId!.Value, item);

        item.ConcurrencyStamp = input.ConcurrencyStamp;
        item.Set(input.NameAr, input.NameEn, sku, input.CategoryId.Value, input.UnitId.Value, input.Kind!.Value, input.MinQuantity!.Value, input.IsActive!.Value);
        await _items.UpdateAsync(item, autoSave: true);

        return await ToDtoAsync(item);
    }

    /// <summary>Only an item nothing points to. One with history is archived with isActive = false instead.</summary>
    [Authorize(HrPermissions.StorePermissions.ItemManage)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var item = await _items.GetAsync(id);
        if (await HasHistoryAsync(id) || await _purchaseLines.AnyAsync(x => x.ItemId == id) || await _voucherLines.AnyAsync(x => x.ItemId == id))
        {
            throw new BusinessException(HrErrorCodes.Item.HasMovements);
        }

        await _items.DeleteAsync(item, autoSave: true);
    }

    /// <summary>An issue or a direct custody that deducts nothing also fixes what the item is, even with no movement yet.</summary>
    private async Task<bool> HasHistoryAsync(Guid id) =>
        await _movements.AnyAsync(x => x.ItemId == id) ||
        await _issues.AnyAsync(x => x.ItemId == id) ||
        await _custodies.AnyAsync(x => x.ItemId == id);

    /// <summary>An inactive category or unit is refused, unless the item already had it.</summary>
    private async Task CheckCatalogAsync(Guid categoryId, Guid unitId, Item? current)
    {
        if (current?.CategoryId != categoryId && !await _categories.AnyAsync(x => x.Id == categoryId && x.IsActive))
        {
            throw new BusinessException(HrErrorCodes.Item.CategoryInactive);
        }

        if (current?.UnitId != unitId && !await _units.AnyAsync(x => x.Id == unitId && x.IsActive))
        {
            throw new BusinessException(HrErrorCodes.Item.UnitInactive);
        }
    }

    private async Task CheckNamesAreFreeAsync(string nameAr, string nameEn, string sku, Guid? exceptId)
    {
        if (await _items.AnyAsync(x => (x.NameAr == nameAr || x.NameEn == nameEn) && x.Id != exceptId))
        {
            throw new BusinessException(HrErrorCodes.Item.NameAlreadyExists);
        }

        if (await _items.AnyAsync(x => x.Sku == sku && x.Id != exceptId))
        {
            throw new BusinessException(HrErrorCodes.Item.SkuAlreadyExists);
        }
    }

    private async Task<ItemDto> ToDtoAsync(Item item) => (await ToDtosAsync(new[] { item })).Single();

    private async Task<List<ItemDto>> ToDtosAsync(IReadOnlyCollection<Item> items)
    {
        var categories = await _storeRefs.GetCategoriesAsync(items.Select(x => x.CategoryId));
        var units = await _storeRefs.GetUnitsAsync(items.Select(x => x.UnitId));
        return items.Select(item => new ItemDto
        {
            Id = item.Id,
            NameAr = item.NameAr,
            NameEn = item.NameEn,
            Sku = item.Sku,
            Category = StoreReferenceReader.Pick(categories, item.CategoryId),
            Unit = StoreReferenceReader.Pick(units, item.UnitId),
            Kind = item.Kind,
            MinQuantity = item.MinQuantity,
            QuantityOnHand = item.QuantityOnHand,
            IsBelowMinimum = item.IsBelowMinimum,
            AverageUnitCost = item.AverageUnitCost,
            StockValue = item.StockValue,
            StockStatus = item.StockStatus,
            IsActive = item.IsActive,
            ConcurrencyStamp = item.ConcurrencyStamp,
            CreationTime = item.CreationTime,
            LastModificationTime = item.LastModificationTime
        }).ToList();
    }
}

public static class ItemQueryExtensions
{
    /// <summary>The same thresholds as <see cref="Item.StatusOf"/>, in SQL.</summary>
    public static IQueryable<Item> WhereStatus(this IQueryable<Item> query, StockStatus? status) => status switch
    {
        StockStatus.OutOfStock => query.Where(x => x.QuantityOnHand <= 0),
        StockStatus.LowStock => query.Where(x => x.QuantityOnHand > 0 && x.QuantityOnHand < x.MinQuantity),
        StockStatus.InStock => query.Where(x => x.QuantityOnHand > 0 && x.QuantityOnHand >= x.MinQuantity),
        _ => query
    };
}

public abstract class ItemInputValidatorBase<T> : AbstractValidator<T> where T : ItemCreateInputDto
{
    protected ItemInputValidatorBase()
    {
        RuleFor(x => x.NameAr).NotNull().Length(1, StoreConsts.MaxNameLength);
        RuleFor(x => x.NameEn).NotNull().Length(1, StoreConsts.MaxNameLength);
        RuleFor(x => x.Sku).NotNull().Must(x => !string.IsNullOrWhiteSpace(x)).MaximumLength(StoreConsts.MaxSkuLength);
        RuleFor(x => x.CategoryId).NotNull();
        RuleFor(x => x.UnitId).NotNull();
        RuleFor(x => x.Kind).NotNull();
        RuleFor(x => x.MinQuantity).NotNull().GreaterThanOrEqualTo(0).PrecisionScale(StoreConsts.QuantityPrecision, StoreConsts.QuantityScale, true);
    }
}

public class ItemCreateInputDtoValidator : ItemInputValidatorBase<ItemCreateInputDto>
{
}

public class ItemUpdateInputDtoValidator : ItemInputValidatorBase<ItemUpdateInputDto>
{
    public ItemUpdateInputDtoValidator()
    {
        RuleFor(x => x.IsActive).NotNull();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}
