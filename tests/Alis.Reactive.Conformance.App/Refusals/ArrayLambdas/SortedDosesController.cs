using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class SortedDosesController : Controller
{
    [HttpGet(RefusalPages.DosesSortedByTheirStatus)]
    public IActionResult ByTheirStatus() => View("/Refusals/ArrayLambdas/DosesSortedByTheirStatus.cshtml", new SortedRound("Morning"));

    [HttpGet(RefusalPages.DosesSortedByTheirMilligrams)]
    public IActionResult ByTheirMilligrams() => View("/Refusals/ArrayLambdas/DosesSortedByTheirMilligrams.cshtml", new SortedRound("Morning"));

    [HttpGet(SortedDoseEndpoints.Doses)]
    public IActionResult Doses() => Json(new List<SortedDose>
    {
        new("Aspirin", 5, DoseStatus.Given),
        new("Digoxin", 1, DoseStatus.Due),
        new("Insulin", 3, DoseStatus.Held),
    });
}

public sealed record SortedRound(string Name);

public sealed record SortedDose(string Medicine, int Milligrams, DoseStatus Status);

public static class SortedDoseEndpoints
{
    public const string Doses = RefusalPages.Area + "array-lambdas/sorted-doses/doses";
}
