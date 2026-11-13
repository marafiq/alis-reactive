using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class StampedNoteController : Controller
{
    [HttpGet(CarePages.StampedNote)]
    public IActionResult OfTheHall() => View("/Care/Record/StampedNote.cshtml", new StampedNote("Main hall"));

    [HttpGet(StampedNoteEndpoints.Board)]
    public IActionResult Board() => PartialView("/Care/Record/StampedNoteBoard.cshtml", new StampedNoteBoard("Noticeboard"));

    [HttpGet(StampedNoteEndpoints.Note)]
    public IActionResult Note() => PartialView("/Care/Record/StampedNoteCard.cshtml", new StampedNoteCard("The hall closes at nine."));
}

public sealed record StampedNote(string Hall);

public sealed record StampedNoteBoard(string Title);

public sealed record StampedNoteCard(string Text);

public static class StampedNoteEndpoints
{
    private const string Root = CarePages.Area + "record/stamped-note/";

    public const string Board = Root + "board";

    public const string Note = Root + "note";
}

public static class StampedNoteButtons
{
    public static readonly AppEvent OpenTheBoard = AppEvent.Declare();

    public static readonly AppEvent PinANote = AppEvent.Declare();
}
