using Alis.Reactive.Declarations;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class ConformanceCallbacks : Component
{
    public static ComponentVendor Declared { get; } = ComponentVendor.DefineCallbackProperties("conformance-callbacks", objectPath: "conformanceCallbacks");

    public override ComponentVendor Vendor => Declared;

    public Event<RaisedRoom> OnRaised => Event<RaisedRoom>();

    public Event<SearchArgs> OnSearch => Event<SearchArgs>();
}
