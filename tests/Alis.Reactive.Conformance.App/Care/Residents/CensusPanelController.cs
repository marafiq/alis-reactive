using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CensusPanelController : Controller
{
    [HttpGet(CarePages.CensusPanelRecords)]
    public IActionResult Records() => View("/Care/Residents/CensusPanelRecords.cshtml", new CensusPanel("Census"));
}

public sealed record CensusPanel(string Title);

public static class CensusPanels
{
    public static readonly AppEvent Count = AppEvent.Declare();
}
