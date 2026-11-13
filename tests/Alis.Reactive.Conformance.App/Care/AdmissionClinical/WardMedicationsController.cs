using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardMedicationsController : Controller
{
    [HttpGet(CarePages.WardMedications)]
    public IActionResult OfTheWard() => View("/Care/AdmissionClinical/WardMedications.cshtml", new WardMedications());

    [HttpGet(WardMedicationsEndpoints.Medications)]
    public IActionResult Medications() => Json(new[] { new Medication("Warfarin"), new Medication("Metformin") });
}

public sealed class WardMedications
{
    public string? CurrentMedication { get; set; }
}

public static class WardMedicationsEndpoints
{
    public const string Medications = CarePages.WardMedications + "/medications";
}

public static class WardMedicationLists
{
    public static readonly AppEvent Load = AppEvent.Declare();
}
