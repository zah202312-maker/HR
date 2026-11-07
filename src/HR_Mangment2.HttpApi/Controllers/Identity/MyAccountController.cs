using System.Threading.Tasks;
using HR_Mangment2.Controllers;
using HR_Mangment2.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HR_Mangment2.Controllers.Identity;

[Route("api/app")]
public class MyAccountController : HR_Mangment2Controller
{
    private readonly IMyAccountAppService _service;

    public MyAccountController(IMyAccountAppService service)
    {
        _service = service;
    }

    [HttpGet("me", Name = "Me_Get")]
    public Task<MyAccountDto> GetAsync() => _service.GetAsync();

    [HttpPatch("me", Name = "Me_Update")]
    public Task<MyAccountDto> UpdateAsync([FromBody] MyAccountUpdateDto input) => _service.UpdateAsync(input);

    [HttpPost("account/change-password", Name = "Account_ChangePassword")]
    [EnableRateLimiting(HrRateLimiting.SensitivePolicy)]
    public Task<ChangePasswordResultDto> ChangePasswordAsync([FromBody] ChangePasswordInputDto input) => _service.ChangePasswordAsync(input);
}
