using Alis.Reactive;

namespace ResidentIntake.Models;

public class ResidentIntakeModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }

    public string FacilityId { get; set; } = string.Empty;
    public string UnitId { get; set; } = string.Empty;
    public string CareLevel { get; set; } = string.Empty;
    public DateTime? AdmissionDate { get; set; }
    public decimal? MonthlyRate { get; set; }

    public bool RequiresMedicationManagement { get; set; }
    public string? PrimaryPhysician { get; set; }
    public DateTime? CognitiveAssessmentDate { get; set; }

    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    public string? RoomPreference { get; set; }
    public decimal? DepositAmount { get; set; }
}

/// <summary>The document events the intake page raises and hears.</summary>
public static class IntakeEvents
{
    /// <summary>Raised once the intake is saved; the page then loads its summary.</summary>
    public static readonly AppEvent IntakeSaved = AppEvent.Named("intake-saved");
}
