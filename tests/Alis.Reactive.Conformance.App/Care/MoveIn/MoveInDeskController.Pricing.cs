using System.Globalization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpPost(MoveInEndpoints.Quotes)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CalculateQuote([FromRoute] string draftId, [FromBody] QuoteRequest request,
        [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator, [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        if (DropsItsConnection()) return new EmptyResult();
        if (coordinator is null || !Desk.Knows(coordinator)) return StatusCode(403, MoveInErrors.Forbidden());
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var validation = this.Validate<QuoteRequest, QuoteRequestRules>(request);
        if (!validation.IsValid) return validation.ToErrorResponse();
        var verdict = Desk.CalculateQuote(draftId, request, coordinator, operation);
        await World.Reach(GateName.Of(CompositionGate.Answer), Request.Path);
        if (verdict.Reply is { } reply)
        {
            Response.Headers[MoveInEndpoints.MonthlyTotalHeader] = reply.Quote.Pricing.MonthlyTotal.ToString("0.00", CultureInfo.InvariantCulture);
            return Json(reply);
        }
        return verdict.Status switch
        {
            403 => StatusCode(403, MoveInErrors.Forbidden()),
            404 => NotFound(MoveInErrors.Missing()),
            410 => StatusCode(410, MoveInErrors.Closed()),
            _ => StatusCode(verdict.Status, MoveInErrors.Refused("pricing-refused", verdict.Reason!)),
        };
    }

    [HttpGet(CareEndpoints.MoveInAssistance)]
    public async Task<IActionResult> Assistance([FromQuery] string? draftId, [FromQuery] string? reference, [FromQuery] int revision)
    {
        if (draftId is null || reference is null) return Content("Choose a quote to read assistance.");
        await World.Reach(GateName.Of(CompositionGate.Answer), Request.Path);
        var terms = Desk.AssistanceOf(draftId, reference, revision);
        return terms.Status switch
        {
            200 => PartialView(Views + "CoverageTerms.cshtml", new CoverageTerms(terms.Terms!)),
            403 => StatusCode(403, MoveInErrors.Forbidden()),
            409 => Conflict(MoveInErrors.Refused("assistance-stale", "The assistance terms have changed.")),
            _ => NotFound(MoveInErrors.Missing()),
        };
    }
}

public sealed class QuoteAssistanceQuery
{
    public string DraftId { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public int Revision { get; set; }
}

public static partial class MoveInEndpoints
{
    public const string Quotes = Page + "/quotes";
    public const string MonthlyTotalHeader = "X-Monthly-Total";
}

internal sealed class QuoteRequestRules : ReactiveValidator<QuoteRequest>
{
    public QuoteRequestRules()
    {
        RuleFor(q => q.Services.Codes).NotEmpty().WithMessage("Choose at least one service.");
        RuleForEach(q => q.Services.Visits).ChildRules(visit => visit.RuleFor(v => v.Count).InclusiveBetween(0, 14).WithMessage("Service visits must be between 0 and 14."));
    }
}

internal static partial class MoveInValidators
{
    static partial void ForTheServices(ReactiveFluentValidationBuilder validators) => validators.Add<QuoteRequestRules>();
}
