#if NET48
using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.PathsAndWire;

internal sealed partial class MemberPath
{
    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> BodyNamings = new();

    internal static partial string BodyName(WrittenMember written, string clrName) => clrName;

    internal static partial JsonSerializerOptions NamedAsTheBody(JsonSerializerOptions options) => BodyNamings.GetValue(options, NamingAsCSharpDeclares);

    internal static partial string? BoundUnderAnotherName(JsonTypeInfo holder, JsonPropertyInfo member)
    {
        var declared = WrittenMember.AsCSharpDeclaresIt(holder, member);
        return string.Equals(declared, member.Name, StringComparison.OrdinalIgnoreCase) ? null : declared;
    }

    private static JsonSerializerOptions NamingAsCSharpDeclares(JsonSerializerOptions options)
    {
        var named = new JsonSerializerOptions(options)
        {
            TypeInfoResolver = (options.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver()).WithAddedModifier(NameAsCSharpDeclares),
        };
        named.MakeReadOnly();
        return named;
    }

    private static void NameAsCSharpDeclares(JsonTypeInfo holder)
    {
        foreach (var member in holder.Properties) member.Name = WrittenMember.AsCSharpDeclaresItElseAsWritten(member);
    }
}
#endif
