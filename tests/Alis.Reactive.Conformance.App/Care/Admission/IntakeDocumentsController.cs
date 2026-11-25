using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class IntakeDocumentsController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.IntakeDocuments)]
    public IActionResult OfAResident() => View("/Care/Admission/IntakeDocuments.cshtml", new IntakeDocuments { Documents = [new IntakeDocument(), new IntakeDocument()] });

    [HttpPost(IntakeDocumentsEndpoints.Saves)]
    public IActionResult Save([FromForm] IntakeDocuments intake)
        => Content(string.Join("; ", [intake.Name, .. intake.Documents.Select(document => $"{document.File?.FileName} {document.File?.Length} {document.Caption}")]), PlainText);
}

public sealed partial class IntakeDocument
{
    public IFormFile? File { get; set; }
}
