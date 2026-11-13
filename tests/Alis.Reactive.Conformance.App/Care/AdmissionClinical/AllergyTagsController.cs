using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AllergyTagsController : Controller
{
    [HttpGet(CarePages.AllergyTags)]
    public IActionResult OfAResident() => View("/Care/AdmissionClinical/AllergyTags.cshtml", new AllergyTags("Rose Alvarez"));
}

public sealed record AllergyTags(string Resident);

public static class AllergyTagChecks
{
    public static readonly AppEvent ShowSelected = AppEvent.Declare();
}
