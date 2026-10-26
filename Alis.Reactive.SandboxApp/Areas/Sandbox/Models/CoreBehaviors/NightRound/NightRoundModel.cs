namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.CoreBehaviors.NightRound
{
    // Journey: the night nurse starts the round on the Garden wing. The round opens as the page does and
    // the desk is told; each room is checked from the page; room 12's bell and thermostat are heard; a
    // handover note is filed with or without its author; and a resident is flagged from the chart's own
    // button. Every trigger the framework knows starts something the nurse sees.
    public class NightRoundModel
    {
        /// <summary>The id of room 12's bell.</summary>
        public const string BellId = "room-bell";

        /// <summary>The room being checked.</summary>
        public string Room { get; set; } = "";

        /// <summary>Room 12's target temperature; the thermostat's value.</summary>
        public int Target { get; set; } = 70;

        /// <summary>What the nurse saw in the room.</summary>
        public string? Note { get; set; }
    }

    /// <summary>A room checked on the round: which room, and what the nurse saw.</summary>
    public sealed class RoomCheck
    {
        public string Room { get; set; } = "";

        public string Note { get; set; } = "";
    }

    /// <summary>A handover note filed from the round; the author is missing when it is filed unsigned.</summary>
    public sealed class HandoverNote
    {
        public string Author { get; set; } = "";
    }

    /// <summary>A resident flagged from the chart: the room and the reason.</summary>
    public sealed class ResidentFlag
    {
        public string Room { get; set; } = "";

        public string Reason { get; set; } = "";
    }

    /// <summary>The document events the night round raises and hears.</summary>
    public static class NightRoundEvents
    {
        public static readonly AppEvent RoundOpened = AppEvent.Declare();

        public static readonly AppEvent<RoomCheck> RoomChecked = AppEvent.Declare<RoomCheck>();

        public static readonly AppEvent<HandoverNote> NoteFiled = AppEvent.Declare<HandoverNote>();

        public static readonly AppEvent<ResidentFlag> ResidentFlagged = AppEvent.Declare<ResidentFlag>();
    }
}
