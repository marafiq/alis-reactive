using Alis.Reactive.Conformance.World;
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
