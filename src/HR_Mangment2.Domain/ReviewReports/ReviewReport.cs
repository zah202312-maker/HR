using System;
using System.Collections.Generic;
using HR_Mangment2.ReviewerEvaluations;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.ReviewReports;

/// <summary>
/// The reviewer's daily report to the executive director (decision 2026-11-11, replacing the period follow-up
/// reports): one per reviewer per day, a full text, and one row per room. A draft changes freely; once submitted it
/// is neither edited nor deleted.
/// </summary>
public class ReviewReport : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid ReviewerUserId { get; private set; }
    public DateOnly ReportDate { get; private set; }
    public string? Content { get; private set; }
    public ReviewReportStatus Status { get; private set; }
    public DateTime? SubmittedAt { get; private set; }

    /// <summary>The executive director's first look at the submitted report.</summary>
    public DateTime? ViewedAt { get; private set; }

    public ICollection<ReviewReportTeam> Teams { get; private set; } = new List<ReviewReportTeam>();

    protected ReviewReport()
    {
    }

    public ReviewReport(Guid id, Guid? tenantId, Guid reviewerUserId, DateOnly reportDate)
        : base(id)
    {
        TenantId = tenantId;
        ReviewerUserId = reviewerUserId;
        ReportDate = reportDate;
        Status = ReviewReportStatus.Draft;
    }

    public bool IsSubmitted => Status == ReviewReportStatus.Submitted;

    /// <summary>The date rules (not in the future, one per day) are checked by the application service.</summary>
    public void SetReportDate(DateOnly reportDate) => ReportDate = reportDate;

    public void SetContent(string? content) => Content = string.IsNullOrWhiteSpace(content) ? null : content.Trim();

    public void Submit(DateTime now)
    {
        Status = ReviewReportStatus.Submitted;
        SubmittedAt = now;
    }

    /// <summary>Safe to repeat: the first look stays.</summary>
    public void MarkViewed(DateTime now) => ViewedAt ??= now;
}

/// <summary>One room in the daily report, frozen when the report starts: its name, lead, department and members.</summary>
public class ReviewReportTeam : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid ReviewReportId { get; private set; }
    public Guid RoomId { get; private set; }
    public string RoomName { get; private set; } = null!;
    public Guid ManagerEmployeeId { get; private set; }
    public Guid? DepartmentId { get; private set; }
    public List<Guid> MemberEmployeeIds { get; private set; } = new();
    public int SortOrder { get; private set; }
    public ReviewerGrade? BackendGrade { get; private set; }
    public string? BackendNotes { get; private set; }
    public ReviewerGrade? FrontendGrade { get; private set; }
    public string? FrontendNotes { get; private set; }
    public string? Notes { get; private set; }

    protected ReviewReportTeam()
    {
    }

    public ReviewReportTeam(
        Guid id,
        Guid? tenantId,
        Guid reviewReportId,
        Guid roomId,
        string roomName,
        Guid managerEmployeeId,
        Guid? departmentId,
        List<Guid> memberEmployeeIds,
        int sortOrder)
        : base(id)
    {
        TenantId = tenantId;
        ReviewReportId = reviewReportId;
        RoomId = roomId;
        RoomName = roomName;
        ManagerEmployeeId = managerEmployeeId;
        DepartmentId = departmentId;
        MemberEmployeeIds = memberEmployeeIds;
        SortOrder = sortOrder;
    }

    public bool IsGraded => BackendGrade != null && FrontendGrade != null;

    public void Assess(ReviewerGrade? backendGrade, string? backendNotes, ReviewerGrade? frontendGrade, string? frontendNotes, string? notes)
    {
        BackendGrade = backendGrade;
        BackendNotes = Clean(backendNotes);
        FrontendGrade = frontendGrade;
        FrontendNotes = Clean(frontendNotes);
        Notes = Clean(notes);
    }

    private static string? Clean(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
