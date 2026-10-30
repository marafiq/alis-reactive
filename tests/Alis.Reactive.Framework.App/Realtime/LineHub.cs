using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.Framework.App.Realtime;

/// <summary>
/// The ward's line, one per page by the world in its URL. It welcomes each call it takes and drops or hangs
/// up on the page when asked. A drop cuts the connection under the hub, so the page calls again; a hang-up
/// closes it (<c>Context.Abort()</c> sends a close that allows no reconnect), and the gate in front of the hub
/// refuses the page's calls until the server answers again.
/// </summary>
public sealed class LineHub(LineExchange exchange) : Hub
{
    private static readonly WardLine Line = new();

    public override async Task OnConnectedAsync()
    {
        var call = World().LineOpened(Context.ConnectionId);
        await Clients.Caller.SendAsync(Line.Welcomed.Name, new { call });
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        World().LineClosed(Context.ConnectionId);
        return Task.CompletedTask;
    }

    public void Drop() => Context.GetHttpContext()!.Abort();

    public void HangUp()
    {
        World().HangUp();
        Context.Abort();
    }

    public Task Find(string connectionId) => Clients.Client(connectionId).SendAsync(Line.Found.Name);

    public Task Note(string text) => Clients.Caller.SendAsync(Line.Noted.Name, new { text });

    public string CallTheNurse() => "a nurse is on the way";

    private LineWorld World() => exchange.World(Context.GetHttpContext()!.Request.Query["world"].ToString());
}

public static class LineHubRoutes
{
    /// <summary>
    /// Maps the line's hub behind a gate: while a page's world refuses calls (its first calls, or every call
    /// while hung up), the server answers the page's call before the hub's handshake with 503, and counts the
    /// refusal once it is sent.
    /// </summary>
    public static void MapLineHub(this WebApplication app, string pattern)
    {
        app.Use(async (context, next) =>
        {
            var exchange = context.RequestServices.GetRequiredService<LineExchange>();
            var world = context.Request.Query["world"].ToString();
            if (context.Request.Path.StartsWithSegments(pattern + "/negotiate") && exchange.World(world).RefusesNegotiation())
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                context.Response.OnCompleted(() =>
                {
                    exchange.World(world).LineCallRefused();
                    return Task.CompletedTask;
                });
                return;
            }
            await next();
        });
        app.MapHub<LineHub>(pattern);
    }
}
