using System;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Leaves;
using HR_Mangment2.Requests;
using Volo.Abp.Auditing;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;

namespace HR_Mangment2.Attendance;

/// <summary>The details of a correction request, sharing the general request's id. The raw punches are never touched.</summary>
[Audited]
public class AttendanceCorrection : Entity<Guid>, IMultiTenant
{
    public const int MaxReasonLength = 2000;
    public const int SettlementPeriodLength = 7;

    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public DateOnly Date { get; private set; }
    public AttendancePunchType PunchType { get; private set; }
    public TimeOnly CorrectTime { get; private set; }
    public string Reason { get; private set; } = null!;

    /// <summary>True until a final decision, so the unique pending check is one query.</summary>
    public bool IsPending { get; private set; }

    /// <summary>The value on the day at submission, shown to the approver.</summary>
    public DateTime? OriginalValue { get; private set; }

    /// <summary>The value the correction replaced when it was applied, for the history.</summary>
    public DateTime? ReplacedValue { get; private set; }

    public DateTime? AppliedValue { get; private set; }
    public DateTime? AppliedAt { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public bool? AffectsClosedMonth { get; private set; }
    public string? SettlementPeriod { get; private set; }
    public DateTime SubmittedAt { get; private set; }

    protected AttendanceCorrection()
    {
    }

    public AttendanceCorrection(
        Guid requestId,
        Guid? tenantId,
        Guid employeeId,
        DateOnly date,
        AttendancePunchType punchType,
        TimeOnly correctTime,
        string reason,
        DateTime? originalValue,
        DateTime submittedAt)
        : base(requestId)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        Date = date;
        PunchType = punchType;
        CorrectTime = correctTime;
        Reason = reason;
        OriginalValue = originalValue;
        SubmittedAt = submittedAt;
        IsPending = true;
    }

    public void MarkApplied(DateTime? replacedValue, DateTime appliedValue, DateTime appliedAt, Guid? approvedByUserId, bool affectsClosedMonth, string? settlementPeriod)
    {
        IsPending = false;
        ReplacedValue = replacedValue;
        AppliedValue = appliedValue;
        AppliedAt = appliedAt;
        ApprovedByUserId = approvedByUserId;
        AffectsClosedMonth = affectsClosedMonth;
        SettlementPeriod = settlementPeriod;
    }

    public void MarkClosed() => IsPending = false;
}

/// <summary>Whether payroll has closed a month. The finance module supplies the real answer in release 3.</summary>
public interface IAttendanceClosedMonthReader
{
    Task<bool> IsClosedAsync(DateOnly date);
}

/// <summary>Used until the finance module exists: no month is closed yet.</summary>
public class NullAttendanceClosedMonthReader : IAttendanceClosedMonthReader, ITransientDependency
{
    public Task<bool> IsClosedAsync(DateOnly date) => Task.FromResult(false);
}

/// <summary>Turns the requested time into a moment, and applies it when the request is finally approved.</summary>
[ExposeServices(typeof(IRequestLifecycleHandler), typeof(AttendanceCorrectionService))]
public class AttendanceCorrectionService : IRequestLifecycleHandler, ITransientDependency
{
    private readonly IRepository<AttendanceCorrection, Guid> _corrections;
    private readonly IRepository<AttendanceDay, Guid> _days;
    private readonly AttendanceDayCalculator _calculator;
    private readonly EmployeeScheduleReader _schedule;
    private readonly IAttendanceClosedMonthReader _closedMonths;
    private readonly ICompanyClock _companyClock;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly IClock _clock;

    public AttendanceCorrectionService(
        IRepository<AttendanceCorrection, Guid> corrections,
        IRepository<AttendanceDay, Guid> days,
        AttendanceDayCalculator calculator,
        EmployeeScheduleReader schedule,
        IAttendanceClosedMonthReader closedMonths,
        ICompanyClock companyClock,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IClock clock)
    {
        _corrections = corrections;
        _days = days;
        _calculator = calculator;
        _schedule = schedule;
        _closedMonths = closedMonths;
        _companyClock = companyClock;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _clock = clock;
    }

    /// <summary>On a night shift, a checkout earlier than the shift's start belongs to the next morning.</summary>
    public virtual async Task<DateTime> ResolveAsync(Guid employeeId, DateOnly date, AttendancePunchType punchType, TimeOnly time)
    {
        var shiftDay = (await _schedule.GetAsync(employeeId, date, date))[0].ShiftDay;
        var nextDay = punchType == AttendancePunchType.CheckOut && shiftDay is { CrossesMidnight: true } && time < shiftDay.StartTime;
        return CompanyClock.ToUtc(nextDay ? date.AddDays(1) : date, time, await _companyClock.GetTimeZoneAsync());
    }

    public virtual async Task OnApprovedAsync(Request request)
    {
        var correction = await _corrections.FindAsync(request.Id);
        if (correction == null)
        {
            return;
        }

        var value = await ResolveAsync(correction.EmployeeId, correction.Date, correction.PunchType, correction.CorrectTime);
        var now = _clock.Now;

        var day = await _days.FirstOrDefaultAsync(x => x.EmployeeId == correction.EmployeeId && x.Date == correction.Date);
        DateTime? replaced;
        if (day == null)
        {
            // A day with no row yet: the calculation below fills in the rest around the corrected time.
            var start = await _companyClock.StartOfDayUtcAsync(correction.Date);
            day = new AttendanceDay(_guidGenerator.Create(), _currentTenant.Id, correction.EmployeeId, correction.Date);
            day.SetWindow(start, start.AddDays(1));
            replaced = null;
            day.ApplyCorrection(correction.PunchType, value);
            await _days.InsertAsync(day, autoSave: true);
        }
        else
        {
            replaced = correction.PunchType == AttendancePunchType.CheckIn ? day.CheckIn : day.CheckOut;
            day.ApplyCorrection(correction.PunchType, value);
            await _days.UpdateAsync(day, autoSave: true);
        }

        var approvedBy = request.Steps.Where(x => x.DecidedByUserId != null).OrderBy(x => x.Order).LastOrDefault()?.DecidedByUserId;
        var closed = await _closedMonths.IsClosedAsync(correction.Date);
        correction.MarkApplied(replaced, value, now, approvedBy, closed, closed ? correction.Date.AddMonths(1).ToString("yyyy-MM") : null);
        await _corrections.UpdateAsync(correction, autoSave: true);

        await _calculator.RecalculateAsync(correction.EmployeeId, correction.Date, correction.Date);
    }

    public virtual Task OnRejectedAsync(Request request) => CloseAsync(request);

    public virtual Task OnCancelledAsync(Request request) => CloseAsync(request);

    private async Task CloseAsync(Request request)
    {
        var correction = await _corrections.FindAsync(request.Id);
        if (correction != null)
        {
            correction.MarkClosed();
            await _corrections.UpdateAsync(correction, autoSave: true);
        }
    }
}
