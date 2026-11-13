using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ShiftNoteController : Controller
{
    private const string Note = "/Care/Handover/ShiftNote.cshtml";

    [HttpGet(CarePages.ShiftNoteWritten)]
    public IActionResult Written() => View(Note, new ShiftHandover { ShiftNote = "Resident slept through the night." });

    [HttpGet(CarePages.ShiftNoteBlank)]
    public IActionResult Blank() => View(Note, new ShiftHandover { ShiftNote = "" });
}

public sealed class ShiftHandover
{
    public string? ShiftNote { get; set; }
}
