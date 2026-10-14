namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ContextMenu
{
    /// <summary>
    /// The Maple wing room board on the evening shift: the charge nurse right-clicks a resident to move
    /// them to an open room, flag them for the night nurse or read their care plan.
    /// </summary>
    public sealed class FusionContextMenuModel
    {
        /// <summary>The shift is handing over; the board is read-only until it ends.</summary>
        public bool ShiftHandover { get; set; }

        /// <summary>The room a move is saving to, while the server has not answered; empty otherwise.</summary>
        public string PendingMove { get; set; } = "";

        /// <summary>The residents on the wing tonight, in room order.</summary>
        public IReadOnlyList<BoardResident> Residents { get; init; } = [];

        /// <summary>The rooms open on the wing tonight, as the board lists them.</summary>
        public string OpenRooms { get; init; } = "";
    }

    /// <summary>A resident on the room board.</summary>
    public sealed record BoardResident(string Key, string Name, string Room, string CareLevel, string CarePlan);

    /// <summary>The room the board asks about, by the menu item that offers it.</summary>
    public sealed class RoomCheckQuery
    {
        public string Room { get; set; } = "";
    }

    /// <summary>Whether a room is held for someone else tonight, and the note that says so.</summary>
    public sealed class RoomCheck
    {
        public bool Held { get; set; }

        public string Note { get; set; } = "";
    }

    /// <summary>A move the charge nurse asks for: which resident, to the room the menu item offers.</summary>
    public sealed class ResidentMoveRequest
    {
        public string Resident { get; set; } = "";

        public string Room { get; set; } = "";
    }

    /// <summary>The answer to a move: what happened, the resident's room now, and the rooms open after it.</summary>
    public sealed class ResidentMoveResult
    {
        public string Message { get; set; } = "";

        public string Room { get; set; } = "";

        public string OpenRooms { get; set; } = "";
    }
}
