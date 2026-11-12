using Alis.Reactive.Declarations;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class ConformanceElement : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Event<ElementSignal> Opened => Event<ElementSignal>();

    public Event<ElementKey> Keydown => Event<ElementKey>();
}
