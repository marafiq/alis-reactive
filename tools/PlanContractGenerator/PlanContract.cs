using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace Alis.Reactive.PlanContractGenerator;

public sealed class PlanContract
{
    private const string VersionMember = "version";
    private const string JsonValueName = "JsonValue";
    private const string JsonValueDeclaration =
        "export type JsonValue = string | number | boolean | null | readonly JsonValue[] | { readonly [key: string]: JsonValue };";

    private static readonly JsonSerializerOptions SchemaText = new()
    {
        WriteIndented = true,
        IndentSize = 2,
        NewLine = "\n",
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly JsonSerializerOptions _options;
    private readonly Type _root;
    private readonly Dictionary<Type, WrittenType> _written;
    private readonly List<string> _order = [];
    private readonly Dictionary<string, Declaration> _declarations = new(StringComparer.Ordinal);
    private readonly Dictionary<Type, string> _named = [];
    private readonly Dictionary<string, string> _kinds = new(StringComparer.Ordinal);
    private readonly NullabilityInfoContext _nullability = new();
    private bool _holdsJson;

    private PlanContract(JsonSerializerOptions options, Type root, IReadOnlyList<WrittenType> written)
    {
        _options = options;
        _root = root;
        _written = written.ToDictionary(row => row.Type);
    }

    public static PlanContract Read(JsonSerializerOptions options, Type root, IReadOnlyList<WrittenType> written)
    {
        var contract = new PlanContract(options, root, written);
        contract.Visit(root, root.Name);
        return contract;
    }

    public string TypeScript()
    {
        var declarations = _order.Select(name => _declarations[name].TypeScript);
        var all = _holdsJson ? declarations.Prepend(JsonValueDeclaration) : declarations;
        return string.Join("\n\n", all) + "\n";
    }

    public string Schema()
    {
        var definitions = new JsonObject();
        foreach (var name in _order) definitions[name] = _declarations[name].Schema.DeepClone();
        var schema = new JsonObject
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
            ["$ref"] = "#/$defs/" + _root.Name,
            ["$defs"] = definitions,
        };
        return schema.ToJsonString(SchemaText) + "\n";
    }

    private Reference Visit(Type type, string holder)
    {
        if (type == typeof(string)) return new Reference("string", Typed("string"));
        if (type == typeof(int)) return new Reference("number", Typed("integer"));
        if (type == typeof(bool)) return new Reference("boolean", Typed("boolean"));
        if (type == typeof(JsonElement)) return AnyJson();
        if (_written.TryGetValue(type, out var row)) return Named(Written(row));

        var info = _options.GetTypeInfo(type);
        return info.Kind switch
        {
            JsonTypeInfoKind.Enumerable => ListOf(Visit(info.ElementType!, holder)),
            JsonTypeInfoKind.Dictionary => MapOf(Visit(info.ElementType!, holder)),
            JsonTypeInfoKind.Object when info.PolymorphismOptions is { } polymorphism => Named(Union(type, polymorphism)),
            JsonTypeInfoKind.Object => Named(Node(WrittenWithItsKind(type, holder), info, kind: null)),
            _ => Named(Choice(type, holder)),
        };
    }

    private Reference AnyJson()
    {
        _holdsJson = true;
        return new Reference(JsonValueName, new JsonObject());
    }

    private static Reference ListOf(Reference item)
    {
        var element = item.TypeScript.Contains(' ') ? "(" + item.TypeScript + ")" : item.TypeScript;
        return new Reference("readonly " + element + "[]", new JsonObject { ["type"] = "array", ["items"] = item.Schema });
    }

    private static Reference MapOf(Reference value)
        => new("Readonly<Record<string, " + value.TypeScript + ">>", new JsonObject { ["type"] = "object", ["additionalProperties"] = value.Schema });

    private static Reference Named(string name) => new(name, new JsonObject { ["$ref"] = "#/$defs/" + name });

    private static JsonObject Typed(string type) => new() { ["type"] = type };

    private string Written(WrittenType row)
    {
        if (_named.TryGetValue(row.Type, out var known)) return known;
        var name = Reserve(row.Type);
        foreach (var kind in row.Kinds) Claim(kind, name);
        foreach (var used in row.Uses) Visit(used, name);
        _declarations[name] = new Declaration(row.TypeScript, row.Schema);
        return name;
    }

    private string Union(Type type, JsonPolymorphismOptions polymorphism)
    {
        if (_named.TryGetValue(type, out var known)) return known;
        var name = Reserve(type);
        var kinds = new List<string>();
        foreach (var derived in polymorphism.DerivedTypes)
        {
            var kind = derived.TypeDiscriminator as string
                ?? throw new GrammarRefusedException($"{derived.DerivedType.Name} is a kind of {name} with no text for its kind.");
            var expected = char.ToUpperInvariant(kind[0]) + kind.Substring(1) + name;
            if (derived.DerivedType.Name != expected)
                throw new GrammarRefusedException($"{derived.DerivedType.Name} writes the kind '{kind}' of {name}; the class of that kind is named {expected}.");
            kinds.Add(Node(derived.DerivedType, _options.GetTypeInfo(derived.DerivedType), new Kind(polymorphism.TypeDiscriminatorPropertyName, kind)));
        }

        _declarations[name] = new Declaration(
            "export type " + name + " = " + string.Join(" | ", kinds) + ";",
            new JsonObject { ["oneOf"] = new JsonArray(kinds.Select(kind => (JsonNode)new JsonObject { ["$ref"] = "#/$defs/" + kind }).ToArray()) });
        return name;
    }

