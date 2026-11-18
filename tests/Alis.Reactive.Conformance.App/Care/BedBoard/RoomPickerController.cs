using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoomPickerController : Controller
{
    private const string Move = "/Care/BedBoard/RoomPicker.cshtml";

    [HttpGet(CarePages.RoomPicker)]
    public IActionResult ToTheNorth() => View(Move, new RoomPicker { Room = ["north-2"], Wings = Wings() });

    [HttpGet(CarePages.RoomPickerToTheSouth)]
    public IActionResult ToTheSouth() => View(Move, new RoomPicker { Room = ["south-1"], Wings = Wings() });

    private static IReadOnlyList<RoomNode> Wings()
    {
        var north = new RoomNode("north", "North wing", [new RoomNode("north-1", "North 1", null), new RoomNode("north-2", "North 2", null)]);
        var south = new RoomNode("south", "South wing", [new RoomNode("south-1", "South 1", null)]);
        return [north, south];
    }
}

public sealed class RoomPicker
{
    public string[]? Room { get; set; }

    public IReadOnlyList<RoomNode> Wings { get; init; } = [];
}

public sealed record RoomNode(string Code, [property: JsonPropertyName("title")] string Label, IReadOnlyList<RoomNode>? Rooms);
