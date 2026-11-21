using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class FluidRatioController : Controller
{
    [HttpGet(RefusalPages.FluidRatioAtZero)]
    public IActionResult AtZero() => View("/Refusals/Conditions/FluidRatioAtZero.cshtml", new FluidRatioCheck("Ada Brooks"));

    [HttpGet(ConditionEndpoints.FluidRatios)]
    public IActionResult Ratios() => Json(new List<FluidRatio>
    {
        new("Ada Brooks", -0.0),
        new("Lena Ortiz", 0.8),
    });
}

public sealed record FluidRatioCheck(string Resident);

public sealed record FluidRatio(string Resident, double Ratio);

public static class FluidRatioChecks
{
    public static readonly AppEvent Check = AppEvent.Declare();
}
