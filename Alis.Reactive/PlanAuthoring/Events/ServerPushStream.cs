namespace Alis.Reactive
{
    /// <summary>
    /// A server-sent event stream, declared once like any JavaScript object: the events it sends, each
    /// named by its member with the payload it carries, and the stream's own members.
    /// </summary>
    /// <remarks>
    /// <code>
    /// public sealed class FacilityAlerts : ServerPushStream
    /// {
    ///     public Event&lt;FacilityAlert&gt; Alert =&gt; Event&lt;FacilityAlert&gt;();   // an event with data the page reads
    ///     public Event AllClear =&gt; Event();                               // an event with no data the page reads
    /// }
    ///
    /// public sealed class FacilityAlert : JsObject
    /// {
    ///     public ReadOnly&lt;string&gt; Message =&gt; ReadOnly&lt;string&gt;();
    /// }
    ///
    /// var alerts = ServerPushStream.At&lt;FacilityAlerts&gt;(url);
    /// Html.On(plan, t =&gt; t.ServerPush(alerts, s =&gt; s.Alert, (alert, p) =&gt; p.Element("alert").Text.Set(alert.Message)));
    /// p.Stream(alerts).Close();
    /// </code>
    /// An event's SSE name is its member's name camel-cased (<c>Alert</c> is <c>alert</c>) unless <c>name:</c>
    /// gives another; messages the server sends with no event name are <c>Event(name: "message")</c>. The
    /// server writes each event under its declared <c>Name</c>, so the stream and the page cannot disagree
    /// on it. The data line is read as JSON, so an event with no data the page reads still writes one
    /// (<c>data: {}</c>): a browser drops an event that has no data. The payload's members are read by
    /// their camel-case JSON names.
    /// </remarks>
    public abstract class ServerPushStream : JsObject
    {
        /// <summary><see cref="ReadyState"/> while the stream connects: <c>EventSource.CONNECTING</c>.</summary>
        public const int Connecting = 0;

        /// <summary><see cref="ReadyState"/> while the stream is open: <c>EventSource.OPEN</c>.</summary>
        public const int Open = 1;

        /// <summary><see cref="ReadyState"/> once the stream is closed: <c>EventSource.CLOSED</c>.</summary>
        public const int Closed = 2;

        /// <summary>The stream at a URL, for a <c>ServerPush</c> trigger and for <c>p.Stream(..)</c>.</summary>
        /// <typeparam name="TStream">The stream's declaration.</typeparam>
        /// <param name="url">The stream's URL.</param>
        public static ServerPushSource<TStream> At<TStream>(string url)
            where TStream : ServerPushStream, new() =>
            new ServerPushSource<TStream>(DeclaredName.RequiredUrl(url), new TStream());

        /// <summary>The connection's state: <see cref="Connecting"/>, <see cref="Open"/> or <see cref="Closed"/>.</summary>
        public ReadOnly<int> ReadyState => ReadOnly<int>();

        /// <summary>Closes the connection; no further event reaches the page.</summary>
        public void Close() => Method();
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
}
