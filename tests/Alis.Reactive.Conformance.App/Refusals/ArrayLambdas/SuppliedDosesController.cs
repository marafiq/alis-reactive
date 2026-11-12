using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class SuppliedDosesController : Controller
{
    [HttpGet(RefusalPages.SuppliedDosesOrdered)]
    public IActionResult Ordered() => View("/Refusals/ArrayLambdas/SuppliedDosesOrdered.cshtml", new SuppliedRound("Morning"));

    [HttpGet(RefusalPages.SuppliedDosesCompared)]
    public IActionResult Compared() => View("/Refusals/ArrayLambdas/SuppliedDosesCompared.cshtml", new SuppliedRound("Morning"));

    [HttpGet(ArrayLambdaEndpoints.SuppliedDoses)]
    public IActionResult Doses() => Json(new List<SuppliedDose>
    {
        new("Aspirin", new DoseSupply { Vials = 2 }),
        new("Digoxin", new DoseSupply { Vials = 10 }),
        new("Insulin", new DoseSupply { Vials = 5 }),
    });
}

public sealed record SuppliedRound(string Name);

public sealed record SuppliedDose(string Medicine, DoseSupply Supply);

[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)]
public sealed class DoseSupply
{
    public int Vials { get; init; }
}
