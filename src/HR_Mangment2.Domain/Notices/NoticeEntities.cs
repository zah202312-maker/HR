using System;
using System.Collections.Generic;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Notices;

/// <summary>
/// A notice from the HR manager or the executive director to employees chosen by name. Neither edited nor deleted
/// after sending, and the recipients are fixed at that moment (decision 2026-11-10).
/// </summary>
[Audited]
public class Notice : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxTitleLength = 200;
    public const int MaxContentLength = 8000;
    public const int MaxRecipients = 200;

    public Guid? TenantId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public Guid SenderUserId { get; private set; }
    public DateTime SentAt { get; private set; }
    public ICollection<NoticeRecipient> Recipients { get; private set; } = new List<NoticeRecipient>();

    protected Notice()
    {
    }

    public Notice(Guid id, Guid? tenantId, string title, string content, Guid senderUserId, DateTime sentAt)
        : base(id)
    {
        TenantId = tenantId;
        Title = title;
        Content = content;
        SenderUserId = senderUserId;
        SentAt = sentAt;
    }
}

public class NoticeRecipient : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid NoticeId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }

    protected NoticeRecipient()
    {
    }

    public NoticeRecipient(Guid id, Guid? tenantId, Guid noticeId, Guid employeeId, Guid userId)
        : base(id)
    {
        TenantId = tenantId;
        NoticeId = noticeId;
        EmployeeId = employeeId;
        UserId = userId;
    }

    /// <summary>Safe to repeat: the first time stays.</summary>
    public void Acknowledge(DateTime now) => AcknowledgedAt ??= now;
}
