using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

public sealed class NurseStation : Component
{
    public static ComponentVendor StationVendor { get; } = ComponentVendor.Define("station", objectPath: "station", subscribe: "addEventListener", unsubscribe: "removeEventListener");

    public override ComponentVendor Vendor => StationVendor;

    public LiveObjects<StationBed> Beds => LiveObjects<StationBed>();

    public ReadOnly<StationSchedule> Schedule => ReadOnly<StationSchedule>();
}

public sealed class StationBed : JsObject
{
    public ReadOnly<string> Resident => ReadOnly<string>();

    public ReadOnly<bool> Occupied => ReadOnly<bool>();

    public Property<string> Status => Property<string>();

    public LiveObjects<BedAlert> Alerts => LiveObjects<BedAlert>();
}

public sealed class BedAlert : JsObject
{
    public ReadOnly<string> Reason => ReadOnly<string>();
}

public sealed record StationSchedule(List<CareRound> Rounds);

public sealed record CareRound(string Time, bool Due, [property: JsonPropertyName("carer_on_round")] Carer? Carer);

public sealed record Carer(string Name)
{
    public int? Grade { get; init; }
}

public sealed record BedSeed(string Resident, IReadOnlyList<string> Alerts);

public sealed class StationBuild
{
    public static StationBuild Current { get; } = new("current");

    public static StationBuild Older { get; } = new("older");

    private StationBuild(string text)
    {
        Text = text;
    }

    internal string Text { get; }
}

public static class NurseStationHtml
{
    public static Rendered<NurseStation, TModel> NurseStation<TModel>(this ReactivePlan<TModel> plan, string id, StationBuild build, IReadOnlyList<BedSeed> beds, StationSchedule? schedule)
    {
        var station = new TagBuilder("section");
        station.Attributes["id"] = id;
        station.Attributes["data-nurse-station"] = build.Text;
        station.Attributes["data-beds"] = JsonSerializer.Serialize(beds, JsonSerializerOptions.Web);
        if (schedule is not null) station.Attributes["data-schedule"] = JsonSerializer.Serialize(schedule, JsonSerializerOptions.Web);
        return plan.Render<NurseStation>(id, station);
    }
}
