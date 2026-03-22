using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.SandboxApp.Hubs;

/// <summary>
/// Background service that pushes live updates every 2 seconds via both hubs.
/// Proves real-time server→client push without user interaction.
/// </summary>
public class RealTimeBroadcastService : BackgroundService
{
    private readonly IHubContext<NotificationHub> _notificationHub;
    private readonly IHubContext<ResidentStatusHub> _residentHub;

    private static readonly string[] Residents =
        ["Margaret Thompson", "Robert Chen", "Dorothy Williams", "James Park", "Helen Martinez"];

    private static readonly string[] Statuses =
        ["Active", "Transferred", "Under Review", "Discharged", "Active"];

    private static readonly string[] CareLevels =
        ["Assisted Living", "Memory Care", "Independent", "Skilled Nursing", "Assisted Living"];

    public RealTimeBroadcastService(
        IHubContext<NotificationHub> notificationHub,
        IHubContext<ResidentStatusHub> residentHub)
    {
        _notificationHub = notificationHub;
        _residentHub = residentHub;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var counter = 0;

        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(2000, ct);
            counter++;

            var now = DateTime.Now.ToString("HH:mm:ss");
            var idx = counter % Residents.Length;

            await _notificationHub.Clients.All.SendAsync("ReceiveNotification", new NotificationPayload
            {
                Count = counter,
                Message = $"[{now}] #{counter} — {Residents[idx]} status update",
                Priority = counter % 3 == 0 ? "high" : "normal"
            }, ct);

            await _residentHub.Clients.All.SendAsync("StatusChanged", new ResidentStatusPayload
            {
                ResidentName = Residents[idx],
                Status = Statuses[idx],
                CareLevel = CareLevels[idx],
                UpdatedAt = DateTime.UtcNow
            }, ct);
        }
    }
}
