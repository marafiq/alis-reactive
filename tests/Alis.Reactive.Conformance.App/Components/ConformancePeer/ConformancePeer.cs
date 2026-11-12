using Alis.Reactive.Declarations;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class ConformancePeer : Component
{
    public static ComponentVendor Declared { get; } = ComponentVendor.Define("conformance-peer", objectPath: "conformancePeer", subscribe: "addEventListener", unsubscribe: "removeEventListener", elementPath: "element");

    public override ComponentVendor Vendor => Declared;

    public Property<double> Ratio => Property<double>();

    public Property<string> Text => Property<string>();

    public Event<Change> Change => Event<Change>();
}
