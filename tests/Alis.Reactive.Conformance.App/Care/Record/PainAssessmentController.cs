using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class PainAssessmentController : Controller
{
    [HttpGet(CarePages.PainAssessment)]
    public IActionResult OfAResident() => View("/Care/Record/PainAssessment.cshtml", new PainAssessment("Ada Brooks", 0));
}

public sealed record PainAssessment(string Resident, int Pain);

public static class PainAssessments
{
    public static readonly AppEvent Assess = AppEvent.Declare();
}
