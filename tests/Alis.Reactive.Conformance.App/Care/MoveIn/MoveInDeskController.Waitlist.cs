using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpGet(MoveInEndpoints.WaitlistTerms)]
    public IActionResult WaitlistTerms([FromRoute] string draftId, [FromRoute] string termsId, [FromQuery] string? bedId)
    {
        var (status, terms) = Desk.OfferedWaitlistTerms(draftId, termsId, bedId);
        if (status == 403) return StatusCode(status, MoveInErrors.Forbidden());
        return terms is null ? NotFound(MoveInErrors.Missing()) : PartialView(Views + "WaitlistTerms.cshtml", terms);
    }

    [HttpGet(CareEndpoints.MoveInWaitlistRecord)]
    public IActionResult WaitlistRecord([FromQuery] DeskQuery query)
        => PartialView(Views + "WaitlistRecord.cshtml", Desk.WaitlistOnFile(query.DraftId));

    [HttpPost(MoveInEndpoints.Waitlist)]
    [ValidateAntiForgeryToken]
    public IActionResult JoinTheWaitlist([FromRoute] string draftId, [FromBody] WaitlistPage sent,
        [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator,
        [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        if (DropsItsConnection()) return new EmptyResult();
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var validation = this.Validate<WaitlistPage, WaitlistRules>(sent);
        if (!validation.IsValid) return validation.ToErrorResponse();
        var result = Desk.JoinTheWaitlist(draftId, sent, coordinator ?? string.Empty, operation);
        return result.Entry is { } entry ? StatusCode(result.Status, entry) : StatusCode(result.Status, result.Error);
    }
}

public static partial class MoveInEndpoints
{
    public const string Waitlist = Page + "/waitlist";

    public const string WaitlistTerms = Page + "/waitlist-terms/{TermsId}";
}

public sealed class WaitlistTermsQuery
{
    public string DraftId { get; set; } = string.Empty;

    public string BedId { get; set; } = string.Empty;

    public string TermsId { get; set; } = string.Empty;
}

public sealed record WaitlistRecordPage(string Record, string Draft, string Community);

public sealed record WaitlistTermsPage(string Hold, WaitlistPage? Entry);
