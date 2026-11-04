namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

public class RealTimeModel
{
    public string? AcknowledgedBy { get; set; }
}

public sealed class FacilityAlert : JsObject
{
    public ReadOnly<string> Message => ReadOnly<string>();

    public ReadOnly<string> Level => ReadOnly<string>();
}

public sealed class FacilityAlertStream : ServerPushStream
{
    public Event<FacilityAlert> Alert => Event<FacilityAlert>();

    public Event AllClear => Event();
}

public class ResidentPanelModel { }
