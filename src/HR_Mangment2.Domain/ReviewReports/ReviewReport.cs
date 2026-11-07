using System;
using System.Collections.Generic;
using HR_Mangment2.Errors;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.ReviewReports;

/// <summary>
/// The reviewer's follow-up report on each team, sent to the executive director (owner's decision 2026-11-07).
/// A draft changes freely; once submitted it is neither edited nor deleted.
/// </summary>
public class ReviewReport : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid ReviewerUserId { get; private set; }
    public ReviewReportPeriodType PeriodType { get; private set; }
    public DateOnly PeriodFrom { get; private set; }
    public DateOnly PeriodTo { get; private set; }
    public string? Summary { get; private set; }
    public ReviewReportStatus Status { get; private set; }
    public DateTime? SubmittedAt { get; private set; }

    /// <summary>The executive director's first look at the submitted report.</summary>
    public DateTime? ViewedAt { get; private set; }

    public ICollection<ReviewReportTeam> Teams { get; private set; } = new List<ReviewReportTeam>();

    protected ReviewReport()
    {
    }

    public ReviewReport(Guid id, Guid? tenantId, Guid reviewerUserId, ReviewReportPeriodType periodType, DateOnly periodFrom, DateOnly periodTo)
        : base(id)
    {
        TenantId = tenantId;
        ReviewerUserId = reviewerUserId;
        Status = ReviewReportStatus.Draft;
        SetPeriod(periodType, periodFrom, periodTo);
    }

    public bool IsSubmitted => Status == ReviewReportStatus.Submitted;

    public void SetPeriod(ReviewReportPeriodType periodType, DateOnly periodFrom, DateOnly periodTo)
    {
        if (!ReviewReportPeriod.IsValid(periodType, periodFrom, periodTo))
        {
            throw new BusinessException(HrErrorCodes.ReviewReports.InvalidPeriod);
        }

        PeriodType = periodType;
        PeriodFrom = periodFrom;
        PeriodTo = periodTo;
    }

    public void SetSummary(string? summary) => Summary = string.IsNullOrWhiteSpace(summary) ? null : summary.Trim();

    public void Submit(DateTime now)
    {
        Status = ReviewReportStatus.Submitted;
        SubmittedAt = now;
    }

    /// <summary>Safe to repeat: the first look stays.</summary>
    public void MarkViewed(DateTime now) => ViewedAt ??= now;
}

/// <summary>One team in the report: a department manager and the people linked to them, frozen when the report starts.</summary>
public class ReviewReportTeam : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid ReviewReportId { get; private set; }
    public Guid ManagerEmployeeId { get; private set; }
    public Guid? DepartmentId { get; private set; }
    public int MembersCount { get; private set; }
    public int SortOrder { get; private set; }
    public int? CompletionPercent { get; private set; }
    public string? Notes { get; private set; }
    public string? ProjectNotes { get; private set; }

    protected ReviewReportTeam()
    {
    }

    public ReviewReportTeam(Guid id, Guid? tenantId, Guid reviewReportId, Guid managerEmployeeId, Guid? departmentId, int membersCount, int sortOrder)
        : base(id)
    {
        TenantId = tenantId;
        ReviewReportId = reviewReportId;
        ManagerEmployeeId = managerEmployeeId;
        DepartmentId = departmentId;
        MembersCount = membersCount;
        SortOrder = sortOrder;
    }

    public void Assess(int? completionPercent, string? notes, string? projectNotes)
    {
        CompletionPercent = completionPercent;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        ProjectNotes = string.IsNullOrWhiteSpace(projectNotes) ? null : projectNotes.Trim();
    }
}

/// <summary>The period rules of ReviewReport_Create in the contract.</summary>
public static class ReviewReportPeriod
{
    public static bool IsValid(ReviewReportPeriodType periodType, DateOnly periodFrom, DateOnly periodTo) => periodType switch
    {
        ReviewReportPeriodType.Daily => periodFrom == periodTo,
        ReviewReportPeriodType.Weekly => periodTo == periodFrom.AddDays(6),
        ReviewReportPeriodType.Monthly => periodFrom.Day == 1 && periodTo == periodFrom.AddMonths(1).AddDays(-1),
        _ => false
    };
}
