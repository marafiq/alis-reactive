namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.AdmissionAssessment;

public class Step3FunctionalModel
{
    // Functional partial receives the Step 1 snapshot before rendering.
    public string ScreeningId { get; set; } = "";
    public decimal Age { get; set; }
    public string ResidentName { get; set; } = "";

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

    // Monitoring result written by Reactive Plan branches.
    public string MonitoringLevel { get; set; } = "";
}
