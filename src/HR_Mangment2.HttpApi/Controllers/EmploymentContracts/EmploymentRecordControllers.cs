using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using HR_Mangment2.Appreciation;
using HR_Mangment2.Employees;
using HR_Mangment2.EmploymentContracts;
using HR_Mangment2.Warnings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Auditing;

namespace HR_Mangment2.Controllers.EmploymentContracts;

/// <summary>The multipart form of ContractCreateInputDto (decision 2026-11-10: the paper contract is attached on create).</summary>
public class ContractCreateInputDto
{
    [Required]
    public Guid? EmployeeId { get; set; }

    [Required]
    public string ContractType { get; set; } = null!;

    [Required]
    public DateOnly? StartDate { get; set; }

    [Required]
    public DateOnly? EndDate { get; set; }

    public Guid? PreviousContractId { get; set; }

    public List<IFormFile>? Attachments { get; set; }
}

[Route("api/app/contract")]
public class ContractController : HR_Mangment2Controller
{
    private readonly IContractAppService _service;

    public ContractController(IContractAppService service)
    {
        _service = service;
    }

    [HttpGet(Name = "Contract_List")]
    public Task<ContractPagedResultDto> GetListAsync([FromQuery] ContractListInput input) => _service.GetListAsync(input);

    [HttpPost(Name = "Contract_Create")]
    [Consumes("multipart/form-data")]
    [DisableAuditing]
    public async Task<ActionResult<ContractDto>> CreateAsync([FromForm] ContractCreateInputDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey)
    {
        var files = new List<UploadedFileInput>();
        foreach (var file in input.Attachments ?? new List<IFormFile>())
        {
            files.Add(await UploadedFiles.ReadAsync(file));
        }

        return StatusCode(201, await _service.CreateAsync(
            new ContractCreateInput
            {
                EmployeeId = input.EmployeeId,
                ContractType = input.ContractType,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                PreviousContractId = input.PreviousContractId,
                Attachments = files
            },
            idempotencyKey));
    }

    [HttpGet("{id:guid}/attachment/{attachmentId:guid}/download-url", Name = "Contract_GetAttachmentDownloadUrl")]
    [ResponseCache(NoStore = true)]
    public Task<FileDownloadUrlDto> GetAttachmentDownloadUrlAsync(Guid id, Guid attachmentId) => _service.GetAttachmentDownloadUrlAsync(id, attachmentId);

    [HttpDelete("{id:guid}/attachment/{attachmentId:guid}", Name = "Contract_DeleteAttachment")]
    public async Task<IActionResult> DeleteAttachmentAsync(Guid id, Guid attachmentId)
    {
        await _service.DeleteAttachmentAsync(id, attachmentId);
        return NoContent();
    }

    [HttpGet("my/{id:guid}/attachment/{attachmentId:guid}/download-url", Name = "Contract_GetMyAttachmentDownloadUrl")]
    [ResponseCache(NoStore = true)]
    public Task<FileDownloadUrlDto> GetMyAttachmentDownloadUrlAsync(Guid id, Guid attachmentId) => _service.GetMyAttachmentDownloadUrlAsync(id, attachmentId);

    [HttpGet("overview", Name = "Contract_GetOverview")]
    public Task<EmployeeContractOverviewPagedResultDto> GetOverviewAsync([FromQuery] ContractOverviewListInput input) => _service.GetOverviewAsync(input);

    [HttpGet("expiring", Name = "Contract_GetExpiring")]
    public Task<ContractExpiringPagedResultDto> GetExpiringAsync([FromQuery] ContractExpiringListInput input) => _service.GetExpiringAsync(input);

