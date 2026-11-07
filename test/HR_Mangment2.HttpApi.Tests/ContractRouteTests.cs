using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HR_Mangment2.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace HR_Mangment2;

[Collection(HostCollection.Name)]
public class ContractRouteTests
{
    /// <summary>Non-API routes the Host may open: sign-in, OAuth, framework scripts, Swagger, status.</summary>
    private static readonly string[] AllowedNonApiPrefixes =
    {
        "/connect/", "/Account/", "/Abp/", "/Error", "/swagger", "/health-status", "/.well-known/", "/test-probe/", "/signalr-hubs/",
        // The target of the opaque short-lived links that the download-url operations return.
        "/files/"
    };

    private readonly HostFixture _host;

    public ContractRouteTests(HostFixture host)
    {
        _host = host;
    }

    private List<(string Method, string Pattern)> RunningRoutes()
    {
        var source = _host.Factory.Services.GetRequiredService<EndpointDataSource>();

        return source.Endpoints
            .OfType<RouteEndpoint>()
            .SelectMany(endpoint =>
            {
                var pattern = "/" + (endpoint.RoutePattern.RawText ?? string.Empty).TrimStart('/');
                var methods = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? new List<string> { "*" };
                return methods.Select(method => (method, pattern));
            })
            .ToList();
    }

    private static string StripConstraints(string pattern) => Regex.Replace(pattern, @"\{([^:}]+)(:[^}]*)?\}", "{$1}");

    private static string ApiKey(string method, string pattern) => method.ToUpperInvariant() + " " + StripConstraints(pattern).ToLowerInvariant();

    private static bool IsStaticAsset(string pattern) => pattern[(pattern.LastIndexOf('/') + 1)..].Contains('.');

    [Fact]
    public void Every_api_route_is_declared_in_the_contract()
    {
        var declared = OpenApiContract.Instance.Operations.Select(o => ApiKey(o.Method, o.Path)).ToHashSet();

        var undeclared = RunningRoutes()
            .Where(r => r.Pattern.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
            .Select(r => ApiKey(r.Method, r.Pattern))
            .Where(key => !declared.Contains(key))
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        undeclared.ShouldBeEmpty();
    }

    [Fact]
    public void Non_api_routes_stay_inside_the_allowed_surface()
    {
        var unexpected = RunningRoutes()
            .Select(r => r.Pattern)
            .Where(p => !p.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
            .Where(p => !IsStaticAsset(p))
            .Where(p => p != "/" && !p.StartsWith("/{") && !p.StartsWith("/Index", StringComparison.OrdinalIgnoreCase))
            .Where(p => !AllowedNonApiPrefixes.Any(a => p.StartsWith(a, StringComparison.OrdinalIgnoreCase)))
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        unexpected.ShouldBeEmpty();
    }

    [Fact]
    public void Framework_module_routes_are_gone()
    {
        var api = RunningRoutes().Select(r => r.Pattern.ToLowerInvariant()).ToList();

        foreach (var prefix in new[]
                 {
                     "/api/identity", "/api/account", "/api/permission-management", "/api/multi-tenancy",
                     "/api/feature-management", "/api/setting-management", "/api/abp/api-definition",
                     "/api/abp/multi-tenancy", "/api/abp/application-localization", "/health-ui", "/health-api"
                 })
        {
            api.ShouldNotContain(p => p.StartsWith(prefix));
        }
    }

    [Fact]
    public void Implemented_operations_exist_with_their_method_and_guid_constraints()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "implemented-operations.txt");
        var implemented = File.ReadAllLines(file)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0 && !l.StartsWith('#'))
            .ToList();
        implemented.ShouldNotBeEmpty();

        var running = RunningRoutes();
        var problems = new List<string>();

        foreach (var id in implemented)
        {
            var operation = OpenApiContract.Instance.Operations.SingleOrDefault(o => o.OperationId == id);
            if (operation == null)
            {
                problems.Add(id + ": not in the contract");
                continue;
            }

            var match = running.FirstOrDefault(r =>
                string.Equals(r.Method, operation.Method, StringComparison.OrdinalIgnoreCase) || r.Method == "*"
                    ? ApiKey(operation.Method, r.Pattern) == ApiKey(operation.Method, operation.Path)
                    : false);

            if (match == default)
            {
                problems.Add(id + ": no route " + operation.Method + " " + operation.Path);
                continue;
            }

            foreach (var parameter in operation.UuidParameters)
            {
                if (!match.Pattern.Contains("{" + parameter + ":guid}", StringComparison.OrdinalIgnoreCase))
                {
                    problems.Add(id + ": {" + parameter + "} has no :guid constraint");
                }
            }
        }

        problems.ShouldBeEmpty();
    }
}
