using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Departments;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Localization;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace HR_Mangment2.Store;

/// <summary>Names of categories, units and teams for the store DTOs, in the request language.</summary>
public class StoreReferenceReader : ITransientDependency
{
    public const string DefaultCurrency = "IQD";

    private readonly IRepository<ItemCategory, Guid> _categories;
    private readonly IRepository<ItemUnit, Guid> _units;
    private readonly IRepository<StoreTeam, Guid> _teams;
    private readonly IConfiguration _configuration;

    public StoreReferenceReader(IRepository<ItemCategory, Guid> categories, IRepository<ItemUnit, Guid> units, IRepository<StoreTeam, Guid> teams, IConfiguration configuration)
    {
        _categories = categories;
        _units = units;
        _teams = teams;
        _configuration = configuration;
    }

    /// <summary>The company currency the accountant's expenses use too.</summary>
    public string Currency => _configuration["Finance:DefaultCurrency"] ?? DefaultCurrency;

    public static StoreLookupRefDto ToRef(StoreCatalogEntry entry) => new() { Id = entry.Id, Name = LocalizedName.Pick(entry.NameAr, entry.NameEn) };

    public virtual async Task<Dictionary<Guid, StoreLookupRefDto>> GetCategoriesAsync(IEnumerable<Guid?> ids)
    {
        var distinct = ids.Where(x => x != null).Select(x => x!.Value).Distinct().ToList();
        return distinct.Count == 0 ? new() : (await _categories.GetListAsync(x => distinct.Contains(x.Id))).ToDictionary(x => x.Id, ToRef);
    }

    public virtual async Task<Dictionary<Guid, StoreLookupRefDto>> GetUnitsAsync(IEnumerable<Guid?> ids)
    {
        var distinct = ids.Where(x => x != null).Select(x => x!.Value).Distinct().ToList();
        return distinct.Count == 0 ? new() : (await _units.GetListAsync(x => distinct.Contains(x.Id))).ToDictionary(x => x.Id, ToRef);
    }

    public virtual async Task<Dictionary<Guid, StoreLookupRefDto>> GetTeamsAsync(IEnumerable<Guid?> ids)
    {
        var distinct = ids.Where(x => x != null).Select(x => x!.Value).Distinct().ToList();
        return distinct.Count == 0
            ? new()
            : (await _teams.GetListAsync(x => distinct.Contains(x.Id))).ToDictionary(x => x.Id, x => new StoreLookupRefDto { Id = x.Id, Name = x.Name });
    }

    public static StoreLookupRefDto? Pick(Dictionary<Guid, StoreLookupRefDto> refs, Guid? id) =>
        id != null && refs.TryGetValue(id.Value, out var found) ? found : null;
}

/// <summary>Categories and units share one shape and one set of rules: unique names, no delete.</summary>
public abstract class StoreCatalogAppServiceBase<TEntry> : HR_Mangment2AppService where TEntry : StoreCatalogEntry
{
    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["nameAr"] = nameof(StoreCatalogEntry.NameAr),
        ["nameEn"] = nameof(StoreCatalogEntry.NameEn),
        ["creationTime"] = nameof(StoreCatalogEntry.CreationTime)
    };

    protected IRepository<TEntry, Guid> Entries { get; }
    private readonly IdempotencyStore _idempotency;

    protected StoreCatalogAppServiceBase(IRepository<TEntry, Guid> entries, IdempotencyStore idempotency)
    {
        Entries = entries;
        _idempotency = idempotency;
    }

    protected abstract string CreateOperation { get; }

    protected abstract string NameTakenCode { get; }

    protected abstract TEntry New(Guid id, string nameAr, string nameEn);

    protected async Task<(long Total, List<TEntry> Rows)> ListAsync(StoreCatalogListInput input)
    {
        var query = (await Entries.GetQueryableAsync())
            .WhereIf(input.IsActive != null, x => x.IsActive == input.IsActive)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.NameAr.Contains(input.Filter!) || x.NameEn.Contains(input.Filter!));

        var total = await AsyncExecuter.LongCountAsync(query);
        var defaultSort = LocalizedName.IsEnglish ? nameof(StoreCatalogEntry.NameEn) : nameof(StoreCatalogEntry.NameAr);
        var rows = await AsyncExecuter.ToListAsync(query.ApplySorting(input.Sorting, defaultSort, SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));
        return (total, rows);
    }

    protected async Task<TEntry> CreateEntryAsync(StoreCatalogEntryCreateInputDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return await Entries.GetAsync(earlier.Value);
        }

        var nameAr = input.NameAr.Trim();
        var nameEn = input.NameEn.Trim();
        await CheckNamesAreFreeAsync(nameAr, nameEn, null);

        var entry = await Entries.InsertAsync(New(GuidGenerator.Create(), nameAr, nameEn), autoSave: true);
        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, input, entry.Id);
        return entry;
    }

    protected async Task<TEntry> UpdateEntryAsync(Guid id, StoreCatalogEntryUpdateInputDto input)
    {
        var entry = await Entries.GetAsync(id);
        var nameAr = input.NameAr.Trim();
        var nameEn = input.NameEn.Trim();
        await CheckNamesAreFreeAsync(nameAr, nameEn, id);

        entry.ConcurrencyStamp = input.ConcurrencyStamp;
        entry.Set(nameAr, nameEn, input.IsActive!.Value);
        await Entries.UpdateAsync(entry, autoSave: true);
        return entry;
    }

    private async Task CheckNamesAreFreeAsync(string nameAr, string nameEn, Guid? exceptId)
    {
        if (await Entries.AnyAsync(x => (x.NameAr == nameAr || x.NameEn == nameEn) && x.Id != exceptId))
        {
            throw new BusinessException(NameTakenCode);
        }
    }

    protected static T Map<T>(StoreCatalogEntry entry) where T : StoreCatalogEntryDto, new() => new()
    {
        Id = entry.Id,
        NameAr = entry.NameAr,
        NameEn = entry.NameEn,
        IsActive = entry.IsActive,
        ConcurrencyStamp = entry.ConcurrencyStamp,
        CreationTime = entry.CreationTime
    };
}

