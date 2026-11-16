using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class VisitBoardController : Controller
{
    [HttpGet(CarePages.VisitBoard)]
    public IActionResult OfTheBoard() => View("/Care/Record/VisitBoard.cshtml", new VisitBoard(VisitBoardEndpoints.KolkataDesk));

    [HttpGet(CarePages.VisitBoardWithNewYorkDesk)]
    public IActionResult WithNewYorkDesk() => View("/Care/Record/VisitBoard.cshtml", new VisitBoard(VisitBoardEndpoints.NewYorkDesk));

    [HttpGet(VisitBoardEndpoints.DeskRoute)]
    public IActionResult Desk() => PartialView("/Care/Record/VisitBoardDesk.cshtml", new VisitBoardDesk(VisitBoardEndpoints.NextVisit));

    [HttpGet(VisitBoardEndpoints.NextVisit)]
    public IActionResult NextVisit() => Json(new BoardVisit(new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero)));
}

public sealed record VisitBoard(string Desk);

public sealed record VisitBoardDesk(string Visit);

public sealed record BoardVisit(DateTimeOffset Starts);

public static class VisitBoardEndpoints
{
    public const string DeskRoute = CarePages.VisitBoard + "/desk";
    public const string KolkataDesk = DeskRoute + "?zone=Asia/Kolkata";
    public const string NewYorkDesk = DeskRoute + "?zone=America/New_York";
    public const string NextVisit = CarePages.VisitBoard + "/next-visit";
}

public static class VisitBoardButtons
{
    public static readonly AppEvent ShowVisit = AppEvent.Declare();

    public static readonly AppEvent OpenKolkataDesk = AppEvent.Declare();

    public static readonly AppEvent ShowDeskVisit = AppEvent.Declare();
}
