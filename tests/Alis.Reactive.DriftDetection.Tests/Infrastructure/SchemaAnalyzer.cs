using System.Text.Json;

namespace Alis.Reactive.DriftDetection.Tests.Infrastructure;

public sealed class SchemaAnalyzer
{
    private readonly Dictionary<string, DefinitionInfo> _defs = new();

    public SchemaAnalyzer(string schemaJson)
    {
        using var doc = JsonDocument.Parse(schemaJson);
        var defsElement = doc.RootElement.GetProperty("$defs");

        foreach (var def in defsElement.EnumerateObject())
        {
            var info = ParseDefinition(def.Name, def.Value);
            _defs[def.Name] = info;
        }
    }

    public DefinitionInfo GetDefinition(string defName)
    {
        if (!_defs.TryGetValue(defName, out var info))
            throw new KeyNotFoundException($"Schema $defs does not contain '{defName}'.");
        return info;
    }

    public IReadOnlyDictionary<string, DefinitionInfo> AllDefinitions => _defs;

    private static DefinitionInfo ParseDefinition(string name, JsonElement element)
    {
        var allProps = new HashSet<string>();
        var required = new HashSet<string>();
        List<string>? enumValues = null;
        List<string>? unionVariants = null;
        bool? additionalProperties = null;

        if (element.TryGetProperty("properties", out var props))
        {
            foreach (var prop in props.EnumerateObject())
                allProps.Add(prop.Name);
        }

        if (element.TryGetProperty("required", out var req))
        {
            foreach (var r in req.EnumerateArray())
                required.Add(r.GetString()!);
        }

        if (element.TryGetProperty("enum", out var enm))
        {
            enumValues = new List<string>();
            foreach (var v in enm.EnumerateArray())
                enumValues.Add(v.GetString()!);
        }

        if (element.TryGetProperty("oneOf", out var oneOf))
        {
            unionVariants = new List<string>();
            foreach (var variant in oneOf.EnumerateArray())
            {
                if (variant.TryGetProperty("$ref", out var refEl))
                {
                    var refPath = refEl.GetString()!;
                    var variantName = refPath.Replace("#/$defs/", "");
                    unionVariants.Add(variantName);
                }
            }
        }

        if (element.TryGetProperty("additionalProperties", out var ap))
        {
            if (ap.ValueKind == JsonValueKind.False)
                additionalProperties = false;
            else if (ap.ValueKind == JsonValueKind.True)
                additionalProperties = true;
        }

        var optional = allProps.Except(required).ToHashSet();

        return new DefinitionInfo(name, allProps, required, optional,
            enumValues, unionVariants, additionalProperties);
    }
}

public sealed record DefinitionInfo(
    string Name,
    HashSet<string> AllProperties,
    HashSet<string> RequiredProperties,
    HashSet<string> OptionalProperties,
    List<string>? EnumValues,
    List<string>? UnionVariants,
    bool? AdditionalProperties)
{
    public bool IsObjectDef => AllProperties.Count > 0;
    public bool IsEnumDef => EnumValues is { Count: > 0 };
    public bool IsUnionDef => UnionVariants is { Count: > 0 };
}