[Authorize]
public class ItemCategoryAppService : StoreCatalogAppServiceBase<ItemCategory>, IItemCategoryAppService
{
    public ItemCategoryAppService(IRepository<ItemCategory, Guid> entries, IdempotencyStore idempotency)
        : base(entries, idempotency)
    {
    }

    protected override string CreateOperation => "ItemCategory_Create";

    protected override string NameTakenCode => HrErrorCodes.ItemCategory.NameAlreadyExists;

    protected override ItemCategory New(Guid id, string nameAr, string nameEn) => new(id, CurrentTenant.Id, nameAr, nameEn);

    [Authorize(HrPermissions.StorePermissions.ItemView)]
    public virtual async Task<ItemCategoryPagedResultDto> GetListAsync(StoreCatalogListInput input)
    {
        var (total, rows) = await ListAsync(input);
        return new ItemCategoryPagedResultDto { TotalCount = total, Items = rows.Select(Map<ItemCategoryDto>).ToList() };
    }

    [Authorize(HrPermissions.StorePermissions.ItemManage)]
    public virtual async Task<ItemCategoryDto> CreateAsync(ItemCategoryCreateInputDto input, Guid? idempotencyKey) =>
        Map<ItemCategoryDto>(await CreateEntryAsync(input, idempotencyKey));

    [Authorize(HrPermissions.StorePermissions.ItemManage)]
    public virtual async Task<ItemCategoryDto> UpdateAsync(Guid id, ItemCategoryUpdateInputDto input) =>
        Map<ItemCategoryDto>(await UpdateEntryAsync(id, input));
}

[Authorize]
public class ItemUnitAppService : StoreCatalogAppServiceBase<ItemUnit>, IItemUnitAppService
{
    public ItemUnitAppService(IRepository<ItemUnit, Guid> entries, IdempotencyStore idempotency)
        : base(entries, idempotency)
    {
    }

    protected override string CreateOperation => "ItemUnit_Create";

    protected override string NameTakenCode => HrErrorCodes.ItemUnit.NameAlreadyExists;

    protected override ItemUnit New(Guid id, string nameAr, string nameEn) => new(id, CurrentTenant.Id, nameAr, nameEn);

    [Authorize(HrPermissions.StorePermissions.ItemView)]
    public virtual async Task<ItemUnitPagedResultDto> GetListAsync(StoreCatalogListInput input)
    {
        var (total, rows) = await ListAsync(input);
        return new ItemUnitPagedResultDto { TotalCount = total, Items = rows.Select(Map<ItemUnitDto>).ToList() };
    }

    [Authorize(HrPermissions.StorePermissions.ItemManage)]
    public virtual async Task<ItemUnitDto> CreateAsync(ItemUnitCreateInputDto input, Guid? idempotencyKey) =>
        Map<ItemUnitDto>(await CreateEntryAsync(input, idempotencyKey));

    [Authorize(HrPermissions.StorePermissions.ItemManage)]
    public virtual async Task<ItemUnitDto> UpdateAsync(Guid id, ItemUnitUpdateInputDto input) =>
        Map<ItemUnitDto>(await UpdateEntryAsync(id, input));
}

