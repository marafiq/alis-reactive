using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.BedAssignment;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Patterns;

/// <summary>
/// Bed assignment: the wings come with the page; the rooms of a wing with an open bed, and the open beds of
/// a room, come from the server as the coordinator chooses; the assignment is confirmed by the server.
/// </summary>
[Area("Sandbox")]
[Route("Sandbox/Patterns/BedAssignment")]
public class BedAssignmentController : Controller
{
    private const string Resident = "Margaret Thompson";

    private static readonly BedWing[] Wings = [new("maple", "Maple"), new("oak", "Oak"), new("cedar", "Cedar")];

    // Wing, room, and the room's open beds by their letters.
    private static readonly (string Wing, string Room, string[] Beds)[] OpenRooms =
    [
        ("maple", "112", ["A", "B"]),
        ("maple", "118", ["B"]),
        ("oak", "204", ["A"]),
        ("oak", "207", ["A", "B"]),
        ("oak", "211", ["B"]),
        ("cedar", "305", ["A", "B"]),
    ];

    [HttpGet("")]
    public IActionResult Index() =>
        View("~/Areas/Sandbox/Views/Patterns/BedAssignment/Index.cshtml", new BedAssignmentModel { Wings = Wings });

    // "Oak has 3 rooms with an open bed."
    [HttpGet("Rooms")]
    public IActionResult Rooms([FromQuery] WingRoomsQuery query)
    {
        var rooms = OpenRooms.Where(open => open.Wing == query.Wing).Select(open => new BedRoom(open.Room, $"Room {open.Room}")).ToList();
        var wing = Wings.FirstOrDefault(candidate => candidate.Code == query.Wing);
        var summary = wing is null
            ? "Choose a wing to see its rooms with an open bed."
            : $"{wing.Name} has {Counted(rooms.Count, "room")} with an open bed.";
        return Ok(new WingRooms(rooms, summary));
    }

    // "Room 207 has 2 open beds."
    [HttpGet("Beds")]
    public IActionResult Beds([FromQuery] RoomBedsQuery query)
    {
        var beds = BedsOf(query.Wing, query.Room).Select(code => new OpenBed(code, BedName(code))).ToList();
        var summary = beds.Count == 0
            ? $"Room {query.Room} has no open beds."
            : $"Room {query.Room} has {Counted(beds.Count, "open bed")}.";
        return Ok(new RoomBeds(beds, summary));
    }

    // "Margaret Thompson is assigned bed 207-B in room 207, Oak wing."
    [HttpPost("Assign")]
    public IActionResult Assign([FromBody] BedAssignmentModel? assignment)
    {
        if (assignment == null) return BadRequest();
        if (string.IsNullOrEmpty(assignment.Bed))
            return BadRequest(new BedAssignmentAnswer($"Choose a bed for {Resident}."));

        var wing = Wings.FirstOrDefault(candidate => candidate.Code == assignment.Wing);
        var isOpen = BedsOf(assignment.Wing, assignment.Room).Contains(assignment.Bed);
        if (wing is null || !isOpen)
            return BadRequest(new BedAssignmentAnswer($"Bed {assignment.Bed} is not an open bed of room {assignment.Room}."));

        return Ok(new BedAssignmentAnswer($"{Resident} is assigned bed {assignment.Bed} in room {assignment.Room}, {wing.Name} wing."));
    }

    private static IEnumerable<string> BedsOf(string? wing, string? room) =>
        OpenRooms.Where(open => open.Wing == wing && open.Room == room).SelectMany(open => open.Beds.Select(letter => $"{open.Room}-{letter}"));

    // Bed A of each room is by the window, bed B by the door.
    private static string BedName(string code) =>
        code.EndsWith("-A", StringComparison.Ordinal) ? $"Bed {code}, by the window" : $"Bed {code}, by the door";

    private static string Counted(int count, string noun) => count == 1 ? $"1 {noun}" : $"{count} {noun}s";
}
