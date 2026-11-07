using System.Collections.Generic;
using System.Text.Json;
using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

[Mirrors("Components/MonitorDesk/monitor-desk", "DeskMonitor")]
public sealed class DeskMonitor : Component
{
    public static ComponentVendor DeskMonitorVendor { get; } = ComponentVendor.Define("desk-monitor", objectPath: "monitor", subscribe: "on", unsubscribe: "off");

    public override ComponentVendor Vendor => DeskMonitorVendor;

    public ReadOnly<string> Resident => ReadOnly<string>();

    public Property<string> Status => Property<string>();

    public void Page() => Method();

    public Event<DeskAlarm> Alarm => Event<DeskAlarm>();
}

public sealed class DeskAlarm : JsObject
{
    public ReadOnly<string> Resident => ReadOnly<string>();
}

[Mirrors("Components/MonitorDesk/monitor-desk", "MonitorDesk")]
public sealed class MonitorDesk : Component
{
    public static ComponentVendor DeskVendor { get; } = ComponentVendor.Define("monitor-desk", objectPath: "desk", subscribe: "on", unsubscribe: "off");

    public override ComponentVendor Vendor => DeskVendor;

    public Event<DeskMonitor> Picked => Event<DeskMonitor>();

    public Event<MonitorHanded> Handed => Event<MonitorHanded>();
}

public sealed class MonitorHanded : JsObject
{
    public DeskMonitor Monitor => LiveObject<DeskMonitor>();
}

public static class MonitorDeskHtml
{
    public static Rendered<DeskMonitor, TModel> DeskMonitor<TModel>(this ReactivePlan<TModel> plan, string id, string resident)
    {
        var host = new TagBuilder("div");
        host.Attributes["id"] = id;
        host.Attributes["data-desk-monitor"] = "";
        host.Attributes["data-resident"] = resident;
        return plan.Render<DeskMonitor>(id, host);
    }

    public static Rendered<MonitorDesk, TModel> MonitorDesk<TModel>(this ReactivePlan<TModel> plan, string id, IReadOnlyList<string> monitors)
    {
        var host = new TagBuilder("div");
        host.Attributes["id"] = id;
        host.Attributes["data-monitor-desk"] = JsonSerializer.Serialize(monitors, JsonSerializerOptions.Web);
        return plan.Render<MonitorDesk>(id, host);
    }
}
