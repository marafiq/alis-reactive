using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CareNoteRecordController : Controller
{
    [HttpGet(CarePages.CareNoteRecordWithANote)]
    public IActionResult WithANote() => View("/Care/Record/CareNoteRecord.cshtml", new CareNoteRecord { CareNotes = "Prefers tea at seven" });

    [HttpGet(CarePages.CareNoteRecordWithNoNote)]
    public IActionResult WithNoNote() => View("/Care/Record/CareNoteRecord.cshtml", new CareNoteRecord { CareNotes = null });
}

public sealed class CareNoteRecord
{
    public string? CareNotes { get; set; }
}
