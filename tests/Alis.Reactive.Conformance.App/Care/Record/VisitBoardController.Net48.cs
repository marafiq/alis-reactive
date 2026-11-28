using System.Web.Mvc;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class VisitBoardController : Controller
{
    [HttpGet]
    [Address(CarePages.VisitBoard)]
    public ActionResult OfTheBoard() => View("/Care/Record/VisitBoard.cshtml", new VisitBoard(VisitBoardEndpoints.KolkataDesk));

    [HttpGet]
    [Address(CarePages.VisitBoardWithNewYorkDesk)]
    public ActionResult WithNewYorkDesk() => View("/Care/Record/VisitBoard.cshtml", new VisitBoard(VisitBoardEndpoints.NewYorkDesk));

    [HttpGet]
    [Address(VisitBoardEndpoints.DeskRoute)]
    public ActionResult Desk() => PartialView("/Care/Record/VisitBoardDesk.cshtml", new VisitBoardDesk(VisitBoardEndpoints.NextVisit));

    [HttpGet]
    [Address(VisitBoardEndpoints.NextVisit)]
    public ActionResult NextVisit() => this.JsonResponse(new BoardVisit(new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero)));
}
