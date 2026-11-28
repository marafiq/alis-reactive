using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class FluidTargetCheckController : Controller
{
    [HttpGet(CarePages.FluidTargetCheck)]
    public IActionResult OfAResident() => View("/Care/Record/FluidTargetCheck.cshtml", new FluidTargetCheck("Ada Hart", 0, 1000));
}

public sealed record FluidTargetCheck(string Resident, int Intake, int? Target);

public static class FluidTargetChecks
{
    public static readonly AppEvent Check = AppEvent.Declare();
}
