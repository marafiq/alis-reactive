namespace Alis.Reactive.PlanTests.Support;

public sealed class Gauge : Component
{
    public static readonly ComponentVendor Library =
        ComponentVendor.Define("gauge", "", "on", "off", VendorEventPayload.Argument);

    public override ComponentVendor Vendor => Library;

    public ReadOnly<string> Label => ReadOnly<string>();

    public Event Tick => Event();
}

public sealed class GaugeAlike : Component
{
    public static readonly ComponentVendor Library =
        ComponentVendor.Define("gauge", "", "addEventListener", "removeEventListener", VendorEventPayload.Element);

    public override ComponentVendor Vendor => Library;

    public Event Tick => Event();
}

public sealed class Meter : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Reading Read(Arg<string> probe) => Object<Reading>(probe);
}

public sealed class Reading : JsObject
{
    public ReadOnly<int> Value => ReadOnly<int>();

    public Event Settled => Event();
}

public sealed class Thermostat : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Property<int> Target => Property<int>(name: "settings.target");
}
