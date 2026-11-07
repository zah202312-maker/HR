using System;
using System.Threading.Tasks;
using HR_Mangment2.Delegations;
using Microsoft.AspNetCore.Mvc;

namespace HR_Mangment2.Controllers.Delegations;

[Route("api/app/delegation")]
public class DelegationController : HR_Mangment2Controller
{
    private readonly IDelegationAppService _service;

    public DelegationController(IDelegationAppService service)
    {
        _service = service;
    }

    [HttpGet(Name = "Delegation_List")]
    public Task<DelegationPagedResultDto> GetListAsync([FromQuery] DelegationListInput input) => _service.GetListAsync(input);

    [HttpPost(Name = "Delegation_Create")]
    public async Task<ActionResult<DelegationDto>> CreateAsync([FromBody] DelegationCreateDto input) =>
        StatusCode(201, await _service.CreateAsync(input));

    [HttpGet("my", Name = "Delegation_GetMy")]
    public Task<DelegationPagedResultDto> GetMyAsync([FromQuery] MyDelegationListInput input) => _service.GetMyAsync(input);

    [HttpGet("candidate", Name = "Delegation_GetCandidates")]
    public Task<DelegationCandidatePagedResultDto> GetCandidatesAsync([FromQuery] DelegationCandidateInput input) => _service.GetCandidatesAsync(input);

    [HttpDelete("{id:guid}", Name = "Delegation_Cancel")]
    public async Task<IActionResult> CancelAsync(Guid id)
    {
        await _service.CancelAsync(id);
        return NoContent();
    }
}
