using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpPost(MoveInEndpoints.Admissions)]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitAdmission([FromRoute] string draftId, [FromBody] MoveInPage sent,
        [FromHeader(Name = MoveInEndpoints.VersionHeader)] int version,
        [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator,
        [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        if (DropsItsConnection()) return new EmptyResult();
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var check = this.Validate<MoveInPage, MoveInRules>(sent, MoveInRules.Submit);
        if (!check.IsValid) return check.ToErrorResponse();

        var decision = Desk.Admit(draftId, version, coordinator ?? string.Empty, operation, sent);
        if (decision.Taken is { } taken) return Conflict(new AdmissionConflict(new(null, taken.Conflict.Bed, taken.Conflict.Waitlist)));
        if (decision.Stale is { } stale) return Conflict(new AdmissionConflict(new(stale.Conflict.Draft, null, null)));
        return decision.Admission is { } admission ? StatusCode(decision.Status, admission) : StatusCode(decision.Status, decision.Error);
    }

    [HttpGet(CareEndpoints.MoveInArrivalChecklist)]
    public async Task<IActionResult> ArrivalChecklist([FromQuery] ChecklistQuery query)
    {
        await World.Reach(GateName.Of(LanesGate.SettleTheTail), Request.Path);
        if (ReviewAccess(query.DraftId) is { } refusal) return refusal;
        return Desk.Checklist(query.DraftId, query.ChecklistId) is { } checklist ? PartialView(Views + "ArrivalChecklist.cshtml", checklist) : NotFound(MoveInErrors.Missing());
    }

    [HttpGet(CareEndpoints.MoveInComparisons)]
    public async Task<IActionResult> Comparison([FromQuery] DraftComparisonQuery query)
    {
        await World.Reach(GateName.Of(LanesGate.SettleTheTail), Request.Path);
        if (ReviewAccess(query.DraftId) is { } refusal) return refusal;
        return Desk.ComparisonOf(query.DraftId, query.Revision) is { } comparison ? PartialView(Views + "DraftComparison.cshtml", comparison) : NotFound(MoveInErrors.Missing());
    }
}

public static partial class MoveInEndpoints
{
    public const string Admissions = Page + "/admissions";
}

public sealed class ChecklistQuery
{
    public string DraftId { get; set; } = string.Empty;
    public string ChecklistId { get; set; } = string.Empty;
}

public sealed class DraftComparisonQuery
{
    public string DraftId { get; set; } = string.Empty;
    public int Revision { get; set; }
}

public sealed record DraftComparison(int Revision, string Coordinator, string Notes);
public sealed record AdmissionConflict(AdmissionConflictDetail Conflict);
public sealed record AdmissionConflictDetail(ConflictingDraft? Draft, ConflictingBed? Bed, WaitlistOffer? Waitlist);
