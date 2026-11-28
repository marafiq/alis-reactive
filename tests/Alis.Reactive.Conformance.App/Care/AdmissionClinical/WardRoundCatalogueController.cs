using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.Declarations;
using Alis.Reactive.Fusion.FusionComponents;
using Alis.Reactive.Plugins;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardRoundCatalogueController : Controller
{
    [HttpGet(CarePages.WardRoundCatalogue)]
    public IActionResult Directory() => View("/Care/AdmissionClinical/WardRoundCatalogue.cshtml", new WardRoundCatalogue());
}

public sealed class WardRoundCatalogue
{
    public string? DirectoryCode { get; set; }
    public string? AvailabilityCode { get; set; } = "M1";
    public string? Route { get; set; } = "Lift";
    public double? Interval { get; set; } = 0;
    public bool? Escort { get; set; } = false;
    public string? RouteRelease { get; set; }
    public double? IntervalRelease { get; set; }
    public bool? EscortRelease { get; set; }
    public string? WardSearch { get; set; }
    public string? LocalQuery { get; set; }
    public string? RouteSearch { get; set; }
    public double? IntervalSearch { get; set; }
    public bool? EscortSearch { get; set; }

    public IReadOnlyList<RoundWard> Day { get; } =
    [
        new("M1", "Maple", "M9", "Maple night", "North wing", "Overnight A", false),
        new("C2", "Cedar", "C8", "Cedar night", "South wing", "Overnight B", false)
    ];

    public IReadOnlyList<RoundWard> Night { get; } =
    [
        new("O3", "Oak", "N3", "Oak night", "South wing", "Night one", false),
        new("W4", "Willow", "N4", "Willow night", "North wing", "Night two", false)
    ];

    public IReadOnlyList<RoundWard> Directory => [.. Day, .. Night];
}

public sealed record RoundWard(string Code, string Name, [property: JsonPropertyName("handoverKey")] string NightCode, [property: JsonPropertyName("afterHoursLabel")] string NightName, string Wing, string NightWing, bool Held);

public static class WardRoundActions
{
    public static readonly AppEvent SearchGuidance = AppEvent.Declare();
    public static readonly AppEvent Day = AppEvent.Declare();
    public static readonly AppEvent Night = AppEvent.Declare();
    public static readonly AppEvent Closed = AppEvent.Declare();
    public static readonly AppEvent Print = AppEvent.Declare();
    public static readonly AppEvent SaveWard = AppEvent.Declare();
    public static readonly AppEvent WithdrawCode = AppEvent.Declare();
    public static readonly AppEvent WithdrawIndex = AppEvent.Declare();
    public static readonly AppEvent WithdrawRow = AppEvent.Declare();
    public static readonly AppEvent WithdrawUnknown = AppEvent.Declare();
    public static readonly AppEvent Known = AppEvent.Declare();
    public static readonly AppEvent Alternative = AppEvent.Declare();
    public static readonly AppEvent Missing = AppEvent.Declare();
    public static readonly AppEvent AppendRoutes = AppEvent.Declare();
    public static readonly AppEvent PriorityRoutes = AppEvent.Declare();
    public static readonly AppEvent AppendIntervals = AppEvent.Declare();
    public static readonly AppEvent PriorityIntervals = AppEvent.Declare();
    public static readonly AppEvent AppendEscort = AppEvent.Declare();
    public static readonly AppEvent PriorityEscort = AppEvent.Declare();
    public static readonly AppEvent Exact = AppEvent.Declare();
    public static readonly AppEvent Broad = AppEvent.Declare();
    public static readonly AppEvent End = AppEvent.Declare();
}

[Mirrors("Care/AdmissionClinical/ward-round-queries", "WardRoundQueries")]
public sealed class WardRoundQueries : Plugin
{
    public WardRoundQueries() : base("ward-round-queries")
    {
    }

    public FusionDropDownListQuery Itinerary => LiveObject<FusionDropDownListQuery>();
    public ReadOnly<FusionDropDownListQuery> FirstStop => ReadOnly<FusionDropDownListQuery>();
    public ReadOnly<FusionDropDownListQuery> StandbyStop => ReadOnly<FusionDropDownListQuery>();
    public ReadOnly<FusionDropDownListQuery> LastStop => ReadOnly<FusionDropDownListQuery>();
}
