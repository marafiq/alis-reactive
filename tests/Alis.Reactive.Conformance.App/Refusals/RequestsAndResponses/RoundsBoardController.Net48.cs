using System.Web.Mvc;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class RoundsBoardController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet]
    [Address(RefusalPages.RoundsBoard)]
    public ActionResult OfTheNorthWard() => View("/Refusals/RequestsAndResponses/RoundsBoard.cshtml", new RoundsBoard("North"));

    [HttpGet]
    [Address(RefusalEndpoints.Rounds)]
    public ActionResult Rounds() => Content("Morning, Noon, Night", PlainText);
}
