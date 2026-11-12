using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using HR_Mangment2.Controllers.EmploymentContracts;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Leaves;
using HR_Mangment2.Store;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace HR_Mangment2.Controllers.Store;

// Store development, decision 2026-11-11.

/// <summary>The multipart body of the two upload routes. The schema name is the contract's.</summary>
public class StoreAttachmentUploadInputDto
{
    [Required]
    public IFormFile File { get; set; } = null!;
}

[Route("api/app/store/item-category")]
public class ItemCategoryController : HR_Mangment2Controller
{
    private readonly IItemCategoryAppService _service;

    public ItemCategoryController(IItemCategoryAppService service)
    {
        _service = service;
    }

    [HttpGet(Name = "ItemCategory_GetList")]
    public Task<ItemCategoryPagedResultDto> GetListAsync([FromQuery] StoreCatalogListInput input) => _service.GetListAsync(input);

    [HttpPost(Name = "ItemCategory_Create")]
    public async Task<ActionResult<ItemCategoryDto>> CreateAsync([FromBody] ItemCategoryCreateInputDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.CreateAsync(input, idempotencyKey));

    [HttpPut("{id:guid}", Name = "ItemCategory_Update")]
    public Task<ItemCategoryDto> UpdateAsync(Guid id, [FromBody] ItemCategoryUpdateInputDto input) => _service.UpdateAsync(id, input);
}

[Route("api/app/store/item-unit")]
public class ItemUnitController : HR_Mangment2Controller
{
    private readonly IItemUnitAppService _service;

    public ItemUnitController(IItemUnitAppService service)
    {
        _service = service;
    }

    [HttpGet(Name = "ItemUnit_GetList")]
    public Task<ItemUnitPagedResultDto> GetListAsync([FromQuery] StoreCatalogListInput input) => _service.GetListAsync(input);

    [HttpPost(Name = "ItemUnit_Create")]
    public async Task<ActionResult<ItemUnitDto>> CreateAsync([FromBody] ItemUnitCreateInputDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.CreateAsync(input, idempotencyKey));

    [HttpPut("{id:guid}", Name = "ItemUnit_Update")]
    public Task<ItemUnitDto> UpdateAsync(Guid id, [FromBody] ItemUnitUpdateInputDto input) => _service.UpdateAsync(id, input);
}

[Route("api/app/store/team")]
public class StoreTeamController : HR_Mangment2Controller
{
    private readonly IStoreTeamAppService _service;

    public StoreTeamController(IStoreTeamAppService service)
    {
        _service = service;
    }

    [HttpGet(Name = "StoreTeam_GetList")]
    public Task<StoreTeamPagedResultDto> GetListAsync([FromQuery] StoreCatalogListInput input) => _service.GetListAsync(input);

    [HttpPost(Name = "StoreTeam_Create")]
    public async Task<ActionResult<StoreTeamDto>> CreateAsync([FromBody] StoreTeamCreateInputDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.CreateAsync(input, idempotencyKey));

    [HttpPut("{id:guid}", Name = "StoreTeam_Update")]
    public Task<StoreTeamDto> UpdateAsync(Guid id, [FromBody] StoreTeamUpdateInputDto input) => _service.UpdateAsync(id, input);
}

[Route("api/app/store/purchase")]
public class StorePurchaseController : HR_Mangment2Controller
{
    private readonly IStorePurchaseAppService _service;

    public StorePurchaseController(IStorePurchaseAppService service)
    {
        _service = service;
    }

    [HttpGet(Name = "StorePurchase_GetList")]
    public Task<StorePurchaseSummaryPagedResultDto> GetListAsync([FromQuery] StorePurchaseListInput input) => _service.GetListAsync(input);

    [HttpPost(Name = "StorePurchase_Create")]
    public async Task<ActionResult<StorePurchaseDto>> CreateAsync([FromBody] StorePurchaseCreateInputDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.CreateAsync(input, idempotencyKey));

    [HttpGet("{id:guid}", Name = "StorePurchase_Get")]
    public Task<StorePurchaseDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpPut("{id:guid}", Name = "StorePurchase_Update")]
    public Task<StorePurchaseDto> UpdateAsync(Guid id, [FromBody] StorePurchaseUpdateInputDto input) => _service.UpdateAsync(id, input);

    [HttpDelete("{id:guid}", Name = "StorePurchase_Delete")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/approve", Name = "StorePurchase_Approve")]
    public Task<StorePurchaseDto> ApproveAsync(Guid id) => _service.ApproveAsync(id);

    [HttpPost("{id:guid}/reverse", Name = "StorePurchase_Reverse")]
    public Task<StorePurchaseDto> ReverseAsync(Guid id, [FromBody] ReasonInputDto input) => _service.ReverseAsync(id, input);

    [HttpPost("{id:guid}/attachment", Name = "StorePurchase_UploadAttachment")]
    [Consumes("multipart/form-data")]
    [DisableAuditing]
    public async Task<AttachmentDto> UploadAttachmentAsync(Guid id, [FromForm] StoreAttachmentUploadInputDto input) =>
        await _service.UploadAttachmentAsync(id, await UploadedFiles.ReadAsync(input.File));

    [HttpDelete("{id:guid}/attachment", Name = "StorePurchase_DeleteAttachment")]
    public async Task<IActionResult> DeleteAttachmentAsync(Guid id)
    {
        await _service.DeleteAttachmentAsync(id);
        return NoContent();
    }

    [HttpGet("{id:guid}/attachment-download-url", Name = "StorePurchase_GetAttachmentDownloadUrl")]
    [ResponseCache(NoStore = true)]
    public Task<FileDownloadUrlDto> GetAttachmentDownloadUrlAsync(Guid id) => _service.GetAttachmentDownloadUrlAsync(id);
}

