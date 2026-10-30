using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Channels;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Brought
{
    /// <summary>
    /// The visit board's open streams, keyed by browser session: desks in one session (one community,
    /// one browser world) hear each other, and no other session does.
    /// </summary>
    public sealed class VisitBoardStreams
    {
        private static readonly JsonSerializerOptions Web = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        private static readonly VisitBoardStream Stream = new VisitBoardStream();

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Channel<BoardPush>, string>> _bySession =
            new ConcurrentDictionary<string, ConcurrentDictionary<Channel<BoardPush>, string>>();

        /// <summary>Opens a desk's stream and tells every desk in the session how many now watch the week.</summary>
        internal Channel<BoardPush> Open(string session, string family)
        {
            var channel = Channel.CreateUnbounded<BoardPush>();
            var streams = _bySession.GetOrAdd(session, _ => new ConcurrentDictionary<Channel<BoardPush>, string>());
            streams[channel] = family;
            TellHowManyWatch(streams);
            return channel;
        }

        /// <summary>Closes a desk's stream and tells the desks still open how many now watch.</summary>
        internal void Close(string session, Channel<BoardPush> channel)
        {
            if (!_bySession.TryGetValue(session, out var streams)) return;
            if (streams.TryRemove(channel, out _)) TellHowManyWatch(streams);
        }

        /// <summary>Tells every desk in the session that serves another family that a time was taken.</summary>
        internal void TellOtherDesks(string session, string family, SlotTaken taken)
        {
            if (!_bySession.TryGetValue(session, out var streams)) return;
            var push = new BoardPush(Stream.SlotTaken.Name, JsonSerializer.Serialize(taken, Web));
            foreach (var stream in streams.Where(open => open.Value != family).Select(open => open.Key).ToList())
                stream.Writer.TryWrite(push);
        }

        // Each open stream is one desk: a page that opened two streams for one URL would count twice.
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

    /// <summary>One server-sent event for a desk: its name and its JSON data.</summary>
    internal sealed record BoardPush(string Event, string Data);
}
