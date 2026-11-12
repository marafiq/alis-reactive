using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class DoseStrengthController : Controller
{
    [HttpGet(RefusalPages.DoseOfAStrengthWrittenAsText)]
    public IActionResult WrittenAsText() => View("/Refusals/Conditions/DoseStrengthWrittenAsText.cshtml", new StrengthCheck("Aspirin"));

    [HttpGet(RefusalPages.DoseOfAStrengthWrittenAsANumber)]
    public IActionResult WrittenAsANumber() => View("/Refusals/Conditions/DoseStrengthWrittenAsANumber.cshtml", new StrengthCheck("Aspirin"));

    [HttpGet(ConditionEndpoints.DosesOfATextStrength)]
    public IActionResult DosesOfATextStrength() => Json(new List<DoseOfATextStrength>
    {
        new("Aspirin", 10),
        new("Digoxin", 2),
    });

    [HttpGet(ConditionEndpoints.DosesOfAStrength)]
    public IActionResult DosesOfAStrength() => Json(new List<DoseOfAStrength>
    {
        new("Aspirin", 10),
        new("Digoxin", 2),
    });
}

public sealed record StrengthCheck(string Medicine);

public sealed record DoseOfATextStrength(string Medicine, [property: JsonNumberHandling(JsonNumberHandling.WriteAsString)] int Strength);

public sealed record DoseOfAStrength(string Medicine, int Strength);

public static class StrengthChecks
{
    public static readonly AppEvent Check = AppEvent.Declare();
}
