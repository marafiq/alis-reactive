using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class PainEscalationController : Controller
{
    [HttpGet(CarePages.PainEscalation)]
    public IActionResult OfAResident() => View("/Care/Record/PainEscalation.cshtml", new PainEscalation("Ada Hart", 0));
}

public sealed record PainEscalation(string Resident, int Pain);

public static class PainEscalations
{
    public static readonly AppEvent Assess = AppEvent.Declare();

    public static readonly AppEvent NurseFree = AppEvent.Declare();

    public static readonly AppEvent NurseBusy = AppEvent.Declare();
}
