using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentNamesController : Controller
{
    [HttpGet(CarePages.ResidentNames)]
    public IActionResult OfTheWing() => View("/Care/BedBoard/ResidentNames.cshtml", new ResidentNames(ResidentNamesEndpoints.Residents));

    [HttpGet(CarePages.ResidentNamesRenamed)]
    public IActionResult OfTheWingRenamed() => View("/Care/BedBoard/ResidentNames.cshtml", new ResidentNames(ResidentNamesEndpoints.RenamedResidents));

    [HttpGet(ResidentNamesEndpoints.Residents)]
    public IActionResult Residents() => Json(new[] { new NamedResident("Ann"), new NamedResident("<img src=x onerror=alert(1)>") });

    [HttpGet(ResidentNamesEndpoints.RenamedResidents)]
    public IActionResult RenamedResidents() => Json(new[] { new NamedResident("Ann"), new NamedResident("Bo") });
}

public sealed record ResidentNames(string ResidentsEndpoint);

public sealed record NamedResident(string Name);

public static class ResidentNamesEndpoints
{
    public const string Residents = CarePages.ResidentNames + "/residents";

    public const string RenamedResidents = CarePages.ResidentNamesRenamed + "/residents";
}
