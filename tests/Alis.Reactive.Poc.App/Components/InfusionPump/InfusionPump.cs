using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Alis.Reactive.Values;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

[Mirrors("Components/InfusionPump/infusion-pump", "CurrentInfusionPump")]
public sealed class InfusionPump : Component
{
    public static ComponentVendor PumpVendor { get; } = ComponentVendor.Define("pump", objectPath: "pump", subscribe: "addEventListener", unsubscribe: "removeEventListener");

    public override ComponentVendor Vendor => PumpVendor;

    public void Start() => Method();

    public void Prime() => Method();

    [SuppressMessage("Alis.Reactive.Contract", "ALIS017", Justification = "Drift on purpose: a_method_declared_to_return_nothing_that_returns_a_promise_stops_the_pipeline_before_its_next_step proves the browser stops on it.")]
    public void Calibrate() => Method();

    [SuppressMessage("Alis.Reactive.Contract", "ALIS018", Justification = "Drift on purpose: a_declared_method_the_pump_holds_as_text_stops_the_pipeline_before_its_next_step proves the browser stops on it.")]
    public void Alarm() => Method();

    public void SetRate(Arg<decimal> volume, Arg<string> unit) => Method([volume, unit]);

    public void SetRateInMillilitresPerHour(Arg<decimal> volume) => Method([volume, Fixed("ml/h")], name: "setRate");

    public void ShowLabels<TLabels>(TypedSource<TLabels> labels)
        where TLabels : IEnumerable<string>
        => Method([Pass<IEnumerable<string>, TLabels>(labels)]);

    public void Caption(Arg<string?> text) => Method([text]);

    public MethodResult<string> Summary() => Method<string>();

    public MethodResult<decimal> RateFor(Arg<string> medicine) => Method<decimal>([medicine]);

    [SuppressMessage("Alis.Reactive.Contract", "ALIS017", Justification = "Drift on purpose: a_method_declared_to_return_a_value_that_returns_a_promise_stops_the_pipeline_before_it_writes_the_value proves the browser stops on it.")]
    public MethodResult<decimal> Reading() => Method<decimal>();

    public void SelfTest() => MethodAsync();

    public MethodResult<int> Volume() => MethodAsync<int>();

    public MethodResult<string?> LastCheck() => MethodAsync<string?>();

    public MethodResult<PumpSettings> RemoteSettings() => MethodAsync<PumpSettings>();

    public void Note(Arg<string> text) => Method([text]);

    public MethodResult<string?> LastAlarm() => Method<string?>();

    public MethodResult<PumpSettings> Settings() => Method<PumpSettings>();

    [SuppressMessage("Alis.Reactive.Contract", "ALIS019", Justification = "Drift on purpose: a_returned_channel_that_reads_nothing_stops_the_pipeline_at_its_declared_member proves the browser stops on a channel the declaration requires and the pump returns as undefined.")]
    public PumpChannel Channel(Arg<int> position) => ReturnedObject<PumpChannel>([position]);

    public LiveObjects<PumpChannel> Channels() => ReturnedObjects<PumpChannel>();
}

public sealed class PumpChannel : JsObject
{
    public ReadOnly<string> Label => ReadOnly<string>();

    public void Pause() => Method();

    public MethodResult<bool> IsRunning() => Method<bool>();

    public PumpChannel Partner() => ReturnedObject<PumpChannel>();

    public MethodResult<bool> SelfCheck() => MethodAsync<bool>();

    public void Recalibrate() => MethodAsync();
}

public sealed record PumpSettings(PumpLimit Limit, string? Note);

public sealed record PumpLimit(int Volume);

public static class InfusionPumpHtml
{
    public static Rendered<InfusionPump, TModel> InfusionPump<TModel>(this ReactivePlan<TModel> plan, string id, string caption)
        => plan.Render<InfusionPump>(id, Pump(id, "current", caption));

    public static Rendered<InfusionPump, TModel> OlderInfusionPump<TModel>(this ReactivePlan<TModel> plan, string id, string caption)
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
