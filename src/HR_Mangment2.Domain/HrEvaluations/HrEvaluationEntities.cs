using System;
using System.Collections.Generic;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.HrEvaluations;

/// <summary>
/// A heading the HR manager scores employees on (professional conduct, attendance...). Separate from the department
/// manager's monthly criteria. Never deleted, only deactivated, so old evaluations keep their names (decision 2026-11-10).
/// </summary>
[Audited]
public class HrEvaluationCriterion : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxNameLength = 100;

    public Guid? TenantId { get; private set; }
    public string NameAr { get; private set; } = null!;
    public string NameEn { get; private set; } = null!;
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    protected HrEvaluationCriterion()
    {
    }

    public HrEvaluationCriterion(Guid id, Guid? tenantId, string nameAr, string nameEn, int sortOrder)
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
/// One HR evaluation of an employee: a 1-10 score per active heading and optional notes. Neither edited nor deleted, and
/// never shown to the employee. It joins a report once the HR manager sends the unsent ones to the executive director.
/// </summary>
[Audited]
public class HrEvaluation : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MinScore = 1;
    public const int MaxScore = 10;
    public const int MaxNotesLength = 4000;
    public const int MaxScores = 50;

    public Guid? TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid EvaluatedByUserId { get; private set; }
    public DateTime EvaluatedAt { get; private set; }
    public string? Notes { get; private set; }
    public Guid? ReportId { get; private set; }
    public DateTime? SentAt { get; private set; }
    public ICollection<HrEvaluationScore> Scores { get; private set; } = new List<HrEvaluationScore>();

    protected HrEvaluation()
    {
    }

    public HrEvaluation(Guid id, Guid? tenantId, Guid employeeId, Guid evaluatedByUserId, DateTime evaluatedAt, string? notes)
        : base(id)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        EvaluatedByUserId = evaluatedByUserId;
        EvaluatedAt = evaluatedAt;
        Notes = notes;
    }

    public void MarkSent(Guid reportId, DateTime sentAt)
    {
        ReportId = reportId;
        SentAt = sentAt;
    }
}

public class HrEvaluationScore : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid HrEvaluationId { get; private set; }
    public Guid CriterionId { get; private set; }
    public int Score { get; private set; }

    protected HrEvaluationScore()
    {
    }

    public HrEvaluationScore(Guid id, Guid? tenantId, Guid hrEvaluationId, Guid criterionId, int score)
        : base(id)
    {
        TenantId = tenantId;
        HrEvaluationId = hrEvaluationId;
        CriterionId = criterionId;
        Score = score;
    }
}

/// <summary>The unsent evaluations of one HR manager, sent together to the executive director (meant weekly).</summary>
[Audited]
public class HrEvaluationReport : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxNoteLength = 2000;

    public Guid? TenantId { get; private set; }
    public Guid SentByUserId { get; private set; }
    public DateTime SentAt { get; private set; }
    public string? Note { get; private set; }
    public int EvaluationsCount { get; private set; }
    public DateTime FirstEvaluatedAt { get; private set; }
    public DateTime LastEvaluatedAt { get; private set; }

    protected HrEvaluationReport()
    {
    }

    public HrEvaluationReport(Guid id, Guid? tenantId, Guid sentByUserId, DateTime sentAt, string? note, int evaluationsCount, DateTime firstEvaluatedAt, DateTime lastEvaluatedAt)
        : base(id)
    {
        TenantId = tenantId;
        SentByUserId = sentByUserId;
        SentAt = sentAt;
        Note = note;
        EvaluationsCount = evaluationsCount;
        FirstEvaluatedAt = firstEvaluatedAt;
        LastEvaluatedAt = lastEvaluatedAt;
    }
}
