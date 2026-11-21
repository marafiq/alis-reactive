using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class IntakeAttachmentsController : Controller
{
    [HttpGet(CarePages.IntakeAttachments)]
    public IActionResult OfAResident() => View("/Care/Admission/IntakeAttachments.cshtml", new IntakeAttachments());

    [HttpPost(CarePages.IntakeAttachments)]
    public IActionResult Save([FromForm] IntakeAttachments intake)
        => Content(string.Join("; ", [intake.Name, $"{intake.Attachments.Count} attachments", .. intake.Attachments.Select(file => $"{file.FileName} {file.Length}")]), "text/plain");
}

public sealed class IntakeAttachments
{
    public string? Name { get; set; }

    public List<IFormFile> Attachments { get; set; } = [];
}
