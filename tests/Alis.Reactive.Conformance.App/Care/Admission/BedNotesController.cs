using System.IO;
using System.Threading.Tasks;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BedNotesController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.BedNotes)]
    public IActionResult OfABed() => View("/Care/Admission/BedNotes.cshtml", new BedNotesPage(onTheEveningShift: false));

    [HttpGet(CarePages.BedNotesOnTheEveningShift)]
    public IActionResult OnTheEveningShift() => View("/Care/Admission/BedNotes.cshtml", new BedNotesPage(onTheEveningShift: true));

    [HttpPost(BedNoteEndpoints.Notes)]
    public async Task<IActionResult> Note(BedNote note)
    {
        using var reader = new StreamReader(Request.Body);
        return Content($"{BedNoteAnswers.Of(note.Ward, note.Shift)}: {await reader.ReadToEndAsync()}", PlainText);
    }

    [HttpGet(BedNoteEndpoints.Notes)]
    public IActionResult ReadNotes(BedNote note) => Content($"{BedNoteAnswers.Of(note.Ward, note.Shift)}: notes read", PlainText);

    [HttpDelete(BedNoteEndpoints.Notes)]
    public IActionResult ClearNotes(BedNote note) => Content($"{BedNoteAnswers.Of(note.Ward, note.Shift)}: notes cleared", PlainText);

    [HttpPost(BedNoteEndpoints.Photos)]
    public IActionResult Photo(BedNote note) => Content($"{BedNoteAnswers.Of(note.Ward, note.Shift)}: {Request.ContentType}, {Request.ContentLength} bytes", PlainText);

    [HttpPost(BedNoteEndpoints.SavedNotes)]
    public IActionResult Save([FromForm] BedNote note) => Content($"{BedNoteAnswers.Of(note.Ward, note.Shift)}: {note.Text}", PlainText);

    [HttpPost(BedNoteEndpoints.EveningSavedNotes)]
    public IActionResult SaveOnTheEveningShift([FromForm] EveningBedNote note) => Content($"{BedNoteAnswers.Of(note.Ward, note.Shift)}: {note.Text}", PlainText);

    [HttpPost(BedNoteEndpoints.Handovers)]
    public IActionResult HandOver([FromBody] BedHandover handover) => Content($"{BedNoteAnswers.Of(handover.Ward, handover.Shift)}: {handover.Text}", PlainText);
}
