using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class FusionDischargeController : Controller
{
    [HttpGet(CarePages.FusionDischarge)]
    public IActionResult OfAResident() => View("/Care/Shell/FusionDischarge.cshtml", new FusionDischarge("Discharge Ada?"));

    [HttpGet(CarePages.FusionDischargeInSpanish)]
    public IActionResult InSpanish() => View("/Care/Shell/FusionDischargeInSpanish.cshtml", new FusionDischargeInSpanish("Ada"));
}

public sealed record FusionDischarge(string Question);

public sealed record FusionDischargeInSpanish(string Resident);

public static class FusionDischargeButtons
{
    public static readonly AppEvent Discharge = AppEvent.Declare();
}
