using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using HR_Mangment2.Employees;
using HR_Mangment2.Errors;
using HR_Mangment2.Idempotency;
using HR_Mangment2.Identity;
using HR_Mangment2.Notifications;
using HR_Mangment2.Permissions;
using HR_Mangment2.Requests;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace HR_Mangment2.Notices;

/// <summary>
/// Notices from the HR manager or the executive director to employees chosen by name (decision 2026-11-10).
/// Shaped like the circulars: fixed recipients, a notification to each, acknowledgement by the recipient only.
/// </summary>
[Authorize]
public class NoticeAppService : HR_Mangment2AppService, INoticeAppService
{
    private const string CreateOperation = "Notice_Create";

    private readonly IRepository<Notice, Guid> _notices;
    private readonly IRepository<NoticeRecipient, Guid> _recipients;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IIdentityUserRepository _users;
    private readonly EmployeeReferenceReader _references;
    private readonly HrNotificationPublisher _notifications;
    private readonly AdministrativeTaskLogger _taskLog;
    private readonly IdempotencyStore _idempotency;

    public NoticeAppService(
        IRepository<Notice, Guid> notices,
        IRepository<NoticeRecipient, Guid> recipients,
        IRepository<Employee, Guid> employees,
        IIdentityUserRepository users,
        EmployeeReferenceReader references,
        HrNotificationPublisher notifications,
        AdministrativeTaskLogger taskLog,
        IdempotencyStore idempotency)
    {
        _notices = notices;
        _recipients = recipients;
        _employees = employees;
        _users = users;
        _references = references;
        _notifications = notifications;
        _taskLog = taskLog;
        _idempotency = idempotency;
    }

    [Authorize(HrPermissions.NoticesPermissions.Read)]
    public virtual async Task<NoticeReceivedPagedResultDto> GetReceivedListAsync(NoticeReceivedListInput input)
    {
        var me = CurrentUser.GetId();
        var received = (await _recipients.GetQueryableAsync())
            .Where(x => x.UserId == me)
            .WhereIf(input.IsAcknowledged == true, x => x.AcknowledgedAt != null)
            .WhereIf(input.IsAcknowledged == false, x => x.AcknowledgedAt == null);

        var query =
            from recipient in received
            join notice in await _notices.GetQueryableAsync() on recipient.NoticeId equals notice.Id
            orderby notice.SentAt descending, notice.Id
            select new { Notice = notice, recipient.AcknowledgedAt };

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));
        var users = await _references.GetUserRefsAsync(rows.Select(x => x.Notice.SenderUserId));

        return new NoticeReceivedPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => ToReceivedDto(x.Notice, x.AcknowledgedAt, users[x.Notice.SenderUserId])).ToList()
        };
    }

    [Authorize(HrPermissions.NoticesPermissions.Send)]
    public virtual async Task<NoticeSentDto> CreateAsync(NoticeCreateDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return ToSentDto(await LoadWithRecipientsAsync(earlier.Value));
        }

        var me = CurrentUser.GetId();
        var recipients = await ResolveRecipientsAsync(me, input.RecipientEmployeeIds);

        var notice = new Notice(GuidGenerator.Create(), CurrentTenant.Id, input.Title, input.Content, me, Clock.Now);
        foreach (var recipient in recipients)
        {
            notice.Recipients.Add(new NoticeRecipient(GuidGenerator.Create(), CurrentTenant.Id, notice.Id, recipient.Id, recipient.UserId!.Value));
        }

        await _notices.InsertAsync(notice, autoSave: true);
        await _taskLog.LogAsync(me, AdministrativeTaskType.NoticeSent, notice.Id);

        foreach (var recipient in recipients)
        {
            await _notifications.PublishAsync(recipient.UserId!.Value, NotificationTemplateCode.NoticeReceived, NotificationResourceType.Notice, notice.Id);
        }

        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, input, notice.Id);
        return ToSentDto(notice);
    }

    [Authorize(HrPermissions.NoticesPermissions.Send)]
    public virtual async Task<NoticeSentPagedResultDto> GetSentListAsync(PagedAndSortedResultRequestDto input)
    {
        var me = CurrentUser.GetId();
        var query = (await _notices.WithDetailsAsync(x => x.Recipients)).Where(x => x.SenderUserId == me);
        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.SentAt).ThenBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new NoticeSentPagedResultDto { TotalCount = total, Items = rows.Select(ToSentDto).ToList() };
    }

    [Authorize(HrPermissions.NoticesPermissions.Read)]
    public virtual async Task<NoticeReceivedDto> GetAsync(Guid id)
    {
        var recipient = await LoadOwnReceiptAsync(id);
        return await ToReceivedDtoAsync(await _notices.GetAsync(id, includeDetails: false), recipient.AcknowledgedAt);
    }

    [Authorize(HrPermissions.NoticesPermissions.Send)]
    public virtual async Task<NoticeReceiptPagedResultDto> GetReceiptsAsync(Guid id, NoticeReceiptListInput input)
    {
        var notice = await _notices.FindAsync(id, includeDetails: false);
        if (notice == null || notice.SenderUserId != CurrentUser.GetId())
        {
            throw new EntityNotFoundException(typeof(Notice), id);
        }

        var query = (await _recipients.GetQueryableAsync())
            .Where(x => x.NoticeId == id)
            .WhereIf(input.IsAcknowledged == true, x => x.AcknowledgedAt != null)
            .WhereIf(input.IsAcknowledged == false, x => x.AcknowledgedAt == null);

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.AcknowledgedAt == null).ThenBy(x => x.AcknowledgedAt).ThenBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount));
        var employees = await _references.GetRefsAsync(rows.Select(x => x.EmployeeId));

        return new NoticeReceiptPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => new NoticeReceiptDto { Recipient = employees[x.EmployeeId], AcknowledgedAt = x.AcknowledgedAt }).ToList()
        };
    }

    [Authorize(HrPermissions.NoticesPermissions.Read)]
    public virtual async Task<NoticeReceivedDto> AcknowledgeAsync(Guid id)
    {
        var recipient = await LoadOwnReceiptAsync(id);
        if (recipient.AcknowledgedAt == null)
        {
            recipient.Acknowledge(Clock.Now);
            await _recipients.UpdateAsync(recipient, autoSave: true);
        }

        return await ToReceivedDtoAsync(await _notices.GetAsync(id, includeDetails: false), recipient.AcknowledgedAt);
    }

    /// <summary>
    /// All or nothing: every chosen employee must be active, have an account, not be the sender, and not be a system
    /// admin with no business role (not a company employee). One bad id refuses the whole request.
    /// </summary>
    private async Task<List<Employee>> ResolveRecipientsAsync(Guid senderUserId, IReadOnlyCollection<Guid> employeeIds)
    {
        var ids = employeeIds.Distinct().ToList();
        var employees = await _employees.GetListAsync(x => ids.Contains(x.Id));
        if (employees.Count != ids.Count)
        {
            throw new BusinessException(HrErrorCodes.Notices.RecipientNotAllowed);
        }

        foreach (var employee in employees)
        {
            if (employee.UserId == null || employee.UserId == senderUserId || employee.Status != EmployeeStatus.Active)
            {
                throw new BusinessException(HrErrorCodes.Notices.RecipientNotAllowed);
            }

            var roles = await _users.GetRoleNamesAsync(employee.UserId.Value);
            if (AccountProvisioner.ReceivesNoCirculars(roles))
            {
                throw new BusinessException(HrErrorCodes.Notices.RecipientNotAllowed);
            }
        }

        return employees;
    }

    private async Task<NoticeRecipient> LoadOwnReceiptAsync(Guid noticeId)
    {
        var me = CurrentUser.GetId();
        return await _recipients.FirstOrDefaultAsync(x => x.NoticeId == noticeId && x.UserId == me)
               ?? throw new EntityNotFoundException(typeof(Notice), noticeId);
    }

    private async Task<Notice> LoadWithRecipientsAsync(Guid id) =>
        await AsyncExecuter.FirstAsync((await _notices.WithDetailsAsync(x => x.Recipients)).Where(x => x.Id == id));

    private static NoticeSentDto ToSentDto(Notice notice) => new()
    {
        Id = notice.Id,
        Title = notice.Title,
        Content = notice.Content,
        SentAt = notice.SentAt,
        RecipientsCount = notice.Recipients.Count,
        AcknowledgedCount = notice.Recipients.Count(x => x.AcknowledgedAt != null)
    };

    private async Task<NoticeReceivedDto> ToReceivedDtoAsync(Notice notice, DateTime? acknowledgedAt)
    {
        var users = await _references.GetUserRefsAsync(new[] { notice.SenderUserId });
        return ToReceivedDto(notice, acknowledgedAt, users[notice.SenderUserId]);
    }

    private static NoticeReceivedDto ToReceivedDto(Notice notice, DateTime? acknowledgedAt, UserRefDto sender) => new()
    {
        Id = notice.Id,
        Title = notice.Title,
        Content = notice.Content,
        Sender = sender,
        SentAt = notice.SentAt,
        AcknowledgedAt = acknowledgedAt
    };
}

