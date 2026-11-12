using Alis.Reactive.Declarations;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class ConformanceRegistered : Component
{
    public static ComponentVendor Declared { get; } = ComponentVendor.Define("conformance-registered", VendorLookup.ByPlugin("conformance-registry"), subscribe: "on", unsubscribe: "off");

    public override ComponentVendor Vendor => Declared;

    public ReadOnly<int> Reading => ReadOnly<int>();

    public Property<int> Threshold => Property<int>();

    public void Raise() => Method();

    public void Lower() => Method();

    public Event Alarmed => Event();
}
