using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentWardsController : Controller
{
    [HttpGet(CarePages.ResidentWards)]
    public IActionResult OfTheWing() => View("/Care/BedBoard/ResidentWards.cshtml", new ResidentWards(ResidentWardsEndpoints.Residents));

    [HttpGet(CarePages.ResidentWardsAllAssigned)]
    public IActionResult OfTheWingAllAssigned() => View("/Care/BedBoard/ResidentWards.cshtml", new ResidentWards(ResidentWardsEndpoints.AssignedResidents));

    [HttpGet(ResidentWardsEndpoints.Residents)]
    public IActionResult Residents() => Json(new object[]
    {
        new WardedResident("Ann", "North"),
        new WardedResident("Bo", null),
        new { Name = "Cy" },
    });

    [HttpGet(ResidentWardsEndpoints.AssignedResidents)]
    public IActionResult AssignedResidents() => Json(new[]
    {
        new WardedResident("Ann", "North"),
        new WardedResident("Bo", "South"),
        new WardedResident("Cy", "East"),
    });
}

public sealed record ResidentWards(string ResidentsEndpoint);

public sealed record WardedResident(string Name, string? Ward);

public static class ResidentWardsEndpoints
{
    public const string Residents = CarePages.ResidentWards + "/residents";

    public const string AssignedResidents = CarePages.ResidentWardsAllAssigned + "/residents";
}
