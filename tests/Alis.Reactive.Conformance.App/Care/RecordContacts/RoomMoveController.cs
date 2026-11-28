using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoomMoveController : Controller
{
    [HttpGet(CarePages.RoomMove)]
    public IActionResult ToNorth() => View("/Care/RecordContacts/RoomMove.cshtml", new RoomMove { Room = "North 12", Bedside = new MoveBedside { Bed = "B2" } });
}

public sealed class RoomMove
{
    public string? Room { get; set; }

    public MoveBedside Bedside { get; set; } = new();
}

public sealed class MoveBedside
{
    public string? Bed { get; set; }
}
