using System;
using System.Threading.Tasks;
using HR_Mangment2.Identity;
using HR_Mangment2.Memos;
using HR_Mangment2.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HR_Mangment2.Controllers.Memos;

[Route("api/app/memo")]
public class MemoController : HR_Mangment2Controller
{
    private readonly IMemoAppService _service;

    public MemoController(IMemoAppService service)
    {
        _service = service;
    }

    [HttpPost(Name = "Memo_Create")]
    public async Task<ActionResult<MemoDto>> CreateAsync(
        [FromBody] MemoCreateInputDto input,
        [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.CreateAsync(input, idempotencyKey));

    [HttpGet("my", Name = "Memo_GetMy")]
    public Task<MemoListItemPagedResultDto> GetMyAsync([FromQuery] MemoMyListInput input) => _service.GetMyAsync(input);

    [HttpGet("inbox", Name = "Memo_GetInbox")]
    public Task<MemoListItemPagedResultDto> GetInboxAsync([FromQuery] MemoInboxInput input) => _service.GetInboxAsync(input);

    [HttpGet("{id:guid}", Name = "Memo_Get")]
    public Task<MemoDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpPost("{id:guid}/forward", Name = "Memo_Forward")]
    public Task<MemoDto> ForwardAsync(Guid id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] NoteInputDto? input) =>
        _service.ForwardAsync(id, input ?? new NoteInputDto());

    [HttpPost("{id:guid}/convert", Name = "Memo_Convert")]
    public Task<MemoDto> ConvertAsync(Guid id, [FromBody] MemoConvertInputDto input) => _service.ConvertAsync(id, input);

    [HttpPost("{id:guid}/close", Name = "Memo_Close")]
    public Task<MemoDto> CloseAsync(Guid id, [FromBody] ReasonInputDto input) => _service.CloseAsync(id, input);
}

[Route("api/app/incident")]
public class IncidentController : HR_Mangment2Controller
{
    private readonly IIncidentAppService _service;

    public IncidentController(IIncidentAppService service)
    {
        _service = service;
    }

    [HttpPost(Name = "Incident_Create")]
    public async Task<ActionResult<IncidentDto>> CreateAsync(
        [FromBody] IncidentCreateInputDto input,
        [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.CreateAsync(input, idempotencyKey));

    [HttpGet("my", Name = "Incident_GetMy")]
    public Task<IncidentListItemPagedResultDto> GetMyAsync([FromQuery] IncidentMyListInput input) => _service.GetMyAsync(input);

    [HttpGet("inbox", Name = "Incident_GetInbox")]
    public Task<IncidentListItemPagedResultDto> GetInboxAsync([FromQuery] IncidentInboxInput input) => _service.GetInboxAsync(input);

    [HttpGet("{id:guid}", Name = "Incident_Get")]
    public Task<IncidentDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpPatch("{id:guid}", Name = "Incident_UpdateStatus")]
    public Task<IncidentDto> UpdateStatusAsync(Guid id, [FromBody] IncidentStatusUpdateInputDto input) => _service.UpdateStatusAsync(id, input);

    [HttpPost("{id:guid}/forward", Name = "Incident_Forward")]
    public Task<IncidentDto> ForwardAsync(Guid id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] NoteInputDto? input) =>
        _service.ForwardAsync(id, input ?? new NoteInputDto());
}
