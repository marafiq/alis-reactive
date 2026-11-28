#if NET48
using Alis.Reactive.Schema;

namespace Alis.Reactive.PathsAndWire;

internal static partial class ViewCulture
{
    private static partial NumberText AFraction() => new(group: false, exponent: true);
}
#endif
