using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class IntakeDocumentsController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.IntakeDocuments)]
    public IActionResult OfAResident() => View("/Care/Admission/IntakeDocuments.cshtml", new IntakeDocuments());

    [HttpPost(IntakeDocumentsEndpoints.Saves)]
    public IActionResult Save([FromForm] IntakeDocuments intake)
        => Content(string.Join("; ", [intake.Name, .. intake.Documents.Select(document => $"{document.File?.FileName} {document.File?.Length} {document.Caption}")]), PlainText);
}

public sealed class IntakeDocuments
{
    public string? Name { get; set; }

    public List<IntakeDocument> Documents { get; set; } = [];
}

public sealed class IntakeDocument
{
    public IFormFile? File { get; set; }

    public string? Caption { get; set; }
}

public static class IntakeDocumentsEndpoints
{
    public const string Saves = CarePages.IntakeDocuments + "/saves";
}

public static class IntakeDocumentSaves
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
