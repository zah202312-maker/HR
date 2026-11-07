using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.Departments;

public class DepartmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int EmployeeCount { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class DepartmentPagedResultDto
{
    public long TotalCount { get; set; }
    public List<DepartmentDto> Items { get; set; } = new();
}

public class DepartmentRefDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

public class DepartmentLookupPagedResultDto
{
    public long TotalCount { get; set; }
    public List<DepartmentRefDto> Items { get; set; } = new();
}

public class DepartmentCreateDto
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
}

public class DepartmentUpdateDto
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string ConcurrencyStamp { get; set; } = null!;
}

public class DepartmentListInput : PagedAndSortedResultRequestDto
{
    public string? Search { get; set; }
}

public interface IDepartmentAppService : IApplicationService
{
    Task<DepartmentPagedResultDto> GetListAsync(DepartmentListInput input);
    Task<DepartmentDto> CreateAsync(DepartmentCreateDto input);
    Task<DepartmentLookupPagedResultDto> LookupAsync(DepartmentListInput input);
    Task<DepartmentDto> GetAsync(Guid id);
    Task<DepartmentDto> UpdateAsync(Guid id, DepartmentUpdateDto input);
    Task DeleteAsync(Guid id);
}
