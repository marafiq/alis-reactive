using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class IntakeFilingController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(RefusalPages.IntakeFiling)]
    public IActionResult OfAda() => View("/Refusals/RequestsAndResponses/IntakeFiling.cshtml", new IntakeFiling("Ada Hart"));

    [HttpPost(RefusalEndpoints.Intake)]
    [ValidateAntiForgeryToken]
    public IActionResult Save([FromForm] FiledIntake intake) => Content($"Filed the intake of {intake.Resident}", PlainText);
}

public sealed record IntakeFiling(string Resident);

public sealed class FiledIntake
{
    public string? Resident { get; set; }
}

public static class IntakeFilings
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
