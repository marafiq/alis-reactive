namespace Alis.Reactive.SandboxApp.RealTime;

public class ResidentStatusPayload
{
    public string ResidentName { get; set; } = "";
    public string Status { get; set; } = "";
    public string CareLevel { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
}