    [HttpGet("{id:guid}", Name = "Contract_Get")]
    public Task<ContractDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpPost("{id:guid}/recommend", Name = "Contract_Recommend")]
    public Task<ContractDto> RecommendAsync(Guid id, [FromBody] ContractRecommendInputDto input) => _service.RecommendAsync(id, input);

    [HttpPost("{id:guid}/decide", Name = "Contract_Decide")]
    public Task<ContractDto> DecideAsync(Guid id, [FromBody] ContractDecideInputDto input) => _service.DecideAsync(id, input);

    [HttpGet("my", Name = "Contract_GetMyList")]
    public Task<ContractEmployeePagedResultDto> GetMyListAsync([FromQuery] PagedAndSortedResultRequestDto input) => _service.GetMyListAsync(input);

    [HttpGet("my/{id:guid}", Name = "Contract_GetMy")]
    public Task<ContractEmployeeDto> GetMyAsync(Guid id) => _service.GetMyAsync(id);
}

/// <summary>The multipart form of WarningCreateInputDto.</summary>
public class WarningCreateInputDto
{
    [Required]
    public Guid? EmployeeId { get; set; }

    [Required]
    public WarningLevel? RecommendedLevel { get; set; }

    [Required]
    public DateOnly? IncidentDate { get; set; }

    [Required]
    public string Description { get; set; } = null!;

    public List<IFormFile>? Attachments { get; set; }
}

[Route("api/app/warning")]
public class WarningController : HR_Mangment2Controller
{
    private readonly IWarningAppService _service;

    public WarningController(IWarningAppService service)
    {
        _service = service;
    }

    [HttpPost(Name = "Warning_Create")]
    [Consumes("multipart/form-data")]
    [DisableAuditing]
    public async Task<ActionResult<WarningRecommendationDto>> CreateAsync([FromForm] WarningCreateInputDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey)
    {
        var files = new List<UploadedFileInput>();
        foreach (var file in input.Attachments ?? new List<IFormFile>())
        {
            files.Add(await UploadedFiles.ReadAsync(file));
        }

        return StatusCode(201, await _service.CreateAsync(
            new WarningCreateInput
            {
                EmployeeId = input.EmployeeId,
                RecommendedLevel = input.RecommendedLevel,
                IncidentDate = input.IncidentDate,
                Description = input.Description,
                Attachments = files
            },
            idempotencyKey));
    }

    [HttpGet(Name = "Warning_List")]
    public Task<WarningPagedResultDto> GetListAsync([FromQuery] WarningListInput input) => _service.GetListAsync(input);

    [HttpGet("objection", Name = "Warning_GetObjections")]
    public Task<WarningPagedResultDto> GetObjectionsAsync([FromQuery] WarningObjectionListInput input) => _service.GetObjectionsAsync(input);

    [HttpGet("recommended-by-me", Name = "Warning_GetRecommendedByMe")]
    public Task<WarningRecommendationPagedResultDto> GetRecommendedByMeAsync([FromQuery] WarningRecommendedListInput input) => _service.GetRecommendedByMeAsync(input);

    [HttpGet("my", Name = "Warning_GetMyList")]
    public Task<WarningEmployeePagedResultDto> GetMyListAsync([FromQuery] PagedAndSortedResultRequestDto input) => _service.GetMyListAsync(input);

    [HttpGet("my/{id:guid}", Name = "Warning_GetMy")]
    public Task<WarningEmployeeDto> GetMyAsync(Guid id) => _service.GetMyAsync(id);

    [HttpGet("employee/{employeeId:guid}", Name = "Warning_GetEmployeeFile")]
    public Task<WarningFileEntryPagedResultDto> GetEmployeeFileAsync(Guid employeeId, [FromQuery] PagedAndSortedResultRequestDto input) => _service.GetEmployeeFileAsync(employeeId, input);

