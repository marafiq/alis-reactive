using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Alis.Reactive.Values;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

public sealed class OxygenGauge : Component
{
    public static ComponentVendor GaugeVendor { get; } = ComponentVendor.Define("oxygengauge", VendorLookup.ByPlugin("oxygen-gauges"), subscribe: "on", unsubscribe: "off");

    public override ComponentVendor Vendor => GaugeVendor;

    public ReadOnly<string> Reading => ReadOnly<string>();

    public Property<int> AlarmBelow => Property<int>();

    public Event<GaugeAlarm> Alarm => Event<GaugeAlarm>();

    public void Lower(Arg<int> points) => Method([points]);
}

public sealed class GaugeAlarm : JsObject
{
    public ReadOnly<string> Message => ReadOnly<string>();
}

public static class OxygenGaugeHtml
{
    public static Rendered<OxygenGauge, TModel> OxygenGauge<TModel>(this ReactivePlan<TModel> plan, string id, int saturation)
    {
        var gauge = new TagBuilder("section");
        gauge.Attributes["id"] = id;
        gauge.Attributes["data-oxygen-gauge"] = saturation.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return plan.Render<OxygenGauge>(id, gauge);
    }

    public static Rendered<OxygenGauge, TModel> UnmountedOxygenGauge<TModel>(this ReactivePlan<TModel> plan, string id)
    {
        var gauge = new TagBuilder("section");
        gauge.Attributes["id"] = id;
        return plan.Render<OxygenGauge>(id, gauge);
    }
}
