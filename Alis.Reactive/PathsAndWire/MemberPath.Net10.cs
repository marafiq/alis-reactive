#if NET10_0_OR_GREATER
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.PathsAndWire;

internal sealed partial class MemberPath
{
    internal static partial string BodyName(WrittenMember written, string clrName) => written.Name;
}
#endif
