namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.AdmissionAssessment;

public class Step2ClinicalModel
{
    // Step 1 snapshot fields copied before the clinical partial renders.
    public string ScreeningId { get; set; } = "";
    public string PrimaryDiagnosis { get; set; } = "";
    public string ResidentName { get; set; } = "";

    public decimal CognitiveScore { get; set; }
    public bool Wanders { get; set; }
    public string WanderFrequency { get; set; } = "";

    public decimal SystolicBP { get; set; }
    public bool HasPacemaker { get; set; }
    public string PacemakerModel { get; set; } = "";
    public DateTime? LastDeviceCheck { get; set; }

    public string DiabetesType { get; set; } = "";
    public decimal A1cLevel { get; set; }
    public bool InsulinDependent { get; set; }
    public string InsulinSchedule { get; set; } = "";

    // Assessment result fields written by Reactive Plan branches and save responses.
    public string CareUnit { get; set; } = "";
    public string CognitiveAssessmentId { get; set; } = "";
    public string CardiacAssessmentId { get; set; } = "";
    public string DiabetesAssessmentId { get; set; } = "";
}
