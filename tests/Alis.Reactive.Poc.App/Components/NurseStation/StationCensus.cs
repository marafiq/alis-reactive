using System.Collections.Generic;
using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

public sealed class StationCensus : Component
{
    public static ComponentVendor CensusVendor { get; } = ComponentVendor.Define("census", objectPath: "census", subscribe: "addEventListener", unsubscribe: "removeEventListener");

    public override ComponentVendor Vendor => CensusVendor;

    public Property<int> Count => Property<int>();

    public Property<IReadOnlyList<string>> Names => Property<IReadOnlyList<string>>();

    public Property<IReadOnlyList<int>> Counts => Property<IReadOnlyList<int>>();
}

public static class StationCensusHtml
{
    public static Rendered<StationCensus> StationCensus<TModel>(this ReactivePlan<TModel> plan, string id)
    {
        var census = new TagBuilder("section");
        census.Attributes["id"] = id;
        census.Attributes["data-station-census"] = "";
        return plan.Render<StationCensus>(id, census);
    }
}
