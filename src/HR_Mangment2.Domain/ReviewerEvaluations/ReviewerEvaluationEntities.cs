using System;
using System.Collections.Generic;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.ReviewerEvaluations;

/// <summary>
/// A heading the reviewer grades employees on. Separate from the HR manager's headings and the monthly criteria.
/// Never deleted, only deactivated, so old evaluations keep their names (decision 2026-11-11).
/// </summary>
[Audited]
public class ReviewerEvaluationCriterion : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxNameLength = 100;

    public Guid? TenantId { get; private set; }
    public string NameAr { get; private set; } = null!;
    public string NameEn { get; private set; } = null!;
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    protected ReviewerEvaluationCriterion()
    {
    }

    public ReviewerEvaluationCriterion(Guid id, Guid? tenantId, string nameAr, string nameEn, int sortOrder)
        : base(id)
    {
        TenantId = tenantId;
        Set(nameAr, nameEn, sortOrder, true);
    }

    public void Set(string nameAr, string nameEn, int sortOrder, bool isActive)
    {
        NameAr = nameAr;
        NameEn = nameEn;
        SortOrder = sortOrder;
        IsActive = isActive;
    }
}

/// <summary>
/// One evaluation of an employee by the reviewer for a day the reviewer picks: a grade and an optional reason per active
/// heading, and optional notes. No numbers and no average. Neither edited nor deleted, and never shown to the employee.
/// </summary>
[Audited]
public class ReviewerEvaluation : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxNotesLength = 4000;
    public const int MaxReasonLength = 2000;
    public const int MaxItems = 50;

    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid EvaluatedByUserId { get; private set; }
    public DateOnly EvaluationDate { get; private set; }
    public DateTime EvaluatedAt { get; private set; }
    public string? Notes { get; private set; }
    public Guid? ReportId { get; private set; }
    public DateTime? SentAt { get; private set; }
    public ICollection<ReviewerEvaluationItem> Items { get; private set; } = new List<ReviewerEvaluationItem>();

    protected ReviewerEvaluation()
    {
    }

    public ReviewerEvaluation(Guid id, Guid? tenantId, Guid employeeId, Guid evaluatedByUserId, DateOnly evaluationDate, DateTime evaluatedAt, string? notes)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        EvaluatedByUserId = evaluatedByUserId;
        EvaluationDate = evaluationDate;
        EvaluatedAt = evaluatedAt;
        Notes = notes;
    }

    public void MarkSent(Guid reportId, DateTime sentAt)
    {
        ReportId = reportId;
        SentAt = sentAt;
    }
}

public class ReviewerEvaluationItem : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid ReviewerEvaluationId { get; private set; }
    public Guid CriterionId { get; private set; }
    public ReviewerGrade Grade { get; private set; }
    public string? Reason { get; private set; }

    protected ReviewerEvaluationItem()
    {
    }

    public ReviewerEvaluationItem(Guid id, Guid? tenantId, Guid reviewerEvaluationId, Guid criterionId, ReviewerGrade grade, string? reason)
        : base(id)
    {
        TenantId = tenantId;
        ReviewerEvaluationId = reviewerEvaluationId;
        CriterionId = criterionId;
        Grade = grade;
        Reason = reason;
    }
}

/// <summary>Evaluations of one reviewer sent together to the executive director.</summary>
[Audited]
public class ReviewerEvaluationReport : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxNoteLength = 2000;

    public Guid? TenantId { get; private set; }
    public Guid SentByUserId { get; private set; }
    public DateTime SentAt { get; private set; }
    public string? Note { get; private set; }
    public int EvaluationsCount { get; private set; }
    public DateOnly FirstEvaluationDate { get; private set; }
    public DateOnly LastEvaluationDate { get; private set; }

    protected ReviewerEvaluationReport()
    {
    }

    public ReviewerEvaluationReport(Guid id, Guid? tenantId, Guid sentByUserId, DateTime sentAt, string? note, int evaluationsCount, DateOnly firstEvaluationDate, DateOnly lastEvaluationDate)
        : base(id)
    {
        TenantId = tenantId;
        SentByUserId = sentByUserId;
        SentAt = sentAt;
        Note = note;
        EvaluationsCount = evaluationsCount;
        FirstEvaluationDate = firstEvaluationDate;
        LastEvaluationDate = lastEvaluationDate;
    }
}
