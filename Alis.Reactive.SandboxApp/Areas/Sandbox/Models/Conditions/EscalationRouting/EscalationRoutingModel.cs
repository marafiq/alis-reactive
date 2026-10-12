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
    public static readonly AppEvent ComponentRouteIsolation = AppEvent.Named("component-route-isolation");
    public static readonly AppEvent ComponentRouteMonitor = AppEvent.Named("component-route-monitor");
    public static readonly AppEvent ComponentRouteRoutine = AppEvent.Named("component-route-routine");
    public static readonly AppEvent<EscalationRoutingTriggerPayload> EscalationTriggerRoute = AppEvent.Named<EscalationRoutingTriggerPayload>("escalation-trigger-route");
    public static readonly AppEvent RefreshComponentRouting = AppEvent.Named("refresh-component-routing");
    public static readonly AppEvent RouteExpedited = AppEvent.Named("route-expedited");
    public static readonly AppEvent RouteIsolation = AppEvent.Named("route-isolation");
    public static readonly AppEvent RouteRoutine = AppEvent.Named("route-routine");
}
