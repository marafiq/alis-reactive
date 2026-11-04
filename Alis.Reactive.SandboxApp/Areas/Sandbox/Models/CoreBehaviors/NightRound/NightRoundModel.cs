namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.CoreBehaviors.NightRound
{
    public class NightRoundModel
    {
        public const string BellId = "room-bell";

        public string Room { get; set; } = "";

        public int Target { get; set; } = 70;

        public string? Note { get; set; }
    }

    public sealed class RoomCheck
    {
        public string Room { get; set; } = "";

        public string Note { get; set; } = "";
    }

    public sealed class HandoverNote
    {
        public string Author { get; set; } = "";
    }

    public sealed class ResidentFlag
    {
        public string Room { get; set; } = "";

        public string Reason { get; set; } = "";
    }

    public static class NightRoundEvents
    {
        public static readonly AppEvent RoundOpened = AppEvent.Declare();

        public static readonly AppEvent<RoomCheck> RoomChecked = AppEvent.Declare<RoomCheck>();

        public static readonly AppEvent<HandoverNote> NoteFiled = AppEvent.Declare<HandoverNote>();

        public static readonly AppEvent<ResidentFlag> ResidentFlagged = AppEvent.Declare<ResidentFlag>();
    }
}
