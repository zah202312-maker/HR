using System;
using System.Threading.Tasks;
using HR_Mangment2.Holidays;
using Microsoft.AspNetCore.Mvc;

namespace HR_Mangment2.Controllers.Holidays;

public class HolidayController : HR_Mangment2Controller
{
    private readonly IHolidayAppService _service;

    public HolidayController(IHolidayAppService service)
    {
        _service = service;
    }

    [HttpGet("api/app/holiday", Name = "Holiday_List")]
    public Task<HolidayPagedResultDto> GetListAsync([FromQuery] HolidayListInput input) => _service.GetListAsync(input);

    [HttpPost("api/app/holiday", Name = "Holiday_Create")]
    public async Task<ActionResult<HolidayDto>> CreateAsync(
        [FromBody] HolidayWriteDto input,
        [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.CreateAsync(input, idempotencyKey));

    [HttpPut("api/app/holiday/{id:guid}", Name = "Holiday_Update")]
    public Task<HolidayDto> UpdateAsync(Guid id, [FromBody] HolidayUpdateDto input) => _service.UpdateAsync(id, input);

    [HttpDelete("api/app/holiday/{id:guid}", Name = "Holiday_Delete")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("api/app/weekly-off-setting", Name = "WeeklyOffSetting_Get")]
    public Task<WeeklyOffSettingDto> GetWeeklyOffAsync() => _service.GetWeeklyOffAsync();

    [HttpPut("api/app/weekly-off-setting", Name = "WeeklyOffSetting_Update")]
    public Task<WeeklyOffSettingDto> UpdateWeeklyOffAsync([FromBody] WeeklyOffSettingUpdateDto input) => _service.UpdateWeeklyOffAsync(input);
}
