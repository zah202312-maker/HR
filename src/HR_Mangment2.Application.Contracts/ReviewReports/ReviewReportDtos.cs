using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR_Mangment2.Departments;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.ReviewerEvaluations;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.ReviewReports;

public class ReviewRoomRefDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

public class ReviewReportTeamDto
{
    public ReviewRoomRefDto Room { get; set; } = null!;
    public EmployeeRefDto Manager { get; set; } = null!;
    public DepartmentRefDto? Department { get; set; }
    public List<EmployeeRefDto> Members { get; set; } = new();
    public ReviewerGrade? BackendGrade { get; set; }
    public string? BackendNotes { get; set; }
    public ReviewerGrade? FrontendGrade { get; set; }
    public string? FrontendNotes { get; set; }
    public string? Notes { get; set; }
}

public class ReviewReportDto
{
    public Guid Id { get; set; }
    public UserRefDto Reviewer { get; set; } = null!;

    /// <summary>Who created the report: the reviewer or their stand-in (decision 2026-11-11).</summary>
    public UserRefDto PreparedBy { get; set; } = null!;
    public DateOnly ReportDate { get; set; }
    public ReviewReportStatus Status { get; set; }
    public string? Content { get; set; }
    public List<ReviewReportTeamDto> Teams { get; set; } = new();
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ViewedAt { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class ReviewReportSummaryDto
{
    public Guid Id { get; set; }
    public UserRefDto Reviewer { get; set; } = null!;
    public UserRefDto PreparedBy { get; set; } = null!;
    public DateOnly ReportDate { get; set; }
    public ReviewReportStatus Status { get; set; }
    public int TeamsCount { get; set; }
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
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public ReviewReportStatus? Status { get; set; }
}

public class ReviewReportCreateDto
{
    /// <summary>The reviewer a stand-in writes for, under an active reviewReports delegation. Empty for the reviewer.</summary>
    public Guid? OnBehalfOfUserId { get; set; }

    public DateOnly? ReportDate { get; set; }
    public string? Content { get; set; }
}

public class ReviewReportTeamInputDto
{
    public Guid? RoomId { get; set; }
    public ReviewerGrade? BackendGrade { get; set; }
    public string? BackendNotes { get; set; }
    public ReviewerGrade? FrontendGrade { get; set; }
    public string? FrontendNotes { get; set; }
    public string? Notes { get; set; }
}

public class ReviewReportUpdateDto
{
    public DateOnly? ReportDate { get; set; }
    public string? Content { get; set; }
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

public class ReviewRoomDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public UserRefDto Reviewer { get; set; } = null!;
    public EmployeeRefDto Manager { get; set; } = null!;
    public DepartmentRefDto? Department { get; set; }
    public List<EmployeeRefDto> Members { get; set; } = new();
    public DateTime CreationTime { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class ReviewRoomPagedResultDto
{
    public long TotalCount { get; set; }
    public List<ReviewRoomDto> Items { get; set; } = new();
}

public class ReviewRoomListInput : PagedResultRequestDto
{
}

public class ReviewRoomCreateDto
{
    /// <summary>The reviewer a stand-in works for, under an active reviewReports delegation. Empty for the reviewer.</summary>
    public Guid? OnBehalfOfUserId { get; set; }

    public string Name { get; set; } = null!;
    public Guid? ManagerEmployeeId { get; set; }
    public List<Guid>? MemberEmployeeIds { get; set; }
}

public class ReviewRoomUpdateDto
{
    public string Name { get; set; } = null!;
    public Guid? ManagerEmployeeId { get; set; }
    public List<Guid>? MemberEmployeeIds { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class ReviewRoomCandidateDto
{
    public EmployeeRefDto Manager { get; set; } = null!;
    public DepartmentRefDto? Department { get; set; }
    public List<EmployeeRefDto> Members { get; set; } = new();
}

public interface IReviewRoomAppService : IApplicationService
{
    Task<List<ReviewRoomCandidateDto>> GetCandidatesAsync();
    Task<ReviewRoomPagedResultDto> GetListAsync(ReviewRoomListInput input);
    Task<ReviewRoomDto> CreateAsync(ReviewRoomCreateDto input);
    Task<ReviewRoomDto> GetAsync(Guid id);
    Task<ReviewRoomDto> UpdateAsync(Guid id, ReviewRoomUpdateDto input);
    Task DeleteAsync(Guid id);
}
