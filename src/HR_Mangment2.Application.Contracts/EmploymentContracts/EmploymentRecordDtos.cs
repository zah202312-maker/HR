using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Leaves;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.EmploymentContracts
{
    /// <summary>The service form of the multipart ContractCreateInputDto: the files already read.</summary>
    public class ContractCreateInput
    {
        public Guid? EmployeeId { get; set; }
        public string ContractType { get; set; } = null!;
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public Guid? PreviousContractId { get; set; }
        public List<HR_Mangment2.Warnings.UploadedFileInput> Attachments { get; set; } = new();
    }

    public class ContractRecommendInputDto
    {
        public string EmployeePosition { get; set; } = null!;
        public ContractRenewalOption? Recommendation { get; set; }
        public string? Note { get; set; }
    }

    public class ContractDecideInputDto
    {
        public ContractRenewalOption? Decision { get; set; }
        public string? Note { get; set; }
        public string? EmployeeMessage { get; set; }
    }

    public class ContractRecommendationDto
    {
        public string EmployeePosition { get; set; } = null!;
        public ContractRenewalOption Recommendation { get; set; }
        public string? Note { get; set; }
        public UserRefDto RecommendedBy { get; set; } = null!;
        public DateTime RecommendedAt { get; set; }
    }

    public class ContractDecisionDto
    {
        public ContractRenewalOption Decision { get; set; }
        public string? Note { get; set; }
        public string? EmployeeMessage { get; set; }
        public UserRefDto DecidedBy { get; set; } = null!;
        public DateTime DecidedAt { get; set; }
    }

    public class ContractDto
    {
        public Guid Id { get; set; }
        public EmployeeRefDto Employee { get; set; } = null!;
        public string ContractType { get; set; } = null!;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public Guid? PreviousContractId { get; set; }
        public ContractStatus Status { get; set; }
        public int DaysRemaining { get; set; }
        public bool IsExpired { get; set; }
        public ContractRecommendationDto? Recommendation { get; set; }
        public ContractDecisionDto? Decision { get; set; }
        public DateOnly? DecisionDueDate { get; set; }
        public bool IsDecisionOverdue { get; set; }
        public DateTime? EmployeeNotifiedAt { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public List<AttachmentDto> Attachments { get; set; } = new();
    }

    /// <summary>The employee's view. The position and the recommendation are absent by design.</summary>
    public class ContractEmployeeDto
    {
        public Guid Id { get; set; }
        public string ContractType { get; set; } = null!;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public ContractEmployeeStatus Status { get; set; }
        public bool IsExpired { get; set; }
        public string? EmployeeMessage { get; set; }
        public DateTime? DecidedAt { get; set; }
        public List<AttachmentDto> Attachments { get; set; } = new();
    }

    public class ContractPagedResultDto
    {
        public long TotalCount { get; set; }
        public List<ContractDto> Items { get; set; } = new();
    }

    public class ContractExpiringPagedResultDto
    {
        public long TotalCount { get; set; }
        public List<ContractDto> Items { get; set; } = new();
        public int AlertDays { get; set; }
    }

    public class ContractEmployeePagedResultDto
    {
        public long TotalCount { get; set; }
        public List<ContractEmployeeDto> Items { get; set; } = new();
    }

    public class ContractListInput : PagedAndSortedResultRequestDto
    {
        public Guid? EmployeeId { get; set; }
        public Guid? DepartmentId { get; set; }
        public ContractStatus? Status { get; set; }
        public DateOnly? EndDateFrom { get; set; }
        public DateOnly? EndDateTo { get; set; }
    }

    /// <summary>Search and paging of Contract_GetOverview (decision 2026-11-11).</summary>
    public class ContractOverviewListInput : PagedResultRequestDto
    {
        [System.ComponentModel.DataAnnotations.StringLength(100)]
        public string? Search { get; set; }
    }

    /// <summary>One line of the executive director's employee contracts page: the current contract's details only.</summary>
    public class EmployeeContractOverviewDto
    {
        public EmployeeRefDto Employee { get; set; } = null!;
        public string? ContractType { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public ContractStatus? Status { get; set; }
    }

    public class EmployeeContractOverviewPagedResultDto
    {
        public long TotalCount { get; set; }
        public List<EmployeeContractOverviewDto> Items { get; set; } = new();
    }

    public class ContractExpiringListInput : PagedAndSortedResultRequestDto
    {
        public int? WithinDays { get; set; }
        public Guid? DepartmentId { get; set; }
    }

    public interface IContractAppService : IApplicationService
    {
        Task<ContractPagedResultDto> GetListAsync(ContractListInput input);
        Task<ContractDto> CreateAsync(ContractCreateInput input, Guid? idempotencyKey);
        Task<ContractExpiringPagedResultDto> GetExpiringAsync(ContractExpiringListInput input);
        Task<EmployeeContractOverviewPagedResultDto> GetOverviewAsync(ContractOverviewListInput input);
        Task<ContractDto> GetAsync(Guid id);
        Task<ContractDto> RecommendAsync(Guid id, ContractRecommendInputDto input);
        Task<ContractDto> DecideAsync(Guid id, ContractDecideInputDto input);
        Task<ContractEmployeePagedResultDto> GetMyListAsync(PagedAndSortedResultRequestDto input);
        Task<ContractEmployeeDto> GetMyAsync(Guid id);
        Task<FileDownloadUrlDto> GetAttachmentDownloadUrlAsync(Guid id, Guid attachmentId);
        Task<FileDownloadUrlDto> GetMyAttachmentDownloadUrlAsync(Guid id, Guid attachmentId);
        Task DeleteAttachmentAsync(Guid id, Guid attachmentId);
    }
}

namespace HR_Mangment2.Warnings
{
    public class UploadedFileInput
    {
        public byte[] Content { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
    }

    public class WarningCreateInput
    {
        public Guid? EmployeeId { get; set; }
        public WarningLevel? RecommendedLevel { get; set; }
        public DateOnly? IncidentDate { get; set; }
        public string Description { get; set; } = null!;
        public List<UploadedFileInput> Attachments { get; set; } = new();
    }

    public class WarningRespondInputDto
    {
        public string Content { get; set; } = null!;
    }

    public class WarningObjectInputDto
    {
        public string Content { get; set; } = null!;
    }

    public class WarningDecideInputDto
    {
        public WarningDecisionOutcome? Decision { get; set; }
        public WarningLevel? IssuedLevel { get; set; }
        public string Note { get; set; } = null!;
    }

    public class WarningResolveObjectionInputDto
    {
        public WarningObjectionOutcome? Outcome { get; set; }
        public string Note { get; set; } = null!;
    }

    public class WarningResponseDto
    {
        public string Content { get; set; } = null!;
        public DateTime SubmittedAt { get; set; }
    }

    public class WarningDecisionDto
    {
        public WarningDecisionOutcome Outcome { get; set; }
        public WarningLevel? IssuedLevel { get; set; }
        public string Note { get; set; } = null!;
        public UserRefDto DecidedBy { get; set; } = null!;
        public DateTime DecidedAt { get; set; }
    }

    public class WarningObjectionDto
    {
        public string Content { get; set; } = null!;
        public DateTime SubmittedAt { get; set; }
        public WarningObjectionOutcome? Outcome { get; set; }
        public string? ResolutionNote { get; set; }
        public UserRefDto? ResolvedBy { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    public class WarningEmployeeObjectionDto
    {
        public string Content { get; set; } = null!;
        public DateTime SubmittedAt { get; set; }
        public WarningObjectionOutcome? Outcome { get; set; }
        public string? ResolutionNote { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    public class WarningDto
    {
        public Guid Id { get; set; }
        public EmployeeRefDto Employee { get; set; } = null!;
        public WarningLevel RecommendedLevel { get; set; }
        public DateOnly IncidentDate { get; set; }
        public string Description { get; set; } = null!;
        public List<AttachmentDto> Attachments { get; set; } = new();
        public UserRefDto RecommendedBy { get; set; } = null!;
        public WarningRecommenderRole RecommenderRole { get; set; }
        public WarningStatus Status { get; set; }
        public DateTime? NotifiedAt { get; set; }
        public DateOnly? ResponseDueDate { get; set; }
        public WarningResponseDto? EmployeeResponse { get; set; }
        public WarningDecisionDto? Decision { get; set; }
        public WarningObjectionDto? Objection { get; set; }
        public DateTime CreationTime { get; set; }
    }

    /// <summary>The employee's view. Who recommended is absent by design.</summary>
    public class WarningEmployeeDto
    {
        public Guid Id { get; set; }
        public WarningLevel Level { get; set; }
        public DateOnly IncidentDate { get; set; }
        public string Description { get; set; } = null!;
        public WarningStatus Status { get; set; }
        public DateTime NotifiedAt { get; set; }
        public DateOnly ResponseDueDate { get; set; }
        public bool CanRespond { get; set; }
        public bool CanObject { get; set; }
        public WarningResponseDto? MyResponse { get; set; }
        public string? DecisionNote { get; set; }
        public DateTime? DecidedAt { get; set; }
        public WarningEmployeeObjectionDto? MyObjection { get; set; }
    }

    /// <summary>The recommender's view. The reply, the objection and the decision notes are absent by design.</summary>
    public class WarningRecommendationDto
    {
        public Guid Id { get; set; }
        public EmployeeRefDto Employee { get; set; } = null!;
        public WarningLevel RecommendedLevel { get; set; }
        public DateOnly IncidentDate { get; set; }
        public string Description { get; set; } = null!;
        public WarningRecommendationStage Stage { get; set; }
        public WarningLevel? IssuedLevel { get; set; }
        public DateTime? DecidedAt { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class WarningFileEntryDto
    {
        public Guid Id { get; set; }
        public WarningLevel Level { get; set; }
        public DateOnly IncidentDate { get; set; }
        public string Description { get; set; } = null!;
        public WarningStatus Status { get; set; }
        public string DecisionNote { get; set; } = null!;
        public DateTime DecidedAt { get; set; }
        public WarningResponseDto? EmployeeResponse { get; set; }
        public WarningObjectionOutcome? ObjectionOutcome { get; set; }
    }

    public class WarningPagedResultDto
    {
        public long TotalCount { get; set; }
        public List<WarningDto> Items { get; set; } = new();
    }

    public class WarningEmployeePagedResultDto
    {
        public long TotalCount { get; set; }
        public List<WarningEmployeeDto> Items { get; set; } = new();
    }

    public class WarningRecommendationPagedResultDto
    {
        public long TotalCount { get; set; }
        public List<WarningRecommendationDto> Items { get; set; } = new();
    }

    public class WarningFileEntryPagedResultDto
    {
        public long TotalCount { get; set; }
        public List<WarningFileEntryDto> Items { get; set; } = new();
    }

    public class WarningListInput : PagedAndSortedResultRequestDto
    {
        public Guid? EmployeeId { get; set; }
        public Guid? DepartmentId { get; set; }
        public WarningStatus? Status { get; set; }
        public WarningLevel? Level { get; set; }
        public DateOnly? IncidentDateFrom { get; set; }
        public DateOnly? IncidentDateTo { get; set; }
    }

    public class WarningObjectionListInput : PagedAndSortedResultRequestDto
    {
        public bool? IsPending { get; set; }
    }

    public class WarningRecommendedListInput : PagedAndSortedResultRequestDto
    {
        public WarningRecommendationStage? Stage { get; set; }
    }

    public interface IWarningAppService : IApplicationService
    {
        Task<WarningRecommendationDto> CreateAsync(WarningCreateInput input, Guid? idempotencyKey);
        Task<WarningPagedResultDto> GetListAsync(WarningListInput input);
        Task<WarningPagedResultDto> GetObjectionsAsync(WarningObjectionListInput input);
        Task<WarningRecommendationPagedResultDto> GetRecommendedByMeAsync(WarningRecommendedListInput input);
        Task<WarningEmployeePagedResultDto> GetMyListAsync(PagedAndSortedResultRequestDto input);
        Task<WarningEmployeeDto> GetMyAsync(Guid id);
        Task<WarningFileEntryPagedResultDto> GetEmployeeFileAsync(Guid employeeId, PagedAndSortedResultRequestDto input);
        Task<WarningDto> GetAsync(Guid id);
        Task<FileDownloadUrlDto> GetAttachmentDownloadUrlAsync(Guid id, Guid attachmentId);
        Task<WarningDto> NotifyAsync(Guid id);
        Task<WarningEmployeeDto> RespondAsync(Guid id, WarningRespondInputDto input);
        Task<WarningDto> DecideAsync(Guid id, WarningDecideInputDto input);
        Task<WarningEmployeeDto> ObjectAsync(Guid id, WarningObjectInputDto input);
        Task<WarningDto> ResolveObjectionAsync(Guid id, WarningResolveObjectionInputDto input);
    }
}

namespace HR_Mangment2.Appreciation
{
    public class AppreciationLetterCreateInput
    {
        public Guid? EmployeeId { get; set; }
        public string Title { get; set; } = null!;
        public string? Note { get; set; }
        public HR_Mangment2.Warnings.UploadedFileInput File { get; set; } = null!;
    }

    public class AppreciationLetterDto
    {
        public Guid Id { get; set; }
        public EmployeeRefDto Employee { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Note { get; set; }
        public UserRefDto IssuedBy { get; set; } = null!;
        public DateTime IssuedAt { get; set; }
        public AttachmentDto File { get; set; } = null!;
        public bool IsAcknowledged { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
    }

    public class AppreciationLetterPagedResultDto
    {
        public long TotalCount { get; set; }
        public List<AppreciationLetterDto> Items { get; set; } = new();
    }

    public class AppreciationLetterListInput : PagedAndSortedResultRequestDto
    {
        public Guid? EmployeeId { get; set; }
        public bool? IsAcknowledged { get; set; }
    }

    public interface IAppreciationLetterAppService : IApplicationService
    {
        Task<AppreciationLetterDto> CreateAsync(AppreciationLetterCreateInput input, Guid? idempotencyKey);
        Task<AppreciationLetterPagedResultDto> GetListAsync(AppreciationLetterListInput input);
        Task<AppreciationLetterPagedResultDto> GetMyListAsync(PagedAndSortedResultRequestDto input);
        Task<AppreciationLetterPagedResultDto> GetEmployeeFileAsync(Guid employeeId, PagedAndSortedResultRequestDto input);
        Task<AppreciationLetterDto> GetAsync(Guid id);
        Task<FileDownloadUrlDto> GetDownloadUrlAsync(Guid id);
        Task<AppreciationLetterDto> AcknowledgeAsync(Guid id);
    }
}
