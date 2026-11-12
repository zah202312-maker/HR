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
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace HR_Mangment2.Evaluations;

[Authorize]
public class CircularAppService : HR_Mangment2AppService, ICircularAppService
{
    private const string CreateOperation = "Circular_Create";

    private readonly IRepository<Circular, Guid> _circulars;
    private readonly IRepository<CircularRecipient, Guid> _recipients;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IdentityUserManager _userManager;
    private readonly IIdentityUserRepository _users;
    private readonly EmployeeReferenceReader _references;
    private readonly HrNotificationPublisher _notifications;
    private readonly AdministrativeTaskLogger _taskLog;
    private readonly IdempotencyStore _idempotency;

    public CircularAppService(
        IRepository<Circular, Guid> circulars,
        IRepository<CircularRecipient, Guid> recipients,
        IRepository<Employee, Guid> employees,
        IdentityUserManager userManager,
        IIdentityUserRepository users,
        EmployeeReferenceReader references,
        HrNotificationPublisher notifications,
        AdministrativeTaskLogger taskLog,
        IdempotencyStore idempotency)
    {
        _circulars = circulars;
        _recipients = recipients;
        _employees = employees;
        _userManager = userManager;
        _users = users;
        _references = references;
        _notifications = notifications;
        _taskLog = taskLog;
        _idempotency = idempotency;
    }

