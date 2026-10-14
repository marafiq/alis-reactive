namespace Alis.Reactive
{
    /// <summary>
    /// A server-sent event stream (an <c>EventSource</c>), declared once: the events it sends, by the
    /// names the server writes, with the payloads they carry.
    /// </summary>
    /// <remarks>
    /// <code>
    /// public sealed class FacilityAlerts : ServerPushStream
    /// {
    ///     public ServerPushMessage&lt;FacilityAlert&gt; Alert =&gt; Message&lt;FacilityAlert&gt;("facility-alert");
    /// }
    ///
    /// Html.On(plan, t =&gt; t.ServerPush(ServerPushStream.At&lt;FacilityAlerts&gt;(url), s =&gt; s.Alert, (alert, p) =&gt; ...));
    /// </code>
    /// Messages the server sends without an event name are <c>Message("message")</c>. The server writes
    /// each event under its declared <c>Name</c>, so the stream and the page cannot disagree on it.
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
        /// <param name="eventType">The SSE event name the server writes.</param>
        protected static ServerPushMessage Message(string eventType) =>
            new ServerPushMessage(DeclaredName.Required(eventType));

        /// <summary>Declares an event the stream sends with a JSON <typeparamref name="TPayload"/>.</summary>
        /// <typeparam name="TPayload">The event's data, read by camel-case JSON names.</typeparam>
        /// <param name="eventType">The SSE event name the server writes.</param>
        protected static ServerPushMessage<TPayload> Message<TPayload>(string eventType)
            where TPayload : class, new() =>
            new ServerPushMessage<TPayload>(DeclaredName.Required(eventType));
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
