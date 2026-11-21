using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class HomeWardsController : Controller
{
    [HttpGet(CarePages.HomeWards)]
    public IActionResult OfTheHome() => View("/Care/AdmissionClinical/HomeWards.cshtml", new HomeWards());

    [HttpGet(HomeWardsEndpoints.Wards)]
    public IActionResult Wards() => Json(new[] { new HomeWard("Maple", "M1"), new HomeWard("Cedar", "C2") });
}

public sealed class HomeWards
{
    public string? WardCode { get; set; }
}

public static class HomeWardsEndpoints
{
    public const string Wards = CarePages.HomeWards + "/wards";
}

public static class HomeWardLists
{
    public static readonly AppEvent Load = AppEvent.Declare();
}
