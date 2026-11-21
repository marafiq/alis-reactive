using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentRosterController : Controller
{
    [HttpGet(CarePages.ResidentRoster)]
    public IActionResult OfTheWing() => View("/Care/BedBoard/ResidentRoster.cshtml", new ResidentRoster("East"));

    [HttpGet(ResidentRosterEndpoints.Residents)]
    public IActionResult Residents() => Json(new[] { new Resident("Ann"), new Resident("Bo") });
}

public sealed record ResidentRoster(string Wing);

public sealed record Resident(string Name);

public static class ResidentRosterEndpoints
{
    public const string Residents = CarePages.ResidentRoster + "/residents";
}
