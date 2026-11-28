using System.Text;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DietNoteController : Controller
{
    private const string TextInWindows1252 = "text/plain; charset=windows-1252";
    private const string PlainText = "text/plain";
    private const string TheNote = "café";

    [HttpGet(CarePages.DietNoteInWindows1252)]
    public IActionResult InWindows1252() => View("/Care/Record/DietNote.cshtml", new DietNote("Ada Hart", DietNoteEndpoints.NoteInWindows1252));

    [HttpGet(CarePages.DietNoteInUtf8)]
    public IActionResult InUtf8() => View("/Care/Record/DietNote.cshtml", new DietNote("Ada Hart", DietNoteEndpoints.NoteInUtf8));

    [HttpGet(DietNoteEndpoints.NoteInWindows1252)]
    public IActionResult TheNoteInWindows1252() => File(CafeInWindows1252(), TextInWindows1252);

    [HttpGet(DietNoteEndpoints.NoteInUtf8)]
    public IActionResult TheNoteInUtf8() => Content(TheNote, PlainText, Encoding.UTF8);

    private static byte[] CafeInWindows1252() => [0x63, 0x61, 0x66, 0xE9];
}

public sealed record DietNote(string Resident, string NoteAddress);

public static class DietNoteEndpoints
{
    private const string Root = CarePages.Area + "record/diet-note/";

    public const string NoteInWindows1252 = Root + "note/windows-1252";

    public const string NoteInUtf8 = Root + "note/utf-8";
}

public static class DietNotes
{
    public static readonly AppEvent Load = AppEvent.Declare();
}
