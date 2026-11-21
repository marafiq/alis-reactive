using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoomStatesController : Controller
{
    [HttpGet(CarePages.RoomStates)]
    public IActionResult OfTheWard() => View("/Care/BedBoard/RoomStates.cshtml", new RoomStates([new StatedRoom("North 1", RoomState.Open), new StatedRoom("North 2", RoomState.Closed)]));

    [HttpGet(CarePages.RoomStatesSwapped)]
    public IActionResult OfTheWardSwapped() => View("/Care/BedBoard/RoomStates.cshtml", new RoomStates([new StatedRoom("North 1", RoomState.Closed), new StatedRoom("North 2", RoomState.Open)]));
}

public sealed record RoomStates(IReadOnlyList<StatedRoom> Rooms);

public sealed record StatedRoom(string Room, RoomState? State);

public enum RoomState
{
    Closed,
    Open,
}
