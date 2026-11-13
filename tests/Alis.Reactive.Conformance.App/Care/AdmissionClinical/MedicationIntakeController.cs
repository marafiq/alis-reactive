using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class MedicationIntakeController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.MedicationIntake)]
    public IActionResult OfAResident()
        => View("/Care/AdmissionClinical/MedicationIntake.cshtml", new MedicationIntake { Medications = [new Medication("Warfarin"), new Medication("Metformin")] });

    [HttpPost(MedicationIntakeEndpoints.Intakes)]
    public IActionResult Save([FromForm] MedicationIntake intake) => Content($"Medication: {intake.CurrentMedication}", PlainText);
}

public sealed class MedicationIntake
{
    public string? CurrentMedication { get; set; }

    public IReadOnlyList<Medication> Medications { get; init; } = [];
}

public sealed record Medication([property: JsonPropertyName("label")] string DisplayLabel);

public static class MedicationIntakeEndpoints
{
    public const string Intakes = CarePages.MedicationIntake + "/intakes";
}

public static class MedicationIntakes
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
