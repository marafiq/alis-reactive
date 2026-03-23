using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.SandboxApp.Hubs;

public class NotificationHub : Hub { }

public class NotificationPayload
{
    public int Count { get; set; }
    public string Message { get; set; } = "";
    public string Priority { get; set; } = "normal";
}
