namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

public class Step1DemographicsModel
{
    public string ScreeningId { get; set; } = "";
    public string ResidentName { get; set; } = "";
    public decimal Age { get; set; }
    public string PrimaryDiagnosis { get; set; } = "";
    public string AttendingPhysician { get; set; } = "";
    public bool IsVeteran { get; set; }
    public string VaId { get; set; } = "";
    public string ServiceBranch { get; set; } = "";
    public string RiskTier { get; set; } = "";
}
