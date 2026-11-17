using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CarePlanDraftController : Controller
{
    [HttpGet(CarePages.CarePlanDraft)]
    public IActionResult OfAResident() => View("/Care/AdmissionClinical/CarePlanDraft.cshtml", new CarePlanDraft());
}

public sealed class CarePlanDraft
{
    public IFormFile? CarePlan { get; set; }
}
