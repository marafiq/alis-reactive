using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.SandboxApp.Hubs;

public class NotificationHub : Hub { }

/// <summary>The client methods <see cref="NotificationHub"/> calls on a page.</summary>
public sealed class NotificationFeed : SignalRHub
{
    public SignalRMessage<NotificationPayload> ReceiveNotification => Message<NotificationPayload>();
}

public class NotificationPayload
{
    public int Count { get; set; }
    public string Message { get; set; } = "";
    public string Priority { get; set; } = "normal";
}
