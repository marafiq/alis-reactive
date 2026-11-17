using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AllergyChangesController : Controller
{
    [HttpGet(CarePages.AllergySource)]
    public IActionResult Source() => View("/Care/AdmissionClinical/AllergySource.cshtml", new AllergySource());

    [HttpGet(CarePages.AllergyTagging)]
    public IActionResult Tagging() => View("/Care/AdmissionClinical/AllergyTagging.cshtml", new AllergyTagging());

    [HttpGet(CarePages.PeanutCheck)]
    public IActionResult Peanut() => View("/Care/AdmissionClinical/PeanutCheck.cshtml", new PeanutCheck { AllergyCodes = [7] });
}

public sealed class AllergySource
{
    public int[]? AllergyCodes { get; set; }
}

public sealed class AllergyTagging
{
    public int[]? AllergyCodes { get; set; }
}

public sealed class PeanutCheck
{
    public int[]? AllergyCodes { get; set; }
}

public static class AllergySources
{
    public static readonly AppEvent LoadFromDischarge = AppEvent.Declare();
}
