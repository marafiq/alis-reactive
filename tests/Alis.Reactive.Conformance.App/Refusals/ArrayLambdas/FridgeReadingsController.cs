using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class FridgeReadingsController : Controller
{
    [HttpGet(RefusalPages.FridgeReadingsAtZero)]
    public IActionResult AtZero() => View("/Refusals/ArrayLambdas/FridgeReadingsAtZero.cshtml", new FridgeLog("Medication fridge"));

    [HttpGet(FridgeReadingEndpoints.Readings)]
    public IActionResult Readings() => Json(new List<FridgeReading>
    {
        new("06:00", 4.5),
        new("08:00", 0.0),
        new("10:00", -0.0),
        new("12:00", 3.2),
    });
}

public sealed record FridgeLog(string Fridge);

public sealed record FridgeReading(string Taken, double Temperature);

public static class FridgeReadingEndpoints
{
    public const string Readings = RefusalPages.FridgeReadingsAtZero + "/readings";
}
