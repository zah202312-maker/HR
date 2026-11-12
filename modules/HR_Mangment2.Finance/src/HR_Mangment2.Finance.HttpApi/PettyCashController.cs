using System;
using System.Threading.Tasks;
using HR_Mangment2.Controllers;
using HR_Mangment2.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HR_Mangment2.Finance;

/// <summary>Petty cash for the accountant (decision 2026-11-11).</summary>
[Route("api/app/finance")]
public class PettyCashController : HR_Mangment2Controller
{
    private readonly IPettyCashAppService _service;

    public PettyCashController(IPettyCashAppService service)
    {
        _service = service;
    }

    [HttpGet("petty-cash-type", Name = "PettyCashType_GetList")]
    public Task<PettyCashTypePagedResultDto> GetTypesAsync([FromQuery] PettyCashTypeListInput input) => _service.GetTypesAsync(input);

    [HttpPost("petty-cash-type", Name = "PettyCashType_Create")]
    public async Task<ActionResult<PettyCashTypeDto>> CreateTypeAsync([FromBody] PettyCashTypeCreateInputDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.CreateTypeAsync(input, idempotencyKey));

    [HttpPut("petty-cash-type/{id:guid}", Name = "PettyCashType_Update")]
    public Task<PettyCashTypeDto> UpdateTypeAsync(Guid id, [FromBody] PettyCashTypeUpdateInputDto input) => _service.UpdateTypeAsync(id, input);

    [HttpGet("petty-cash-assignment", Name = "PettyCashAssignment_GetList")]
    public Task<PettyCashAssignmentPagedResultDto> GetAssignmentsAsync([FromQuery] PettyCashAssignmentListInput input) => _service.GetAssignmentsAsync(input);

    [HttpPost("petty-cash-assignment", Name = "PettyCashAssignment_Create")]
    public async Task<ActionResult<PettyCashAssignmentDto>> CreateAssignmentAsync([FromBody] PettyCashAssignmentCreateInputDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.CreateAssignmentAsync(input, idempotencyKey));

    [HttpPut("petty-cash-assignment/{id:guid}", Name = "PettyCashAssignment_Update")]
    public Task<PettyCashAssignmentDto> UpdateAssignmentAsync(Guid id, [FromBody] PettyCashAssignmentUpdateInputDto input) => _service.UpdateAssignmentAsync(id, input);

    [HttpGet("petty-cash/month", Name = "PettyCashEntry_GetMonth")]
    public Task<PettyCashMonthDto> GetMonthAsync([FromQuery] PettyCashMonthInput input) => _service.GetMonthAsync(input);

    [HttpPut("petty-cash/entry/{id:guid}", Name = "PettyCashEntry_Update")]
    public Task<PettyCashEntryDto> UpdateEntryAsync(Guid id, [FromBody] PettyCashEntryUpdateInputDto input) => _service.UpdateEntryAsync(id, input);

    [HttpPost("petty-cash/deliver", Name = "PettyCashEntry_Deliver")]
    public Task<PettyCashEntryListDto> DeliverAsync([FromBody] PettyCashDeliveryInputDto input) => _service.DeliverAsync(input);

    [HttpPost("petty-cash/entry/{id:guid}/undo-delivery", Name = "PettyCashEntry_UndoDelivery")]
    public Task<PettyCashEntryDto> UndoDeliveryAsync(Guid id, [FromBody] ReasonInputDto input) => _service.UndoDeliveryAsync(id, input);
}
