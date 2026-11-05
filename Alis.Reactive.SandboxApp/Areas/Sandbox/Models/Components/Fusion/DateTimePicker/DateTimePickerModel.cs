namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class DateTimePickerModel
    {
        public static DateTime FirstOpening => new DateTime(2026, 11, 9);

        public static DateTime WindowCloses => new DateTime(2026, 11, 21);

        public static DateTime OfferedTime => new DateTime(2026, 11, 12, 14, 0, 0);

        public static bool InClinicHours(DateTime visit)
            => visit.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday)
            && visit.TimeOfDay >= TimeSpan.FromHours(9)
            && visit.TimeOfDay <= TimeSpan.FromHours(16);

        public DateTime? FollowUpAppointment { get; set; }
    }

    public sealed class ClinicHoursQuery
    {
        public DateTime? Visit { get; set; }
    }

    public sealed record ClinicHoursAnswer(DateTime Visit, bool InClinic, string Answer);

    public sealed record FollowUpBooked(string Confirmation);
}
