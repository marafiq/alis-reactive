namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.RoomOffer
{
    /// <summary>The room offer an admissions coordinator prepares for a prospective resident.</summary>
    public class RoomOfferModel
    {
        /// <summary>The care level's code: assisted, respite or memory; empty until the coordinator chooses one.</summary>
        public string CareLevel { get; set; } = "";

        /// <summary>The move-in deposit that holds the room.</summary>
        public decimal Deposit { get; set; }

        /// <summary>The room offered.</summary>
        public OfferedRoom Room { get; set; } = new();
    }

    /// <summary>A room in one of Maple Grove's wings.</summary>
    public class OfferedRoom
    {
        /// <summary>Maple, Oak or Cedar; empty until the coordinator chooses one.</summary>
        public string Wing { get; set; } = "";

        /// <summary>The floor's number; empty until chosen.</summary>
        public string Floor { get; set; } = "";

        public decimal MonthlyRate { get; set; }
    }

    /// <summary>The document events the room offer raises and hears.</summary>
    public static class RoomOfferEvents
    {
        /// <summary>The coordinator started the offer over for the next prospective resident.</summary>
        public static readonly AppEvent StartedOver = AppEvent.Declare();
    }

    /// <summary>The server's confirmation that the offer was sent.</summary>
    public sealed record RoomOfferSent(string Confirmation);
}
