using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace HR_Mangment2.Infrastructure;

public record ContractOperation(string Method, string Path, string OperationId, IReadOnlySet<string> UuidParameters);

/// <summary>Reads api-contract/openapi.yaml, the binding source for routes and schemas.</summary>
public sealed class OpenApiContract
{
    private static readonly string[] Methods = { "get", "post", "put", "patch", "delete" };

    private static readonly Lazy<OpenApiContract> Lazy = new(Load);

    public static OpenApiContract Instance => Lazy.Value;

    public IReadOnlyList<ContractOperation> Operations { get; private init; } = Array.Empty<ContractOperation>();

    public IReadOnlyDictionary<string, IReadOnlySet<string>> SchemaProperties { get; private init; } =
        new Dictionary<string, IReadOnlySet<string>>();

    private static OpenApiContract Load()
    {
        var path = Path.Combine(TestDatabase.FindRepositoryRoot(), "api-contract", "openapi.yaml");
        var stream = new YamlStream();
        using (var reader = new StreamReader(path))
        {
            stream.Load(reader);
        }

        var document = (YamlMappingNode)stream.Documents[0].RootNode;
        var components = Map(document, "components")!;
        var parameters = Map(components, "parameters")!;

        var operations = new List<ContractOperation>();
        foreach (var (pathKey, pathValue) in Map(document, "paths")!.Children)
        {
            var pathItem = (YamlMappingNode)pathValue;
            var shared = ReadUuidParameters(pathItem, parameters);

            foreach (var method in Methods)
            {
                if (Map(pathItem, method) is not { } operation)
                {
                    continue;
                }

                var uuids = new HashSet<string>(shared);
                uuids.UnionWith(ReadUuidParameters(operation, parameters));
                operations.Add(new ContractOperation(
                    method.ToUpperInvariant(),
                    ((YamlScalarNode)pathKey).Value!,
                    Scalar(operation, "operationId")!,
                    uuids));
            }
        }

        var schemas = new Dictionary<string, IReadOnlySet<string>>();
        foreach (var (key, value) in Map(components, "schemas")!.Children)
        {
            var properties = value is YamlMappingNode schema ? Map(schema, "properties") : null;
            schemas[((YamlScalarNode)key).Value!] = properties == null
                ? new HashSet<string>()
                : properties.Children.Keys.Select(k => ((YamlScalarNode)k).Value!).ToHashSet();
        }

        return new OpenApiContract { Operations = operations, SchemaProperties = schemas };
    }

    private static HashSet<string> ReadUuidParameters(YamlMappingNode owner, YamlMappingNode shared)
    {
        var result = new HashSet<string>();
        if (!owner.Children.TryGetValue(new YamlScalarNode("parameters"), out var node) || node is not YamlSequenceNode list)
        {
            return result;
        }

        foreach (var entry in list.Children.OfType<YamlMappingNode>())
        {
            var parameter = entry;
            var reference = Scalar(entry, "$ref");
            if (reference != null)
            {
                parameter = Map(shared, reference.Split('/').Last())!;
            }

            if (Scalar(parameter, "in") == "path" && Map(parameter, "schema") is { } schema && Scalar(schema, "format") == "uuid")
            {
                result.Add(Scalar(parameter, "name")!);
            }
        }

        return result;
    }

    private static YamlMappingNode? Map(YamlMappingNode node, string key) =>
        node.Children.TryGetValue(new YamlScalarNode(key), out var value) ? value as YamlMappingNode : null;

    private static string? Scalar(YamlMappingNode node, string key) =>
        node.Children.TryGetValue(new YamlScalarNode(key), out var value) ? (value as YamlScalarNode)?.Value : null;
}
