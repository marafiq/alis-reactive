using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class RiskBoardController : Controller
{
    [HttpGet(RefusalPages.RiskBoardByADecimalScore)]
    public IActionResult ByADecimalScore() => View("/Refusals/FusionTemplates/RiskBoard.cshtml", new RiskBoard([new ScoredResident("Ada", 3), new ScoredResident("Ruth", 1)]));

    [HttpGet(RefusalPages.RiskBoardByAWholeScore)]
    public IActionResult ByAWholeScore() => View("/Refusals/FusionTemplates/RiskBoardByAWholeScore.cshtml", new RiskBoard([new ScoredResident("Ada", 3), new ScoredResident("Ruth", 1)]));
}

public sealed record RiskBoard(IReadOnlyList<ScoredResident> Residents);

public sealed record ScoredResident(string Name, double FallScore);
