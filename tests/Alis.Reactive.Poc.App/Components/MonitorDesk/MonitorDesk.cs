using System.Collections.Generic;
using System.Text.Json;
using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

[Mirrors("Components/MonitorDesk/monitor-desk", "DeskMonitor")]
public sealed class DeskMonitor : Component
{
    public static ComponentVendor DeskMonitorVendor { get; } = ComponentVendor.Define("desk-monitor", objectPath: "monitor", subscribe: "on", unsubscribe: "off", elementPath: "element");

    public override ComponentVendor Vendor => DeskMonitorVendor;

    public ReadOnly<string> Resident => ReadOnly<string>();

    public Property<string> Status => Property<string>();

    public void Page() => Method();

    public MethodResult<string> Check() => MethodAsync<string>();

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

[Mirrors("Components/MonitorDesk/monitor-desk", "DeskMonitor")]
public sealed class OlderDeskMonitor : Component
{
    public static ComponentVendor OlderDeskMonitorVendor { get; } = ComponentVendor.Define("older-desk-monitor", objectPath: "monitor", subscribe: "on", unsubscribe: "off");

    public override ComponentVendor Vendor => OlderDeskMonitorVendor;

    public Event<DeskAlarm> Alarm => Event<DeskAlarm>();
}

[Mirrors("Components/MonitorDesk/monitor-desk", "MonitorDesk")]
public sealed class OlderMonitorDesk : Component
{
    public static ComponentVendor OlderDeskVendor { get; } = ComponentVendor.Define("older-monitor-desk", objectPath: "desk", subscribe: "on", unsubscribe: "off");

    public override ComponentVendor Vendor => OlderDeskVendor;

    public Event<OlderMonitorHanded> Handed => Event<OlderMonitorHanded>();
}

public sealed class OlderMonitorHanded : JsObject
{
    public OlderDeskMonitor Monitor => LiveObject<OlderDeskMonitor>();
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
        => plan.Render<MonitorDesk>(id, DeskHost(id, monitors));

    public static Rendered<OlderMonitorDesk, TModel> OlderMonitorDesk<TModel>(this ReactivePlan<TModel> plan, string id, IReadOnlyList<string> monitors)
        => plan.Render<OlderMonitorDesk>(id, DeskHost(id, monitors));

    private static TagBuilder DeskHost(string id, IReadOnlyList<string> monitors)
    {
        var host = new TagBuilder("div");
        host.Attributes["id"] = id;
        host.Attributes["data-monitor-desk"] = JsonSerializer.Serialize(monitors, JsonSerializerOptions.Web);
        return host;
    }
}
