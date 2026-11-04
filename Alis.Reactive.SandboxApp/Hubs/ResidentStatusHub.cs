using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.SandboxApp.Hubs;

public class ResidentStatusHub : Hub
{
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, HubCallerContext>> DrillConnections = new();
    private static readonly ConcurrentDictionary<string, bool> BrokenDrills = new();

    public override Task OnConnectedAsync()
    {
        DrillConnections.GetOrAdd(DrillIdFor(Context), _ => new ConcurrentDictionary<string, HubCallerContext>())
            .TryAdd(Context.ConnectionId, Context);
        return base.OnConnectedAsync();
    }

    public static bool RefusesNegotiate(HttpRequest request)
        => request.Path.Equals("/hubs/resident-status/negotiate", StringComparison.OrdinalIgnoreCase)
        && BrokenDrills.GetValueOrDefault(request.Query["drill"].ToString());

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (DrillConnections.TryGetValue(DrillIdFor(Context), out var connections))
            connections.TryRemove(Context.ConnectionId, out _);
        return base.OnDisconnectedAsync(exception);
    }

    public Task ClearBoard()
        => Clients.Clients(ConnectionsOf(DrillIdFor(Context))).SendAsync(new ResidentStatusFeed().BoardCleared.Name);

    public string Acknowledge(string acknowledgedBy)
    {
        if (string.IsNullOrWhiteSpace(acknowledgedBy)) return "Say who is acknowledging.";
        var screens = ConnectionsOf(DrillIdFor(Context)).Count;
        return $"Acknowledged by {acknowledgedBy} on {screens} screen{(screens == 1 ? "" : "s")}.";
    }

    public static void BreakDrill(string drillId)
    {
        BrokenDrills[drillId] = true;
        if (!DrillConnections.TryGetValue(drillId, out var connections)) return;

        foreach (var connection in connections.Values)
        {
            var transport = connection.GetHttpContext()
                ?? throw new InvalidOperationException("Outage drill: the hub connection has no HTTP connection to drop.");
            transport.Abort();
        }
    }

    public static void RestoreDrill(string drillId) => BrokenDrills[drillId] = false;

    public static IReadOnlyList<string> ConnectionsOf(string drillId)
        => DrillConnections.TryGetValue(drillId, out var connections) ? connections.Keys.ToList() : [];

    private static string DrillIdFor(HubCallerContext context)
        => context.GetHttpContext()?.Request.Query["drill"].ToString() ?? "";
}

public sealed class ResidentStatusFeed : SignalRHub
{
    public Event<ResidentStatusArgs> StatusChanged => Event<ResidentStatusArgs>();

    public Event BoardCleared => Event();

    public void ClearBoard() => Send();

    public TypedSource<string> Acknowledge(Arg<string> acknowledgedBy) => Invoke<string>(acknowledgedBy);
}

public sealed class ResidentStatusArgs : JsObject
{
    public ReadOnly<int> Connections => ReadOnly<int>();

    public ReadOnly<string> ResidentName => ReadOnly<string>();

    public ReadOnly<string> Status => ReadOnly<string>();

    public ReadOnly<string> CareLevel => ReadOnly<string>();

    public ReadOnly<DateTime> UpdatedAt => ReadOnly<DateTime>();
}

public class ResidentStatusPayload
{
    public int Connections { get; set; }

    public string ResidentName { get; set; } = "";
    public string Status { get; set; } = "";
    public string CareLevel { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
}
