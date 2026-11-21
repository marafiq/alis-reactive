using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpPost(MoveInEndpoints.Reception)]
    [ValidateAntiForgeryToken]
    [Consumes("application/x-www-form-urlencoded")]
    public IActionResult SignInVisitor([FromRoute] string draftId, [FromForm] ReceptionPage sent, [FromForm] bool navigation,
        [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator, [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation,
        [FromServices] IOptions<AntiforgeryOptions> options)
    {
        if (DropsItsConnection()) return new EmptyResult();
        var token = Request.Form[options.Value.FormFieldName];
        if (token.Count != 1 || string.IsNullOrEmpty(token[0])) return BadRequest();
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var check = this.Validate<ReceptionPage, ReceptionRules>(sent);
        if (!check.IsValid) return check.ToErrorResponse();
        var result = Desk.SignIn(draftId, sent, navigation, navigation ? sent.CoordinatorId : coordinator, navigation ? sent.OperationId : operation);
        if (result.Receipt is not { } receipt) return new JsonResult(result.Error) { StatusCode = result.Status };
        return navigation ? View(Views + "ReceptionReceipt.cshtml", receipt) : PartialView(Views + "ReceptionReceipt.cshtml", receipt);
    }

    [HttpGet(MoveInEndpoints.History)]
    public IActionResult AdmissionHistory([FromRoute] string draftId)
    {
        var result = Desk.AdmissionHistoryOf(draftId);
        if (result.Page is { } history) return View(Views + "AdmissionHistory.cshtml", history);
        return result.Status == 403 ? StatusCode(403, MoveInErrors.Forbidden()) : NotFound(MoveInErrors.Missing());
    }
}

public static partial class MoveInEndpoints
{
    public const string Reception = Page + "/reception";
    public const string History = Page + "/history";
}
