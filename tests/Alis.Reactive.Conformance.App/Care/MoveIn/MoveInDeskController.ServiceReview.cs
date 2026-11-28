using System.Globalization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpGet(MoveInEndpoints.ServicePanel)]
    public IActionResult ServicePanel([FromQuery] string? draftId)
    {
        if (ReviewAccess(draftId) is { } refusal) return refusal;
        var page = Desk.PageOf(draftId!)!;
        return PartialView(Views + "QuoteDesk.cshtml", new ServiceReviewPage(page.DraftId, Desk.ReviewComponent(page)));
    }

    [HttpPost(MoveInEndpoints.ServicePanel)]
    [ValidateAntiForgeryToken]
    public IActionResult ReviewSelection([FromQuery] string? draftId, [FromBody] QuoteRequest selected)
    {
        if (ReviewAccess(draftId) is { } refusal) return refusal;
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var page = Desk.PageOf(draftId!)!;
        page.Services = selected.Services;
        return PartialView(Views + "QuoteDesk.cshtml", new ServiceReviewPage(page.DraftId, Desk.ReviewComponent(page)));
    }

    private IActionResult? ReviewAccess(string? draftId)
    {
        if (string.IsNullOrEmpty(draftId)) return Content("Choose a move-in to read its service review.");
        if (Desk.PageOf(draftId) is not { } page) return NotFound(MoveInErrors.Missing());
        return Desk.CoordinatorServes(page.CoordinatorId, page.Location.CommunityId) ? null : StatusCode(403, MoveInErrors.Forbidden());
    }

    [HttpGet(MoveInEndpoints.ReviewContext)]
    public IActionResult ReviewContext([FromQuery] string? draftId)
        => ReviewAccess(draftId) ?? Json(Desk.ReviewRead(draftId!)!);

    [HttpPost(MoveInEndpoints.Page + "/review/visits")]
    public async Task<IActionResult> ServiceVisitCount([FromRoute] string draftId, [FromQuery] ServiceInput selected)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var reading = Desk.CurrentServiceRecord(draftId, selected);
        await World.Reach(GateName.Of(RunGate.Settle), Request.Path);
        return reading.Status == 200 ? Json(reading.Visits) : StatusCode(reading.Status);
    }

    [HttpPost(MoveInEndpoints.Page + "/review/services")]
    public async Task<IActionResult> ServiceRecord([FromRoute] string draftId, [FromQuery] ServiceInput selected)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var reading = Desk.CurrentServiceRecord(draftId, selected);
        await World.Reach(GateName.Of(RunGate.Settle), Request.Path);
        if (reading.Status != 200) return StatusCode(reading.Status);
        return reading.Record is { } record ? Json(record) : StatusCode(503);
    }

    [HttpGet(MoveInEndpoints.CurrentOffer)]
    public async Task<IActionResult> CurrentOffer([FromQuery] string? draftId, [FromQuery] int? previousRevision)
    {
        if (ReviewAccess(draftId) is { } refusal) return refusal;
        await World.Reach(GateName.Of(CompositionGate.Answer), Request.Path);
        if (ReviewAccess(draftId) is { } changedAccess) return changedAccess;
        if (Desk.OfferOf(draftId!) is not { } offer) return NotFound(MoveInErrors.Missing());
        return PartialView(Views + "QuoteOffer.cshtml", offer with { PreviousRevision = previousRevision });
    }

    [HttpPost(MoveInEndpoints.Page + "/quote-acceptances")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptQuote([FromRoute] string draftId, [FromBody] QuoteAcceptance request,
        [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator, [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        if (DropsItsConnection()) return new EmptyResult();
        if (coordinator is null || !Desk.Knows(coordinator)) return StatusCode(403, MoveInErrors.Forbidden());
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        await World.Reach(GateName.Of(RequestGate.Attempt), Request.Path);
        var verdict = Desk.AcceptQuote(draftId, request, coordinator, operation);
        await World.Reach(GateName.Of(CompositionGate.Answer), Request.Path);
        if (verdict.Conflict is { } conflict)
        {
            Response.Headers[MoveInEndpoints.MonthlyTotalHeader] = conflict.Quote.Pricing.MonthlyTotal.ToString("0.00", CultureInfo.InvariantCulture);
            return Conflict(conflict);
        }
        if (verdict.Receipt is { } receipt) return Json(receipt);
        return StatusCode(verdict.Status, MoveInErrors.Refused("acceptance-refused", "This quote cannot be accepted."));
    }
}

public static partial class MoveInEndpoints
{
    public const string ServicePanel = CarePages.MoveIns + "service-panel";
    public const string ReviewContext = CarePages.MoveIns + "service-review";
    public const string CurrentOffer = CareEndpoints.MoveInCurrentOffer;
}

public static class QuoteEvents
{
    public static readonly AppEvent ServicesEdited = AppEvent.Declare();
    public static readonly AppEvent<int> Accepted = AppEvent.Declare<int>();
    public static readonly AppEvent Calculated = AppEvent.Declare();
    public static readonly AppEvent<int> CheckCurrentPrice = AppEvent.Declare<int>();
    public static readonly AppEvent<int> PricesChanged = AppEvent.Declare<int>();
}
