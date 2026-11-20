using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpGet(CareEndpoints.MoveInDocumentCard)]
    public IActionResult DocumentCard([FromQuery] DeskQuery query)
    {
        if (string.IsNullOrEmpty(query.DraftId)) return Content("Choose a draft to upload admission documents.");
        if (ReviewAccess(query.DraftId) is { } refusal) return refusal;
        return PartialView(Views + "Documents.cshtml", Desk.DocumentForm(query.DraftId));
    }

    [HttpOptions(MoveInEndpoints.Documents)]
    public async Task<IActionResult> DocumentPolicy([FromRoute] string draftId, [FromQuery] DocumentPolicyQuery query)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var policy = Desk.UploadPolicyOf(draftId, query.Kind);
        await World.Reach(GateName.Of(RequestGate.Route), Request.Path);
        Response.Headers.Allow = "GET, HEAD, OPTIONS, POST, PUT";
        return policy.Policy is { } allowed ? Ok(new UploadPolicyReply(allowed)) : StatusCode(policy.Status, MoveInErrors.Refused("upload-unavailable", "Document uploads are unavailable."));
    }

    [HttpPost(MoveInEndpoints.Documents)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDocuments([FromRoute] string draftId, [FromForm] DocumentsPage form,
        [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator, [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        if (DropsItsConnection()) return new EmptyResult();
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (form.IncludeAuthority != (form.AuthorityFile is not null) || form.Files.Count != form.LastModified.Count || form.Files.Count == 0)
            return BadRequest(MoveInErrors.Refused("invalid-documents", "Choose supported admission files."));
        if (form.Files.Any(file => file.Length > MoveInDesk.MaximumDocumentBytes) || form.AuthorityFile?.Length > MoveInDesk.MaximumDocumentBytes)
            return BadRequest(MoveInErrors.Refused("upload-limit", "A file exceeds the upload limit."));
        var batch = new List<DocumentState>();
        for (var i = 0; i < form.Files.Count; i++)
            batch.Add(await ReadDocument(form.Files[i], form.Kind, form.Caption, form.LastModified[i]));
        if (form.AuthorityFile is { } authority)
        {
            if (form.AuthorityLastModified is not { } modified) return BadRequest(MoveInErrors.Refused("invalid-authority", "Choose an authority file."));
            batch.Add(await ReadDocument(authority, DocumentKind.Authority, form.Caption, modified));
        }
        await World.Reach(GateName.Of(RequestGate.Attempt), Request.Path);
        var result = Desk.AcceptDocuments(draftId, form, batch, coordinator, operation);
        await World.Reach(GateName.Of(CompositionGate.Answer), Request.Path);
        return result.Reply is { } reply ? StatusCode(result.Status, reply) : StatusCode(result.Status, MoveInErrors.Refused("upload-refused", result.Reason));
    }

    private static async Task<DocumentState> ReadDocument(IFormFile file, DocumentKind kind, string caption, long modified)
    {
        using var content = new MemoryStream();
        await file.CopyToAsync(content);
        return new DocumentState { Name = file.FileName, Kind = kind, Bytes = content.ToArray(), Caption = caption, Size = content.Length, ContentType = file.ContentType, LastModified = modified };
    }
}

public static partial class MoveInEndpoints
{
    public const string Documents = Page + "/documents";
}

public sealed class DocumentPolicyQuery
{
    public string DraftId { get; set; } = string.Empty;

    public DocumentKind Kind { get; set; }

    public string Culture { get; set; } = string.Empty;
}
