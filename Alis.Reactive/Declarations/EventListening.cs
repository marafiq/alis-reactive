using Alis.Reactive.Schema;

namespace Alis.Reactive.Declarations;

/// <summary>
/// How an object's events are listened to and released: the JavaScript members a listener is added and removed by.
/// </summary>
public sealed class EventListening
{
    internal EventListening(Listening grammar)
    {
        Grammar = grammar;
    }

    internal Listening Grammar { get; }

    /// <summary>Gets the listening of a DOM event target: <c>addEventListener</c> and <c>removeEventListener</c>.</summary>
    public static EventListening EventTarget { get; } = new(new MethodListening("addEventListener", "removeEventListener"));

    /// <summary>Gets the listening of an object that raises an event by calling the property the event names.</summary>
    /// <remarks>
    /// The framework assigns one function to the property at its first listener. The function calls what the
    /// property held before, then each listener in the order it was added, and the last release puts that value back.
    /// </remarks>
    public static EventListening CallbackProperties { get; } = new(AssignmentListening.Instance);

    /// <summary>Listens by calling one method with the event's name and the listener, and releases by calling another with the same two.</summary>
    /// <param name="subscribe">The JavaScript name of the method that adds a listener.</param>
    /// <param name="unsubscribe">The JavaScript name of the method that removes one.</param>
    /// <returns>The listening.</returns>
    [TypedDslExemption("The two methods are named by their JavaScript names: this door describes the object's own API to the framework.")]
    public static EventListening Methods(string subscribe, string unsubscribe) => new(new MethodListening(subscribe, unsubscribe));

    /// <summary>Listens by calling a method with the event's name and the listener, and releases by calling the function that call returned.</summary>
    /// <param name="subscribe">The JavaScript name of the method that adds a listener and returns its remover.</param>
    /// <returns>The listening.</returns>
    [TypedDslExemption("The method is named by its JavaScript name: this door describes the object's own API to the framework.")]
    public static EventListening ReturnedUnsubscriber(string subscribe) => new(new ReturnedListening(subscribe));
}
