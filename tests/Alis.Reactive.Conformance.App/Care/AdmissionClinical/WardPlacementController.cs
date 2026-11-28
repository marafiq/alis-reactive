using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardPlacementController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.WardPlacement)]
    public IActionResult OfAResident()
        => View("/Care/AdmissionClinical/WardPlacement.cshtml", new WardPlacement { Wards = [new HomeWard("Maple", "M1"), new HomeWard("Cedar", "C2")] });

    [HttpPost(WardPlacementEndpoints.Placements)]
    public IActionResult Save([FromForm] WardPlacement placement) => Content($"Ward: {placement.WardCode}", PlainText);
}

public sealed class WardPlacement
{
    public string? WardCode { get; set; }

    public IReadOnlyList<HomeWard> Wards { get; init; } = [];
}

public sealed record HomeWard([property: JsonPropertyName("label")] string DisplayLabel, string WardCode);

public static class WardPlacementEndpoints
{
    public const string Placements = CarePages.WardPlacement + "/placements";
}

public static class WardPlacements
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
