using System;
using System.Threading.Tasks;
using HR_Mangment2.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HR_Mangment2.Controllers.Identity;

[Route("api/app")]
public class MfaController : HR_Mangment2Controller
{
    private readonly IMfaAppService _service;

    public MfaController(IMfaAppService service)
    {
        _service = service;
    }

    [HttpGet("account/mfa", Name = "Mfa_GetStatus")]
    public Task<MfaStatusDto> GetStatusAsync() => _service.GetStatusAsync();

    [HttpPost("account/mfa/enroll", Name = "Mfa_StartEnrollment")]
    [ResponseCache(NoStore = true)]
    public Task<MfaEnrollmentDto> StartEnrollmentAsync() => _service.StartEnrollmentAsync();

    [HttpPost("account/mfa/confirm", Name = "Mfa_ConfirmEnrollment")]
    [ResponseCache(NoStore = true)]
    [EnableRateLimiting(HrRateLimiting.SensitivePolicy)]
    public Task<MfaRecoveryCodesDto> ConfirmEnrollmentAsync([FromBody] MfaCodeInputDto input) => _service.ConfirmEnrollmentAsync(input);

    [HttpPost("account/mfa/recovery-codes", Name = "Mfa_RegenerateRecoveryCodes")]
    [ResponseCache(NoStore = true)]
    [EnableRateLimiting(HrRateLimiting.SensitivePolicy)]
    public Task<MfaRecoveryCodesDto> RegenerateRecoveryCodesAsync([FromBody] MfaCodeInputDto input) => _service.RegenerateRecoveryCodesAsync(input);

    [HttpPost("user-account/{id:guid}/mfa/reset", Name = "UserAccount_ResetMfa")]
    public async Task<IActionResult> ResetAsync(Guid id, [FromBody] ReasonInputDto input)
    {
        await _service.ResetForUserAsync(id, input);
        return NoContent();
    }
}
