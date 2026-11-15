using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;

namespace Alis.Reactive.Conformance.App.Care;

[Mirrors("Care/Record/hall-bell", "HallBell")]
public sealed class HallBell : Plugin
{
    public HallBell()
        : base("hall-bell")
    {
    }

    public MethodResult<int> Ring() => Method<int>();
}
