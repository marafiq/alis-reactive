using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardRefreshController : Controller
{
    [HttpGet(CarePages.WardRefresh)]
    public IActionResult OfAda() => View("/Care/Record/WardRefresh.cshtml", new WardRefresh("Ada Hart", new WardRefreshCard("Resident card")));

    [HttpGet(WardRefreshEndpoints.Resident)]
    [AtTheAnswerGate]
    public IActionResult Resident() => NoContent();
}

public sealed record WardRefresh(string Resident, WardRefreshCard Card);

public sealed record WardRefreshCard(string Title);

public static class WardRefreshEndpoints
{
    public const string Resident = CarePages.WardRefresh + "/residents/7";
}

public static class WardRefreshes
{
    public static readonly AppEvent Refresh = AppEvent.Declare();

    public static readonly AppEvent CloseTheCard = AppEvent.Declare();
}
