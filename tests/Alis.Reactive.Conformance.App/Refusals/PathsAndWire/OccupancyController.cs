using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class OccupancyController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(RefusalPages.OccupancyByRoomKey)]
    public IActionResult ByRoomKey() => View("/Refusals/PathsAndWire/OccupancyByRoomKey.cshtml", OfEastFour());

    [HttpGet(RefusalPages.OccupancyByRoomCode)]
    public IActionResult ByRoomCode() => View("/Refusals/PathsAndWire/OccupancyByRoomCode.cshtml", OfEastFour());

    [HttpPost(Occupancies.SavedBeds)]
    public IActionResult SaveBeds(OccupiedBeds saved) => Content(string.Join(", ", saved.Beds.Select(entry => $"{entry.Key}: {entry.Value}")), PlainText);

    private static RoomOccupancy OfEastFour() => new(new RoomKey { Ward = "East", Room = "4" }, 0);
}

public sealed record RoomOccupancy(RoomKey Key, int Occupied);

public sealed class RoomKey
{
    public string Ward { get; init; } = "";

    public string Room { get; init; } = "";
}

public sealed class OccupiedBedsByRoom
{
    public Dictionary<RoomKey, int> Beds { get; init; } = [];
}

public sealed class OccupiedBeds
{
    public Dictionary<string, int> Beds { get; init; } = [];
}

public static class Occupancies
{
    public const string SavedBeds = RefusalPages.Area + "paths-and-wire/occupancy/beds";

    public static readonly AppEvent Save = AppEvent.Declare();
}
