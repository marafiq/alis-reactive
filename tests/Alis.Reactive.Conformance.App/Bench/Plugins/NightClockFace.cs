using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;

namespace Alis.Reactive.Conformance.App.Bench;

[Mirrors("Bench/Plugins/night-desk", "NightClock")]
public sealed class NightClockFace : Plugin
{
    public NightClockFace()
        : base("nightclock")
    {
    }

    public ReadOnly<string> Hour => ReadOnly<string>();
}
