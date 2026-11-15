#if NET10_0_OR_GREATER
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Schema;

namespace Alis.Reactive.PathsAndWire;

internal sealed partial class MemberPath
{
    internal partial IReadOnlyList<PathStep> BodyPath => [.. _links.Select(link => new MemberStep(link.Written.Name))];

    internal partial IReadOnlyList<string> BodyNames => [.. _links.Select(link => link.Written.Name)];
}
#endif
