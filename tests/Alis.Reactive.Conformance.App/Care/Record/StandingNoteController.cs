using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class StandingNoteController : Controller
{
    [HttpGet(CarePages.StandingNote)]
    public IActionResult OfTheHall() => View("/Care/Record/StandingNote.cshtml", new StandingNote("Main hall"));

    [HttpGet(StandingNoteEndpoints.Board)]
    public IActionResult Board() => PartialView("/Care/Record/StandingNoteBoard.cshtml", new StandingNoteBoard("Noticeboard", new StandingNoteCard("Lunch is served at noon.")));
}

public sealed record StandingNote(string Hall);

public sealed record StandingNoteBoard(string Title, StandingNoteCard Note);

public sealed record StandingNoteCard(string Text);

public static class StandingNoteEndpoints
{
    private const string Root = CarePages.Area + "record/standing-note/";

    public const string Board = Root + "board";
}

public static class StandingNoteButtons
{
    public static readonly AppEvent OpenTheBoard = AppEvent.Declare();

    public static readonly AppEvent TakeTheNoteDown = AppEvent.Declare();
}
