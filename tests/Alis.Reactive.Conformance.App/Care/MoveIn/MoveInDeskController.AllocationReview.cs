using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpPost(MoveInEndpoints.ReviewCheck)]
    public async Task<IActionResult> CheckTheReview([FromRoute] string draftId)
    {
        await World.Reach(GateName.Of(RunGate.Settle), Request.Path);
        if (DropsItsConnection()) return new EmptyResult();

        return Desk.IsReadyForReview(draftId) switch
        {
            true => Json(true),
            false => Conflict(false),
            null => NotFound(MoveInErrors.Missing()),
        };
    }

    [HttpPost(MoveInEndpoints.AllocationReview)]
    [ValidateAntiForgeryToken]
    public IActionResult ReviewTheAllocation(
        [FromRoute] string draftId,
        [FromBody] AllocationRequest asked,
        [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator,
        [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        if (DropsItsConnection()) return new EmptyResult();

        var validation = this.Validate<AllocationRequest, AllocationRequestRules>(asked);
        if (!validation.IsValid) return validation.ToErrorResponse();
        if (coordinator is null || !Desk.MayAdmit(coordinator)) return StatusCode(StatusCodes.Status403Forbidden, MoveInErrors.Forbidden());
        if (Desk.IsClosed(draftId)) return StatusCode(StatusCodes.Status410Gone, MoveInErrors.Closed());

        var verdict = Desk.ReviewTheAllocation(draftId, coordinator, operation, asked);
        if (verdict.Taken is { } taken) return Conflict(taken);
        if (verdict.Refused is { } refused) return UnprocessableEntity(refused);
        if (verdict.Cleared is not { } cleared) return NotFound(MoveInErrors.Missing());

        return Json(cleared);
    }

    [HttpGet(CareEndpoints.MoveInAllocationDesk)]
    public IActionResult AllocationDesk([FromQuery] DeskQuery query)
    {
        if (DropsItsConnection()) return new EmptyResult();

        return PartialView(Views + "AllocationDesk.cshtml", new AllocationDeskPage { DraftId = query.DraftId });
    }
}

public static partial class MoveInEndpoints
{
    public const string AllocationReview = Page + "/allocation-review";

    public const string ReviewCheck = Page + "/review/check";

    public static string ReviewCheckOf(string draftId) => CarePages.MoveIns + draftId + "/review/check";
}

public static class MoveInAllocationEvents
{
    public static readonly AppEvent ReviewAsked = AppEvent.Declare();
}
