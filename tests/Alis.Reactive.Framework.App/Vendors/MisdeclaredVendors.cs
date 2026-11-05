using Alis.Reactive.Framework.App.Components;

namespace Alis.Reactive.Framework.App.Vendors;

public enum VendorMistake
{
    NameAVendorStartingWithADigit,
    PathAnObjectWithAStrayDot,
    NameTwoVendorsAlike,
    ReadTheSlateAsAMeter,
}

public sealed class VendorRefusalsPage
{
    public VendorMistake Mistake { get; set; }
}

public sealed class DigitNamedSlate : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Define(
        "1slate", objectPath: "slate", subscribe: "on", unsubscribe: "off", payload: VendorEventPayload.Argument);

    public void Clear() => Method();
}

public sealed class StrayDotSlate : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Define(
        "slate", objectPath: "slate.", subscribe: "on", unsubscribe: "off", payload: VendorEventPayload.Argument);

    public void Clear() => Method();
}

public sealed class Board : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Define(
        Slate.SlateVendor.Name, objectPath: "board", subscribe: "on", unsubscribe: "off", payload: VendorEventPayload.Argument);

    public void Clear() => Method();
}
