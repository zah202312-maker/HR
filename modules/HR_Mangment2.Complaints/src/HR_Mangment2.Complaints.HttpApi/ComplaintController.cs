using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using HR_Mangment2.Controllers;
using HR_Mangment2.Leaves;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Auditing;
using Volo.Abp.Uow;

namespace HR_Mangment2.Complaints;

/// <summary>The multipart form of ComplaintAttachmentUploadDto.</summary>
public class ComplaintAttachmentUploadDto
{
    [Required]
    public IFormFile File { get; set; } = null!;
}

[Route("api/app/complaint")]
[DisableAuditing]
public class ComplaintController : HR_Mangment2Controller
{
    private readonly IComplaintAppService _service;

    public ComplaintController(IComplaintAppService service)
    {
        _service = service;
    }

    [HttpGet("respondents", Name = "Complaint_GetRespondents")]
    public Task<ComplaintRespondentPagedResultDto> GetRespondentsAsync([FromQuery] ComplaintRespondentListInput input) => _service.GetRespondentsAsync(input);

    [HttpPost("preview-routing", Name = "Complaint_PreviewRouting")]
    public Task<ComplaintRoutingPreviewDto> PreviewRoutingAsync([FromBody] ComplaintRoutingPreviewInputDto input) => _service.PreviewRoutingAsync(input);

    [HttpPost(Name = "Complaint_Submit")]
    public async Task<ActionResult<ComplaintSubmitResultDto>> SubmitAsync(
        [FromBody] ComplaintCreateDto input,
        [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.SubmitAsync(input, idempotencyKey));

    [HttpGet("my", Name = "Complaint_GetMyList")]
    [ResponseCache(NoStore = true)]
    public Task<ComplaintComplainantSummaryPagedResultDto> GetMyListAsync([FromQuery] ComplaintMyListInput input) => _service.GetMyListAsync(input);

    [HttpGet("my/{id:guid}", Name = "Complaint_GetMyDetail")]
    [UnitOfWork(isTransactional: true)]
    [ResponseCache(NoStore = true)]
    public Task<ComplaintComplainantDetailDto> GetMyDetailAsync(Guid id) => _service.GetMyDetailAsync(id);

    [HttpGet("assigned", Name = "Complaint_GetAssignedList")]
    [ResponseCache(NoStore = true)]
    public Task<ComplaintHandlerSummaryPagedResultDto> GetAssignedListAsync([FromQuery] ComplaintAssignedListInput input) => _service.GetAssignedListAsync(input);

    [HttpGet("{id:guid}", Name = "Complaint_GetDetail")]
    [UnitOfWork(isTransactional: true)]
    [ResponseCache(NoStore = true)]
    public Task<ComplaintHandlerDetailDto> GetDetailAsync(Guid id) => _service.GetDetailAsync(id);

    [HttpPost("{id:guid}/raise-level", Name = "Complaint_RaiseLevel")]
    public Task<ComplaintTransitionResultDto> RaiseLevelAsync(Guid id, [FromBody] ComplaintRaiseLevelInputDto input) => _service.RaiseLevelAsync(id, input);

    [HttpPost("{id:guid}/action", Name = "Complaint_AddAction")]
    public async Task<ActionResult<ComplaintActionDto>> AddActionAsync(
        Guid id,
        [FromBody] ComplaintActionCreateDto input,
        [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.AddActionAsync(id, input, idempotencyKey));

    [HttpPost("{id:guid}/forward", Name = "Complaint_Forward")]
    public Task<ComplaintTransitionResultDto> ForwardAsync(Guid id, [FromBody] ComplaintForwardInputDto input) => _service.ForwardAsync(id, input);

    [HttpPost("{id:guid}/close", Name = "Complaint_Close")]
    public Task<ComplaintTransitionResultDto> CloseAsync(Guid id, [FromBody] ComplaintCloseInputDto input) => _service.CloseAsync(id, input);

    [HttpGet("{id:guid}/access-log", Name = "Complaint_GetAccessLog")]
    [UnitOfWork(isTransactional: true)]
    [ResponseCache(NoStore = true)]
    public Task<ComplaintAccessLogEntryPagedResultDto> GetAccessLogAsync(Guid id, [FromQuery] PagedAndSortedResultRequestDto input) => _service.GetAccessLogAsync(id, input);

    [HttpPost("{id:guid}/attachment", Name = "Complaint_UploadAttachment")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<AttachmentDto>> UploadAttachmentAsync(
        Guid id,
        [FromForm] ComplaintAttachmentUploadDto input,
        [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey)
    {
        using var buffer = new MemoryStream();
        await input.File.CopyToAsync(buffer);

        return StatusCode(201, await _service.UploadAttachmentAsync(
            id,
            new ComplaintAttachmentUploadInput { Content = buffer.ToArray(), FileName = input.File.FileName, ContentType = input.File.ContentType },
            idempotencyKey));
    }

    [HttpGet("{id:guid}/attachment/{attachmentId:guid}", Name = "Complaint_DownloadAttachment")]
    [UnitOfWork(isTransactional: true)]
    [ResponseCache(NoStore = true)]
    public async Task<IActionResult> DownloadAttachmentAsync(Guid id, Guid attachmentId)
    {
        var file = await _service.DownloadAttachmentAsync(id, attachmentId);
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(file.Content, "application/octet-stream", file.FileName);
    }
}

[Route("api/app/complaint-handler")]
public class ComplaintHandlerController : HR_Mangment2Controller
{
    private readonly IComplaintHandlerAppService _service;

    public ComplaintHandlerController(IComplaintHandlerAppService service)
    {
        _service = service;
    }

    [HttpPost(Name = "ComplaintHandler_Request")]
    public async Task<ActionResult<ComplaintHandlerAssignmentDto>> RequestAsync(
        [FromBody] ComplaintHandlerAssignmentCreateDto input,
        [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.RequestAsync(input, idempotencyKey));

    [HttpGet(Name = "ComplaintHandler_GetList")]
    public Task<ComplaintHandlerAssignmentPagedResultDto> GetListAsync([FromQuery] ComplaintHandlerListInput input) => _service.GetListAsync(input);

    [HttpPost("{id:guid}/approve", Name = "ComplaintHandler_Approve")]
    public Task<ComplaintHandlerAssignmentDto> ApproveAsync(Guid id, [FromBody] ComplaintHandlerAssignmentApproveDto input) => _service.ApproveAsync(id, input);

    [HttpPost("{id:guid}/reject", Name = "ComplaintHandler_Reject")]
    public Task<ComplaintHandlerAssignmentDto> RejectAsync(Guid id, [FromBody] ComplaintHandlerAssignmentReasonDto input) => _service.RejectAsync(id, input);

    [HttpPost("{id:guid}/revoke", Name = "ComplaintHandler_Revoke")]
    public Task<ComplaintHandlerAssignmentDto> RevokeAsync(Guid id, [FromBody] ComplaintHandlerAssignmentReasonDto input) => _service.RevokeAsync(id, input);
}
