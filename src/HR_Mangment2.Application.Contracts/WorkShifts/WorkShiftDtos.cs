using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.WorkShifts;

public class WorkShiftDayDto
{
    public WorkShiftDayOfWeek? DayOfWeek { get; set; }
    public int? CycleDayNumber { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public bool? CrossesMidnight { get; set; }
}

public class WorkShiftDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public WorkShiftType Type { get; set; }
    public int GraceMinutes { get; set; }
    public WorkShiftOvertimePolicy OvertimePolicy { get; set; }
    public decimal? OvertimeAfterHours { get; set; }
    public int? RotationCycleDays { get; set; }
    public List<WorkShiftDayDto> Days { get; set; } = new();
    public bool IsActive { get; set; }
    public int AssignedEmployeeCount { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class WorkShiftPagedResultDto
{
    public long TotalCount { get; set; }
    public List<WorkShiftDto> Items { get; set; } = new();
}

public class WorkShiftRefDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public WorkShiftType Type { get; set; }
}

public class WorkShiftCreateDto
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public WorkShiftType? Type { get; set; }
    public int? GraceMinutes { get; set; }
    public WorkShiftOvertimePolicy? OvertimePolicy { get; set; }
    public decimal? OvertimeAfterHours { get; set; }
    public int? RotationCycleDays { get; set; }
    public List<WorkShiftDayDto> Days { get; set; } = null!;
}

public class WorkShiftUpdateDto : WorkShiftCreateDto
{
    public bool? IsActive { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class WorkShiftListInput : PagedAndSortedResultRequestDto
{
    public WorkShiftType? Type { get; set; }
    public bool? IsActive { get; set; }
}

public interface IWorkShiftAppService : IApplicationService
{
    Task<WorkShiftPagedResultDto> GetListAsync(WorkShiftListInput input);
    Task<WorkShiftDto> CreateAsync(WorkShiftCreateDto input, Guid? idempotencyKey);
    Task<WorkShiftDto> GetAsync(Guid id);
    Task<WorkShiftDto> UpdateAsync(Guid id, WorkShiftUpdateDto input);
}
