using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Files;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Identity;
using HR_Mangment2.Localization;
using HR_Mangment2.Permissions;
using HR_Mangment2.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace HR_Mangment2.Leaves;

[Authorize]
public class LeaveRequestAppService : HR_Mangment2AppService, ILeaveRequestAppService
{
    private const string CreateOperation = "LeaveRequest_Create";
    private const int MaxCalendarDays = 366;

    private static readonly Dictionary<string, byte[]> AttachmentSignatures = new(StringComparer.OrdinalIgnoreCase)
    {
        ["application/pdf"] = new byte[] { 0x25, 0x50, 0x44, 0x46 },
        ["image/jpeg"] = new byte[] { 0xFF, 0xD8, 0xFF },
        ["image/png"] = new byte[] { 0x89, 0x50, 0x4E, 0x47 }
    };

    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["creationTime"] = nameof(LeaveRequest.SubmittedAt),
        ["startDate"] = nameof(LeaveRequest.RangeStart),
        ["endDate"] = nameof(LeaveRequest.RangeEnd),
        ["kind"] = nameof(LeaveRequest.Kind)
    };

    private readonly IRepository<LeaveRequest, Guid> _leaves;
    private readonly IRepository<LeaveAttachment, Guid> _attachments;
    private readonly IRepository<OtherLeaveType, Guid> _otherTypes;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IRepository<Request, Guid> _requests;
    private readonly LeavePlanner _planner;
    private readonly LeaveSubmissionService _submissions;
    private readonly DedicatedRequestOwnerResolver _owners;
    private readonly RequestWorkflowService _workflow;
    private readonly RequestDeciderReader _deciders;
    private readonly EmployeeReferenceReader _references;
    private readonly IBlobContainer<HrFilesContainer> _files;
    private readonly FileDownloadLinkManager _links;
    private readonly IVirusScanner _scanner;
    private readonly IdempotencyStore _idempotency;
    private readonly ICompanyClock _companyClock;

    public LeaveRequestAppService(
        IRepository<LeaveRequest, Guid> leaves,
        IRepository<LeaveAttachment, Guid> attachments,
        IRepository<OtherLeaveType, Guid> otherTypes,
        IRepository<Employee, Guid> employees,
        IRepository<Request, Guid> requests,
        LeavePlanner planner,
        LeaveSubmissionService submissions,
        DedicatedRequestOwnerResolver owners,
        RequestWorkflowService workflow,
        RequestDeciderReader deciders,
        EmployeeReferenceReader references,
        IBlobContainer<HrFilesContainer> files,
        FileDownloadLinkManager links,
        IVirusScanner scanner,
        IdempotencyStore idempotency,
        ICompanyClock companyClock)
    {
        _leaves = leaves;
        _attachments = attachments;
        _otherTypes = otherTypes;
        _employees = employees;
        _requests = requests;
        _planner = planner;
        _submissions = submissions;
        _owners = owners;
        _workflow = workflow;
        _deciders = deciders;
        _references = references;
        _files = files;
        _links = links;
        _scanner = scanner;
        _idempotency = idempotency;
        _companyClock = companyClock;
    }

    [Authorize(HrPermissions.LeavePermissions.RequestCreate)]
    public virtual async Task<LeaveRequestPreviewDto> PreviewAsync(LeaveRequestPreviewInputDto input)
    {
        var (owner, _) = await ResolveOwnerAsync(input.EmployeeId);
        var plan = await _planner.PlanAsync(owner, ToPlanInput(input));

        return new LeaveRequestPreviewDto
        {
            Kind = plan.Kind,
            CalendarDays = plan.CalendarDays,
            WorkingDays = plan.WorkingDays,
            ExcludedWeeklyOffDays = plan.ExcludedWeeklyOffDays,
            ExcludedHolidayDays = plan.ExcludedHolidayDays,
            RequestedHours = plan.RequestedHours,
            HourlyQuotaHoursUsed = plan.HourlyQuotaHoursUsed,
            HourlyExcessHours = plan.HourlyExcessHours,
            DeductsFromAnnualBalance = plan.DeductsFromAnnualBalance,
            AnnualDays = plan.AnnualDays,
            FromCarriedDays = plan.FromCarriedDays,
            FromEntitlementDays = plan.FromEntitlementDays,
            IsPaid = plan.OtherLeaveType?.IsPaid,
            RequiresAttachment = plan.OtherLeaveType?.RequiresAttachment ?? false,
            Segments = plan.Segments.Select(x => new LeaveYearSegmentDto
            {
                YearStartDate = x.YearStart,
                YearEndDate = x.YearEnd,
                IsFutureYear = x.IsFutureYear,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                WorkingDays = x.WorkingDays,
                FromCarriedDays = x.FromCarriedDays,
                FromEntitlementDays = x.FromEntitlementDays
            }).ToList(),
            DayBreakdown = plan.DayBreakdown.Select(x => new LeaveDayBreakdownDto
            {
                Date = x.Date,
                Classification = x.Classification,
                HolidayName = x.HolidayNameAr == null ? null : LocalizedName.Pick(x.HolidayNameAr, x.HolidayNameEn!),
                IsCounted = x.Classification == LeaveDayClassification.WorkingDay
            }).ToList(),
            AvailableDaysBefore = plan.AvailableDaysBefore,
            RemainingDaysAfter = plan.RemainingDaysAfter,
            AvailableHourlyHoursBefore = plan.AvailableHourlyHoursBefore,
            IsBalanceSufficient = plan.ShortageDays == 0,
            ShortageDays = plan.ShortageDays,
            CanSubmit = plan.Issues.Count == 0,
            Issues = plan.Issues.Select(code => new LeavePreviewIssueDto { Code = code, Message = L[code] }).ToList()
        };
    }

    [Authorize(HrPermissions.LeavePermissions.RequestCreate)]
    public virtual async Task<LeaveRequestDto> CreateAsync(LeaveRequestCreateDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return await GetAsync(earlier.Value);
        }

        var me = CurrentUser.GetId();
        var (owner, isOnBehalf) = await ResolveOwnerAsync(input.EmployeeId);
        var plan = await _planner.PlanAsync(owner, ToPlanInput(input));

        ThrowFirstIssue(plan);

        if (plan.OtherLeaveType is { RequiresAttachment: true } && input.AttachmentIds.Count == 0)
        {
            throw new BusinessException(HrErrorCodes.Leave.AttachmentRequired);
        }

        var attachments = await LoadUsableAttachmentsAsync(input.AttachmentIds, me);

        var leave = await _submissions.SubmitAsync(new LeaveSubmission(owner, plan, me, isOnBehalf, input.Notes, attachments));

        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, input, leave.Id);
        return (await ToDtosAsync(new[] { leave })).Single();
    }

    [Authorize(HrPermissions.LeavePermissions.RequestViewOwn)]
    public virtual async Task<LeaveRequestPagedResultDto> GetMyListAsync(LeaveRequestListInput input)
    {
        var me = await _owners.ResolveAsync(CurrentUser.GetId(), null, HrErrorCodes.Leave.ExecutiveDirectorNotEligible, null);
        return await QueryAsync(me.Owner.Id, input);
    }

    [Authorize(HrPermissions.LeavePermissions.RequestViewEmployee)]
    public virtual async Task<LeaveRequestPagedResultDto> GetListByEmployeeAsync(Guid employeeId, LeaveRequestListInput input)
    {
        var employee = await _employees.FindAsync(employeeId) ?? throw new EntityNotFoundException(typeof(Employee), employeeId);
        var userId = CurrentUser.GetId();

        if (!(await _deciders.GetAsync(userId)).Roles.Contains(HrRoles.HrManager) &&
            !await _employees.AnyAsync(x => x.Id == employee.ManagerEmployeeId && x.UserId == userId))
        {
            throw new AbpAuthorizationException();
        }

        return await QueryAsync(employeeId, input);
    }

    [Authorize(HrPermissions.LeavePermissions.RequestView)]
    public virtual async Task<LeaveRequestDto> GetAsync(Guid id)
    {
        var leave = await LoadAsync(id);
        await CheckCanViewAsync(id);
        return (await ToDtosAsync(new[] { leave })).Single();
    }

    [Authorize(HrPermissions.LeavePermissions.RequestReverse)]
    public virtual async Task<LeaveRequestDto> ReverseAsync(Guid id, HR_Mangment2.Identity.ReasonInputDto input)
    {
        var leave = await LoadAsync(id);
        var employee = await _employees.GetAsync(leave.EmployeeId);
        if (employee.UserId == CurrentUser.GetId())
        {
            throw new BusinessException(HrErrorCodes.Leave.SelfAdjustmentNotAllowed);
        }

        if (leave.State != LeaveRequestState.Approved)
        {
            throw new BusinessException(HrErrorCodes.Leave.RequestNotReversible);
        }

        var reason = input.Reason.Trim();
        await LazyServiceProvider.LazyGetRequiredService<LeaveBalanceManager>().ReverseDeductionsAsync(employee, leave.Id, reason);
        leave.MarkClosed();
        await _leaves.UpdateAsync(leave, autoSave: true);

        var request = await _requests.GetAsync(id);
        request.AddTimeline(GuidGenerator.Create(), Clock.Now, RequestTimelineActions.Cancelled, CurrentUser.GetId(), reason);
        request.Cancel();
        await _requests.UpdateAsync(request, autoSave: true);

        var today = await _companyClock.GetTodayAsync();
        if (leave.RangeStart <= today)
        {
            await LazyServiceProvider.LazyGetRequiredService<HR_Mangment2.Attendance.IAttendanceRecalculator>().RecalculateRangesAsync(new[]
            {
                new HR_Mangment2.Attendance.AttendanceRecalculationRange
                {
                    EmployeeId = leave.EmployeeId,
                    From = leave.RangeStart,
                    To = leave.RangeEnd < today ? leave.RangeEnd : today
                }
            });
        }

        return (await ToDtosAsync(new[] { leave })).Single();
    }

    [Authorize(HrPermissions.LeavePermissions.RequestCreate)]
    public virtual async Task<AttachmentDto> UploadAttachmentAsync(LeaveAttachmentUploadInput input)
    {
        var signatureMatches = AttachmentSignatures.TryGetValue(input.ContentType, out var signature) && input.Content.AsSpan().StartsWith(signature);
        if (!signatureMatches || input.Content.LongLength > LeaveAttachment.MaxSizeBytes || !await IsCleanAsync(input.Content))
        {
            throw new BusinessException(HrErrorCodes.Leave.AttachmentRejected);
        }

        var id = GuidGenerator.Create();
        var blobName = id.ToString("N");
        await _files.SaveAsync(blobName, input.Content);

        var attachment = new LeaveAttachment(id, CurrentTenant.Id, CurrentUser.GetId(), Path.GetFileName(input.FileName), input.ContentType, input.Content.LongLength, blobName);
        await _attachments.InsertAsync(attachment, autoSave: true);
        return ToDto(attachment);
    }

    [Authorize(HrPermissions.LeavePermissions.RequestView)]
    public virtual async Task<FileDownloadUrlDto> GetAttachmentDownloadUrlAsync(Guid id, Guid attachmentId)
    {
        await LoadAsync(id);
        await CheckCanViewAsync(id);

        var attachment = await _attachments.FirstOrDefaultAsync(x => x.Id == attachmentId && x.LeaveRequestId == id)
                         ?? throw new EntityNotFoundException(typeof(LeaveAttachment), attachmentId);

        var link = await _links.IssueAsync(attachment.Id, attachment.BlobName, attachment.FileName, attachment.ContentType);
        return new FileDownloadUrlDto { Url = link.Url, ExpiresAt = link.ExpiresAt };
    }

    [Authorize(HrPermissions.LeavePermissions.CalendarViewTeam)]
    public virtual async Task<LeaveCalendarEntryPagedResultDto> GetTeamCalendarAsync(LeaveCalendarListInput input)
    {
        var from = input.FromDate!.Value;
        var to = input.ToDate!.Value;
        if (to < from || to.DayNumber - from.DayNumber > MaxCalendarDays)
        {
            throw new BusinessException(HrErrorCodes.Leave.InvalidDateRange);
        }

        var userId = CurrentUser.GetId();
        var me = await _employees.FirstOrDefaultAsync(x => x.UserId == userId);
        if (me == null)
        {
            return new LeaveCalendarEntryPagedResultDto();
        }

        var team = (await _employees.GetQueryableAsync()).Where(x => x.ManagerEmployeeId == me.Id).Select(x => x.Id);
        var query = (await _leaves.GetQueryableAsync())
            .Where(x => team.Contains(x.EmployeeId) && x.State != LeaveRequestState.Closed && x.RangeStart <= to && x.RangeEnd >= from);

        if (input.EmployeeId != null)
        {
            query = query.Where(x => x.EmployeeId == input.EmployeeId);
        }

        if (input.IncludePending == false)
        {
            query = query.Where(x => x.State == LeaveRequestState.Approved);
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(LeaveRequest.RangeStart), SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        var requestIds = items.Select(x => x.Id).ToList();
        var requests = (await _requests.GetListAsync(x => requestIds.Contains(x.Id))).ToDictionary(x => x.Id);
        var employees = await _references.GetRefsAsync(items.Select(x => x.EmployeeId));
        var types = await LoadOtherTypesAsync(items);

        return new LeaveCalendarEntryPagedResultDto
        {
            TotalCount = total,
            Items = items.Select(x => new LeaveCalendarEntryDto
            {
                RequestId = x.Id,
                Employee = employees[x.EmployeeId],
                Kind = x.Kind,
                OtherLeaveTypeName = x.OtherLeaveTypeId == null ? null : LocalizedName.Pick(types[x.OtherLeaveTypeId.Value].NameAr, types[x.OtherLeaveTypeId.Value].NameEn),
                StartDate = x.RangeStart,
                EndDate = x.RangeEnd,
                FromTime = x.FromTime,
                ToTime = x.ToTime,
                Status = requests[x.Id].Status,
                WorkingDays = x.Kind == LeaveKind.Hourly ? null : x.WorkingDays,
                RequestedHours = x.RequestedHours
            }).ToList()
        };
    }

    private Task<(Employee Owner, bool IsOnBehalf)> ResolveOwnerAsync(Guid? employeeId) =>
        _owners.ResolveAsync(CurrentUser.GetId(), employeeId, HrErrorCodes.Leave.ExecutiveDirectorNotEligible, HrErrorCodes.Leave.EmployeeNotActive);

    private static LeavePlanInput ToPlanInput(LeaveRequestPreviewInputDto input) =>
        new(input.Kind!.Value, input.Date, input.FromTime, input.ToTime, input.StartDate, input.EndDate, input.OtherLeaveTypeId);

    private static void ThrowFirstIssue(LeavePlan plan)
    {
        if (plan.Issues.Count == 0)
        {
            return;
        }

        var exception = new BusinessException(plan.Issues[0]);
        if (plan.Issues[0] == HrErrorCodes.Leave.InsufficientBalance)
        {
            exception.WithData("availableDays", plan.AvailableDaysBefore)
                .WithData("requestedDays", plan.AnnualDays)
                .WithData("shortageDays", plan.ShortageDays);
        }

        throw exception;
    }

    /// <summary>Only the uploader's own attachments, not yet used and not expired. Anything else is an invalid attachment, whose existence is not revealed further.</summary>
    private async Task<List<LeaveAttachment>> LoadUsableAttachmentsAsync(IReadOnlyCollection<Guid> ids, Guid uploaderId)
    {
        if (ids.Count == 0)
        {
            return new List<LeaveAttachment>();
        }

        var distinct = ids.Distinct().ToList();
        var cutoff = Clock.Now - LeaveAttachment.UnusedLifetime;
        var found = await _attachments.GetListAsync(x =>
            distinct.Contains(x.Id) && x.UploadedByUserId == uploaderId && x.LeaveRequestId == null && x.CreationTime >= cutoff);

        return found.Count == distinct.Count ? found : throw new BusinessException(HrErrorCodes.Leave.InvalidAttachment);
    }

    /// <summary>Scanned before anything is stored. A scanner that cannot answer counts as a rejection, so an unscanned file never gets in.</summary>
    private async Task<bool> IsCleanAsync(byte[] content)
    {
        try
        {
            return await _scanner.ScanAsync(content) == VirusScanResult.Clean;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Virus scan failed for a leave attachment.");
            return false;
        }
    }

    private async Task<LeaveRequest> LoadAsync(Guid id)
    {
        var query = (await _leaves.WithDetailsAsync(x => x.Segments)).Where(x => x.Id == id);
        return await AsyncExecuter.FirstOrDefaultAsync(query) ?? throw new EntityNotFoundException(typeof(LeaveRequest), id);
    }

    private async Task CheckCanViewAsync(Guid requestId)
    {
        var query = (await _requests.WithDetailsAsync(x => x.Steps)).Where(x => x.Id == requestId);
        var request = await AsyncExecuter.FirstAsync(query);

        if (!await _workflow.CanViewAsync(request, await _deciders.GetAsync(CurrentUser.GetId())))
        {
            throw new AbpAuthorizationException();
        }
    }

    private async Task<LeaveRequestPagedResultDto> QueryAsync(Guid employeeId, LeaveRequestListInput input)
    {
        var query = (await _leaves.WithDetailsAsync(x => x.Segments)).Where(x => x.EmployeeId == employeeId);

        if (input.Status != null)
        {
            var matching = (await _requests.GetQueryableAsync()).Where(x => x.Status == input.Status).Select(x => x.Id);
            query = query.Where(x => matching.Contains(x.Id));
        }

        if (input.Kind != null)
        {
            query = query.Where(x => x.Kind == input.Kind);
        }

        if (input.FromDate != null)
        {
            query = query.Where(x => x.RangeEnd >= input.FromDate);
        }

        if (input.ToDate != null)
        {
            query = query.Where(x => x.RangeStart <= input.ToDate);
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(LeaveRequest.SubmittedAt) + " desc", SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new LeaveRequestPagedResultDto { TotalCount = total, Items = await ToDtosAsync(items) };
    }

    private async Task<Dictionary<Guid, OtherLeaveType>> LoadOtherTypesAsync(IReadOnlyCollection<LeaveRequest> leaves)
    {
        var ids = leaves.Where(x => x.OtherLeaveTypeId != null).Select(x => x.OtherLeaveTypeId!.Value).Distinct().ToList();
        return ids.Count == 0
            ? new Dictionary<Guid, OtherLeaveType>()
            : (await _otherTypes.GetListAsync(x => ids.Contains(x.Id))).ToDictionary(x => x.Id);
    }

    private async Task<List<LeaveRequestDto>> ToDtosAsync(IReadOnlyCollection<LeaveRequest> leaves)
    {
        var ids = leaves.Select(x => x.Id).ToList();
        var employeeIds = leaves.Select(x => x.EmployeeId).Distinct().ToList();
        var requests = (await _requests.GetListAsync(x => ids.Contains(x.Id))).ToDictionary(x => x.Id);
        var refs = await _references.GetRefsAsync(employeeIds);
        var users = await _references.GetUserRefsAsync(requests.Values.Select(x => x.SubmittedByUserId));
        var types = await LoadOtherTypesAsync(leaves);
        var attachments = (await _attachments.GetListAsync(x => x.LeaveRequestId != null && ids.Contains(x.LeaveRequestId.Value))).ToLookup(x => x.LeaveRequestId!.Value);
        var hireDates = (await _employees.GetListAsync(x => employeeIds.Contains(x.Id))).ToDictionary(x => x.Id, x => x.HireDate);
        var today = await _companyClock.GetTodayAsync();

        return leaves.Select(leave =>
        {
            var request = requests[leave.Id];
            var currentYearEnd = LeaveYearMath.YearContaining(hireDates[leave.EmployeeId], today).End;

            return new LeaveRequestDto
            {
                RequestId = leave.Id,
                Employee = refs[leave.EmployeeId],
                SubmittedBy = users[request.SubmittedByUserId],
                IsOnBehalf = request.IsOnBehalf,
                Kind = leave.Kind,
                Date = leave.Date,
                FromTime = leave.FromTime,
                ToTime = leave.ToTime,
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                OtherLeaveType = leave.OtherLeaveTypeId == null ? null : OtherLeaveTypeAppService.ToOption(types[leave.OtherLeaveTypeId.Value]),
                Notes = leave.Notes,
                Status = request.Status,
                BalanceEffect = leave.BalanceEffect,
                WorkingDays = leave.WorkingDays,
                RequestedHours = leave.RequestedHours,
                HourlyQuotaHoursUsed = leave.HourlyQuotaHoursUsed,
                HourlyExcessHours = leave.HourlyExcessHours,
                AnnualDays = leave.AnnualDays,
                FromCarriedDays = leave.FromCarriedDays,
                FromEntitlementDays = leave.FromEntitlementDays,
                Segments = leave.Segments.OrderBy(x => x.YearStart).Select(x => new LeaveYearSegmentDto
                {
                    YearStartDate = x.YearStart,
                    YearEndDate = x.YearEnd,
                    IsFutureYear = x.YearStart > currentYearEnd,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    WorkingDays = x.WorkingDays,
                    FromCarriedDays = x.FromCarriedDays,
                    FromEntitlementDays = x.FromEntitlementDays
                }).ToList(),
                Attachments = attachments[leave.Id].Select(ToDto).ToList(),
                CreationTime = leave.SubmittedAt
            };
        }).ToList();
    }

    private static AttachmentDto ToDto(LeaveAttachment attachment) => new()
    {
        Id = attachment.Id,
        FileName = attachment.FileName,
        ContentType = attachment.ContentType,
        SizeBytes = attachment.SizeBytes,
        CreationTime = attachment.CreationTime
    };
}

/// <summary>The contract requires each kind's own fields and requires the others to be absent.</summary>
public abstract class LeaveRequestInputValidator<T> : AbstractValidator<T>
    where T : LeaveRequestPreviewInputDto
{
    protected LeaveRequestInputValidator()
    {
        RuleFor(x => x.Kind).NotNull();

        When(x => x.Kind == LeaveKind.Hourly, () =>
        {
            RuleFor(x => x.Date).NotNull();
            RuleFor(x => x.FromTime).NotNull();
            RuleFor(x => x.ToTime).NotNull();
            RuleFor(x => x.StartDate).Null();
            RuleFor(x => x.EndDate).Null();
            RuleFor(x => x.OtherLeaveTypeId).Null();
        });

        When(x => x.Kind is LeaveKind.Annual or LeaveKind.Other, () =>
        {
            RuleFor(x => x.Date).Null();
            RuleFor(x => x.FromTime).Null();
            RuleFor(x => x.ToTime).Null();
            RuleFor(x => x.StartDate).NotNull();
            RuleFor(x => x.EndDate).NotNull();
        });

        When(x => x.Kind == LeaveKind.Annual, () => RuleFor(x => x.OtherLeaveTypeId).Null());
        When(x => x.Kind == LeaveKind.Other, () => RuleFor(x => x.OtherLeaveTypeId).NotNull());
    }
}

public class LeaveRequestPreviewInputDtoValidator : LeaveRequestInputValidator<LeaveRequestPreviewInputDto>
{
}

public class LeaveRequestCreateDtoValidator : LeaveRequestInputValidator<LeaveRequestCreateDto>
{
    public LeaveRequestCreateDtoValidator()
    {
        RuleFor(x => x.AttachmentIds).Must(x => x == null || x.Count <= 10);
        RuleFor(x => x.Notes).MaximumLength(LeaveRequest.MaxNotesLength);
    }
}

public class LeaveCalendarListInputValidator : AbstractValidator<LeaveCalendarListInput>
{
    public LeaveCalendarListInputValidator()
    {
        RuleFor(x => x.FromDate).NotNull();
        RuleFor(x => x.ToDate).NotNull();
    }
}
