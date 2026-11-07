using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.SecurityLog;
using Volo.Abp.Uow;

namespace HR_Mangment2.Auditing;

/// <summary>Sign-in events reach ABP's security log from the login pages. This also shows them on the audit log screen.</summary>
[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(ISecurityLogStore))]
public class HrSecurityLogStore : ISecurityLogStore, ITransientDependency
{
    private readonly IdentitySecurityLogStore _inner;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IRepository<HrAuditEntry, Guid> _entries;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<HrSecurityLogStore> _logger;

    public HrSecurityLogStore(
        IdentitySecurityLogStore inner,
        IUnitOfWorkManager unitOfWorkManager,
        IRepository<HrAuditEntry, Guid> entries,
        IGuidGenerator guidGenerator,
        ILogger<HrSecurityLogStore> logger)
    {
        _inner = inner;
        _unitOfWorkManager = unitOfWorkManager;
        _entries = entries;
        _guidGenerator = guidGenerator;
        _logger = logger;
    }

    /// <summary>A password accepted with a code still to come is not a sign-in yet, and signing out is not listed by the contract.</summary>
    public static AuditActionType? MapAction(string? action)
    {
        if (action == null || !action.StartsWith("Login", StringComparison.Ordinal) ||
            action == IdentitySecurityLogActionConsts.LoginRequiresTwoFactor)
        {
            return null;
        }

        return action == IdentitySecurityLogActionConsts.LoginSucceeded ? AuditActionType.Login : AuditActionType.LoginFailed;
    }

    public async Task SaveAsync(SecurityLogInfo securityLogInfo)
    {
        await _inner.SaveAsync(securityLogInfo);

        var action = MapAction(securityLogInfo.Action);
        if (action == null)
        {
            return;
        }

        try
        {
            using var uow = _unitOfWorkManager.Begin(requiresNew: true);
            await _entries.InsertAsync(new HrAuditEntry(
                _guidGenerator.Create(),
                securityLogInfo.TenantId,
                securityLogInfo.CreationTime.Kind == DateTimeKind.Utc ? securityLogInfo.CreationTime : securityLogInfo.CreationTime.ToUniversalTime(),
                securityLogInfo.UserId,
                action.Value,
                "UserAccount",
                securityLogInfo.UserId?.ToString(),
                Array.Empty<string>(),
                securityLogInfo.ClientIpAddress,
                action == AuditActionType.Login));
            await uow.CompleteAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not write the audit log row of a sign-in event");
        }
    }
}
