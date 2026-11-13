using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class HallNoticesController : Controller
{
    [HttpGet(CarePages.HallNotices)]
    public IActionResult OfTheHall() => View("/Care/Record/HallNotices.cshtml", new HallNotices("Main hall"));

    [HttpGet(HallNoticesEndpoints.Board)]
    public IActionResult Board() => PartialView("/Care/Record/HallNoticesBoard.cshtml", new HallNoticesBoard("Noticeboard"));

    [HttpGet(HallNoticesEndpoints.Note)]
    public IActionResult Note() => PartialView("/Care/Record/HallNoticesNote.cshtml", new HallNoticesNote("The hall closes at nine."));
}

public sealed record HallNotices(string Hall);

public sealed record HallNoticesBoard(string Title);

public sealed record HallNoticesNote(string Text);

public static class HallNoticesEndpoints
{
    private const string Root = CarePages.Area + "record/hall-notices/";

    public const string Board = Root + "board";

    public const string Note = Root + "note";
}

public static class HallNoticesButtons
{
    public static readonly AppEvent OpenTheBoard = AppEvent.Declare();

    public static readonly AppEvent CloseTheBoard = AppEvent.Declare();

    public static readonly AppEvent PinANote = AppEvent.Declare();
}
