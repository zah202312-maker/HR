using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR_Mangment2.Identity;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace HR_Mangment2.Auditing;

public class AuditLogEntryDto
{
    public Guid Id { get; set; }
    public DateTime OccurredAt { get; set; }
    public UserRefDto? User { get; set; }
    public AuditActionType Action { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public List<string> ChangedFields { get; set; } = new();
    public string? IpAddress { get; set; }
    public bool Succeeded { get; set; }
}

public class AuditLogEntryPagedResultDto
{
    public long TotalCount { get; set; }
    public List<AuditLogEntryDto> Items { get; set; } = new();
}

public class AuditLogListInput : PagedAndSortedResultRequestDto
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public Guid? UserId { get; set; }
    public AuditActionType? Action { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public bool? Succeeded { get; set; }
}

public interface IAuditLogAppService : IApplicationService
{
    Task<AuditLogEntryPagedResultDto> GetListAsync(AuditLogListInput input);
}
