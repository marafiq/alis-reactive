using System.IO;
using System.Web.Mvc;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BedNotesController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet]
    [Address(CarePages.BedNotes)]
    public ActionResult OfABed() => View("/Care/Admission/BedNotes.cshtml", new BedNotesPage(onTheEveningShift: false));

    [HttpGet]
    [Address(CarePages.BedNotesOnTheEveningShift)]
    public ActionResult OnTheEveningShift() => View("/Care/Admission/BedNotes.cshtml", new BedNotesPage(onTheEveningShift: true));

    [HttpPost]
    [Address(BedNoteEndpoints.Notes)]
    public ActionResult Note(BedNote note)
    {
        using var reader = new StreamReader(Request.InputStream);
        return Content($"{BedNoteAnswers.Of(note.Ward, note.Shift)}: {reader.ReadToEnd()}", PlainText);
    }

    [HttpGet]
    [Address(BedNoteEndpoints.Notes)]
    public ActionResult ReadNotes(BedNote note) => Content($"{BedNoteAnswers.Of(note.Ward, note.Shift)}: notes read", PlainText);

    [HttpDelete]
    [Address(BedNoteEndpoints.Notes)]
    public ActionResult ClearNotes(BedNote note) => Content($"{BedNoteAnswers.Of(note.Ward, note.Shift)}: notes cleared", PlainText);

    [HttpPost]
    [Address(BedNoteEndpoints.Photos)]
    public ActionResult Photo(BedNote note) => Content($"{BedNoteAnswers.Of(note.Ward, note.Shift)}: {Request.ContentType}, {Request.ContentLength} bytes", PlainText);

    [HttpPost]
    [Address(BedNoteEndpoints.SavedNotes)]
    public ActionResult Save(BedNote note) => Content($"{BedNoteAnswers.Of(note.Ward, note.Shift)}: {note.Text}", PlainText);

    [HttpPost]
    [Address(BedNoteEndpoints.EveningSavedNotes)]
    public ActionResult SaveOnTheEveningShift(EveningBedNote note) => Content($"{BedNoteAnswers.Of(note.Ward, note.Shift)}: {note.Text}", PlainText);

    [HttpPost]
    [Address(BedNoteEndpoints.Handovers)]
    public ActionResult HandOver(BedHandover handover) => Content($"{BedNoteAnswers.Of(handover.Ward, handover.Shift)}: {handover.Text}", PlainText);
}
