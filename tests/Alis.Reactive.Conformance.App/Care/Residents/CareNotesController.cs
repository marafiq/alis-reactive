using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CareNotesController : Controller
{
    [HttpGet(CarePages.CareNotesLookedThroughForNothing)]
    public IActionResult LookedThroughForNothing() => View("/Care/Residents/CareNotes.cshtml", new CareNotes(null));

    [HttpGet(CarePages.CareNotesLookedThroughForALetter)]
    public IActionResult LookedThroughForALetter() => View("/Care/Residents/CareNotes.cshtml", new CareNotes("a"));

    [HttpGet(CareNotesEndpoints.Notes)]
    public IActionResult Notes() => Json(new List<CareNote>
    {
        new(new CareNoteDetail("a\u0085")),
        new(new CareNoteDetail("﻿a")),
        new(new CareNoteDetail("straße")),
        new(new CareNoteDetail("ΟΔΟΣ")),
        new(new CareNoteDetail("\u0085")),
        new(null),
    });
}

public sealed record CareNotes(string? LookedFor);

public sealed record CareNote(CareNoteDetail? Detail);

public sealed record CareNoteDetail(string Text);

public static class CareNotesEndpoints
{
    public const string Notes = CarePages.Area + "residents/care-notes/notes";
}
