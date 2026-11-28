using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AdmissionDeskController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.AdmissionDesk)]
    public IActionResult OfAResident() => View("/Care/Admission/AdmissionDesk.cshtml", new AdmissionRequest());

    [HttpPost(AdmissionDeskEndpoints.Admissions)]
    public IActionResult Admit([FromForm] AdmissionRequest admission)
    {
        if (string.IsNullOrEmpty(admission.Name)) ModelState.AddModelError(nameof(AdmissionRequest.Name), "Name the resident.");
        if (ModelState.IsValid) return Content($"Admitted {admission.Name}", PlainText);
        return ValidationProblem(ModelState);
    }
}

public sealed class AdmissionRequest
{
    public string? Name { get; set; }
}

public static class AdmissionDeskEndpoints
{
    public const string Admissions = CarePages.Area + "admission/admission-desk/admissions";
}

public static class AdmissionDeskButtons
{
    public static readonly AppEvent Admit = AppEvent.Declare();
}
