namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ContextMenu
{
    public sealed class FusionContextMenuModel
    {
        public bool ShiftHandover { get; set; }

        public string PendingMove { get; set; } = "";

        public IReadOnlyList<BoardResident> Residents { get; init; } = [];

        public string OpenRooms { get; init; } = "";
    }

    public sealed record BoardResident(string Key, string Name, string Room, string CareLevel, string CarePlan);

    public sealed class RoomCheckQuery
    {
        public string Room { get; set; } = "";
    }

    public sealed class RoomCheck
    {
        public bool Held { get; set; }

        public string Note { get; set; } = "";
    }

    public sealed class ResidentMoveRequest
    {
        public string Resident { get; set; } = "";

        public string Room { get; set; } = "";
    }

    public sealed class ResidentMoveResult
    {
        public string Message { get; set; } = "";

        public string Room { get; set; } = "";

        public string OpenRooms { get; set; } = "";
    }
}
