using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class NightClock : Plugin
{
    public NightClock()
        : base("nightclock", EventListening.CallbackProperties)
    {
    }

    public Event Struck => Event();
}
