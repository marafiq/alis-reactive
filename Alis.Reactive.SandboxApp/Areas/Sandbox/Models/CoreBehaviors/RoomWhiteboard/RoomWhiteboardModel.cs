namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.CoreBehaviors.RoomWhiteboard
{
    public class RoomWhiteboardModel
    {
        public const string BoardId = "room-board";

        public string? Goal { get; set; }

        public string? PageWho { get; set; }

        public string? Note { get; set; }
    }

    public sealed class BoardPost
    {
        public string Nurse { get; set; } = "";

        public string Room { get; set; } = "";

        public BoardCheck Check { get; set; } = new BoardCheck();
    }

    public sealed class BoardCheck
    {
        public int? Minutes { get; set; }

        public string Goal { get; set; } = "";
    }

    public sealed class CheckSchedule
    {
        public int Minutes { get; set; }
    }

    public static class RoomWhiteboardEvents
    {
        public static readonly AppEvent<BoardPost> BoardPosted = AppEvent.Declare<BoardPost>();

        public static readonly AppEvent<CheckSchedule> CheckScheduled = AppEvent.Declare<CheckSchedule>();
    }
}
