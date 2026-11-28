using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

[Mirrors("Components/BedsideMonitor/bedside-monitor", "BedsideMonitor")]
public sealed class BedsideMonitor : Component
{
    public static ComponentVendor MonitorVendor { get; } = ComponentVendor.Define("bedside", objectPath: "monitor", subscribe: "on", unsubscribe: "off");

    public override ComponentVendor Vendor => MonitorVendor;

    public Event<BedsideAlarm> Alarm => Event<BedsideAlarm>();
}

public sealed class BedsideAlarm : JsObject
{
    public ReadOnly<string> Resident => ReadOnly<string>();
}

public static class BedsideMonitorHtml
{
    public static Rendered<BedsideMonitor, TModel> BedsideMonitor<TModel>(this ReactivePlan<TModel> plan, string id, string resident)
        => plan.Render<BedsideMonitor>(id, Host(id, resident));

    public static TagBuilder BedsideMonitorHost(string id, string resident) => Host(id, resident);

    private static TagBuilder Host(string id, string resident)
    {
        var host = new TagBuilder("div");
        host.Attributes["id"] = id;
        host.Attributes["data-bedside-monitor"] = "";
        host.Attributes["data-resident"] = resident;
        return host;
    }
}
