namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class FusionDatePickerModel
    {
        public static DateTime RoomReady => new DateTime(2026, 11, 2);

        public static DateTime HoldEnds => new DateTime(2026, 11, 30);

        public DateTime? AdmissionDay { get; set; }

        public DateTime? FirstCarePlanReview { get; set; }
    }

    public sealed class CarePlanReviewQuery
    {
        public DateTime? AdmissionDay { get; set; }
    }

    public sealed record CarePlanReviewProposal(DateTime ReviewDay);

    public sealed record AdmissionBooked(string Confirmation);
}