    [HttpGet("{id:guid}", Name = "Warning_Get")]
    public Task<WarningDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpGet("{id:guid}/attachment/{attachmentId:guid}/download-url", Name = "Warning_GetAttachmentDownloadUrl")]
    [ResponseCache(NoStore = true)]
    public Task<FileDownloadUrlDto> GetAttachmentDownloadUrlAsync(Guid id, Guid attachmentId) => _service.GetAttachmentDownloadUrlAsync(id, attachmentId);

    [HttpPost("{id:guid}/notify", Name = "Warning_Notify")]
    public Task<WarningDto> NotifyAsync(Guid id) => _service.NotifyAsync(id);

    [HttpPost("{id:guid}/respond", Name = "Warning_Respond")]
    public Task<WarningEmployeeDto> RespondAsync(Guid id, [FromBody] WarningRespondInputDto input) => _service.RespondAsync(id, input);

    [HttpPost("{id:guid}/decide", Name = "Warning_Decide")]
    public Task<WarningDto> DecideAsync(Guid id, [FromBody] WarningDecideInputDto input) => _service.DecideAsync(id, input);

    [HttpPost("{id:guid}/object", Name = "Warning_Object")]
    public Task<WarningEmployeeDto> ObjectAsync(Guid id, [FromBody] WarningObjectInputDto input) => _service.ObjectAsync(id, input);

    [HttpPost("{id:guid}/resolve-objection", Name = "Warning_ResolveObjection")]
    public Task<WarningDto> ResolveObjectionAsync(Guid id, [FromBody] WarningResolveObjectionInputDto input) => _service.ResolveObjectionAsync(id, input);
}

/// <summary>The multipart form of AppreciationLetterCreateInputDto.</summary>
public class AppreciationLetterCreateInputDto
{
    [Required]
    public Guid? EmployeeId { get; set; }

    [Required]
    public string Title { get; set; } = null!;

    public string? Note { get; set; }

    [Required]
    public IFormFile File { get; set; } = null!;
}

[Route("api/app/appreciation-letter")]
public class AppreciationLetterController : HR_Mangment2Controller
{
    private readonly IAppreciationLetterAppService _service;

    public AppreciationLetterController(IAppreciationLetterAppService service)
    {
        _service = service;
    }

    [HttpPost(Name = "AppreciationLetter_Create")]
    [Consumes("multipart/form-data")]
    [DisableAuditing]
    public async Task<ActionResult<AppreciationLetterDto>> CreateAsync([FromForm] AppreciationLetterCreateInputDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.CreateAsync(
            new AppreciationLetterCreateInput
            {
                EmployeeId = input.EmployeeId,
                Title = input.Title,
                Note = input.Note,
                File = await UploadedFiles.ReadAsync(input.File)
            },
            idempotencyKey));

    [HttpGet(Name = "AppreciationLetter_List")]
    public Task<AppreciationLetterPagedResultDto> GetListAsync([FromQuery] AppreciationLetterListInput input) => _service.GetListAsync(input);

    [HttpGet("my", Name = "AppreciationLetter_GetMyList")]
    public Task<AppreciationLetterPagedResultDto> GetMyListAsync([FromQuery] PagedAndSortedResultRequestDto input) => _service.GetMyListAsync(input);

    [HttpGet("employee/{employeeId:guid}", Name = "AppreciationLetter_GetEmployeeFile")]
    public Task<AppreciationLetterPagedResultDto> GetEmployeeFileAsync(Guid employeeId, [FromQuery] PagedAndSortedResultRequestDto input) => _service.GetEmployeeFileAsync(employeeId, input);

    [HttpGet("{id:guid}", Name = "AppreciationLetter_Get")]
    public Task<AppreciationLetterDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpGet("{id:guid}/download-url", Name = "AppreciationLetter_GetDownloadUrl")]
    [ResponseCache(NoStore = true)]
    public Task<FileDownloadUrlDto> GetDownloadUrlAsync(Guid id) => _service.GetDownloadUrlAsync(id);

    [HttpPost("{id:guid}/acknowledge", Name = "AppreciationLetter_Acknowledge")]
    public Task<AppreciationLetterDto> AcknowledgeAsync(Guid id) => _service.AcknowledgeAsync(id);
}

public static class UploadedFiles
{
    public static async Task<UploadedFileInput> ReadAsync(IFormFile file)
    {
        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer);

        return new UploadedFileInput { Content = buffer.ToArray(), FileName = file.FileName, ContentType = file.ContentType };
    }
}
