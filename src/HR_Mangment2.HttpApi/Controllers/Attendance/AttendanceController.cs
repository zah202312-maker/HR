using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using HR_Mangment2.Attendance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Auditing;

namespace HR_Mangment2.Controllers.Attendance;

[Route("api/app/attendance")]
public class AttendanceController : HR_Mangment2Controller
{
    private readonly IAttendanceAppService _service;

    public AttendanceController(IAttendanceAppService service)
    {
        _service = service;
    }

    [HttpGet("my", Name = "Attendance_GetMy")]
    public Task<AttendanceDayPagedResultDto> GetMyAsync([FromQuery] AttendanceMyListInput input) => _service.GetMyAsync(input);

    [HttpGet("team", Name = "Attendance_GetTeam")]
    public Task<AttendanceDayPagedResultDto> GetTeamAsync([FromQuery] AttendanceTeamListInput input) => _service.GetTeamAsync(input);

    [HttpGet("employee/{id:guid}", Name = "Attendance_GetByEmployee")]
    public Task<AttendanceDayPagedResultDto> GetByEmployeeAsync(Guid id, [FromQuery] AttendanceMyListInput input) => _service.GetByEmployeeAsync(id, input);

    [HttpGet("day/{id:guid}", Name = "Attendance_GetDayDetail")]
    public Task<AttendanceDayDetailDto> GetDayDetailAsync(Guid id) => _service.GetDayDetailAsync(id);
}

[Route("api/app/attendance-settings")]
public class AttendanceSettingsController : HR_Mangment2Controller
{
    private readonly IAttendanceSettingsAppService _service;

    public AttendanceSettingsController(IAttendanceSettingsAppService service)
    {
        _service = service;
    }

    [HttpGet(Name = "AttendanceSettings_Get")]
    public Task<AttendanceSettingsDto> GetAsync() => _service.GetAsync();

    [HttpPut(Name = "AttendanceSettings_Update")]
    public Task<AttendanceSettingsDto> UpdateAsync([FromBody] AttendanceSettingsUpdateDto input) => _service.UpdateAsync(input);
}

[Route("api/app/attendance-device")]
public class AttendanceDeviceController : HR_Mangment2Controller
{
    private readonly IAttendanceDeviceAppService _service;

    public AttendanceDeviceController(IAttendanceDeviceAppService service)
    {
        _service = service;
    }

    [HttpGet(Name = "AttendanceDevice_List")]
    public Task<AttendanceDevicePagedResultDto> GetListAsync([FromQuery] AttendanceDeviceListInput input) => _service.GetListAsync(input);

    [HttpPost(Name = "AttendanceDevice_Create")]
    [ResponseCache(NoStore = true)]
    public async Task<ActionResult<AttendanceDeviceKeyDto>> CreateAsync(
        [FromBody] AttendanceDeviceCreateDto input,
        [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.CreateAsync(input, idempotencyKey));

    [HttpPut("{id:guid}", Name = "AttendanceDevice_Update")]
    public Task<AttendanceDeviceDto> UpdateAsync(Guid id, [FromBody] AttendanceDeviceUpdateDto input) => _service.UpdateAsync(id, input);

    [HttpPost("{id:guid}/rotate-key", Name = "AttendanceDevice_RotateKey")]
    [ResponseCache(NoStore = true)]
    public Task<AttendanceDeviceKeyDto> RotateKeyAsync(Guid id) => _service.RotateKeyAsync(id);

    [HttpGet("{id:guid}/user-map", Name = "DeviceUserMap_List")]
    public Task<DeviceUserMapPagedResultDto> GetUserMapAsync(Guid id, [FromQuery] DeviceUserMapListInput input) => _service.GetUserMapAsync(id, input);

    [HttpPost("{id:guid}/user-map", Name = "DeviceUserMap_Create")]
    public async Task<ActionResult<DeviceUserMapDto>> CreateUserMapAsync(Guid id, [FromBody] DeviceUserMapCreateDto input) =>
        StatusCode(201, await _service.CreateUserMapAsync(id, input));

    [HttpDelete("{id:guid}/user-map/{mapId:guid}", Name = "DeviceUserMap_Delete")]
    public async Task<IActionResult> DeleteUserMapAsync(Guid id, Guid mapId)
    {
        await _service.DeleteUserMapAsync(id, mapId);
        return NoContent();
    }

    [HttpGet("{id:guid}/unmapped-user", Name = "DeviceUserMap_ListUnmapped")]
    public Task<UnmappedDeviceUserPagedResultDto> GetUnmappedUsersAsync(Guid id, [FromQuery] PagedAndSortedResultRequestDto input) =>
        _service.GetUnmappedUsersAsync(id, input);
}

[Route("api/attendance-devices")]
[AllowAnonymous]
public class AttendancePunchController : HR_Mangment2Controller
{
    private const string DeviceKeyHeader = "X-Device-Key";

    private readonly IAttendancePunchAppService _service;

    public AttendancePunchController(IAttendancePunchAppService service)
    {
        _service = service;
    }

    [HttpPost("punches", Name = "AttendanceDevice_IngestPunches")]
    [DisableAuditing]
    [EnableRateLimiting(HrRateLimiting.DevicePolicy)]
    public Task<AttendancePunchBatchResultDto> IngestAsync([FromBody] AttendancePunchBatchDto? input) =>
        _service.IngestAsync(Request.Headers[DeviceKeyHeader].ToString(), input);
}

/// <summary>The multipart form of AttendanceImportUploadDto.</summary>
public class AttendanceImportUploadDto
{
    [Required]
    public IFormFile File { get; set; } = null!;

    [Required]
    public Guid? DeviceId { get; set; }
}

[Route("api/app/attendance-import")]
public class AttendanceImportController : HR_Mangment2Controller
{
    private readonly IAttendanceImportAppService _service;

    public AttendanceImportController(IAttendanceImportAppService service)
    {
        _service = service;
    }

    [HttpPost(Name = "AttendanceImport_Create")]
    [Consumes("multipart/form-data")]
    [DisableAuditing]
    public async Task<ActionResult<AttendanceImportDto>> CreateAsync([FromForm] AttendanceImportUploadDto input)
    {
        using var buffer = new MemoryStream();
        await input.File.CopyToAsync(buffer);

        return StatusCode(202, await _service.CreateAsync(new AttendanceImportUploadInput
        {
            Content = buffer.ToArray(),
            FileName = input.File.FileName,
            DeviceId = input.DeviceId!.Value
        }));
    }

    [HttpGet(Name = "AttendanceImport_List")]
    public Task<AttendanceImportPagedResultDto> GetListAsync([FromQuery] AttendanceImportListInput input) => _service.GetListAsync(input);

    [HttpGet("{id:guid}", Name = "AttendanceImport_Get")]
    public Task<AttendanceImportDto> GetAsync(Guid id) => _service.GetAsync(id);
}
