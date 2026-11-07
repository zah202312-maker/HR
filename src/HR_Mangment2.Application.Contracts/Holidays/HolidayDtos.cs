using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR_Mangment2.WorkShifts;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.Holidays;

public class HolidayDto
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public string Name { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string ConcurrencyStamp { get; set; } = null!;
}

public class HolidayPagedResultDto
{
    public long TotalCount { get; set; }
    public List<HolidayDto> Items { get; set; } = new();
}

public class HolidayWriteDto
{
    public DateOnly? Date { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
}

public class HolidayUpdateDto : HolidayWriteDto
{
    public string ConcurrencyStamp { get; set; } = null!;
}

public class HolidayListInput : PagedAndSortedResultRequestDto
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}

public class WeeklyOffSettingDto
{
    public List<WorkShiftDayOfWeek> Days { get; set; } = new();
    public string ConcurrencyStamp { get; set; } = null!;
}

public class WeeklyOffSettingUpdateDto
{
    public List<WorkShiftDayOfWeek> Days { get; set; } = null!;
    public string ConcurrencyStamp { get; set; } = null!;
}

public interface IHolidayAppService : IApplicationService
{
    Task<HolidayPagedResultDto> GetListAsync(HolidayListInput input);
    Task<HolidayDto> CreateAsync(HolidayWriteDto input, Guid? idempotencyKey);
    Task<HolidayDto> UpdateAsync(Guid id, HolidayUpdateDto input);
    Task DeleteAsync(Guid id);
    Task<WeeklyOffSettingDto> GetWeeklyOffAsync();
    Task<WeeklyOffSettingDto> UpdateWeeklyOffAsync(WeeklyOffSettingUpdateDto input);
}
