using System;
using System.Runtime.CompilerServices;
using Alis.Reactive.Declarations;

namespace Alis.Reactive.TriggersAndPipelines;

/// <summary>
/// An event of the page that an app declares once, so the step or the markup that raises it and the handler that
/// hears it name one declaration.
/// </summary>
/// <remarks>
/// Declare it as a static member: <c>public static readonly AppEvent Opened = AppEvent.Declare();</c>. Its name in
/// the browser is the declaring member's name in camel case, or the <c>name:</c> it gives.
/// </remarks>
public sealed class AppEvent
{
    private AppEvent()
    {
    }

    /// <summary>Declares an event that carries no payload.</summary>
    /// <param name="name">The event's name in the browser, when it is not the declaring member's name in camel case.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    /// <returns>The event.</returns>
    [TypedDslExemption(NamesTheEventAsTheBrowserHearsIt)]
    public static AppEvent Declare(string? name = null, [CallerMemberName] string declaredBy = "") => throw new NotImplementedException();

    /// <summary>Declares an event that carries a payload.</summary>
    /// <typeparam name="TPayload">The class of the payload, as the app's JSON writes it.</typeparam>
    /// <param name="name">The event's name in the browser, when it is not the declaring member's name in camel case.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    /// <returns>The event.</returns>
    [TypedDslExemption(NamesTheEventAsTheBrowserHearsIt)]
    public static AppEvent<TPayload> Declare<TPayload>(string? name = null, [CallerMemberName] string declaredBy = "") => throw new NotImplementedException();

    private const string NamesTheEventAsTheBrowserHearsIt = "The name is the event's own in the browser, where another script raises or hears it.";
}

/// <summary>
/// An event of the page that carries a payload; <see cref="AppEvent"/> declares it.
/// </summary>
/// <typeparam name="TPayload">The class of the payload, as the app's JSON writes it.</typeparam>
public sealed class AppEvent<TPayload>
{
    internal AppEvent()
    {
    }
}
