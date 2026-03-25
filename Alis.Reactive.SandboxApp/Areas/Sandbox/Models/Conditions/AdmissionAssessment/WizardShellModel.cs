namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

public class WizardShellModel
{
    public string ScreeningId { get; set; } = "";
    public Step1DemographicsModel Step1 { get; set; } = new();
    public Step2ClinicalModel Step2 { get; set; } = new();
    public Step3FunctionalModel Step3 { get; set; } = new();
    public Step4ReviewModel Step4 { get; set; } = new();
}
