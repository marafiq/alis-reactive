using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.SandboxApp.Hubs;

// Outage drills are scoped per page load: connections carry a drill id in the hub URL
// query, Break aborts that drill's connections and refuses its reconnect attempts,
// Restore heals it. Nothing is process-global — a reload is a fresh, healthy drill.
public class ResidentStatusHub : Hub
{
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, HubCallerContext>> DrillConnections = new();
    private static readonly ConcurrentDictionary<string, bool> BrokenDrills = new();

    public override Task OnConnectedAsync()
    {
        var drillId = DrillIdFor(Context);

        // Throwing fails the SignalR handshake, so the client counts the attempt as a
        // FAILED reconnect and its schedule runs to exhaustion. Aborting instead would
        // complete the handshake first — a "successful" reconnect that instantly drops,
        // which resets the client schedule and loops forever without ever closing.
        if (BrokenDrills.GetValueOrDefault(drillId))
            throw new HubException("Outage drill: this page's hub connection is broken.");

        DrillConnections.GetOrAdd(drillId, _ => new ConcurrentDictionary<string, HubCallerContext>())
            .TryAdd(Context.ConnectionId, Context);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (DrillConnections.TryGetValue(DrillIdFor(Context), out var connections))
            connections.TryRemove(Context.ConnectionId, out _);
        return base.OnDisconnectedAsync(exception);
    }

    public static void BreakDrill(string drillId)
    {
        BrokenDrills[drillId] = true;
        if (!DrillConnections.TryGetValue(drillId, out var connections)) return;

        foreach (var connection in connections.Values) connection.Abort();
    }

    public static void RestoreDrill(string drillId) => BrokenDrills[drillId] = false;

    /// <summary>The connections of one page load, so a send reaches that page alone.</summary>
    public static IReadOnlyList<string> ConnectionsOf(string drillId) =>
        DrillConnections.TryGetValue(drillId, out var connections) ? connections.Keys.ToList() : [];

    private static string DrillIdFor(HubCallerContext context) =>
        context.GetHttpContext()?.Request.Query["drill"].ToString() ?? "";
}

/// <summary>The client methods <see cref="ResidentStatusHub"/> calls on a page.</summary>
public sealed class ResidentStatusFeed : SignalRHub
{
    public SignalRMessage<ResidentStatusPayload> StatusChanged => Message<ResidentStatusPayload>();

    /// <summary>The board was cleared at shift change: called with no argument.</summary>
    public SignalRMessage BoardCleared => Message();
}

public class ResidentStatusPayload
{
    /// <summary>How many live connections the page that pushed the status holds to this hub.</summary>
    public int Connections { get; set; }

    public string ResidentName { get; set; } = "";
    public string Status { get; set; } = "";
    public string CareLevel { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
}
