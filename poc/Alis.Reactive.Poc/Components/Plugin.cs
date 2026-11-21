using System;

namespace Alis.Reactive.Components;

public abstract class Plugin : JsObject
{
    protected Plugin(string name)
        : this(name, EventListening.EventTarget)
    {
    }

    protected Plugin(string name, EventListening events)
    {
        Name = name;
        Events = events;
    }

    public string Name { get; }

    internal EventListening Events { get; }

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
