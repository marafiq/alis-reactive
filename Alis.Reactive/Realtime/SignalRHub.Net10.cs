#if NET10_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;
using Microsoft.AspNetCore.SignalR;

namespace Alis.Reactive.Realtime;

/// <summary>
/// A SignalR hub, declared once and shared by the hub that pushes to the page and the view that listens to it and calls it.
/// </summary>
/// <remarks>
/// Declare each push the hub sends as a member, with the class of its message:
/// <c>public HubPush&lt;StatusChange&gt; StatusChanged =&gt; Push&lt;StatusChange&gt;();</c>, which the hub sends with
/// <see cref="HubPush{TMessage}.SendAsync"/> and a view handles with <c>trigger.SignalR</c>. Declare each hub method a
/// view calls as a method named as the hub names it: <c>public MethodResult&lt;string&gt; Roster() =&gt; Invoke&lt;string&gt;();</c>.
/// A page holds the hub while a plan that names it lives: it connects at the first plan that holds it, closes when the
/// last one leaves, and loads the SignalR client only on a page whose plans name a hub.
/// </remarks>
public abstract class SignalRHub : JsObject
{
    /// <summary>Names the hub at the address the app maps it to.</summary>
    /// <typeparam name="THub">The hub's declaration.</typeparam>
    /// <param name="url">The address the app maps the hub to.</param>
    /// <returns>The hub, for a trigger or a step of a view.</returns>
    public static SignalRSource<THub> At<THub>(string url)
        where THub : SignalRHub, new()
    {
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("A hub needs the address the app maps it to.", nameof(url));
        return new SignalRSource<THub>(url);
    }

    /// <summary>Gets the state of the page's connection to the hub.</summary>
    public ReadOnly<ConnectionState> State => ReadOnly<ConnectionState>();

    /// <summary>Gets the id the hub knows the page's connection by, which reads nothing while the page is not connected.</summary>
    public ReadOnly<string?> ConnectionId => ReadOnly<string?>();

    /// <summary>Gets the event raised when the connection drops and the client starts to reconnect.</summary>
    public Event Reconnecting => Event();

    /// <summary>Gets the event raised when the connection opens again after it was lost, by the client or by a retry of the reconnect offer, with the id the hub now knows the page by.</summary>
    public Event<HubReconnected> Reconnected => Event<HubReconnected>();

    /// <summary>Gets the event raised when the client gives up on the connection, its first start included.</summary>
    public Event Closed => Event();

    /// <summary>Declares a push the hub sends, with the class of its message.</summary>
    /// <typeparam name="TMessage">The class of the message, as the hub's JSON writes it.</typeparam>
    /// <param name="name">The push's name on the wire, when it is not the declaring member's name in camel case.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    /// <returns>The push.</returns>
    [TypedDslExemption("The name is the push's own on the wire, which the hub sends and the browser hears.")]
    protected HubPush<TMessage> Push<TMessage>(string? name = null, [CallerMemberName] string declaredBy = "")
    {
        if (name is not null && string.IsNullOrWhiteSpace(name)) throw new ArgumentException("A push needs a name.", nameof(name));
        if (name is null && string.IsNullOrWhiteSpace(declaredBy)) throw new ArgumentException("Declare a push as a member of the hub, for example public HubPush<StatusChange> StatusChanged => Push<StatusChange>();", nameof(declaredBy));
        return new HubPush<TMessage>(name ?? JsonNamingPolicy.CamelCase.ConvertName(declaredBy));
    }

    /// <summary>Declares a hub method that takes no argument and answers, and invokes it where the pipeline reaches the line; the pipeline waits for the answer.</summary>
    /// <remarks>A call before the hub's first start settles waits for it; a call while the hub reconnects or after it closed stops the pipeline with one logged error.</remarks>
    /// <typeparam name="TResult">The type of the hub method's answer.</typeparam>
    /// <param name="declaredBy">The name of the C# member that declares it, the hub method's name, which the compiler supplies.</param>
    /// <returns>The hub's answer, a source every later read reads as it was then.</returns>
    protected MethodResult<TResult> Invoke<TResult>([CallerMemberName] string declaredBy = "") => Invoke<TResult>([], declaredBy);

