using System.Runtime.CompilerServices;

namespace Alis.Reactive
{
    /// <summary>
    /// A SignalR hub, declared once like any JavaScript object: the methods the server calls on the page
    /// as events, each named by its member with the argument it sends; the methods the page calls on the
    /// server, each named by its member; and the connection's own members.
    /// </summary>
    /// <remarks>
    /// <code>
    /// public sealed class ResidentStatusFeed : SignalRHub
    /// {
    ///     public Event&lt;ResidentStatus&gt; StatusChanged =&gt; Event&lt;ResidentStatus&gt;();          // the server calls it with one argument
    ///     public Event BoardCleared =&gt; Event();                                          // the server calls it with none
    ///     public void ClearBoard() =&gt; Send();                                            // the page sends it to the server
    ///     public TypedSource&lt;string&gt; Acknowledge(Arg&lt;string&gt; nurse) =&gt; Invoke&lt;string&gt;(nurse); // the page invokes it and reads the answer
    /// }
    ///
    /// var residents = SignalRHub.At&lt;ResidentStatusFeed&gt;("/hubs/resident-status");
    /// Html.On(plan, t =&gt; t.SignalR(residents, h =&gt; h.StatusChanged, (status, p) =&gt; ...));
    /// Html.On(plan, t =&gt; t.SignalR(residents, h =&gt; h.Reconnected, (args, p) =&gt; p.Element("link").Text.Set(args.ConnectionId)));
    /// p.Hub(residents).ClearBoard();
    /// </code>
    /// A method's name is its member's name camel-cased (<c>StatusChanged</c> is <c>statusChanged</c>; SignalR
    /// matches it in any case) unless <c>name:</c> gives another. The server calls each client method by its
    /// declared <c>Name</c>, so the hub and the page cannot disagree on it, and an argument's members are read
    /// by their camel-case JSON names. The SignalR client ships in the framework's script.
    /// </remarks>
    public abstract class SignalRHub : JsObject
    {
        /// <summary>The hub at a URL, for a <c>SignalR</c> trigger and for <c>p.Hub(..)</c>.</summary>
        /// <typeparam name="THub">The hub's declaration.</typeparam>
        /// <param name="url">The hub's URL.</param>
        public static SignalRSource<THub> At<THub>(string url)
            where THub : SignalRHub, new() =>
            new SignalRSource<THub>(DeclaredName.RequiredUrl(url), new THub());

        /// <summary>The connection's state.</summary>
        public ReadOnly<SignalRConnectionState> State => ReadOnly<SignalRConnectionState>();

        /// <summary>The connection's id while connected; the server knows the page by it.</summary>
        public ReadOnly<string?> ConnectionId => ReadOnly<string?>();

        /// <summary>The connection dropped and is being re-established.</summary>
        public Event Reconnecting => Event();

        /// <summary>The connection is re-established, under a new id.</summary>
        public Event<SignalRReconnectedArgs> Reconnected => Event<SignalRReconnectedArgs>();

        /// <summary>The connection closed for good; the retry indicator offers to reconnect.</summary>
        public Event Close => Event();

        /// <summary>Declares a hub method the page sends with no argument; the steps after it wait until it is sent.</summary>
        /// <param name="name">The hub method's name, when it is not the declaring member's name camel-cased.</param>
        /// <param name="declaredBy">The declaring member; the compiler supplies it.</param>
        protected void Send(string? name = null, [CallerMemberName] string declaredBy = "") =>
            MethodAsync(Fixed(DeclaredName.Of(name, declaredBy)), name: "send", declaredBy: declaredBy);

        /// <summary>Declares a hub method the page sends with one argument; the steps after it wait until it is sent.</summary>
        /// <param name="argument">The argument: a literal, or a value read when the step runs.</param>
        /// <param name="name">The hub method's name, when it is not the declaring member's name camel-cased.</param>
        /// <param name="declaredBy">The declaring member; the compiler supplies it.</param>
        protected void Send(Arg argument, string? name = null, [CallerMemberName] string declaredBy = "") =>
            MethodAsync(Fixed(DeclaredName.Of(name, declaredBy)), argument, name: "send", declaredBy: declaredBy);

        /// <summary>Declares a hub method the page invokes with no argument; the steps after it wait until the server completes it.</summary>
        /// <param name="name">The hub method's name, when it is not the declaring member's name camel-cased.</param>
        /// <param name="declaredBy">The declaring member; the compiler supplies it.</param>
        protected void Invoke(string? name = null, [CallerMemberName] string declaredBy = "") =>
            MethodAsync(Fixed(DeclaredName.Of(name, declaredBy)), name: "invoke", declaredBy: declaredBy);

        /// <summary>Declares a hub method the page invokes with one argument; the steps after it wait until the server completes it.</summary>
        /// <param name="argument">The argument: a literal, or a value read when the step runs.</param>
        /// <param name="name">The hub method's name, when it is not the declaring member's name camel-cased.</param>
        /// <param name="declaredBy">The declaring member; the compiler supplies it.</param>
        protected void Invoke(Arg argument, string? name = null, [CallerMemberName] string declaredBy = "") =>
            MethodAsync(Fixed(DeclaredName.Of(name, declaredBy)), argument, name: "invoke", declaredBy: declaredBy);

        /// <summary>Declares a hub method the page invokes with no argument and whose answer it reads; the step that first reads the answer waits for it.</summary>
        /// <typeparam name="TResult">The C# type of the answer.</typeparam>
        /// <param name="name">The hub method's name, when it is not the declaring member's name camel-cased.</param>
        /// <param name="declaredBy">The declaring member; the compiler supplies it.</param>
        protected TypedSource<TResult> Invoke<TResult>(string? name = null, [CallerMemberName] string declaredBy = "") =>
            MethodAsync<TResult>(Fixed(DeclaredName.Of(name, declaredBy)), name: "invoke", declaredBy: declaredBy);

        /// <summary>Declares a hub method the page invokes with one argument and whose answer it reads; the step that first reads the answer waits for it.</summary>
        /// <typeparam name="TResult">The C# type of the answer.</typeparam>
        /// <param name="argument">The argument: a literal, or a value read when the step runs.</param>
        /// <param name="name">The hub method's name, when it is not the declaring member's name camel-cased.</param>
        /// <param name="declaredBy">The declaring member; the compiler supplies it.</param>
        protected TypedSource<TResult> Invoke<TResult>(Arg argument, string? name = null, [CallerMemberName] string declaredBy = "") =>
            MethodAsync<TResult>(Fixed(DeclaredName.Of(name, declaredBy)), argument, name: "invoke", declaredBy: declaredBy);
    }

    /// <summary>A hub connection's state, as the SignalR client names it.</summary>
    public enum SignalRConnectionState
    {
        /// <summary>Not connected.</summary>
        Disconnected,

        /// <summary>Connecting for the first time.</summary>
        Connecting,

        /// <summary>Connected.</summary>
        Connected,

        /// <summary>Closing.</summary>
        Disconnecting,

        /// <summary>Re-establishing a dropped connection.</summary>
        Reconnecting,
    }

    /// <summary>What <see cref="SignalRHub.Reconnected"/> hands its listener.</summary>
    public sealed class SignalRReconnectedArgs : JsObject
    {
        /// <summary>The connection's new id.</summary>
        public ReadOnly<string> ConnectionId => ReadOnly<string>();
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
}
