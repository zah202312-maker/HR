using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.Auditing;
using Volo.Abp.AuditLogging;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Uow;

namespace HR_Mangment2.Auditing;

/// <summary>Keeps ABP's own log and adds the rows of the audit log screen. A failure here never fails the request.</summary>
public class HrAuditingStore : AuditingStore
{
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private const string FinanceNamespace = "HR_Mangment2.Finance.";
    private const string FinancePath = "/api/app/finance";

    private readonly IRepository<HrAuditEntry, Guid> _entries;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<HrAuditingStore> _logger;

    public HrAuditingStore(
        IAuditLogRepository auditLogRepository,
        IUnitOfWorkManager unitOfWorkManager,
        IOptions<AbpAuditingOptions> options,
        IAuditLogInfoToAuditLogConverter converter,
        IRepository<HrAuditEntry, Guid> entries,
        IGuidGenerator guidGenerator,
        ILogger<HrAuditingStore> logger)
        : base(auditLogRepository, unitOfWorkManager, options, converter)
    {
        _unitOfWorkManager = unitOfWorkManager;
        _entries = entries;
        _guidGenerator = guidGenerator;
        _logger = logger;
    }

    public override async Task SaveAsync(AuditLogInfo auditInfo)
    {
        var drafts = HrAuditEntryBuilder.Build(auditInfo);
        RemoveFinancialValues(auditInfo);
        await base.SaveAsync(auditInfo);

        if (drafts.Count == 0)
        {
            return;
        }

        try
        {
            using var uow = _unitOfWorkManager.Begin(requiresNew: true);
            await _entries.InsertManyAsync(drafts.Select(x => new HrAuditEntry(
                _guidGenerator.Create(),
                auditInfo.TenantId,
                auditInfo.ExecutionTime,
                auditInfo.UserId,
                x.Action,
                x.EntityType,
                x.EntityId,
                x.ChangedFields,
                auditInfo.ClientIpAddress,
                x.Succeeded)));
            await uow.CompleteAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not write the audit log rows of a request");
        }
    }

    /// <summary>
    /// ABP's own tables keep request parameters and old and new values. Salaries and amounts must not sit there in clear,
    /// so a financial request keeps who changed which field and when, without the values.
    /// </summary>
    public static void RemoveFinancialValues(AuditLogInfo auditInfo)
    {
        foreach (var change in auditInfo.EntityChanges.Where(x => x.EntityTypeFullName != null && x.EntityTypeFullName.StartsWith(FinanceNamespace, StringComparison.Ordinal)))
        {
            foreach (var property in change.PropertyChanges)
            {
                property.OriginalValue = null;
                property.NewValue = null;
            }
        }

        if (auditInfo.Url?.Split('?')[0].StartsWith(FinancePath, StringComparison.OrdinalIgnoreCase) == true)
        {
            foreach (var action in auditInfo.Actions)
            {
                action.Parameters = "{}";
            }
        }
    }
}
