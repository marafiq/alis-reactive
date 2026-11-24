using System.IO;
using System.Threading.Tasks;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BedNotesController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.BedNotes)]
    public IActionResult OfABed() => View("/Care/Admission/BedNotes.cshtml", new BedNotesPage(StartsInAdmissions: false));

    [HttpGet(CarePages.BedNotesStartingInAdmissions)]
    public IActionResult StartingInAdmissions() => View("/Care/Admission/BedNotes.cshtml", new BedNotesPage(StartsInAdmissions: true));

    [HttpPost(BedNoteEndpoints.Notes)]
    public Task<IActionResult> Note([FromForm] BedNote note) => Noted(note.Ward);

    [HttpPost(BedNoteEndpoints.AdmissionsNotes)]
    public Task<IActionResult> AdmissionsNote([FromForm] AdmissionsBedNote note) => Noted(note.Ward);

    [HttpDelete(BedNoteEndpoints.Notes)]
    public IActionResult ClearNotes([FromForm] BedNote note) => Cleared(note.Ward);

    [HttpDelete(BedNoteEndpoints.AdmissionsNotes)]
    public IActionResult ClearAdmissionsNotes([FromForm] AdmissionsBedNote note) => Cleared(note.Ward);

    [HttpPost(BedNoteEndpoints.Photos)]
    public IActionResult Photo([FromForm] BedNote note) => Photographed(note.Ward);

    [HttpPost(BedNoteEndpoints.AdmissionsPhotos)]
    public IActionResult AdmissionsPhoto([FromForm] AdmissionsBedNote note) => Photographed(note.Ward);

    private async Task<IActionResult> Noted(string? ward)
    {
        using var reader = new StreamReader(Request.Body);
        return Content($"{ward}: {await reader.ReadToEndAsync()}", PlainText);
    }

    private ContentResult Cleared(string? ward) => Content($"{ward}: notes cleared", PlainText);

    private ContentResult Photographed(string? ward) => Content($"{ward}: {Request.ContentType}, {Request.ContentLength} bytes", PlainText);
}

public sealed record BedNotesPage(bool StartsInAdmissions);

public sealed class BedNote
{
    public string? Ward { get; set; }
}

public sealed class AdmissionsBedNote
{
    public string Ward { get; set; } = "Admissions";
}

public sealed class BedNoteRules : ReactiveValidator<BedNote>
{
    public BedNoteRules() => RuleFor(note => note.Ward).NotEmpty().WithMessage("Ward is required.");
}

public sealed class AdmissionsBedNoteRules : ReactiveValidator<AdmissionsBedNote>
{
    public AdmissionsBedNoteRules() => RuleFor(note => note.Ward).NotEmpty().WithMessage("Ward is required.");
}

public static class BedNoteEndpoints
{
    public const string Notes = CarePages.BedNotes + "/wards/{ward}/notes";

    public const string Photos = CarePages.BedNotes + "/wards/{ward}/photos";

    public const string AdmissionsNotes = CarePages.BedNotesStartingInAdmissions + "/wards/{ward}/notes";

    public const string AdmissionsPhotos = CarePages.BedNotesStartingInAdmissions + "/wards/{ward}/photos";
}
