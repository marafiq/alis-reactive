using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class NotePreviewController : Controller
{
    [HttpGet(CarePages.NotePreview)]
    public IActionResult OfAResident() => View("/Care/Record/NotePreview.cshtml", new NotePreview("Ada Brooks"));

    [HttpGet(NotePreviewEndpoints.Draft)]
    public IActionResult Draft() => PartialView("/Care/Record/NotePreviewDraft.cshtml", new NoteDraft());
}

public sealed record NotePreview(string Resident);

public sealed class NoteDraft
{
    public string? Note { get; set; }
}

public static class NotePreviewEndpoints
{
    public const string Draft = CarePages.Area + "record/note-preview/draft";
}

public static class NotePreviews
{
    public static readonly AppEvent OpenTheNote = AppEvent.Declare();

    public static readonly AppEvent Preview = AppEvent.Declare();
}
