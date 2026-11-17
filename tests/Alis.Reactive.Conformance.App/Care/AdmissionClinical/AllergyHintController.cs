using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AllergyHintController : Controller
{
    private const string Page = "/Care/AdmissionClinical/AllergyHint.cshtml";

    [HttpGet(CarePages.AllergyHintGiven)]
    public IActionResult Given() => View(Page, new AllergyHint { Hint = "Choose Helen's allergies" });

    [HttpGet(CarePages.AllergyHintUnset)]
    public IActionResult Unset() => View(Page, new AllergyHint());
}

public sealed class AllergyHint
{
    public int[]? AllergyCodes { get; set; }

    public string? Hint { get; init; }
}

public static class AllergyHints
{
    public static readonly AppEvent Show = AppEvent.Declare();
}
