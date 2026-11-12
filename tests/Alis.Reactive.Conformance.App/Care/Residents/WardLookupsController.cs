using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardLookupsController : Controller
{
    [HttpGet(CarePages.WardLookupsOfEastAndNorth)]
    public IActionResult OfEastAndNorth() => View("/Care/Residents/WardLookups.cshtml", new WardLookups(["East", "North"], [RoundWing.East, RoundWing.North]));

    [HttpGet(CarePages.WardLookupsOfEast)]
    public IActionResult OfEast() => View("/Care/Residents/WardLookups.cshtml", new WardLookups(["East"], [RoundWing.East]));

    [HttpGet(WardLookupEndpoints.Doses)]
    public IActionResult Doses() => Json(new List<LookedUpDose>
    {
        new("Aspirin", "East", RoundWing.East, [8, 20], ["A1"]),
        new("Morphine", "West", RoundWing.West, [9], ["M1", "M2"]),
        new("Heparin", "North", RoundWing.North, [6, 18], ["H1"]),
        new("Insulin", "East", RoundWing.East, [7, 12, 17], ["I1"]),
    });
}

public sealed record WardLookups(IReadOnlyList<string> Wards, IReadOnlyList<RoundWing> Wings);

public sealed record LookedUpDose(string Medicine, string Ward, RoundWing Wing, List<int> Hours, string[] Codes);

public enum RoundWing
{
    East,
    West,
    North,
}

public static class WardLookupEndpoints
{
    private const string Root = CarePages.Area + "residents/ward-lookups/";

    public const string Doses = Root + "doses";
}
