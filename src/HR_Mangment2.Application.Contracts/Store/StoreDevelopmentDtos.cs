using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR_Mangment2.Departments;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Leaves;
using HR_Mangment2.Reports;
using HR_Mangment2.Requests;
using HR_Mangment2.Warnings;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.Store;

// Store development, decision 2026-11-11. Names follow the contract schemas.

public class StoreLookupRefDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

public class StoreCatalogEntryDto
{
    public Guid Id { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public bool IsActive { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
    public DateTime CreationTime { get; set; }
}

public class ItemCategoryDto : StoreCatalogEntryDto
{
}

public class ItemUnitDto : StoreCatalogEntryDto
{
}

public class ItemCategoryPagedResultDto
{
    public long TotalCount { get; set; }
    public List<ItemCategoryDto> Items { get; set; } = new();
}

public class ItemUnitPagedResultDto
{
    public long TotalCount { get; set; }
    public List<ItemUnitDto> Items { get; set; } = new();
}

public class StoreCatalogEntryCreateInputDto
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
}

public class StoreCatalogEntryUpdateInputDto : StoreCatalogEntryCreateInputDto
{
    public bool? IsActive { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class ItemCategoryCreateInputDto : StoreCatalogEntryCreateInputDto
{
}

public class ItemCategoryUpdateInputDto : StoreCatalogEntryUpdateInputDto
{
}

public class ItemUnitCreateInputDto : StoreCatalogEntryCreateInputDto
{
}

public class ItemUnitUpdateInputDto : StoreCatalogEntryUpdateInputDto
{
}

public class StoreCatalogListInput : PagedAndSortedResultRequestDto
{
    public bool? IsActive { get; set; }
    public string? Filter { get; set; }
}

public class StoreTeamDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public DepartmentRefDto? Department { get; set; }
    public bool IsActive { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}

public class StoreTeamPagedResultDto
{
    public long TotalCount { get; set; }
    public List<StoreTeamDto> Items { get; set; } = new();
}

public class StoreTeamCreateInputDto
{
    public string Name { get; set; } = null!;
    public Guid? DepartmentId { get; set; }
}

public class StoreTeamUpdateInputDto : StoreTeamCreateInputDto
{
    public bool? IsActive { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class StorePurchaseLineDto
{
    public Guid Id { get; set; }
    public ItemRefDto Item { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public StoreLookupRefDto? Category { get; set; }
    public StoreLookupRefDto? Unit { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class StorePurchaseDto
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; } = null!;
    public string SupplierName { get; set; } = null!;
    public DateOnly PurchaseDate { get; set; }
    public DateOnly ReceivedDate { get; set; }
    public string? Note { get; set; }
    public StoreDocumentStatus Status { get; set; }
    public string Currency { get; set; } = null!;
    public List<StorePurchaseLineDto> Lines { get; set; } = new();
    public decimal TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
    public AttachmentDto? Attachment { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public UserRefDto? ApprovedBy { get; set; }
    public DateTime? ReversedAt { get; set; }
    public UserRefDto? ReversedBy { get; set; }
    public string? ReverseReason { get; set; }
    public UserRefDto CreatedBy { get; set; } = null!;
    public string ConcurrencyStamp { get; set; } = null!;
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}

public class StorePurchaseSummaryDto
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; } = null!;
    public string SupplierName { get; set; } = null!;
    public DateOnly PurchaseDate { get; set; }
    public DateOnly ReceivedDate { get; set; }
    public StoreDocumentStatus Status { get; set; }
    public int LinesCount { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = null!;
    public bool HasAttachment { get; set; }
    public UserRefDto CreatedBy { get; set; } = null!;
    public DateTime CreationTime { get; set; }
}

public class StorePurchaseSummaryPagedResultDto
{
    public long TotalCount { get; set; }
    public List<StorePurchaseSummaryDto> Items { get; set; } = new();
}

public class StorePurchaseLineInputDto
{
    public Guid? ItemId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
}

public class StorePurchaseCreateInputDto
{
    public string ReceiptNumber { get; set; } = null!;
    public string SupplierName { get; set; } = null!;
    public DateOnly? PurchaseDate { get; set; }
    public DateOnly? ReceivedDate { get; set; }
    public string? Note { get; set; }
    public List<StorePurchaseLineInputDto> Lines { get; set; } = null!;
}

public class StorePurchaseUpdateInputDto : StorePurchaseCreateInputDto
{
    public string ConcurrencyStamp { get; set; } = null!;
}

public class StorePurchaseListInput : PagedAndSortedResultRequestDto
{
    public StoreDocumentStatus? Status { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public Guid? ItemId { get; set; }
    public string? Filter { get; set; }
}

public class StoreIssueVoucherLineDto
{
    public Guid Id { get; set; }
    public ItemRefDto Item { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public StoreLookupRefDto? Unit { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? LineValue { get; set; }
    public Guid? CustodyId { get; set; }
}

public class StoreIssueVoucherDto
{
    public Guid Id { get; set; }
    public string VoucherNumber { get; set; } = null!;
    public DateOnly IssueDate { get; set; }
    public StoreLookupRefDto Team { get; set; } = null!;
    public EmployeeRefDto? Employee { get; set; }
    public string? Note { get; set; }
    public StoreDocumentStatus Status { get; set; }
    public List<StoreIssueVoucherLineDto> Lines { get; set; } = new();
    public decimal TotalQuantity { get; set; }
    public decimal? TotalValue { get; set; }
    public string Currency { get; set; } = null!;
    public AttachmentDto? Attachment { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public UserRefDto? ApprovedBy { get; set; }
    public DateTime? ReversedAt { get; set; }
    public UserRefDto? ReversedBy { get; set; }
    public string? ReverseReason { get; set; }
    public UserRefDto CreatedBy { get; set; } = null!;
    public string ConcurrencyStamp { get; set; } = null!;
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}

public class StoreIssueVoucherSummaryDto
{
    public Guid Id { get; set; }
    public string VoucherNumber { get; set; } = null!;
    public DateOnly IssueDate { get; set; }
    public StoreLookupRefDto Team { get; set; } = null!;
    public EmployeeRefDto? Employee { get; set; }
    public StoreDocumentStatus Status { get; set; }
    public int LinesCount { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal? TotalValue { get; set; }
    public string Currency { get; set; } = null!;
    public bool HasAttachment { get; set; }
    public UserRefDto CreatedBy { get; set; } = null!;
    public DateTime CreationTime { get; set; }
}

public class StoreIssueVoucherSummaryPagedResultDto
{
    public long TotalCount { get; set; }
    public List<StoreIssueVoucherSummaryDto> Items { get; set; } = new();
}

public class StoreIssueVoucherLineInputDto
{
    public Guid? ItemId { get; set; }
    public decimal? Quantity { get; set; }
}

public class StoreIssueVoucherCreateInputDto
{
    public string VoucherNumber { get; set; } = null!;
    public DateOnly? IssueDate { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? Note { get; set; }
    public List<StoreIssueVoucherLineInputDto> Lines { get; set; } = null!;
}

public class StoreIssueVoucherUpdateInputDto : StoreIssueVoucherCreateInputDto
{
    public string ConcurrencyStamp { get; set; } = null!;
}

public class StoreIssueVoucherListInput : PagedAndSortedResultRequestDto
{
    public StoreDocumentStatus? Status { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? ItemId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public string? Filter { get; set; }
}

public class StoreDashboardDto
{
    public int ItemsCount { get; set; }
    public int PurchasesCount { get; set; }
    public int IssueVouchersCount { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public decimal TotalStockValue { get; set; }
    public string Currency { get; set; } = null!;
}

public class StorePurchaseReportRowDto
{
    public Guid PurchaseId { get; set; }
    public string ReceiptNumber { get; set; } = null!;
    public string SupplierName { get; set; } = null!;
    public DateOnly PurchaseDate { get; set; }
    public ItemRefDto Item { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public StoreLookupRefDto? Unit { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class StorePurchaseReportDto
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public int PurchasesCount { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = null!;
    public long TotalCount { get; set; }
    public List<StorePurchaseReportRowDto> Items { get; set; } = new();
}

public class StoreStockReportRowDto
{
    public ItemRefDto Item { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public StoreLookupRefDto? Category { get; set; }
    public StoreLookupRefDto? Unit { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal MinQuantity { get; set; }
    public decimal AverageUnitCost { get; set; }
    public decimal StockValue { get; set; }
    public StockStatus StockStatus { get; set; }
}

public class StoreStockReportDto
{
    public int ItemsCount { get; set; }
    public decimal TotalStockValue { get; set; }
    public string Currency { get; set; } = null!;
    public long TotalCount { get; set; }
    public List<StoreStockReportRowDto> Items { get; set; } = new();
}

public class StoreTeamIssueReportRowDto
{
    public StoreLookupRefDto Team { get; set; } = null!;
    public ItemRefDto Item { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public StoreLookupRefDto? Unit { get; set; }
    public decimal Quantity { get; set; }
    public decimal Value { get; set; }
}

public class StoreTeamIssueReportDto
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalValue { get; set; }
    public string Currency { get; set; } = null!;
    public long TotalCount { get; set; }
    public List<StoreTeamIssueReportRowDto> Items { get; set; } = new();
}

public class StoreItemMovementReportDto
{
    public ItemRefDto Item { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public StoreLookupRefDto? Unit { get; set; }
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal TotalIn { get; set; }
    public decimal TotalOut { get; set; }
    public decimal ClosingBalance { get; set; }
    public long TotalCount { get; set; }
    public List<StockMovementDto> Items { get; set; } = new();
}

public class StorePurchaseReportInput : PagedAndSortedResultRequestDto
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public Guid? ItemId { get; set; }
    public string? Filter { get; set; }
}

public class StoreStockReportInput : PagedAndSortedResultRequestDto
{
    public StockStatus? StockStatus { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Filter { get; set; }
}

public class StoreTeamIssueReportInput : PagedAndSortedResultRequestDto
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? ItemId { get; set; }
}

public class StoreItemMovementReportInput : PagedAndSortedResultRequestDto
{
    public Guid? ItemId { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
}

public class StoreReportExportInput
{
    public ReportExportFormat? Format { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public Guid? ItemId { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Filter { get; set; }
}

public interface IItemCategoryAppService : IApplicationService
{
    Task<ItemCategoryPagedResultDto> GetListAsync(StoreCatalogListInput input);
    Task<ItemCategoryDto> CreateAsync(ItemCategoryCreateInputDto input, Guid? idempotencyKey);
    Task<ItemCategoryDto> UpdateAsync(Guid id, ItemCategoryUpdateInputDto input);
}

public interface IItemUnitAppService : IApplicationService
{
    Task<ItemUnitPagedResultDto> GetListAsync(StoreCatalogListInput input);
    Task<ItemUnitDto> CreateAsync(ItemUnitCreateInputDto input, Guid? idempotencyKey);
    Task<ItemUnitDto> UpdateAsync(Guid id, ItemUnitUpdateInputDto input);
}

public interface IStoreTeamAppService : IApplicationService
{
    Task<StoreTeamPagedResultDto> GetListAsync(StoreCatalogListInput input);
    Task<StoreTeamDto> CreateAsync(StoreTeamCreateInputDto input, Guid? idempotencyKey);
    Task<StoreTeamDto> UpdateAsync(Guid id, StoreTeamUpdateInputDto input);
}

public interface IStorePurchaseAppService : IApplicationService
{
    Task<StorePurchaseSummaryPagedResultDto> GetListAsync(StorePurchaseListInput input);
    Task<StorePurchaseDto> CreateAsync(StorePurchaseCreateInputDto input, Guid? idempotencyKey);
    Task<StorePurchaseDto> GetAsync(Guid id);
    Task<StorePurchaseDto> UpdateAsync(Guid id, StorePurchaseUpdateInputDto input);
    Task DeleteAsync(Guid id);
    Task<StorePurchaseDto> ApproveAsync(Guid id);
    Task<StorePurchaseDto> ReverseAsync(Guid id, ReasonInputDto input);
    Task<AttachmentDto> UploadAttachmentAsync(Guid id, UploadedFileInput file);
    Task DeleteAttachmentAsync(Guid id);
    Task<FileDownloadUrlDto> GetAttachmentDownloadUrlAsync(Guid id);
}

public interface IStoreIssueVoucherAppService : IApplicationService
{
    Task<StoreIssueVoucherSummaryPagedResultDto> GetListAsync(StoreIssueVoucherListInput input);
    Task<StoreIssueVoucherDto> CreateAsync(StoreIssueVoucherCreateInputDto input, Guid? idempotencyKey);
    Task<StoreIssueVoucherDto> GetAsync(Guid id);
    Task<StoreIssueVoucherDto> UpdateAsync(Guid id, StoreIssueVoucherUpdateInputDto input);
    Task DeleteAsync(Guid id);
    Task<StoreIssueVoucherDto> ApproveAsync(Guid id);
    Task<StoreIssueVoucherDto> ReverseAsync(Guid id, ReasonInputDto input);
    Task<AttachmentDto> UploadAttachmentAsync(Guid id, UploadedFileInput file);
    Task DeleteAttachmentAsync(Guid id);
    Task<FileDownloadUrlDto> GetAttachmentDownloadUrlAsync(Guid id);
}

public interface IStoreReportAppService : IApplicationService
{
    Task<StoreDashboardDto> GetDashboardAsync();
    Task<StorePurchaseReportDto> GetPurchasesAsync(StorePurchaseReportInput input);
    Task<StoreStockReportDto> GetStockAsync(StoreStockReportInput input);
    Task<StoreTeamIssueReportDto> GetIssuesByTeamAsync(StoreTeamIssueReportInput input);
    Task<StoreItemMovementReportDto> GetItemMovementAsync(StoreItemMovementReportInput input);
    Task<ReportFile> ExportAsync(StoreReportCode code, StoreReportExportInput input);
}
