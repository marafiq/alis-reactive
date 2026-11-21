using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpGet(CareEndpoints.MoveInAllocationOwner)]
    public async Task<IActionResult> AllocationOwner([FromQuery] DeskQuery query)
    {
        await World.Reach(GateName.Of(LanesGate.Respond), Request.Path);
        return PartialView(Views + "AllocationOwner.cshtml", Desk.AssessmentOwnerOf(query.DraftId));
    }

    [HttpGet(CareEndpoints.MoveInOwnerHistory)]
    public IActionResult ReviewHistory([FromQuery] DeskQuery query)
        => PartialView(Views + "OwnerHistory.cshtml", Desk.OwnerHistoryOf(query.DraftId));

    [HttpPost(MoveInEndpoints.MobilityReview)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MobilityReview([FromRoute] string draftId, [FromBody] MobilityRequest sent,
        [FromHeader(Name = MoveInEndpoints.AssessmentHeader)] string? reference,
        [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator,
        [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        if (DropsItsConnection() || HttpContext.RequestAborted.IsCancellationRequested) return new EmptyResult();
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var validation = this.Validate<MobilityRequest, MobilityRules>(sent);
        if (!validation.IsValid) return validation.ToErrorResponse();
        var result = Desk.ReviewMobility(draftId, reference ?? string.Empty, coordinator ?? string.Empty, operation, sent);
        await World.Reach(GateName.Of(LanesGate.Respond), Request.Path);
        return OwnerReviewAnswer(result);
    }

    private IActionResult OwnerReviewAnswer(OwnerReviewVerdict result)
        => result.Receipt is { } receipt ? StatusCode(result.Status, receipt) : StatusCode(result.Status, result.Error);
}

public static partial class MoveInEndpoints
{
    public const string AssessmentHeader = "X-Assessment";
    public const string MobilityReview = Page + "/mobility-reviews";
}

public sealed record AllocationOwnerPage(string DraftId, string Reference);

public sealed record OwnerDeskPage(string DraftId, int Capacity, OwnerHistoryPage History);

public sealed record OwnerHistoryPage(IReadOnlyList<string> Mobility, IReadOnlyList<string> Rooms, string Assessment, bool Locked);
