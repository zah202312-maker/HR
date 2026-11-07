using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Common;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Files;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Identity;
using HR_Mangment2.Leaves;
using HR_Mangment2.Permissions;
using HR_Mangment2.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Auditing;
using Volo.Abp.Authorization;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Settings;
using Volo.Abp.Users;
using Volo.Abp.Validation;

namespace HR_Mangment2.Complaints;

/// <summary>
/// Nothing here may reach a log or the general audit trail. Permissions only open the door for submitting: everything
/// about one complaint goes through ComplaintAccessPolicy, and a refusal is always a 404 or an empty list.
/// </summary>
[Authorize]
[DisableAuditing]
public class ComplaintAppService : HR_Mangment2AppService, IComplaintAppService
{
    private const string SubmitOperation = "Complaint_Submit";
    private const string ActionOperation = "Complaint_AddAction";
    private const string UploadOperation = "Complaint_UploadAttachment";
    private const int ReferenceLength = 12;
    private const string ReferenceAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["submittedAt"] = nameof(Complaint.SubmittedAt),
        ["status"] = nameof(Complaint.Status),
        ["level"] = nameof(Complaint.Level),
        ["dueAt"] = nameof(Complaint.DueAt),
        ["closedAt"] = nameof(Complaint.ClosedAt)
    };

    private static readonly Dictionary<string, string> LogSortFields = new()
    {
        ["at"] = nameof(ComplaintAccessLog.At),
        ["action"] = nameof(ComplaintAccessLog.Action)
    };

    private static readonly Dictionary<string, string> RespondentSortFields = new()
    {
        ["fullName"] = nameof(Employee.FullNameAr)
    };

    private static readonly Dictionary<string, byte[]> FileSignatures = new(StringComparer.OrdinalIgnoreCase)
    {
        ["application/pdf"] = new byte[] { 0x25, 0x50, 0x44, 0x46 },
        ["image/jpeg"] = new byte[] { 0xFF, 0xD8, 0xFF },
        ["image/png"] = new byte[] { 0x89, 0x50, 0x4E, 0x47 }
    };

    private readonly IRepository<Complaint, Guid> _complaints;
    private readonly IRepository<ComplaintAction, Guid> _actions;
    private readonly IRepository<ComplaintAttachment, Guid> _attachments;
    private readonly IRepository<ComplaintAccessLog, Guid> _accessLogs;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IRepository<IdentityUser, Guid> _userRecords;
    private readonly IIdentityUserRepository _users;
    private readonly IdentityUserManager _userManager;
    private readonly ComplaintAccessPolicy _policy;
    private readonly ComplaintKeyService _keys;
    private readonly ComplaintNotifier _notifier;
    private readonly EmployeeReferenceReader _references;
    private readonly IdempotencyStore _idempotency;
    private readonly HrSettingsStore _settings;
    private readonly ISettingProvider _settingProvider;
    private readonly WorkCalendarService _calendar;
    private readonly ICompanyClock _companyClock;
    private readonly IBlobContainer<HrFilesContainer> _files;
    private readonly IVirusScanner _scanner;

    public ComplaintAppService(
        IRepository<Complaint, Guid> complaints,
        IRepository<ComplaintAction, Guid> actions,
        IRepository<ComplaintAttachment, Guid> attachments,
        IRepository<ComplaintAccessLog, Guid> accessLogs,
        IRepository<Employee, Guid> employees,
        IRepository<IdentityUser, Guid> userRecords,
        IIdentityUserRepository users,
        IdentityUserManager userManager,
        ComplaintAccessPolicy policy,
        ComplaintKeyService keys,
        ComplaintNotifier notifier,
        EmployeeReferenceReader references,
        IdempotencyStore idempotency,
        HrSettingsStore settings,
        ISettingProvider settingProvider,
        WorkCalendarService calendar,
        ICompanyClock companyClock,
        IBlobContainer<HrFilesContainer> files,
        IVirusScanner scanner)
    {
        _complaints = complaints;
        _actions = actions;
        _attachments = attachments;
        _accessLogs = accessLogs;
        _employees = employees;
        _userRecords = userRecords;
        _users = users;
        _userManager = userManager;
        _policy = policy;
        _keys = keys;
        _notifier = notifier;
        _references = references;
        _idempotency = idempotency;
        _settings = settings;
        _settingProvider = settingProvider;
        _calendar = calendar;
        _companyClock = companyClock;
        _files = files;
        _scanner = scanner;
    }

    [Authorize(HrPermissions.ComplaintsPermissions.Submit)]
    public virtual async Task<ComplaintRespondentPagedResultDto> GetRespondentsAsync(ComplaintRespondentListInput input)
    {
        var me = CurrentUser.GetId();
        var excluded = (await _userManager.GetUsersInRoleAsync(HrRoles.ExecutiveDirector))
            .Concat(await _userManager.GetUsersInRoleAsync(HrRoles.SystemAdmin))
            .Select(x => x.Id)
            .Append(me)
            .Distinct()
            .ToList();

        var activeUsers = (await _userRecords.GetQueryableAsync()).Where(x => x.IsActive).Select(x => x.Id);
        var query = (await _employees.GetQueryableAsync()).Where(x =>
            x.UserId != null &&
            x.Status == EmployeeStatus.Active &&
            activeUsers.Contains(x.UserId.Value) &&
            !excluded.Contains(x.UserId.Value));

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var filter = input.Filter.Trim().ToLower();
            query = query.Where(x => x.FullNameAr.ToLower().Contains(filter) || (x.FullNameEn != null && x.FullNameEn.ToLower().Contains(filter)));
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(Employee.FullNameAr), RespondentSortFields).Skip(input.SkipCount).Take(input.MaxResultCount));
        var refs = await _references.ToRefsAsync(items);

        return new ComplaintRespondentPagedResultDto { TotalCount = total, Items = items.Select(x => refs[x.Id]).ToList() };
    }

    [Authorize(HrPermissions.ComplaintsPermissions.Submit)]
    public virtual async Task<ComplaintRoutingPreviewDto> PreviewRoutingAsync(ComplaintRoutingPreviewInputDto input)
    {
        var plan = await PlanAsync(input);
        var levels = ComplaintRoutingService.SelectableLevels(plan.MinimumLevel);

        return new ComplaintRoutingPreviewDto
        {
            MinimumLevel = plan.MinimumLevel,
            SelectableLevels = levels.ToList(),
            Routes = levels.Select(level => new ComplaintRouteOptionDto
            {
                Level = level,
                HandlerPath = ComplaintRoutingService.RouteFor(level, plan.ManagerUserId != null).ToList()
            }).ToList()
        };
    }

    [Authorize(HrPermissions.ComplaintsPermissions.Submit)]
    public virtual async Task<ComplaintSubmitResultDto> SubmitAsync(ComplaintCreateDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(SubmitOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return ToSubmitResult(await _complaints.GetAsync(earlier.Value, includeDetails: false));
        }

        var plan = await PlanAsync(input);
        var me = CurrentUser.GetId();
        var requested = input.Level!.Value;
        var level = requested < plan.MinimumLevel ? plan.MinimumLevel : requested;
        var route = ComplaintRoutingService.RouteFor(level, plan.ManagerUserId != null);
        var status = ComplaintRoutingService.FirstStatus(route);

        var id = GuidGenerator.Create();
        var (key, wrappedKey) = await _keys.CreateAsync(id, CurrentTenant.Id);
        var now = Clock.Now;

        var complaint = new Complaint(
            id,
            CurrentTenant.Id,
            NewReferenceNumber(),
            plan.Complainant.Id,
            me,
            input.TargetKind!.Value,
            plan.Respondent?.Id,
            plan.Respondent?.UserId,
            plan.Profile.Category,
            level,
            plan.MinimumLevel,
            level != requested,
            status,
            status == ComplaintStatus.PendingDepartmentManager ? plan.ManagerUserId : null,
            wrappedKey,
            key.SealText(ComplaintFields.Subject, input.Subject),
            key.SealText(ComplaintFields.Content, input.Content),
            now,
            await GetDueAtAsync());

        if ((await _policy.GetHandlerUserIdsAsync(complaint)).Count == 0)
        {
            throw new BusinessException(HrErrorCodes.Complaints.NoEligibleHandler);
        }

        complaint.AddAction(GuidGenerator.Create(), ComplaintActionType.Submitted, me, null, now);
        await _complaints.InsertAsync(complaint, autoSave: true);

        await _notifier.NotifyHandlersAsync(complaint);
        await _idempotency.SaveAsync(SubmitOperation, idempotencyKey, input, id);
        return ToSubmitResult(complaint);
    }

    public virtual async Task<ComplaintComplainantSummaryPagedResultDto> GetMyListAsync(ComplaintMyListInput input)
    {
        var me = CurrentUser.GetId();
        var query = ApplyFilters((await _complaints.GetQueryableAsync()).Where(x => x.ComplainantUserId == me), input);

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(Complaint.SubmittedAt) + " desc", SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        var result = new List<ComplaintComplainantSummaryDto>();
        foreach (var complaint in items)
        {
            var key = await _keys.OpenAsync(complaint);
            result.Add(new ComplaintComplainantSummaryDto
            {
                Id = complaint.Id,
                ReferenceNumber = complaint.ReferenceNumber,
                Subject = key.OpenText(ComplaintFields.Subject, complaint.EncryptedSubject),
                Level = complaint.Level,
                Status = complaint.Status,
                SubmittedAt = complaint.SubmittedAt,
                ClosedAt = complaint.ClosedAt
            });
        }

        return new ComplaintComplainantSummaryPagedResultDto { TotalCount = total, Items = result };
    }

    public virtual async Task<ComplaintComplainantDetailDto> GetMyDetailAsync(Guid id)
    {
        var me = CurrentUser.GetId();
        var complaint = await FindAsync(id);
        if (complaint == null || complaint.ComplainantUserId != me)
        {
            throw NotFound(id);
        }

        await WriteLogAsync(complaint, me, ComplaintAccessAction.ViewDetail);

        var key = await _keys.OpenAsync(complaint);
        var respondent = complaint.RespondentEmployeeId == null ? null : (await _references.GetRefsAsync(new[] { complaint.RespondentEmployeeId.Value }))[complaint.RespondentEmployeeId.Value];

        return new ComplaintComplainantDetailDto
        {
            Id = complaint.Id,
            ReferenceNumber = complaint.ReferenceNumber,
            TargetKind = complaint.TargetKind,
            Respondent = respondent,
            Level = complaint.Level,
            Status = complaint.Status,
            Subject = key.OpenText(ComplaintFields.Subject, complaint.EncryptedSubject),
            Content = key.OpenText(ComplaintFields.Content, complaint.EncryptedContent),
            Attachments = complaint.Attachments.Where(x => x.UploadedByUserId == me).OrderBy(x => x.CreationTime).Select(x => ToDto(x, key)).ToList(),
            SubmittedAt = complaint.SubmittedAt,
            ClosedAt = complaint.ClosedAt,
            ClosureDecision = complaint.EncryptedClosureDecision == null ? null : key.OpenText(ComplaintFields.Decision, complaint.EncryptedClosureDecision),
            ConcurrencyStamp = complaint.ConcurrencyStamp
        };
    }

    /// <summary>Someone who handles nothing gets an empty list, so the answer reveals nothing.</summary>
    public virtual async Task<ComplaintHandlerSummaryPagedResultDto> GetAssignedListAsync(ComplaintAssignedListInput input)
    {
        var context = await _policy.GetContextAsync(CurrentUser.GetId());
        var query = ApplyFilters(context.ApplyHandlerScope(await _complaints.GetQueryableAsync()), input);
        var now = Clock.Now;

        if (input.IsLate != null)
        {
            query = input.IsLate.Value
                ? query.Where(x => x.Status != ComplaintStatus.Closed && x.DueAt != null && x.DueAt < now)
                : query.Where(x => x.Status == ComplaintStatus.Closed || x.DueAt == null || x.DueAt >= now);
        }

        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(Complaint.SubmittedAt), SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new ComplaintHandlerSummaryPagedResultDto
        {
            TotalCount = total,
            Items = items.Select(x => new ComplaintHandlerSummaryDto
            {
                Id = x.Id,
                ReferenceNumber = x.ReferenceNumber,
                Level = x.Level,
                Status = x.Status,
                SubmittedAt = x.SubmittedAt,
                DueAt = x.DueAt,
                IsLate = IsLate(x, now),
                ClosedAt = x.ClosedAt
            }).ToList()
        };
    }

    public virtual async Task<ComplaintHandlerDetailDto> GetDetailAsync(Guid id)
    {
        var (complaint, _) = await LoadForHandlerAsync(id);
        var me = CurrentUser.GetId();

        // The read is on record before anything is decrypted. If recording fails, nothing comes back.
        await WriteLogAsync(complaint, me, ComplaintAccessAction.ViewDetail);

        var key = await _keys.OpenAsync(complaint);
        var handlers = complaint.IsClosed ? new List<Guid>() : await _policy.GetHandlerUserIdsAsync(complaint);
        var handlerId = handlers.Contains(me) && !complaint.IsClosed && complaint.Status == ComplaintStatus.PendingDepartmentManager ? me : handlers.FirstOrDefault();

        var employeeIds = new List<Guid> { complaint.ComplainantEmployeeId };
        if (complaint.RespondentEmployeeId != null)
        {
            employeeIds.Add(complaint.RespondentEmployeeId.Value);
        }

        var employees = await _references.GetRefsAsync(employeeIds);
        var actions = complaint.Actions.OrderBy(x => x.CreationTime).ThenBy(x => x.Id).ToList();
        var userIds = actions.Select(x => x.ActorUserId).ToList();
        if (handlerId != Guid.Empty)
        {
            userIds.Add(handlerId);
        }

        var users = await _references.GetUserRefsAsync(userIds);

        return new ComplaintHandlerDetailDto
        {
            Id = complaint.Id,
            ReferenceNumber = complaint.ReferenceNumber,
            Complainant = employees[complaint.ComplainantEmployeeId],
            TargetKind = complaint.TargetKind,
            Respondent = complaint.RespondentEmployeeId == null ? null : employees[complaint.RespondentEmployeeId.Value],
            RespondentCategory = complaint.RespondentCategory,
            Level = complaint.Level,
            MinimumLevel = complaint.MinimumLevel,
            Status = complaint.Status,
            Handler = handlerId == Guid.Empty ? null : users[handlerId],
            Subject = key.OpenText(ComplaintFields.Subject, complaint.EncryptedSubject),
            Content = key.OpenText(ComplaintFields.Content, complaint.EncryptedContent),
            Attachments = complaint.Attachments.OrderBy(x => x.CreationTime).Select(x => ToDto(x, key)).ToList(),
            Actions = actions.Select(x => ToDto(x, key, users)).ToList(),
            SubmittedAt = complaint.SubmittedAt,
            DueAt = complaint.DueAt,
            IsLate = IsLate(complaint, Clock.Now),
            ClosedAt = complaint.ClosedAt,
            ClosureDecision = complaint.EncryptedClosureDecision == null ? null : key.OpenText(ComplaintFields.Decision, complaint.EncryptedClosureDecision),
            ConcurrencyStamp = complaint.ConcurrencyStamp
        };
    }

    /// <summary>QUESTIONS.md item 37: the complainant and the current handler. There is no way down, only up.</summary>
    public virtual async Task<ComplaintTransitionResultDto> RaiseLevelAsync(Guid id, ComplaintRaiseLevelInputDto input)
    {
        var me = CurrentUser.GetId();
        var complaint = await FindAsync(id) ?? throw NotFound(id);
        var context = await _policy.GetContextAsync(me);
        var isComplainant = complaint.ComplainantUserId == me;

        if (!isComplainant && !context.CanHandle(complaint))
        {
            throw NotFound(id);
        }

        CheckOpen(complaint);

        var level = input.Level!.Value;
        if (level <= complaint.Level)
        {
            throw new BusinessException(HrErrorCodes.Complaints.LevelNotHigher);
        }

        var key = await _keys.OpenAsync(complaint);
        complaint.ConcurrencyStamp = input.ConcurrencyStamp;
        complaint.RaiseTo(level);
        AddActionTo(complaint, key, ComplaintActionType.LevelRaised, me, input.Reason);
        await _complaints.UpdateAsync(complaint, autoSave: true);

        await WriteLogAsync(complaint, me, ComplaintAccessAction.RaiseLevel);
        await _notifier.NotifyHandlersAsync(complaint, me);

        return ToTransition(complaint, isComplainant || context.CanHandle(complaint));
    }

    public virtual async Task<ComplaintActionDto> AddActionAsync(Guid id, ComplaintActionCreateDto input, Guid? idempotencyKey)
    {
        var (complaint, _) = await LoadForHandlerAsync(id, track: false);
        var me = CurrentUser.GetId();
        var key = await _keys.OpenAsync(complaint);

        var earlier = await _idempotency.FindResourceAsync(ActionOperation, idempotencyKey, new { id, input.Type, input.Note });
        if (earlier != null)
        {
            var existing = complaint.Actions.First(x => x.Id == earlier.Value);
            return ToDto(existing, key, await _references.GetUserRefsAsync(new[] { existing.ActorUserId }));
        }

        CheckOpen(complaint);

        // Inserted on its own: the screen holding the complaint's stamp must stay able to forward or close it, and this reply carries no new stamp.
        var actionId = GuidGenerator.Create();
        var action = new ComplaintAction(
            actionId,
            CurrentTenant.Id,
            complaint.Id,
            Enum.Parse<ComplaintActionType>(input.Type!.Value.ToString()),
            me,
            key.SealText(ComplaintFields.ActionNote(actionId), input.Note),
            Clock.Now);
        await _actions.InsertAsync(action, autoSave: true);

        await WriteLogAsync(complaint, me, ComplaintAccessAction.AddAction);
        await _idempotency.SaveAsync(ActionOperation, idempotencyKey, new { id, input.Type, input.Note }, action.Id);
        return ToDto(action, key, await _references.GetUserRefsAsync(new[] { me }));
    }

    public virtual async Task<ComplaintTransitionResultDto> ForwardAsync(Guid id, ComplaintForwardInputDto input)
    {
        var (complaint, _) = await LoadForHandlerAsync(id);
        var me = CurrentUser.GetId();

        CheckOpen(complaint);

        if (complaint.Status != ComplaintStatus.PendingDepartmentManager)
        {
            throw new BusinessException(HrErrorCodes.Complaints.InvalidTransition);
        }

        var key = await _keys.OpenAsync(complaint);
        complaint.ConcurrencyStamp = input.ConcurrencyStamp;
        complaint.MoveToHr();
        AddActionTo(complaint, key, ComplaintActionType.Forwarded, me, input.Note);
        await _complaints.UpdateAsync(complaint, autoSave: true);

        await WriteLogAsync(complaint, me, ComplaintAccessAction.Forward);
        await _notifier.NotifyHandlersAsync(complaint, me);

        return ToTransition(complaint, false);
    }

    public virtual async Task<ComplaintTransitionResultDto> CloseAsync(Guid id, ComplaintCloseInputDto input)
    {
        var (complaint, context) = await LoadForHandlerAsync(id);
        var me = CurrentUser.GetId();

        // A department manager handles but never closes: to them this complaint offers no such operation.
        if (complaint.Status == ComplaintStatus.PendingDepartmentManager)
        {
            throw NotFound(id);
        }

        CheckOpen(complaint);

        var key = await _keys.OpenAsync(complaint);
        var now = Clock.Now;
        complaint.ConcurrencyStamp = input.ConcurrencyStamp;
        complaint.Close(key.SealText(ComplaintFields.Decision, input.Decision), now);
        AddActionTo(complaint, key, ComplaintActionType.Closed, me, null);
        await _complaints.UpdateAsync(complaint, autoSave: true);

        await WriteLogAsync(complaint, me, ComplaintAccessAction.Close);
        await _notifier.NotifyComplainantAsync(complaint);

        return ToTransition(complaint, context.CanHandle(complaint));
    }

    public virtual async Task<ComplaintAccessLogEntryPagedResultDto> GetAccessLogAsync(Guid id, PagedAndSortedResultRequestDto input)
    {
        var (complaint, _) = await LoadForHandlerAsync(id);
        await WriteLogAsync(complaint, CurrentUser.GetId(), ComplaintAccessAction.ViewAccessLog);

        var query = (await _accessLogs.GetQueryableAsync()).Where(x => x.ComplaintId == id);
        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, nameof(ComplaintAccessLog.At) + " desc", LogSortFields).Skip(input.SkipCount).Take(input.MaxResultCount));
        var users = await _references.GetUserRefsAsync(items.Select(x => x.UserId));

        return new ComplaintAccessLogEntryPagedResultDto
        {
            TotalCount = total,
            Items = items.Select(x => new ComplaintAccessLogEntryDto { Id = x.Id, User = users[x.UserId], Action = x.Action, At = x.At }).ToList()
        };
    }

    public virtual async Task<AttachmentDto> UploadAttachmentAsync(Guid id, ComplaintAttachmentUploadInput input, Guid? idempotencyKey)
    {
        var me = CurrentUser.GetId();
        var complaint = await FindUntrackedAsync(id) ?? throw NotFound(id);

        if (complaint.ComplainantUserId != me && !(await _policy.GetContextAsync(me)).CanHandle(complaint))
        {
            throw NotFound(id);
        }

        var key = await _keys.OpenAsync(complaint);
        var fingerprint = new { id, input.FileName, Hash = Convert.ToHexString(SHA256.HashData(input.Content)) };

        var earlier = await _idempotency.FindResourceAsync(UploadOperation, idempotencyKey, fingerprint);
        if (earlier != null)
        {
            return ToDto(complaint.Attachments.First(x => x.Id == earlier.Value), key);
        }

        CheckOpen(complaint);
        await EnsureFileAllowedAsync(input);

        var attachmentId = GuidGenerator.Create();
        var blobName = "complaint-" + attachmentId.ToString("N");
        await _files.SaveAsync(blobName, key.Seal(ComplaintFields.AttachmentFile(attachmentId), input.Content));

        var attachment = new ComplaintAttachment(
            attachmentId,
            CurrentTenant.Id,
            complaint.Id,
            me,
            key.SealText(ComplaintFields.AttachmentName(attachmentId), Path.GetFileName(input.FileName)),
            input.ContentType,
            input.Content.LongLength,
            blobName,
            Clock.Now);

        await _attachments.InsertAsync(attachment, autoSave: true);

        await WriteLogAsync(complaint, me, ComplaintAccessAction.UploadAttachment);
        await _idempotency.SaveAsync(UploadOperation, idempotencyKey, fingerprint, attachmentId);
        return ToDto(attachment, key);
    }

    public virtual async Task<ComplaintAttachmentFile> DownloadAttachmentAsync(Guid id, Guid attachmentId)
    {
        var me = CurrentUser.GetId();
        var complaint = await FindAsync(id) ?? throw NotFound(id);
        var isComplainant = complaint.ComplainantUserId == me;

        if (!isComplainant && !(await _policy.GetContextAsync(me)).CanHandle(complaint))
        {
            throw NotFound(id);
        }

        // The attachment is looked up inside the complaint just checked, and a complainant reaches only their own.
        var attachment = complaint.Attachments.FirstOrDefault(x => x.Id == attachmentId && (!isComplainant || x.UploadedByUserId == me));
        if (attachment == null)
        {
            throw NotFound(id);
        }

        await WriteLogAsync(complaint, me, ComplaintAccessAction.DownloadAttachment);

        var key = await _keys.OpenAsync(complaint);
        var sealedFile = await _files.GetAllBytesAsync(attachment.BlobName);

        return new ComplaintAttachmentFile
        {
            Content = key.Open(ComplaintFields.AttachmentFile(attachment.Id), sealedFile),
            FileName = key.OpenText(ComplaintFields.AttachmentName(attachment.Id), attachment.EncryptedFileName),
            ContentType = attachment.ContentType
        };
    }

    private sealed record ComplaintPlan(
        Employee Complainant,
        Employee? Respondent,
        ComplaintRespondentProfile Profile,
        ComplaintLevel MinimumLevel,
        Guid? ManagerUserId);

    /// <summary>Everything the server decides before a complaint exists. The preview and the submission share it.</summary>
    private async Task<ComplaintPlan> PlanAsync(ComplaintRoutingPreviewInputDto input)
    {
        var me = CurrentUser.GetId();
        var complainant = await _employees.FirstOrDefaultAsync(x => x.UserId == me) ?? throw new AbpAuthorizationException();
        var myRoles = await _users.GetRoleNamesAsync(me);

        // QUESTIONS.md item 34: the executive does not submit inside the system, whatever other role they hold.
        if (myRoles.Contains(HrRoles.ExecutiveDirector))
        {
            throw new AbpAuthorizationException();
        }

        Employee? respondent = null;
        var profile = ComplaintRoutingService.GeneralComplaint;

        if (input.TargetKind == ComplaintTargetKind.Employee)
        {
            if (input.RespondentId == null)
            {
                throw new BusinessException(HrErrorCodes.Complaints.RespondentRequired);
            }

            if (input.RespondentId == complainant.Id)
            {
                throw new BusinessException(HrErrorCodes.Complaints.SelfComplaintNotAllowed);
            }

            respondent = await _employees.FindAsync(input.RespondentId.Value);
            profile = await ClassifyAsync(respondent) ?? throw new BusinessException(HrErrorCodes.Complaints.RespondentNotAllowed);
        }

        var minimum = ComplaintRoutingService.MinimumLevelFor(profile, myRoles);
        var manager = profile.SkipsDepartmentManager || myRoles.Contains(HrRoles.DepartmentManager)
            ? null
            : await FindEligibleManagerAsync(complainant, respondent);

        return new ComplaintPlan(complainant, respondent, profile, minimum, manager);
    }

    private async Task<ComplaintRespondentProfile?> ClassifyAsync(Employee? respondent)
    {
        if (respondent?.UserId == null || respondent.Status != EmployeeStatus.Active)
        {
            return null;
        }

        var user = await _userManager.FindByIdAsync(respondent.UserId.Value.ToString());
        return user is { IsActive: true } ? ComplaintRoutingService.Classify(await _users.GetRoleNamesAsync(user.Id)) : null;
    }

    /// <summary>With no manager able to take it, a normal complaint goes straight to HR rather than waiting for no one.</summary>
    private async Task<Guid?> FindEligibleManagerAsync(Employee complainant, Employee? respondent)
    {
        if (complainant.ManagerEmployeeId == null || complainant.ManagerEmployeeId == respondent?.Id)
        {
            return null;
        }

        var manager = await _employees.FindAsync(complainant.ManagerEmployeeId.Value);
        if (manager?.UserId == null || manager.Status != EmployeeStatus.Active)
        {
            return null;
        }

        var user = await _userManager.FindByIdAsync(manager.UserId.Value.ToString());
        if (user is not { IsActive: true })
        {
            return null;
        }

        var roles = await _users.GetRoleNamesAsync(user.Id);
        return roles.Contains(HrRoles.DepartmentManager) && !roles.Contains(HrRoles.SystemAdmin) ? user.Id : null;
    }

    private async Task<DateTime?> GetDueAtAsync()
    {
        var days = (await _settings.PeekValueAsync<ComplaintSettings>()).DelayDeadlineWorkingDays;
        if (days is null or <= 0)
        {
            return null;
        }

        var dueDate = await _calendar.AddWorkingDaysAsync(await _companyClock.GetTodayAsync(), days.Value);
        return dueDate == null ? null : await _companyClock.StartOfDayUtcAsync(dueDate.Value.AddDays(1));
    }

    private async Task EnsureFileAllowedAsync(ComplaintAttachmentUploadInput input)
    {
        var maxSize = long.Parse((await _settingProvider.GetOrNullAsync(HR_Mangment2Settings.DocumentMaxFileSizeBytes))!);
        if (input.Content.LongLength > maxSize)
        {
            throw new BusinessException(HrErrorCodes.Complaints.AttachmentTooLarge);
        }

        var allowed = ((await _settingProvider.GetOrNullAsync(HR_Mangment2Settings.DocumentAllowedContentTypes)) ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var signatureMatches = !FileSignatures.TryGetValue(input.ContentType, out var signature) || input.Content.AsSpan().StartsWith(signature);
        if (!allowed.Contains(input.ContentType, StringComparer.OrdinalIgnoreCase) || !signatureMatches)
        {
            throw new BusinessException(HrErrorCodes.Document.FileTypeNotAllowed);
        }

        VirusScanResult result;
        try
        {
            result = await _scanner.ScanAsync(input.Content);
        }
        catch (Exception ex)
        {
            // The exception type only: it could carry details of the file.
            Logger.LogWarning("A virus scan failed for a complaint attachment: {ExceptionType}.", ex.GetType().Name);
            throw new BusinessException(HrErrorCodes.Complaints.AttachmentRejected);
        }

        if (result != VirusScanResult.Clean)
        {
            throw new BusinessException(HrErrorCodes.Complaints.AttachmentRejected);
        }
    }

    private static EntityNotFoundException NotFound(Guid id) => new(typeof(Complaint), id);

    private static void CheckOpen(Complaint complaint)
    {
        if (complaint.IsClosed)
        {
            throw new BusinessException(HrErrorCodes.Complaints.ComplaintClosed);
        }
    }

    private async Task<Complaint?> FindAsync(Guid id)
    {
        var query = (await _complaints.WithDetailsAsync(x => x.Actions, x => x.Attachments)).Where(x => x.Id == id);
        return await AsyncExecuter.FirstOrDefaultAsync(query);
    }

    /// <summary>
    /// For operations that only add a child row. With the complaint tracked, ABP would change its stamp for the new
    /// child, and the handler's open screen could no longer forward or close it.
    /// </summary>
    private async Task<Complaint?> FindUntrackedAsync(Guid id)
    {
        using (_complaints.DisableTracking())
        {
            return await FindAsync(id);
        }
    }

    /// <summary>The complainant has their own path: on this one they get a 404 even if they also hold a handler role.</summary>
    private async Task<(Complaint Complaint, ComplaintAccessContext Context)> LoadForHandlerAsync(Guid id, bool track = true)
    {
        var complaint = (track ? await FindAsync(id) : await FindUntrackedAsync(id)) ?? throw NotFound(id);
        var context = await _policy.GetContextAsync(CurrentUser.GetId());
        return context.CanHandle(complaint) ? (complaint, context) : throw NotFound(id);
    }

    private Task WriteLogAsync(Complaint complaint, Guid userId, ComplaintAccessAction action) =>
        _accessLogs.InsertAsync(new ComplaintAccessLog(GuidGenerator.Create(), CurrentTenant.Id, complaint.Id, userId, action, Clock.Now), autoSave: true);

    private ComplaintAction AddActionTo(Complaint complaint, ComplaintContentKey key, ComplaintActionType type, Guid actorUserId, string? note)
    {
        var actionId = GuidGenerator.Create();
        var sealedNote = string.IsNullOrEmpty(note) ? null : key.SealText(ComplaintFields.ActionNote(actionId), note);
        complaint.AddAction(actionId, type, actorUserId, sealedNote, Clock.Now);
        return complaint.Actions.First(x => x.Id == actionId);
    }

    private static string NewReferenceNumber()
    {
        var value = new char[ReferenceLength];
        for (var i = 0; i < ReferenceLength; i++)
        {
            value[i] = ReferenceAlphabet[RandomNumberGenerator.GetInt32(ReferenceAlphabet.Length)];
        }

        return "C-" + new string(value);
    }

    private static bool IsLate(Complaint complaint, DateTime now) => !complaint.IsClosed && complaint.DueAt != null && complaint.DueAt < now;

    private static IQueryable<Complaint> ApplyFilters(IQueryable<Complaint> query, ComplaintMyListInput input)
    {
        if (input.Status != null)
        {
            query = query.Where(x => x.Status == input.Status);
        }

        if (input.Level != null)
        {
            query = query.Where(x => x.Level == input.Level);
        }

        return query;
    }

    private static ComplaintSubmitResultDto ToSubmitResult(Complaint complaint) => new()
    {
        Id = complaint.Id,
        ReferenceNumber = complaint.ReferenceNumber,
        Level = complaint.Level,
        LevelRaisedBySystem = complaint.LevelRaisedBySystem,
        Status = complaint.Status,
        SubmittedAt = complaint.SubmittedAt
    };

    private static ComplaintTransitionResultDto ToTransition(Complaint complaint, bool callerRetainsAccess) => new()
    {
        Id = complaint.Id,
        Level = complaint.Level,
        Status = complaint.Status,
        ClosedAt = complaint.ClosedAt,
        CallerRetainsAccess = callerRetainsAccess,
        ConcurrencyStamp = complaint.ConcurrencyStamp
    };

    private static AttachmentDto ToDto(ComplaintAttachment attachment, ComplaintContentKey key) => new()
    {
        Id = attachment.Id,
        FileName = key.OpenText(ComplaintFields.AttachmentName(attachment.Id), attachment.EncryptedFileName),
        ContentType = attachment.ContentType,
        SizeBytes = attachment.SizeBytes,
        CreationTime = attachment.CreationTime
    };

    private static ComplaintActionDto ToDto(ComplaintAction action, ComplaintContentKey key, IReadOnlyDictionary<Guid, UserRefDto> users) => new()
    {
        Id = action.Id,
        Type = action.Type,
        Actor = users[action.ActorUserId],
        Note = action.EncryptedNote == null ? null : key.OpenText(ComplaintFields.ActionNote(action.Id), action.EncryptedNote),
        CreationTime = action.CreationTime
    };
}

