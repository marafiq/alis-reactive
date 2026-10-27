using Alis.Reactive.Components;

namespace Alis.Reactive.PlanTests.Support;

// Components brought from outside the framework, declared on vendors of their own, and the
// declarations a developer can get wrong.

/// <summary>A gauge whose library takes listeners through on and off.</summary>
public sealed class Gauge : Component
{
    public static readonly ComponentVendor Library =
        ComponentVendor.Define("gauge", "", "on", "off", VendorEventPayload.Argument);

    public override ComponentVendor Vendor => Library;

    public ReadOnly<string> Label => ReadOnly<string>();

    public Event Tick => Event();
}

/// <summary>A second gauge library that took the same vendor name with other listening methods.</summary>
public sealed class GaugeAlike : Component
{
    public static readonly ComponentVendor Library =
        ComponentVendor.Define("gauge", "", "addEventListener", "removeEventListener", VendorEventPayload.DomEvent);

    public override ComponentVendor Vendor => Library;

    public Event Tick => Event();
}

/// <summary>A meter whose read returns an object; an event declared on that object has no owner to subscribe on.</summary>
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

/// <summary>A thermostat whose target is declared by a path into its settings object instead of as that object's member.</summary>
public sealed class Thermostat : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Property<int> Target => Property<int>(name: "settings.target");
}

/// <summary>A plugin an app registers on the page.</summary>
public sealed class Clock : Plugin
{
    public Clock() : base("clock")
    {
    }

    public ReadOnly<string> Now => ReadOnly<string>();
}
