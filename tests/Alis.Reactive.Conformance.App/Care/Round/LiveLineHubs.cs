using System.Text.Json;
using System.Threading.Channels;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.Conformance.App.Care;

internal abstract class LiveLineHub : Hub
{
    protected abstract string Line { get; }

    protected abstract HubPush<Welcome> Welcome { get; }

    protected WorldStream Stream => Context.GetHttpContext()!.RequestServices.GetRequiredService<CurrentWorld>().Data.Streams.Of(Line);

    public override async Task OnConnectedAsync()
    {
        var stream = Stream;
        var pushes = stream.Opens(Context.ConnectionAborted);
        await Welcome.SendAsync(Clients.Caller, new Welcome { Accepted = stream.Accepted });
        _ = Relays(pushes, Clients.Caller, Context.GetHttpContext()!);
    }

    private static async Task Relays(ChannelReader<Pushed> pushes, IClientProxy caller, HttpContext transport)
    {
        await foreach (var push in pushes.ReadAllAsync()) await caller.SendCoreAsync(push.Event, ArgumentsOf(push));
        transport.Abort();
    }

    private static object?[] ArgumentsOf(Pushed push)
        => [.. JsonSerializer.Deserialize<JsonElement[]>(push.Data) ?? throw new InvalidOperationException($"The harness pushed '{push.Data}' on '{push.Event}', which is no list of a push's arguments.")];
}

internal sealed class WardLineHub : LiveLineHub
{
    protected override string Line => CareEndpoints.WardLine;

    protected override HubPush<Welcome> Welcome => new WardLine().Welcome;

    public string Roster() => "Ada, Ben";
}

internal sealed class RoundFeedHub : LiveLineHub
{
    protected override string Line => CareEndpoints.RoundFeed;

    protected override HubPush<Welcome> Welcome => new RoundFeed().Welcome;
}

internal sealed class BedLineHub : LiveLineHub
{
    protected override string Line => CareEndpoints.BedLine;

    protected override HubPush<Welcome> Welcome => new BedLine().Welcome;
}

internal sealed class CardLineHub : LiveLineHub
{
    protected override string Line => CareEndpoints.Line;

    protected override HubPush<Welcome> Welcome => new CardLine().Welcome;
}

internal sealed class NurseLineHub : LiveLineHub
{
    protected override string Line => CareEndpoints.NurseLine;

    protected override HubPush<Welcome> Welcome => new NurseLine().Welcome;

    public string Call() => Stream.FirstTraceparent;
}

internal static class LiveLines
{
    internal static void MapLiveLines(this WebApplication app)
    {
        app.MapHub<WardLineHub>(CareEndpoints.WardLine);
        app.MapHub<RoundFeedHub>(CareEndpoints.RoundFeed);
        app.MapHub<BedLineHub>(CareEndpoints.BedLine);
        app.MapHub<CardLineHub>(CareEndpoints.Line);
        app.MapHub<NurseLineHub>(CareEndpoints.NurseLine);
    }
}
