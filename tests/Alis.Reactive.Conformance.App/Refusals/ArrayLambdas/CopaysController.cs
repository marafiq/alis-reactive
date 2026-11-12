using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class CopaysController : Controller
{
    [HttpGet(RefusalPages.CopaysLookedUp)]
    public IActionResult LookedUp() => View("/Refusals/ArrayLambdas/CopaysLookedUp.cshtml", new CopayRound("Morning"));

    [HttpGet(RefusalPages.CopaysCompared)]
    public IActionResult Compared() => View("/Refusals/ArrayLambdas/CopaysCompared.cshtml", new CopayRound("Morning"));

    [HttpGet(ArrayLambdaEndpoints.Copays)]
    public IActionResult Doses() => Json(new List<CopayDose>
    {
        new("Aspirin", 0.1m),
        new("Digoxin", 0.25m),
        new("Insulin", 0.5m),
    });
}

public sealed record CopayRound(string Name);

public sealed record CopayDose(
    string Medicine,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString)] decimal Copay);
