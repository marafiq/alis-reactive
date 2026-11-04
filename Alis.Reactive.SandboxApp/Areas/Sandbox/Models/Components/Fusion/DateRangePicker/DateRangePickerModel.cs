namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class DateRangePickerModel
    {
        public static DateTime FamilyArrival => new DateTime(2026, 12, 10);

        public static DateTime FamilyDeparture => new DateTime(2026, 12, 17);

        public static DateTime SuiteFreeFrom => new DateTime(2026, 12, 7);

        public static DateTime SuiteFreeThrough => new DateTime(2026, 12, 20);

        public const decimal NightlyRate = 245m;

        public DateTime[]? StayPeriod { get; set; }
    }

    public sealed class RespiteStayDates
    {
        public DateTime? Arrives { get; set; }

        public DateTime? Leaves { get; set; }
    }

    public sealed record RespiteQuote(string Quote);

    public sealed record StayReservation(string Confirmation);
}
