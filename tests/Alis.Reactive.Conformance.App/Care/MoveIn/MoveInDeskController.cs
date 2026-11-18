using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
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
        => Desk.PageOf(draftId) is { } page ? View(Views + "MoveIn.cshtml", page) : NotFound(MoveInErrors.Missing());

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
