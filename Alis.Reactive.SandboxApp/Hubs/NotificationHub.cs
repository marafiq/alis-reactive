using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.SandboxApp.Hubs;

public class NotificationHub : Hub { }

/// <summary>The client methods <see cref="NotificationHub"/> calls on a page.</summary>
public sealed class NotificationFeed : SignalRHub
{
    public Event<NotificationArgs> ReceiveNotification => Event<NotificationArgs>();
}

/// <summary>What the page reads of a notification: the members <see cref="NotificationPayload"/> serializes.</summary>
public sealed class NotificationArgs : JsObject
{
    public ReadOnly<int> Count => ReadOnly<int>();

    public ReadOnly<string> Message => ReadOnly<string>();

    public ReadOnly<string> Priority => ReadOnly<string>();
}

/// <summary>A notification as the server sends it, and as the page posts one for a manual push.</summary>
public class NotificationPayload
{
    public int Count { get; set; }
    public string Message { get; set; } = "";
    public string Priority { get; set; } = "normal";
}