    [Authorize(HrPermissions.CircularsPermissions.Read)]
    public virtual async Task<CircularReceivedPagedResultDto> GetReceivedListAsync(CircularReceivedListInput input)
    {
        var me = CurrentUser.GetId();
        var received = (await _recipients.GetQueryableAsync())
            .Where(x => x.UserId == me)
            .WhereIf(input.IsAcknowledged == true, x => x.AcknowledgedAt != null)
            .WhereIf(input.IsAcknowledged == false, x => x.AcknowledgedAt == null);

        var query =
            from recipient in received
            join circular in await _circulars.GetQueryableAsync() on recipient.CircularId equals circular.Id
            orderby circular.SentAt descending, circular.Id
            select new { Circular = circular, recipient.AcknowledgedAt };

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));
        var users = await _references.GetUserRefsAsync(rows.Select(x => x.Circular.SenderUserId));

        return new CircularReceivedPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => ToReceivedDto(x.Circular, x.AcknowledgedAt, users[x.Circular.SenderUserId])).ToList()
        };
    }

    [Authorize(HrPermissions.CircularsPermissions.Send)]
    public virtual async Task<CircularSentDto> CreateAsync(CircularCreateDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return await ToSentDtoAsync(await LoadWithRecipientsAsync(earlier.Value));
        }

        var me = CurrentUser.GetId();
        var scope = input.Scope!.Value;
        await CheckScopeAsync(me, scope);

        var recipients = await ResolveRecipientsAsync(me, scope);
        if (recipients.Count == 0)
        {
            throw new BusinessException(HrErrorCodes.Circulars.NoRecipients);
        }

        var circular = new Circular(GuidGenerator.Create(), CurrentTenant.Id, scope, input.Title, input.Content, me, Clock.Now);
        foreach (var recipient in recipients)
        {
            circular.Recipients.Add(new CircularRecipient(GuidGenerator.Create(), CurrentTenant.Id, circular.Id, recipient.Id, recipient.UserId!.Value));
        }

        await _circulars.InsertAsync(circular, autoSave: true);
        await _taskLog.LogAsync(me, AdministrativeTaskType.CircularSent, circular.Id);

        foreach (var recipient in recipients)
        {
            await _notifications.PublishAsync(recipient.UserId!.Value, NotificationTemplateCode.CircularReceived, NotificationResourceType.Circular, circular.Id);
        }

        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, input, circular.Id);
        return await ToSentDtoAsync(circular);
    }

    [Authorize(HrPermissions.CircularsPermissions.Send)]
    public virtual async Task<CircularSentPagedResultDto> GetSentListAsync(PagedAndSortedResultRequestDto input)
    {
        var me = CurrentUser.GetId();
        var query = (await _circulars.WithDetailsAsync(x => x.Recipients)).Where(x => x.SenderUserId == me);
        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.SentAt).ThenBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new CircularSentPagedResultDto { TotalCount = total, Items = rows.Select(ToSentDto).ToList() };
    }

    [Authorize(HrPermissions.CircularsPermissions.Read)]
    public virtual async Task<CircularReceivedDto> GetAsync(Guid id)
    {
        var recipient = await LoadOwnReceiptAsync(id);
        return await ToReceivedDtoAsync(await _circulars.GetAsync(id, includeDetails: false), recipient.AcknowledgedAt);
    }

    [Authorize(HrPermissions.CircularsPermissions.Send)]
    public virtual async Task<CircularReceiptPagedResultDto> GetReceiptsAsync(Guid id, CircularReceiptListInput input)
    {
        var circular = await _circulars.GetAsync(id, includeDetails: false);
        if (circular.SenderUserId != CurrentUser.GetId())
        {
            throw new EntityNotFoundException(typeof(Circular), id);
        }

        var query = (await _recipients.GetQueryableAsync())
            .Where(x => x.CircularId == id)
            .WhereIf(input.IsAcknowledged == true, x => x.AcknowledgedAt != null)
            .WhereIf(input.IsAcknowledged == false, x => x.AcknowledgedAt == null);

        var total = await AsyncExecuter.LongCountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.AcknowledgedAt == null).ThenBy(x => x.AcknowledgedAt).ThenBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount));
        var employees = await _references.GetRefsAsync(rows.Select(x => x.EmployeeId));

        return new CircularReceiptPagedResultDto
        {
            TotalCount = total,
            Items = rows.Select(x => new CircularReceiptDto { Recipient = employees[x.EmployeeId], AcknowledgedAt = x.AcknowledgedAt }).ToList()
        };
    }

    [Authorize(HrPermissions.CircularsPermissions.Read)]
    public virtual async Task<CircularReceivedDto> AcknowledgeAsync(Guid id)
    {
        var recipient = await LoadOwnReceiptAsync(id);
        if (recipient.AcknowledgedAt == null)
        {
            recipient.Acknowledge(Clock.Now);
            await _recipients.UpdateAsync(recipient, autoSave: true);
        }

        return await ToReceivedDtoAsync(await _circulars.GetAsync(id, includeDetails: false), recipient.AcknowledgedAt);
    }

    private async Task CheckScopeAsync(Guid userId, CircularScope scope)
    {
        var roles = await _users.GetRoleNamesAsync(userId);
        var allowed = scope == CircularScope.Team
            ? roles.Contains(HrRoles.DepartmentManager)
            : roles.Contains(HrRoles.HrManager) || roles.Contains(HrRoles.ExecutiveDirector);

        if (!allowed)
        {
            throw new BusinessException(HrErrorCodes.Circulars.ScopeNotAllowed);
        }
    }

    /// <summary>
    /// Fixed now: someone who joins later never sees this circular. The sender does not receive their own. A system
    /// admin with no business role is not a company employee and receives none; with a business role they do
    /// (decision 2026-11-10).
    /// </summary>
    private async Task<List<Employee>> ResolveRecipientsAsync(Guid senderUserId, CircularScope scope)
    {
        // System admins, accountants and store officers with no other business role receive none (decisions of
        // 2026-11-10 and 2026-11-11).
        var excluded = new List<Guid>();
        foreach (var roleName in new[] { HrRoles.SystemAdmin, HrRoles.Accountant, HrRoles.StoreOfficer })
        {
            foreach (var user in await _userManager.GetUsersInRoleAsync(roleName))
            {
                if (!excluded.Contains(user.Id) && AccountProvisioner.ReceivesNoCirculars((await _userManager.GetRolesAsync(user)).ToList()))
                {
                    excluded.Add(user.Id);
                }
            }
        }

        var query = (await _employees.GetQueryableAsync()).Where(x =>
            x.UserId != null && x.UserId != senderUserId && x.Status == EmployeeStatus.Active && !excluded.Contains(x.UserId!.Value));

        switch (scope)
        {
            case CircularScope.Team:
                var me = await _employees.FirstOrDefaultAsync(x => x.UserId == senderUserId);
                if (me == null)
                {
                    return new List<Employee>();
                }

                query = query.Where(x => x.ManagerEmployeeId == me.Id);
                break;

            case CircularScope.DepartmentManagers:
                var managerIds = (await _userManager.GetUsersInRoleAsync(HrRoles.DepartmentManager)).Where(x => x.IsActive).Select(x => x.Id).ToList();
                query = query.Where(x => managerIds.Contains(x.UserId!.Value));
                break;
        }

        return await AsyncExecuter.ToListAsync(query);
    }

    private async Task<CircularRecipient> LoadOwnReceiptAsync(Guid circularId)
    {
        var me = CurrentUser.GetId();
        return await _recipients.FirstOrDefaultAsync(x => x.CircularId == circularId && x.UserId == me)
               ?? throw new EntityNotFoundException(typeof(Circular), circularId);
    }

    private async Task<Circular> LoadWithRecipientsAsync(Guid id) =>
        await AsyncExecuter.FirstAsync((await _circulars.WithDetailsAsync(x => x.Recipients)).Where(x => x.Id == id));

    private Task<CircularSentDto> ToSentDtoAsync(Circular circular) => Task.FromResult(ToSentDto(circular));

    private static CircularSentDto ToSentDto(Circular circular) => new()
    {
        Id = circular.Id,
        Scope = circular.Scope,
        Title = circular.Title,
        Content = circular.Content,
        SentAt = circular.SentAt,
        RecipientsCount = circular.Recipients.Count,
        AcknowledgedCount = circular.Recipients.Count(x => x.AcknowledgedAt != null)
    };

    private async Task<CircularReceivedDto> ToReceivedDtoAsync(Circular circular, DateTime? acknowledgedAt)
    {
        var users = await _references.GetUserRefsAsync(new[] { circular.SenderUserId });
        return ToReceivedDto(circular, acknowledgedAt, users[circular.SenderUserId]);
    }

    /// <summary>QUESTIONS.md: acknowledgement is required on every circular, because the monthly report collects it. See D31.</summary>
    private static CircularReceivedDto ToReceivedDto(Circular circular, DateTime? acknowledgedAt, UserRefDto sender) => new()
    {
        Id = circular.Id,
        Scope = circular.Scope,
        Title = circular.Title,
        Content = circular.Content,
        Sender = sender,
        SentAt = circular.SentAt,
        AcknowledgementRequired = true,
        AcknowledgedAt = acknowledgedAt
    };
}

public class CircularCreateDtoValidator : AbstractValidator<CircularCreateDto>
{
    public CircularCreateDtoValidator()
    {
        RuleFor(x => x.Scope).NotNull();
        RuleFor(x => x.Title).NotNull().Length(1, Circular.MaxTitleLength);
        RuleFor(x => x.Content).NotNull().Length(1, Circular.MaxContentLength);
    }
}
