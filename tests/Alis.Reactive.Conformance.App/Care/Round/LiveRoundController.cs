using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Channels;
using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.Realtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class LiveRoundController : Controller
{
    private const string Live = "/Care/Round/_LiveLayout.cshtml";
    private const string LiveTraced = "/Care/Round/_LiveTracedLayout.cshtml";

    private WorldData World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;

    private JsonSerializerOptions AppJson => HttpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;

    [HttpGet(CarePages.LiveBulletins)]
    public IActionResult Bulletins() => View("/Care/Round/LiveBulletins.cshtml", new LiveRound("North", Live));

    [HttpGet(CarePages.LiveTracedBulletins)]
    public IActionResult TracedBulletins() => View("/Care/Round/LiveBulletins.cshtml", new LiveRound("North", LiveTraced));

    [HttpGet(CarePages.LiveNews)]
    public IActionResult News() => View("/Care/Round/LiveNews.cshtml", new LiveRound("North", Live));

    [HttpGet(CarePages.LiveAlerts)]
    public IActionResult Alerts() => View("/Care/Round/LiveAlerts.cshtml", new LiveRound("North", Live));

    [HttpGet(CarePages.LiveOffer)]
    public IActionResult Offer() => View("/Care/Round/LiveOffer.cshtml", new LiveRound("North", Live));

    [HttpGet(CarePages.LiveBoardCard)]
    public IActionResult BoardCard() => View("/Care/Round/LiveBoardCard.cshtml", new LiveRound("North", Live));

    [HttpGet(CarePages.LiveDoses)]
    public IActionResult Doses() => View("/Care/Round/LiveDoses.cshtml", new LiveRound("North", Live));

    [HttpGet(CarePages.LiveFeed)]
    public IActionResult Feed() => View("/Care/Round/LiveFeed.cshtml", new LiveRound("North", Live));

    [HttpGet(CarePages.LiveFeedItems)]
    public IActionResult FeedItems() => View("/Care/Round/LiveFeedItems.cshtml", new LiveRound("North", Live));

    [HttpGet(CareEndpoints.BedCard)]
    public IActionResult BedCard() => PartialView("/Care/Round/LiveBedCard.cshtml", new BedCard("Bed 4"));

    [HttpGet(CareEndpoints.BoardCard)]
    public IActionResult TheBoardCard() => PartialView("/Care/Round/LiveWardCard.cshtml", new BedCard("Bed 4"));

    [HttpPost(CareEndpoints.LiveDoses)]
    [AtTheRealtimeAnswerGate]
    public IActionResult GiveDose([FromBody] GivenDose dose) => Json(dose);

    [HttpPost(CareEndpoints.LiveItems)]
    [AtTheRealtimeAnswerGate]
    public IActionResult SaveItem([FromBody] SavedItem item) => Json(item);

    [HttpGet(CareEndpoints.LiveItems)]
    [AtTheRealtimeAnswerGate]
    public IActionResult Items() => Json(new SavedItem("Gauze"));

    [HttpGet(CareEndpoints.LiveList)]
    public IActionResult List() => Json(new SavedItem("Gauze"));

    [HttpGet(CareEndpoints.WardBoard)]
    public async Task<IResult> TheWardBoard(CancellationToken stopped)
    {
        var stream = World.Streams.Of(CareEndpoints.WardBoard);
        if (await stream.Admits(stopped) is not { } accepted) return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        return TypedResults.ServerSentEvents(Relayed(JsonSerializer.Serialize(new Welcome { Accepted = accepted }, AppJson), stream.Opens(stopped), stopped));
    }

    [HttpGet(CareEndpoints.RoundNews)]
    public async Task<IResult> TheRoundNews(CancellationToken stopped)
    {
        var stream = World.Streams.Of(CareEndpoints.RoundNews);
        if (await stream.Admits(stopped) is not { } accepted) return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        var news = new RoundNews();
        return ServerEvents.Write(HttpContext, Typed(news.Welcome.Of(new Welcome { Accepted = accepted }), stream.Opens(stopped), push => news.Dose.Of(Read<Dose>(push)), stopped));
    }

    [HttpGet(CareEndpoints.Feed)]
    public async Task<IResult> TheFeed(CancellationToken stopped)
    {
        var stream = World.Streams.Of(CareEndpoints.Feed);
        if (await stream.Admits(stopped) is not { } accepted) return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        var feed = new Feed();
        return ServerEvents.Write(HttpContext, Typed(feed.Welcome.Of(new Welcome { Accepted = accepted }), stream.Opens(stopped), push => feed.Item.Of(Read<Item>(push)), stopped));
    }

    [HttpGet(CareEndpoints.FacilityAlerts)]
    public async Task<IResult> TheFacilityAlerts(CancellationToken stopped)
    {
        var stream = World.Streams.Of(CareEndpoints.FacilityAlerts);
        if (await stream.Admits(stopped) is not { } accepted) return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        var alerts = new FacilityAlerts();
        return ServerEvents.Write(HttpContext, Typed(alerts.Welcome.Of(new Welcome { Accepted = accepted }), stream.Opens(stopped), push => alerts.Alert.Of(Read<Alert>(push)), stopped));
    }

    [HttpGet(CareEndpoints.BedBoard)]
    public async Task<IResult> TheBedBoard(CancellationToken stopped)
    {
        var stream = World.Streams.Of(CareEndpoints.BedBoard);
        if (await stream.Admits(stopped) is not { } accepted) return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        var beds = new BedBoard();
        return ServerEvents.Write(HttpContext, Typed(beds.Welcome.Of(new Welcome { Accepted = accepted }), stream.Opens(stopped), push => beds.Welcome.Of(Read<Welcome>(push)), stopped));
    }

    [HttpGet(CareEndpoints.Switchboard)]
    public async Task<IResult> TheSwitchboard(CancellationToken stopped)
    {
        var stream = World.Streams.Of(CareEndpoints.Switchboard);
        if (await stream.Admits(stopped) is null) return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        stream.Opens(stopped);
        var lines = new Switchboard();
        return ServerEvents.Write(HttpContext, Counted(lines, World.Streams.Of(CareEndpoints.WardBoard).OpenCounts(stopped)));
    }

    private static async IAsyncEnumerable<SseItem<string>> Relayed(string welcome, ChannelReader<Pushed> pushed, [EnumeratorCancellation] CancellationToken stopped)
    {
        yield return new SseItem<string>(welcome, "welcome") { ReconnectionInterval = LiveStreams.Reconnection };
        await foreach (var push in pushed.ReadAllAsync(stopped)) yield return new SseItem<string>(push.Data, push.Event);
    }

    private static async IAsyncEnumerable<ServerMessage> Typed(ServerMessage welcome, ChannelReader<Pushed> pushed, Func<Pushed, ServerMessage> typed, [EnumeratorCancellation] CancellationToken stopped)
    {
        yield return welcome;
        await foreach (var push in pushed.ReadAllAsync(stopped)) yield return typed(push);
    }

    private static async IAsyncEnumerable<ServerMessage> Counted(Switchboard lines, IAsyncEnumerable<int> counts)
    {
        await foreach (var open in counts) yield return lines.Lines.Of(new Lines { Open = open });
    }

    private T Read<T>(Pushed push) => JsonSerializer.Deserialize<T>(push.Data, AppJson) ?? throw new InvalidOperationException($"The harness pushed '{push.Data}', which reads as no message of the event '{push.Event}'.");
}
