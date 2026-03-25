namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

public class ScreeningAlertResponse
{
    public string Message { get; set; } = "";
    public string Urgency { get; set; } = "";
}

public class SaveSectionResponse
{
    public string Id { get; set; } = "";
    public string Message { get; set; } = "";
}

public class VerifyVaResponse
{
    public string Message { get; set; } = "";
    public bool Eligible { get; set; }
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
    public List<PhysicianItem> Physicians { get; set; } = new();
}

// PhysicianItem reused from AutoCompleteModel (same namespace) — has Text, Value, Specialty
