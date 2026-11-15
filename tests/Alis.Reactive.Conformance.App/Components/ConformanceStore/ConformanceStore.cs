using Alis.Reactive.Declarations;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.Conformance.App.Components;

[Mirrors("Components/ConformanceStore/conformance-store", "ConformanceStore")]
public sealed class ConformanceStore : Component
{
    public static ComponentVendor Declared { get; } = ComponentVendor.DefineReturnedUnsubscriber("conformance-store", objectPath: "conformanceStore", subscribe: "subscribe");

    public override ComponentVendor Vendor => Declared;

    public Event<Posted> Posted => Event<Posted>();
}
