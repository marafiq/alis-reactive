namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.CoreBehaviors.RoomWhiteboard
{
    // Journey: the evening nurse keeps room 12's whiteboard up to date. Every kind of reaction the
    // framework runs writes something the nurse sees on the board or beside it: a property set from a
    // literal, a source or to nothing; a method called with a literal, a source, a fixed and a field
    // argument, with a result drawn once and used twice, and with a promise waited for; an element's
    // text, HTML, visibility and classes; a payload composed from several sources; and a chain run in
    // the order it was written. The whiteboard is a widget the sandbox wrote itself
    // (BroughtComponents/Whiteboard).
    public class RoomWhiteboardModel
    {
        /// <summary>The id of the room's whiteboard.</summary>
        public const string BoardId = "room-board";

        /// <summary>The goal to post on the board.</summary>
        public string? Goal { get; set; }

        /// <summary>Who to page from the board.</summary>
        public string? PageWho { get; set; }

        /// <summary>A note to add to the board.</summary>
        public string? Note { get; set; }
    }

    /// <summary>What the board said when it was posted to the desk: who, where, and the check and goal together.</summary>
    public sealed class BoardPost
    {
        public string Nurse { get; set; } = "";

        public string Room { get; set; } = "";

        public BoardCheck Check { get; set; } = new BoardCheck();
    }

    /// <summary>The check and goal a posted board carries.</summary>
    public sealed class BoardCheck
    {
        public int? Minutes { get; set; }

        public string Goal { get; set; } = "";
    }

    /// <summary>A check scheduled for the room, in minutes from now.</summary>
    public sealed class CheckSchedule
    {
        public int Minutes { get; set; }
    }

    /// <summary>The document events the whiteboard page raises and hears.</summary>
    public static class RoomWhiteboardEvents
    {
        public static readonly AppEvent<BoardPost> BoardPosted = AppEvent.Declare<BoardPost>();

        public static readonly AppEvent<CheckSchedule> CheckScheduled = AppEvent.Declare<CheckSchedule>();
    }
}
