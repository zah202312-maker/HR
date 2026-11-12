using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.Identity;

public interface IMyAccountAppService : IApplicationService
{
    Task<MyAccountDto> GetAsync();
    Task<MyAccountDto> UpdateAsync(MyAccountUpdateDto input);
    Task<ChangePasswordResultDto> ChangePasswordAsync(ChangePasswordInputDto input);
    Task<MyPhotoDto> UploadPhotoAsync(MyPhotoUploadInput input);
    Task DeletePhotoAsync();
    Task<ProfilePhotoContent> GetPhotoAsync(Guid photoId);
}

public interface IMfaAppService : IApplicationService
{
    Task<MfaStatusDto> GetStatusAsync();
    Task<MfaEnrollmentDto> StartEnrollmentAsync();
    Task<MfaRecoveryCodesDto> ConfirmEnrollmentAsync(MfaCodeInputDto input);
    Task<MfaRecoveryCodesDto> RegenerateRecoveryCodesAsync(MfaCodeInputDto input);
    Task ResetForUserAsync(Guid userId, ReasonInputDto input);
}

public class RoleListInput : PagedAndSortedResultRequestDto
{
    public string? Search { get; set; }
}

public interface IRoleAppService : IApplicationService
{
    Task<RolePagedResultDto> GetListAsync(RoleListInput input);
    Task<RoleDto> CreateAsync(RoleCreateDto input);
    Task<RoleDto> GetAsync(Guid id);
    Task<RoleDto> UpdateAsync(Guid id, RoleUpdateDto input);
    Task DeleteAsync(Guid id);
    Task<PermissionListResultDto> GetPermissionsAsync(Guid id);
    Task<PermissionListResultDto> UpdatePermissionsAsync(Guid id, PermissionGrantUpdateDto input);
}
