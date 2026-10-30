using Alis.Reactive.Framework.App.Components;

namespace Alis.Reactive.Framework.App.Vendors;

/// <summary>The mistakes a page can make in declaring its components' vendor, or in naming them, each refused where the page renders.</summary>
public enum VendorMistake
{
    NameAVendorStartingWithADigit,
    PathAnObjectWithAStrayDot,
    NameTwoVendorsAlike,
    ReadTheSlateAsAMeter,
}

/// <summary>A page that declares a vendor wrongly, the way its mistake asks.</summary>
public sealed class VendorRefusalsPage
{
    public VendorMistake Mistake { get; set; }
}

/// <summary>The slate, declared with a vendor whose name starts with a digit.</summary>
public sealed class DigitNamedSlate : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Define(
        "1slate", objectPath: "slate", subscribe: "on", unsubscribe: "off", payload: VendorEventPayload.Argument);

    public void Clear() => Method();
}

/// <summary>The slate, declared with a vendor whose object path ends in a dot.</summary>
public sealed class StrayDotSlate : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Define(
        "slate", objectPath: "slate.", subscribe: "on", unsubscribe: "off", payload: VendorEventPayload.Argument);

    public void Clear() => Method();
}

/// <summary>A board declared with a vendor of the slate's name that keeps its object somewhere else.</summary>
public sealed class Board : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Define(
        Slate.SlateVendor.Name, objectPath: "board", subscribe: "on", unsubscribe: "off", payload: VendorEventPayload.Argument);

    public void Clear() => Method();
}
