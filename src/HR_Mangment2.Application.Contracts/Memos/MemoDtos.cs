using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Leaves;
using HR_Mangment2.Requests;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.Memos;

public class MemoCreateInputDto
{
    public MemoKind? Kind { get; set; }
    public string Content { get; set; } = null!;
}

public class MemoDto
{
    public Guid Id { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public MemoKind Kind { get; set; }
    public string Content { get; set; } = null!;
    public MemoStatus Status { get; set; }
    public string StatusLabel { get; set; } = null!;
    public AppRole? CurrentHandlerRole { get; set; }
    public string? CloseReason { get; set; }
    public Guid? ConvertedRequestId { get; set; }
    public Guid? ConvertedIncidentId { get; set; }
    public List<TimelineEntryDto> Timeline { get; set; } = new();
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}

public class MemoListItemDto
{
    public Guid Id { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public MemoKind Kind { get; set; }
    public string ContentPreview { get; set; } = null!;
    public MemoStatus Status { get; set; }
    public string StatusLabel { get; set; } = null!;
    public DateTime CreationTime { get; set; }
}

public class MemoListItemPagedResultDto
{
    public long TotalCount { get; set; }
    public List<MemoListItemDto> Items { get; set; } = new();
}

public class MemoLeaveConversionDto
{
    public LeaveKind? Kind { get; set; }
    public Guid? OtherLeaveTypeId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateOnly? LeaveDate { get; set; }
    public TimeOnly? FromTime { get; set; }
    public TimeOnly? ToTime { get; set; }
}

public class MemoConvertInputDto
{
    public MemoConvertTarget? Target { get; set; }
    public MemoLeaveConversionDto? Leave { get; set; }
    public Guid? ItemId { get; set; }
    public string? Note { get; set; }
}

public class MemoMyListInput : PagedAndSortedResultRequestDto
{
    public MemoStatus? Status { get; set; }
    public MemoKind? Kind { get; set; }
}

public class MemoInboxInput : MemoMyListInput
{
    public Guid? EmployeeId { get; set; }
}

public interface IMemoAppService : IApplicationService
{
    Task<MemoDto> CreateAsync(MemoCreateInputDto input, Guid? idempotencyKey);

    Task<MemoListItemPagedResultDto> GetMyAsync(MemoMyListInput input);

    Task<MemoListItemPagedResultDto> GetInboxAsync(MemoInboxInput input);

    Task<MemoDto> GetAsync(Guid id);

    Task<MemoDto> ForwardAsync(Guid id, NoteInputDto input);

    Task<MemoDto> ConvertAsync(Guid id, MemoConvertInputDto input);

    Task<MemoDto> CloseAsync(Guid id, ReasonInputDto input);
}

public class IncidentCreateInputDto
{
    public Guid? ItemId { get; set; }
    public string Description { get; set; } = null!;
}

public class IncidentDto
{
    public Guid Id { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public ItemRefDto? Item { get; set; }
    public string Description { get; set; } = null!;
    public IncidentStatus Status { get; set; }
    public string StatusLabel { get; set; } = null!;
    public AppRole? CurrentHandlerRole { get; set; }
    public Guid? SourceMemoId { get; set; }
    public List<TimelineEntryDto> Timeline { get; set; } = new();
    public string ConcurrencyStamp { get; set; } = null!;
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}

public class IncidentListItemDto
{
    public Guid Id { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public ItemRefDto? Item { get; set; }
    public string DescriptionPreview { get; set; } = null!;
    public IncidentStatus Status { get; set; }
    public string StatusLabel { get; set; } = null!;
    public DateTime CreationTime { get; set; }
}

public class IncidentListItemPagedResultDto
{
    public long TotalCount { get; set; }
    public List<IncidentListItemDto> Items { get; set; } = new();
}

public class IncidentStatusUpdateInputDto
{
    public IncidentStatusTarget? Status { get; set; }
    public string? Note { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class IncidentMyListInput : PagedAndSortedResultRequestDto
{
    public IncidentStatus? Status { get; set; }
}

public class IncidentInboxInput : IncidentMyListInput
{
    public Guid? EmployeeId { get; set; }
}

public interface IIncidentAppService : IApplicationService
{
    Task<IncidentDto> CreateAsync(IncidentCreateInputDto input, Guid? idempotencyKey);

    Task<IncidentListItemPagedResultDto> GetMyAsync(IncidentMyListInput input);

    Task<IncidentListItemPagedResultDto> GetInboxAsync(IncidentInboxInput input);

    Task<IncidentDto> GetAsync(Guid id);

    Task<IncidentDto> UpdateStatusAsync(Guid id, IncidentStatusUpdateInputDto input);

    Task<IncidentDto> ForwardAsync(Guid id, NoteInputDto input);
}
