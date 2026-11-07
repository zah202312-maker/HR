using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Localization;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;

namespace HR_Mangment2.WorkShifts;

[Authorize]
public class WorkShiftAppService : HR_Mangment2AppService, IWorkShiftAppService
{
    private const string CreateOperation = "WorkShift_Create";

    private readonly IRepository<WorkShift, Guid> _shifts;
    private readonly IWorkShiftUsageReader _usage;
    private readonly IdempotencyStore _idempotency;

    public WorkShiftAppService(IRepository<WorkShift, Guid> shifts, IWorkShiftUsageReader usage, IdempotencyStore idempotency)
    {
        _shifts = shifts;
        _usage = usage;
        _idempotency = idempotency;
    }

    private static string DefaultSorting => LocalizedName.IsEnglish ? nameof(WorkShift.NameEn) : nameof(WorkShift.NameAr);

    private static Dictionary<string, string> SortFields => new()
    {
        ["name"] = DefaultSorting,
        ["nameAr"] = nameof(WorkShift.NameAr),
        ["nameEn"] = nameof(WorkShift.NameEn),
        ["type"] = nameof(WorkShift.Type),
        ["isActive"] = nameof(WorkShift.IsActive),
        ["creationTime"] = nameof(WorkShift.CreationTime)
    };

    [Authorize(HrPermissions.WorkShiftPermissions.View)]
    public virtual async Task<WorkShiftPagedResultDto> GetListAsync(WorkShiftListInput input)
    {
        var query = await _shifts.GetQueryableAsync();

        if (input.Type.HasValue)
        {
            query = query.Where(x => x.Type == input.Type.Value);
        }

        if (input.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == input.IsActive.Value);
        }

        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, DefaultSorting, SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));
        var counts = await _usage.GetAssignedEmployeeCountsAsync(items.Select(x => x.Id).ToList());

        return new WorkShiftPagedResultDto
        {
            TotalCount = total,
            Items = items.Select(x => ToDto(x, counts.GetValueOrDefault(x.Id))).ToList()
        };
    }

    [Authorize(HrPermissions.WorkShiftPermissions.Manage)]
    public virtual async Task<WorkShiftDto> CreateAsync(WorkShiftCreateDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return await GetAsync(earlier.Value);
        }

        var shift = new WorkShift(GuidGenerator.Create(), CurrentTenant.Id);
        Apply(shift, input);
        await _shifts.InsertAsync(shift, autoSave: true);
        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, input, shift.Id);

        return ToDto(shift, 0);
    }

    [Authorize(HrPermissions.WorkShiftPermissions.View)]
    public virtual async Task<WorkShiftDto> GetAsync(Guid id)
    {
        var shift = await _shifts.GetAsync(id);
        return ToDto(shift, await GetAssignedCountAsync(id));
    }

    [Authorize(HrPermissions.WorkShiftPermissions.Manage)]
    public virtual async Task<WorkShiftDto> UpdateAsync(Guid id, WorkShiftUpdateDto input)
    {
        var shift = await _shifts.GetAsync(id);
        shift.ConcurrencyStamp = input.ConcurrencyStamp;
        Apply(shift, input);
        shift.SetActive(input.IsActive!.Value);
        await _shifts.UpdateAsync(shift, autoSave: true);

        return ToDto(shift, await GetAssignedCountAsync(id));
    }

    private static void Apply(WorkShift shift, WorkShiftCreateDto input) => shift.Set(
        input.NameAr,
        input.NameEn,
        input.Type!.Value,
        input.GraceMinutes!.Value,
        input.OvertimePolicy!.Value,
        input.OvertimeAfterHours,
        input.RotationCycleDays,
        input.Days.Select(d => new WorkShiftDay(d.DayOfWeek, d.CycleDayNumber, d.StartTime!.Value, d.EndTime!.Value, d.CrossesMidnight!.Value)).ToList());

    private async Task<int> GetAssignedCountAsync(Guid id) =>
        (await _usage.GetAssignedEmployeeCountsAsync(new[] { id })).GetValueOrDefault(id);

    private static WorkShiftDto ToDto(WorkShift shift, int assignedEmployeeCount) => new()
    {
        Id = shift.Id,
        Name = LocalizedName.Pick(shift.NameAr, shift.NameEn),
        NameAr = shift.NameAr,
        NameEn = shift.NameEn,
        Type = shift.Type,
        GraceMinutes = shift.GraceMinutes,
        OvertimePolicy = shift.OvertimePolicy,
        OvertimeAfterHours = shift.OvertimeAfterHours,
        RotationCycleDays = shift.RotationCycleDays,
        Days = shift.Days
            .OrderBy(d => d.CycleDayNumber ?? (int?)d.DayOfWeek)
            .Select(d => new WorkShiftDayDto
            {
                DayOfWeek = d.DayOfWeek,
                CycleDayNumber = d.CycleDayNumber,
                StartTime = d.StartTime,
                EndTime = d.EndTime,
                CrossesMidnight = d.CrossesMidnight
            })
            .ToList(),
        IsActive = shift.IsActive,
        AssignedEmployeeCount = assignedEmployeeCount,
        CreationTime = shift.CreationTime,
        LastModificationTime = shift.LastModificationTime,
        ConcurrencyStamp = shift.ConcurrencyStamp
    };
}

public class WorkShiftDayDtoValidator : AbstractValidator<WorkShiftDayDto>
{
    public WorkShiftDayDtoValidator()
    {
        RuleFor(x => x.CycleDayNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.StartTime).NotNull();
        RuleFor(x => x.EndTime).NotNull();
        RuleFor(x => x.CrossesMidnight).NotNull();
    }
}

public abstract class WorkShiftInputValidatorBase<T> : AbstractValidator<T> where T : WorkShiftCreateDto
{
    protected WorkShiftInputValidatorBase()
    {
        RuleFor(x => x.NameAr).NotNull().Length(1, WorkShift.MaxNameLength);
        RuleFor(x => x.NameEn).NotNull().Length(1, WorkShift.MaxNameLength);
        RuleFor(x => x.Type).NotNull();
        RuleFor(x => x.GraceMinutes).NotNull().InclusiveBetween(0, 240);
        RuleFor(x => x.OvertimePolicy).NotNull();
        RuleFor(x => x.OvertimeAfterHours).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RotationCycleDays).InclusiveBetween(2, 366);
        RuleFor(x => x.Days).NotEmpty();
        RuleForEach(x => x.Days).SetValidator(new WorkShiftDayDtoValidator());
    }
}

public class WorkShiftCreateDtoValidator : WorkShiftInputValidatorBase<WorkShiftCreateDto>
{
}

public class WorkShiftUpdateDtoValidator : WorkShiftInputValidatorBase<WorkShiftUpdateDto>
{
    public WorkShiftUpdateDtoValidator()
    {
        RuleFor(x => x.IsActive).NotNull();
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}
