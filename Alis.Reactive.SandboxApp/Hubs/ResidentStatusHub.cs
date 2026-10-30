using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.SandboxApp.Hubs;

// Outage drills are scoped per page load: connections carry a drill id in the hub URL
// query, Break drops that drill's connections and refuses its negotiates (Program.cs),
// Restore heals it. Nothing is process-global — a reload is a fresh, healthy drill.
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

    /// <summary>
    /// Whether the request is a negotiate for a broken drill. Refusing it there, before any hub
    /// connection exists, is what makes the client's reconnect attempt fail: a hub that accepts the
    /// handshake and then closes counts as a successful reconnect that dropped, and the client's
    /// schedule starts over.
    /// </summary>
    public static bool RefusesNegotiate(HttpRequest request) =>
        request.Path.Equals("/hubs/resident-status/negotiate", StringComparison.OrdinalIgnoreCase)
        && BrokenDrills.GetValueOrDefault(request.Query["drill"].ToString());

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (DrillConnections.TryGetValue(DrillIdFor(Context), out var connections))
            connections.TryRemove(Context.ConnectionId, out _);
        return base.OnDisconnectedAsync(exception);
    }

    /// <summary>Clears the board on every screen of the caller's drill: the page sends it at shift change.</summary>
    public Task ClearBoard() =>
        Clients.Clients(ConnectionsOf(DrillIdFor(Context))).SendAsync(new ResidentStatusFeed().BoardCleared.Name);

    /// <summary>Acknowledges the status shown: the page invokes it and shows the receipt the hub answers.</summary>
    public string Acknowledge(string acknowledgedBy)
    {
        if (string.IsNullOrWhiteSpace(acknowledgedBy)) return "Say who is acknowledging.";
        var screens = ConnectionsOf(DrillIdFor(Context)).Count;
        return $"Acknowledged by {acknowledgedBy} on {screens} screen{(screens == 1 ? "" : "s")}.";
    }

    // The drop is the transport's, not the hub's: aborting the hub connection would send the client a
    // close message that forbids reconnecting, so the page would never see Reconnecting. Aborting the
    // HTTP connection under the WebSocket drops it the way a lost network does, and the client's
    // automatic reconnect runs its schedule against the refused negotiates.
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

    /// <summary>The connections of one page load, so a send reaches that page alone.</summary>
    public static IReadOnlyList<string> ConnectionsOf(string drillId) =>
        DrillConnections.TryGetValue(drillId, out var connections) ? connections.Keys.ToList() : [];

    private static string DrillIdFor(HubCallerContext context) =>
        context.GetHttpContext()?.Request.Query["drill"].ToString() ?? "";
}

/// <summary>
/// The resident status hub as the page sees it: the methods the server calls on the page, and the
/// methods the page calls on the server.
/// </summary>
public sealed class ResidentStatusFeed : SignalRHub
{
    public Event<ResidentStatusArgs> StatusChanged => Event<ResidentStatusArgs>();

    /// <summary>The board was cleared at shift change: called with no argument.</summary>
    public Event BoardCleared => Event();

    /// <summary>Clears the board on every screen of this drill; the server answers with <see cref="BoardCleared"/>.</summary>
    public void ClearBoard() => Send();

    /// <summary>Acknowledges the status shown; the server answers with a receipt naming who, and on how many screens.</summary>
    public TypedSource<string> Acknowledge(Arg<string> acknowledgedBy) => Invoke<string>(acknowledgedBy);
}

/// <summary>What the page reads of a status: the members <see cref="ResidentStatusPayload"/> serializes.</summary>
public sealed class ResidentStatusArgs : JsObject
{
    /// <summary>How many live connections the page that pushed the status holds to this hub.</summary>
    public ReadOnly<int> Connections => ReadOnly<int>();

    public ReadOnly<string> ResidentName => ReadOnly<string>();

    public ReadOnly<string> Status => ReadOnly<string>();

    public ReadOnly<string> CareLevel => ReadOnly<string>();

    public ReadOnly<DateTime> UpdatedAt => ReadOnly<DateTime>();
}

/// <summary>A status as the server sends it, and as the page posts one for a manual push.</summary>
public class ResidentStatusPayload
{
    /// <summary>How many live connections the page that pushed the status holds to this hub.</summary>
    public int Connections { get; set; }

    public string ResidentName { get; set; } = "";
    public string Status { get; set; } = "";
    public string CareLevel { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
}
