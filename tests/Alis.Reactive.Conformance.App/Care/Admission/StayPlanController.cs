using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class StayPlanController : Controller
{
    [HttpGet(CarePages.Stay)]
    public IActionResult OfAResident() => View("/Care/Admission/StayPlan.cshtml", new StayPlan("Ada Hart"));
}

public sealed record StayPlan(string Resident);

public static class StayPlans
{
    public static readonly AppEvent Check = AppEvent.Declare();
}
