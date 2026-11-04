using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.Framework.App.Realtime;

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
