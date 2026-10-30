namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Admission booking: the coordinator books the day Dorothy Klein moves into Maple 118, and the care
    /// team's first review of her care plan, which the server proposes two weeks after that day.
    /// </summary>
    public class FusionDatePickerModel
    {
        /// <summary>The first day Maple 118 is ready for Dorothy.</summary>
        public static DateTime RoomReady => new DateTime(2026, 11, 2);

        /// <summary>The last day the community holds Maple 118 for her.</summary>
        public static DateTime HoldEnds => new DateTime(2026, 11, 30);

        public DateTime? AdmissionDay { get; set; }

        public DateTime? FirstCarePlanReview { get; set; }
    }

    /// <summary>The admission day the server proposes the first care-plan review from.</summary>
    public sealed class CarePlanReviewQuery
    {
        public DateTime? AdmissionDay { get; set; }
    }

    /// <summary>The day the server proposes for the first care-plan review.</summary>
    public sealed record CarePlanReviewProposal(DateTime ReviewDay);

    /// <summary>What the server booked, written out in full.</summary>
    public sealed record AdmissionBooked(string Confirmation);
}