public abstract class ComplaintTargetValidator<T> : AbstractValidator<T>
    where T : ComplaintRoutingPreviewInputDto
{
    protected ComplaintTargetValidator()
    {
        RuleFor(x => x.TargetKind).NotNull();
        When(x => x.TargetKind == ComplaintTargetKind.General, () => RuleFor(x => x.RespondentId).Null());
    }
}

public class ComplaintRoutingPreviewInputDtoValidator : ComplaintTargetValidator<ComplaintRoutingPreviewInputDto>
{
}

public class ComplaintCreateDtoValidator : ComplaintTargetValidator<ComplaintCreateDto>
{
    public ComplaintCreateDtoValidator()
    {
        RuleFor(x => x.Level).NotNull();
        RuleFor(x => x.Subject).NotNull().Length(1, Complaint.MaxSubjectLength);
        RuleFor(x => x.Content).NotNull().Length(1, Complaint.MaxContentLength);
    }
}

public class ComplaintRaiseLevelInputDtoValidator : AbstractValidator<ComplaintRaiseLevelInputDto>
{
    public ComplaintRaiseLevelInputDtoValidator()
    {
        RuleFor(x => x.Level).NotNull();
        RuleFor(x => x.Reason).MaximumLength(Complaint.MaxTransitionNoteLength);
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public class ComplaintForwardInputDtoValidator : AbstractValidator<ComplaintForwardInputDto>
{
    public ComplaintForwardInputDtoValidator()
    {
        RuleFor(x => x.Note).MaximumLength(Complaint.MaxTransitionNoteLength);
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public class ComplaintCloseInputDtoValidator : AbstractValidator<ComplaintCloseInputDto>
{
    public ComplaintCloseInputDtoValidator()
    {
        RuleFor(x => x.Decision).NotNull().Length(1, Complaint.MaxDecisionLength);
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}

public class ComplaintActionCreateDtoValidator : AbstractValidator<ComplaintActionCreateDto>
{
    public ComplaintActionCreateDtoValidator()
    {
        RuleFor(x => x.Type).NotNull();
        RuleFor(x => x.Note).NotNull().Length(1, Complaint.MaxNoteLength);
    }
}
