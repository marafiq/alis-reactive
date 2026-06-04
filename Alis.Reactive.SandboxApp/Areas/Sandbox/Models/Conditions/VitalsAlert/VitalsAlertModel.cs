namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

public class VitalsAlertModel
{
    public decimal HeartRate { get; set; }
}

public class AlertResponse
{
    public string Message { get; set; } = "";
    public string Timestamp { get; set; } = "";
    public string Level { get; set; } = "";
}
