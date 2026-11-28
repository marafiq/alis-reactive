using System.Collections.Generic;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ShiftNotesController : Controller
{
    [HttpGet(CarePages.ShiftNotes)]
    public IActionResult Still() => View("/Care/Shell/ShiftNotes.cshtml", new ShiftNotesPage(DrawerMotion.Still));

    [HttpGet(CarePages.ShiftNotesSliding)]
    public IActionResult Sliding() => View("/Care/Shell/ShiftNotes.cshtml", new ShiftNotesPage(DrawerMotion.Sliding));

    [HttpGet(ShiftNotesEndpoints.Notes)]
    public IActionResult Notes() => PartialView("/Care/Shell/ShiftNotesList.cshtml", new ResidentShiftNotes("Ada", ["Ada ate her breakfast.", "Ada walked to the garden."]));
}

public enum DrawerMotion
{
    Still,
    Sliding,
}

public sealed record ShiftNotesPage(DrawerMotion Motion);

public sealed record ResidentShiftNotes(string Resident, IReadOnlyList<string> Notes);

public static class ShiftNotesEndpoints
{
    public const string Notes = CarePages.Area + "shell/shift-notes/notes";
}

public static class ShiftNotesButtons
{
    public static readonly AppEvent OpenNotes = AppEvent.Declare();

    public static readonly AppEvent OpenEmpty = AppEvent.Declare();
}
