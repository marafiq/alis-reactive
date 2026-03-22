namespace ResidentIntake.Models;

public class ResidentIntakeModel
{
    // Personal Info
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }

    // Placement
    public string? FacilityId { get; set; }
    public string? UnitId { get; set; }
    public string? CareLevel { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public decimal? MonthlyRate { get; set; }

    // Medical
    public bool RequiresMedicationManagement { get; set; }
    public string? PrimaryPhysician { get; set; }
    public DateTime? CognitiveAssessmentDate { get; set; }

    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    // Facility-specific (loaded dynamically via partial + ResolvePlan)
    public string? RoomPreference { get; set; }
    public decimal? DepositAmount { get; set; }
}
