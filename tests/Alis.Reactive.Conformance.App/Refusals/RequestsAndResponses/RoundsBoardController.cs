using Alis.Reactive.Conformance.World;
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
