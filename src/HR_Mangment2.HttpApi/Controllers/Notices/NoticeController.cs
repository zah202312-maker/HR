using System;
using System.Threading.Tasks;
using HR_Mangment2.Notices;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;

namespace HR_Mangment2.Controllers.Notices;

/// <summary>Notice_* (decision 2026-11-10).</summary>
public class NoticeController : HR_Mangment2Controller
{
    private readonly INoticeAppService _service;

    public NoticeController(INoticeAppService service)
    {
        _service = service;
    }

    [HttpGet("api/app/notice", Name = "Notice_GetReceivedList")]
    public Task<NoticeReceivedPagedResultDto> GetReceivedListAsync([FromQuery] NoticeReceivedListInput input) => _service.GetReceivedListAsync(input);

    [HttpPost("api/app/notice", Name = "Notice_Create")]
    public async Task<ActionResult<NoticeSentDto>> CreateAsync([FromBody] NoticeCreateDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.CreateAsync(input, idempotencyKey));

    [HttpGet("api/app/notice/sent", Name = "Notice_GetSentList")]
    public Task<NoticeSentPagedResultDto> GetSentListAsync([FromQuery] PagedAndSortedResultRequestDto input) => _service.GetSentListAsync(input);

    [HttpGet("api/app/notice/{id:guid}", Name = "Notice_Get")]
    public Task<NoticeReceivedDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpGet("api/app/notice/{id:guid}/receipt", Name = "Notice_GetReceipts")]
    public Task<NoticeReceiptPagedResultDto> GetReceiptsAsync(Guid id, [FromQuery] NoticeReceiptListInput input) => _service.GetReceiptsAsync(id, input);

    [HttpPost("api/app/notice/{id:guid}/acknowledge", Name = "Notice_Acknowledge")]
    public Task<NoticeReceivedDto> AcknowledgeAsync(Guid id) => _service.AcknowledgeAsync(id);
}
