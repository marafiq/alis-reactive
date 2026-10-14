namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.VitalsAlert;

/// <summary>The heart rate the nurse records for Walter Cho on the evening round, in beats per minute.</summary>
public class VitalsAlertModel
{
    public decimal HeartRate { get; set; }
}

/// <summary>The reading an alert carries to the responder: the heart rate, in the body.</summary>
public sealed class HeartRateReading
{
    public decimal HeartRate { get; set; }
}

/// <summary>The alert as its responder answered it: who was reached (<paramref name="Status"/>) and what they were told.</summary>
public sealed record HeartRateAlert(string Status, string Message);
