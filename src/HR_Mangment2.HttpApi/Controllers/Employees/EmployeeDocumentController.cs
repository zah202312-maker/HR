using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace HR_Mangment2.Controllers.Employees;

/// <summary>The multipart form of EmployeeDocumentUploadDto.</summary>
public class EmployeeDocumentUploadDto
{
    [Required]
    public IFormFile File { get; set; } = null!;

    [Required]
    public EmployeeDocumentType? Type { get; set; }

    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = null!;

    [Required]
    public DocumentSensitivity? Sensitivity { get; set; }
}

[Route("api/app")]
public class EmployeeDocumentController : HR_Mangment2Controller
{
    private readonly IEmployeeDocumentAppService _service;

    public EmployeeDocumentController(IEmployeeDocumentAppService service)
    {
        _service = service;
    }

    [HttpGet("employee/{id:guid}/documents", Name = "EmployeeDocument_List")]
    public Task<EmployeeDocumentPagedResultDto> GetListAsync(Guid id, [FromQuery] EmployeeDocumentListInput input) => _service.GetListAsync(id, input);

    [HttpPost("employee/{id:guid}/documents", Name = "EmployeeDocument_Upload")]
    [Consumes("multipart/form-data")]
    [DisableAuditing]
    public async Task<ActionResult<EmployeeDocumentDto>> UploadAsync(Guid id, [FromForm] EmployeeDocumentUploadDto input)
    {
        using var buffer = new MemoryStream();
        await input.File.CopyToAsync(buffer);

        return StatusCode(201, await _service.UploadAsync(id, new EmployeeDocumentUploadInput
        {
            Content = buffer.ToArray(),
            FileName = input.File.FileName,
            ContentType = input.File.ContentType,
            Type = input.Type!.Value,
            Title = input.Title,
            Sensitivity = input.Sensitivity!.Value
        }));
    }

    [HttpGet("document/{id:guid}/download-url", Name = "EmployeeDocument_GetDownloadUrl")]
    [ResponseCache(NoStore = true)]
    public Task<FileDownloadUrlDto> GetDownloadUrlAsync(Guid id) => _service.GetDownloadUrlAsync(id);

    [HttpDelete("document/{id:guid}", Name = "EmployeeDocument_Delete")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}

/// <summary>
/// The target of the short-lived links the contract's download-url operations return. It sits outside /api
/// because the contract describes those links as opaque URLs, not as an operation.
/// </summary>
[Route("files")]
[ApiExplorerSettings(IgnoreApi = true)]
[AllowAnonymous]
[DisableAuditing]
public class FileDownloadController : HR_Mangment2Controller
{
    private readonly IFileDownloadAppService _service;

    public FileDownloadController(IFileDownloadAppService service)
    {
        _service = service;
    }

    [HttpGet("{token:length(64)}")]
    [ResponseCache(NoStore = true)]
    public async Task<IActionResult> DownloadAsync(string token)
    {
        var file = await _service.OpenAsync(token);
        if (file == null)
        {
            // A plain 404, not the redirect to the error page that the sign-in pages use.
            var statusCodePages = HttpContext.Features.Get<IStatusCodePagesFeature>();
            if (statusCodePages != null)
            {
                statusCodePages.Enabled = false;
            }

            return NotFound();
        }

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(file.Content, file.ContentType, file.FileName);
    }
}
