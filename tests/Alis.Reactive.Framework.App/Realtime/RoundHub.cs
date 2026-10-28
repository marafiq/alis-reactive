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
}
