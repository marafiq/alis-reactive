using System.Text;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpGet(MoveInEndpoints.Packet)]
    public IActionResult DownloadPacket([FromRoute] string draftId, [FromQuery] AdmissionPacketQuery query, [FromQuery] bool requirements)
    {
        if (requirements && query.QuoteRevision is null)
            return Desk.RequirementsOf(draftId) is { } admission ? View(Views + "PacketRequirements.cshtml", admission) : NotFound();
        var read = Desk.PacketOf(draftId, query.QuoteRevision, true);
        if (read.Status == 503) return StatusCode(503);
        return read.Packet is { } packet ? File(Encoding.UTF8.GetBytes(packet.Contents), "text/plain", $"admission-{draftId}.txt") : StatusCode(read.Status, MoveInErrors.Refused("packet-refused", "Accept the current quote and complete the admission documents."));
    }

    [HttpGet(CareEndpoints.MoveInPacketPreview)]
    public IActionResult PreviewPacket([FromQuery] AdmissionPacketQuery query)
    {
        if (string.IsNullOrEmpty(query.DraftId)) return Content("Choose a move-in to preview its packet.");
        var read = Desk.PacketOf(query.DraftId, query.QuoteRevision, false);
        return read.Packet is { } packet ? PartialView(Views + "PacketPreview.cshtml", packet) : StatusCode(read.Status, MoveInErrors.Refused("packet-refused", "Accept the current quote and complete the admission documents."));
    }

    [HttpPost(MoveInEndpoints.PacketNotes)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePacketNote([FromRoute] string draftId, [FromQuery] string coordinatorId, [FromHeader(Name = MoveInEndpoints.PageUrl)] string? url,
        [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator, [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        if (!IsPageOf(draftId, url)) return BadRequest(MoveInErrors.Refused("packet-address", "The packet must be saved from this draft."));
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var note = Desk.SavePacketNote(draftId, coordinatorId, url!, await reader.ReadToEndAsync(HttpContext.RequestAborted), coordinator, operation);
        return new ContentResult { ContentType = "text/plain", StatusCode = note.Status, Content = note.Text };
    }

    [HttpPost(MoveInEndpoints.PrintIntents)]
    [ValidateAntiForgeryToken]
    public IActionResult PrintPacket([FromRoute] string draftId, [FromBody] PrintIntent sent,
        [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator, [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (!IsPageOf(draftId, sent.PageUrl)) return BadRequest(MoveInErrors.Refused("packet-address", "The packet must be printed from this draft."));
        var print = Desk.RequestPrint(draftId, sent, coordinator, operation);
        return print.Receipt is { } receipt ? StatusCode(print.Status, receipt) : StatusCode(print.Status, MoveInErrors.Refused("packet-changed", "The packet price changed. Reopen the quote."));
    }

    private bool IsPageOf(string draftId, string? address) => Uri.TryCreate(address, UriKind.Absolute, out var url)
        && url.Scheme == Request.Scheme && url.Authority == Request.Host.Value && url.AbsolutePath == Request.PathBase + CarePages.MoveIns + draftId;
}

public static partial class MoveInEndpoints
{
    public const string Packet = Page + "/packet";
    public const string PacketNotes = Page + "/packet-notes";
    public const string PrintIntents = Page + "/print-intents";
    public const string PageUrl = "X-Page-Url";
}
