using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class WardDosesController : Controller
{
    [HttpGet(RefusalPages.DosesOfNoWards)]
    public IActionResult OfNoWards() => View("/Refusals/ArrayLambdas/WardDoses.cshtml", new WardRound(null!));

    [HttpGet(RefusalPages.DosesOfTwoWards)]
    public IActionResult OfTwoWards() => View("/Refusals/ArrayLambdas/WardDoses.cshtml", new WardRound(["East", "West"]));

    [HttpGet(ArrayLambdaEndpoints.DosesByWard)]
    public IActionResult Doses() => Json(new List<WardDose>
    {
        new("Aspirin", "East"),
        new("Digoxin", "West"),
        new("Insulin", "North"),
        new("Warfarin", "East"),
    });
}

public sealed record WardRound(IReadOnlyList<string> Wards);

public sealed record WardDose(string Medicine, string Ward);
