namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class MoveInReviewModel
    {
        public string? ResidentName { get; set; }

        public DateTime? MoveInDate { get; set; }
        public int? FallRiskScore { get; set; }
        public string? MobilityNotes { get; set; }

        public string? ReassessmentPlan { get; set; }

        public string? TherapistReviewer { get; set; }
        public bool UsesWheelchair { get; set; }
        public bool UsesOxygen { get; set; }

        public string? EvacuationAssist { get; set; }
        public string? AssignedRoom { get; set; }

        public string? SecuredUnitConsentBy { get; set; }
        public string? FamilyPortalLink { get; set; }

        public string? WristbandCode { get; set; }

        public string? TemporaryBandReason { get; set; }
        public bool IsReturning { get; set; }
        public string? PreviousRoom { get; set; }

        public string? ForwardingZip { get; set; }

        public string? CarePlanSummary { get; set; }
        public PharmacyContact Pharmacy { get; set; } = new PharmacyContact();

        public string? ReviewedBy { get; set; }
    }

    public class PharmacyContact
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }

        public DateTime? FirstDelivery { get; set; }

        public string? Network { get; set; }
    }

    public sealed record MoveInReviewSaved(string Confirmation);
}
