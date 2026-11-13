using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class PinBoardController : Controller
{
    [HttpGet(CarePages.PinBoard)]
    public IActionResult OfTheHall() => View("/Care/Record/PinBoard.cshtml", new PinBoard("Main hall"));

    [HttpGet(PinBoardEndpoints.Board)]
    public IActionResult Board() => PartialView("/Care/Record/PinBoardBoard.cshtml", new PinBoardBoard("Noticeboard"));

    [HttpGet(PinBoardEndpoints.Note)]
    public IActionResult Note() => PartialView("/Care/Record/PinBoardNote.cshtml", new PinBoardNote("Choir practice moves to Friday."));
}

public sealed record PinBoard(string Hall);

public sealed record PinBoardBoard(string Title);

public sealed record PinBoardNote(string Text);

public static class PinBoardEndpoints
{
    private const string Root = CarePages.Area + "record/pin-board/";

    public const string Board = Root + "board";

    public const string Note = Root + "note";
}

public static class PinBoardButtons
{
    public static readonly AppEvent OpenTheBoard = AppEvent.Declare();

    public static readonly AppEvent CloseTheBoard = AppEvent.Declare();

    public static readonly AppEvent PinANote = AppEvent.Declare();
}
