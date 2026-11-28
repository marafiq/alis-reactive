using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class TeamBoardController : Controller
{
    [HttpGet(CarePages.TeamBoard)]
    public IActionResult OfAResident() => View("/Care/Record/TeamBoard.cshtml", new TeamBoard("Ada Hart"));

    [HttpGet(TeamBoardEndpoints.Tools)]
    public IActionResult Tools() => PartialView("/Care/Record/TeamBoardTools.cshtml", new TeamBoardTools("Day shift"));
}

public sealed record TeamBoard(string Resident);

public sealed record TeamBoardTools(string Shift);

public static class TeamBoardEndpoints
{
    private const string Root = CarePages.Area + "record/team-board/";

    public const string Tools = Root + "tools";
}

public static class TeamBoards
{
    public static readonly AppEvent OpenTheTools = AppEvent.Declare();

    public static readonly AppEvent ClearTheBoard = AppEvent.Declare();

    public static readonly AppEvent ClearTheNoticeboard = AppEvent.Declare();
}
