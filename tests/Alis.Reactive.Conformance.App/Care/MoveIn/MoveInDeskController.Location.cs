using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    private const string TheDesksCoordinator = "CO1";

    [HttpGet(CareEndpoints.MoveInBuildings)]
    public async Task<IActionResult> Buildings([FromQuery] LocationQuery query, [FromHeader(Name = MoveInEndpoints.CommunityHeader)] string? desk)
    {
        await World.Reach(GateName.Of(LanesGate.Respond), Request.Path);
        if (DropsItsConnection()) return new EmptyResult();
        if (desk is { Length: > 0 } && !Desk.CoordinatorServes(TheDesksCoordinator, desk)) return StatusCode(StatusCodes.Status403Forbidden, MoveInErrors.Forbidden());

        return PartialView(Views + "Buildings.cshtml", Asked(query));
    }

    [HttpGet(CareEndpoints.MoveInApartments)]
    public IActionResult Apartments([FromQuery] LocationQuery query)
    {
        if (DropsItsConnection()) return new EmptyResult();
        if (Desk.BuildingOf(query.CommunityId, query.BuildingId) is { Closed: true } closed)
            return UnprocessableEntity(MoveInErrors.Refused("building-closed", closed.Name + " is closed."));

        return PartialView(Views + "Apartments.cshtml", Asked(query));
    }

    [HttpGet(CareEndpoints.MoveInBeds)]
    public IActionResult Beds([FromQuery] LocationQuery query)
    {
        if (DropsItsConnection()) return new EmptyResult();
        if (Desk.BuildingOf(query.CommunityId, query.BuildingId) is { Closed: true } closed)
            return UnprocessableEntity(MoveInErrors.Refused("building-closed", closed.Name + " is closed."));

        return PartialView(Views + "Beds.cshtml", Asked(query));
    }

    private static MoveInPage Asked(LocationQuery query) => new()
    {
        DraftId = query.DraftId,
        Location = new LocationInput { CommunityId = query.CommunityId, BuildingId = query.BuildingId, ApartmentId = query.ApartmentId },
        Care = new CareInput { Level = query.Level },
    };
}

public static partial class MoveInEndpoints
{
    public const string CommunityHeader = "X-Community";
}

public static class MoveInLocationEvents
{
    public static readonly AppEvent CommunityChosen = AppEvent.Declare();
}
