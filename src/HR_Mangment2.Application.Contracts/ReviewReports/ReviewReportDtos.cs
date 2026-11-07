using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR_Mangment2.Departments;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.ReviewReports;

public class ReviewReportTeamDto
{
    public EmployeeRefDto Manager { get; set; } = null!;
    public DepartmentRefDto? Department { get; set; }
    public int MembersCount { get; set; }
    public int? CompletionPercent { get; set; }
    public string? Notes { get; set; }
    public string? ProjectNotes { get; set; }
}

public class ReviewReportDto
{
    public Guid Id { get; set; }
    public UserRefDto Reviewer { get; set; } = null!;
    public ReviewReportPeriodType PeriodType { get; set; }
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public ReviewReportStatus Status { get; set; }
    public string? Summary { get; set; }
    public int? AverageCompletion { get; set; }
    public List<ReviewReportTeamDto> Teams { get; set; } = new();
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ViewedAt { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class ReviewReportSummaryDto
{
    public Guid Id { get; set; }
    public UserRefDto Reviewer { get; set; } = null!;
    public ReviewReportPeriodType PeriodType { get; set; }
    public DateOnly PeriodFrom { get; set; }
    public DateOnly PeriodTo { get; set; }
    public ReviewReportStatus Status { get; set; }
    public int TeamsCount { get; set; }
    public int? AverageCompletion { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ViewedAt { get; set; }
}

public class ReviewReportSummaryPagedResultDto
{
    public long TotalCount { get; set; }
    public List<ReviewReportSummaryDto> Items { get; set; } = new();
}

public class ReviewReportListInput : PagedAndSortedResultRequestDto
{
    public ReviewReportPeriodType? PeriodType { get; set; }
    public ReviewReportStatus? Status { get; set; }
}

public class ReviewReportCreateDto
{
    public ReviewReportPeriodType? PeriodType { get; set; }
    public DateOnly? PeriodFrom { get; set; }
    public DateOnly? PeriodTo { get; set; }
    public string? Summary { get; set; }
}

public class ReviewReportTeamInputDto
{
    public Guid? ManagerEmployeeId { get; set; }
    public int? CompletionPercent { get; set; }
    public string? Notes { get; set; }
    public string? ProjectNotes { get; set; }
}

public class ReviewReportUpdateDto
{
    public ReviewReportPeriodType? PeriodType { get; set; }
    public DateOnly? PeriodFrom { get; set; }
    public DateOnly? PeriodTo { get; set; }
    public string? Summary { get; set; }
    public List<ReviewReportTeamInputDto>? Teams { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class ReviewReportSubmitDto
{
    public string ConcurrencyStamp { get; set; } = null!;
}

public interface IReviewReportAppService : IApplicationService
{
    Task<ReviewReportSummaryPagedResultDto> GetListAsync(ReviewReportListInput input);
    Task<ReviewReportDto> CreateAsync(ReviewReportCreateDto input);
    Task<ReviewReportDto> GetAsync(Guid id);
    Task<ReviewReportDto> UpdateAsync(Guid id, ReviewReportUpdateDto input);
    Task DeleteAsync(Guid id);
    Task<ReviewReportDto> SubmitAsync(Guid id, ReviewReportSubmitDto input);
}
