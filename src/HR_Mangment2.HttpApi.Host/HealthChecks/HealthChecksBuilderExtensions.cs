using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;

namespace HR_Mangment2.HealthChecks;

public static class HealthChecksBuilderExtensions
{
    /// <summary>Status endpoint for the hosting platform. It returns the status text only.</summary>
    public static void AddHR_Mangment2HealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<HR_Mangment2DatabaseCheck>("HR_Mangment2 DbContext Check", tags: new[] { "database" });

        services.Configure<AbpEndpointRouterOptions>(options =>
        {
            options.EndpointConfigureActions.Add(endpointContext =>
            {
                endpointContext.Endpoints.MapHealthChecks(
                    new PathString("/health-status"),
                    new HealthCheckOptions { AllowCachingResponses = false });
            });
        });
    }
}
