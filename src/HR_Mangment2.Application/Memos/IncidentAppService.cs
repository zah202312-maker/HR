using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace HR_Mangment2.Memos;

[Authorize]
public class IncidentAppService : HR_Mangment2AppService, IIncidentAppService
{
    private const string CreateOperation = "Incident_Create";

    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["creationTime"] = nameof(Incident.CreationTime),
        ["status"] = nameof(Incident.Status)
    };

    private readonly IRepository<Incident, Guid> _incidents;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly ManagerLinkService _managerLinks;
    private readonly IdentityUserManager _userManager;
    private readonly RequestDeciderReader _deciders;
    private readonly MemoIncidentAudience _audience;
    private readonly IncidentCreator _creator;
    private readonly HrNotificationPublisher _notifications;
    private readonly AdministrativeTaskLogger _taskLog;
    private readonly EmployeeReferenceReader _references;
    private readonly HR_Mangment2.Store.ItemReferenceReader _itemRefs;
    private readonly IdempotencyStore _idempotency;

    public IncidentAppService(
        IRepository<Incident, Guid> incidents,
        IRepository<Employee, Guid> employees,
        ManagerLinkService managerLinks,
        IdentityUserManager userManager,
        RequestDeciderReader deciders,
        MemoIncidentAudience audience,
        IncidentCreator creator,
        HrNotificationPublisher notifications,
        AdministrativeTaskLogger taskLog,
        EmployeeReferenceReader references,
        HR_Mangment2.Store.ItemReferenceReader itemRefs,
        IdempotencyStore idempotency)
    {
        _itemRefs = itemRefs;
        _incidents = incidents;
        _employees = employees;
        _managerLinks = managerLinks;
        _userManager = userManager;
        _deciders = deciders;
        _audience = audience;
        _creator = creator;
        _notifications = notifications;
        _taskLog = taskLog;
        _references = references;
        _idempotency = idempotency;
    }

    [Authorize(HrPermissions.IncidentPermissions.Create)]
    public virtual async Task<IncidentDto> CreateAsync(IncidentCreateInputDto input, Guid? idempotencyKey)
    {
        var earlier = await _idempotency.FindResourceAsync(CreateOperation, idempotencyKey, input);
        if (earlier != null)
        {
            return await GetAsync(earlier.Value);
        }

        var me = CurrentUser.GetId();
        var submitter = await _employees.FirstOrDefaultAsync(x => x.UserId == me) ?? throw new AbpAuthorizationException();
        var row = RequestRoutePlanner.ResolveRow(await _managerLinks.GetRolesAsync(submitter));

        // QUESTIONS.md item 27: only the employee row has a manager step on an incident.
        var managerUserId = row == RequestOwnerRow.Employee ? await GetLinkedManagerUserIdAsync(submitter) : (Guid?)null;
        var incident = await _creator.CreateAsync(submitter, me, input.ItemId, input.Description, managerUserId, null, me);

        await _idempotency.SaveAsync(CreateOperation, idempotencyKey, input, incident.Id);
        return await ToDtoAsync(incident);
    }

    [Authorize(HrPermissions.IncidentPermissions.ViewOwn)]
    public virtual async Task<IncidentListItemPagedResultDto> GetMyAsync(IncidentMyListInput input)
    {
        var me = CurrentUser.GetId();
        var query = (await _incidents.GetQueryableAsync()).Where(x => x.SubmitterUserId == me);

        if (input.Status != null)
        {
            query = query.Where(x => x.Status == input.Status);
        }

        return await ToPageAsync(query, input, nameof(Incident.CreationTime) + " desc");
    }

    public virtual async Task<IncidentListItemPagedResultDto> GetInboxAsync(IncidentInboxInput input)
    {
        var decider = await _deciders.GetForMemosAndIncidentsAsync(CurrentUser.GetId());
        await CheckGrantedOrDelegatedAsync(HrPermissions.IncidentPermissions.Process, decider);

        var query = await _incidents.GetQueryableAsync();

        if (!HasHrAuthority(decider))
        {
            var team = await GetTeamEmployeeIdsAsync(decider);
            var me = decider.UserId;
            query = query.Where(x => team.Contains(x.EmployeeId) && x.SubmitterUserId != me);
        }

        query = input.Status != null
            ? query.Where(x => x.Status == input.Status)
            : query.Where(x => x.Status != IncidentStatus.Resolved);

        if (input.EmployeeId != null)
        {
            query = query.Where(x => x.EmployeeId == input.EmployeeId);
        }

        return await ToPageAsync(query, input, nameof(Incident.CreationTime));
    }

    [Authorize(HrPermissions.IncidentPermissions.View)]
    public virtual async Task<IncidentDto> GetAsync(Guid id)
    {
        var incident = await LoadAsync(id);
        await CheckCanViewAsync(incident, await _deciders.GetForMemosAndIncidentsAsync(CurrentUser.GetId()));
        return await ToDtoAsync(incident);
    }

    public virtual async Task<IncidentDto> UpdateStatusAsync(Guid id, IncidentStatusUpdateInputDto input)
    {
        var decider = await _deciders.GetForMemosAndIncidentsAsync(CurrentUser.GetId());
        if (!decider.Delegators.Values.Any(x => x.Contains(HrRoles.HrManager)))
        {
            await AuthorizationService.CheckAsync(HrPermissions.IncidentPermissions.UpdateStatus);
        }

        var incident = await LoadAsync(id);
        var target = input.Status!.Value;

        if (!incident.TryMoveTo(target))
        {
            throw new BusinessException(HrErrorCodes.Incident.InvalidTransition);
        }

        incident.ConcurrencyStamp = input.ConcurrencyStamp;
        incident.AddTimeline(
            GuidGenerator.Create(),
            Clock.Now,
            target == IncidentStatusTarget.InProgress ? IncidentTimelineActions.InProgress : IncidentTimelineActions.Resolved,
            decider.UserId,
            input.Note);
        await _incidents.UpdateAsync(incident, autoSave: true);

        await NotifySubmitterAsync(incident, decider.UserId);
        return await ToDtoAsync(incident);
    }

    public virtual async Task<IncidentDto> ForwardAsync(Guid id, NoteInputDto input)
    {
        var decider = await _deciders.GetForMemosAndIncidentsAsync(CurrentUser.GetId());
        await CheckGrantedOrDelegatedAsync(HrPermissions.IncidentPermissions.Forward, decider);

        var incident = await LoadAsync(id);
        await CheckCanViewAsync(incident, decider);

        if (incident.Status != IncidentStatus.PendingManager)
        {
            throw new BusinessException(HrErrorCodes.Incident.InvalidState);
        }

        // The operation is limited to the linked manager's team. A delegate never handles their own incident.
        var holdsStep = incident.CurrentManagerUserId == decider.UserId ||
                        (incident.CurrentManagerUserId != null && decider.Delegators.ContainsKey(incident.CurrentManagerUserId.Value));
        if (!holdsStep || incident.SubmitterUserId == decider.UserId)
        {
            throw new AbpAuthorizationException();
        }

        incident.ForwardToHr();
        incident.AddTimeline(GuidGenerator.Create(), Clock.Now, IncidentTimelineActions.ForwardedToHr, decider.UserId, input.Note);
        await _incidents.UpdateAsync(incident, autoSave: true);

        await _taskLog.LogAsync(decider.UserId, AdministrativeTaskType.IncidentForwarded, incident.Id);

        foreach (var recipient in await _audience.GetAsync(HrRoles.HrManager, null, incident.SubmitterUserId))
        {
            await _notifications.PublishAsync(recipient, NotificationTemplateCode.IncidentUpdate, NotificationResourceType.Incident, incident.Id);
        }

        await NotifySubmitterAsync(incident, decider.UserId);
        return await ToDtoAsync(incident);
    }

    private async Task<Guid> GetLinkedManagerUserIdAsync(Employee submitter)
    {
        var manager = submitter.ManagerEmployeeId == null ? null : await _employees.FindAsync(submitter.ManagerEmployeeId.Value);
        if (manager?.UserId == null || manager.Status != EmployeeStatus.Active)
        {
            throw new BusinessException(HrErrorCodes.Incident.NoManagerLinked);
        }

        var user = await _userManager.FindByIdAsync(manager.UserId.Value.ToString());
        return user is { IsActive: true } ? manager.UserId.Value : throw new BusinessException(HrErrorCodes.Incident.NoManagerLinked);
    }

    private static bool HasHrAuthority(RequestDeciderContext decider) =>
        decider.Roles.Contains(HrRoles.HrManager) || decider.Delegators.Values.Any(x => x.Contains(HrRoles.HrManager));

    /// <summary>The people linked to this user, and to the department managers who delegated to them.</summary>
    private async Task<List<Guid>> GetTeamEmployeeIdsAsync(RequestDeciderContext decider)
    {
        var managerUserIds = decider.Delegators
            .Where(x => x.Value.Contains(HrRoles.DepartmentManager))
            .Select(x => x.Key)
            .Append(decider.UserId)
            .ToList();

        var employees = await _employees.GetQueryableAsync();
        var managerIds = employees.Where(x => x.UserId != null && managerUserIds.Contains(x.UserId.Value)).Select(x => x.Id);
        return await AsyncExecuter.ToListAsync(
            employees.Where(x => x.ManagerEmployeeId != null && managerIds.Contains(x.ManagerEmployeeId.Value)).Select(x => x.Id));
    }

    /// <summary>The submitter, their linked manager, and HR. A delegate reaches what their delegator reaches.</summary>
    private async Task CheckCanViewAsync(Incident incident, RequestDeciderContext decider)
    {
        if (incident.SubmitterUserId == decider.UserId || HasHrAuthority(decider))
        {
            return;
        }

        var owner = await _employees.GetAsync(incident.EmployeeId);
        var manager = owner.ManagerEmployeeId == null ? null : await _employees.FindAsync(owner.ManagerEmployeeId.Value);

        if (manager?.UserId == null || (manager.UserId != decider.UserId && !decider.Delegators.ContainsKey(manager.UserId.Value)))
        {
            throw new AbpAuthorizationException();
        }
    }

    private async Task CheckGrantedOrDelegatedAsync(string permission, RequestDeciderContext decider)
    {
        if (decider.Delegators.Count == 0)
        {
            await AuthorizationService.CheckAsync(permission);
        }
    }

    private async Task NotifySubmitterAsync(Incident incident, Guid actorUserId)
    {
        if (incident.SubmitterUserId != actorUserId)
        {
            await _notifications.PublishAsync(incident.SubmitterUserId, NotificationTemplateCode.IncidentUpdate, NotificationResourceType.Incident, incident.Id);
        }
    }

    private async Task<Incident> LoadAsync(Guid id)
    {
        var query = (await _incidents.WithDetailsAsync(x => x.Timeline)).Where(x => x.Id == id);
        return await AsyncExecuter.FirstOrDefaultAsync(query) ?? throw new EntityNotFoundException(typeof(Incident), id);
    }

    private async Task<IncidentListItemPagedResultDto> ToPageAsync(IQueryable<Incident> query, IncidentMyListInput input, string defaultSorting)
    {
        var total = await AsyncExecuter.LongCountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.ApplySorting(input.Sorting, defaultSorting, SortFields).Skip(input.SkipCount).Take(input.MaxResultCount));
        var employees = await _references.GetRefsAsync(items.Select(x => x.EmployeeId));
        var itemRefs = await _itemRefs.GetRefsAsync(items.Where(x => x.ItemId != null).Select(x => x.ItemId!.Value));

        return new IncidentListItemPagedResultDto
        {
            TotalCount = total,
            Items = items.Select(x => new IncidentListItemDto
            {
                Id = x.Id,
                Employee = employees[x.EmployeeId],
                Item = x.ItemId == null ? null : itemRefs.GetValueOrDefault(x.ItemId.Value),
                DescriptionPreview = x.Description.Length <= Incident.PreviewLength ? x.Description : x.Description[..Incident.PreviewLength],
                Status = x.Status,
                StatusLabel = StatusLabel(x),
                CreationTime = x.CreationTime
            }).ToList()
        };
    }

    private async Task<IncidentDto> ToDtoAsync(Incident incident)
    {
        var timeline = incident.Timeline.OrderBy(x => x.OccurredAt).ThenBy(x => x.Id).ToList();
        var users = await _references.GetUserRefsAsync(timeline.Where(x => x.ActorUserId != null).Select(x => x.ActorUserId!.Value));
        var employees = await _references.GetRefsAsync(new[] { incident.EmployeeId });
        var itemRefs = await _itemRefs.GetRefsAsync(incident.ItemId == null ? Array.Empty<Guid>() : new[] { incident.ItemId.Value });

        return new IncidentDto
        {
            Id = incident.Id,
            Employee = employees[incident.EmployeeId],
            Item = incident.ItemId == null ? null : itemRefs.GetValueOrDefault(incident.ItemId.Value),
            Description = incident.Description,
            Status = incident.Status,
            StatusLabel = StatusLabel(incident),
            CurrentHandlerRole = incident.CurrentHandlerRole == null ? null : Enum.Parse<AppRole>(incident.CurrentHandlerRole, ignoreCase: true),
            SourceMemoId = incident.SourceMemoId,
            Timeline = timeline.Select(x => new TimelineEntryDto
            {
                OccurredAt = x.OccurredAt,
                Action = x.Action,
                Actor = x.ActorUserId == null ? null : users[x.ActorUserId.Value],
                Note = x.Note
            }).ToList(),
            ConcurrencyStamp = incident.ConcurrencyStamp,
            CreationTime = incident.CreationTime,
            LastModificationTime = incident.LastModificationTime
        };
    }

    private string StatusLabel(Incident incident) => L["Incident:Status:" + JsonNamingPolicy.CamelCase.ConvertName(incident.Status.ToString())];
}

public class IncidentCreateInputDtoValidator : AbstractValidator<IncidentCreateInputDto>
{
    public IncidentCreateInputDtoValidator()
    {
        RuleFor(x => x.Description).NotNull().Length(1, Incident.MaxDescriptionLength);
    }
}

public class IncidentStatusUpdateInputDtoValidator : AbstractValidator<IncidentStatusUpdateInputDto>
{
    public IncidentStatusUpdateInputDtoValidator()
    {
        RuleFor(x => x.Status).NotNull();
        RuleFor(x => x.Note).MaximumLength(Incident.MaxNoteLength);
        RuleFor(x => x.ConcurrencyStamp).NotEmpty();
    }
}
