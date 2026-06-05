using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.SandboxApp.Hubs;

/// <summary>
/// Background service that pushes live updates every 2 seconds via both hubs.
/// Proves real-time server→client push without user interaction.
/// </summary>
public class RealTimeBroadcastService(
    IHubContext<NotificationHub> notificationHub,
    IHubContext<ResidentStatusHub> residentHub,
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
        var broadcastCount = 0;

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

            broadcastCount++;
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var residentIndex = broadcastCount % Residents.Length;

            try
            {
                await notificationHub.Clients.All.SendAsync("ReceiveNotification", new NotificationPayload
                {
                    Count = broadcastCount,
                    Message = $"[{timestamp}] #{broadcastCount} — {Residents[residentIndex]} status update",
                    Priority = broadcastCount % 3 == 0 ? "high" : "normal"
                }, stoppingToken);

                await residentHub.Clients.All.SendAsync("StatusChanged", new ResidentStatusPayload
                {
                    ResidentName = Residents[residentIndex],
                    Status = Statuses[residentIndex],
                    CareLevel = CareLevels[residentIndex],
                    UpdatedAt = DateTime.UtcNow
                }, stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Broadcast failed — will retry next cycle");
            }
        }
    }
}
