using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WeightCheckController : Controller
{
    [HttpGet(CarePages.WeightCheckOfAnUnweighedResident)]
    public IActionResult OfAnUnweighedResident() => View("/Care/Record/WeightCheck.cshtml", new WeightCheck("Ada Hart", WeightCheckEndpoints.UnweighedResident));

    [HttpGet(CarePages.WeightCheckOfAWeighedResident)]
    public IActionResult OfAWeighedResident() => View("/Care/Record/WeightCheck.cshtml", new WeightCheck("Ben Okoro", WeightCheckEndpoints.WeighedResident));

    [HttpGet(WeightCheckEndpoints.UnweighedResident)]
    public IActionResult TheUnweighedResident() => Json(new WeighedResident("Ada Hart", null));

    [HttpGet(WeightCheckEndpoints.WeighedResident)]
    public IActionResult TheWeighedResident() => Json(new WeighedResident("Ben Okoro", 60));
}

public sealed record WeightCheck(string Resident, string Weighing);

public sealed record WeighedResident(string Name, int? Weight);

public static class WeightCheckEndpoints
{
    private const string Root = CarePages.Area + "record/weight-check/";

    public const string UnweighedResident = Root + "unweighed/resident";

    public const string WeighedResident = Root + "weighed/resident";
}

public static class WeightChecks
{
    public static readonly AppEvent Check = AppEvent.Declare();
}
