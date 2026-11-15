using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;

namespace Alis.Reactive.Conformance.App.Bench;

[Mirrors("Bench/Plugins/shift-log-desk", "NightClock")]
public sealed class NightClock : Plugin
{
    public NightClock()
        : base("nightclock", EventListening.CallbackProperties)
    {
    }

    public Event Struck => Event("onStruck");
}
