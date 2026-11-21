using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CensusActiveCountsController : Controller
{
    [HttpGet(CarePages.CensusActiveCounts)]
    public IActionResult ActiveCounts() => View("/Care/Residents/CensusActiveCounts.cshtml", new CensusActiveCounts(
        [new ItemStart { Name = "Lounge", Active = true }, new ItemStart { Name = "Garden", LacksActive = true }]));
}

public sealed record CensusActiveCounts(IReadOnlyList<ItemStart> North);

public static class CensusActiveCountEvents
{
    public static readonly AppEvent North = AppEvent.Declare();

    public static readonly AppEvent Census = AppEvent.Declare();
}
