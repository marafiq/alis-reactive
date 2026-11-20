using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpGet(MoveInEndpoints.MedicationRows)]
    public IActionResult MedicationRows([FromQuery] SectionQuery query)
    {
        if (ReviewAccess(query.DraftId) is { } refusal) return refusal;
        var page = Desk.PageOf(query.DraftId)!;
        return PartialView(Views + "MedicationRows.cshtml", new MedicationEdit { Medications = page.Medications });
    }

    [HttpPost(MoveInEndpoints.Medications)]
    [ValidateAntiForgeryToken]
    public IActionResult SaveMedications([FromRoute] string draftId, [FromForm] MedicationEdit edit,
        [FromHeader(Name = MoveInEndpoints.VersionHeader)] int version, [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator,
        [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        if (DropsItsConnection()) return new EmptyResult();
        return MedicationResult(draftId, edit, null, version, coordinator, operation);
    }

    [HttpDelete(MoveInEndpoints.Medication)]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveMedication([FromRoute] string draftId, [FromRoute] string removeId, [FromBody] MedicationEdit edit,
        [FromHeader(Name = MoveInEndpoints.VersionHeader)] int version, [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator,
        [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        if (DropsItsConnection()) return new EmptyResult();
        return MedicationResult(draftId, edit, removeId, version, coordinator, operation);
    }

    private IActionResult MedicationResult(string draftId, MedicationEdit edit, string? removeId, int version, string? coordinator, Guid operation)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var check = this.Validate<MedicationEdit, MedicationEditRules>(edit);
        if (!check.IsValid) return check.ToErrorResponse();
        var decision = Desk.SaveMedications(draftId, edit, removeId, version, coordinator, operation);
        if (decision.Field is { } field)
        {
            ModelState.AddModelError(field, decision.Reason!);
            return ValidationProblem(ModelState);
        }
        return decision.Reply is { } reply ? Json(reply) : StatusCode(decision.Status, MoveInErrors.Refused("prescription-change", "The medication orders changed. Reopen this draft."));
    }

    [HttpPost(MoveInEndpoints.Page + "/pharmacy-record")]
    public IActionResult PharmacyRecord([FromRoute] string draftId)
    {
        var reading = Desk.PharmacyRecord(draftId);
        return reading.Record is { } record ? Json(record) : StatusCode(reading.Status);
    }

    [HttpPost(MoveInEndpoints.Pharmacy)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckMedications([FromRoute] string draftId, [FromBody] MoveInPage sent, [FromQuery] string? reference,
        [FromHeader(Name = MoveInEndpoints.ReviewHeader)] string? reviewHeader, [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator,
        [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        await World.Reach(GateName.Of(OwnershipGate.SendsSlotInput), Request.Path);
        if (DropsItsConnection()) return new EmptyResult();
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (sent.Review.Reference is null && (Request.Query.ContainsKey(nameof(MoveInPage.Reference)) || Request.Headers.ContainsKey(MoveInEndpoints.ReviewHeader)))
        {
            ModelState.AddModelError(string.Empty, "Choose the pharmacy reference or omit it.");
            return ValidationProblem(ModelState);
        }
        var shared = await this.ValidateAsync<MoveInPage, MoveInRules>(sent, MoveInRules.Pharmacy, HttpContext.RequestAborted);
        if (!shared.IsValid) return shared.ToErrorResponse();
        var request = new PharmacyRequest { DraftId = draftId, ResidentId = sent.ResidentId, Version = sent.Version, Medications = sent.Medications, Allergies = sent.Resident.Allergies, Review = sent.Review };
        var decision = Desk.CheckPharmacy(draftId, request, reference, reviewHeader, coordinator, operation);
        if (decision.Failures is { } failures)
        {
            var clinical = await new PharmacyAllergyRules(failures).ValidateAsync(request, HttpContext.RequestAborted);
            await World.Reach(GateName.Of(OwnershipGate.ShowsFailures), Request.Path);
            return clinical.ToErrorResponse();
        }
        if (decision.Conflict is { } conflict) return Conflict(conflict);
        if (decision.Reply is { } reply) return Json(reply);
        if (decision.Reason is { } reason)
        {
            ModelState.AddModelError(string.Empty, reason);
            return ValidationProblem(ModelState);
        }
        return StatusCode(decision.Status, MoveInErrors.Refused("pharmacy-access", "This pharmacy review is unavailable."));
    }

    [HttpGet(CareEndpoints.MoveInDirections)]
    public async Task<IActionResult> PharmacyInstructions([FromQuery] ReceiptQuery query)
    {
        await World.Reach(GateName.Of(CompositionGate.Answer), Request.Path);
        return ReadPharmacyAdvice(query, false);
    }

    [HttpGet(CareEndpoints.MoveInAlternatives)]
    public async Task<IActionResult> PharmacyAlternatives([FromQuery] ReceiptQuery query)
    {
        await World.Reach(GateName.Of(LanesGate.SettleTheTail), Request.Path);
        return ReadPharmacyAdvice(query, true);
    }

    private IActionResult ReadPharmacyAdvice(ReceiptQuery query, bool alternatives)
    {
        if (ReviewAccess(query.DraftId) is { } refusal) return refusal;
        if (DropsItsConnection()) return new EmptyResult();
        return Desk.PharmacyAdvice(query.DraftId, query.Reference, alternatives) is { } advice
            ? PartialView(Views + "PharmacyAdvice.cshtml", new PharmacyAdvice(advice)) : NotFound(MoveInErrors.Missing());
    }
}

public static partial class MoveInEndpoints
{
    public const string Medications = Page + "/medications";
    public const string Medication = Medications + "/{RemoveId}";
    public const string MedicationRows = CarePages.MoveIns + "medication-rows";
    public const string Pharmacy = Page + "/pharmacy-review";
    public const string ReviewHeader = "X-Review";
}

public sealed record PharmacyAdvice(string Text);
