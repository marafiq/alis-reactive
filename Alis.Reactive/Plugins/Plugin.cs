using System;
using System.Linq;
using Alis.Reactive.Declarations;
using Alis.Reactive.Schema;

namespace Alis.Reactive.Plugins;

/// <summary>
/// A JavaScript object the page registers under a name: the declaration of an app's or a component's own browser code.
/// </summary>
/// <remarks>
/// A plugin declares its members with the doors of <see cref="JsObject"/>, each typed, and a pipeline reads,
/// sets and calls them as it does a component's. A name no script registers, or two scripts register, stops the
/// pipeline that reaches the plugin.
/// </remarks>
public abstract class Plugin : EventedObject
{
    private const string NoKey = "";

    private readonly EventListening _events;

    /// <summary>Declares a plugin whose events are listened to as the DOM listens.</summary>
    /// <param name="name">The name the browser code registers the object under.</param>
    protected Plugin(string name)
        : this(name, EventListening.EventTarget)
    {
    }

    /// <summary>Declares a plugin and how its events are listened to.</summary>
    /// <param name="name">The name the browser code registers the object under.</param>
    /// <param name="events">How the plugin's events are listened to and released.</param>
    protected Plugin(string name, EventListening events)
    {
        Name = name;
        _events = events;
    }

    /// <summary>Gets the name the browser code registers the object under.</summary>
    public string Name { get; }

    /// <inheritdoc />
    protected internal sealed override EventListening Listening => _events;

    internal string HeardBy => _events.Written;

    internal PluginLocator Reached() => LocatedBy(EventListening.EventTarget);

    internal PluginLocator Heard() => LocatedBy(_events);

    private static bool IsLetter(char character) => character is >= 'A' and <= 'Z' or >= 'a' and <= 'z';

    private PluginLocator LocatedBy(EventListening listening)
    {
        var namedByItsCharacters = Name.Length > 0 && IsLetter(Name[0]) && Name.All(character => IsLetter(character) || character is >= '0' and <= '9' or '_' or '-');
        if (!namedByItsCharacters)
            throw new InvalidOperationException($"Plugin name '{Name}' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen. A name with a dot is the framework's own.");
        return new PluginLocator(Name, NoKey, listening.Grammar, NonePluginData.Instance);
    }
}
