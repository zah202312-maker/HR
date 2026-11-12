using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.Finance;

// Salary delivery and petty cash (decision 2026-11-11). Names follow the contract schemas.

public class PayrollDeliveryInputDto
{
    public List<Guid>? EmployeeIds { get; set; }
    public DateOnly? DeliveredOn { get; set; }
}

public class PayrollUndoDeliveryInputDto
{
    public List<Guid> EmployeeIds { get; set; } = null!;
    public string Reason { get; set; } = null!;
}

public class PettyCashTypeDto
{
    public Guid Id { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public bool IsActive { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
    public DateTime CreationTime { get; set; }
}

public class PettyCashTypePagedResultDto
{
    public long TotalCount { get; set; }
    public List<PettyCashTypeDto> Items { get; set; } = new();
}

public class PettyCashTypeCreateInputDto
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
}

public class PettyCashTypeUpdateInputDto : PettyCashTypeCreateInputDto
{
    public bool? IsActive { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class PettyCashTypeRefDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

public class PettyCashTypeListInput : PagedAndSortedResultRequestDto
{
    public bool? IsActive { get; set; }
}

public class PettyCashAssignmentDto
{
    public Guid Id { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public PettyCashTypeRefDto Type { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public string FromPeriod { get; set; } = null!;
    public string? ToPeriod { get; set; }
    public string? Note { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
    public DateTime CreationTime { get; set; }
}

public class PettyCashAssignmentPagedResultDto
{
    public long TotalCount { get; set; }
    public List<PettyCashAssignmentDto> Items { get; set; } = new();
}

public class PettyCashAssignmentCreateInputDto
{
    public Guid? EmployeeId { get; set; }
    public Guid? TypeId { get; set; }
    public decimal? Amount { get; set; }
    public string FromPeriod { get; set; } = null!;
    public string? ToPeriod { get; set; }
    public string? Note { get; set; }
}

public class PettyCashAssignmentUpdateInputDto
{
    public decimal? Amount { get; set; }
    public string? ToPeriod { get; set; }
    public string? Note { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class PettyCashAssignmentListInput : PagedAndSortedResultRequestDto
{
    public Guid? EmployeeId { get; set; }
    public Guid? TypeId { get; set; }
    public string? ActiveIn { get; set; }
}

public class PettyCashEntryDto
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public PettyCashTypeRefDto Type { get; set; } = null!;
    public string Period { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public PettyCashEntryStatus Status { get; set; }
    public DateOnly? DeliveredOn { get; set; }
    public UserRefDto? DeliveredBy { get; set; }
    public Guid? ExpenseId { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class PettyCashMonthDto
{
    public string Period { get; set; } = null!;
    public string Currency { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public decimal DeliveredAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public long TotalCount { get; set; }
    public List<PettyCashEntryDto> Items { get; set; } = new();
}

public class PettyCashMonthInput : PagedAndSortedResultRequestDto
{
    public string Period { get; set; } = null!;
    public PettyCashEntryStatus? Status { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid? TypeId { get; set; }
}

public class PettyCashEntryUpdateInputDto
{
    public decimal? Amount { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class PettyCashDeliveryInputDto
{
    public List<Guid> EntryIds { get; set; } = null!;
    public DateOnly? DeliveredOn { get; set; }
}

public class PettyCashEntryListDto
{
    public List<PettyCashEntryDto> Items { get; set; } = new();
}

public interface IPettyCashAppService : IApplicationService
{
    Task<PettyCashTypePagedResultDto> GetTypesAsync(PettyCashTypeListInput input);
    Task<PettyCashTypeDto> CreateTypeAsync(PettyCashTypeCreateInputDto input, Guid? idempotencyKey);
    Task<PettyCashTypeDto> UpdateTypeAsync(Guid id, PettyCashTypeUpdateInputDto input);
    Task<PettyCashAssignmentPagedResultDto> GetAssignmentsAsync(PettyCashAssignmentListInput input);
    Task<PettyCashAssignmentDto> CreateAssignmentAsync(PettyCashAssignmentCreateInputDto input, Guid? idempotencyKey);
    Task<PettyCashAssignmentDto> UpdateAssignmentAsync(Guid id, PettyCashAssignmentUpdateInputDto input);
    Task<PettyCashMonthDto> GetMonthAsync(PettyCashMonthInput input);
    Task<PettyCashEntryDto> UpdateEntryAsync(Guid id, PettyCashEntryUpdateInputDto input);
    Task<PettyCashEntryListDto> DeliverAsync(PettyCashDeliveryInputDto input);
    Task<PettyCashEntryDto> UndoDeliveryAsync(Guid id, ReasonInputDto input);
}
