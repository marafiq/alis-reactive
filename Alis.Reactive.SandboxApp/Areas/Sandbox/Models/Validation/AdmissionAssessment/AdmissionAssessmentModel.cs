using System.Text.Json.Serialization;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class AdmissionAssessmentModel
    {
        public string? ResidentName { get; set; }
        public string? PreferredName { get; set; }

        public string? WristbandLabel { get; set; }

        public string? RoomCode { get; set; }
        public string? FamilyEmail { get; set; }
        public string? PortalLink { get; set; }

        public string? ConsentWord { get; set; }

        public int? SignaturesCollected { get; set; }
        public int? Age { get; set; }
        public int? WeightKg { get; set; }
        public int? PainScore { get; set; }
        public DateTime? MoveInDate { get; set; }
        public DateTime? AssessmentDate { get; set; }
        public DateTime? NextReviewDate { get; set; }
        public AssessedCareLevel? CareLevel { get; set; }

        public ResidencyKind Residency { get; set; }

        public MobilityAidKind MobilityAid { get; set; }
        public bool? FellInLastYear { get; set; }

        public bool ConsentOnFile { get; set; }

        public bool PhotoConsent { get; set; }

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

    public sealed record AdmissionAssessmentConfirmation(string Confirmation);
}
