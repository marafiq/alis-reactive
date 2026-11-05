namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class OutingRosterModel
    {
        public string[]? MobilityAids { get; set; }

        public string? LiftAttendant { get; set; }
    }

    public sealed record MobilityAidOption(string Code, string Name);

    public sealed record OutingRosterAnswer(string Residents);

    public sealed record OutingBookingAnswer(string Confirmation);

    public sealed record OutingBookingRefusal(string Reason);
}
