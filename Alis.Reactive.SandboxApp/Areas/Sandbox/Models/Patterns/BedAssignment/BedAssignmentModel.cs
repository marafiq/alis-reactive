namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.BedAssignment
{
    public class BedAssignmentModel
    {
        public string Wing { get; set; } = "";

        public string Room { get; set; } = "";

        public string Bed { get; set; } = "";

        public IReadOnlyList<BedWing> Wings { get; set; } = [];
    }

    public sealed record BedWing(string Code, string Name);

    public sealed record BedRoom(string Number, string Name);

    public sealed record OpenBed(string Code, string Name);

    public sealed class WingRoomsQuery
    {
        public string Wing { get; set; } = "";
    }

    public sealed class RoomBedsQuery
    {
        public string Wing { get; set; } = "";

        public string Room { get; set; } = "";
    }

    public sealed record WingRooms(IReadOnlyList<BedRoom> Rooms, string Summary);

    public sealed record RoomBeds(IReadOnlyList<OpenBed> Beds, string Summary);

    public sealed record BedAssignmentAnswer(string Message);
}
