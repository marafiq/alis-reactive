using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class NightClockFace : Plugin
{
    public NightClockFace()
        : base("nightclock")
    {
    }

    public ReadOnly<string> Hour => ReadOnly<string>();
}
