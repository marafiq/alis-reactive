using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class SpareRoomsController : Controller
{
    [HttpGet(CarePages.SpareRooms)]
    public IActionResult AboveTwo() => View("/Care/BedBoard/SpareRooms.cshtml", new SpareRooms(2, [new SpareRoom("North 1", 3), new SpareRoom("North 2", 1)]));

    [HttpGet(CarePages.SpareRoomsAboveNone)]
    public IActionResult AboveNone() => View("/Care/BedBoard/SpareRooms.cshtml", new SpareRooms(0, [new SpareRoom("North 1", 3), new SpareRoom("North 2", 1)]));
}

public sealed record SpareRooms(int MinSpare, IReadOnlyList<SpareRoom> Rooms);

public sealed record SpareRoom(string Room, int Beds);
