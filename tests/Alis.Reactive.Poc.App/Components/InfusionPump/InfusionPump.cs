using System.Collections.Generic;
using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Alis.Reactive.Values;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

public sealed class InfusionPump : Component
{
    public static ComponentVendor PumpVendor { get; } = ComponentVendor.Define("pump", objectPath: "pump", subscribe: "addEventListener", unsubscribe: "removeEventListener");

    public override ComponentVendor Vendor => PumpVendor;

    public void Start() => Method();

    public void Prime() => Method();

    public void Calibrate() => Method();

    public void Alarm() => Method();

    public void SetRate(Arg<decimal> volume, Arg<string> unit) => Method([volume, unit]);

    public void SetRateInMillilitresPerHour(Arg<decimal> volume) => Method([volume, Fixed("ml/h")], name: "setRate");

    public void ShowLabels<TLabels>(TypedSource<TLabels> labels)
        where TLabels : IEnumerable<string>
        => Method([Pass<IEnumerable<string>, TLabels>(labels)]);

    public void Caption(Arg<string?> text) => Method([text]);

    public MethodResult<string> Summary() => Method<string>();

    public MethodResult<decimal> RateFor(Arg<string> medicine) => Method<decimal>([medicine]);

    public MethodResult<decimal> Reading() => Method<decimal>();

    public MethodResult<string?> LastAlarm() => Method<string?>();

    public MethodResult<PumpSettings> Settings() => Method<PumpSettings>();

    public PumpChannel Channel(Arg<int> position) => ReturnedObject<PumpChannel>([position]);

    public LiveObjects<PumpChannel> Channels() => ReturnedObjects<PumpChannel>();
}

public sealed class PumpChannel : JsObject
{
    public ReadOnly<string> Label => ReadOnly<string>();

    public void Pause() => Method();

    public MethodResult<bool> IsRunning() => Method<bool>();

    public PumpChannel Partner() => ReturnedObject<PumpChannel>();
}

public sealed record PumpSettings(PumpLimit Limit, string? Note);

public sealed record PumpLimit(int Volume);

public static class InfusionPumpHtml
{
    public static Rendered<InfusionPump> InfusionPump<TModel>(this ReactivePlan<TModel> plan, string id, string caption)
        => plan.Render<InfusionPump>(id, Pump(id, "current", caption));

    public static Rendered<InfusionPump> OlderInfusionPump<TModel>(this ReactivePlan<TModel> plan, string id, string caption)
        => plan.Render<InfusionPump>(id, Pump(id, "older", caption));

    private static TagBuilder Pump(string id, string build, string caption)
    {
        var pump = new TagBuilder("section");
        pump.Attributes["id"] = id;
        pump.Attributes["data-infusion-pump"] = build;
        pump.Attributes["data-caption"] = caption;
        return pump;
    }
}
