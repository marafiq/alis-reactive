namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.AdmissionAssessment;

public class Step4ReviewModel
{
    public string EmergencyContact { get; set; } = "";
    public string ScreeningId { get; set; } = "";

    // Review summary fields rebuilt from saved step drafts.
    public string RiskTier { get; set; } = "";
    public string CareUnit { get; set; } = "";
    public string MonitoringLevel { get; set; } = "";
    public List<string> AlertsSent { get; set; } = new();
    public bool Step1Saved { get; set; }
    public bool Step2Saved { get; set; }
    public bool Step3Saved { get; set; }
}
