using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class NotesDeskController : Controller
{
    [HttpGet(CarePages.NotesDesk)]
    public IActionResult OfAResident() => View("/Care/Record/NotesDesk.cshtml", new NotesDesk("Ada Hart"));

    [HttpGet(NotesDeskEndpoints.Tools)]
    public IActionResult Tools() => PartialView("/Care/Record/NotesDeskTools.cshtml", new NotesDeskTools("Night shift"));

    [HttpGet(NotesDeskEndpoints.Card)]
    public IActionResult Card() => PartialView("/Care/Record/NightNoteCard.cshtml", new NightNoteCard("Slept through the night."));
}

public sealed record NotesDesk(string Resident);

public sealed record NotesDeskTools(string Shift);

public sealed record NightNoteCard(string Note);

public static class NotesDeskEndpoints
{
    private const string Root = CarePages.Area + "record/notes-desk/";

    public const string Tools = Root + "tools";

    public const string Card = Root + "card";
}

public static class NotesDesks
{
    public static readonly AppEvent OpenTheDesk = AppEvent.Declare();

    public static readonly AppEvent CloseTheDesk = AppEvent.Declare();

    public static readonly AppEvent MarkNoCardOpen = AppEvent.Declare();

    public static readonly AppEvent LoadTheCard = AppEvent.Declare();

    public static readonly AppEvent CloseTheCard = AppEvent.Declare();

    public static readonly AppEvent Acknowledge = AppEvent.Declare();
}
