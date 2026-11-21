using System.Globalization;
using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpPut(MoveInEndpoints.Draft)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        [FromRoute] string draftId,
        [FromBody] MoveInPage moveIn,
        [FromHeader(Name = MoveInEndpoints.VersionHeader)] int version,
        [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator,
        [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        if (DropsItsConnection()) return new EmptyResult();
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (coordinator is null || !Desk.Knows(coordinator)) return StatusCode(StatusCodes.Status403Forbidden, MoveInErrors.Forbidden());

        var coverage = this.Validate<MoveInPage, MoveInRules>(moveIn, MoveInRules.Draft);
        if (!coverage.IsValid) return coverage.ToErrorResponse();

        if (moveIn.IncludesDiet)
        {
            var diet = this.Validate<DietRequest, DietRules>(new DietRequest { Diet = moveIn.Diet });
            if (!diet.IsValid) return diet.ToErrorResponse();
            if (Desk.DraftKitchenRefusal(draftId, moveIn) is { } refusal)
            {
                ModelState.AddModelError("Diet.Needs", refusal);
                return ValidationProblem(ModelState);
            }
        }

        await World.Reach(GateName.Of(CompositionGate.Answer), Request.Path);
        if (World.NextAnswer(Request.Path) == 503) return StatusCode(503);
        var verdict = Desk.Save(draftId, version, coordinator, operation, moveIn);
        if (verdict.Closed) return StatusCode(StatusCodes.Status410Gone, MoveInErrors.Closed());
        if (verdict.MedicationRefusal is { } medicationRefusal)
        {
            ModelState.AddModelError(nameof(MoveInPage.Medications), medicationRefusal);
            return ValidationProblem(ModelState);
        }
        if (verdict.Stale is { } stale) return Conflict(stale);
        if (verdict.Saved is not { } saved) return NotFound(MoveInErrors.Missing());

        Response.Headers.ETag = $"\"{saved.Receipt.Version.ToString(CultureInfo.InvariantCulture)}\"";
        return Json(saved);
    }
}

public static partial class MoveInEndpoints
{
    public const string Draft = Page;

    public const string VersionHeader = "If-Match";

    public const string CoordinatorHeader = "X-Coordinator";

    public const string OperationHeader = "X-Operation";
}

public static class MoveInDraftEvents
{
    public static readonly AppEvent<DraftCommand> Command = AppEvent.Declare<DraftCommand>();
}

public sealed class DraftCommand
{
    public DraftCommandKind Kind { get; set; }
    public ContactInput? NewContact { get; set; }
    public string RemoveId { get; set; } = string.Empty;
    public int Version { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<DraftCommandKind>))]
public enum DraftCommandKind
{
    SaveDraft,
    SubmitAdmission,
    SaveContacts,
    AddContact,
    RemoveContact,
    SaveMedications,
    AddMedication,
    RemoveMedication,
}
