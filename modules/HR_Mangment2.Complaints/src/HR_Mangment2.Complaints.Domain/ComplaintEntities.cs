using System;
using System.Collections.Generic;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Complaints;

/// <summary>
/// Never deleted, and without ISoftDelete on purpose. Content is sealed with the complaint's own key, and the
/// general audit log must never see it. People are plain ids: nothing on an employee's file leads back here.
/// </summary>
[DisableAuditing]
public class Complaint : AggregateRoot<Guid>, IMultiTenant
{
    public const int MaxSubjectLength = 200;
    public const int MaxContentLength = 8000;
    public const int MaxNoteLength = 4000;
    public const int MaxTransitionNoteLength = 2000;
    public const int MaxDecisionLength = 4000;
    public const int ReferenceNumberLength = 16;

    public Guid? TenantId { get; private set; }
    public string ReferenceNumber { get; private set; } = null!;
    public Guid ComplainantEmployeeId { get; private set; }
    public Guid ComplainantUserId { get; private set; }
    public ComplaintTargetKind TargetKind { get; private set; }
    public Guid? RespondentEmployeeId { get; private set; }
    public Guid? RespondentUserId { get; private set; }
    public ComplaintRespondentCategory? RespondentCategory { get; private set; }
    public ComplaintLevel Level { get; private set; }
    public ComplaintLevel MinimumLevel { get; private set; }
    public bool LevelRaisedBySystem { get; private set; }
    public ComplaintStatus Status { get; private set; }

    /// <summary>The department manager while it waits for them. After that the handler follows from the level and the rules.</summary>
    public Guid? DepartmentManagerUserId { get; private set; }

    public byte[] WrappedKey { get; private set; } = null!;
    public byte[] EncryptedSubject { get; private set; } = null!;
    public byte[] EncryptedContent { get; private set; } = null!;
    public byte[]? EncryptedClosureDecision { get; private set; }
    public DateTime SubmittedAt { get; private set; }
    public DateTime? DueAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public DateTime? LastOverdueReminderAt { get; private set; }

    public ICollection<ComplaintAction> Actions { get; private set; } = new List<ComplaintAction>();
    public ICollection<ComplaintAttachment> Attachments { get; private set; } = new List<ComplaintAttachment>();

    protected Complaint()
    {
    }

    public Complaint(
        Guid id,
        Guid? tenantId,
        string referenceNumber,
        Guid complainantEmployeeId,
        Guid complainantUserId,
        ComplaintTargetKind targetKind,
        Guid? respondentEmployeeId,
        Guid? respondentUserId,
        ComplaintRespondentCategory? respondentCategory,
        ComplaintLevel level,
        ComplaintLevel minimumLevel,
        bool levelRaisedBySystem,
        ComplaintStatus status,
        Guid? departmentManagerUserId,
        byte[] wrappedKey,
        byte[] encryptedSubject,
        byte[] encryptedContent,
        DateTime submittedAt,
        DateTime? dueAt)
        : base(id)
    {
        TenantId = tenantId;
        ReferenceNumber = referenceNumber;
        ComplainantEmployeeId = complainantEmployeeId;
        ComplainantUserId = complainantUserId;
        TargetKind = targetKind;
        RespondentEmployeeId = respondentEmployeeId;
        RespondentUserId = respondentUserId;
        RespondentCategory = respondentCategory;
        Level = level;
        MinimumLevel = minimumLevel;
        LevelRaisedBySystem = levelRaisedBySystem;
        Status = status;
        DepartmentManagerUserId = departmentManagerUserId;
        WrappedKey = wrappedKey;
        EncryptedSubject = encryptedSubject;
        EncryptedContent = encryptedContent;
        SubmittedAt = submittedAt;
        DueAt = dueAt;
    }

    public bool IsClosed => Status == ComplaintStatus.Closed;

    public void RaiseTo(ComplaintLevel level)
    {
        Level = level;
        DepartmentManagerUserId = null;
        Status = level == ComplaintLevel.HighlyConfidential ? ComplaintStatus.PendingExecutiveDirector : ComplaintStatus.PendingHrManager;
    }

    public void MoveToHr()
    {
        DepartmentManagerUserId = null;
        Status = ComplaintStatus.PendingHrManager;
    }

    public void Close(byte[] encryptedDecision, DateTime closedAt)
    {
        EncryptedClosureDecision = encryptedDecision;
        ClosedAt = closedAt;
        Status = ComplaintStatus.Closed;
        DepartmentManagerUserId = null;
    }

    public void MarkOverdueReminded(DateTime now) => LastOverdueReminderAt = now;

    public void AddAction(Guid actionId, ComplaintActionType type, Guid actorUserId, byte[]? encryptedNote, DateTime now) =>
        Actions.Add(new ComplaintAction(actionId, TenantId, Id, type, actorUserId, encryptedNote, now));
}

