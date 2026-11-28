using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class FusionDoseLogController : Controller
{
    [HttpGet(CarePages.DoseLogWithFusionToast)]
    public IActionResult ForTheWard() => View("/Care/Shell/FusionDoseLog.cshtml", new FusionDoseLog(""));
}

public sealed record FusionDoseLog(string Note);

public static class FusionDoseLogButtons
{
    public static readonly AppEvent Record = AppEvent.Declare();
    public static readonly AppEvent DismissOldest = AppEvent.Declare();
}
