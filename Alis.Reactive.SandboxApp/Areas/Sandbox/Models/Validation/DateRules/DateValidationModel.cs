namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>A resident's stay: admission and discharge, and the dates scheduled inside the stay.</summary>
    public class DateValidationModel
    {
        public DateTime? AdmissionDate { get; set; }
        public DateTime? DischargeDate { get; set; }

        /// <summary>When the care team reviews the care plan during the stay.</summary>
        public DateTime? CarePlanReviewDate { get; set; }

        /// <summary>When the physician's follow-up visit is booked.</summary>
        public DateTime? FollowUpVisitDate { get; set; }

        /// <summary>When the room is inspected as the resident moves out.</summary>
        public DateTime? MoveOutInspectionDate { get; set; }
    }
}
