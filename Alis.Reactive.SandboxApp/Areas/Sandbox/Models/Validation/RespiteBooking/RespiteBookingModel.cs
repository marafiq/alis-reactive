namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// A short respite stay booked by the admissions coordinator. The community, daily rate,
    /// expected nights, and transport answer are nullable in the model: blank means
    /// "not entered yet", never zero or "no".
    /// </summary>
    public class RespiteBookingModel
    {
        public string? ResidentName { get; set; }
        public int? CommunityId { get; set; }
        public decimal? DailyRate { get; set; }
        public int? ExpectedNights { get; set; }
        public bool? NeedsTransport { get; set; }
    }

    /// <summary>The booking confirmation the server returns once the stay is booked.</summary>
    public sealed record RespiteBookingConfirmation(string Confirmation);
}
