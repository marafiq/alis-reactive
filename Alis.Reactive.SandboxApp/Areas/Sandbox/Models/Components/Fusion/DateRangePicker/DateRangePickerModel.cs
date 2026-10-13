namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class DateRangePickerModel
    {
        public DateTime[]? StayPeriod { get; set; }
        public string? ResidentName { get; set; }
    }

    /// <summary>The confirmation the server returns once the respite stay is reserved.</summary>
    public sealed record StayReservation(string Confirmation);
}
