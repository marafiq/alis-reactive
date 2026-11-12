using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoundBoardController : Controller
{
    [HttpGet(CarePages.RoundBoard)]
    public IActionResult OfAWard() => View("/Care/Record/RoundBoard.cshtml", new RoundBoard("East ward", new RoundBell("Night shift")));

    [HttpGet(RoundBoardEndpoints.Handover)]
    public IActionResult Handover() => PartialView("/Care/Record/RoundHandover.cshtml", new RoundHandover("Two residents to check before six."));
}

public sealed record RoundBoard(string Ward, RoundBell Bell);

public sealed record RoundBell(string Shift);

public sealed record RoundHandover(string Note);

public static class RoundBoardEndpoints
{
    private const string Root = CarePages.Area + "record/round-board/";

    public const string Handover = Root + "handover";
}

public static class RoundBoards
{
    public static readonly AppEvent RoundStarted = AppEvent.Declare();

    public static readonly AppEvent SlotOpened = AppEvent.Declare();

    public static readonly AppEvent OpenTheHandover = AppEvent.Declare();

    public static readonly AppEvent CallThePorter = AppEvent.Declare();

    public static readonly AppEvent PorterCalled = AppEvent.Declare();
}
