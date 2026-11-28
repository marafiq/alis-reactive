using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoomsController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.RoomsOfTheFloor)]
    public IActionResult OfTheFloor() => View("/Care/Admission/Rooms.cshtml", new FloorRooms("East floor", ["B12", "B14"]));

    [HttpGet(CarePages.OpenRoom)]
    public IActionResult Open() => View("/Care/Admission/OpenRoom.cshtml", new RoomDesk("East floor", ""));

    [HttpGet(CareEndpoints.Room)]
    public IActionResult Room(string code) => Content($"Room {code} is ready", PlainText);
}

public sealed record FloorRooms(string Floor, IReadOnlyList<string> Codes);

public sealed record RoomDesk(string Floor, string Code);

public sealed class RoomQuery
{
    public string Code { get; set; } = "";
}

public static class RoomDesks
{
    public static readonly AppEvent Open = AppEvent.Declare();
}
