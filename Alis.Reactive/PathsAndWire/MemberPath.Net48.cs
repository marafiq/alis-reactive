#if NET48
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Schema;

namespace Alis.Reactive.PathsAndWire;

internal sealed partial class MemberPath
{
    internal partial IReadOnlyList<PathStep> BodyPath => [.. _links.Select(link => new MemberStep(link.ClrName))];
}
#endif
