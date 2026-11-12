using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class RoomsController : Controller
{
    [HttpGet(RefusalPages.TheRoomBeforeTheFirst)]
    public IActionResult BeforeTheFirst() => View("/Refusals/ArrayLambdas/Rooms.cshtml", TheRoomAt(-1));

    [HttpGet(RefusalPages.TheFirstRoom)]
    public IActionResult First() => View("/Refusals/ArrayLambdas/Rooms.cshtml", TheRoomAt(0));

    private static RoomPick TheRoomAt(int position)
        => new(position, [new ItemStart { Name = "Hall", Active = true }, new ItemStart { Name = "Bath" }]);
}

public sealed record RoomPick(int Position, IReadOnlyList<ItemStart> Rooms);
