using Alis.Reactive.Declarations;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.ModuleTests.Vendors;

public sealed class RoundDrawer : Component, IAppLevelComponent
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public string DefaultId => "round-drawer";

    public ReadOnly<string> Section => ReadOnly<string>();

    public Property<string?> Heading => Property<string?>();

    public void Open() => Method();

    public Event Closed => Event();

    public Event<RoundClosing> BeforeClose => Event<RoundClosing>();
}

public sealed class RoundClosing : JsObject
{
    public ReadOnly<string> ClosedBy => ReadOnly<string>();

    public Property<bool> Cancel => Property<bool>();
}