[Authorize]
public class StoreTeamAppService : HR_Mangment2AppService, IStoreTeamAppService
{
    private const string CreateOperation = "StoreTeam_Create";

    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["name"] = nameof(StoreTeam.Name),
        ["creationTime"] = nameof(StoreTeam.CreationTime)
    };

    private readonly IRepository<StoreTeam, Guid> _teams;
    private readonly IRepository<Department, Guid> _departments;
    private readonly EmployeeReferenceReader _references;
    private readonly IdempotencyStore _idempotency;

    public StoreTeamAppService(IRepository<StoreTeam, Guid> teams, IRepository<Department, Guid> departments, EmployeeReferenceReader references, IdempotencyStore idempotency)
    {
        _teams = teams;
        _departments = departments;
        _references = references;
        _idempotency = idempotency;
    }

    [Authorize(HrPermissions.StorePermissions.TeamView)]
    public virtual async Task<StoreTeamPagedResultDto> GetListAsync(StoreCatalogListInput input)
    {
        var query = (await _teams.GetQueryableAsync())
            .WhereIf(input.IsActive != null, x => x.IsActive == input.IsActive)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Name.Contains(input.Filter!));

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.ApplySorting(input.Sorting, nameof(StoreTeam.Name), SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));
        return new StoreTeamPagedResultDto { TotalCount = total, Items = await ToDtosAsync(rows) };
    }

    [Authorize(HrPermissions.StorePermissions.TeamManage)]
    public virtual async Task<StoreTeamDto> CreateAsync(StoreTeamCreateInputDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return (await ToDtosAsync(new[] { await _teams.GetAsync(earlier.Value) })).Single();
        }

        var name = input.Name.Trim();
        await CheckAsync(name, input.DepartmentId, null);

        var team = await _teams.InsertAsync(new StoreTeam(GuidGenerator.Create(), CurrentTenant.Id, name, input.DepartmentId), autoSave: true);
        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, input, team.Id);
        return (await ToDtosAsync(new[] { team })).Single();
    }

    [Authorize(HrPermissions.StorePermissions.TeamManage)]
    public virtual async Task<StoreTeamDto> UpdateAsync(Guid id, StoreTeamUpdateInputDto input)
    {
        var team = await _teams.GetAsync(id);
        var name = input.Name.Trim();
        await CheckAsync(name, input.DepartmentId, id);

        team.ConcurrencyStamp = input.ConcurrencyStamp;
        team.Set(name, input.DepartmentId, input.IsActive!.Value);
        await _teams.UpdateAsync(team, autoSave: true);
        return (await ToDtosAsync(new[] { team })).Single();
    }

    private async Task CheckAsync(string name, Guid? departmentId, Guid? exceptId)
    {
        if (await _teams.AnyAsync(x => x.Name == name && x.Id != exceptId))
        {
            throw new BusinessException(HrErrorCodes.StoreTeam.NameAlreadyExists);
        }

        if (departmentId != null && !await _departments.AnyAsync(x => x.Id == departmentId))
        {
            throw new BusinessException(HrErrorCodes.StoreTeam.DepartmentInvalid);
        }
    }

    private async Task<List<StoreTeamDto>> ToDtosAsync(IReadOnlyCollection<StoreTeam> rows)
    {
        var departments = await _references.GetDepartmentsAsync(rows.Where(x => x.DepartmentId != null).Select(x => x.DepartmentId!.Value));
        return rows.Select(x => new StoreTeamDto
        {
            Id = x.Id,
            Name = x.Name,
            Department = x.DepartmentId == null ? null : departments.GetValueOrDefault(x.DepartmentId.Value),
            IsActive = x.IsActive,
            ConcurrencyStamp = x.ConcurrencyStamp,
            CreationTime = x.CreationTime,
            LastModificationTime = x.LastModificationTime
        }).ToList();
    }
}

public abstract class StoreCatalogCreateValidatorBase<T> : AbstractValidator<T> where T : StoreCatalogEntryCreateInputDto
{
    protected StoreCatalogCreateValidatorBase()
    {
        RuleFor(x => x.NameAr).NotNull().Must(x => !string.IsNullOrWhiteSpace(x)).MaximumLength(StoreConsts.MaxNameLength);
        RuleFor(x => x.NameEn).NotNull().Must(x => !string.IsNullOrWhiteSpace(x)).MaximumLength(StoreConsts.MaxNameLength);
    }
}

public class ItemCategoryCreateInputDtoValidator : StoreCatalogCreateValidatorBase<ItemCategoryCreateInputDto>
{
}

public class ItemUnitCreateInputDtoValidator : StoreCatalogCreateValidatorBase<ItemUnitCreateInputDto>
{
}

public class ItemCategoryUpdateInputDtoValidator : StoreCatalogCreateValidatorBase<ItemCategoryUpdateInputDto>
{
    public ItemCategoryUpdateInputDtoValidator()
    {
        RuleFor(x => x.IsActive).NotNull();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public class ItemUnitUpdateInputDtoValidator : StoreCatalogCreateValidatorBase<ItemUnitUpdateInputDto>
{
    public ItemUnitUpdateInputDtoValidator()
    {
        RuleFor(x => x.IsActive).NotNull();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public class StoreTeamCreateInputDtoValidator : AbstractValidator<StoreTeamCreateInputDto>
{
    public StoreTeamCreateInputDtoValidator()
    {
        RuleFor(x => x.Name).NotNull().Must(x => !string.IsNullOrWhiteSpace(x)).MaximumLength(StoreConsts.MaxNameLength);
    }
}

public class StoreTeamUpdateInputDtoValidator : AbstractValidator<StoreTeamUpdateInputDto>
{
    public StoreTeamUpdateInputDtoValidator()
    {
        RuleFor(x => x.Name).NotNull().Must(x => !string.IsNullOrWhiteSpace(x)).MaximumLength(StoreConsts.MaxNameLength);
        RuleFor(x => x.IsActive).NotNull();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}
