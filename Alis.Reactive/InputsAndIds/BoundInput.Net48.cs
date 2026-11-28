#if NET48
using System.Text.Json;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.InputsAndIds;

internal sealed partial class BoundInput
{
    private static partial string NameOf(WrittenMember member, string clrName) => clrName;

    private static partial string PrefixMemberName(string clrName, JsonSerializerOptions app) => clrName;
}
#endif
