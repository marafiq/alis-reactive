using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Alis.Reactive.TypeKnowledge;

internal static class Contracts
{
    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> Bindings = new();

    internal static bool IsList(this JsonTypeInfo type, [NotNullWhen(true)] out JsonTypeInfo? item)
    {
        item = type.Kind == JsonTypeInfoKind.Enumerable ? type.Options.GetTypeInfo(type.ElementType!) : null;
        return item is not null;
    }

    internal static bool IsDictionary(this JsonTypeInfo type, [NotNullWhen(true)] out JsonTypeInfo? key, [NotNullWhen(true)] out JsonTypeInfo? value)
    {
        var holdsEntries = type.Kind == JsonTypeInfoKind.Dictionary;
        key = holdsEntries ? type.Options.GetTypeInfo(type.KeyType!) : null;
        value = holdsEntries ? type.Options.GetTypeInfo(type.ElementType!) : null;
        return holdsEntries;
    }

    internal static IReadOnlyList<string> RequiredNames(this JsonTypeInfo holder)
        => [.. holder.Properties.Where(property => property.IsRequired).Select(property => property.Name)];

    internal static bool StartHangsOnTheData(this JsonTypeInfo holder) => holder.PolymorphismOptions is not null;

    internal static JsonElement StartOf(this JsonTypeInfo type)
    {
        var start = JsonSerializer.Deserialize("{}", Binding(type.Options).GetTypeInfo(type.Type));
        return JsonSerializer.SerializeToElement(start, type);
    }

    internal static JsonSerializerOptions Binding(JsonSerializerOptions app) => Bindings.GetValue(app, EveryMemberOptional);

    private static JsonSerializerOptions EveryMemberOptional(JsonSerializerOptions app)
    {
        var binding = new JsonSerializerOptions(app)
        {
            TypeInfoResolver = (app.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver()).WithAddedModifier(MembersOptional),
        };
        binding.MakeReadOnly();
        return binding;
    }

    private static void MembersOptional(JsonTypeInfo type)
    {
        foreach (var property in type.Properties) property.IsRequired = false;
    }
}
