using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HR_Mangment2.Infrastructure;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace HR_Mangment2;

[Collection(HostCollection.Name)]
public class SwaggerContractReportTests
{
    private static readonly string[] HttpMethods = { "get", "post", "put", "patch", "delete" };

    private readonly HostFixture _host;
    private readonly ITestOutputHelper _output;

    public SwaggerContractReportTests(HostFixture host, ITestOutputHelper output)
    {
        _host = host;
        _output = output;
    }

    private static string Normalize(string path) => Regex.Replace(path, @"\{([^:}]+)(:[^}]*)?\}", "{$1}").ToLowerInvariant();

    [Fact]
    public async Task Swagger_is_a_subset_of_the_contract_and_the_differences_are_reported()
    {
        var swagger = await _host.CreateClient().GetFromJsonAsync<JsonElement>("/swagger/v1/swagger.json");

        var contract = OpenApiContract.Instance;
        var declared = contract.Operations.ToDictionary(o => o.Method + " " + Normalize(o.Path));
        var served = new List<string>();

        foreach (var path in swagger.GetProperty("paths").EnumerateObject())
        {
            if (path.Name.StartsWith("/test-probe", StringComparison.Ordinal))
            {
                continue;
            }

            served.AddRange(path.Value.EnumerateObject()
                .Where(p => HttpMethods.Contains(p.Name))
                .Select(p => p.Name.ToUpperInvariant() + " " + Normalize(path.Name)));
        }

        var extra = served.Where(key => !declared.ContainsKey(key)).OrderBy(x => x).ToList();
        var matched = served.Where(declared.ContainsKey).OrderBy(x => x).ToList();
        var missing = declared.Keys.Except(served).Count();

        var contractSchemas = contract.SchemaProperties;
        var swaggerSchemas = swagger.GetProperty("components").GetProperty("schemas");
        var schemaLines = new List<string>();
        foreach (var schema in swaggerSchemas.EnumerateObject().Where(s => contractSchemas.ContainsKey(s.Name)))
        {
            var actual = schema.Value.TryGetProperty("properties", out var properties)
                ? properties.EnumerateObject().Select(p => p.Name).ToHashSet()
                : new HashSet<string>();
            var expected = contractSchemas[schema.Name];
            var notInContract = actual.Except(expected).OrderBy(x => x).ToList();
            var notServed = expected.Except(actual).OrderBy(x => x).ToList();

            if (notInContract.Count + notServed.Count > 0)
            {
                schemaLines.Add($"- {schema.Name}: not served [{string.Join(", ", notServed)}]; extra [{string.Join(", ", notInContract)}]");
            }
        }

        var longNames = swaggerSchemas.EnumerateObject().Where(s => s.Name.Contains('.')).Select(s => s.Name).ToList();

        var report = new StringBuilder();
        report.AppendLine("# Swagger vs openapi.yaml");
        report.AppendLine($"- Contract operations: {contract.Operations.Count}");
        report.AppendLine($"- Served and declared: {matched.Count}");
        foreach (var key in matched) report.AppendLine("  - " + key);
        report.AppendLine($"- Served but not declared: {extra.Count}");
        foreach (var key in extra) report.AppendLine("  - " + key);
        report.AppendLine($"- Declared and not built yet: {missing}");
        report.AppendLine("## Schema differences on shared names");
        foreach (var line in schemaLines) report.AppendLine(line);

        var text = report.ToString();
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "swagger-vs-contract.md"), text);
        _output.WriteLine(text);

        extra.ShouldBeEmpty();
        longNames.ShouldBeEmpty();
    }
}
