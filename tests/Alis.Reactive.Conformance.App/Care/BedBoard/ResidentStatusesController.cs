using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentStatusesController : Controller
{
    [HttpGet(CarePages.ResidentStatuses)]
    public IActionResult OfTheWing() => View("/Care/BedBoard/ResidentStatuses.cshtml", new ResidentStatuses(
        [new StatusedResident("Ann", "Active"), new StatusedResident("Bo", "On leave")]));

    [HttpGet(CarePages.ResidentStatusesAllActive)]
    public IActionResult OfTheWingAllActive() => View("/Care/BedBoard/ResidentStatuses.cshtml", new ResidentStatuses(
        [new StatusedResident("Ann", "Active"), new StatusedResident("Bo", "Active")]));
}

public sealed record ResidentStatuses(IReadOnlyList<StatusedResident> Residents);

public sealed record StatusedResident(string Name, string Status);
