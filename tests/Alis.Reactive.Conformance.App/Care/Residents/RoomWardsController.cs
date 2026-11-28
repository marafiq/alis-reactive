using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoomWardsController : Controller
{
    [HttpGet(CarePages.RoomWardsLookedUpWithAnUnsetWard)]
    public IActionResult LookedUpWithAnUnsetWard() => View("/Care/Residents/RoomWards.cshtml", new RoomWards(["East", null]));

    [HttpGet(CarePages.RoomWardsLookedUpInTheEastWard)]
    public IActionResult LookedUpInTheEastWard() => View("/Care/Residents/RoomWards.cshtml", new RoomWards(["East"]));

    [HttpGet(RoomWardsEndpoints.Doses)]
    public IActionResult Doses() => Json(new List<RoomedDose>
    {
        new("Aspirin", new DoseRoom("East")),
        new("Digoxin", new DoseRoom(null)),
        new("Insulin", null),
    });
}

public sealed record RoomWards(IReadOnlyList<string?> Wards);

public sealed record RoomedDose(string Medicine, DoseRoom? Room);

public sealed record DoseRoom(string? Ward);

public static class RoomWardsEndpoints
{
    public const string Doses = CarePages.Area + "residents/room-wards/doses";
}