[Route("api/app/store/issue-voucher")]
public class StoreIssueVoucherController : HR_Mangment2Controller
{
    private readonly IStoreIssueVoucherAppService _service;

    public StoreIssueVoucherController(IStoreIssueVoucherAppService service)
    {
        _service = service;
    }

    [HttpGet(Name = "StoreIssueVoucher_GetList")]
    public Task<StoreIssueVoucherSummaryPagedResultDto> GetListAsync([FromQuery] StoreIssueVoucherListInput input) => _service.GetListAsync(input);

    [HttpPost(Name = "StoreIssueVoucher_Create")]
    public async Task<ActionResult<StoreIssueVoucherDto>> CreateAsync([FromBody] StoreIssueVoucherCreateInputDto input, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey) =>
        StatusCode(201, await _service.CreateAsync(input, idempotencyKey));

    [HttpGet("{id:guid}", Name = "StoreIssueVoucher_Get")]
    public Task<StoreIssueVoucherDto> GetAsync(Guid id) => _service.GetAsync(id);

    [HttpPut("{id:guid}", Name = "StoreIssueVoucher_Update")]
    public Task<StoreIssueVoucherDto> UpdateAsync(Guid id, [FromBody] StoreIssueVoucherUpdateInputDto input) => _service.UpdateAsync(id, input);

    [HttpDelete("{id:guid}", Name = "StoreIssueVoucher_Delete")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/approve", Name = "StoreIssueVoucher_Approve")]
    public Task<StoreIssueVoucherDto> ApproveAsync(Guid id) => _service.ApproveAsync(id);

    [HttpPost("{id:guid}/reverse", Name = "StoreIssueVoucher_Reverse")]
    public Task<StoreIssueVoucherDto> ReverseAsync(Guid id, [FromBody] ReasonInputDto input) => _service.ReverseAsync(id, input);

    [HttpPost("{id:guid}/attachment", Name = "StoreIssueVoucher_UploadAttachment")]
    [Consumes("multipart/form-data")]
    [DisableAuditing]
    public async Task<AttachmentDto> UploadAttachmentAsync(Guid id, [FromForm] StoreAttachmentUploadInputDto input) =>
        await _service.UploadAttachmentAsync(id, await UploadedFiles.ReadAsync(input.File));

    [HttpDelete("{id:guid}/attachment", Name = "StoreIssueVoucher_DeleteAttachment")]
    public async Task<IActionResult> DeleteAttachmentAsync(Guid id)
    {
        await _service.DeleteAttachmentAsync(id);
        return NoContent();
    }

    [HttpGet("{id:guid}/attachment-download-url", Name = "StoreIssueVoucher_GetAttachmentDownloadUrl")]
    [ResponseCache(NoStore = true)]
    public Task<FileDownloadUrlDto> GetAttachmentDownloadUrlAsync(Guid id) => _service.GetAttachmentDownloadUrlAsync(id);
}

[Route("api/app/store")]
public class StoreReportController : HR_Mangment2Controller
{
    private readonly IStoreReportAppService _service;

    public StoreReportController(IStoreReportAppService service)
    {
        _service = service;
    }

    [HttpGet("dashboard", Name = "StoreDashboard_Get")]
    public Task<StoreDashboardDto> GetDashboardAsync() => _service.GetDashboardAsync();

    [HttpGet("report/purchases", Name = "StoreReport_GetPurchases")]
    public Task<StorePurchaseReportDto> GetPurchasesAsync([FromQuery] StorePurchaseReportInput input) => _service.GetPurchasesAsync(input);

    [HttpGet("report/stock", Name = "StoreReport_GetStock")]
    public Task<StoreStockReportDto> GetStockAsync([FromQuery] StoreStockReportInput input) => _service.GetStockAsync(input);

    [HttpGet("report/issues-by-team", Name = "StoreReport_GetIssuesByTeam")]
    public Task<StoreTeamIssueReportDto> GetIssuesByTeamAsync([FromQuery] StoreTeamIssueReportInput input) => _service.GetIssuesByTeamAsync(input);

    [HttpGet("report/item-movement", Name = "StoreReport_GetItemMovement")]
    public Task<StoreItemMovementReportDto> GetItemMovementAsync([FromQuery] StoreItemMovementReportInput input) => _service.GetItemMovementAsync(input);

    [HttpGet("report/{code}/export", Name = "StoreReport_Export")]
    public async Task<IActionResult> ExportAsync(StoreReportCode code, [FromQuery] StoreReportExportInput input)
    {
        // ABP's validation filter skips actions returning IActionResult.
        ValidateModel();

        var file = await _service.ExportAsync(code, input);
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(file.Content, file.ContentType, file.FileName);
    }
}
