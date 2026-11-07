using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR_Mangment2.Identity;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.Delegations;

public class DelegationCreateDto
{
    public Guid? FromUserId { get; set; }

    public Guid? ToUserId { get; set; }
    public DelegationScope? Scope { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public string Reason { get; set; } = null!;
}

public class DelegationDto
{
    public Guid Id { get; set; }
    public UserRefDto FromUser { get; set; } = null!;
    public UserRefDto ToUser { get; set; } = null!;
    public DelegationScope Scope { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string Reason { get; set; } = null!;
    public DelegationStatus Status { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreationTime { get; set; }
}

public class DelegationPagedResultDto
{
    public long TotalCount { get; set; }
    public List<DelegationDto> Items { get; set; } = new();
}

public class DelegationCandidatePagedResultDto
{
    public long TotalCount { get; set; }
    public List<UserRefDto> Items { get; set; } = new();
}

public class DelegationListInput : PagedAndSortedResultRequestDto
{
    public Guid? FromUserId { get; set; }
    public Guid? ToUserId { get; set; }
    public DelegationStatus? Status { get; set; }
}

public class MyDelegationListInput : PagedAndSortedResultRequestDto
{
    public DelegationDirection? Direction { get; set; }
    public DelegationStatus? Status { get; set; }
}

public class DelegationCandidateInput : PagedAndSortedResultRequestDto
{
    public string? Search { get; set; }
}

public interface IDelegationAppService : IApplicationService
{
    Task<DelegationPagedResultDto> GetListAsync(DelegationListInput input);
    Task<DelegationDto> CreateAsync(DelegationCreateDto input);
    Task<DelegationPagedResultDto> GetMyAsync(MyDelegationListInput input);
    Task<DelegationCandidatePagedResultDto> GetCandidatesAsync(DelegationCandidateInput input);
    Task CancelAsync(Guid id);
}
