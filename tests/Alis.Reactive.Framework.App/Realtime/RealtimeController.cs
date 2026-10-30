using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Realtime;

/// <summary>
/// Server-sent news for one page's world, and the pushes that send it (capabilities of realtime); and the
/// ward's line and board, which the server drops, ends, closes for good and takes back.
/// </summary>
public sealed class RealtimeController(LineExchange exchange) : Controller
{
    private static readonly RoundStream Stream = new();
    private static readonly WardBoard Board = new();
    private static readonly Switchboard Switch = new();
    private static readonly ConcurrentDictionary<string, World> Worlds = new();

    public IActionResult Round() => View(new RoundPage());

    /// <summary>The line page; the server refuses its first calls to the line when asked to.</summary>
    public IActionResult Line([FromQuery] int refusals = 0, [FromQuery] bool offersRetry = true)
    {
        var page = new LinePage(offersRetry);
        exchange.World(page.World).RefuseNegotiations(refusals);
        return View(page);
    }

    /// <summary>The ward page; the server refuses its first calls to the line when asked to.</summary>
    public IActionResult Ward([FromQuery] bool pageListens = false, [FromQuery] int refusals = 0)
    {
        var page = new WardPage(pageListens);
        exchange.World(page.World).RefuseNegotiations(refusals);
        return View(page);
    }

    public IActionResult Bed([FromQuery] string world) => PartialView(new BedCard(world));

    public IActionResult CallPoint([FromQuery] string world) => PartialView(new CallPointCard(world));

    public IActionResult Names() => View();

    public IActionResult BlankEventName() => View();

    public IActionResult EventNamedByNoMember() => View();

    /// <summary>The world's stream: its first news on connecting, then each push as it comes.</summary>
    [HttpGet("/Realtime/News")]
    public async Task News([FromQuery] string world, CancellationToken stopped)
    {
        var news = Worlds.GetOrAdd(world, _ => new World());
        Response.ContentType = "text/event-stream";
        await Write(Stream.News.Name, new { message = "Ada is up", count = news.Next() }, stopped);
        try
        {
            await foreach (var (name, data) in news.Pushes.Reader.ReadAllAsync(stopped))
                await Write(name, data, stopped);
        }
        catch (OperationCanceledException)
        {
            // The page closed the stream, or left.
        }
    }

    [HttpPost("/Realtime/Push")]
    public IActionResult Push([FromQuery] string world, [FromQuery] string kind)
    {
        var news = Worlds.GetOrAdd(world, _ => new World());
        var count = news.Next();
        news.Pushes.Writer.TryWrite(kind == "allClear"
            ? (Stream.AllClear.Name, new { })
            : (Stream.News.Name, new { message = $"news {count}", count }));
        return Ok();
    }

    [HttpPost("/Realtime/Line/Answer")]
    public IActionResult AnswerTheLine([FromQuery] string world)
    {
        exchange.World(world).Answer();
        return Ok();
    }

    /// <summary>The ward's board: a welcome on each call it takes, then each push, until the page leaves or the server ends it.</summary>
    [HttpGet("/Realtime/Board")]
    public async Task WardBoard([FromQuery] string world, CancellationToken stopped)
    {
        var line = exchange.World(world);
        if (line.BoardClosed)
        {
            Response.StatusCode = StatusCodes.Status204NoContent;
            return;
        }

        using var call = line.BoardOpened(stopped);
        Response.ContentType = "text/event-stream";
        try
        {
            // A browser calls a stream that ended again after the retry the stream names, in milliseconds.
            await Response.WriteAsync("retry: 100\n\n", call.Ended);
            await Write(Board.Welcome.Name, new { call = call.Number }, call.Ended);
            call.Answer();
            await foreach (var (name, data) in call.Pushes.Reader.ReadAllAsync(call.Ended))
                await WriteData(name, data, call.Ended);
        }
        catch (OperationCanceledException)
        {
            // The page left, or the server ended the stream.
        }
    }

    [HttpPost("/Realtime/Board/End")]
    public IActionResult EndTheBoard([FromQuery] string world)
    {
        exchange.World(world).EndBoard();
        return Ok();
    }

    [HttpPost("/Realtime/Board/Close")]
    public IActionResult CloseTheBoard([FromQuery] string world)
    {
        exchange.World(world).CloseBoard();
        return Ok();
    }

    [HttpPost("/Realtime/Board/Open")]
    public IActionResult OpenTheBoard([FromQuery] string world)
    {
        exchange.World(world).OpenBoard();
        return Ok();
    }

    [HttpPost("/Realtime/Board/News")]
    public IActionResult PushNews([FromQuery] string world, [FromQuery] string text)
    {
        exchange.World(world).PushToBoard(Board.News.Name, JsonSerializer.Serialize(new { text }));
        return Ok();
    }

    /// <summary>A bulletin, its data JSON or garbled, and then the all clear.</summary>
    [HttpPost("/Realtime/Board/Bulletin")]
    public IActionResult PushBulletin([FromQuery] string world, [FromQuery] bool garbled)
    {
        var board = exchange.World(world);
        board.PushToBoard(Board.Bulletin.Name, garbled ? "a bulletin, not JSON" : "{}");
        board.PushToBoard(Board.AllClear.Name, "{}");
        return Ok();
    }

    /// <summary>How many of the page's line and board connections the server holds open, and how many calls to the line it refused, now and as they change.</summary>
    [HttpGet("/Realtime/Switchboard")]
    public async Task Lines([FromQuery] string world, CancellationToken stopped)
    {
        var line = exchange.World(world);
        var watcher = line.Watch();
        Response.ContentType = "text/event-stream";
        try
        {
            await foreach (var count in watcher.Reader.ReadAllAsync(stopped))
                await Write(Switch.Lines.Name, new { hub = count.Hub, board = count.Board, refused = count.Refused }, stopped);
        }
        catch (OperationCanceledException)
        {
            // The page left.
        }
        finally
        {
            line.Unwatch(watcher);
        }
    }

    private Task Write(string name, object data, CancellationToken stopped) =>
        WriteData(name, JsonSerializer.Serialize(data), stopped);

    private async Task WriteData(string name, string data, CancellationToken stopped)
    {
        await Response.WriteAsync($"event: {name}\ndata: {data}\n\n", stopped);
        await Response.Body.FlushAsync(stopped);
    }

    private sealed class World
    {
        private int _count;

        internal Channel<(string Name, object Data)> Pushes { get; } = Channel.CreateUnbounded<(string, object)>();

        internal int Next() => Interlocked.Increment(ref _count);
    }
}
