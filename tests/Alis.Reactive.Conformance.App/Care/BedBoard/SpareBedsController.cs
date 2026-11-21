using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class SpareBedsController : Controller
{
    [HttpGet(CarePages.SpareBeds)]
    public IActionResult OfTheWing() => View("/Care/BedBoard/SpareBeds.cshtml", new SpareBeds(
        [new SpareBedResident("Ann", "Active", 3), new SpareBedResident("Bo", "Away", 3)]));

    [HttpGet(CarePages.SpareBedsAllActive)]
    public IActionResult OfTheWingAllActive() => View("/Care/BedBoard/SpareBeds.cshtml", new SpareBeds(
        [new SpareBedResident("Ann", "Active", 3), new SpareBedResident("Bo", "Active", 3)]));
}

public sealed record SpareBeds(IReadOnlyList<SpareBedResident> Residents);

public sealed record SpareBedResident(string Name, string Status, int Beds);
