using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CensusScoresController : Controller
{
    [HttpGet(CarePages.CensusScoresWithAHeadCountOnEveryItem)]
    public IActionResult WithAHeadCountOnEveryItem() => ScoredWithTheLounge(new ItemStart { Name = "Lounge", OptionalCount = 3 });

    [HttpGet(CarePages.CensusScoresWithAnItemLackingItsHeadCount)]
    public IActionResult WithAnItemLackingItsHeadCount() => ScoredWithTheLounge(new ItemStart { Name = "Lounge" });

    private ViewResult ScoredWithTheLounge(ItemStart lounge) => View("/Care/Residents/CensusScores.cshtml", new CensusScores(
        [new ItemStart { Name = "Hall", OptionalScore = 4, OptionalCount = 1 }, new ItemStart { Name = "Bath", OptionalScore = 7, OptionalCount = 2 }, lounge]));
}

public sealed record CensusScores(IReadOnlyList<ItemStart> Rooms);

public static class CensusScoreEvents
{
    public static readonly AppEvent CountScores = AppEvent.Declare();

    public static readonly AppEvent CountHeadCounts = AppEvent.Declare();
}
