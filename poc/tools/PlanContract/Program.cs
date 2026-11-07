using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.Components;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: PlanContract <path of plan.ts>");
    return 2;
}

var core = typeof(Component).Assembly;
var planJson = core.GetType("Alis.Reactive.Plan.PlanJson", throwOnError: true)!;
var options = (JsonSerializerOptions)planJson.GetField("Options", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
var root = core.GetType("Alis.Reactive.Plan.PlanDocument", throwOnError: true)!;
var choice = core.GetType("Alis.Reactive.Plan.IChoice", throwOnError: true)!;
var pathStep = core.GetType("Alis.Reactive.Plan.PathStep", throwOnError: true)!;

var declarations = new SortedDictionary<string, string>(StringComparer.Ordinal);
var visited = new HashSet<Type>();
var contract = new Contract(options, choice, pathStep, declarations, visited);
contract.Visit(root);
contract.Visit(core.GetType("Alis.Reactive.Plan.HttpSend", throwOnError: true)!);
contract.Visit(core.GetType("Alis.Reactive.Plan.HttpResponse", throwOnError: true)!);

var text = new StringBuilder();
text.Append("export type JsonValue = string | number | boolean | null | readonly JsonValue[] | { readonly [key: string]: JsonValue };\n");
foreach (var declaration in declarations.Values)
    text.Append('\n').Append(declaration);
File.WriteAllText(args[0], text.ToString());
Console.WriteLine($"plan-contract: wrote {declarations.Count} declarations to {args[0]}");
return 0;

internal sealed class Contract(JsonSerializerOptions options, Type choice, Type pathStep, SortedDictionary<string, string> declarations, HashSet<Type> visited)
{
    internal string Visit(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(int)) return "number";
        if (type == typeof(bool)) return "boolean";
        if (type == typeof(JsonElement)) return "JsonValue";
        if (choice.IsAssignableFrom(type)) return Choice(type);
        if (type == pathStep) return Declare(type, "export type PathStep = string | number;\n");

        var info = options.GetTypeInfo(type);
        return info.Kind switch
        {
            JsonTypeInfoKind.Enumerable => $"readonly {Visit(info.ElementType!)}[]",
            JsonTypeInfoKind.Dictionary => $"{{ readonly [key: string]: {Visit(info.ElementType!)} }}",
            JsonTypeInfoKind.Object when info.PolymorphismOptions is { } polymorphic => Union(type, polymorphic),
            JsonTypeInfoKind.Object => Shape(type, info, discriminator: null),
            _ => throw new InvalidOperationException($"The plan holds '{type.FullName}', which the contract does not map."),
        };
    }

    private string Union(Type type, JsonPolymorphismOptions polymorphic)
    {
        var name = type.Name;
        if (!visited.Add(type)) return name;
        var members = new List<string>();
        foreach (var derived in polymorphic.DerivedTypes)
        {
            var discriminator = (string)derived.TypeDiscriminator!;
            members.Add(Shape(derived.DerivedType, options.GetTypeInfo(derived.DerivedType), discriminator));
        }

        declarations[name] = $"export type {name} = {string.Join(" | ", members)};\n";
        return name;
    }

    private string Shape(Type type, JsonTypeInfo info, string? discriminator)
    {
        var name = type.Name;
        var union = type.BaseType;
        var writtenWithoutItsKind = discriminator is null && union is not null && union != typeof(object) && options.GetTypeInfo(union).PolymorphismOptions is not null;
        if (writtenWithoutItsKind)
            throw new InvalidOperationException($"'{type.FullName}' is a node of the union '{union!.Name}' held by a member typed as itself, so the plan writes it without its kind. Type the member as the union.");
        if (!visited.Add(type)) return name;
        var lines = new StringBuilder();
        lines.Append("export interface " + name + " {\n");
        if (discriminator is not null)
            lines.Append("  readonly kind: \"" + discriminator + "\";\n");
        foreach (var property in info.Properties)
            lines.Append("  readonly " + property.Name + ": " + Visit(property.PropertyType) + ";\n");
        lines.Append("}\n");
        declarations[name] = lines.ToString();
        return name;
    }

    private string Choice(Type type)
    {
        var name = type.Name;
        if (!visited.Add(type)) return name;
        var cases = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
            .Where(field => field.FieldType == type)
            .Select(field => "\"" + (string)type.GetProperty("Text")!.GetValue(field.GetValue(null))! + "\"");
        declarations[name] = $"export type {name} = {string.Join(" | ", cases)};\n";
        return name;
    }

    private string Declare(Type type, string declaration)
    {
        if (visited.Add(type)) declarations[type.Name] = declaration;
        return type.Name;
    }
}
