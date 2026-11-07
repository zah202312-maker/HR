using System;
using System.Collections.Generic;
using HR_Mangment2.Identity;
using HR_Mangment2.Requests;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Memos;

/// <summary>Moves from one holder to the next and ends converted or closed. The row is never deleted.</summary>
[Audited]
public class Memo : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxContentLength = 4000;
    public const int MaxNoteLength = 2000;
    public const int PreviewLength = 200;

    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid WriterUserId { get; private set; }
    public MemoKind Kind { get; private set; }
    public string Content { get; private set; } = null!;
    public MemoStatus Status { get; private set; }

    /// <summary>The writer's row when the memo was written. It decides where the last stage is, so a later role change does not reroute it.</summary>
    public RequestOwnerRow WriterRow { get; private set; }

    /// <summary>Null once the memo is final.</summary>
    public string? CurrentHandlerRole { get; private set; }

    /// <summary>Null means any holder of the role.</summary>
    public Guid? CurrentHandlerUserId { get; private set; }

    public string? CloseReason { get; private set; }
    public Guid? ConvertedRequestId { get; private set; }
    public Guid? ConvertedIncidentId { get; private set; }

    /// <summary>Everyone the memo has reached, which is who may still read it.</summary>
    public ICollection<MemoHandler> Handlers { get; private set; } = new List<MemoHandler>();

    public ICollection<MemoTimelineEntry> Timeline { get; private set; } = new List<MemoTimelineEntry>();

    protected Memo()
    {
    }

    public Memo(Guid id, Guid? tenantId, Guid employeeId, Guid writerUserId, MemoKind kind, string content, RequestOwnerRow writerRow)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        WriterUserId = writerUserId;
        Kind = kind;
        Content = content;
        WriterRow = writerRow;
    }

    public bool IsFinal => CurrentHandlerRole == null;

    /// <summary>A memo from the HR manager cannot go to HR, so the executive is the last stage for it. Everyone else's last stage is HR.</summary>
    public bool IsAtLastStage => Status == MemoStatus.PendingHr || (Status == MemoStatus.PendingExecutive && WriterRow == RequestOwnerRow.HrManager);

    public bool CanForwardToHr => Status == MemoStatus.PendingManager || (Status == MemoStatus.PendingExecutive && WriterRow != RequestOwnerRow.HrManager);

    public void ArriveAt(Guid handlerEntryId, MemoStatus status, string role, Guid? userId, DateTime now)
    {
        Status = status;
        CurrentHandlerRole = role;
        CurrentHandlerUserId = userId;
        Handlers.Add(new MemoHandler(handlerEntryId, TenantId, Id, role, userId, now));
    }

    public void ReassignManager(Guid handlerEntryId, Guid newManagerUserId, DateTime now) =>
        ArriveAt(handlerEntryId, Status, CurrentHandlerRole!, newManagerUserId, now);

    public void MarkConvertedToRequest(Guid requestId)
    {
        ConvertedRequestId = requestId;
        Finish(MemoStatus.ConvertedToRequest);
    }

    public void MarkConvertedToIncident(Guid incidentId)
    {
        ConvertedIncidentId = incidentId;
        Finish(MemoStatus.ConvertedToIncident);
    }

    public void Close(string reason)
    {
        CloseReason = reason;
        Finish(MemoStatus.Closed);
    }

    public void AddTimeline(Guid entryId, DateTime occurredAt, string action, Guid? actorUserId, string? note) =>
        Timeline.Add(new MemoTimelineEntry(entryId, TenantId, Id, occurredAt, action, actorUserId, note));

    private void Finish(MemoStatus status)
    {
        Status = status;
        CurrentHandlerRole = null;
        CurrentHandlerUserId = null;
    }
}

public class MemoHandler : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid MemoId { get; private set; }
    public string HandlerRole { get; private set; } = null!;
    public Guid? HandlerUserId { get; private set; }
    public DateTime ArrivedAt { get; private set; }

    protected MemoHandler()
    {
    }

    public MemoHandler(Guid id, Guid? tenantId, Guid memoId, string handlerRole, Guid? handlerUserId, DateTime arrivedAt)
        : base(id)
    {
        TenantId = tenantId;
        MemoId = memoId;
        HandlerRole = handlerRole;
        HandlerUserId = handlerUserId;
        ArrivedAt = arrivedAt;
    }
}

