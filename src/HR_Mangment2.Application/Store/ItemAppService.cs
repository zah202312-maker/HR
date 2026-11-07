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
        ["kind"] = nameof(Item.Kind),
        ["minQuantity"] = nameof(Item.MinQuantity),
        ["quantityOnHand"] = nameof(Item.QuantityOnHand),
        ["creationTime"] = nameof(Item.CreationTime)
    };

    private readonly IRepository<Item, Guid> _items;
    private readonly IRepository<StockMovement, Guid> _movements;
    private readonly IRepository<StockIssue, Guid> _issues;
    private readonly IRepository<Custody, Guid> _custodies;
    private readonly IdempotencyStore _idempotency;

    public ItemAppService(
        IRepository<Item, Guid> items,
        IRepository<StockMovement, Guid> movements,
        IRepository<StockIssue, Guid> issues,
        IRepository<Custody, Guid> custodies,
        IdempotencyStore idempotency)
    {
        _items = items;
        _movements = movements;
        _issues = issues;
        _custodies = custodies;
        _idempotency = idempotency;
    }

    [Authorize(HrPermissions.StorePermissions.ItemView)]
    public virtual async Task<ItemPagedResultDto> GetListAsync(ItemListInput input)
    {
        var query = await _items.GetQueryableAsync();
        query = query
            .WhereIf(input.Kind != null, x => x.Kind == input.Kind)
            .WhereIf(input.IsActive != null, x => x.IsActive == input.IsActive)
            .WhereIf(input.BelowMinimum == true, x => x.QuantityOnHand < x.MinQuantity)
            .WhereIf(input.BelowMinimum == false, x => x.QuantityOnHand >= x.MinQuantity)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.NameAr.Contains(input.Filter!) || x.NameEn.Contains(input.Filter!));

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(Item.NameAr), SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new ItemPagedResultDto { TotalCount = total, Items = items.Select(ToDto).ToList() };
    }

    [Authorize(HrPermissions.StorePermissions.ItemManage)]
    public virtual async Task<ItemDto> CreateAsync(ItemCreateInputDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return ToDto(await _items.GetAsync(earlier.Value));
        }

        await CheckNamesAreFreeAsync(input.NameAr, input.NameEn, null);

        var item = new Item(GuidGenerator.Create(), CurrentTenant.Id, input.NameAr, input.NameEn, input.Kind!.Value, input.MinQuantity!.Value);
        await _items.InsertAsync(item, autoSave: true);
        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, input, item.Id);

        return ToDto(item);
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
    public virtual async Task<ItemDto> GetAsync(Guid id) => ToDto(await _items.GetAsync(id));

    [Authorize(HrPermissions.StorePermissions.ItemManage)]
    public virtual async Task<ItemDto> UpdateAsync(Guid id, ItemUpdateInputDto input)
    {
        var item = await _items.GetAsync(id);
        if (item.Kind != input.Kind && await HasHistoryAsync(id))
        {
            throw new BusinessException(HrErrorCodes.Item.KindLocked);
        }

        await CheckNamesAreFreeAsync(input.NameAr, input.NameEn, id);

        item.ConcurrencyStamp = input.ConcurrencyStamp;
        item.Set(input.NameAr, input.NameEn, input.Kind!.Value, input.MinQuantity!.Value, input.IsActive!.Value);
        await _items.UpdateAsync(item, autoSave: true);

        return ToDto(item);
    }

    /// <summary>An issue or a direct custody that deducts nothing also fixes what the item is, even with no movement yet.</summary>
    private async Task<bool> HasHistoryAsync(Guid id) =>
        await _movements.AnyAsync(x => x.ItemId == id) ||
        await _issues.AnyAsync(x => x.ItemId == id) ||
        await _custodies.AnyAsync(x => x.ItemId == id);

    private async Task CheckNamesAreFreeAsync(string nameAr, string nameEn, Guid? exceptId)
    {
        if (await _items.AnyAsync(x => (x.NameAr == nameAr || x.NameEn == nameEn) && x.Id != exceptId))
        {
            throw new BusinessException(HrErrorCodes.Item.NameAlreadyExists);
        }
    }

    private static ItemDto ToDto(Item item) => new()
    {
        Id = item.Id,
        NameAr = item.NameAr,
        NameEn = item.NameEn,
        Kind = item.Kind,
        MinQuantity = item.MinQuantity,
        QuantityOnHand = item.QuantityOnHand,
        IsBelowMinimum = item.IsBelowMinimum,
        IsActive = item.IsActive,
        ConcurrencyStamp = item.ConcurrencyStamp,
        CreationTime = item.CreationTime,
        LastModificationTime = item.LastModificationTime
    };
}

public abstract class ItemInputValidatorBase<T> : AbstractValidator<T> where T : ItemCreateInputDto
{
    protected ItemInputValidatorBase()
    {
        RuleFor(x => x.NameAr).NotNull().Length(1, StoreConsts.MaxNameLength);
        RuleFor(x => x.NameEn).NotNull().Length(1, StoreConsts.MaxNameLength);
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
