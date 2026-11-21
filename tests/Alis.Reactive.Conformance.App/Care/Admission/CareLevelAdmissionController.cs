using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CareLevelAdmissionController : Controller
{
    private const string TheResident = "Ada Hart";

    [HttpGet(CarePages.CareLevelAdmissionWithAnOlderPicker)]
    public IActionResult WithAnOlderPicker() => View("/Care/Admission/CareLevelAdmission.cshtml", new CareLevelAdmission(TheResident, ComponentVersion.Older, [DefaultHooks.SessionScript]));

    [HttpGet(CarePages.CareLevelAdmissionWithTheCurrentPicker)]
    public IActionResult WithTheCurrentPicker() => View("/Care/Admission/CareLevelAdmission.cshtml", new CareLevelAdmission(TheResident, ComponentVersion.Current, [DefaultHooks.SessionScript]));

    [HttpPost(CareLevelAdmissionEndpoints.Admissions)]
    public IActionResult Admit() => NoContent();

    [HttpPost(CareLevelAdmissionEndpoints.Beds)]
    public IActionResult AssignABed() => NoContent();
}

public sealed record CareLevelAdmission(string Resident, ComponentVersion Version, IReadOnlyList<string> Scripts) : IScriptedAdmissionPage;

public static class CareLevelAdmissionEndpoints
{
    public const string Admissions = CarePages.Area + "admission/care-level-admission/admissions";

    public const string Beds = CarePages.Area + "admission/care-level-admission/beds";
}

public static class CareLevelAdmissions
{
    public static readonly AppEvent Admit = AppEvent.Declare();
}
