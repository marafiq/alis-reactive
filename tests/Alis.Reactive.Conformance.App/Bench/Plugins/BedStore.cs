using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;

namespace Alis.Reactive.Conformance.App.Bench;

[Mirrors("Bench/Plugins/shift-log-desk", "BedStore")]
public sealed class BedStore : Plugin
{
    public BedStore()
        : base("bedstore", EventListening.ReturnedUnsubscriber("subscribe"))
    {
    }

    public Event<BedFreed> Freed => Event<BedFreed>();
}

public sealed class BedFreed : JsObject
{
    public ReadOnly<string> Bed => ReadOnly<string>();
}
