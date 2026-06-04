namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.AdmissionAssessment;

public class HealthScreeningModel
{
    public string ResidentName { get; set; } = "";
    public decimal Age { get; set; }
    public string PrimaryDiagnosis { get; set; } = "";
    public string AttendingPhysician { get; set; } = "";
    public bool IsVeteran { get; set; }
    public string VaId { get; set; } = "";
    public string ServiceBranch { get; set; } = "";

    public decimal CognitiveScore { get; set; }
    public bool Wanders { get; set; }
    public string WanderFrequency { get; set; } = "";

    public bool HasPacemaker { get; set; }
    public string PacemakerModel { get; set; } = "";
    public DateTime? LastDeviceCheck { get; set; }
    public decimal SystolicBP { get; set; }

    public string DiabetesType { get; set; } = "";
    public decimal A1cLevel { get; set; }
    public bool InsulinDependent { get; set; }
    public string InsulinSchedule { get; set; } = "";

    public string FallHistory { get; set; } = "";
    public bool CausedInjury { get; set; }
    public string InjuryType { get; set; } = "";
    public decimal FallRiskScore { get; set; }
    public string MobilityAid { get; set; } = "";

    public decimal MedicationCount { get; set; }
    public bool TakesBloodThinners { get; set; }
    public bool TakesPainMedication { get; set; }
    public decimal PainLevel { get; set; }
    public string PainLocation { get; set; } = "";

    public string EmergencyContact { get; set; } = "";

    // Set by plan reactions and HTTP responses.
    public string RiskTier { get; set; } = "";
    public string CareUnit { get; set; } = "";
    public string MonitoringLevel { get; set; } = "";
    public string CognitiveAssessmentId { get; set; } = "";
    public string CardiacAssessmentId { get; set; } = "";
    public string DiabetesAssessmentId { get; set; } = "";
}
