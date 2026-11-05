namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.RoomOffer
{
    public class RoomOfferModel
    {
        public string CareLevel { get; set; } = "";

        public decimal Deposit { get; set; }

        public OfferedRoom Room { get; set; } = new();
    }

    public class OfferedRoom
    {
        public string Wing { get; set; } = "";

        public string Floor { get; set; } = "";

        public decimal MonthlyRate { get; set; }
    }

    public static class RoomOfferEvents
    {
        public static readonly AppEvent StartedOver = AppEvent.Declare();
    }

    public sealed record RoomOfferSent(string Confirmation);
}
