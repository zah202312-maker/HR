using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.HrEvaluations;

public class HrEvaluationCriterionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class HrEvaluationCriterionPagedResultDto
{
    public long TotalCount { get; set; }
    public List<HrEvaluationCriterionDto> Items { get; set; } = new();
}

public class HrEvaluationCriterionCreateDto
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int? SortOrder { get; set; }
}

public class HrEvaluationCriterionUpdateDto
{
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class HrEvaluationCriterionListInput : PagedAndSortedResultRequestDto
{
    public bool? IsActive { get; set; }
}

public class HrEvaluationScoreInputDto
{
    public Guid? CriterionId { get; set; }
    public int? Score { get; set; }
}

public class HrEvaluationCreateDto
{
    public Guid? EmployeeId { get; set; }
    public List<HrEvaluationScoreInputDto> Scores { get; set; } = null!;
    public string? Notes { get; set; }
}

public class HrEvaluationScoreDto
{
    public Guid CriterionId { get; set; }
    public string CriterionName { get; set; } = null!;
    public int Score { get; set; }
}

public class HrEvaluationDto
{
    public Guid Id { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public UserRefDto EvaluatedBy { get; set; } = null!;
    public DateTime EvaluatedAt { get; set; }
    public List<HrEvaluationScoreDto> Scores { get; set; } = new();
    public decimal AverageScore { get; set; }
    public string? Notes { get; set; }
    public Guid? ReportId { get; set; }
    public DateTime? SentAt { get; set; }
}

public class HrEvaluationPagedResultDto
{
    public long TotalCount { get; set; }
    public List<HrEvaluationDto> Items { get; set; } = new();
}

public class HrEvaluationListInput : PagedAndSortedResultRequestDto
{
    public Guid? EmployeeId { get; set; }
    public bool? IsSent { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
}

public class HrEvaluationReportCreateDto
{
    public string? Note { get; set; }
}

public class HrEvaluationReportSummaryDto
{
    public Guid Id { get; set; }
    public UserRefDto SentBy { get; set; } = null!;
    public DateTime SentAt { get; set; }
    public string? Note { get; set; }
    public int EvaluationsCount { get; set; }
    public DateTime FirstEvaluatedAt { get; set; }
    public DateTime LastEvaluatedAt { get; set; }
}

public class HrEvaluationReportDto : HrEvaluationReportSummaryDto
{
    public List<HrEvaluationDto> Evaluations { get; set; } = new();
}

public class HrEvaluationReportPagedResultDto
{
    public long TotalCount { get; set; }
    public List<HrEvaluationReportSummaryDto> Items { get; set; } = new();
}

public interface IHrEvaluationCriterionAppService : IApplicationService
{
    Task<HrEvaluationCriterionPagedResultDto> GetListAsync(HrEvaluationCriterionListInput input);
    Task<HrEvaluationCriterionDto> CreateAsync(HrEvaluationCriterionCreateDto input);
    Task<HrEvaluationCriterionDto> UpdateAsync(Guid id, HrEvaluationCriterionUpdateDto input);
}

public interface IHrEvaluationAppService : IApplicationService
{
    Task<HrEvaluationPagedResultDto> GetListAsync(HrEvaluationListInput input);
    Task<HrEvaluationDto> CreateAsync(HrEvaluationCreateDto input, Guid? idempotencyKey);
    Task<HrEvaluationDto> GetAsync(Guid id);
    Task<HrEvaluationReportPagedResultDto> GetReportListAsync(PagedAndSortedResultRequestDto input);
    Task<HrEvaluationReportDto> SendReportAsync(HrEvaluationReportCreateDto input, Guid? idempotencyKey);
    Task<HrEvaluationReportDto> SendOneAsync(Guid id, Guid? idempotencyKey);
    Task<HrEvaluationReportDto> GetReportAsync(Guid id);
}

/// <summary>Low average alert of HR evaluations (decision 2026-11-11).</summary>
public class HrEvaluationAlertDto
{
    public Guid Id { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public Guid CriterionId { get; set; }
    public string CriterionName { get; set; } = null!;
    public decimal AverageScore { get; set; }
    public int EvaluationsCount { get; set; }
    public decimal Threshold { get; set; }
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public DateTime CreationTime { get; set; }
}

public class HrEvaluationAlertPagedResultDto
{
    public long TotalCount { get; set; }
    public List<HrEvaluationAlertDto> Items { get; set; } = new();
}

public class HrEvaluationAlertListInput : PagedAndSortedResultRequestDto
{
    public Guid? EmployeeId { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
}

public interface IHrEvaluationAlertAppService : IApplicationService
{
    Task<HrEvaluationAlertPagedResultDto> GetListAsync(HrEvaluationAlertListInput input);
    Task<HrEvaluationAlertDto> GetAsync(Guid id);
}
