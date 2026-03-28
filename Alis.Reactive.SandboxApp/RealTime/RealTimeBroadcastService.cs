using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.SandboxApp.RealTime;

/// <summary>
/// Background service that pushes live updates every 2 seconds via both hubs.
/// Proves real-time server→client push without user interaction.
/// </summary>
public class RealTimeBroadcastService(
    IHubContext<NotificationHub, INotificationClient> notificationHub,
    IHubContext<ResidentStatusHub, IResidentStatusClient> residentHub,
    ILogger<RealTimeBroadcastService> logger) : BackgroundService
{
    private static readonly string[] Residents =
        ["Margaret Thompson", "Robert Chen", "Dorothy Williams", "James Park", "Helen Martinez"];

    private static readonly string[] Statuses =
        ["Active", "Transferred", "Under Review", "Discharged", "Active"];

    private static readonly string[] CareLevels =
        ["Assisted Living", "Memory Care", "Independent", "Skilled Nursing", "Assisted Living"];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var counter = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(2000, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            counter++;
            var now = DateTime.Now.ToString("HH:mm:ss");
            var idx = counter % Residents.Length;

            try
            {
                await notificationHub.Clients.All.ReceiveNotification(new NotificationPayload
                {
                    Count = counter,
                    Message = $"[{now}] #{counter} — {Residents[idx]} status update",
                    Priority = counter % 3 == 0 ? "high" : "normal"
                });

                await residentHub.Clients.All.StatusChanged(new ResidentStatusPayload
                {
                    ResidentName = Residents[idx],
                    Status = Statuses[idx],
                    CareLevel = CareLevels[idx],
                    UpdatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Broadcast failed — will retry next cycle");
            }
        }
    }
}
