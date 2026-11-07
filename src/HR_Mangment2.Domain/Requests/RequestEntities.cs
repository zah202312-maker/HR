using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Requests;

/// <summary>There is no delete: an unwanted type is switched off, and its open requests finish their routes.</summary>
[Audited]
public class RequestType : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxCodeLength = 64;
    public const int MaxNameLength = 128;

    public Guid? TenantId { get; private set; }
    public string Code { get; private set; } = null!;
    public string NameAr { get; private set; } = null!;
    public string NameEn { get; private set; } = null!;
    public RequestDestination Destination { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsSystem { get; private set; }

    protected RequestType()
    {
    }

    public RequestType(Guid id, Guid? tenantId, string code, string nameAr, string nameEn, RequestDestination destination, bool isActive, bool isSystem)
        : base(id)
    {
        TenantId = tenantId;
        Code = code;
        IsSystem = isSystem;
        Set(nameAr, nameEn, destination, isActive);
    }

    public void Set(string nameAr, string nameEn, RequestDestination destination, bool isActive)
    {
        NameAr = nameAr;
        NameEn = nameEn;
        Destination = destination;
        IsActive = isActive;
    }
}

/// <summary>
/// One record for every kind of request, leave and attendance corrections included. The route is built once at
/// submission, so later changes to a type do not touch it, and the row is never deleted.
/// </summary>
[Audited]
public class Request : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxDescriptionLength = 4000;
    public const int MaxNoteLength = 2000;
    public const int MaxSummaryLength = 512;

    public Guid? TenantId { get; private set; }
    public Guid RequestTypeId { get; private set; }

    /// <summary>Kept apart from the type, because changing a type's destination must not change requests already open.</summary>
    public RequestDestination Destination { get; private set; }

    public Guid EmployeeId { get; private set; }

    /// <summary>The owner's account, when the employee file has one. Self-approval checks compare against this.</summary>
    public Guid? OwnerUserId { get; private set; }

    public Guid SubmittedByUserId { get; private set; }
    public bool IsOnBehalf { get; private set; }
    public string? Description { get; private set; }
    public string DetailsSummaryAr { get; private set; } = null!;
    public string DetailsSummaryEn { get; private set; } = null!;
    public Guid? ItemId { get; private set; }
    public decimal? Quantity { get; private set; }
    public Guid? SourceMemoId { get; private set; }
    public RequestStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }

    /// <summary>Null once the request is final. These two copy the current step, so the inbox can filter in SQL.</summary>
    public string? CurrentApproverRole { get; private set; }

    public Guid? CurrentApproverUserId { get; private set; }
    public int? CurrentStepOrder { get; private set; }

    /// <summary>When the request reached its current step. For display and reminders, and nothing ever acts on it by itself.</summary>
    public DateTime CurrentStepSince { get; private set; }

    public DateTime? LastReminderAt { get; private set; }

    public ICollection<RequestStep> Steps { get; private set; } = new List<RequestStep>();
    public ICollection<RequestTimelineEntry> Timeline { get; private set; } = new List<RequestTimelineEntry>();

    protected Request()
    {
    }

    public Request(
        Guid id,
        Guid? tenantId,
        RequestType type,
        Guid employeeId,
        Guid? ownerUserId,
        Guid submittedByUserId,
        bool isOnBehalf,
        string? description,
        string detailsSummaryAr,
        string detailsSummaryEn,
        Guid? itemId,
        decimal? quantity,
        Guid? sourceMemoId)
        : base(id)
    {
        TenantId = tenantId;
        RequestTypeId = type.Id;
        Destination = type.Destination;
        EmployeeId = employeeId;
        OwnerUserId = ownerUserId;
        SubmittedByUserId = submittedByUserId;
        IsOnBehalf = isOnBehalf;
        Description = description;
        DetailsSummaryAr = Shorten(detailsSummaryAr);
        DetailsSummaryEn = Shorten(detailsSummaryEn);
        ItemId = itemId;
        Quantity = quantity;
        SourceMemoId = sourceMemoId;
    }

    public bool IsPending => CurrentStepOrder != null;

    public RequestStep? CurrentStep => Steps.FirstOrDefault(x => x.Order == CurrentStepOrder);

    /// <summary>Moves to the first step nobody has decided, or leaves the request approved or completed if none remains.</summary>
    public void AdvanceToNextStep(DateTime now)
    {
        var next = Steps.Where(x => x.Decision == RequestStepDecision.NotReached).OrderBy(x => x.Order).FirstOrDefault();
        if (next == null)
        {
            Finish(Destination == RequestDestination.None ? RequestStatus.Approved : RequestStatus.Completed);
            return;
        }

        next.MarkPending();
        Status = next.Stage;
        CurrentStepOrder = next.Order;
        CurrentApproverRole = next.ApproverRole;
        CurrentApproverUserId = next.ApproverUserId;
        CurrentStepSince = now;
        LastReminderAt = null;
    }

    public void Reject(string reason)
    {
        RejectionReason = reason;
        Finish(RequestStatus.Rejected);
    }

    public void Cancel() => Finish(RequestStatus.Cancelled);

    public void ReassignCurrentApprover(Guid newApproverUserId)
    {
        CurrentApproverUserId = newApproverUserId;
        CurrentStep?.ReassignTo(newApproverUserId);
        LastReminderAt = null;
    }

    public void MarkReminded(DateTime now) => LastReminderAt = now;

    public void AddTimeline(Guid entryId, DateTime occurredAt, string action, Guid? actorUserId, string? note) =>
        Timeline.Add(new RequestTimelineEntry(entryId, TenantId, Id, occurredAt, action, actorUserId, note));

    private void Finish(RequestStatus status)
    {
        Status = status;
        CurrentStepOrder = null;
        CurrentApproverRole = null;
        CurrentApproverUserId = null;
    }

    private static string Shorten(string value) => value.Length <= MaxSummaryLength ? value : value[..MaxSummaryLength];
}

