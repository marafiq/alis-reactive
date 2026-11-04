namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class MoveInBookingModel
    {
        public DateOnly? MoveInDay { get; set; }

        public DateTime? WelcomeVisit { get; set; }

        public DateTimeOffset? TruckArrives { get; set; }
    }

    public sealed record MoveInBooked(string Confirmation);
}
