using System;
using Alis.Reactive.Plan;

namespace Alis.Reactive.Components;

public abstract class Plugin : JsObject
{
    protected Plugin(string name)
        : this(name, PluginEvents.EventTarget)
    {
    }

    protected Plugin(string name, PluginEvents events)
    {
        Name = name;
        Events = events;
    }

    public string Name { get; }

    internal PluginEvents Events { get; }

    internal string CheckedName()
    {
        if (!JavaScriptNames.IsRegistryName(Name))
        {
            throw new InvalidOperationException(
                $"Plugin name '{Name}' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen. A name with a dot is the framework's own.");
        }

        return Name;
    }
}

public sealed class PluginEvents
{
    private PluginEvents(Listening listen)
    {
        Listen = listen;
    }

    public static PluginEvents EventTarget { get; } = new(new MethodListening("addEventListener", "removeEventListener"));

    public static PluginEvents CallbackProperties { get; } = new(new AssignmentListening());

    internal Listening Listen { get; }

    public static PluginEvents Methods(string subscribe, string unsubscribe)
        => new(new MethodListening(JavaScriptNames.Checked(subscribe), JavaScriptNames.Checked(unsubscribe)));

    public static PluginEvents ReturnedUnsubscriber(string subscribe)
        => new(new ReturnedListening(JavaScriptNames.Checked(subscribe)));
}
