using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpGet(CareEndpoints.MoveInContactRows)]
    public IActionResult ContactRows([FromQuery] SectionQuery query)
        => PartialView(Views + "ContactRows.cshtml", Desk.PageOf(query.DraftId) ?? new MoveInPage());

    [HttpPost(MoveInEndpoints.Contacts)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveContacts(
        [FromRoute] string draftId,
        [FromBody] ContactEdit edit,
        [FromHeader(Name = MoveInEndpoints.VersionHeader)] int version,
        [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator,
        [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        await World.Reach(GateName.Of(LanesGate.Respond), Request.Path);
        if (DropsItsConnection()) return new EmptyResult();
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var check = this.Validate<ContactEdit, ContactEditRules>(edit);
        if (!check.IsValid) return check.ToErrorResponse();
        if (coordinator is null || !Desk.Knows(coordinator)) return StatusCode(StatusCodes.Status403Forbidden, MoveInErrors.Forbidden());
        if (Desk.IsClosed(draftId)) return StatusCode(StatusCodes.Status410Gone, MoveInErrors.Closed());

        return Answered(Desk.SaveContacts(draftId, version, coordinator, operation, edit));
    }

    [HttpDelete(MoveInEndpoints.Contact)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveContact(
        [FromRoute] string draftId,
        [FromRoute] string removeId,
        [FromBody] ContactEdit edit,
        [FromHeader(Name = MoveInEndpoints.VersionHeader)] int version,
        [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator,
        [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        await World.Reach(GateName.Of(LanesGate.Respond), Request.Path);
        if (DropsItsConnection()) return new EmptyResult();
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var named = this.Validate<ContactEdit, ContactEditRules>(edit, ContactEditRules.Removal);
        if (!named.IsValid) return named.ToErrorResponse();
        var kept = this.Validate<ContactEdit, ContactEditRules>(edit);
        if (!kept.IsValid) return kept.ToErrorResponse();
        if (coordinator is null || !Desk.Knows(coordinator)) return StatusCode(StatusCodes.Status403Forbidden, MoveInErrors.Forbidden());
        if (Desk.IsClosed(draftId)) return StatusCode(StatusCodes.Status410Gone, MoveInErrors.Closed());

        return Answered(Desk.RemoveContact(draftId, removeId, version, coordinator, operation, edit));
    }

    [HttpGet(CareEndpoints.MoveInPayerExplanation)]
    public IActionResult PayerExplanation([FromQuery] PayerExplanationQuery query)
        => PartialView(Views + "PayerExplanation.cshtml", new PayerExplanationPage { Explanation = Desk.PayerExplanationOf(query.DraftId, query.ContactId, query.PolicyNumber) });

    private IActionResult Answered(ContactsVerdict verdict)
    {
        if (verdict.Stale is { } stale) return Conflict(stale);
        if (verdict.DependedOn is { } dependency) return Conflict(dependency);
        if (verdict.TooMany) return UnprocessableEntity(MoveInErrors.Refused("too-many-contacts", "An admission has at most four contacts."));
        if (verdict.Saved is not { } saved) return NotFound(MoveInErrors.Missing());

        return Json(saved);
    }
}

public static partial class MoveInEndpoints
{
    public const string Contacts = Page + "/contacts";

    public const string Contact = Contacts + "/{RemoveId}";
}

public static class MoveInContactEvents
{
    public static readonly AppEvent<ContactCommand> Command = AppEvent.Declare<ContactCommand>();

    public static readonly AppEvent Added = AppEvent.Declare();
}

public sealed class ContactCommand
{
    public ContactCommandKind Kind { get; set; }

    public ContactInput? NewContact { get; set; }

    public string RemoveId { get; set; } = string.Empty;

    public int Version { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<ContactCommandKind>))]
public enum ContactCommandKind
{
    SaveContacts,
    AddContact,
    RemoveContact,
}

public sealed class PayerExplanationQuery
{
    public string DraftId { get; set; } = string.Empty;

    public string ContactId { get; set; } = string.Empty;

    public string PolicyNumber { get; set; } = string.Empty;

    public string Culture { get; set; } = string.Empty;
}

public sealed class PayerExplanationPage
{
    public string? Explanation { get; set; }
}
