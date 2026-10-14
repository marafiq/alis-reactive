namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Respite stay: the coordinator books Eleanor Voss into Garden Suite 3 for the days the family asks
    /// for, and the server prices and reserves the stay.
    /// </summary>
    public class DateRangePickerModel
    {
        /// <summary>The day the family asked Eleanor to arrive.</summary>
        public static DateTime FamilyArrival => new DateTime(2026, 12, 10);

        /// <summary>The day the family asked Eleanor to leave.</summary>
        public static DateTime FamilyDeparture => new DateTime(2026, 12, 17);

        /// <summary>The first day Garden Suite 3 is free.</summary>
        public static DateTime SuiteFreeFrom => new DateTime(2026, 12, 7);

        /// <summary>The last day Garden Suite 3 is free before it is booked again.</summary>
        public static DateTime SuiteFreeThrough => new DateTime(2026, 12, 20);

        /// <summary>A respite stay is billed by the night.</summary>
        public const decimal NightlyRate = 245m;

        /// <summary>The arrival and departure days.</summary>
        public DateTime[]? StayPeriod { get; set; }
    }

    /// <summary>The stay the server prices and reserves: the day Eleanor arrives and the day she leaves.</summary>
    public sealed class RespiteStayDates
    {
        public DateTime? Arrives { get; set; }

        public DateTime? Leaves { get; set; }
    }

    /// <summary>The price of the stay, written out.</summary>
    public sealed record RespiteQuote(string Quote);

    /// <summary>The confirmation the server returns once the respite stay is reserved.</summary>
    public sealed record StayReservation(string Confirmation);
}
