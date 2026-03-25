namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

/// <summary>
/// Model for the Vitals Alert vertical slice.
/// Exercises realistic condition → HTTP patterns:
///   When(comp.Value()).Gt(140) → POST alert to nurse station
///   ElseIf ladder → different POST per severity tier
///   Commands before/after condition blocks always execute
/// Senior living domain: nurse vital sign monitoring.
/// </summary>
public class VitalsAlertModel
{
    public decimal HeartRate { get; set; }
}

// ── Typed response DTOs ──

public class AlertResponse
{
    public string Message { get; set; } = "";
    public string Timestamp { get; set; } = "";
    public string Level { get; set; } = "";
}
