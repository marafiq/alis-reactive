using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.Framework.App.Realtime;

/// <summary>The round's hub. It calls back only the page that called it, so pages never hear each other.</summary>
public sealed class RoundHub : Hub
{
    private static readonly RoundFeed Feed = new();

    public Task ClearBoard() => Clients.Caller.SendAsync(Feed.BoardCleared.Name);

    public Task Ping(string resident) =>
        Clients.Caller.SendAsync(Feed.StatusChanged.Name, new { resident, status = "up" });

    public string Acknowledge(string nurse) => $"acknowledged by {nurse}";

    /// <summary>Tells the page it is tidying before it completes, so the page hears it before the steps after the call.</summary>
    public Task Tidy() =>
        Clients.Caller.SendAsync(Feed.StatusChanged.Name, new { resident = "everyone", status = "tidying" });

    public Task TidyFor(string resident) =>
        Clients.Caller.SendAsync(Feed.StatusChanged.Name, new { resident, status = "tidying" });

    public string Roster() => "Ada and Ben";

    public Task Find(string connectionId) => Clients.Client(connectionId).SendAsync(Feed.Found.Name);

    public async Task AskForABareStatus()
    {
        await Clients.Caller.SendAsync(Feed.StatusChanged.Name, "up");
        await Clients.Caller.SendAsync(Feed.BoardCleared.Name);
    }
}
