using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class PainReliefController : Controller
{
    [HttpGet(CarePages.PainReliefCheckedInItsTerms)]
    public IActionResult CheckedInItsTerms() => View("/Care/Record/PainReliefCheckedInItsTerms.cshtml", new PainRelief("Ada Hart", 0));

    [HttpGet(CarePages.PainReliefCheckedBeforeItsWhen)]
    public IActionResult CheckedBeforeItsWhen() => View("/Care/Record/PainReliefCheckedBeforeItsWhen.cshtml", new PainRelief("Ada Hart", 0));
}

public sealed record PainRelief(string Resident, int Pain);

public static class PainReliefs
{
    public static readonly AppEvent GiveParacetamol = AppEvent.Declare();

    public static readonly AppEvent CallTheDoctor = AppEvent.Declare();
}
