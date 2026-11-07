using System.Threading.Tasks;
using Volo.Abp.MultiTenancy;

namespace HR_Mangment2.MultiTenancy;

/// <summary>
/// The server decides the tenant from its configuration. Clients cannot choose one, so the
/// framework resolvers (the __tenant header, query string and cookie) are removed.
/// </summary>
public class ServerTenantResolveContributor : TenantResolveContributorBase
{
    public const string ContributorName = "Server";

    private readonly string _tenantName;

    public ServerTenantResolveContributor(string tenantName)
    {
        _tenantName = tenantName;
    }

    public override string Name => ContributorName;

    public override Task ResolveAsync(ITenantResolveContext context)
    {
        context.TenantIdOrName = _tenantName;
        return Task.CompletedTask;
    }
}
