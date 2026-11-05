using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Channels;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Brought
{
    public sealed class VisitBoardStreams
    {
        private static readonly JsonSerializerOptions Web = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        private static readonly VisitBoardStream Stream = new VisitBoardStream();

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Channel<BoardPush>, string>> _bySession =
            new ConcurrentDictionary<string, ConcurrentDictionary<Channel<BoardPush>, string>>();

        internal Channel<BoardPush> Open(string session, string family)
        {
            var channel = Channel.CreateUnbounded<BoardPush>();
            var streams = _bySession.GetOrAdd(session, _ => new ConcurrentDictionary<Channel<BoardPush>, string>());
            streams[channel] = family;
            TellHowManyWatch(streams);
            return channel;
        }

        internal void Close(string session, Channel<BoardPush> channel)
        {
            if (!_bySession.TryGetValue(session, out var streams)) return;
            if (streams.TryRemove(channel, out _)) TellHowManyWatch(streams);
        }

        internal void TellOtherDesks(string session, string family, SlotTaken taken)
        {
            if (!_bySession.TryGetValue(session, out var streams)) return;
            var push = new BoardPush(Stream.SlotTaken.Name, JsonSerializer.Serialize(taken, Web));
            foreach (var stream in streams.Where(open => open.Value != family).Select(open => open.Key).ToList())
                stream.Writer.TryWrite(push);
        }

        private static void TellHowManyWatch(ConcurrentDictionary<Channel<BoardPush>, string> streams)
        {
            var open = streams.Keys.ToList();
            var watching = new DesksWatching
            {
                Watching = open.Count,
                Summary = open.Count == 1 ? "1 desk is watching this week" : $"{open.Count} desks are watching this week",
            };
            var push = new BoardPush(Stream.DesksWatching.Name, JsonSerializer.Serialize(watching, Web));
            foreach (var stream in open)
                stream.Writer.TryWrite(push);
        }
    }

    internal sealed record BoardPush(string Event, string Data);
}
