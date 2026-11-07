using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.Identity;

public class UserAccountDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string? Email { get; set; }
    public EmployeeRefDto? Employee { get; set; }
    public bool IsActive { get; set; }
    public bool IsLockedOut { get; set; }
    public bool MustChangePassword { get; set; }
    public bool MfaRequired { get; set; }
    public bool MfaEnabled { get; set; }
    public List<string> RoleNames { get; set; } = new();
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class UserAccountPagedResultDto
{
    public long TotalCount { get; set; }
    public List<UserAccountDto> Items { get; set; } = new();
}

public class UserAccountCreateDto
{
    public Guid? EmployeeId { get; set; }
    public string UserName { get; set; } = null!;
    public string? Email { get; set; }
    public List<string> RoleNames { get; set; } = new();
}

public class UserAccountCreatedDto
{
    public UserAccountDto Account { get; set; } = null!;
    public TemporaryCredentialsDto Credentials { get; set; } = null!;
}

public class UserRolesUpdateDto
{
    public List<string> RoleNames { get; set; } = new();
    public Guid? ManagerEmployeeId { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class PasswordResetResultDto
{
    public Guid UserId { get; set; }
    public PasswordResetDelivery DeliveredVia { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class UserAccountListInput : PagedAndSortedResultRequestDto
{
    public string? Search { get; set; }
    public string? RoleName { get; set; }
    public bool? IsActive { get; set; }
    public bool? MustChangePassword { get; set; }
}

public class UserAccountLookupInput : PagedResultRequestDto
{
    public string? Search { get; set; }
}

public interface IUserAccountAppService : IApplicationService
{
    Task<UserAccountPagedResultDto> GetListAsync(UserAccountListInput input);
    Task<UserAccountCreatedDto> CreateAsync(UserAccountCreateDto input);
    Task<UserRefPagedResultDto> LookupAsync(UserAccountLookupInput input);
    Task<UserAccountDto> GetAsync(Guid id);
    Task<PasswordResetResultDto> ResetPasswordAsync(Guid id, ReasonInputDto input);
    Task<UserAccountDto> SetRolesAsync(Guid id, UserRolesUpdateDto input);
    Task<PermissionListResultDto> GetPermissionsAsync(Guid id);
    Task<PermissionListResultDto> UpdatePermissionsAsync(Guid id, PermissionGrantUpdateDto input);
}
