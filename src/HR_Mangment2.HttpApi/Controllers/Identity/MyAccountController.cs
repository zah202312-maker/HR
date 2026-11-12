using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using HR_Mangment2.Controllers;
using HR_Mangment2.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;
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

    /// <summary>Me_UploadPhoto (decision 2026-11-11). The owner only; the bytes never reach the audit log.</summary>
    [HttpPost("me/photo", Name = "Me_UploadPhoto")]
    [Consumes("multipart/form-data")]
    [DisableAuditing]
    public async Task<MyPhotoDto> UploadPhotoAsync([FromForm] MyPhotoUploadFormDto input)
    {
        using var buffer = new MemoryStream();
        await input.File!.CopyToAsync(buffer);
        return await _service.UploadPhotoAsync(new MyPhotoUploadInput { Content = buffer.ToArray(), ContentType = input.File.ContentType ?? string.Empty });
    }

    [HttpDelete("me/photo", Name = "Me_DeletePhoto")]
    public async Task<IActionResult> DeletePhotoAsync()
    {
        await _service.DeletePhotoAsync();
        return NoContent();
    }

    /// <summary>ProfilePhoto_Get. A photo id never changes content, so the client keeps it.</summary>
    [HttpGet("profile-photo/{id:guid}", Name = "ProfilePhoto_Get")]
    [DisableAuditing]
    public async Task<IActionResult> GetPhotoAsync(Guid id)
    {
        var photo = await _service.GetPhotoAsync(id);
        Response.Headers["Cache-Control"] = "private, max-age=31536000, immutable";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(photo.Content, photo.ContentType);
    }

    [HttpPost("account/change-password", Name = "Account_ChangePassword")]
    [EnableRateLimiting(HrRateLimiting.SensitivePolicy)]
    public Task<ChangePasswordResultDto> ChangePasswordAsync([FromBody] ChangePasswordInputDto input) => _service.ChangePasswordAsync(input);
}

public class MyPhotoUploadFormDto
{
    [Required]
    public IFormFile? File { get; set; }
}
