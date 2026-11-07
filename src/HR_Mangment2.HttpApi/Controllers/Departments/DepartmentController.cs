using System;
using System.Threading.Tasks;
using HR_Mangment2.Departments;
using Microsoft.AspNetCore.Mvc;

namespace HR_Mangment2.Controllers.Departments;

[Route("api/app/department")]
public class DepartmentController : HR_Mangment2Controller
{
    private readonly IDepartmentAppService _service;

    public DepartmentController(IDepartmentAppService service)
    {
        _service = service;
    }

    [HttpGet(Name = "Department_List")]
    public Task<DepartmentPagedResultDto> GetListAsync([FromQuery] DepartmentListInput input) => _service.GetListAsync(input);

    [HttpPost(Name = "Department_Create")]
    public async Task<ActionResult<DepartmentDto>> CreateAsync([FromBody] DepartmentCreateDto input) => StatusCode(201, await _service.CreateAsync(input));

    [HttpGet("lookup", Name = "Department_Lookup")]
    public Task<DepartmentLookupPagedResultDto> LookupAsync([FromQuery] DepartmentListInput input) => _service.LookupAsync(input);

    [HttpGet("{id:guid}", Name = "Department_Get")]
    public Task<DepartmentDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpPut("{id:guid}", Name = "Department_Update")]
    public Task<DepartmentDto> UpdateAsync(Guid id, [FromBody] DepartmentUpdateDto input) => _service.UpdateAsync(id, input);

    [HttpDelete("{id:guid}", Name = "Department_Delete")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
