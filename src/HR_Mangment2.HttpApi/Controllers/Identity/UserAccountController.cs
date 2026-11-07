using System;
using System.Threading.Tasks;
using HR_Mangment2.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HR_Mangment2.Controllers.Identity;

[Route("api/app/user-account")]
public class UserAccountController : HR_Mangment2Controller
{
    private readonly IUserAccountAppService _service;

    public UserAccountController(IUserAccountAppService service)
    {
        _service = service;
    }

    [HttpGet(Name = "UserAccount_List")]
    public Task<UserAccountPagedResultDto> GetListAsync([FromQuery] UserAccountListInput input) => _service.GetListAsync(input);

    [HttpPost(Name = "UserAccount_Create")]
    [ResponseCache(NoStore = true)]
    public async Task<ActionResult<UserAccountCreatedDto>> CreateAsync([FromBody] UserAccountCreateDto input) =>
        StatusCode(201, await _service.CreateAsync(input));

    [HttpGet("lookup", Name = "UserAccount_Lookup")]
    public Task<UserRefPagedResultDto> LookupAsync([FromQuery] UserAccountLookupInput input) => _service.LookupAsync(input);

    [HttpGet("{id:guid}", Name = "UserAccount_Get")]
    public Task<UserAccountDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpPost("{id:guid}/reset-password", Name = "UserAccount_ResetPassword")]
    public Task<PasswordResetResultDto> ResetPasswordAsync(Guid id, [FromBody] ReasonInputDto input) => _service.ResetPasswordAsync(id, input);

    [HttpPut("{id:guid}/roles", Name = "UserAccount_SetRoles")]
    public Task<UserAccountDto> SetRolesAsync(Guid id, [FromBody] UserRolesUpdateDto input) => _service.SetRolesAsync(id, input);

    [HttpGet("{id:guid}/permission", Name = "UserAccount_GetPermissions")]
    public Task<PermissionListResultDto> GetPermissionsAsync(Guid id) => _service.GetPermissionsAsync(id);

    [HttpPut("{id:guid}/permission", Name = "UserAccount_UpdatePermissions")]
    public Task<PermissionListResultDto> UpdatePermissionsAsync(Guid id, [FromBody] PermissionGrantUpdateDto input) => _service.UpdatePermissionsAsync(id, input);
}
