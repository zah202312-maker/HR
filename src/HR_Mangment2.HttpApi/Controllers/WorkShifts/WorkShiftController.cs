using System;
using System.Threading.Tasks;
using HR_Mangment2.WorkShifts;
using Microsoft.AspNetCore.Mvc;

namespace HR_Mangment2.Controllers.WorkShifts;

[Route("api/app/work-shift")]
public class WorkShiftController : HR_Mangment2Controller
{
    private readonly IWorkShiftAppService _service;

    public WorkShiftController(IWorkShiftAppService service)
    {
        _service = service;
    }

    [HttpGet(Name = "WorkShift_List")]
    public Task<WorkShiftPagedResultDto> GetListAsync([FromQuery] WorkShiftListInput input) => _service.GetListAsync(input);

    [HttpPost(Name = "WorkShift_Create")]
    public async Task<ActionResult<WorkShiftDto>> CreateAsync(
        [FromBody] WorkShiftCreateDto input,
        [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.CreateAsync(input, idempotencyKey));

    [HttpGet("{id:guid}", Name = "WorkShift_Get")]
    public Task<WorkShiftDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpPut("{id:guid}", Name = "WorkShift_Update")]
    public Task<WorkShiftDto> UpdateAsync(Guid id, [FromBody] WorkShiftUpdateDto input) => _service.UpdateAsync(id, input);
}
