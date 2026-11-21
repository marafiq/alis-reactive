using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class ChartedDosesController : Controller
{
    [HttpGet(RefusalPages.ChartedDosesLookedUp)]
    public IActionResult LookedUp() => View("/Refusals/ArrayLambdas/ChartedDosesLookedUp.cshtml", new ChartedRound("Morning"));

    [HttpGet(RefusalPages.ChartedDosesCompared)]
    public IActionResult Compared() => View("/Refusals/ArrayLambdas/ChartedDosesCompared.cshtml", new ChartedRound("Morning"));

    [HttpGet(ArrayLambdaEndpoints.ChartedDoses)]
    public IActionResult Doses() => Json(new List<ChartedDose>
    {
        new("Aspirin", DoseStatus.Given),
        new("Digoxin", DoseStatus.Held),
        new("Insulin", DoseStatus.Held),
        new("Warfarin", DoseStatus.Due),
        new("Morphine", DoseStatus.Due),
    });
}

public sealed record ChartedRound(string Name);

public sealed record ChartedDose(string Medicine, [property: JsonConverter(typeof(JsonStringEnumConverter))] DoseStatus Charted);

public enum DoseStatus
{
    Due,
    Given,
    Held,
}
