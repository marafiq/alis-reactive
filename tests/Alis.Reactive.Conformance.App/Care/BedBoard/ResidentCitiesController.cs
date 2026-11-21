using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentCitiesController : Controller
{
    [HttpGet(CarePages.ResidentCities)]
    public IActionResult OfTheWing() => View("/Care/BedBoard/ResidentCities.cshtml", new ResidentCities(ResidentCitiesEndpoints.Residents));

    [HttpGet(CarePages.ResidentCitiesAllPlaced)]
    public IActionResult OfTheWingAllPlaced() => View("/Care/BedBoard/ResidentCities.cshtml", new ResidentCities(ResidentCitiesEndpoints.PlacedResidents));

    [HttpGet(ResidentCitiesEndpoints.Residents)]
    public IActionResult Residents() => Json(new object[]
    {
        new PlacedResident("Ann", new ResidentAddress("Leeds")),
        new { Name = "Bo", Address = (ResidentAddress?)null },
        new { Name = "Cy" },
    });

    [HttpGet(ResidentCitiesEndpoints.PlacedResidents)]
    public IActionResult PlacedResidents() => Json(new[]
    {
        new PlacedResident("Ann", new ResidentAddress("Leeds")),
        new PlacedResident("Bo", new ResidentAddress("York")),
        new PlacedResident("Cy", new ResidentAddress("Hull")),
    });
}

public sealed record ResidentCities(string ResidentsEndpoint);

public sealed record PlacedResident(string Name, ResidentAddress Address);

public sealed record ResidentAddress(string City);

public static class ResidentCitiesEndpoints
{
    public const string Residents = CarePages.ResidentCities + "/residents";

    public const string PlacedResidents = CarePages.ResidentCitiesAllPlaced + "/residents";
}
