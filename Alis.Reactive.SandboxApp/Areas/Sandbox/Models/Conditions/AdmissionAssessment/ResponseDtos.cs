namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.AdmissionAssessment;

/// <summary>An alert's answer: what the service that took the alert says it will do.</summary>
public class ScreeningAlertResponse
{
    public string Message { get; set; } = "";
}

public class SubmitScreeningResponse
{
    public string ScreeningId { get; set; } = "";
    public string CareUnit { get; set; } = "";
    public string MonitoringLevel { get; set; } = "";
    public string Message { get; set; } = "";
    public List<string> Alerts { get; set; } = new();
}

public class PhysicianSearchResponse
{
    public List<AssessmentPhysicianItem> Physicians { get; set; } = new();
}

public class AssessmentPhysicianItem
{
    public string Value { get; set; } = "";
    public string Text { get; set; } = "";
    public string Specialty { get; set; } = "";
}

public class SaveStepResponse
{
    public string ScreeningId { get; set; } = "";
    public string Message { get; set; } = "";
}
