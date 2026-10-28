using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Realtime;

/// <summary>Server-sent news for one page's world, and the pushes that send it (capabilities of realtime).</summary>
public sealed class RealtimeController : Controller
{
    private static readonly RoundStream Stream = new();
    private static readonly ConcurrentDictionary<string, World> Worlds = new();

    public IActionResult Round() => View(new RoundPage());

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

    private async Task Write(string name, object data, CancellationToken stopped)
    {
        await Response.WriteAsync($"event: {name}\ndata: {JsonSerializer.Serialize(data)}\n\n", stopped);
        await Response.Body.FlushAsync(stopped);
    }

    private sealed class World
    {
        private int _count;

        internal Channel<(string Name, object Data)> Pushes { get; } = Channel.CreateUnbounded<(string, object)>();

        internal int Next() => Interlocked.Increment(ref _count);
    }
}
