using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class UnitOccupancyController : Controller
{
    private const string Occupancy = "/Care/BedBoard/UnitOccupancy.cshtml";

    [HttpGet(CarePages.UnitOccupancy)]
    public IActionResult OfTheUnits() => View(Occupancy, new UnitOccupancy([new UnitEntry("North", 1), new UnitEntry("North", 1), new UnitEntry("East", 1)]));

    [HttpGet(CarePages.UnitOccupancyAfterAMove)]
    public IActionResult AfterAMove() => View(Occupancy, new UnitOccupancy([new UnitEntry("North", 1), new UnitEntry("East", 1), new UnitEntry("East", 1)]));
}

public sealed record UnitOccupancy(IReadOnlyList<UnitEntry> Residents);

public sealed record UnitEntry([property: JsonPropertyName("unit")] string Ward, int Beds);
