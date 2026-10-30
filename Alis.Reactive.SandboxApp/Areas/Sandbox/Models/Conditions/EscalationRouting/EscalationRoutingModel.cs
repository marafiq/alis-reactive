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

/// <summary>The document events the EscalationRouting page raises and hears.</summary>
public static class EscalationRoutingEvents
{
    public static readonly AppEvent ComponentRouteIsolation = AppEvent.Declare();
    public static readonly AppEvent ComponentRouteMonitor = AppEvent.Declare();
    public static readonly AppEvent ComponentRouteRoutine = AppEvent.Declare();
    public static readonly AppEvent<EscalationRoutingTriggerPayload> EscalationTriggerRoute = AppEvent.Declare<EscalationRoutingTriggerPayload>();
    public static readonly AppEvent RefreshComponentRouting = AppEvent.Declare();
    public static readonly AppEvent RouteExpedited = AppEvent.Declare();
    public static readonly AppEvent RouteIsolation = AppEvent.Declare();
    public static readonly AppEvent RouteRoutine = AppEvent.Declare();
}
