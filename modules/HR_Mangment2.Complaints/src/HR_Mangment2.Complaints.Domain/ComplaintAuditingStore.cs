using System;
using System.Threading.Tasks;
using HR_Mangment2.Auditing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.Auditing;
using Volo.Abp.AuditLogging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Uow;

namespace HR_Mangment2.Complaints;

/// <summary>
/// [DisableAuditing] removes the action, but a complaint request also changes other tables, such as notifications, and
/// that alone would still write a row showing that someone used a complaint. So these requests never reach the general
/// audit log at all; ComplaintAccessLogs replaces it. Handler assignments stay, because they hold no complaint data.
/// </summary>
[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IAuditingStore))]
public class ComplaintAuditingStore : HrAuditingStore
{
    private const string ComplaintPath = "/api/app/complaint";

    public ComplaintAuditingStore(
        IAuditLogRepository auditLogRepository,
        IUnitOfWorkManager unitOfWorkManager,
        IOptions<AbpAuditingOptions> options,
        IAuditLogInfoToAuditLogConverter converter,
        IRepository<HrAuditEntry, Guid> entries,
        IGuidGenerator guidGenerator,
        ILogger<HrAuditingStore> logger)
        : base(auditLogRepository, unitOfWorkManager, options, converter, entries, guidGenerator, logger)
    {
    }

    public override Task SaveAsync(AuditLogInfo auditInfo) =>
        IsComplaintRequest(auditInfo.Url) ? Task.CompletedTask : base.SaveAsync(auditInfo);

    public static bool IsComplaintRequest(string? url)
    {
        if (url == null)
        {
            return false;
        }

        var path = url.Split('?')[0].TrimEnd('/');
        return path.Equals(ComplaintPath, StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith(ComplaintPath + "/", StringComparison.OrdinalIgnoreCase);
    }
}
