using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.ReviewerEvaluations;

public class ReviewerEvaluationCriterionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class ReviewerEvaluationCriterionPagedResultDto
{
    public long TotalCount { get; set; }
    public List<ReviewerEvaluationCriterionDto> Items { get; set; } = new();
}

public class ReviewerEvaluationCriterionCreateDto
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int? SortOrder { get; set; }
}

public class ReviewerEvaluationCriterionUpdateDto
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class ReviewerEvaluationCriterionListInput : PagedAndSortedResultRequestDto
{
    public bool? IsActive { get; set; }
}

public class ReviewerEvaluationItemInputDto
{
    public Guid? CriterionId { get; set; }
    public ReviewerGrade? Grade { get; set; }
    public string? Reason { get; set; }
}

public class ReviewerEvaluationCreateDto
{
    public Guid? EmployeeId { get; set; }
    public DateOnly? EvaluationDate { get; set; }
    public List<ReviewerEvaluationItemInputDto> Items { get; set; } = null!;
    public string? Notes { get; set; }
}

public class ReviewerEvaluationItemDto
{
    public Guid CriterionId { get; set; }
    public string CriterionName { get; set; } = null!;
    public ReviewerGrade Grade { get; set; }
    public string? Reason { get; set; }
}

/// <summary>A graded evaluation with reasons; no numbers and no average (decision 2026-11-11).</summary>
public class ReviewerEvaluationDto
{
    public Guid Id { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public UserRefDto EvaluatedBy { get; set; } = null!;
    public DateOnly EvaluationDate { get; set; }
    public DateTime EvaluatedAt { get; set; }
    public List<ReviewerEvaluationItemDto> Items { get; set; } = new();
    public string? Notes { get; set; }
    public Guid? ReportId { get; set; }
    public DateTime? SentAt { get; set; }
}

public class ReviewerEvaluationPagedResultDto
{
    public long TotalCount { get; set; }
    public List<ReviewerEvaluationDto> Items { get; set; } = new();
}

public class ReviewerEvaluationListInput : PagedAndSortedResultRequestDto
{
    public Guid? EmployeeId { get; set; }
    public bool? IsSent { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
}

/// <summary>Search of ReviewerEvaluation_GetCandidates.</summary>
public class ReviewerEvaluationCandidateInput : PagedResultRequestDto
{
    [System.ComponentModel.DataAnnotations.StringLength(100)]
    public string? Search { get; set; }
}

public class ReviewerEvaluationReportCreateDto
{
    public string? Note { get; set; }
}

public class ReviewerEvaluationReportSummaryDto
{
    public Guid Id { get; set; }
    public UserRefDto SentBy { get; set; } = null!;
    public DateTime SentAt { get; set; }
    public string? Note { get; set; }
    public int EvaluationsCount { get; set; }
    public DateOnly FirstEvaluationDate { get; set; }
    public DateOnly LastEvaluationDate { get; set; }
}

public class ReviewerEvaluationReportDto : ReviewerEvaluationReportSummaryDto
{
    public List<ReviewerEvaluationDto> Evaluations { get; set; } = new();
}

public class ReviewerEvaluationReportPagedResultDto
{
    public long TotalCount { get; set; }
    public List<ReviewerEvaluationReportSummaryDto> Items { get; set; } = new();
}

public interface IReviewerEvaluationCriterionAppService : IApplicationService
{
    Task<ReviewerEvaluationCriterionPagedResultDto> GetListAsync(ReviewerEvaluationCriterionListInput input);
    Task<ReviewerEvaluationCriterionDto> CreateAsync(ReviewerEvaluationCriterionCreateDto input);
    Task<ReviewerEvaluationCriterionDto> UpdateAsync(Guid id, ReviewerEvaluationCriterionUpdateDto input);
}

public interface IReviewerEvaluationAppService : IApplicationService
{
    Task<ReviewerEvaluationPagedResultDto> GetListAsync(ReviewerEvaluationListInput input);
    Task<ReviewerEvaluationDto> CreateAsync(ReviewerEvaluationCreateDto input, Guid? idempotencyKey);
    Task<ReviewerEvaluationDto> GetAsync(Guid id);
    Task<EmployeeLookupPagedResultDto> GetCandidatesAsync(ReviewerEvaluationCandidateInput input);
    Task<ReviewerEvaluationReportDto> SendOneAsync(Guid id, Guid? idempotencyKey);
    Task<ReviewerEvaluationReportPagedResultDto> GetReportListAsync(PagedAndSortedResultRequestDto input);
    Task<ReviewerEvaluationReportDto> SendReportAsync(ReviewerEvaluationReportCreateDto input, Guid? idempotencyKey);
    Task<ReviewerEvaluationReportDto> GetReportAsync(Guid id);
}
