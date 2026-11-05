using System.Text.Json.Serialization;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class DischargePlanModel
    {
        public string? ResidentName { get; set; }
        public bool GoingHome { get; set; }
        public string? HomeAddress { get; set; }
        public bool LivesAlone { get; set; }

        public string? DailyCheckInBy { get; set; }
        public bool HasPets { get; set; }
        public string? PetCarePlan { get; set; }

        public int? FollowUpDays { get; set; }
        public string? FollowUpClinic { get; set; }
        public string? VaCaseNumber { get; set; }
        public string? PrimaryCaregiver { get; set; }
        public string? CaregiverPhone { get; set; }
        public DischargeDestination? Destination { get; set; }
        public string? BedConfirmation { get; set; }
        public string? TransferSummary { get; set; }
        public string? ReceivingNurse { get; set; }
        public string? HomeHealthAgency { get; set; }
        public bool NeedsTransport { get; set; }
        public string? TransportCompany { get; set; }
        public int StairsAtHome { get; set; }
        public string? StairPlan { get; set; }
        public int FallsLast90Days { get; set; }
        public DateTime? HomeSafetyVisit { get; set; }
        public string? Medications { get; set; }
        public bool PharmacistReviewed { get; set; }
        public DateTime? InrCheckDate { get; set; }
        public string? DiagnosisCode { get; set; }
        public string? MemorySupportPlan { get; set; }
        public string? RoomCode { get; set; }
        public string? IsolationClearedBy { get; set; }
        public List<string> Equipment { get; set; } = new();
        public string? OxygenVendor { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DischargeDestination
    {
        Home,
        SkilledNursing,
        Hospice,
        Hospital,
    }

    public sealed record DischargePlanConfirmation(string Confirmation);
}
