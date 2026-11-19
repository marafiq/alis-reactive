using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpGet(MoveInEndpoints.PrimaryCoverage)]
    public IActionResult PrimaryCoverage([FromQuery] SectionQuery query)
    {
        if (query.DraftId.Length == 0) return Content("Choose a move-in to enter primary coverage.");
        if (Desk.PageOf(query.DraftId) is not { } page) return NotFound(MoveInErrors.Missing());
        if (Desk.IsClosed(query.DraftId)) return StatusCode(410, MoveInErrors.Closed());
        if (!Desk.CoordinatorServes(page.CoordinatorId, page.Location.CommunityId)) return StatusCode(403, MoveInErrors.Forbidden());
        if (query.Section != SectionKind.PrimaryPayer || !Enum.IsDefined(query.PayerKind)) return UnprocessableEntity(MoveInErrors.Refused("payer-kind", "Choose a funding source."));
        if (page.Payer.Kind != query.PayerKind) { page.Payer.HasSecondary = false; page.Payer.Secondary = null; }
        page.Payer.Kind = query.PayerKind;
        return PartialView(Views + "PrimaryPayer.cshtml", page);
    }

    [HttpGet(CareEndpoints.MoveInSecondaryCoverage)]
    public async Task<IActionResult> SecondaryCoverage([FromQuery] SectionQuery query)
    {
        if (query.DraftId.Length == 0) return Content("Choose a move-in to enter secondary coverage.");
        await World.Reach(GateName.Of(LanesGate.Respond), Request.Path);
        if (DropsItsConnection()) return new EmptyResult();
        if (Desk.PageOf(query.DraftId) is not { } page) return NotFound(MoveInErrors.Missing());
        if (Desk.IsClosed(query.DraftId)) return StatusCode(410, MoveInErrors.Closed());
        if (!Desk.CoordinatorServes(page.CoordinatorId, page.Location.CommunityId)) return StatusCode(403, MoveInErrors.Forbidden());
        if (query.Section != SectionKind.SecondaryPayer || query.PayerKind == PayerKind.Private) return UnprocessableEntity(MoveInErrors.Refused("payer-kind", "Choose primary coverage first."));
        page.Payer.Secondary ??= new SecondaryCoverageInput { StartsOn = page.Payer.Primary.StartsOn, EndsOn = page.Payer.Primary.EndsOn, ContributionPercent = 100m - page.Payer.Primary.ContributionPercent };
        return PartialView(Views + "SecondaryPayer.cshtml", page);
    }

    [HttpGet(MoveInEndpoints.VerificationForm)]
    public IActionResult VerificationForm([FromQuery] string? draftId)
    {
        if (draftId is null) return Content("Choose a move-in to verify coverage.");
        if (Desk.PageOf(draftId) is not { } page) return NotFound(MoveInErrors.Missing());
        if (Desk.IsClosed(draftId)) return StatusCode(410, MoveInErrors.Closed());
        if (!Desk.CoordinatorServes(page.CoordinatorId, page.Location.CommunityId)) return StatusCode(403, MoveInErrors.Forbidden());
        return PartialView(Views + "PayerVerification.cshtml", new PayerVerificationPage { DraftId = page.DraftId, ResidentId = page.ResidentId, ContactId = page.Payer.ContactId, FundingSource = page.Payer.Kind, Policy = page.Payer.Primary });
    }

    [HttpPost(MoveInEndpoints.PayerVerification)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyCoverage([FromRoute] string draftId, [FromQuery] string? reference, [FromForm] PayerVerificationPage page,
        [FromHeader(Name = MoveInEndpoints.CommunityHeader)] string? community, [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator,
        [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        if (DropsItsConnection()) return new EmptyResult();
        await World.Reach(GateName.Of(CompositionGate.Answer), Request.Path);
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (coordinator is null || !Desk.Knows(coordinator)) return StatusCode(403, MoveInErrors.Forbidden());
        page.Reference = reference;
        var check = await this.ValidateAsync<PayerVerificationPage, PayerVerificationRules>(page, HttpContext.RequestAborted);
        if (!check.IsValid)
        {
            if (Desk.PolicyRefusal(page) is { } refusal && page.Consent) return UnprocessableEntity(MoveInErrors.Refused("policy-ineligible", refusal));
            return check.ToErrorResponse();
        }
        var verdict = Desk.Verify(draftId, community, reference, page, coordinator, operation);
        return verdict.Status switch
        {
            200 => Json(verdict.Reply),
            403 => StatusCode(403, MoveInErrors.Forbidden()),
            410 => StatusCode(410, MoveInErrors.Closed()),
            422 => UnprocessableEntity(MoveInErrors.Refused("policy-ineligible", verdict.Reason!)),
            _ => NotFound(MoveInErrors.Missing()),
        };
    }

    [HttpGet(MoveInEndpoints.PolicyDetails)]
    public IActionResult PolicyDetails([FromQuery] string? draftId, [FromQuery] string? reference)
    {
        if (draftId is null || reference is null) return Content("Choose a verified policy to read its coverage.");
        if (Desk.PageOf(draftId) is not { } page) return NotFound(MoveInErrors.Missing());
        if (!Desk.CoordinatorServes(page.CoordinatorId, page.Location.CommunityId)) return StatusCode(403, MoveInErrors.Forbidden());
        return Desk.PolicyDetails(draftId, reference) is { } details ? PartialView(Views + "CoverageTerms.cshtml", new CoverageTerms(details)) : NotFound(MoveInErrors.Missing());
    }
}

public static partial class MoveInEndpoints
{
    public const string PrimaryCoverage = CarePages.MoveIns + "primary-coverage";
    public const string VerificationForm = CarePages.MoveIns + "payer-verification";
    public const string PayerVerification = Page + "/payer-verifications";
    public const string PolicyDetails = CarePages.MoveIns + "policy-details";
}

public sealed record CoverageTerms(string Description);