public class RequestStep : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid RequestId { get; private set; }
    public int Order { get; private set; }
    public RequestStatus Stage { get; private set; }
    public string ApproverRole { get; private set; } = null!;

    /// <summary>Null means any holder of the role may decide.</summary>
    public Guid? ApproverUserId { get; private set; }

    public RequestStepDecision Decision { get; private set; }
    public Guid? DecidedByUserId { get; private set; }
    public Guid? DelegatedFromUserId { get; private set; }
    public DateTime? DecidedAt { get; private set; }
    public string? Note { get; private set; }

    protected RequestStep()
    {
    }

    public RequestStep(Guid id, Guid? tenantId, Guid requestId, int order, RequestStatus stage, string approverRole, Guid? approverUserId)
        : base(id)
    {
        TenantId = tenantId;
        RequestId = requestId;
        Order = order;
        Stage = stage;
        ApproverRole = approverRole;
        ApproverUserId = approverUserId;
        Decision = RequestStepDecision.NotReached;
    }

    public void MarkPending() => Decision = RequestStepDecision.Pending;

    public void Decide(RequestStepDecision decision, Guid decidedByUserId, Guid? delegatedFromUserId, DateTime decidedAt, string? note)
    {
        Decision = decision;
        DecidedByUserId = decidedByUserId;
        DelegatedFromUserId = delegatedFromUserId;
        DecidedAt = decidedAt;
        Note = note;
    }

    public void ReassignTo(Guid approverUserId) => ApproverUserId = approverUserId;
}

public class RequestTimelineEntry : Entity<Guid>, IMultiTenant
{
    public const int MaxActionLength = 32;

    public Guid? TenantId { get; private set; }
    public Guid RequestId { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string Action { get; private set; } = null!;
    public Guid? ActorUserId { get; private set; }
    public string? Note { get; private set; }

    protected RequestTimelineEntry()
    {
    }

    public RequestTimelineEntry(Guid id, Guid? tenantId, Guid requestId, DateTime occurredAt, string action, Guid? actorUserId, string? note)
        : base(id)
    {
        TenantId = tenantId;
        RequestId = requestId;
        OccurredAt = occurredAt;
        Action = action;
        ActorUserId = actorUserId;
        Note = note;
    }
}
