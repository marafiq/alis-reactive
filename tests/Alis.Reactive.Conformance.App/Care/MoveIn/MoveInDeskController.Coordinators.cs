using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpGet(CareEndpoints.MoveInCoordinatorCard)]
    public IActionResult CoordinatorCard([FromQuery] string? draftId)
    {
        if (string.IsNullOrEmpty(draftId)) return Content("Choose a move-in to open its coordinators' line.");
        if (Desk.PageOf(draftId) is not { } page) return NotFound(MoveInErrors.Missing());
        if (!Desk.CoordinatorServes(page.CoordinatorId, page.Location.CommunityId)) return StatusCode(403, MoveInErrors.Forbidden());
        return PartialView(Views + "CoordinatorLine.cshtml", new CoordinatorLinePage { DraftId = draftId, CoordinatorId = page.CoordinatorId });
    }

    [HttpPost(MoveInEndpoints.WardPreparation)]
    [ValidateAntiForgeryToken]
    [Consumes("multipart/form-data")]
    public IActionResult PrepareWard([FromRoute] string draftId, [FromForm] WardPreparation preparation,
        [FromHeader(Name = MoveInEndpoints.CoordinatorHeader)] string? coordinator, [FromHeader(Name = MoveInEndpoints.OperationHeader)] Guid operation)
    {
        if (DropsItsConnection()) return new EmptyResult();
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var (status, instruction) = Desk.PrepareWard(draftId, preparation, coordinator, operation);
        return new ContentResult { StatusCode = status, Content = instruction, ContentType = "text/plain" };
    }
}

public static partial class MoveInEndpoints
{
    public const string WardPreparation = Page + "/ward-preparation";
}

internal sealed partial class MoveInReads
{
    internal string? WaterOnFile(string draftId) => Desk.WaterOnFile(draftId);
}
