using System;
using System.Threading.Tasks;
using HR_Mangment2.Identity;
using HR_Mangment2.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Volo.Abp.Application.Dtos;

namespace HR_Mangment2.Controllers.Requests;

[Route("api/app/request-type")]
public class RequestTypeController : HR_Mangment2Controller
{
    private readonly IRequestTypeAppService _service;

    public RequestTypeController(IRequestTypeAppService service)
    {
        _service = service;
    }

    [HttpGet(Name = "RequestType_GetList")]
    public Task<RequestTypePagedResultDto> GetListAsync([FromQuery] PagedAndSortedResultRequestDto input) => _service.GetListAsync(input);

    [HttpPost(Name = "RequestType_Create")]
    public async Task<ActionResult<RequestTypeAdminDto>> CreateAsync([FromBody] RequestTypeCreateInputDto input) =>
        StatusCode(201, await _service.CreateAsync(input));

    [HttpGet("admin", Name = "RequestType_GetAdminList")]
    public Task<RequestTypeAdminPagedResultDto> GetAdminListAsync([FromQuery] RequestTypeAdminListInput input) => _service.GetAdminListAsync(input);

    [HttpGet("{id:guid}", Name = "RequestType_Get")]
    public Task<RequestTypeAdminDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpPut("{id:guid}", Name = "RequestType_Update")]
    public Task<RequestTypeAdminDto> UpdateAsync(Guid id, [FromBody] RequestTypeUpdateInputDto input) => _service.UpdateAsync(id, input);
}

[Route("api/app/request")]
public class RequestController : HR_Mangment2Controller
{
    private readonly IRequestAppService _service;

    public RequestController(IRequestAppService service)
    {
        _service = service;
    }

    [HttpPost(Name = "Request_Submit")]
    public async Task<ActionResult<RequestDetailsDto>> SubmitAsync(
        [FromBody] RequestSubmitInputDto input,
        [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.SubmitAsync(input, idempotencyKey));

    [HttpGet(Name = "Request_GetList")]
    public Task<RequestSummaryPagedResultDto> GetListAsync([FromQuery] RequestListInput input) => _service.GetListAsync(input);

    [HttpGet("my", Name = "Request_GetMy")]
    public Task<MyRequestPagedResultDto> GetMyAsync([FromQuery] MyRequestListInput input) => _service.GetMyAsync(input);

    [HttpGet("inbox", Name = "Request_GetInbox")]
    public Task<RequestInboxItemPagedResultDto> GetInboxAsync([FromQuery] RequestInboxInput input) => _service.GetInboxAsync(input);

    [HttpGet("{id:guid}", Name = "Request_Get")]
    public Task<RequestDetailsDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpPost("{id:guid}/approve", Name = "Request_Approve")]
    public Task<RequestDetailsDto> ApproveAsync(Guid id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] NoteInputDto? input) =>
        _service.ApproveAsync(id, input ?? new NoteInputDto());

    [HttpPost("{id:guid}/reject", Name = "Request_Reject")]
    public Task<RequestDetailsDto> RejectAsync(Guid id, [FromBody] ReasonInputDto input) => _service.RejectAsync(id, input);

    [HttpPost("{id:guid}/cancel", Name = "Request_Cancel")]
    public Task<RequestDetailsDto> CancelAsync(Guid id, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] NoteInputDto? input) =>
        _service.CancelAsync(id, input ?? new NoteInputDto());
}

[Route("api/app/me/inbox-summary")]
public class InboxSummaryController : HR_Mangment2Controller
{
    private readonly IInboxSummaryAppService _service;

    public InboxSummaryController(IInboxSummaryAppService service)
    {
        _service = service;
    }

    [HttpGet(Name = "Me_GetInboxSummary")]
    public Task<InboxSummaryDto> GetAsync() => _service.GetAsync();
}
