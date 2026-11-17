using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardOccupancyController : Controller
{
    private const string Occupancy = "/Care/BedBoard/WardOccupancy.cshtml";

    [HttpGet(CarePages.WardOccupancy)]
    public IActionResult OfTheWards() => View(Occupancy, new WardOccupancy([new OccupancyEntry("North", 1), new OccupancyEntry("North", 1), new OccupancyEntry("East", 1)]));

    [HttpGet(CarePages.WardOccupancyAfterAMove)]
    public IActionResult AfterAMove() => View(Occupancy, new WardOccupancy([new OccupancyEntry("North", 1), new OccupancyEntry("East", 1), new OccupancyEntry("East", 1)]));
}

public sealed record WardOccupancy(IReadOnlyList<OccupancyEntry> Residents);

public sealed record OccupancyEntry(string Ward, [property: JsonPropertyName("headcount")] int Residents);
