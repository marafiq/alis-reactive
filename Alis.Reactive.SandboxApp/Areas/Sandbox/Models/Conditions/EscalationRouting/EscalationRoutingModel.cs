namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.EscalationRouting;

public class EscalationRoutingModel
{
    public decimal AssessmentScore { get; set; }
    public bool SupervisorOverride { get; set; }
    public string CareTrack { get; set; } = "";
}

public class EscalationRoutingTriggerPayload
{
    public int Score { get; set; }
    public bool RequiresIsolation { get; set; }
    public bool ManualOverride { get; set; }
}

public class EscalationRoutingCareTrackOption
{
    public string Value { get; set; } = "";
    public string Text { get; set; } = "";
}
