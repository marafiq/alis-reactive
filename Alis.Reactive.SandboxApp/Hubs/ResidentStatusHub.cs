using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.SandboxApp.Hubs;

public class ResidentStatusHub : Hub { }

public class ResidentStatusPayload
{
    public string ResidentName { get; set; } = "";
    public string Status { get; set; } = "";
    public string CareLevel { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
}