/// <summary>Append only.</summary>
[DisableAuditing]
public class ComplaintAction : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid ComplaintId { get; private set; }
    public ComplaintActionType Type { get; private set; }
    public Guid ActorUserId { get; private set; }
    public byte[]? EncryptedNote { get; private set; }
    public DateTime CreationTime { get; private set; }

    protected ComplaintAction()
    {
    }

    public ComplaintAction(Guid id, Guid? tenantId, Guid complaintId, ComplaintActionType type, Guid actorUserId, byte[]? encryptedNote, DateTime creationTime)
        : base(id)
    {
        TenantId = tenantId;
        ComplaintId = complaintId;
        Type = type;
        ActorUserId = actorUserId;
        EncryptedNote = encryptedNote;
        CreationTime = creationTime;
    }
}

/// <summary>Never deleted. The file is sealed before it reaches the store.</summary>
[DisableAuditing]
public class ComplaintAttachment : Entity<Guid>, IMultiTenant
{
    public const int MaxContentTypeLength = 128;

    public Guid? TenantId { get; private set; }
    public Guid ComplaintId { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public byte[] EncryptedFileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long SizeBytes { get; private set; }
    public string BlobName { get; private set; } = null!;
    public DateTime CreationTime { get; private set; }

    protected ComplaintAttachment()
    {
    }

    public ComplaintAttachment(Guid id, Guid? tenantId, Guid complaintId, Guid uploadedByUserId, byte[] encryptedFileName, string contentType, long sizeBytes, string blobName, DateTime creationTime)
        : base(id)
    {
        TenantId = tenantId;
        ComplaintId = complaintId;
        UploadedByUserId = uploadedByUserId;
        EncryptedFileName = encryptedFileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        BlobName = blobName;
        CreationTime = creationTime;
    }
}

/// <summary>Append only: no service updates or deletes a row, and the restricted database account cannot either.</summary>
[DisableAuditing]
public class ComplaintAccessLog : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid ComplaintId { get; private set; }
    public Guid UserId { get; private set; }
    public ComplaintAccessAction Action { get; private set; }
    public DateTime At { get; private set; }

    protected ComplaintAccessLog()
    {
    }

    public ComplaintAccessLog(Guid id, Guid? tenantId, Guid complaintId, Guid userId, ComplaintAccessAction action, DateTime at)
        : base(id)
    {
        TenantId = tenantId;
        ComplaintId = complaintId;
        UserId = userId;
        Action = action;
        At = at;
    }
}

/// <summary>Holds no complaint data, so unlike the complaint itself it stays in the general audit log.</summary>
[Audited]
public class ComplaintHandlerAssignment : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxReasonLength = 2000;

    public Guid? TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public ComplaintLevel Level { get; private set; }
    public ComplaintHandlerAssignmentStatus Status { get; private set; }
    public DateTime From { get; private set; }
    public DateTime? To { get; private set; }
    public string Reason { get; private set; } = null!;
    public Guid RequestedByUserId { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public Guid? DecidedByUserId { get; private set; }
    public DateTime? DecidedAt { get; private set; }
    public string? DecisionReason { get; private set; }

    /// <summary>True once the screen permissions given at approval have been taken back.</summary>
    public bool PermissionsReleased { get; private set; }

    protected ComplaintHandlerAssignment()
    {
    }

    public ComplaintHandlerAssignment(Guid id, Guid? tenantId, Guid userId, ComplaintLevel level, DateTime from, DateTime? to, string reason, Guid requestedByUserId, DateTime requestedAt)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        Level = level;
        Status = ComplaintHandlerAssignmentStatus.PendingApproval;
        From = from;
        To = to;
        Reason = reason;
        RequestedByUserId = requestedByUserId;
        RequestedAt = requestedAt;
    }

    public bool IsEffectiveAt(DateTime now) =>
        Status == ComplaintHandlerAssignmentStatus.Approved && From <= now && (To == null || To > now);

    public void Approve(Guid deciderUserId, DateTime now) => Decide(ComplaintHandlerAssignmentStatus.Approved, deciderUserId, null, now);

    public void Reject(Guid deciderUserId, string reason, DateTime now) => Decide(ComplaintHandlerAssignmentStatus.Rejected, deciderUserId, reason, now);

    public void Revoke(Guid deciderUserId, string reason, DateTime now)
    {
        Decide(ComplaintHandlerAssignmentStatus.Revoked, deciderUserId, reason, now);
        PermissionsReleased = true;
    }

    public void MarkPermissionsReleased() => PermissionsReleased = true;

    private void Decide(ComplaintHandlerAssignmentStatus status, Guid deciderUserId, string? reason, DateTime now)
    {
        Status = status;
        DecidedByUserId = deciderUserId;
        DecidedAt = now;
        DecisionReason = reason;
    }
}
