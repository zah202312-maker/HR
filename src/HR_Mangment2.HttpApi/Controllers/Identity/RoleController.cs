using System;
using System.Threading.Tasks;
using HR_Mangment2.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HR_Mangment2.Controllers.Identity;

[Route("api/app/role")]
public class RoleController : HR_Mangment2Controller
{
    private readonly IRoleAppService _service;

    public RoleController(IRoleAppService service)
    {
        _service = service;
    }

    [HttpGet(Name = "Role_List")]
    public Task<RolePagedResultDto> GetListAsync([FromQuery] RoleListInput input) => _service.GetListAsync(input);

    [HttpPost(Name = "Role_Create")]
    public async Task<ActionResult<RoleDto>> CreateAsync([FromBody] RoleCreateDto input) => StatusCode(201, await _service.CreateAsync(input));

    [HttpGet("{id:guid}", Name = "Role_Get")]
    public Task<RoleDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpPut("{id:guid}", Name = "Role_Update")]
    public Task<RoleDto> UpdateAsync(Guid id, [FromBody] RoleUpdateDto input) => _service.UpdateAsync(id, input);

    [HttpDelete("{id:guid}", Name = "Role_Delete")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id:guid}/permission", Name = "Role_GetPermissions")]
    public Task<PermissionListResultDto> GetPermissionsAsync(Guid id) => _service.GetPermissionsAsync(id);

    [HttpPut("{id:guid}/permission", Name = "Role_UpdatePermissions")]
    public Task<PermissionListResultDto> UpdatePermissionsAsync(Guid id, [FromBody] PermissionGrantUpdateDto input) => _service.UpdatePermissionsAsync(id, input);
}
