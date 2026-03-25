namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

public class Step3FunctionalModel
{
    // Cross-step: copied from Step 1 draft by controller
    public decimal Age { get; set; }
    public string ResidentName { get; set; } = "";

    // Falls
    public string FallHistory { get; set; } = "";
    public bool CausedInjury { get; set; }
    public string InjuryType { get; set; } = "";
    public decimal FallRiskScore { get; set; }

    // Mobility
    public string MobilityAid { get; set; } = "";

    // Medications
    public decimal MedicationCount { get; set; }
    public bool TakesBloodThinners { get; set; }
    public bool TakesPainMedication { get; set; }
    public decimal PainLevel { get; set; }
    public string PainLocation { get; set; } = "";

    // Auto-populated
    public string MonitoringLevel { get; set; } = "";
}
