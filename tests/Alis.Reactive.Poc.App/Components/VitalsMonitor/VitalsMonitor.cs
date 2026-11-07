using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

public sealed class VitalsMonitor : Component
{
    public static ComponentVendor MonitorVendor { get; } = ComponentVendor.Define("monitor", objectPath: "monitor", subscribe: "on", unsubscribe: "off");

    public override ComponentVendor Vendor => MonitorVendor;

    public Event<MonitorAlarm> Alarm => Event<MonitorAlarm>();

    public Event Silenced => Event();

    public void Acknowledge() => MethodAsync();
}

public sealed class MonitorAlarm : JsObject
{
    public ReadOnly<string> Bed => ReadOnly<string>();

    public Property<bool> Cancel => Property<bool>();

    public ReadOnly<string?> Note => ReadOnly<string?>();

    public ReadOnly<AlarmReading> Reading => ReadOnly<AlarmReading>();

    public AlarmPatient Patient => LiveObject<AlarmPatient>();

    public void Silence() => Method();
}

public sealed class AlarmPatient : JsObject
{
    public Property<string> Status => Property<string>();

    public void Page() => Method();
}

public sealed record AlarmReading(int Rate)
{
    public string Unit { get; init; } = "bpm";
}

public static class VitalsMonitorHtml
{
    public static Rendered<VitalsMonitor> VitalsMonitor<TModel>(this ReactivePlan<TModel> plan, string id)
        => plan.Render<VitalsMonitor>(id, Monitor(id, "current"));

    public static Rendered<VitalsMonitor> OlderVitalsMonitor<TModel>(this ReactivePlan<TModel> plan, string id)
        => plan.Render<VitalsMonitor>(id, Monitor(id, "older"));

    private static TagBuilder Monitor(string id, string build)
    {
        var monitor = new TagBuilder("section");
        monitor.Attributes["id"] = id;
        monitor.Attributes["data-vitals-monitor"] = build;
        return monitor;
    }
}
