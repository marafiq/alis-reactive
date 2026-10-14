using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ContextMenu;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// The Maple wing room board. Every page starts from the residents and open rooms below; a move answers
    /// with the board as it would be after the move and keeps nothing, so each page owns its own board.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/ContextMenu")]
    public class ContextMenuController : Controller
    {
        // Maple 218 looks open on the board, but the front desk holds it for tonight's admission.
        private const string HeldRoom = "maple-218";

        private static readonly IReadOnlyList<BoardResident> Residents =
        [
            new("margaret", "Margaret Hale", "Maple 214", "Assisted living",
                "Walks with a frame; two-person assist to shower. Low-sodium diet. Daughter Claire visits Sundays."),
            new("arthur", "Arthur Bell", "Maple 215", "Assisted living",
                "Walks with a cane; reminders for evening insulin. Diabetic diet. Son Michael calls at 7 PM."),
            new("rosa", "Rosa Diaz", "Maple 217", "Memory care",
                "Sundowns after 5 PM; quiet music helps. Finger foods. Sister Marisol visits Wednesdays.")
        ];

        // The rooms open on the wing tonight, by the code a room item's id ends with.
        private static readonly IReadOnlyList<(string Code, string Name)> OpenRooms =
        [
            ("maple-216", "Maple 216"),
            (HeldRoom, "Maple 218")
        ];

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index() =>
            View(
                "~/Areas/Sandbox/Views/Components/Fusion/ContextMenu/Index.cshtml",
                new FusionContextMenuModel
                {
                    Residents = Residents,
                    OpenRooms = string.Join(", ", OpenRooms.Select(open => open.Name))
                });

        [HttpGet("RoomCheck")]
        public IActionResult RoomCheck([FromQuery] string? room) =>
            Ok(RoomCode(room) == HeldRoom
                ? new RoomCheck { Held = true, Note = "Maple 218 is held for tonight's 9:00 PM admission." }
                : new RoomCheck());

        [HttpPost("MoveResident")]
        public IActionResult MoveResident([FromBody] ResidentMoveRequest? request)
        {
            var resident = Residents.FirstOrDefault(r => r.Key == request?.Resident);
            var code = RoomCode(request?.Room);
            var room = OpenRooms.Where(open => open.Code == code).Select(open => open.Name).FirstOrDefault();
            if (resident == null || room == null)
                return BadRequest(new ResidentMoveResult { Message = "The board could not find that resident or room." });

            if (code == HeldRoom)
                return Conflict(new ResidentMoveResult
                {
                    Message = $"{room} is held for tonight's admission; choose another room for {resident.Name}.",
                    Room = resident.Room
                });

            var openAfterMove = OpenRooms.Select(open => open.Name).Where(open => open != room).Append(resident.Room).Order();
            return Ok(new ResidentMoveResult
            {
                Message = $"{resident.Name} moved to {room}.",
                Room = room,
                OpenRooms = string.Join(", ", openAfterMove)
            });
        }

        // A room item's id is the resident's key, "-room-", then the room's code: "arthur-room-maple-216".
        private static string RoomCode(string? itemId)
        {
            const string Marker = "-room-";
            var at = itemId?.IndexOf(Marker, StringComparison.Ordinal) ?? -1;
            return at < 0 ? "" : itemId![(at + Marker.Length)..];
        }
    }
}
