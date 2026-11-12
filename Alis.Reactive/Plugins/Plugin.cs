using System;
using Alis.Reactive.Declarations;

namespace Alis.Reactive.Plugins;

/// <summary>
/// A JavaScript object the page registers under a name: the declaration of an app's or a component's own browser code.
/// </summary>
/// <remarks>
/// A plugin declares its members with the doors of <see cref="JsObject"/>, each typed, and a pipeline reads,
/// sets and calls them as it does a component's. A name no script registers, or two scripts register, stops the
/// pipeline that reaches the plugin.
/// </remarks>
public abstract class Plugin : JsObject
{
    /// <summary>Declares a plugin whose events are listened to as the DOM listens.</summary>
    /// <param name="name">The name the browser code registers the object under.</param>
    protected Plugin(string name) => throw new NotImplementedException();

    /// <summary>Declares a plugin and how its events are listened to.</summary>
    /// <param name="name">The name the browser code registers the object under.</param>
    /// <param name="events">How the plugin's events are listened to and released.</param>
    protected Plugin(string name, EventListening events) => throw new NotImplementedException();

    /// <summary>Gets the name the browser code registers the object under.</summary>
    public string Name => throw new NotImplementedException();
}
