namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.BedAssignment
{
    /// <summary>
    /// The bed an admissions coordinator assigns a new resident: a wing, then a room of that wing with an open
    /// bed, then one of that room's open beds.
    /// </summary>
    public class BedAssignmentModel
    {
        /// <summary>The wing's code; empty until the coordinator chooses one.</summary>
        public string Wing { get; set; } = "";

        /// <summary>The room's number; empty until chosen.</summary>
        public string Room { get; set; } = "";

        /// <summary>The bed's code, its room and letter; empty until chosen.</summary>
        public string Bed { get; set; } = "";

        /// <summary>The wings the wing list offers, rendered with the page.</summary>
        public IReadOnlyList<BedWing> Wings { get; set; } = [];
    }

    /// <summary>A wing of Maple Grove.</summary>
    public sealed record BedWing(string Code, string Name);

    /// <summary>A room the room list offers.</summary>
    public sealed record BedRoom(string Number, string Name);

    /// <summary>An open bed the bed list offers.</summary>
    public sealed record OpenBed(string Code, string Name);

    /// <summary>The rooms lookup: <c>?Wing=</c>, the wing chosen.</summary>
    public sealed class WingRoomsQuery
    {
        public string Wing { get; set; } = "";
    }

    /// <summary>The beds lookup: <c>?Wing=&amp;Room=</c>, the wing and the room chosen.</summary>
    public sealed class RoomBedsQuery
    {
        public string Wing { get; set; } = "";

        public string Room { get; set; } = "";
    }

    /// <summary>The rooms of a wing that have an open bed, and the sentence that counts them.</summary>
    public sealed record WingRooms(IReadOnlyList<BedRoom> Rooms, string Summary);

    /// <summary>The open beds of a room, and the sentence that counts them.</summary>
    public sealed record RoomBeds(IReadOnlyList<OpenBed> Beds, string Summary);

    /// <summary>The server's answer to an assignment: the confirmation, or what is missing.</summary>
    public sealed record BedAssignmentAnswer(string Message);
}