public class NoticeCreateDtoValidator : AbstractValidator<NoticeCreateDto>
{
    public NoticeCreateDtoValidator()
    {
        RuleFor(x => x.RecipientEmployeeIds).NotNull();
        RuleFor(x => x.RecipientEmployeeIds)
            .Must(x => x.Count is >= 1 and <= Notice.MaxRecipients)
            .WithMessage($"Choose between 1 and {Notice.MaxRecipients} recipients.")
            .Must(x => x.Distinct().Count() == x.Count)
            .WithMessage("Each recipient may appear only once.")
            .When(x => x.RecipientEmployeeIds != null);
        RuleForEach(x => x.RecipientEmployeeIds).NotEmpty().When(x => x.RecipientEmployeeIds != null);
        RuleFor(x => x.Title).NotNull().Length(1, Notice.MaxTitleLength);
        RuleFor(x => x.Content).NotNull().Length(1, Notice.MaxContentLength);
    }
}

/// <summary>Notices waiting for the user's acknowledgement. Not delegable, so the delegated count is always zero.</summary>
public class NoticeInboxSummaryContributor : IInboxSummaryContributor, ITransientDependency
{
    private readonly IRepository<NoticeRecipient, Guid> _recipients;

    public NoticeInboxSummaryContributor(IRepository<NoticeRecipient, Guid> recipients)
    {
        _recipients = recipients;
    }

    public virtual async Task<IReadOnlyCollection<InboxSummaryItemDto>> GetAsync(Guid userId) => new[]
    {
        new InboxSummaryItemDto
        {
            Category = InboxCategory.NoticeAcknowledgement,
            Count = await _recipients.CountAsync(x => x.UserId == userId && x.AcknowledgedAt == null),
            DelegatedCount = 0
        }
    };
}
