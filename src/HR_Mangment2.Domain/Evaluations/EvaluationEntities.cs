using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.Evaluations;

public static class EvaluationConsts
{
    public const int MaxCriterionNameLength = 200;
    public const int MaxGradeNameLength = 100;
    public const int MaxGradeCodeLength = 50;
    public const int MaxNotesLength = 4000;
    public const int MaxResponseLength = 2000;
    public const int MaxReasonLength = 2000;
    public const int MaxInternalNoteLength = 4000;
}

[Audited]
public class EvaluationCriterion : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string NameAr { get; private set; } = null!;
    public string NameEn { get; private set; } = null!;
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    protected EvaluationCriterion()
    {
    }

    public EvaluationCriterion(Guid id, Guid? tenantId, string nameAr, string nameEn, int sortOrder, bool isActive)
        : base(id)
    {
        TenantId = tenantId;
        Set(nameAr, nameEn, sortOrder, isActive);
    }

    public void Set(string nameAr, string nameEn, int sortOrder, bool isActive)
    {
        NameAr = nameAr;
        NameEn = nameEn;
        SortOrder = sortOrder;
        IsActive = isActive;
    }
}

/// <summary>A verbal grade. It has no numeric value anywhere, because the system uses no percentages.</summary>
[Audited]
public class EvaluationGrade : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string Code { get; private set; } = null!;
    public string NameAr { get; private set; } = null!;
    public string NameEn { get; private set; } = null!;
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    protected EvaluationGrade()
    {
    }

    public EvaluationGrade(Guid id, Guid? tenantId, string code, string nameAr, string nameEn, int sortOrder, bool isActive)
        : base(id)
    {
        TenantId = tenantId;
        Code = code;
        Set(nameAr, nameEn, sortOrder, isActive);
    }

    public void Set(string nameAr, string nameEn, int sortOrder, bool isActive)
    {
        NameAr = nameAr;
        NameEn = nameEn;
        SortOrder = sortOrder;
        IsActive = isActive;
    }
}

public class EvaluationCycle : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public EvaluationCycleStatus Status { get; private set; }
    public DateTime OpensAt { get; private set; }
    public DateTime DataFreezeAt { get; private set; }
    public DateTime DueAt { get; private set; }
    public DateTime EscalatesAt { get; private set; }
    public DateTime? FrozenAt { get; private set; }
    public DateTime? LastReminderAt { get; private set; }
    public DateTime? EscalatedAt { get; private set; }

    /// <summary>Copied when the cycle opens, so changing the scale later affects only later cycles.</summary>
    public ICollection<EvaluationCycleCriterion> Criteria { get; private set; } = new List<EvaluationCycleCriterion>();

    public ICollection<EvaluationCycleGrade> Grades { get; private set; } = new List<EvaluationCycleGrade>();

    protected EvaluationCycle()
    {
    }

    public EvaluationCycle(Guid id, Guid? tenantId, int year, int month, DateTime opensAt, DateTime dataFreezeAt, DateTime dueAt, DateTime escalatesAt)
        : base(id)
    {
        TenantId = tenantId;
        Year = year;
        Month = month;
        Status = EvaluationCycleStatus.Scheduled;
        OpensAt = opensAt;
        DataFreezeAt = dataFreezeAt;
        DueAt = dueAt;
        EscalatesAt = escalatesAt;
    }

    public DateOnly PeriodFrom => new(Year, Month, 1);

    public DateOnly PeriodTo => PeriodFrom.AddMonths(1).AddDays(-1);

    public void Reschedule(DateTime opensAt, DateTime dataFreezeAt, DateTime dueAt, DateTime escalatesAt)
    {
        OpensAt = opensAt;
        DataFreezeAt = dataFreezeAt;
        DueAt = dueAt;
        EscalatesAt = escalatesAt;
    }

    public void Open() => Status = EvaluationCycleStatus.Open;

    public void Close() => Status = EvaluationCycleStatus.Closed;

    public void MarkFrozen(DateTime now) => FrozenAt = now;

    public void MarkReminded(DateTime now) => LastReminderAt = now;

    public void MarkEscalated(DateTime now) => EscalatedAt = now;
}

public class EvaluationCycleCriterion : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid CycleId { get; private set; }
    public Guid CriterionId { get; private set; }
    public string NameAr { get; private set; } = null!;
    public string NameEn { get; private set; } = null!;
    public int SortOrder { get; private set; }

    protected EvaluationCycleCriterion()
    {
    }

    public EvaluationCycleCriterion(Guid id, Guid? tenantId, Guid cycleId, EvaluationCriterion criterion)
        : base(id)
    {
        TenantId = tenantId;
        CycleId = cycleId;
        CriterionId = criterion.Id;
        NameAr = criterion.NameAr;
        NameEn = criterion.NameEn;
        SortOrder = criterion.SortOrder;
    }
}

