using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpGet(CareEndpoints.MoveInPayerOwner)]
    public IActionResult PayerOwner([FromQuery] DeskQuery query)
        => PartialView(Views + "PayerOwner.cshtml", Desk.PayerOwnerOf(query.DraftId));

    [HttpPost(MoveInEndpoints.RoomReview)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RoomReview([FromRoute] string draftId, [FromBody] RoomReviewRequest sent,
        [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator,
        [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        await World.Reach(GateName.Of(LanesGate.Respond), Request.Path);
        if (DropsItsConnection()) return new EmptyResult();
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var validation = this.Validate<RoomReviewRequest, RoomReviewRules>(sent);
        if (!validation.IsValid) return validation.ToErrorResponse();
        return OwnerReviewAnswer(Desk.ReviewRoom(draftId, coordinator ?? string.Empty, operation, sent));
    }

    [HttpGet(CareEndpoints.MoveInRoomSearch)]
    public IActionResult RoomSearch([FromQuery] RoomSearchQuery search)
    {
        if (DropsItsConnection()) return new EmptyResult();
        var (status, names) = Desk.RoomNames(search);
        if (status == 503) return StatusCode(status, MoveInErrors.Refused("directory-unavailable", "The room directory is unavailable."));
        return status == 200 ? Json(names) : StatusCode(status, MoveInErrors.Forbidden());
    }
}

public static partial class MoveInEndpoints
{
    public const string RoomReview = Page + "/room-reviews";
}

public sealed record PayerOwnerPage(string DraftId, string Reference);

public sealed class RoomSearchQuery
{
    public string DraftId { get; set; } = string.Empty;

    public string CommunityId { get; set; } = string.Empty;

    public string Query { get; set; } = string.Empty;
}
