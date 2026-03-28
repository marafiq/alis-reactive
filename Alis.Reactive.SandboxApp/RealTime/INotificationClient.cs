namespace Alis.Reactive.SandboxApp.RealTime;

public interface INotificationClient
{
    Task ReceiveNotification(NotificationPayload payload);
}