public class EvaluationCycleGrade : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid CycleId { get; private set; }
    public string Code { get; private set; } = null!;
    public string NameAr { get; private set; } = null!;
    public string NameEn { get; private set; } = null!;
    public int SortOrder { get; private set; }

    protected EvaluationCycleGrade()
    {
    }

    public EvaluationCycleGrade(Guid id, Guid? tenantId, Guid cycleId, EvaluationGrade grade)
        : base(id)
    {
        TenantId = tenantId;
        CycleId = cycleId;
        Code = grade.Code;
        NameAr = grade.NameAr;
        NameEn = grade.NameEn;
        SortOrder = grade.SortOrder;
    }
}

[Audited]
public class Evaluation : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid CycleId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public EvaluationStatus Status { get; private set; }
    public string? EmployeeNotes { get; private set; }

    /// <summary>Null until submitted. Until then the evaluator is whoever manages the employee now, so a change of manager moves the form by itself.</summary>
    public Guid? SubmittedByUserId { get; private set; }

    public DateTime? SubmittedAt { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }
    public EvaluationObjectionStatus ObjectionStatus { get; private set; }

    /// <summary>Filled when the cycle freezes. Until then the numbers are read live.</summary>
    public DateTime? AttendanceFrozenAt { get; private set; }

    public int ScheduledWorkingDays { get; private set; }
    public int PresentDays { get; private set; }
    public int AbsentDays { get; private set; }
    public int LateCount { get; private set; }
    public int LateMinutes { get; private set; }
    public decimal LeaveDays { get; private set; }

    public ICollection<EvaluationScore> Scores { get; private set; } = new List<EvaluationScore>();
    public ICollection<EvaluationResponse> Responses { get; private set; } = new List<EvaluationResponse>();

    protected Evaluation()
    {
    }

    public Evaluation(Guid id, Guid? tenantId, Guid cycleId, Guid employeeId)
        : base(id)
    {
        TenantId = tenantId;
        CycleId = cycleId;
        EmployeeId = employeeId;
        Status = EvaluationStatus.Draft;
        ObjectionStatus = EvaluationObjectionStatus.None;
    }

    public void SetEmployeeNotes(string? notes) => EmployeeNotes = notes;

    public void Submit(Guid userId, DateTime now)
    {
        Status = EvaluationStatus.Submitted;
        SubmittedByUserId = userId;
        SubmittedAt = now;
    }

    public void Acknowledge(DateTime now)
    {
        Status = EvaluationStatus.Acknowledged;
        AcknowledgedAt = now;
    }

    public void SetObjectionStatus(EvaluationObjectionStatus status) => ObjectionStatus = status;

    public void FreezeAttendance(DateTime now, int scheduled, int present, int absent, int lateCount, int lateMinutes, decimal leaveDays)
    {
        AttendanceFrozenAt = now;
        ScheduledWorkingDays = scheduled;
        PresentDays = present;
        AbsentDays = absent;
        LateCount = lateCount;
        LateMinutes = lateMinutes;
        LeaveDays = leaveDays;
    }
}

public class EvaluationScore : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid EvaluationId { get; private set; }
    public Guid CriterionId { get; private set; }
    public string GradeCode { get; private set; } = null!;

    protected EvaluationScore()
    {
    }

    public EvaluationScore(Guid id, Guid? tenantId, Guid evaluationId, Guid criterionId, string gradeCode)
        : base(id)
    {
        TenantId = tenantId;
        EvaluationId = evaluationId;
        CriterionId = criterionId;
        GradeCode = gradeCode;
    }
}

public class EvaluationResponse : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid EvaluationId { get; private set; }
    public EvaluationResponseType Type { get; private set; }
    public string Content { get; private set; } = null!;
    public DateTime CreationTime { get; private set; }
    public EvaluationObjectionDecision? Decision { get; private set; }
    public string? DecisionReason { get; private set; }
    public Guid? DecidedByUserId { get; private set; }
    public DateTime? DecidedAt { get; private set; }

    protected EvaluationResponse()
    {
    }

    public EvaluationResponse(Guid id, Guid? tenantId, Guid evaluationId, EvaluationResponseType type, string content, DateTime now)
        : base(id)
    {
        TenantId = tenantId;
        EvaluationId = evaluationId;
        Type = type;
        Content = content;
        CreationTime = now;
    }

    public void Decide(EvaluationObjectionDecision decision, string reason, Guid userId, DateTime now)
    {
        Decision = decision;
        DecisionReason = reason;
        DecidedByUserId = userId;
        DecidedAt = now;
    }
}

/// <summary>Add-only, and never part of any evaluation DTO, notification, export or report. Only its author and the HR manager read it, through its own routes.</summary>
public class EvaluationInternalNote : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid EvaluationId { get; private set; }
    public Guid AuthorUserId { get; private set; }
    public string Content { get; private set; } = null!;

    protected EvaluationInternalNote()
    {
    }

    public EvaluationInternalNote(Guid id, Guid? tenantId, Guid evaluationId, Guid authorUserId, string content)
        : base(id)
    {
        TenantId = tenantId;
        EvaluationId = evaluationId;
        AuthorUserId = authorUserId;
        Content = content;
    }
}

