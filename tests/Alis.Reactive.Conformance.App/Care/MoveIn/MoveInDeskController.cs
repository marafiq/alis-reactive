using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController : Controller
{
    private const string Views = "/Care/MoveIn/";
    private const string HelensDraft = "D100";
    private const string TheEmptyDraft = "D200";

    [HttpGet(CarePages.MoveIn)]
    public IActionResult OfHelensDraft() => PageOf(HelensDraft);

    [HttpGet(CarePages.MoveInOfTheEmptyDraft)]
    public IActionResult OfTheEmptyDraft() => PageOf(TheEmptyDraft);

    private WorldData World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;

    private MoveInDesk Desk => World.Owns(MoveInDesk.Opened);

    private IActionResult PageOf(string draftId)
    {
        if (Desk.PageOf(draftId) is not { } page) return NotFound(MoveInErrors.Missing());
        if (page.Status == DraftStatus.Discarded) return View(Views + "DiscardedDraft.cshtml", Desk.DraftActionsOf(draftId));
        if (page.Diet.Referral is { } imported)
        {
            var check = this.Validate<DietRequest, DietRules>(new DietRequest { Diet = new DietInput { Needs = imported, MealCount = page.Diet.MealCount } });
            page.Diet.ReferralRefusal = check.Errors.FirstOrDefault()?.ErrorMessage;
        }
        return View(Views + "MoveIn.cshtml", page);
    }

    private bool DropsItsConnection()
    {
        if (!Desk.Drops(Request.Path)) return false;

        HttpContext.Abort();
        return true;
    }
}

public static partial class MoveInEndpoints
{
    public const string Page = CarePages.MoveIns + "{DraftId}";
}

internal static class MoveInErrors
{
    internal static ErrorEnvelope Forbidden() => new(new ErrorDetail("forbidden", "This coordinator cannot perform this action.", new RecoveryAdvice(false, string.Empty)));

    internal static ErrorEnvelope Missing() => new(new ErrorDetail("missing", "The admission record was not found.", new RecoveryAdvice(false, string.Empty)));

    internal static ErrorEnvelope Closed() => new(new ErrorDetail("closed", "This draft is closed.", new RecoveryAdvice(false, string.Empty)));

    internal static ErrorEnvelope Refused(string code, string message) => new(new ErrorDetail(code, message, new RecoveryAdvice(false, string.Empty)));
}
