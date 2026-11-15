#if NET10_0_OR_GREATER
using System.Text.Json;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.InputsAndIds;

internal sealed partial class BoundInput
{
    private static partial string NameOf(WrittenMember member, string clrName) => member.Name;

    private static partial string PrefixMemberName(string clrName, JsonSerializerOptions app)
        => app.PropertyNamingPolicy is { } policy ? policy.ConvertName(clrName) : clrName;
}
#endif
