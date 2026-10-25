using System.Runtime.CompilerServices;

namespace Alis.Reactive
{
    /// <summary>
    /// A server-sent event stream (an <c>EventSource</c>), declared once: the events it sends, each named
    /// by its member, with the payloads they carry.
    /// </summary>
    /// <remarks>
    /// <code>
    /// public sealed class FacilityAlerts : ServerPushStream
    /// {
    ///     public ServerPushMessage&lt;FacilityAlert&gt; Alert =&gt; Message&lt;FacilityAlert&gt;();
    /// }
    ///
    /// Html.On(plan, t =&gt; t.ServerPush(ServerPushStream.At&lt;FacilityAlerts&gt;(url), s =&gt; s.Alert, (alert, p) =&gt; ...));
    /// </code>
    /// An event's SSE name is its member's name camel-cased (<c>Alert</c> is <c>alert</c>) unless <c>name:</c>
    /// gives another; messages the server sends with no event name are <c>Message(name: "message")</c>. The
    /// server writes each event under its declared <c>Name</c>, so the stream and the page cannot disagree on it.
    /// </remarks>
    public abstract class ServerPushStream
    {
        /// <summary>The stream at a URL, for a <c>ServerPush</c> trigger.</summary>
        /// <typeparam name="TStream">The stream's declaration.</typeparam>
        /// <param name="url">The stream's URL.</param>
        public static ServerPushSource<TStream> At<TStream>(string url)
            where TStream : ServerPushStream, new() =>
            new ServerPushSource<TStream>(DeclaredName.RequiredUrl(url), new TStream());

        /// <summary>Declares an event the stream sends with no payload the page reads.</summary>
        /// <remarks>
        /// The server still writes a data line, as JSON (<c>data: {}</c>): a browser drops an event that
        /// has no data, and the page reads every event's data as JSON.
        /// </remarks>
        /// <param name="name">The SSE event name, when it is not the declaring member's name camel-cased.</param>
        /// <param name="declaredBy">The declaring member; the compiler supplies it.</param>
        protected static ServerPushMessage Message(string? name = null, [CallerMemberName] string declaredBy = "") =>
            new ServerPushMessage(DeclaredName.Of(name, declaredBy));

        /// <summary>Declares an event the stream sends with a JSON <typeparamref name="TPayload"/>.</summary>
        /// <typeparam name="TPayload">The event's data, read by camel-case JSON names.</typeparam>
        /// <param name="name">The SSE event name, when it is not the declaring member's name camel-cased.</param>
        /// <param name="declaredBy">The declaring member; the compiler supplies it.</param>
        protected static ServerPushMessage<TPayload> Message<TPayload>(string? name = null, [CallerMemberName] string declaredBy = "")
            where TPayload : class, new() =>
            new ServerPushMessage<TPayload>(DeclaredName.Of(name, declaredBy));
    }

    /// <summary>A declared stream at a URL; see <see cref="ServerPushStream.At{TStream}"/>.</summary>
    /// <typeparam name="TStream">The stream's declaration.</typeparam>
    public sealed class ServerPushSource<TStream> where TStream : ServerPushStream, new()
    {
        internal ServerPushSource(string url, TStream stream)
        {
            Url = url;
            Stream = stream;
        }

        internal string Url { get; }
        internal TStream Stream { get; }
    }

    /// <summary>An event a <see cref="ServerPushStream"/> sends, with no payload the page reads.</summary>
    public sealed class ServerPushMessage
    {
        internal ServerPushMessage(string eventType) => Name = eventType;

        /// <summary>The SSE event name, for the server that writes the stream.</summary>
        public string Name { get; }

        /// <inheritdoc />
        public override string ToString() => Name;
    }

    /// <summary>An event a <see cref="ServerPushStream"/> sends, with its payload.</summary>
    /// <typeparam name="TPayload">The event's data.</typeparam>
    public sealed class ServerPushMessage<TPayload> where TPayload : class, new()
    {
        internal ServerPushMessage(string eventType) => Name = eventType;

        /// <summary>The SSE event name, for the server that writes the stream.</summary>
        public string Name { get; }

        /// <inheritdoc />
        public override string ToString() => Name;
    }
}
