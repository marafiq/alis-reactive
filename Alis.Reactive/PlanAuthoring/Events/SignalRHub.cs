using System.Runtime.CompilerServices;

namespace Alis.Reactive
{
    /// <summary>
    /// A SignalR hub, declared once: the methods the server calls on the page, each named by its member,
    /// with the argument each sends.
    /// </summary>
    /// <remarks>
    /// <code>
    /// public sealed class ResidentStatusUpdates : SignalRHub
    /// {
    ///     public SignalRMessage&lt;ResidentStatus&gt; StatusChanged =&gt; Message&lt;ResidentStatus&gt;();
    /// }
    ///
    /// Html.On(plan, t =&gt; t.SignalR(SignalRHub.At&lt;ResidentStatusUpdates&gt;("/hubs/resident-status"), h =&gt; h.StatusChanged, (status, p) =&gt; ...));
    /// </code>
    /// A method's name is its member's name camel-cased (<c>StatusChanged</c> is <c>statusChanged</c>; SignalR
    /// matches it in any case) unless <c>name:</c> gives another. The server calls each method by its declared
    /// <c>Name</c>, so the hub and the page cannot disagree on it. The page loads the SignalR client script
    /// before the framework's.
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
        /// <param name="name">The client method name, when it is not the declaring member's name camel-cased.</param>
        /// <param name="declaredBy">The declaring member; the compiler supplies it.</param>
        protected static SignalRMessage Message(string? name = null, [CallerMemberName] string declaredBy = "") =>
            new SignalRMessage(DeclaredName.Of(name, declaredBy));

        /// <summary>Declares a method the server calls with one <typeparamref name="TPayload"/> argument.</summary>
        /// <typeparam name="TPayload">The argument, read by camel-case JSON names.</typeparam>
        /// <param name="name">The client method name, when it is not the declaring member's name camel-cased.</param>
        /// <param name="declaredBy">The declaring member; the compiler supplies it.</param>
        protected static SignalRMessage<TPayload> Message<TPayload>(string? name = null, [CallerMemberName] string declaredBy = "")
            where TPayload : class, new() =>
            new SignalRMessage<TPayload>(DeclaredName.Of(name, declaredBy));
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
