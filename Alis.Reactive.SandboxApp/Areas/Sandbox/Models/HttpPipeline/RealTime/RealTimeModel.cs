namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

public class RealTimeModel
{
    /// <summary>Who acknowledges the resident status on this screen.</summary>
    public string? AcknowledgedBy { get; set; }
}

/// <summary>What the page reads of a facility alert: the JSON the alert event carries.</summary>
public sealed class FacilityAlert : JsObject
{
    public ReadOnly<string> Message => ReadOnly<string>();

    public ReadOnly<string> Level => ReadOnly<string>();
}

/// <summary>The facility alert stream: an alert, and the all-clear that ends a drill.</summary>
public sealed class FacilityAlertStream : ServerPushStream
{
    public Event<FacilityAlert> Alert => Event<FacilityAlert>();

    public Event AllClear => Event();
}

public class ResidentPanelModel { }
