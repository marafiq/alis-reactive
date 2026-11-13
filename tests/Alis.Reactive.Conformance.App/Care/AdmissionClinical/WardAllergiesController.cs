using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardAllergiesController : Controller
{
    [HttpGet(CarePages.WardAllergies)]
    public IActionResult OfTheWard() => View("/Care/AdmissionClinical/WardAllergies.cshtml", new WardAllergies());

    [HttpGet(WardAllergiesEndpoints.Allergies)]
    public IActionResult Allergies() => Json(new[] { new Allergy("Penicillin", 4), new Allergy("Latex", 9) });
}

public sealed class WardAllergies
{
    public int[]? AllergyCodes { get; set; }
}

public static class WardAllergiesEndpoints
{
    public const string Allergies = CarePages.WardAllergies + "/allergies";
}

public static class WardAllergyLists
{
    public static readonly AppEvent Load = AppEvent.Declare();
}
