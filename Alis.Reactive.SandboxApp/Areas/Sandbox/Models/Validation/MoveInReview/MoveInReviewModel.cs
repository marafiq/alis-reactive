namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The review a coordinator completes before a resident moves in: the fall-risk score decides how
    /// much the mobility plan must say, and a returning resident's previous room is checked by the
    /// server against the community's records.
    /// </summary>
    public class MoveInReviewModel
    {
        public string? ResidentName { get; set; }
        public int? FallRiskScore { get; set; }
        public string? MobilityNotes { get; set; }

        /// <summary>When and how a resident with a moderate fall risk is reassessed.</summary>
        public string? ReassessmentPlan { get; set; }

        /// <summary>The therapist who reviews a long mobility plan.</summary>
        public string? TherapistReviewer { get; set; }
        public bool UsesWheelchair { get; set; }
        public bool UsesOxygen { get; set; }

        /// <summary>Who helps the resident out of the building in an evacuation.</summary>
        public string? EvacuationAssist { get; set; }
        public string? AssignedRoom { get; set; }

        /// <summary>The family member who consented to a room on the secured memory care wing.</summary>
        public string? SecuredUnitConsentBy { get; set; }
        public string? FamilyPortalLink { get; set; }

        /// <summary>The code the community's wristband printer prints: WB- and six digits.</summary>
        public string? WristbandCode { get; set; }

        /// <summary>Why the resident wears a temporary wristband (codes WB-9 and five digits).</summary>
        public string? TemporaryBandReason { get; set; }
        public bool IsReturning { get; set; }
        public string? PreviousRoom { get; set; }

        /// <summary>Where the resident's mail is forwarded from: a five-digit ZIP code.</summary>
        public string? ForwardingZip { get; set; }

        /// <summary>The care team writes the care plan summary on its own page; this review never renders it.</summary>
        public string? CarePlanSummary { get; set; }
        public PharmacyContact Pharmacy { get; set; } = new PharmacyContact();

        /// <summary>The coordinator on duty; the server fills it in, so the page never renders it.</summary>
        public string? ReviewedBy { get; set; }
    }

    /// <summary>The pharmacy that fills the resident's prescriptions.</summary>
    public class PharmacyContact
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }

        /// <summary>The network the community contracts; the page sends it with every review, beside the phone.</summary>
        public string? Network { get; set; }
    }

    /// <summary>What the server returns once the review is saved.</summary>
    public sealed record MoveInReviewSaved(string Confirmation);
}
