namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Cardiology follow-up: the nurse books Arthur Bell's visit to Dr. Osei after his hospital stay, at a
    /// time inside the follow-up window and the clinic's hours.
    /// </summary>
    public class DateTimePickerModel
    {
        /// <summary>The first day Dr. Osei sees Arthur after he came home from hospital.</summary>
        public static DateTime FirstOpening => new DateTime(2026, 11, 9);

        /// <summary>The day after the last day of the follow-up window, which ends on Friday, November 20.</summary>
        public static DateTime WindowCloses => new DateTime(2026, 11, 21);

        /// <summary>The time Dr. Osei's office is holding for Arthur.</summary>
        public static DateTime OfferedTime => new DateTime(2026, 11, 12, 14, 0, 0);

        /// <summary>Dr. Osei sees residents on weekdays from 9:00 AM to 4:00 PM.</summary>
        public static bool InClinicHours(DateTime visit) =>
            visit.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday)
            && visit.TimeOfDay >= TimeSpan.FromHours(9)
            && visit.TimeOfDay <= TimeSpan.FromHours(16);

        public DateTime? FollowUpAppointment { get; set; }
    }

    /// <summary>The time the clinic is asked about.</summary>
    public sealed class ClinicHoursQuery
    {
        public DateTime? Visit { get; set; }
    }

    /// <summary>The time asked about, whether Dr. Osei is in clinic then, and the answer written out.</summary>
    public sealed record ClinicHoursAnswer(DateTime Visit, bool InClinic, string Answer);

    /// <summary>What the clinic booked, written out in full.</summary>
    public sealed record FollowUpBooked(string Confirmation);
}
