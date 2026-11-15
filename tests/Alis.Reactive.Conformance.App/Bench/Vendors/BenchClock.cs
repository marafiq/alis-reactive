using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;

namespace Alis.Reactive.Conformance.App.Bench;

[Mirrors("Bench/Vendors/bench-clock", "BenchClock")]
public sealed class BenchClock : Plugin
{
    public BenchClock()
        : base("bench-clock", EventListening.Methods("on", "off"))
    {
    }

    public Event Tick => Event();
}
