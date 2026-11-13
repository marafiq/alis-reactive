using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class WardHandoversController : Controller
{
    [HttpGet(RefusalPages.WardHandovers)]
    public IActionResult Index() => View("/Refusals/ArrayLambdas/WardHandovers.cshtml");

    [HttpGet(RefusalPages.WardHandoverCheck)]
    public IActionResult Check() => View("/Refusals/ArrayLambdas/WardHandoverCheck.cshtml", new HandoverCutoff("North", new DateTime(2026, 10, 31, 7, 0, 0)));

    [HttpGet(WardHandoverEndpoints.Handover)]
    public IActionResult Handover() => Json(new WardHandover("North", new DateTime(2026, 10, 30, 19, 0, 0)));
}

public sealed record HandoverCutoff(string Ward, DateTime Cutoff);

public sealed record WardHandover(string Ward, DateTime At);

public static class WardHandoverChecks
{
    public static readonly AppEvent Check = AppEvent.Declare();
}

public static class WardHandoverEndpoints
{
    public const string Handover = RefusalPages.WardHandovers + "/handover";
}
