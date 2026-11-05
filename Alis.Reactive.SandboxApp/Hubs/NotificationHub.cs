using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.SandboxApp.Hubs;

public class NotificationHub : Hub { }

public sealed class NotificationFeed : SignalRHub
{
    public Event<NotificationArgs> ReceiveNotification => Event<NotificationArgs>();
}

public sealed class NotificationArgs : JsObject
{
    public ReadOnly<int> Count => ReadOnly<int>();

    public ReadOnly<string> Message => ReadOnly<string>();

    public ReadOnly<string> Priority => ReadOnly<string>();
}

public class NotificationPayload
{
    public int Count { get; set; }
    public string Message { get; set; } = "";
    public string Priority { get; set; } = "normal";
}
