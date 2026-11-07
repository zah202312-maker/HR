using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Errors;
using HR_Mangment2.Localization;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace HR_Mangment2.Departments;

[Authorize]
public class DepartmentAppService : HR_Mangment2AppService, IDepartmentAppService
{
    private readonly IRepository<Department, Guid> _departments;
    private readonly IDepartmentUsageReader _usage;

    public DepartmentAppService(IRepository<Department, Guid> departments, IDepartmentUsageReader usage)
    {
        _departments = departments;
        _usage = usage;
    }

    private static string DefaultSorting => LocalizedName.IsEnglish ? nameof(Department.NameEn) : nameof(Department.NameAr);

    private static Dictionary<string, string> SortFields => new()
    {
        ["name"] = DefaultSorting,
        ["nameAr"] = nameof(Department.NameAr),
        ["nameEn"] = nameof(Department.NameEn),
        ["creationTime"] = nameof(Department.CreationTime),
        ["lastModificationTime"] = nameof(Department.LastModificationTime)
    };

    [Authorize(HrPermissions.DepartmentPermissions.Read)]
    public virtual async Task<DepartmentPagedResultDto> GetListAsync(DepartmentListInput input)
    {
        var (total, items) = await QueryPageAsync(input);
        var counts = await _usage.GetEmployeeCountsAsync(items.Select(x => x.Id).ToList());

        return new DepartmentPagedResultDto
        {
            TotalCount = total,
            Items = items.Select(x => ToDto(x, counts.GetValueOrDefault(x.Id))).ToList()
        };
    }

    [Authorize(HrPermissions.DepartmentPermissions.Manage)]
    public virtual async Task<DepartmentDto> CreateAsync(DepartmentCreateDto input)
    {
        var department = new Department(GuidGenerator.Create(), CurrentTenant.Id, input.NameAr, input.NameEn);
        await _departments.InsertAsync(department, autoSave: true);
        return ToDto(department, 0);
    }

    [Authorize(HrPermissions.DepartmentPermissions.Lookup)]
    public virtual async Task<DepartmentLookupPagedResultDto> LookupAsync(DepartmentListInput input)
    {
        var (total, items) = await QueryPageAsync(input);

        return new DepartmentLookupPagedResultDto
        {
            TotalCount = total,
            Items = items.Select(x => new DepartmentRefDto { Id = x.Id, Name = LocalizedName.Pick(x.NameAr, x.NameEn) }).ToList()
        };
    }

    [Authorize(HrPermissions.DepartmentPermissions.Read)]
    public virtual async Task<DepartmentDto> GetAsync(Guid id)
    {
        var department = await _departments.GetAsync(id);
        return ToDto(department, await GetEmployeeCountAsync(id));
    }

    [Authorize(HrPermissions.DepartmentPermissions.Manage)]
    public virtual async Task<DepartmentDto> UpdateAsync(Guid id, DepartmentUpdateDto input)
    {
        var department = await _departments.GetAsync(id);
        department.ConcurrencyStamp = input.ConcurrencyStamp;
        department.Rename(input.NameAr, input.NameEn);
        await _departments.UpdateAsync(department, autoSave: true);
        return ToDto(department, await GetEmployeeCountAsync(id));
    }

    [Authorize(HrPermissions.DepartmentPermissions.Manage)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var department = await _departments.GetAsync(id);

        if (await _usage.HasEmployeesAsync(id))
        {
            throw new BusinessException(HrErrorCodes.Department.HasEmployees);
        }

        if (await _usage.HasSharedCustodyAsync(id))
        {
            throw new BusinessException(HrErrorCodes.Department.HasSharedCustody);
        }

        await _departments.DeleteAsync(department);
    }

    private async Task<(long Total, List<Department> Items)> QueryPageAsync(DepartmentListInput input)
    {
        var query = await _departments.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var search = input.Search.Trim().ToLower();
            query = query.Where(x => x.NameAr.ToLower().Contains(search) || x.NameEn.ToLower().Contains(search));
        }

        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, DefaultSorting, SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        return (total, items);
    }

    private async Task<int> GetEmployeeCountAsync(Guid id) =>
        (await _usage.GetEmployeeCountsAsync(new[] { id })).GetValueOrDefault(id);

    private static DepartmentDto ToDto(Department department, int employeeCount) => new()
    {
        Id = department.Id,
        Name = LocalizedName.Pick(department.NameAr, department.NameEn),
        NameAr = department.NameAr,
        NameEn = department.NameEn,
        EmployeeCount = employeeCount,
        CreationTime = department.CreationTime,
        LastModificationTime = department.LastModificationTime,
        ConcurrencyStamp = department.ConcurrencyStamp
    };
}

public class DepartmentListInputValidator : AbstractValidator<DepartmentListInput>
{
    public DepartmentListInputValidator()
    {
        RuleFor(x => x.Search).MaximumLength(Department.MaxNameLength);
    }
}

public class DepartmentCreateDtoValidator : AbstractValidator<DepartmentCreateDto>
{
    public DepartmentCreateDtoValidator()
    {
        RuleFor(x => x.NameAr).NotNull().Length(1, Department.MaxNameLength);
        RuleFor(x => x.NameEn).NotNull().Length(1, Department.MaxNameLength);
    }
}

public class DepartmentUpdateDtoValidator : AbstractValidator<DepartmentUpdateDto>
{
    public DepartmentUpdateDtoValidator()
    {
        RuleFor(x => x.NameAr).NotNull().Length(1, Department.MaxNameLength);
        RuleFor(x => x.NameEn).NotNull().Length(1, Department.MaxNameLength);
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}
