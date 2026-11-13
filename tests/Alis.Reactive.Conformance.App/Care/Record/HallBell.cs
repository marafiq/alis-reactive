using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class HallBell : Plugin
{
    public HallBell()
        : base("hall-bell")
    {
    }

    public MethodResult<int> Ring() => Method<int>();
}
