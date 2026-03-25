namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

public class Step2ClinicalModel
{
    // Cross-step: copied from Step 1 draft by controller
    public string PrimaryDiagnosis { get; set; } = "";
    public string ResidentName { get; set; } = "";

    // Cognitive (Alzheimer's / Parkinson's)
    public decimal CognitiveScore { get; set; }
    public bool Wanders { get; set; }
    public string WanderFrequency { get; set; } = "";

    // Cardiac (Heart Disease)
    public decimal SystolicBP { get; set; }
    public bool HasPacemaker { get; set; }
    public string PacemakerModel { get; set; } = "";
    public DateTime? LastDeviceCheck { get; set; }

    // Diabetes
    public string DiabetesType { get; set; } = "";
    public decimal A1cLevel { get; set; }
    public bool InsulinDependent { get; set; }
    public string InsulinSchedule { get; set; } = "";

    // Auto-populated
    public string CareUnit { get; set; } = "";
    public string CognitiveAssessmentId { get; set; } = "";
    public string CardiacAssessmentId { get; set; } = "";
    public string DiabetesAssessmentId { get; set; } = "";
}
