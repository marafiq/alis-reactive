#if NET48
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.PathsAndWire;

internal sealed partial class MemberPath
{
    internal static partial string BodyName(WrittenMember written, string clrName) => clrName;
}
#endif
