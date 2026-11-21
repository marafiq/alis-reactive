using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CensusController : Controller
{
    [HttpGet(CarePages.CensusOfTheEastWard)]
    public IActionResult OfTheEastWard() => View("/Care/Residents/Census.cshtml", new Census("East"));

    [HttpGet(CarePages.CensusOfTheWestWard)]
    public IActionResult OfTheWestWard() => View("/Care/Residents/Census.cshtml", new Census("West"));

    [HttpGet(ResidentsEndpoints.CensusResidents)]
    public IActionResult Residents() => Json(new List<CensusResident>
    {
        new("Ada Hart", "East"),
        new("Ben Okoro", "West"),
        new("Cora Diaz", "West"),
        new("Dev Patel", "East"),
        new("Eli Stone", "West"),
    });
}

public sealed record Census(string Ward);

public sealed record CensusResident(string Name, string Ward);
