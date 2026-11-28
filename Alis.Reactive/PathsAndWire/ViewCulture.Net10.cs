#if NET10_0_OR_GREATER
using Alis.Reactive.Schema;

namespace Alis.Reactive.PathsAndWire;

internal static partial class ViewCulture
{
    private static partial NumberText AFraction() => new(group: true, exponent: true);
}
#endif
