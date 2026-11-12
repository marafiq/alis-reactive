using System;
using Alis.Reactive.Declarations;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.Plugins;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// A view's plan: what the page's handlers do in the browser, written by the doors that take it.
/// </summary>
/// <typeparam name="TModel">The model of the view that writes the plan.</typeparam>
public sealed partial class ReactivePlan<TModel>
{
    internal ReactivePlan(PlanContent content)
    {
        Content = content;
    }

    internal PlanContent Content { get; }

    /// <summary>Reaches a plugin the page registers, to handle its events.</summary>
    /// <typeparam name="TPlugin">The plugin's declaration.</typeparam>
    /// <returns>The plugin, whose events a view handles with <c>Reactive</c>.</returns>
    public ReactivePlugin<TPlugin, TModel> Plugin<TPlugin>()
        where TPlugin : Plugin, new()
        => throw new NotImplementedException();

    /// <summary>Writes a value as the plan writes a literal, for the options a render helper starts its component with.</summary>
    /// <remarks>A date, an enum and a number reach the component's code in the forms the plan's own values take.</remarks>
    /// <param name="value">The value to write.</param>
    /// <returns>The value's JSON.</returns>
    public string Json<TValue>(TValue value) => throw new NotImplementedException();

    /// <summary>Names the rows of a grid-like list, for a render helper to declare each row action once and render it in every row.</summary>
    /// <param name="rowsId">The id the list's row controls are numbered under.</param>
    /// <typeparam name="TRow">The type of a row.</typeparam>
    /// <returns>The rows.</returns>
    public Rows<TModel, TRow> Rows<TRow>(string rowsId) => throw new NotImplementedException();
}

/// <summary>
/// A component a view rendered: its markup, and the door that handles its events.
/// </summary>
/// <typeparam name="TComponent">The component's declaration.</typeparam>
/// <typeparam name="TModel">The model of the view that rendered it.</typeparam>
public sealed partial class Rendered<TComponent, TModel>
    where TComponent : Component, new()
{
    internal Rendered()
    {
    }

    /// <summary>Runs a pipeline each time the component raises an event that has no payload.</summary>
    /// <param name="raised">The event, as a member of the component's declaration.</param>
    /// <param name="pipeline">The steps to run on each raise.</param>
    /// <returns>The rendered component.</returns>
    public Rendered<TComponent, TModel> Reactive(Func<TComponent, Event> raised, Action<Pipeline<TModel>> pipeline) => throw new NotImplementedException();

    /// <summary>Runs a pipeline each time the component raises an event, with the event's payload.</summary>
    /// <param name="raised">The event, as a member of the component's declaration.</param>
    /// <param name="pipeline">The steps to run on each raise, with what the event's listener receives.</param>
    /// <returns>The rendered component.</returns>
    public Rendered<TComponent, TModel> Reactive<TArgs>(Func<TComponent, Event<TArgs>> raised, Action<TArgs, Pipeline<TModel>> pipeline)
        where TArgs : JsObject, new()
        => throw new NotImplementedException();
}

/// <summary>
/// A plugin the page registers, reached from a view's plan to handle its events.
/// </summary>
/// <typeparam name="TPlugin">The plugin's declaration.</typeparam>
/// <typeparam name="TModel">The model of the view that reaches it.</typeparam>
public sealed class ReactivePlugin<TPlugin, TModel>
    where TPlugin : Plugin, new()
{
    internal ReactivePlugin()
    {
    }

    /// <summary>Runs a pipeline each time the plugin raises an event that has no payload.</summary>
    /// <param name="raised">The event, as a member of the plugin's declaration.</param>
    /// <param name="pipeline">The steps to run on each raise.</param>
    /// <returns>The plugin.</returns>
    public ReactivePlugin<TPlugin, TModel> Reactive(Func<TPlugin, Event> raised, Action<Pipeline<TModel>> pipeline) => throw new NotImplementedException();

    /// <summary>Runs a pipeline each time the plugin raises an event, with the event's payload.</summary>
    /// <param name="raised">The event, as a member of the plugin's declaration.</param>
    /// <param name="pipeline">The steps to run on each raise, with what the event's listener receives.</param>
    /// <returns>The plugin.</returns>
    public ReactivePlugin<TPlugin, TModel> Reactive<TArgs>(Func<TPlugin, Event<TArgs>> raised, Action<TArgs, Pipeline<TModel>> pipeline)
        where TArgs : JsObject, new()
        => throw new NotImplementedException();
}

/// <summary>
/// The rows of a grid-like list a view renders, on which a render helper declares its row actions.
/// </summary>
/// <typeparam name="TModel">The model of the view that renders the list.</typeparam>
/// <typeparam name="TRow">The type of a row.</typeparam>
public sealed class Rows<TModel, TRow>
{
    internal Rows()
    {
    }
}
