using System;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Leaves;
using HR_Mangment2.Permissions;
using HR_Mangment2.Requests;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace HR_Mangment2.Attendance;

[Authorize]
public class AttendanceCorrectionAppService : HR_Mangment2AppService, IAttendanceCorrectionAppService
{
    private const string CreateOperation = "AttendanceCorrection_Create";

    private readonly IRepository<AttendanceCorrection, Guid> _corrections;
    private readonly IRepository<AttendanceDay, Guid> _days;
    private readonly IRepository<Request, Guid> _requests;
    private readonly IRepository<RequestType, Guid> _types;
    private readonly AttendanceCorrectionService _service;
    private readonly DedicatedRequestOwnerResolver _owners;
    private readonly RequestWorkflowService _workflow;
    private readonly RequestDeciderReader _deciders;
    private readonly EmployeeReferenceReader _references;
    private readonly IdempotencyStore _idempotency;
    private readonly ICompanyClock _companyClock;

    public AttendanceCorrectionAppService(
        IRepository<AttendanceCorrection, Guid> corrections,
        IRepository<AttendanceDay, Guid> days,
        IRepository<Request, Guid> requests,
        IRepository<RequestType, Guid> types,
        AttendanceCorrectionService service,
        DedicatedRequestOwnerResolver owners,
        RequestWorkflowService workflow,
        RequestDeciderReader deciders,
        EmployeeReferenceReader references,
        IdempotencyStore idempotency,
        ICompanyClock companyClock)
    {
        _corrections = corrections;
        _days = days;
        _requests = requests;
        _types = types;
        _service = service;
        _owners = owners;
        _workflow = workflow;
        _deciders = deciders;
        _references = references;
        _idempotency = idempotency;
        _companyClock = companyClock;
    }

    [Authorize(HrPermissions.AttendancePermissions.CorrectionCreate)]
    public virtual async Task<AttendanceCorrectionDto> CreateAsync(AttendanceCorrectionCreateDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return await GetAsync(earlier.Value);
        }

        var me = CurrentUser.GetId();
        var (owner, isOnBehalf) = await _owners.ResolveAsync(me, input.EmployeeId, HrErrorCodes.Attendance.ExecutiveDirectorNotCovered, null);

        var date = input.Date!.Value;
        var punchType = input.PunchType!.Value;
        var time = input.CorrectTime!.Value;

        if (date > await _companyClock.GetTodayAsync())
        {
            throw new BusinessException(HrErrorCodes.Attendance.CorrectionDateInFuture);
        }

        if (await _corrections.AnyAsync(x => x.EmployeeId == owner.Id && x.Date == date && x.PunchType == punchType && x.IsPending))
        {
            throw new BusinessException(HrErrorCodes.Attendance.CorrectionAlreadyPending);
        }

        if (date < owner.HireDate)
        {
            throw new BusinessException(HrErrorCodes.Attendance.CorrectionTimeInvalid);
        }

        var value = await _service.ResolveAsync(owner.Id, date, punchType, time);
        var day = await _days.FirstOrDefaultAsync(x => x.EmployeeId == owner.Id && x.Date == date);

        var invalid = value > Clock.Now ||
                      (punchType == AttendancePunchType.CheckIn && day?.CheckOut != null && value >= day.CheckOut) ||
                      (punchType == AttendancePunchType.CheckOut && day?.CheckIn != null && value <= day.CheckIn);
        if (invalid)
        {
            throw new BusinessException(HrErrorCodes.Attendance.CorrectionTimeInvalid);
        }

        var type = await _types.FirstAsync(x => x.Code == RequestTypeCodes.AttendanceCorrection);
        var label = time.ToString("HH:mm");
        var request = await _workflow.SubmitAsync(new RequestSubmission(
            type,
            owner,
            me,
            isOnBehalf,
            null,
            $"تصحيح {(punchType == AttendancePunchType.CheckIn ? "دخول" : "خروج")} ليوم {date:yyyy-MM-dd} إلى {label}",
            $"Correct {(punchType == AttendancePunchType.CheckIn ? "check-in" : "check-out")} on {date:yyyy-MM-dd} to {label}"));

        var original = punchType == AttendancePunchType.CheckIn ? day?.CheckIn : day?.CheckOut;
        var correction = await _corrections.InsertAsync(
            new AttendanceCorrection(request.Id, CurrentTenant.Id, owner.Id, date, punchType, time, input.Reason, original, Clock.Now),
            autoSave: true);

        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, input, request.Id);
        return await ToDtoAsync(correction, request);
    }

    [Authorize(HrPermissions.AttendancePermissions.CorrectionView)]
    public virtual async Task<AttendanceCorrectionDto> GetAsync(Guid id)
    {
        var correction = await _corrections.FindAsync(id) ?? throw new EntityNotFoundException(typeof(AttendanceCorrection), id);
        var request = await LoadRequestAsync(id);

        if (!await _workflow.CanViewAsync(request, await _deciders.GetAsync(CurrentUser.GetId())))
        {
            throw new AbpAuthorizationException();
        }

        return await ToDtoAsync(correction, request);
    }

    private async Task<Request> LoadRequestAsync(Guid id)
    {
        var query = System.Linq.Queryable.Where(await _requests.WithDetailsAsync(x => x.Steps), x => x.Id == id);
        return await AsyncExecuter.FirstAsync(query);
    }

    private async Task<AttendanceCorrectionDto> ToDtoAsync(AttendanceCorrection correction, Request request) => new()
    {
        RequestId = correction.Id,
        Employee = (await _references.GetRefsAsync(new[] { correction.EmployeeId }))[correction.EmployeeId],
        SubmittedBy = (await _references.GetUserRefsAsync(new[] { request.SubmittedByUserId }))[request.SubmittedByUserId],
        IsOnBehalf = request.IsOnBehalf,
        Date = correction.Date,
        PunchType = correction.PunchType,
        CorrectTime = correction.CorrectTime,
        Reason = correction.Reason,
        Status = request.Status,
        OriginalValue = correction.OriginalValue,
        AppliedValue = correction.AppliedValue,
        AppliedAt = correction.AppliedAt,
        AffectsClosedMonth = correction.AffectsClosedMonth,
        SettlementPeriod = correction.SettlementPeriod,
        CreationTime = correction.SubmittedAt
    };
}

public class AttendanceCorrectionCreateDtoValidator : AbstractValidator<AttendanceCorrectionCreateDto>
{
    public AttendanceCorrectionCreateDtoValidator()
    {
        RuleFor(x => x.Date).NotNull();
        RuleFor(x => x.PunchType).NotNull();
        RuleFor(x => x.CorrectTime).NotNull();
        RuleFor(x => x.Reason).NotNull().Length(1, AttendanceCorrection.MaxReasonLength);
    }
}
