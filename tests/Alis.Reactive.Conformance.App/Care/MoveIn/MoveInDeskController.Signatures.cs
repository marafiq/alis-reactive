using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpHead(MoveInEndpoints.Document)]
    public async Task<IActionResult> InspectDocument([FromRoute] string draftId, [FromRoute] string documentId)
    {
        var read = Desk.InspectDocument(draftId, documentId);
        await World.Reach(GateName.Of(CompositionGate.Answer), Request.Path);
        if (read.Metadata is not { } metadata) return StatusCode(read.Status);
        Response.ContentLength = metadata.Size;
        Response.ContentType = metadata.Type;
        Response.Headers.ETag = $"\"{metadata.Revision}\"";
        if (read.Caption is { Length: > 0 } caption) Response.Headers[MoveInEndpoints.DocumentCaption] = Uri.EscapeDataString(caption);
        return new EmptyResult();
    }

    [HttpPut(MoveInEndpoints.DocumentContent)]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(1048576)]
    public async Task<IActionResult> ReplaceSignature([FromRoute] string draftId, [FromRoute] string documentId, [FromQuery] SignatureRequest sent,
        [FromHeader(Name = MoveInEndpoints.VersionHeader)] string? etag,
        [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator, [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (Request.ContentType != sent.Type || sent.Size is < 1 or > 1048576) return BadRequest(MoveInErrors.Refused("signature-file", "Choose a plain text signature file."));
        using var contents = new MemoryStream();
        await Request.Body.CopyToAsync(contents, HttpContext.RequestAborted);
        await World.Reach(GateName.Of(RequestGate.Attempt), Request.Path);
        var result = Desk.ReplaceSignature(draftId, documentId, etag, sent, contents.ToArray(), coordinator, operation);
        return result.Reply is { } reply ? Ok(reply) : StatusCode(result.Status, MoveInErrors.Refused("signature-refused", result.Reason));
    }
}

public static partial class MoveInEndpoints
{
    public const string DocumentCaption = "X-Document-Caption";
    public const string Document = Documents + "/{DocumentId}";
    public const string DocumentContent = Document + "/content";
}
