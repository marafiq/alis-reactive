using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentsByWingController : Controller
{
    [HttpGet(CarePages.ResidentsOfTheEastWing)]
    public IActionResult OfTheEastWing() => View("/Care/Residents/ResidentsByWing.cshtml", new ResidentsByWing { Wing = "East", Wings = TheWings() });

    [HttpGet(CarePages.ResidentsOfTheWestWing)]
    public IActionResult OfTheWestWing() => View("/Care/Residents/ResidentsByWing.cshtml", new ResidentsByWing { Wing = "West", Wings = TheWings() });

    private static List<SelectListItem> TheWings() => [new SelectListItem("North", "North"), new SelectListItem("East", "East"), new SelectListItem("West", "West")];
}

public sealed class ResidentsByWing
{
    public string Wing { get; init; } = "";

    public IReadOnlyList<SelectListItem> Wings { get; init; } = [];
}
