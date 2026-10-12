namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

public class RealTimeModel { }

public class FacilityAlert
{
    public string Message { get; set; } = "";
    public string Level { get; set; } = "";
}

/// <summary>The facility alert stream: one event, the alert.</summary>
public sealed class FacilityAlertStream : ServerPushStream
{
    public ServerPushMessage<FacilityAlert> Alert => Message<FacilityAlert>("facility-alert");
}

public class ResidentPanelModel { }
