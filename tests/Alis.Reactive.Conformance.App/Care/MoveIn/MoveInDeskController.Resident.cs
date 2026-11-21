using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpPost(MoveInEndpoints.IdentityReview)]
    [ValidateAntiForgeryToken]
    public IActionResult ReviewTheIdentity(
        [FromRoute] string draftId,
        [FromQuery] string? residentId,
        [FromBody] MoveInPage moveIn,
        [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator,
        [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        if (DropsItsConnection()) return new EmptyResult();

        moveIn.Today = Desk.Today();
        var validation = this.Validate<MoveInPage, MoveInRules>(moveIn, MoveInRules.Identity);
        if (!validation.IsValid) return validation.ToErrorResponse();
        if (coordinator is null || !Desk.Knows(coordinator)) return StatusCode(StatusCodes.Status403Forbidden, MoveInErrors.Forbidden());
        if (Desk.IsClosed(draftId)) return StatusCode(StatusCodes.Status410Gone, MoveInErrors.Closed());

        var verdict = Desk.ReviewTheIdentity(draftId, residentId ?? string.Empty, coordinator, operation, moveIn.Resident);
        if (verdict.Duplicate is { } duplicate) return Conflict(duplicate);
        if (verdict.Checked is not { } reviewed) return NotFound(MoveInErrors.Missing());

        return Json(reviewed);
    }

    [HttpGet(CareEndpoints.MoveInResidentDesk)]
    public IActionResult ResidentDesk([FromQuery] DeskQuery query)
    {
        if (DropsItsConnection()) return new EmptyResult();

        return PartialView(Views + "ResidentDesk.cshtml", new ResidentDeskPage { DraftId = query.DraftId });
    }
}

public static partial class MoveInEndpoints
{
    public const string IdentityReview = Page + "/identity-review";
}

public static class MoveInResidentEvents
{
    public static readonly AppEvent VerifyIdentity = AppEvent.Declare();
}