/// <summary>One per author per evaluation. It stays with its author, so it does not follow the form to a new manager.</summary>
public class EvaluationInternalNoteDraft : AuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid EvaluationId { get; private set; }
    public Guid AuthorUserId { get; private set; }
    public string Content { get; private set; } = null!;

    protected EvaluationInternalNoteDraft()
    {
    }

    public EvaluationInternalNoteDraft(Guid id, Guid? tenantId, Guid evaluationId, Guid authorUserId, string content)
        : base(id)
    {
        TenantId = tenantId;
        EvaluationId = evaluationId;
        AuthorUserId = authorUserId;
        Content = content;
    }

    public void SetContent(string content) => Content = content;
}

public class DepartmentReportEmployeeRow
{
    public Guid EmployeeId { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateCount { get; set; }
    public int LateMinutes { get; set; }
    public decimal LeaveDays { get; set; }
    public int PendingRequestsCount { get; set; }
}

/// <summary>What the report froze on the first working day of the next month. Holds no internal note, money or complaint.</summary>
public class DepartmentReportSnapshot
{
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public DateTime FrozenAt { get; set; }
    public List<DepartmentReportEmployeeRow> Employees { get; set; } = new();
    public int SharedItemsCount { get; set; }
    public int PersonalItemsCount { get; set; }
    public int UnacknowledgedItemsCount { get; set; }
    public DateOnly? LastCountDate { get; set; }
    public int CircularsSentCount { get; set; }
    public int CircularRecipientsCount { get; set; }
    public int CircularAcknowledgedCount { get; set; }
}

/// <summary>The evaluation side of a report, read live until the report is submitted and fixed by that submission.</summary>
public class DepartmentReportEvaluationPin
{
    public Dictionary<Guid, EvaluationStatus> Statuses { get; set; } = new();
    public int ExpectedCount { get; set; }
    public int SubmittedCount { get; set; }
    public int AcknowledgedCount { get; set; }
    public int PendingObjectionsCount { get; set; }
    public Dictionary<string, int> GradeCounts { get; set; } = new();
}

[Audited]
public class DepartmentReport : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid CycleId { get; private set; }
    public Guid ManagerUserId { get; private set; }

    /// <summary>Null when the manager's staff span several departments.</summary>
    public Guid? DepartmentId { get; private set; }

    public DepartmentReportStatus Status { get; private set; }
    public string SnapshotJson { get; private set; } = null!;
    public string? EvaluationPinJson { get; private set; }
    public string? Notes { get; private set; }
    public DateTime? SubmittedAt { get; private set; }

    protected DepartmentReport()
    {
    }

    public DepartmentReport(Guid id, Guid? tenantId, Guid cycleId, Guid managerUserId, Guid? departmentId, string snapshotJson)
        : base(id)
    {
        TenantId = tenantId;
        CycleId = cycleId;
        ManagerUserId = managerUserId;
        DepartmentId = departmentId;
        SnapshotJson = snapshotJson;
        Status = DepartmentReportStatus.Draft;
    }

    public void SetNotes(string? notes) => Notes = notes;

    public void Submit(DateTime now, string evaluationPinJson)
    {
        Status = DepartmentReportStatus.Submitted;
        SubmittedAt = now;
        EvaluationPinJson = evaluationPinJson;
    }
}

/// <summary>Neither edited nor deleted after sending.</summary>
[Audited]
public class Circular : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public const int MaxTitleLength = 200;
    public const int MaxContentLength = 8000;

    public Guid? TenantId { get; private set; }
    public CircularScope Scope { get; private set; }
    public string Title { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public Guid SenderUserId { get; private set; }
    public DateTime SentAt { get; private set; }
    public ICollection<CircularRecipient> Recipients { get; private set; } = new List<CircularRecipient>();

    protected Circular()
    {
    }

    public Circular(Guid id, Guid? tenantId, CircularScope scope, string title, string content, Guid senderUserId, DateTime sentAt)
        : base(id)
    {
        TenantId = tenantId;
        Scope = scope;
        Title = title;
        Content = content;
        SenderUserId = senderUserId;
        SentAt = sentAt;
    }
}

public class CircularRecipient : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid CircularId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }

    protected CircularRecipient()
    {
    }

    public CircularRecipient(Guid id, Guid? tenantId, Guid circularId, Guid employeeId, Guid userId)
        : base(id)
    {
        TenantId = tenantId;
        CircularId = circularId;
        EmployeeId = employeeId;
        UserId = userId;
    }

    /// <summary>Safe to repeat: the first time stays.</summary>
    public void Acknowledge(DateTime now) => AcknowledgedAt ??= now;
}
