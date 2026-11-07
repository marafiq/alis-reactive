using System;
using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Alis.Reactive.Values;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

public sealed class DoseChart : Component
{
    public static ComponentVendor ChartVendor { get; } = ComponentVendor.Define("dosechart", objectPath: "chart", subscribe: "on", unsubscribe: "off");

    public override ComponentVendor Vendor => ChartVendor;

    public Callback<ChartTick, string> TickLabel => Callback<ChartTick, string>();

    public Callback<TickValue, string> ValueAccessor => Callback<TickValue, string>();

    public void SortTicks(Func<ChartTick, TypedSource<int>> key) => Method([Answer(key)]);

    public MethodResult<string> Unit() => Method<string>();

    public MethodResult<string> Legend() => MethodAsync<string>();
}

public sealed class ChartTick : JsObject
{
    public ReadOnly<string> Label => ReadOnly<string>();

    public ReadOnly<int> Doses => ReadOnly<int>();
}

public sealed class TickValue : JsObject
{
    public ReadOnly<string> Field => Argument<string>(0);

    public ReadOnly<TickData> Tick => Argument<TickData>(1);
}

public sealed record TickData(string Label, int Doses);

public static class DoseChartHtml
{
    public static Rendered<DoseChart> DoseChart<TModel>(this ReactivePlan<TModel> plan, string id)
    {
        var chart = new TagBuilder("section");
        chart.Attributes["id"] = id;
        chart.Attributes["data-dose-chart"] = "";
        return plan.Render<DoseChart>(id, chart);
    }
}
