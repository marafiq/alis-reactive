using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AdmissionStepsController : Controller
{
    private const int Medications = 1;
    private const int Room = 2;

    [HttpGet(CarePages.AdmissionStepsAtMedications)]
    public IActionResult AtMedications() => View("/Care/AdmissionClinical/AdmissionSteps.cshtml", new AdmissionProgress(Medications));

    [HttpGet(CarePages.AdmissionStepsAtRoom)]
    public IActionResult AtRoom() => View("/Care/AdmissionClinical/AdmissionSteps.cshtml", new AdmissionProgress(Room));
}

public sealed record AdmissionProgress(int ActiveStep);

public static class AdmissionStepChecks
{
    public static readonly AppEvent WhichStep = AppEvent.Declare();
}
