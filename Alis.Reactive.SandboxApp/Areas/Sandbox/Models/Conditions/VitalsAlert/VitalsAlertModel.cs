namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.VitalsAlert;

public class VitalsAlertModel
{
    public decimal HeartRate { get; set; }
}

public sealed class HeartRateReading
{
    public decimal HeartRate { get; set; }
}

public sealed record HeartRateAlert(string Status, string Message);