public class MemoTimelineEntry : Entity<Guid>, IMultiTenant
{
    public const int MaxActionLength = 32;

    public Guid? TenantId { get; private set; }
    public Guid MemoId { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string Action { get; private set; } = null!;
    public Guid? ActorUserId { get; private set; }
    public string? Note { get; private set; }

    protected MemoTimelineEntry()
    {
    }

    public MemoTimelineEntry(Guid id, Guid? tenantId, Guid memoId, DateTime occurredAt, string action, Guid? actorUserId, string? note)
        : base(id)
    {
        TenantId = tenantId;
        MemoId = memoId;
        OccurredAt = occurredAt;
        Action = action;
        ActorUserId = actorUserId;
        Note = note;
    }
}

[Audited]
public class Incident : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxDescriptionLength = 4000;
    public const int MaxNoteLength = 2000;
    public const int PreviewLength = 200;

    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid SubmitterUserId { get; private set; }
    public Guid? ItemId { get; private set; }
    public string Description { get; private set; } = null!;
    public IncidentStatus Status { get; private set; }

    /// <summary>The linked manager while the incident waits for them. HR is a role, so no one is named after that.</summary>
    public Guid? CurrentManagerUserId { get; private set; }

    public Guid? SourceMemoId { get; private set; }

    public ICollection<IncidentTimelineEntry> Timeline { get; private set; } = new List<IncidentTimelineEntry>();

    protected Incident()
    {
    }

    public Incident(Guid id, Guid? tenantId, Guid employeeId, Guid submitterUserId, Guid? itemId, string description, Guid? managerUserId, Guid? sourceMemoId)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        SubmitterUserId = submitterUserId;
        ItemId = itemId;
        Description = description;
        CurrentManagerUserId = managerUserId;
        Status = managerUserId == null ? IncidentStatus.PendingHr : IncidentStatus.PendingManager;
        SourceMemoId = sourceMemoId;
    }

    public string? CurrentHandlerRole => Status switch
    {
        IncidentStatus.PendingManager => HrRoles.DepartmentManager,
        IncidentStatus.Resolved => null,
        _ => HrRoles.HrManager
    };

    public void ForwardToHr()
    {
        Status = IncidentStatus.PendingHr;
        CurrentManagerUserId = null;
    }

    public void ReassignManager(Guid newManagerUserId) => CurrentManagerUserId = newManagerUserId;

    /// <returns>False when the rules do not allow this move.</returns>
    public bool TryMoveTo(IncidentStatusTarget target)
    {
        switch (Status, target)
        {
            case (IncidentStatus.PendingHr, IncidentStatusTarget.InProgress):
                Status = IncidentStatus.InProgress;
                return true;
            case (IncidentStatus.PendingHr, IncidentStatusTarget.Resolved):
            case (IncidentStatus.InProgress, IncidentStatusTarget.Resolved):
                Status = IncidentStatus.Resolved;
                return true;
            default:
                return false;
        }
    }

    public void AddTimeline(Guid entryId, DateTime occurredAt, string action, Guid? actorUserId, string? note) =>
        Timeline.Add(new IncidentTimelineEntry(entryId, TenantId, Id, occurredAt, action, actorUserId, note));
}

public class IncidentTimelineEntry : Entity<Guid>, IMultiTenant
{
    public const int MaxActionLength = 32;

    public Guid? TenantId { get; private set; }
    public Guid IncidentId { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string Action { get; private set; } = null!;
    public Guid? ActorUserId { get; private set; }
    public string? Note { get; private set; }

    protected IncidentTimelineEntry()
    {
    }

    public IncidentTimelineEntry(Guid id, Guid? tenantId, Guid incidentId, DateTime occurredAt, string action, Guid? actorUserId, string? note)
        : base(id)
    {
        TenantId = tenantId;
        IncidentId = incidentId;
        OccurredAt = occurredAt;
        Action = action;
        ActorUserId = actorUserId;
        Note = note;
    }
}
