using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using HR_Mangment2.Attendance;
using HR_Mangment2.Employees;
using HR_Mangment2.Leaves;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace HR_Mangment2.Controllers.Leaves;

[Route("api/app")]
public class LeaveBalanceController : HR_Mangment2Controller
{
    private readonly ILeaveBalanceAppService _service;

    public LeaveBalanceController(ILeaveBalanceAppService service)
    {
        _service = service;
    }

    [HttpGet("leave-balance/my", Name = "LeaveBalance_GetMy")]
    public Task<LeaveBalanceDto> GetMyAsync() => _service.GetMyAsync();

    [HttpGet("leave-balance/employee/{id:guid}", Name = "LeaveBalance_GetByEmployee")]
    public Task<LeaveBalanceDto> GetByEmployeeAsync(Guid id) => _service.GetByEmployeeAsync(id);

    [HttpPost("leave-balance/employee/{id:guid}/adjust", Name = "LeaveBalance_Adjust")]
    public Task<LeaveBalanceDto> AdjustAsync(
        Guid id,
        [FromBody] LeaveBalanceAdjustDto input,
        [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        _service.AdjustAsync(id, input, idempotencyKey);

    [HttpGet("leave-ledger/employee/{id:guid}", Name = "LeaveLedger_GetByEmployee")]
    public Task<LeaveLedgerEntryPagedResultDto> GetLedgerByEmployeeAsync(Guid id, [FromQuery] LeaveLedgerListInput input) => _service.GetLedgerByEmployeeAsync(id, input);

    [HttpGet("leave-ledger/my", Name = "LeaveLedger_GetMy")]
    public Task<LeaveLedgerEntryPagedResultDto> GetMyLedgerAsync([FromQuery] LeaveLedgerListInput input) => _service.GetMyLedgerAsync(input);
}

/// <summary>The multipart form of LeaveAttachmentUploadDto.</summary>
public class LeaveAttachmentUploadDto
{
    [Required]
    public IFormFile File { get; set; } = null!;
}

[Route("api/app")]
public class LeaveRequestController : HR_Mangment2Controller
{
    private readonly ILeaveRequestAppService _service;

    public LeaveRequestController(ILeaveRequestAppService service)
    {
        _service = service;
    }

    [HttpPost("leave-request/preview", Name = "LeaveRequest_Preview")]
    public Task<LeaveRequestPreviewDto> PreviewAsync([FromBody] LeaveRequestPreviewInputDto input) => _service.PreviewAsync(input);

    [HttpPost("leave-request", Name = "LeaveRequest_Create")]
    public async Task<ActionResult<LeaveRequestDto>> CreateAsync(
        [FromBody] LeaveRequestCreateDto input,
        [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.CreateAsync(input, idempotencyKey));

    [HttpGet("leave-request/my", Name = "LeaveRequest_ListMy")]
    public Task<LeaveRequestPagedResultDto> GetMyListAsync([FromQuery] LeaveRequestListInput input) => _service.GetMyListAsync(input);

    [HttpGet("leave-request/employee/{id:guid}", Name = "LeaveRequest_ListByEmployee")]
    public Task<LeaveRequestPagedResultDto> GetListByEmployeeAsync(Guid id, [FromQuery] LeaveRequestListInput input) => _service.GetListByEmployeeAsync(id, input);

    [HttpPost("leave-request/{id:guid}/reverse", Name = "LeaveRequest_Reverse")]
    public Task<LeaveRequestDto> ReverseAsync(Guid id, [FromBody] HR_Mangment2.Identity.ReasonInputDto input) => _service.ReverseAsync(id, input);

    [HttpGet("leave-request/{id:guid}", Name = "LeaveRequest_Get")]
    public Task<LeaveRequestDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpPost("leave-request/attachment", Name = "LeaveRequest_UploadAttachment")]
    [Consumes("multipart/form-data")]
    [DisableAuditing]
    public async Task<ActionResult<AttachmentDto>> UploadAttachmentAsync([FromForm] LeaveAttachmentUploadDto input)
    {
        using var buffer = new MemoryStream();
        await input.File.CopyToAsync(buffer);

        return StatusCode(201, await _service.UploadAttachmentAsync(new LeaveAttachmentUploadInput
        {
            Content = buffer.ToArray(),
            FileName = input.File.FileName,
            ContentType = input.File.ContentType
        }));
    }

    [HttpGet("leave-request/{id:guid}/attachment/{attachmentId:guid}/download-url", Name = "LeaveRequest_GetAttachmentDownloadUrl")]
    [ResponseCache(NoStore = true)]
    public Task<FileDownloadUrlDto> GetAttachmentDownloadUrlAsync(Guid id, Guid attachmentId) => _service.GetAttachmentDownloadUrlAsync(id, attachmentId);

    [HttpGet("leave-calendar/team", Name = "LeaveCalendar_GetTeam")]
    public Task<LeaveCalendarEntryPagedResultDto> GetTeamCalendarAsync([FromQuery] LeaveCalendarListInput input) => _service.GetTeamCalendarAsync(input);
}

[Route("api/app/other-leave-type")]
public class OtherLeaveTypeController : HR_Mangment2Controller
{
    private readonly IOtherLeaveTypeAppService _service;

    public OtherLeaveTypeController(IOtherLeaveTypeAppService service)
    {
        _service = service;
    }

    [HttpGet(Name = "OtherLeaveType_List")]
    public Task<OtherLeaveTypePagedResultDto> GetListAsync([FromQuery] OtherLeaveTypeListInput input) => _service.GetListAsync(input);

    [HttpPost(Name = "OtherLeaveType_Create")]
    public async Task<ActionResult<OtherLeaveTypeDto>> CreateAsync(
        [FromBody] OtherLeaveTypeCreateDto input,
        [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.CreateAsync(input, idempotencyKey));

    [HttpPut("{id:guid}", Name = "OtherLeaveType_Update")]
    public Task<OtherLeaveTypeDto> UpdateAsync(Guid id, [FromBody] OtherLeaveTypeUpdateDto input) => _service.UpdateAsync(id, input);

    [HttpGet("available", Name = "OtherLeaveType_ListAvailable")]
    public Task<OtherLeaveTypeOptionListDto> GetAvailableAsync([FromQuery] Guid? employeeId) => _service.GetAvailableAsync(employeeId);
}

[Route("api/app/attendance-correction")]
public class AttendanceCorrectionController : HR_Mangment2Controller
{
    private readonly IAttendanceCorrectionAppService _service;

    public AttendanceCorrectionController(IAttendanceCorrectionAppService service)
    {
        _service = service;
    }

    [HttpPost(Name = "AttendanceCorrection_Create")]
    public async Task<ActionResult<AttendanceCorrectionDto>> CreateAsync(
        [FromBody] AttendanceCorrectionCreateDto input,
        [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.CreateAsync(input, idempotencyKey));

    [HttpGet("{id:guid}", Name = "AttendanceCorrection_Get")]
    public Task<AttendanceCorrectionDto> GetAsync(Guid id) => _service.GetAsync(id);
}
