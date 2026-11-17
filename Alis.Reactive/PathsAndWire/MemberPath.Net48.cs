#if NET48
using System;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.PathsAndWire;

internal sealed partial class MemberPath
{
    internal static partial string BodyName(WrittenMember written, string clrName) => clrName;

    internal static partial string? BoundUnderAnotherName(JsonTypeInfo holder, JsonPropertyInfo member)
    {
        var declared = WrittenMember.AsCSharpDeclaresIt(holder, member);
        return string.Equals(declared, member.Name, StringComparison.OrdinalIgnoreCase) ? null : declared;
    }
}
#endif
