using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class RoundsBoardController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(RefusalPages.RoundsBoard)]
    public IActionResult OfTheNorthWard() => View("/Refusals/RequestsAndResponses/RoundsBoard.cshtml", new RoundsBoard("North"));

    [HttpGet(RefusalEndpoints.Rounds)]
    public IActionResult Rounds() => Content("Morning, Noon, Night", PlainText);
}

public sealed record RoundsBoard(string Ward);

public static class RoundsBoards
{
    public static readonly AppEvent Load = AppEvent.Declare();
}
