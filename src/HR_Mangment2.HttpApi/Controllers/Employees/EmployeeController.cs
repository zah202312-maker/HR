using System;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;

namespace HR_Mangment2.Controllers.Employees;

[Route("api/app")]
public class EmployeeController : HR_Mangment2Controller
{
    private readonly IEmployeeAppService _service;
    private readonly IEmployeeShiftAppService _shifts;

    public EmployeeController(IEmployeeAppService service, IEmployeeShiftAppService shifts)
    {
        _service = service;
        _shifts = shifts;
    }

    [HttpGet("employee", Name = "Employee_List")]
    public Task<EmployeeListItemPagedResultDto> GetListAsync([FromQuery] EmployeeListInput input) => _service.GetListAsync(input);

    [HttpPost("employee", Name = "Employee_Create")]
    [ResponseCache(NoStore = true)]
    public async Task<ActionResult<EmployeeCreatedDto>> CreateAsync([FromBody] EmployeeCreateDto input) =>
        StatusCode(201, await _service.CreateAsync(input));

    [HttpGet("employee/lookup", Name = "Employee_Lookup")]
    public Task<EmployeeLookupPagedResultDto> LookupAsync([FromQuery] EmployeeLookupInput input) => _service.LookupAsync(input);

    [HttpGet("employee/{id:guid}", Name = "Employee_Get")]
    public Task<EmployeeHrDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpPost("employee/{id:guid}/correct-hire-date", Name = "Employee_CorrectHireDate")]
    public Task<EmployeeHrDto> CorrectHireDateAsync(Guid id, [FromBody] EmployeeHireDateCorrectionDto input) => _service.CorrectHireDateAsync(id, input);

    [HttpPut("employee/{id:guid}", Name = "Employee_Update")]
    public Task<EmployeeHrDto> UpdateAsync(Guid id, [FromBody] EmployeeUpdateDto input) => _service.UpdateAsync(id, input);

    [HttpPut("employee/{id:guid}/manager", Name = "Employee_ChangeManager")]
    public Task<ManagerChangeResultDto> ChangeManagerAsync(Guid id, [FromBody] ManagerChangeInputDto input) => _service.ChangeManagerAsync(id, input);

    [HttpGet("employee/{id:guid}/manager-history", Name = "Employee_GetManagerHistory")]
    public Task<EmployeeManagerHistoryPagedResultDto> GetManagerHistoryAsync(Guid id, [FromQuery] PagedAndSortedResultRequestDto input) =>
        _service.GetManagerHistoryAsync(id, input);

    [HttpGet("employee/{id:guid}/administrative-task-log", Name = "Employee_GetAdministrativeTaskLog")]
    public Task<AdministrativeTaskLogPagedResultDto> GetAdministrativeTaskLogAsync(Guid id, [FromQuery] AdministrativeTaskLogInput input) =>
        _service.GetAdministrativeTaskLogAsync(id, input);

    [HttpPost("manager/{id:guid}/assign-employees", Name = "Manager_AssignEmployees")]
    public Task<ManagerAssignEmployeesResultDto> AssignEmployeesAsync(Guid id, [FromBody] ManagerAssignEmployeesInputDto input) =>
        _service.AssignEmployeesAsync(id, input);

    [HttpGet("manager/my-team", Name = "Manager_GetMyTeam")]
    public Task<EmployeeTeamMemberPagedResultDto> GetMyTeamAsync([FromQuery] MyTeamInput input) => _service.GetMyTeamAsync(input);

    [HttpGet("manager/my-team/{id:guid}", Name = "Manager_GetTeamMember")]
    public Task<EmployeeTeamMemberDto> GetTeamMemberAsync(Guid id) => _service.GetTeamMemberAsync(id);

    [HttpGet("me/employee-profile", Name = "Me_GetEmployeeProfile")]
    public Task<EmployeeSelfDto> GetMyProfileAsync() => _service.GetMyProfileAsync();

    [HttpGet("me/administrative-task-log", Name = "Me_GetAdministrativeTaskLog")]
    public Task<AdministrativeTaskLogPagedResultDto> GetMyAdministrativeTaskLogAsync([FromQuery] AdministrativeTaskLogInput input) =>
        _service.GetMyAdministrativeTaskLogAsync(input);

    [HttpPut("employee/{id:guid}/shift", Name = "EmployeeShift_Assign")]
    public Task<EmployeeShiftDto> AssignShiftAsync(Guid id, [FromBody] EmployeeShiftAssignDto input) => _shifts.AssignAsync(id, input);

    [HttpGet("employee/{id:guid}/shift", Name = "EmployeeShift_GetHistory")]
    public Task<EmployeeShiftPagedResultDto> GetShiftHistoryAsync(Guid id, [FromQuery] PagedAndSortedResultRequestDto input) => _shifts.GetHistoryAsync(id, input);
}
