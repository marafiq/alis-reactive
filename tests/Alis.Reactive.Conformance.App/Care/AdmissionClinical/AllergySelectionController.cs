using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AllergySelectionController : Controller
{
    private const string Page = "/Care/AdmissionClinical/AllergySelection.cshtml";

    [HttpGet(CarePages.AllergySelectionEmpty)]
    public IActionResult HoldingNone() => View(Page, new AllergySelection());

    [HttpGet(CarePages.AllergySelectionOfLatex)]
    public IActionResult OfLatex() => View(Page, new AllergySelection { AllergyCodes = [9] });
}

public sealed class AllergySelection
{
    public int[]? AllergyCodes { get; set; }
}

public static class AllergySelections
{
    public static readonly AppEvent Every = AppEvent.Declare();

    public static readonly AppEvent None = AppEvent.Declare();
}
