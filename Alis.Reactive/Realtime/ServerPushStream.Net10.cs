#if NET10_0_OR_GREATER
using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Alis.Reactive.Declarations;

namespace Alis.Reactive.Realtime;

/// <summary>
/// A server-sent event stream, declared once and shared by the endpoint that writes it and the view that listens to it.
/// </summary>
/// <remarks>
/// Declare each event the endpoint sends as a member, with the class of its message:
/// <c>public ServerEvent&lt;Bulletin&gt; Bulletin =&gt; Message&lt;Bulletin&gt;();</c>. The endpoint writes it with
/// <see cref="ServerEvents.Write"/> and a view handles it with <c>trigger.ServerPush</c>, so both name one event with one
/// message type. A page holds the stream while a plan that names it lives, and the stream opens at the first plan that
/// holds it and closes when the last one leaves.
/// </remarks>
public abstract class ServerPushStream : JsObject
{
    /// <summary>Names the stream at the address its endpoint answers.</summary>
    /// <typeparam name="TStream">The stream's declaration.</typeparam>
    /// <param name="url">The address of the stream's endpoint.</param>
    /// <returns>The stream, for a trigger or a step of a view.</returns>
    public static ServerPushSource<TStream> At<TStream>(string url)
        where TStream : ServerPushStream, new()
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("A stream needs the address of its endpoint.", nameof(url));
        return new ServerPushSource<TStream>(url);
    }

    /// <summary>Gets the state of the page's connection to the stream.</summary>
    public ReadOnly<ConnectionState> State => ReadOnly<ConnectionState>();

    /// <summary>Gets the event raised when the connection drops and the browser starts to reconnect.</summary>
    public Event Reconnecting => Event();

    /// <summary>Gets the event raised when the connection opens again after it was lost.</summary>
    public Event Reconnected => Event();

    /// <summary>Gets the event raised when the browser gives up on the connection.</summary>
    /// <remarks>A close a step makes raises nothing.</remarks>
    public Event Closed => Event();

    /// <summary>Closes the page's connection for every plan that holds it, so no later message runs a handler.</summary>
    /// <remarks>A closed stream runs no closed steps and shows no reconnect offer; it opens again only when a plan holds it after the last plan that held it has left.</remarks>
    public void Close() => Method();

    /// <summary>Declares an event the endpoint sends, with the class of its message.</summary>
    /// <typeparam name="TMessage">The class of the message, as the app's JSON writes it.</typeparam>
    /// <param name="name">The event's name on the wire, when it is not the declaring member's name in camel case; a message the endpoint sends with no event name is <c>"message"</c>.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    /// <returns>The event.</returns>
    [TypedDslExemption("The name is the event's own on the wire, which the endpoint writes and the browser hears.")]
    protected ServerEvent<TMessage> Message<TMessage>(string? name = null, [CallerMemberName] string declaredBy = "")
    {
        if (name is not null && string.IsNullOrWhiteSpace(name)) throw new ArgumentException("An event needs a name.", nameof(name));
        if (name is null && string.IsNullOrWhiteSpace(declaredBy)) throw new ArgumentException("Declare an event as a member of the stream, for example public ServerEvent<Bulletin> Bulletin => Message<Bulletin>();", nameof(declaredBy));
        return new ServerEvent<TMessage>(this, name ?? JsonNamingPolicy.CamelCase.ConvertName(declaredBy));
    }
}

/// <summary>
/// A server-sent event stream at the address of its endpoint, which a view's trigger and step name.
/// </summary>
/// <typeparam name="TStream">The stream's declaration.</typeparam>
public sealed class ServerPushSource<TStream>
    where TStream : ServerPushStream, new()
{
    internal ServerPushSource(string url)
    {
        Url = url;
    }

    internal string Url { get; }
}

/// <summary>
/// An event a stream's endpoint sends, with the class of its message.
/// </summary>
/// <typeparam name="TMessage">The class of the message, as the app's JSON writes it.</typeparam>
public sealed class ServerEvent<TMessage>
{
    internal ServerEvent(ServerPushStream stream, string name)
    {
        Stream = stream;
        Name = name;
    }

    internal ServerPushStream Stream { get; }

    internal string Name { get; }

    /// <summary>Makes the event's message, for the endpoint to write.</summary>
    /// <param name="message">The message.</param>
    /// <returns>The message under the event's name.</returns>
    public ServerMessage Of(TMessage message) => new(Name, options => JsonSerializer.Serialize(message, options));
}
#endif
