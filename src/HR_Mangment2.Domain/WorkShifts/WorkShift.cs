using System;
using System.Collections.Generic;
using System.Linq;
using HR_Mangment2.Errors;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.WorkShifts;

/// <summary>A shift template. It is never deleted: IsActive = false stops new assignments only.</summary>
[Audited]
public class WorkShift : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxNameLength = 128;

    public Guid? TenantId { get; private set; }
    public string NameAr { get; private set; } = null!;
    public string NameEn { get; private set; } = null!;
    public WorkShiftType Type { get; private set; }
    public int GraceMinutes { get; private set; }
    public WorkShiftOvertimePolicy OvertimePolicy { get; private set; }
    public decimal? OvertimeAfterHours { get; private set; }
    public int? RotationCycleDays { get; private set; }
    public bool IsActive { get; private set; }
    public List<WorkShiftDay> Days { get; private set; } = new();

    protected WorkShift()
    {
    }

    public WorkShift(Guid id, Guid? tenantId)
        : base(id)
    {
        TenantId = tenantId;
        IsActive = true;
    }

    public void Set(
        string nameAr,
        string nameEn,
        WorkShiftType type,
        int graceMinutes,
        WorkShiftOvertimePolicy overtimePolicy,
        decimal? overtimeAfterHours,
        int? rotationCycleDays,
        IReadOnlyCollection<WorkShiftDay> days)
    {
        CheckSchedule(type, overtimePolicy, overtimeAfterHours, rotationCycleDays, days);

        NameAr = nameAr;
        NameEn = nameEn;
        Type = type;
        GraceMinutes = graceMinutes;
        OvertimePolicy = overtimePolicy;
        OvertimeAfterHours = overtimeAfterHours;
        RotationCycleDays = rotationCycleDays;

        Days.Clear();
        Days.AddRange(days);
    }

    public void SetActive(bool isActive) => IsActive = isActive;

    private static void CheckSchedule(
        WorkShiftType type,
        WorkShiftOvertimePolicy overtimePolicy,
        decimal? overtimeAfterHours,
        int? rotationCycleDays,
        IReadOnlyCollection<WorkShiftDay> days)
    {
        if (days.Any(d => d.CrossesMidnight != d.EndTime <= d.StartTime))
        {
            throw new BusinessException(HrErrorCodes.Shift.CrossesMidnightMismatch);
        }

        var valid =
            (overtimePolicy == WorkShiftOvertimePolicy.AfterDailyHours) == overtimeAfterHours.HasValue &&
            (type == WorkShiftType.Rotating ? IsValidRotating(rotationCycleDays, days) : IsValidWeekly(type, rotationCycleDays, days));

        if (!valid)
        {
            throw new BusinessException(HrErrorCodes.Shift.InvalidSchedule);
        }
    }

    private static bool IsValidRotating(int? rotationCycleDays, IReadOnlyCollection<WorkShiftDay> days) =>
        rotationCycleDays.HasValue &&
        days.All(d => d.DayOfWeek == null && d.CycleDayNumber >= 1 && d.CycleDayNumber <= rotationCycleDays) &&
        days.Select(d => d.CycleDayNumber).Distinct().Count() == days.Count;

    private static bool IsValidWeekly(WorkShiftType type, int? rotationCycleDays, IReadOnlyCollection<WorkShiftDay> days) =>
        rotationCycleDays == null &&
        days.All(d => d.DayOfWeek != null && d.CycleDayNumber == null) &&
        days.Select(d => d.DayOfWeek).Distinct().Count() == days.Count &&
        (type != WorkShiftType.Overnight || days.Any(d => d.CrossesMidnight));
}

public class WorkShiftDay
{
    public WorkShiftDayOfWeek? DayOfWeek { get; private set; }
    public int? CycleDayNumber { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public bool CrossesMidnight { get; private set; }

    protected WorkShiftDay()
    {
    }

    public WorkShiftDay(WorkShiftDayOfWeek? dayOfWeek, int? cycleDayNumber, TimeOnly startTime, TimeOnly endTime, bool crossesMidnight)
    {
        DayOfWeek = dayOfWeek;
        CycleDayNumber = cycleDayNumber;
        StartTime = startTime;
        EndTime = endTime;
        CrossesMidnight = crossesMidnight;
    }
}
