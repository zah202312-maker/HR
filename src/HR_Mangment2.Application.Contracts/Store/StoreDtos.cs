using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR_Mangment2.Departments;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Requests;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.Store;

public class ItemDto
{
    public Guid Id { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public ItemKind Kind { get; set; }
    public decimal MinQuantity { get; set; }
    public decimal QuantityOnHand { get; set; }
    public bool IsBelowMinimum { get; set; }
    public bool IsActive { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}

public class ItemPagedResultDto
{
    public long TotalCount { get; set; }
    public List<ItemDto> Items { get; set; } = new();
}

public class ItemRefPagedResultDto
{
    public long TotalCount { get; set; }
    public List<ItemRefDto> Items { get; set; } = new();
}

public class ItemCreateInputDto
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public ItemKind? Kind { get; set; }
    public decimal? MinQuantity { get; set; }
}

public class ItemUpdateInputDto : ItemCreateInputDto
{
    public bool? IsActive { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class ItemListInput : PagedAndSortedResultRequestDto
{
    public ItemKind? Kind { get; set; }
    public bool? IsActive { get; set; }
    public bool? BelowMinimum { get; set; }
    public string? Filter { get; set; }
}

public class ItemLookupInput : PagedAndSortedResultRequestDto
{
    public ItemKind? Kind { get; set; }
    public string? Filter { get; set; }
}

public class StockMovementDto
{
    public Guid Id { get; set; }
    public ItemRefDto Item { get; set; } = null!;
    public StockMovementType Type { get; set; }
    public decimal Quantity { get; set; }
    public decimal BalanceAfter { get; set; }
    public Guid? RequestId { get; set; }
    public Guid? StockIssueId { get; set; }
    public Guid? CustodyId { get; set; }
    public Guid? ReversesMovementId { get; set; }
    public EmployeeRefDto? Employee { get; set; }
    public UserRefDto PerformedBy { get; set; } = null!;
    public string? Note { get; set; }
    public DateTime CreationTime { get; set; }
}

public class StockMovementPagedResultDto
{
    public long TotalCount { get; set; }
    public List<StockMovementDto> Items { get; set; } = new();
}

public class StockReceiveInputDto
{
    public Guid? ItemId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? TotalCost { get; set; }
    public Guid? StockIssueId { get; set; }
    public string? Note { get; set; }
}

public class StockAdjustInputDto
{
    public Guid? ItemId { get; set; }
    public decimal? Quantity { get; set; }
    public string Reason { get; set; } = null!;
    public Guid? ReversesMovementId { get; set; }
}

public class StockMovementListInput : PagedAndSortedResultRequestDto
{
    public Guid? ItemId { get; set; }
    public StockMovementType? Type { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}

public class StockIssueDto
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public ItemRefDto Item { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal QuantityOnHand { get; set; }
    public bool IsAvailable { get; set; }
    public StockIssueStatus Status { get; set; }
    public string StatusLabel { get; set; } = null!;
    public EmployeeRefDto Employee { get; set; } = null!;
    public DateTime? AcknowledgedAt { get; set; }
    public UserRefDto? AcknowledgedBy { get; set; }
    public DateTime? PurchasingAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public UserRefDto? DeliveredBy { get; set; }
    public Guid? CustodyId { get; set; }
    public Guid? StockMovementId { get; set; }
    public DateTime CreationTime { get; set; }
}

public class StockIssuePagedResultDto
{
    public long TotalCount { get; set; }
    public List<StockIssueDto> Items { get; set; } = new();
}

public class StockIssueListInput : PagedAndSortedResultRequestDto
{
    public StockIssueStatus? Status { get; set; }
    public Guid? ItemId { get; set; }
    public Guid? EmployeeId { get; set; }
}

public class StockIssueDeliverInputDto
{
    public CustodyKind? CustodyKind { get; set; }
    public string? Note { get; set; }
}

public class CustodyDto
{
    public Guid Id { get; set; }
    public ItemRefDto Item { get; set; } = null!;
    public CustodyKind Kind { get; set; }
    public EmployeeRefDto? Employee { get; set; }
    public DepartmentRefDto? Department { get; set; }
    public decimal Quantity { get; set; }
    public CustodyStatus Status { get; set; }
    public DateTime AssignedAt { get; set; }
    public UserRefDto AssignedBy { get; set; } = null!;
    public DateTime? AcknowledgedAt { get; set; }
    public UserRefDto? AcknowledgedBy { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public Guid? RequestId { get; set; }
    public Guid? StockIssueId { get; set; }
    public string? Note { get; set; }
}

public class CustodyPagedResultDto
{
    public long TotalCount { get; set; }
    public List<CustodyDto> Items { get; set; } = new();
}

public class CustodyCreateInputDto
{
    public Guid? ItemId { get; set; }
    public CustodyKind? Kind { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid? DepartmentId { get; set; }
    public decimal? Quantity { get; set; }
    public bool? DeductFromStock { get; set; }
    public string? Note { get; set; }
}

public class CustodyReturnInputDto
{
    public bool? Restock { get; set; }
    public string? Note { get; set; }
}

public class CustodyListInput : PagedAndSortedResultRequestDto
{
    public CustodyKind? Kind { get; set; }
    public CustodyStatus? Status { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? ItemId { get; set; }
}

public class CustodyMyListInput : PagedAndSortedResultRequestDto
{
    public CustodyStatus? Status { get; set; }
}

public class CustodyTeamListInput : PagedAndSortedResultRequestDto
{
    public CustodyKind? Kind { get; set; }
    public CustodyStatus? Status { get; set; }
    public Guid? EmployeeId { get; set; }
}

public class CustodyCountLineInputDto
{
    public Guid? CustodyId { get; set; }
    public decimal? CountedQuantity { get; set; }
    public string? Note { get; set; }
}

public class CustodyCountCreateInputDto
{
    public Guid? DepartmentId { get; set; }
    public DateOnly? CountDate { get; set; }
    public string? Note { get; set; }
    public List<CustodyCountLineInputDto> Lines { get; set; } = null!;
}

public class CustodyCountLineDto
{
    public Guid CustodyId { get; set; }
    public ItemRefDto Item { get; set; } = null!;
    public decimal ExpectedQuantity { get; set; }
    public decimal CountedQuantity { get; set; }
    public string? Note { get; set; }
}

public class CustodyCountDto
{
    public Guid Id { get; set; }
    public DepartmentRefDto Department { get; set; } = null!;
    public DateOnly CountDate { get; set; }
    public CustodyCountResult Result { get; set; }
    public string? Note { get; set; }
    public UserRefDto CountedBy { get; set; } = null!;
    public List<CustodyCountLineDto> Lines { get; set; } = new();
    public DateTime CreationTime { get; set; }
}

public class CustodyCountPagedResultDto
{
    public long TotalCount { get; set; }
    public List<CustodyCountDto> Items { get; set; } = new();
}

public class CustodyCountListInput : PagedAndSortedResultRequestDto
{
    public Guid? DepartmentId { get; set; }
    public CustodyCountResult? Result { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}

public interface IItemAppService : IApplicationService
{
    Task<ItemPagedResultDto> GetListAsync(ItemListInput input);
    Task<ItemDto> CreateAsync(ItemCreateInputDto input, Guid? idempotencyKey);
    Task<ItemRefPagedResultDto> LookupAsync(ItemLookupInput input);
    Task<ItemDto> GetAsync(Guid id);
    Task<ItemDto> UpdateAsync(Guid id, ItemUpdateInputDto input);
}

public interface IStockAppService : IApplicationService
{
    Task<StockMovementDto> ReceiveAsync(StockReceiveInputDto input, Guid? idempotencyKey);
    Task<StockMovementDto> AdjustAsync(StockAdjustInputDto input, Guid? idempotencyKey);
    Task<StockMovementPagedResultDto> GetMovementsAsync(StockMovementListInput input);
}

public interface IStockIssueAppService : IApplicationService
{
    Task<StockIssuePagedResultDto> GetListAsync(StockIssueListInput input);
    Task<StockIssueDto> GetAsync(Guid id);
    Task<StockIssueDto> AcknowledgeAsync(Guid id);
    Task<StockIssueDto> MarkPurchasingAsync(Guid id, NoteInputDto input);
    Task<StockIssueDto> DeliverAsync(Guid id, StockIssueDeliverInputDto input, Guid? idempotencyKey);
}

public interface ICustodyAppService : IApplicationService
{
    Task<CustodyDto> CreateAsync(CustodyCreateInputDto input, Guid? idempotencyKey);
    Task<CustodyPagedResultDto> GetListAsync(CustodyListInput input);
    Task<CustodyPagedResultDto> GetMyAsync(CustodyMyListInput input);
    Task<CustodyPagedResultDto> GetTeamAsync(CustodyTeamListInput input);
    Task<CustodyDto> GetAsync(Guid id);
    Task<CustodyDto> AcknowledgeAsync(Guid id);
    Task<CustodyDto> ReturnAsync(Guid id, CustodyReturnInputDto input);
}

public interface ICustodyCountAppService : IApplicationService
{
    Task<CustodyCountDto> CreateAsync(CustodyCountCreateInputDto input, Guid? idempotencyKey);
    Task<CustodyCountPagedResultDto> GetListAsync(CustodyCountListInput input);
}
