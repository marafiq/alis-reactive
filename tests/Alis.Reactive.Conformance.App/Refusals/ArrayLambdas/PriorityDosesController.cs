using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class PriorityDosesController : Controller
{
    [HttpGet(RefusalPages.DosesOfNoPriority)]
    public IActionResult OfNoPriority() => View("/Refusals/ArrayLambdas/PriorityDoses.cshtml", new PriorityRound(null));

    [HttpGet(RefusalPages.DosesOfAPriority)]
    public IActionResult OfAPriority() => View("/Refusals/ArrayLambdas/PriorityDoses.cshtml", new PriorityRound(2));

    [HttpGet(ArrayLambdaEndpoints.DosesByPriority)]
    public IActionResult Doses() => Json(new List<PriorityDose>
    {
        new("Aspirin", 2),
        new("Digoxin", 1),
        new("Insulin", null),
        new("Warfarin", 2),
    });
}

public sealed record PriorityRound(int? Priority);

public sealed record PriorityDose(string Medicine, int? Priority);
