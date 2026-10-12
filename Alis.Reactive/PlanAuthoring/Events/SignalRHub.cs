namespace Alis.Reactive
{
    /// <summary>
    /// A SignalR hub, declared once: the methods the server calls on the page, by name, with the
    /// argument each sends.
    /// </summary>
    /// <remarks>
    /// <code>
    /// public sealed class ResidentStatusUpdates : SignalRHub
    /// {
    ///     public SignalRMessage&lt;ResidentStatus&gt; StatusChanged =&gt; Message&lt;ResidentStatus&gt;("StatusChanged");
    /// }
    ///
    /// Html.On(plan, t =&gt; t.SignalR(SignalRHub.At&lt;ResidentStatusUpdates&gt;("/hubs/resident-status"), h =&gt; h.StatusChanged, (status, p) =&gt; ...));
    /// </code>
    /// The server calls each method by its declared <c>Name</c>, so the hub and the page cannot disagree
    /// on it. The page loads the SignalR client script before the framework's.
    /// </remarks>
    public abstract class SignalRHub
    {
        /// <summary>The hub at a URL, for a <c>SignalR</c> trigger.</summary>
        /// <typeparam name="THub">The hub's declaration.</typeparam>
        /// <param name="url">The hub's URL.</param>
        public static SignalRSource<THub> At<THub>(string url)
            where THub : SignalRHub, new() =>
            new SignalRSource<THub>(DeclaredName.RequiredUrl(url), new THub());

        /// <summary>Declares a method the server calls with no argument the page reads.</summary>
        /// <param name="methodName">The client method name the server invokes.</param>
        protected static SignalRMessage Message(string methodName) =>
            new SignalRMessage(DeclaredName.Required(methodName));

        /// <summary>Declares a method the server calls with one <typeparamref name="TPayload"/> argument.</summary>
        /// <typeparam name="TPayload">The argument, read by camel-case JSON names.</typeparam>
        /// <param name="methodName">The client method name the server invokes.</param>
        protected static SignalRMessage<TPayload> Message<TPayload>(string methodName)
            where TPayload : class, new() =>
            new SignalRMessage<TPayload>(DeclaredName.Required(methodName));
    }

    /// <summary>A declared hub at a URL; see <see cref="SignalRHub.At{THub}"/>.</summary>
    /// <typeparam name="THub">The hub's declaration.</typeparam>
    public sealed class SignalRSource<THub> where THub : SignalRHub, new()
    {
        internal SignalRSource(string url, THub hub)
        {
            Url = url;
            Hub = hub;
        }

        internal string Url { get; }
        internal THub Hub { get; }
    }

    /// <summary>A method a <see cref="SignalRHub"/>'s server calls, with no argument the page reads.</summary>
    public sealed class SignalRMessage
    {
        internal SignalRMessage(string methodName) => Name = methodName;

        /// <summary>The client method name, for the server that calls it.</summary>
        public string Name { get; }

        /// <inheritdoc />
        public override string ToString() => Name;
    }

    /// <summary>A method a <see cref="SignalRHub"/>'s server calls, with its argument.</summary>
    /// <typeparam name="TPayload">The argument.</typeparam>
    public sealed class SignalRMessage<TPayload> where TPayload : class, new()
    {
        internal SignalRMessage(string methodName) => Name = methodName;

        /// <summary>The client method name, for the server that calls it.</summary>
        public string Name { get; }

        /// <inheritdoc />
        public override string ToString() => Name;
    }
}
