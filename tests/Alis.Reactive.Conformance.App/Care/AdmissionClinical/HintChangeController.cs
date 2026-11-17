using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class HintChangeController : Controller
{
    [HttpGet(CarePages.HintChange)]
    public IActionResult OfHelensIntake() => View("/Care/AdmissionClinical/HintChange.cshtml", new HintChange());
}

public sealed class HintChange
{
    public int[]? AllergyCodes { get; set; }

    public string Note { get; set; } = "";
}

public static class HintChanges
{
    public static readonly AppEvent PenicillinOnly = AppEvent.Declare();

    public static readonly AppEvent ClearHint = AppEvent.Declare();
}
