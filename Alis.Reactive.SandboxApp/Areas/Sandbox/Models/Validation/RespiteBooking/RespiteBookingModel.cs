namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class RespiteBookingModel
    {
        public string? ResidentName { get; set; }
        public int? CommunityId { get; set; }
        public decimal? DailyRate { get; set; }
        public int? ExpectedNights { get; set; }

        public string? EndOfStayPlan { get; set; }

        public string? LastDayPickupTime { get; set; }
        public bool? NeedsTransport { get; set; }
    }

    public sealed record RespiteBookingConfirmation(string Confirmation);
}
