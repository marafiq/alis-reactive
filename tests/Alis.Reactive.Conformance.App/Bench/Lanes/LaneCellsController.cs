using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Channels;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.Realtime;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class LaneCellsController : Controller
{
    [HttpGet(BenchPages.LaneSlotLoad)]
    public IActionResult LaneSlotLoad() => View("/Bench/Lanes/LaneSlotLoad.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.LaneSlotUnloadListener)]
    public IActionResult LaneSlotUnloadListener() => View("/Bench/Lanes/LaneSlotUnloadListener.cshtml", TheWorldsBoard());

    [HttpGet(BenchPages.LaneSlotHostDestroyed)]
    public IActionResult LaneSlotHostDestroyed() => View("/Bench/Lanes/LaneSlotHostDestroyed.cshtml", TheWorldsBoard());

    [HttpGet(LaneCards.ComponentCard)]
    public IActionResult ComponentCard() => PartialView("/Bench/Lanes/LaneComponentCard.cshtml", new LaneComponentCard("The card listens to its own component.", "a-1", ["a-1", "a-2"]));

    [HttpGet(BenchPages.LaneConfirm)]
    public IActionResult LaneConfirm() => View("/Bench/Lanes/LaneConfirm.cshtml", TheWorldsBoard());

    [HttpGet(BenchEndpoints.ListeningCard)]
    public IActionResult ListeningCard() => PartialView("/Bench/Lanes/LaneListeningCard.cshtml", new LaneCard("The card listens to the page's component."));

    [HttpGet(BenchPages.LanePush)]
    public IActionResult LanePush() => View("/Bench/Lanes/LanePush.cshtml", TheWorldsBoard());

    [HttpGet(BenchEndpoints.Stream)]
    public async Task<IResult> TheStream(CancellationToken stopped)
    {
        var stream = World.Streams.Of(BenchEndpoints.Stream);
        if (await stream.Admits(stopped) is not { } accepted) return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        var feed = new BenchFeed();
        return ServerEvents.Write(HttpContext, Typed(feed.Welcome.Of(new Welcome { Accepted = accepted }), stream.Opens(stopped), push => feed.Renamed.Of(Read<Renamed>(push)), stopped));
    }

    private WorldData World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;

    private JsonSerializerOptions AppJson => HttpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;

    private LaneBoard TheWorldsBoard() => new(World.ComponentText, "a-1", ["a-1", "a-2"]);

    private static async IAsyncEnumerable<ServerMessage> Typed(ServerMessage welcome, ChannelReader<Pushed> pushed, Func<Pushed, ServerMessage> typed, [EnumeratorCancellation] CancellationToken stopped)
    {
        yield return welcome;
        await foreach (var push in pushed.ReadAllAsync(stopped)) yield return typed(push);
    }

    private T Read<T>(Pushed push) => JsonSerializer.Deserialize<T>(push.Data, AppJson) ?? throw new InvalidOperationException($"The harness pushed '{push.Data}', which reads as no message of the event '{push.Event}'.");
}

public sealed record LaneBoard(string Text, string Value, IReadOnlyList<string> Codes);

public sealed record LaneCard(string Note);

public sealed record LaneComponentCard(string Note, string Value, IReadOnlyList<string> Codes);

public static class LaneCards
{
    public const string ComponentCard = BenchPages.Area + "lanes/cards/component";
}

public static class LaneCells
{
    public static readonly AppEvent CloseCard = AppEvent.Declare();

    public static readonly AppEvent OpenCard = AppEvent.Declare();

    public static readonly AppEvent Move = AppEvent.Declare();
}
