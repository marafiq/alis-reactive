using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CensusItemPositionsController : Controller
{
    [HttpGet(CarePages.CensusItemPositions)]
    public IActionResult Positions() => View("/Care/Residents/CensusItemPositions.cshtml", new CensusItemPositions(
        [new ItemStart { Name = "Hall", Value = 20 }, new ItemStart { Name = "Bath", Value = 21 }]));
}

public sealed record CensusItemPositions(IReadOnlyList<ItemStart> Rooms);

public static class CensusItemPositionEvents
{
    public static readonly AppEvent RaiseSecond = AppEvent.Declare();

    public static readonly AppEvent ReadThird = AppEvent.Declare();

    public static readonly AppEvent ReadSecond = AppEvent.Declare();
}
