using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AllergyIntakeController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.AllergyIntake)]
    public IActionResult OfAResident()
        => View("/Care/AdmissionClinical/AllergyIntake.cshtml", new AllergyIntake { Allergies = [new Allergy("Penicillin", 4), new Allergy("Latex", 9)] });

    [HttpPost(AllergyIntakeEndpoints.Intakes)]
    public IActionResult Save([FromForm] AllergyIntake intake) => Content($"Allergies: {string.Join(", ", intake.AllergyCodes ?? [])}", PlainText);
}

public sealed class AllergyIntake
{
    public int[]? AllergyCodes { get; set; }

    public IReadOnlyList<Allergy> Allergies { get; init; } = [];
}

public sealed record Allergy([property: JsonPropertyName("label")] string DisplayLabel, int AllergyCode);

public static class AllergyIntakeEndpoints
{
    public const string Intakes = CarePages.AllergyIntake + "/intakes";
}

public static class AllergyIntakes
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
