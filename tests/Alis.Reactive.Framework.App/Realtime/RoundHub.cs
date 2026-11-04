using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.Framework.App.Realtime;

public sealed class RoundHub : Hub
{
    private static readonly RoundFeed Feed = new();

    public Task ClearBoard() => Clients.Caller.SendAsync(Feed.BoardCleared.Name);

    public Task Ping(string resident) =>
        Clients.Caller.SendAsync(Feed.StatusChanged.Name, new { resident, status = "up" });

    public string Acknowledge(string nurse) => $"acknowledged by {nurse}";

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
