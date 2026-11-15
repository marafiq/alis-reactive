using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
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
    private AppEvent(string name)
    {
        Name = name;
    }

    internal string Name { get; }

    /// <summary>Declares an event that carries no payload.</summary>
    /// <param name="name">The event's name in the browser, when it is not the declaring member's name in camel case.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    /// <returns>The event.</returns>
    [TypedDslExemption(NamesTheEventAsTheBrowserHearsIt)]
    public static AppEvent Declare(string? name = null, [CallerMemberName] string declaredBy = "") => new(NameOf(name, declaredBy));

    /// <summary>Declares an event that carries a payload.</summary>
    /// <typeparam name="TPayload">The class of the payload, as the app's JSON writes it.</typeparam>
    /// <param name="name">The event's name in the browser, when it is not the declaring member's name in camel case.</param>
    /// <param name="declaredBy">The name of the C# member that declares it, which the compiler supplies.</param>
    /// <returns>The event.</returns>
    [TypedDslExemption(NamesTheEventAsTheBrowserHearsIt)]
    public static AppEvent<TPayload> Declare<TPayload>(string? name = null, [CallerMemberName] string declaredBy = "") => new(NameOf(name, declaredBy));

    private const string NamesTheEventAsTheBrowserHearsIt = "The name is the event's own in the browser, where another script raises or hears it.";

    private static string NameOf(string? name, string declaredBy)
    {
        if (name is not null) return Given(name);
        if (string.IsNullOrWhiteSpace(declaredBy)) throw new ArgumentException(DeclareItAsAMember, nameof(declaredBy));
        return JsonNamingPolicy.CamelCase.ConvertName(declaredBy);
    }

    private const string DeclareItAsAMember = "Declare an event as a member of a class, for example public static readonly AppEvent Saved = AppEvent.Declare();";

    private static string Given(string name) => string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("An event needs a name.", nameof(name)) : name;
}

/// <summary>
/// An event of the page that carries a payload; <see cref="AppEvent"/> declares it.
/// </summary>
/// <typeparam name="TPayload">The class of the payload, as the app's JSON writes it.</typeparam>
public sealed class AppEvent<TPayload>
{
    internal AppEvent(string name)
    {
        Name = name;
    }

    internal string Name { get; }
}
