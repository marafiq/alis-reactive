using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Brought
{
    /// <summary>
    /// The visit board's open streams, keyed by browser session: desks in one session (one community,
    /// one browser world) hear each other, and no other session does.
    /// </summary>
    public sealed class VisitBoardStreams
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Channel<string>, string>> _bySession =
            new ConcurrentDictionary<string, ConcurrentDictionary<Channel<string>, string>>();

        internal Channel<string> Open(string session, string family)
        {
            var channel = Channel.CreateUnbounded<string>();
            _bySession.GetOrAdd(session, _ => new ConcurrentDictionary<Channel<string>, string>())[channel] = family;
            return channel;
        }

        internal void Close(string session, Channel<string> channel)
        {
            if (_bySession.TryGetValue(session, out var streams)) streams.TryRemove(channel, out _);
        }

        /// <summary>Tells every desk in the session that serves another family.</summary>
        internal void TellOtherDesks(string session, string family, string message)
        {
            if (!_bySession.TryGetValue(session, out var streams)) return;
            foreach (var stream in streams.Where(open => open.Value != family).Select(open => open.Key).ToList())
                stream.Writer.TryWrite(message);
        }
    }
}