    private Type WrittenWithItsKind(Type type, string holder)
    {
        for (var union = type.BaseType; union != null && union != typeof(object); union = union.BaseType)
        {
            var isAKindOfIt = _options.GetTypeInfo(union).PolymorphismOptions?.DerivedTypes.Any(derived => derived.DerivedType == type) ?? false;
            if (isAKindOfIt)
                throw new GrammarRefusedException($"{holder} is typed as {type.Name}, a kind of {union.Name}, so the plan would write it without its kind; type the member as {union.Name}.");
        }

        return type;
    }

    private string Node(Type type, JsonTypeInfo info, Kind? kind)
    {
        if (_named.TryGetValue(type, out var known)) return known;
        var name = Reserve(type);
        if (!type.IsSealed) throw new GrammarRefusedException($"{name} is a node of the plan and is not sealed.");

        var typeScript = new StringBuilder("export interface ").Append(name).Append(" {\n");
        var properties = new JsonObject();
        var required = new JsonArray();
        if (kind != null)
        {
            Claim(kind.Text, name);
            Member(kind.Member, Constant(kind.Text));
        }

        foreach (var property in info.Properties)
        {
            var member = name + "." + property.Name;
            if (HoldsNull(NullabilityOf(property)))
                throw new GrammarRefusedException($"{member} is nullable; every field of a node is required and none holds null as a state.");
            var isThePlansVersion = type == _root && property.Name == VersionMember;
            Member(property.Name, isThePlansVersion ? Constant(property.Get!(RuntimeHelpers.GetUninitializedObject(type))!) : Visit(property.PropertyType, member));
        }

        typeScript.Append('}');
        _declarations[name] = new Declaration(
            typeScript.ToString(),
            new JsonObject { ["type"] = "object", ["properties"] = properties, ["required"] = required, ["additionalProperties"] = false });
        return name;

        void Member(string member, Reference reference)
        {
            typeScript.Append("  readonly ").Append(member).Append(": ").Append(reference.TypeScript).Append(";\n");
            properties[member] = reference.Schema;
            required.Add(member);
        }
    }

    private static Reference Constant(object value)
        => new(JsonSerializer.Serialize(value), new JsonObject { ["const"] = JsonSerializer.SerializeToNode(value) });

    private NullabilityInfo NullabilityOf(JsonPropertyInfo property)
        => property.AttributeProvider is FieldInfo field ? _nullability.Create(field) : _nullability.Create((PropertyInfo)property.AttributeProvider!);

    private static bool HoldsNull(NullabilityInfo member)
        => member.ReadState == NullabilityState.Nullable
           || (member.ElementType != null && HoldsNull(member.ElementType))
           || member.GenericTypeArguments.Any(HoldsNull);

    private string Choice(Type type, string holder)
    {
        if (_named.TryGetValue(type, out var known)) return known;
        var cases = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(field => field.FieldType == type && field.IsInitOnly)
            .ToList();
        var isAChoice = type.IsClass && cases.Count > 0;
        if (!isAChoice) throw new GrammarRefusedException($"{holder} is a {type.FullName}; the plan's contract has no row for it.");

        var name = Reserve(type);
        var canBeMadeOutside = !type.IsSealed
            || type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Any(constructor => !constructor.IsPrivate);
        if (canBeMadeOutside)
            throw new GrammarRefusedException($"{name} is a closed choice and a case of it can be made outside it; seal it and make its constructor private.");

        var texts = new List<string>();
        foreach (var field in cases)
        {
            var written = JsonSerializer.SerializeToElement(field.GetValue(null), type, _options);
            if (written.ValueKind != JsonValueKind.String)
                throw new GrammarRefusedException($"{name}.{field.Name} is written as {written.GetRawText()}; a case of a closed choice is written as its text.");
            var text = written.GetString()!;
            if (texts.Contains(text))
                throw new GrammarRefusedException($"{name}.{field.Name} is written as \"{text}\", the text of another case of {name}.");
            texts.Add(text);
        }

        _declarations[name] = new Declaration(
            "export type " + name + " = " + string.Join(" | ", texts.Select(text => JsonSerializer.Serialize(text))) + ";",
            new JsonObject { ["enum"] = new JsonArray(texts.Select(text => (JsonNode)text).ToArray()) });
        return name;
    }

    private string Reserve(Type type)
    {
        var name = type.Name;
        if (_order.Contains(name))
            throw new GrammarRefusedException($"{type.FullName} is the second type of the plan named {name}; a name of the contract is one type.");
        _named[type] = name;
        _order.Add(name);
        return name;
    }

    private void Claim(string kind, string name)
    {
        if (_kinds.TryGetValue(kind, out var first))
            throw new GrammarRefusedException($"{name} and {first} both write the kind '{kind}'; a kind is one name across the plan.");
        _kinds[kind] = name;
    }

    private sealed record Kind(string Member, string Text);

    private sealed record Reference(string TypeScript, JsonNode Schema);

    private sealed record Declaration(string TypeScript, JsonNode Schema);
}
