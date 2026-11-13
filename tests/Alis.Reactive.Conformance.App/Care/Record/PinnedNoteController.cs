using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class PinnedNoteController : Controller
{
    [HttpGet(CarePages.PinnedNote)]
    public IActionResult OfAResident() => View("/Care/Record/PinnedNote.cshtml", new PinnedNote("Ada Hart", "Check the pressure dressing at noon."));
}

public sealed record PinnedNote(string Resident, string Note);

public static class PinnedNotes
{
    public static readonly AppEvent Hide = AppEvent.Declare();

    public static readonly AppEvent Show = AppEvent.Declare();

    public static readonly AppEvent Stamp = AppEvent.Declare();
}
