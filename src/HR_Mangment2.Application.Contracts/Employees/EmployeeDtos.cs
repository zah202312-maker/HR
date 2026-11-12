using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HR_Mangment2.Departments;
using HR_Mangment2.Identity;
using HR_Mangment2.WorkShifts;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Auditing;

namespace HR_Mangment2.Employees;

public class EmployeeAccountSummaryDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public bool IsActive { get; set; }
    public bool MustChangePassword { get; set; }
    public bool MfaRequired { get; set; }
    public bool MfaEnabled { get; set; }
    public List<string> RoleNames { get; set; } = new();
}

public class EmployeeHireDateCorrectionDto
{
    public DateOnly? HireDate { get; set; }
    public string Reason { get; set; } = null!;
    public string ConcurrencyStamp { get; set; } = null!;
}

public class EmployeeContractSummaryDto
{
    public Guid ContractId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public class EmployeeShiftSummaryDto
{
    public Guid WorkShiftId { get; set; }
    public string Name { get; set; } = null!;
    public DateOnly EffectiveFrom { get; set; }
}

public class EmployeeCreateDto
{
    public string FullNameAr { get; set; } = null!;
    public string? FullNameEn { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public Guid? DepartmentId { get; set; }
    public string JobTitle { get; set; } = null!;
    public DateOnly? HireDate { get; set; }
    public Guid? WorkShiftId { get; set; }
    public Guid? ManagerEmployeeId { get; set; }
    public string UserName { get; set; } = null!;
    public List<string> RoleNames { get; set; } = new();
    public AppGender? Gender { get; set; }
    public string? EmployeeNumber { get; set; }

    /// <summary>The salary agreed at hiring, optional: it becomes the first salary version (decision 2026-11-10). Kept out of the audit log.</summary>
    [Volo.Abp.Auditing.DisableAuditing]
    public decimal? BaseSalary { get; set; }

    [Volo.Abp.Auditing.DisableAuditing]
    public string? SalaryCurrency { get; set; }
}

public class EmployeeUpdateDto
{
    public string FullNameAr { get; set; } = null!;
    public string? FullNameEn { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public Guid? DepartmentId { get; set; }
    public string JobTitle { get; set; } = null!;
    public DateOnly? HireDate { get; set; }
    public EmployeeStatus? Status { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
    public AppGender? Gender { get; set; }
    public string? EmployeeNumber { get; set; }
}

public class EmployeeHrDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public Guid? PhotoId { get; set; }
    public string FullNameAr { get; set; } = null!;
    public string? FullNameEn { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public DepartmentRefDto Department { get; set; } = null!;
    public string JobTitle { get; set; } = null!;
    public DateOnly HireDate { get; set; }
    public EmployeeStatus Status { get; set; }
    public EmployeeRefDto? Manager { get; set; }
    public EmployeeShiftSummaryDto? CurrentShift { get; set; }
    public EmployeeContractSummaryDto? CurrentContract { get; set; }
    public EmployeeAccountSummaryDto? Account { get; set; }
    public int DirectReportCount { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
    public AppGender? Gender { get; set; }
    public string? EmployeeNumber { get; set; }
}

/// <summary>The employee's own view. It has no account internals and no concurrency stamp, since it is read only.</summary>
public class EmployeeSelfDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public Guid? PhotoId { get; set; }
    public string FullNameAr { get; set; } = null!;
    public string? FullNameEn { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public DepartmentRefDto Department { get; set; } = null!;
    public string JobTitle { get; set; } = null!;
    public DateOnly HireDate { get; set; }
    public EmployeeStatus Status { get; set; }
    public EmployeeRefDto? Manager { get; set; }
    public EmployeeShiftSummaryDto? CurrentShift { get; set; }
    public EmployeeContractSummaryDto? CurrentContract { get; set; }
    public List<string> RoleNames { get; set; } = new();
    public AppGender? Gender { get; set; }
    public string? EmployeeNumber { get; set; }
}

/// <summary>The direct manager's view. Financial, complaint and contract data are absent by design.</summary>
public class EmployeeTeamMemberDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public Guid? PhotoId { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DepartmentRefDto Department { get; set; } = null!;
    public string JobTitle { get; set; } = null!;
    public DateOnly HireDate { get; set; }
    public EmployeeStatus Status { get; set; }
    public EmployeeShiftSummaryDto? CurrentShift { get; set; }
}

public class EmployeeTeamMemberPagedResultDto
{
    public long TotalCount { get; set; }
    public List<EmployeeTeamMemberDto> Items { get; set; } = new();
}

public class EmployeeListItemDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public Guid? PhotoId { get; set; }
    public DepartmentRefDto Department { get; set; } = null!;
    public string JobTitle { get; set; } = null!;
    public DateOnly HireDate { get; set; }
    public EmployeeStatus Status { get; set; }
    public EmployeeRefDto? Manager { get; set; }
    public List<string> RoleNames { get; set; } = new();
    public bool HasAccount { get; set; }
}

public class EmployeeListItemPagedResultDto
{
    public long TotalCount { get; set; }
    public List<EmployeeListItemDto> Items { get; set; } = new();
}

public class EmployeeLookupPagedResultDto
{
    public long TotalCount { get; set; }
    public List<EmployeeRefDto> Items { get; set; } = new();
}

public class TemporaryCredentialsDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;

    [DisableAuditing]
    public string TemporaryPassword { get; set; } = null!;

    public bool MustChangePassword { get; set; }
}

public class EmployeeCreatedDto
{
    public EmployeeHrDto Employee { get; set; } = null!;
    public TemporaryCredentialsDto Credentials { get; set; } = null!;
}

public class EmployeeManagerHistoryDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public EmployeeRefDto Manager { get; set; } = null!;
    public DateOnly FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public UserRefDto? ChangedBy { get; set; }
    public string? Reason { get; set; }
}

public class EmployeeManagerHistoryPagedResultDto
{
    public long TotalCount { get; set; }
    public List<EmployeeManagerHistoryDto> Items { get; set; } = new();
}

public class ManagerChangeInputDto
{
    public Guid? ManagerEmployeeId { get; set; }
    public string? Reason { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class ManagerChangeResultDto
{
    public EmployeeRefDto Employee { get; set; } = null!;
    public EmployeeRefDto? PreviousManager { get; set; }
    public EmployeeRefDto NewManager { get; set; } = null!;
    public DateOnly EffectiveFrom { get; set; }
    public int MovedPendingItemCount { get; set; }
    public string ConcurrencyStamp { get; set; } = null!;
}

public class ManagerAssignEmployeesInputDto
{
    public List<Guid> EmployeeIds { get; set; } = new();
    public string? Reason { get; set; }
}

public class ManagerAssignEmployeesResultDto
{
    public EmployeeRefDto Manager { get; set; } = null!;
    public int AssignedCount { get; set; }
    public int AlreadyAssignedCount { get; set; }
    public int MovedPendingItemCount { get; set; }
    public List<ManagerChangeResultDto> Items { get; set; } = new();
}

public class AdministrativeTaskLogEntryDto
{
    public DateTime OccurredAt { get; set; }
    public AdministrativeTaskType TaskType { get; set; }
    public string Title { get; set; } = null!;
    public Guid? ReferenceId { get; set; }
}

public class AdministrativeTaskLogPagedResultDto
{
    public long TotalCount { get; set; }
    public List<AdministrativeTaskLogEntryDto> Items { get; set; } = new();
}

public class EmployeeShiftAssignDto
{
    public Guid? ShiftId { get; set; }
    public DateOnly? StartDate { get; set; }
}

public class EmployeeShiftDto
{
    public Guid Id { get; set; }
    public EmployeeRefDto Employee { get; set; } = null!;
    public WorkShiftRefDto Shift { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateTime CreationTime { get; set; }
    public UserRefDto? CreatedBy { get; set; }
}

public class EmployeeShiftPagedResultDto
{
    public long TotalCount { get; set; }
    public List<EmployeeShiftDto> Items { get; set; } = new();
}

public class EmployeeDocumentDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public EmployeeDocumentType Type { get; set; }
    public string Title { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long SizeBytes { get; set; }
    public DocumentSensitivity Sensitivity { get; set; }
    public DocumentScanStatus ScanStatus { get; set; }
    public UserRefDto? UploadedBy { get; set; }
    public DateTime CreationTime { get; set; }
}

public class EmployeeDocumentPagedResultDto
{
    public long TotalCount { get; set; }
    public List<EmployeeDocumentDto> Items { get; set; } = new();
}

public class FileDownloadUrlDto
{
    public string Url { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}

/// <summary>The upload as the service sees it. The controller builds it from the multipart form.</summary>
public class EmployeeDocumentUploadInput
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public EmployeeDocumentType Type { get; set; }
    public string Title { get; set; } = null!;
    public DocumentSensitivity Sensitivity { get; set; }
}

public record DownloadableFile(Stream Content, string FileName, string ContentType);

public class EmployeeListInput : PagedAndSortedResultRequestDto
{
    public string? Search { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? ManagerEmployeeId { get; set; }
    public EmployeeStatus? Status { get; set; }
    public string? RoleName { get; set; }
}

public class EmployeeLookupInput : PagedAndSortedResultRequestDto
{
    public string? Search { get; set; }
    public Guid? DepartmentId { get; set; }
}

public class MyTeamInput : PagedAndSortedResultRequestDto
{
    public string? Search { get; set; }
    public EmployeeStatus? Status { get; set; }
}

public class AdministrativeTaskLogInput : PagedAndSortedResultRequestDto
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public AdministrativeTaskType? TaskType { get; set; }
}

public class EmployeeDocumentListInput : PagedAndSortedResultRequestDto
{
    public EmployeeDocumentType? Type { get; set; }
}

public interface IEmployeeAppService : IApplicationService
{
    Task<EmployeeListItemPagedResultDto> GetListAsync(EmployeeListInput input);
    Task<EmployeeCreatedDto> CreateAsync(EmployeeCreateDto input);
    Task<EmployeeLookupPagedResultDto> LookupAsync(EmployeeLookupInput input);
    Task<EmployeeHrDto> GetAsync(Guid id);
    Task<EmployeeHrDto> UpdateAsync(Guid id, EmployeeUpdateDto input);
    Task<EmployeeHrDto> CorrectHireDateAsync(Guid id, EmployeeHireDateCorrectionDto input);
    Task<ManagerChangeResultDto> ChangeManagerAsync(Guid id, ManagerChangeInputDto input);
    Task<EmployeeManagerHistoryPagedResultDto> GetManagerHistoryAsync(Guid id, PagedAndSortedResultRequestDto input);
    Task<AdministrativeTaskLogPagedResultDto> GetAdministrativeTaskLogAsync(Guid id, AdministrativeTaskLogInput input);
    Task<ManagerAssignEmployeesResultDto> AssignEmployeesAsync(Guid managerEmployeeId, ManagerAssignEmployeesInputDto input);
    Task<EmployeeTeamMemberPagedResultDto> GetMyTeamAsync(MyTeamInput input);
    Task<EmployeeTeamMemberDto> GetTeamMemberAsync(Guid id);
    Task<EmployeeSelfDto> GetMyProfileAsync();
    Task<AdministrativeTaskLogPagedResultDto> GetMyAdministrativeTaskLogAsync(AdministrativeTaskLogInput input);
}

public interface IEmployeeShiftAppService : IApplicationService
{
    Task<EmployeeShiftDto> AssignAsync(Guid employeeId, EmployeeShiftAssignDto input);
    Task<EmployeeShiftPagedResultDto> GetHistoryAsync(Guid employeeId, PagedAndSortedResultRequestDto input);
}

public interface IEmployeeDocumentAppService : IApplicationService
{
    Task<EmployeeDocumentPagedResultDto> GetListAsync(Guid employeeId, EmployeeDocumentListInput input);
    Task<EmployeeDocumentDto> UploadAsync(Guid employeeId, EmployeeDocumentUploadInput input);
    Task<FileDownloadUrlDto> GetDownloadUrlAsync(Guid id);
    Task DeleteAsync(Guid id);
}

/// <summary>Serves a file for a download link. The link itself is the credential, so it takes no signed-in user.</summary>
public interface IFileDownloadAppService : IApplicationService
{
    Task<DownloadableFile?> OpenAsync(string token);
}
