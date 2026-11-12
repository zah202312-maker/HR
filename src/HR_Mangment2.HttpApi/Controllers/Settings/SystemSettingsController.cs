using System.Threading.Tasks;
using HR_Mangment2.Settings;
using Microsoft.AspNetCore.Mvc;

namespace HR_Mangment2.Controllers.Settings;

[Route("api/app/system-settings")]
public class SystemSettingsController : HR_Mangment2Controller
{
    private readonly ISystemSettingsAppService _service;

    public SystemSettingsController(ISystemSettingsAppService service)
    {
        _service = service;
    }

    [HttpGet(Name = "SystemSettings_ListSections")]
    public Task<SystemSettingsSectionListDto> ListSectionsAsync() => _service.ListSectionsAsync();

    [HttpGet("reminders", Name = "SystemSettings_GetReminders")]
    public Task<SystemSettingsRemindersDto> GetRemindersAsync() => _service.GetRemindersAsync();

    [HttpPut("reminders", Name = "SystemSettings_UpdateReminders")]
    public Task<SystemSettingsRemindersDto> UpdateRemindersAsync([FromBody] SystemSettingsRemindersUpdateInputDto input) => _service.UpdateRemindersAsync(input);

    [HttpGet("contracts", Name = "SystemSettings_GetContracts")]
    public Task<SystemSettingsContractsDto> GetContractsAsync() => _service.GetContractsAsync();

    [HttpPut("contracts", Name = "SystemSettings_UpdateContracts")]
    public Task<SystemSettingsContractsDto> UpdateContractsAsync([FromBody] SystemSettingsContractsUpdateInputDto input) => _service.UpdateContractsAsync(input);

    [HttpGet("complaints", Name = "SystemSettings_GetComplaints")]
    public Task<SystemSettingsComplaintsDto> GetComplaintsAsync() => _service.GetComplaintsAsync();

    [HttpPut("complaints", Name = "SystemSettings_UpdateComplaints")]
    public Task<SystemSettingsComplaintsDto> UpdateComplaintsAsync([FromBody] SystemSettingsComplaintsUpdateInputDto input) => _service.UpdateComplaintsAsync(input);

    [HttpGet("annual-leave", Name = "SystemSettings_GetAnnualLeave")]
    public Task<SystemSettingsAnnualLeaveDto> GetAnnualLeaveAsync() => _service.GetAnnualLeaveAsync();

    [HttpPut("annual-leave", Name = "SystemSettings_UpdateAnnualLeave")]
    public Task<SystemSettingsAnnualLeaveDto> UpdateAnnualLeaveAsync([FromBody] SystemSettingsAnnualLeaveUpdateInputDto input) => _service.UpdateAnnualLeaveAsync(input);

    [HttpGet("hr-evaluation-alert", Name = "SystemSettings_GetHrEvaluationAlert")]
    public Task<SystemSettingsHrEvaluationAlertDto> GetHrEvaluationAlertAsync() => _service.GetHrEvaluationAlertAsync();

    [HttpPut("hr-evaluation-alert", Name = "SystemSettings_UpdateHrEvaluationAlert")]
    public Task<SystemSettingsHrEvaluationAlertDto> UpdateHrEvaluationAlertAsync([FromBody] SystemSettingsHrEvaluationAlertUpdateInputDto input) => _service.UpdateHrEvaluationAlertAsync(input);

    [HttpGet("hourly-leave", Name = "SystemSettings_GetHourlyLeave")]
    public Task<SystemSettingsHourlyLeaveDto> GetHourlyLeaveAsync() => _service.GetHourlyLeaveAsync();

    [HttpPut("hourly-leave", Name = "SystemSettings_UpdateHourlyLeave")]
    public Task<SystemSettingsHourlyLeaveDto> UpdateHourlyLeaveAsync([FromBody] SystemSettingsHourlyLeaveUpdateInputDto input) => _service.UpdateHourlyLeaveAsync(input);

    [HttpGet("finance", Name = "SystemSettings_GetFinance")]
    public Task<SystemSettingsFinanceDto> GetFinanceAsync() => _service.GetFinanceAsync();

    [HttpPut("finance", Name = "SystemSettings_UpdateFinance")]
    public Task<SystemSettingsFinanceDto> UpdateFinanceAsync([FromBody] SystemSettingsFinanceUpdateInputDto input) => _service.UpdateFinanceAsync(input);
}
