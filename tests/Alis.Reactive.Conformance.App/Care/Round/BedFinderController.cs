using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BedFinderController : Controller
{
    [HttpGet(CarePages.BedFinder)]
    public IActionResult Finder() => View("/Care/Round/BedFinder.cshtml", new BedFinder("North"));

    [HttpGet(CareEndpoints.FreeBeds)]
    [AtTheRespondGate]
    public IActionResult FreeBeds() => Json(new FreeBeds("4"));
}

public sealed record BedFinder(string Ward);

public sealed record FreeBeds(string Count);

public static class BedFinderEvents
{
    public static readonly AppEvent Find = AppEvent.Declare();
}
