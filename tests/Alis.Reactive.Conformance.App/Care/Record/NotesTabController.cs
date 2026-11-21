using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class NotesTabController : Controller
{
    [HttpGet(CarePages.NotesTab)]
    public IActionResult OfAda() => View("/Care/Record/NotesTab.cshtml", new NotesTab("Ada Hart", new NotesTabSheet("Care notes"), [DefaultHooks.SessionScript]));

    [HttpGet(NotesTabEndpoints.Resident)]
    public IActionResult Resident() => NoContent();
}

public sealed record NotesTab(string Resident, NotesTabSheet Sheet, IReadOnlyList<string> Scripts) : IScriptedRecordPage;

public sealed record NotesTabSheet(string Title);

public static class NotesTabEndpoints
{
    public const string Resident = CarePages.NotesTab + "/residents/7";
}

public static class NotesTabs
{
    public static readonly AppEvent Refresh = AppEvent.Declare();

    public static readonly AppEvent CloseTheTab = AppEvent.Declare();
}