    /// <summary>Declares a hub method that answers, and invokes it with its arguments where the pipeline reaches the line; the pipeline waits for the answer.</summary>
    /// <remarks>A call before the hub's first start settles waits for it; a call while the hub reconnects or after it closed stops the pipeline with one logged error.</remarks>
    /// <typeparam name="TResult">The type of the hub method's answer.</typeparam>
    /// <param name="arguments">The call's arguments in order, each read when the line runs.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, the hub method's name, which the compiler supplies.</param>
    /// <returns>The hub's answer, a source every later read reads as it was then.</returns>
    protected MethodResult<TResult> Invoke<TResult>(IReadOnlyList<Arg> arguments, [CallerMemberName] string declaredBy = "")
        => ResultAwaited<TResult>([Fixed(HubMethod(declaredBy)), .. arguments], "invoke", declaredBy);

    /// <summary>Declares a hub method that takes no argument, and sends it where the pipeline reaches the line; the pipeline waits until the hub has it and expects no answer.</summary>
    /// <remarks>A call before the hub's first start settles waits for it; a call while the hub reconnects or after it closed stops the pipeline with one logged error.</remarks>
    /// <param name="declaredBy">The name of the C# member that declares it, the hub method's name, which the compiler supplies.</param>
    protected void Send([CallerMemberName] string declaredBy = "") => Send([], declaredBy);

    /// <summary>Declares a hub method, and sends it with its arguments where the pipeline reaches the line; the pipeline waits until the hub has it and expects no answer.</summary>
    /// <remarks>A call before the hub's first start settles waits for it; a call while the hub reconnects or after it closed stops the pipeline with one logged error.</remarks>
    /// <param name="arguments">The call's arguments in order, each read when the line runs.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, the hub method's name, which the compiler supplies.</param>
    protected void Send(IReadOnlyList<Arg> arguments, [CallerMemberName] string declaredBy = "")
        => CallAwaited([Fixed(HubMethod(declaredBy)), .. arguments], "send", declaredBy);

    private static string HubMethod(string declaredBy)
    {
        if (string.IsNullOrWhiteSpace(declaredBy)) throw new ArgumentException("Declare a hub method as a member of the hub, for example public MethodResult<string> Roster() => Invoke<string>();", nameof(declaredBy));
        return declaredBy;
    }
}

/// <summary>
/// A SignalR hub at the address the app maps it to, which a view's trigger and step name.
/// </summary>
/// <typeparam name="THub">The hub's declaration.</typeparam>
public sealed class SignalRSource<THub>
    where THub : SignalRHub, new()
{
    internal SignalRSource(string url)
    {
        Url = url;
    }

    internal string Url { get; }
}

/// <summary>
/// A push a hub sends, with the class of its message.
/// </summary>
/// <typeparam name="TMessage">The class of the message, as the hub's JSON writes it.</typeparam>
public sealed class HubPush<TMessage>
{
    internal HubPush(string name)
    {
        Name = name;
    }

    internal string Name { get; }

    /// <summary>Sends the push to the clients, its message as the one argument a view's handler reads.</summary>
    /// <remarks>The hub writes the message with its JSON protocol's options, so a view reads it as the app's JSON writes it only when the hub writes under the app's JSON options.</remarks>
    /// <param name="clients">The clients to push to, such as <c>Clients.Caller</c> or <c>Clients.Group(name)</c>.</param>
    /// <param name="message">The message.</param>
    /// <param name="cancellationToken">Cancels the send.</param>
    /// <returns>The send.</returns>
    public Task SendAsync(IClientProxy clients, TMessage message, CancellationToken cancellationToken = default)
    {
        if (clients is null) throw new ArgumentNullException(nameof(clients));
        return clients.SendCoreAsync(Name, [message], cancellationToken);
    }
}

/// <summary>
/// What a hub's reconnected handler receives: the id the hub now knows the page's connection by.
/// </summary>
public sealed class HubReconnected : JsObject
{
    /// <summary>Gets the id the hub now knows the page's connection by.</summary>
    public ReadOnly<string> ConnectionId => ReadOnly<string>();
}
#endif
