using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class PlannedDosesController : Controller
{
    [HttpGet(RefusalPages.DosesChartedAsPlanned)]
    public IActionResult ChartedAsPlanned() => View("/Refusals/ArrayLambdas/DosesChartedAsPlanned.cshtml", new PlannedRound("Morning"));

    [HttpGet(RefusalPages.DosesGivenAsPlanned)]
    public IActionResult GivenAsPlanned() => View("/Refusals/ArrayLambdas/DosesGivenAsPlanned.cshtml", new PlannedRound("Morning"));

    [HttpGet(ArrayLambdaEndpoints.PlannedDoses)]
    public IActionResult Doses() => Json(new List<PlannedDose>
    {
        new("Aspirin", DoseStatus.Given, DoseStatus.Given, DoseStatus.Given),
        new("Digoxin", DoseStatus.Held, DoseStatus.Held, DoseStatus.Held),
        new("Insulin", DoseStatus.Due, DoseStatus.Due, DoseStatus.Given),
        new("Warfarin", DoseStatus.Given, DoseStatus.Given, DoseStatus.Due),
    });
}

public sealed record PlannedRound(string Name);

public sealed record PlannedDose(
    string Medicine,
    DoseStatus Status,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] DoseStatus Charted,
    DoseStatus Planned);
