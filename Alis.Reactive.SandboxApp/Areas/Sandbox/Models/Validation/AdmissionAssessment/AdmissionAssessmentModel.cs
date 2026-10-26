using System.Text.Json.Serialization;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The nurse's admission assessment. Every kind of rule the page can run is on it, on the type it
    /// applies to: text, whole numbers, dates, yes/no answers, a care level. Each rule has a boundary,
    /// and the server draws it in the same place.
    /// </summary>
    public class AdmissionAssessmentModel
    {
        public string? ResidentName { get; set; }
        public string? PreferredName { get; set; }

        /// <summary>The label the wristband printer prints: twelve characters fit.</summary>
        public string? WristbandLabel { get; set; }

        /// <summary>The room as wing and number, MA-214: six characters.</summary>
        public string? RoomCode { get; set; }
        public string? FamilyEmail { get; set; }
        public string? PortalLink { get; set; }

        /// <summary>The word the nurse types to confirm the resident consented to the assessment.</summary>
        public string? ConsentWord { get; set; }

        /// <summary>The signatures on the consent: the resident's and a witness's.</summary>
        public int? SignaturesCollected { get; set; }
        public int? Age { get; set; }
        public int? WeightKg { get; set; }
        public int? PainScore { get; set; }
        public DateTime? MoveInDate { get; set; }
        public DateTime? AssessmentDate { get; set; }
        public DateTime? NextReviewDate { get; set; }
        public AssessedCareLevel? CareLevel { get; set; }

        /// <summary>Permanent or respite; the default, not set, is no answer.</summary>
        public ResidencyKind Residency { get; set; }

        /// <summary>The aid the resident walks with; the default, not assessed, is no answer.</summary>
        public MobilityAidKind MobilityAid { get; set; }
        public bool? FellInLastYear { get; set; }

        /// <summary>The nurse confirms the signed consent is in the file.</summary>
        public bool ConsentOnFile { get; set; }

        /// <summary>Whether the resident consents to photos; an unticked box is a no.</summary>
        public bool PhotoConsent { get; set; }

        /// <summary>How many times the resident fell in the last year; none is zero.</summary>
        public int FallsLastYear { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AssessedCareLevel
    {
        Independent,
        Assisted,
        MemoryCare,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ResidencyKind
    {
        NotSet,
        Permanent,
        Respite,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MobilityAidKind
    {
        NotAssessed,
        None,
        Cane,
        Walker,
        Wheelchair,
    }

    /// <summary>What the server says once the assessment is saved, or printed for the chart.</summary>
    public sealed record AdmissionAssessmentConfirmation(string Confirmation);
}
