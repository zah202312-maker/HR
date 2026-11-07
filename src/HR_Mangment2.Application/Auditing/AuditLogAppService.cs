using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR_Mangment2.Employees;
using HR_Mangment2.Identity;
using HR_Mangment2.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;

namespace HR_Mangment2.Auditing;

/// <summary>Read only. The contract has no route to change or remove a row, and neither does this service.</summary>
[Authorize(HrPermissions.AuditLogPermissions.Read)]
public class AuditLogAppService : HR_Mangment2AppService, IAuditLogAppService
{
    private static readonly Dictionary<string, string> SortFields = new()
    {
        ["occurredAt"] = nameof(HrAuditEntry.OccurredAt),
        ["action"] = nameof(HrAuditEntry.Action),
        ["entityType"] = nameof(HrAuditEntry.EntityType),
        ["succeeded"] = nameof(HrAuditEntry.Succeeded)
    };

    private readonly IRepository<HrAuditEntry, Guid> _entries;
    private readonly EmployeeReferenceReader _references;

    public AuditLogAppService(IRepository<HrAuditEntry, Guid> entries, EmployeeReferenceReader references)
    {
        _entries = entries;
        _references = references;
    }

    public virtual async Task<AuditLogEntryPagedResultDto> GetListAsync(AuditLogListInput input)
    {
        var query = await _entries.GetQueryableAsync();

        query = query
            .Where(x => input.From == null || x.OccurredAt >= input.From)
            .Where(x => input.To == null || x.OccurredAt <= input.To)
            .Where(x => input.UserId == null || x.UserId == input.UserId)
            .Where(x => input.Action == null || x.Action == input.Action)
            .Where(x => input.EntityType == null || x.EntityType == input.EntityType)
            .Where(x => input.EntityId == null || x.EntityId == input.EntityId)
            .Where(x => input.Succeeded == null || x.Succeeded == input.Succeeded);

        var totalCount = await AsyncExecuter.LongCountAsync(query);
        var page = await AsyncExecuter.ToListAsync(query
            .ApplySorting(input.Sorting, nameof(HrAuditEntry.OccurredAt) + " desc", SortFields)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount));

        var users = await _references.GetUserRefsAsync(page.Where(x => x.UserId != null).Select(x => x.UserId!.Value).Distinct());

        return new AuditLogEntryPagedResultDto
        {
            TotalCount = totalCount,
            Items = page.Select(x => new AuditLogEntryDto
            {
                Id = x.Id,
                OccurredAt = x.OccurredAt,
                User = x.UserId != null && users.TryGetValue(x.UserId.Value, out var user) ? user : null,
                Action = x.Action,
                EntityType = x.EntityType,
                EntityId = x.EntityId,
                ChangedFields = x.ChangedFields.ToList(),
                IpAddress = x.IpAddress,
                Succeeded = x.Succeeded
            }).ToList()
        };
    }
}
