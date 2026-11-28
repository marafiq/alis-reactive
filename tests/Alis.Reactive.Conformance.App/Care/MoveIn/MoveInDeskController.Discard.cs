using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpGet(CarePages.MoveInOfTheSubmittedDraft)]
    public IActionResult OfOmarsDraft() => PageOf("D300");

    [HttpGet(CareEndpoints.MoveInDraftActions)]
    public async Task<IActionResult> DraftActions([FromQuery] DeskQuery query)
    {
        if (string.IsNullOrEmpty(query.DraftId)) return Content("Choose a move-in to see its draft actions.");
        await World.Reach(GateName.Of(CompositionGate.Answer), Request.Path);
        return Desk.DraftActionsOf(query.DraftId) is { } actions ? PartialView(Views + "DraftActions.cshtml", actions) : NotFound(MoveInErrors.Missing());
    }

    [HttpDelete(MoveInEndpoints.Draft)]
    [ValidateAntiForgeryToken]
    public IActionResult Discard([FromRoute] string draftId,
        [FromHeader(Name = MoveInEndpoints.VersionHeader)] int version,
        [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator,
        [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        if (World.NextAnswer(Request.Path) == 503) return StatusCode(503);
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (DropsItsConnection()) return new EmptyResult();
        var result = Desk.Discard(draftId, version, coordinator ?? string.Empty, operation);
        if (result.Stale is { } stale) return Conflict(stale);
        if (result.Error is { } error) return StatusCode(result.Status, error);
        return NoContent();
    }
}

public sealed record DraftActionsPage(string DraftId, string ResidentName, DraftStatus Status, int Version);

public static class DraftActionEvents
{
    public static readonly AppEvent Discarded = AppEvent.Declare();
}
