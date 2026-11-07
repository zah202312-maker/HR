using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Attendance;
using HR_Mangment2.Common;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Localization;
using HR_Mangment2.Permissions;
using HR_Mangment2.Settings;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;

namespace HR_Mangment2.Holidays;

[Authorize]
public class HolidayAppService : HR_Mangment2AppService, IHolidayAppService
{
    private const string CreateOperation = "Holiday_Create";

    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["date"] = nameof(Holiday.Date),
        ["nameAr"] = nameof(Holiday.NameAr),
        ["nameEn"] = nameof(Holiday.NameEn)
    };

    private readonly IRepository<Holiday, Guid> _holidays;
    private readonly IdempotencyStore _idempotency;
    private readonly IAttendanceRecalculator _attendance;
    private readonly HrSettingsStore _settings;

    public HolidayAppService(
        IRepository<Holiday, Guid> holidays,
        IdempotencyStore idempotency,
        IAttendanceRecalculator attendance,
        HrSettingsStore settings)
    {
        _holidays = holidays;
        _idempotency = idempotency;
        _attendance = attendance;
        _settings = settings;
    }

    [Authorize(HrPermissions.HolidayPermissions.View)]
    public virtual async Task<HolidayPagedResultDto> GetListAsync(HolidayListInput input)
    {
        var query = await _holidays.GetQueryableAsync();

        if (input.FromDate.HasValue)
        {
            query = query.Where(x => x.Date >= input.FromDate.Value);
        }

        if (input.ToDate.HasValue)
        {
            query = query.Where(x => x.Date <= input.ToDate.Value);
        }

        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(Holiday.Date), SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new HolidayPagedResultDto { TotalCount = total, Items = items.Select(ToDto).ToList() };
    }

    [Authorize(HrPermissions.HolidayPermissions.Manage)]
    public virtual async Task<HolidayDto> CreateAsync(HolidayWriteDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return ToDto(await _holidays.GetAsync(earlier.Value));
        }

        var date = input.Date!.Value;
        await CheckDateIsFreeAsync(date, null);

        var holiday = new Holiday(GuidGenerator.Create(), CurrentTenant.Id, date, input.NameAr, input.NameEn);
        await _holidays.InsertAsync(holiday, autoSave: true);
        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, input, holiday.Id);
        await _attendance.RecalculateDateAsync(date);

        return ToDto(holiday);
    }

    [Authorize(HrPermissions.HolidayPermissions.Manage)]
    public virtual async Task<HolidayDto> UpdateAsync(Guid id, HolidayUpdateDto input)
    {
        var holiday = await _holidays.GetAsync(id);
        var oldDate = holiday.Date;
        var newDate = input.Date!.Value;
        await CheckDateIsFreeAsync(newDate, id);

        holiday.ConcurrencyStamp = input.ConcurrencyStamp;
        holiday.Set(newDate, input.NameAr, input.NameEn);
        await _holidays.UpdateAsync(holiday, autoSave: true);

        await _attendance.RecalculateDateAsync(oldDate);
        if (newDate != oldDate)
        {
            await _attendance.RecalculateDateAsync(newDate);
        }

        return ToDto(holiday);
    }

    [Authorize(HrPermissions.HolidayPermissions.Manage)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var holiday = await _holidays.GetAsync(id);
        await _holidays.DeleteAsync(holiday, autoSave: true);
        await _attendance.RecalculateDateAsync(holiday.Date);
    }

    [Authorize(HrPermissions.HolidayPermissions.View)]
    public virtual async Task<WeeklyOffSettingDto> GetWeeklyOffAsync() => ToDto(await _settings.GetAsync<WeeklyOffSettings>());

    [Authorize(HrPermissions.HolidayPermissions.Manage)]
    public virtual async Task<WeeklyOffSettingDto> UpdateWeeklyOffAsync(WeeklyOffSettingUpdateDto input) =>
        ToDto(await _settings.SetAsync(new WeeklyOffSettings { Days = input.Days }, input.ConcurrencyStamp));

    private async Task CheckDateIsFreeAsync(DateOnly date, Guid? exceptId)
    {
        if (await _holidays.AnyAsync(x => x.Date == date && x.Id != exceptId))
        {
            throw new HrConflictException(L["Conflict:HolidayDateExists"]);
        }
    }

    private static HolidayDto ToDto(Holiday holiday) => new()
    {
        Id = holiday.Id,
        Date = holiday.Date,
        Name = LocalizedName.Pick(holiday.NameAr, holiday.NameEn),
        NameAr = holiday.NameAr,
        NameEn = holiday.NameEn,
        ConcurrencyStamp = holiday.ConcurrencyStamp
    };

    private static WeeklyOffSettingDto ToDto(SettingsSnapshot<WeeklyOffSettings> snapshot) => new()
    {
        Days = snapshot.Value.Days,
        ConcurrencyStamp = snapshot.ConcurrencyStamp
    };
}

public abstract class HolidayInputValidatorBase<T> : AbstractValidator<T> where T : HolidayWriteDto
{
    protected HolidayInputValidatorBase()
    {
        RuleFor(x => x.Date).NotNull();
        RuleFor(x => x.NameAr).NotNull().Length(1, Holiday.MaxNameLength);
        RuleFor(x => x.NameEn).NotNull().Length(1, Holiday.MaxNameLength);
    }
}

public class HolidayWriteDtoValidator : HolidayInputValidatorBase<HolidayWriteDto>
{
}

public class HolidayUpdateDtoValidator : HolidayInputValidatorBase<HolidayUpdateDto>
{
    public HolidayUpdateDtoValidator()
    {
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public class WeeklyOffSettingUpdateDtoValidator : AbstractValidator<WeeklyOffSettingUpdateDto>
{
    public WeeklyOffSettingUpdateDtoValidator()
    {
        RuleFor(x => x.Days).NotNull();
        RuleFor(x => x.Days)
            .Must(days => days.Distinct().Count() == days.Count)
            .When(x => x.Days != null)
            .WithMessage("Days must be unique.");
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}
