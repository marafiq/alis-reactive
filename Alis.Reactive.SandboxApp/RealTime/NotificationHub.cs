using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.SandboxApp.RealTime;

/// <summary>
/// Strongly-typed hub for notification broadcasts.
/// Client method: ReceiveNotification — matches JS connection.on("ReceiveNotification", handler).
/// </summary>
public class NotificationHub : Hub<INotificationClient>
{
    /// <summary>
    /// Broadcasts a notification to all connected clients.
    /// </summary>
    public async Task SendNotification(NotificationPayload payload)
        => await Clients.All.ReceiveNotification(payload);
}
