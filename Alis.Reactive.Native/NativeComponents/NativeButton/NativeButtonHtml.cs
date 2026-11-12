using System;
using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.TriggersAndPipelines;

namespace Alis.Reactive.Native.NativeComponents;

/// <summary>
/// A button a view renders: what the view sets on it, and the door that handles its events.
/// </summary>
/// <remarks>The builder writes the button's markup where the view puts it.</remarks>
/// <typeparam name="TModel">The model of the view that renders the button.</typeparam>
public sealed partial class NativeButtonBuilder<TModel>
{
    internal NativeButtonBuilder()
    {
    }

    /// <summary>Sets the CSS classes of the button's element.</summary>
    /// <param name="names">The class names, separated by spaces.</param>
    /// <returns>The builder.</returns>
    public NativeButtonBuilder<TModel> CssClass(string names) => throw new NotImplementedException();

    /// <summary>Runs a pipeline each time the button raises an event, with the event's payload, from the plan's start.</summary>
    /// <param name="plan">The view's plan.</param>
    /// <param name="raised">The event, as a member of the button's declaration.</param>
    /// <param name="pipeline">The steps to run on each raise, with what the event's listener receives.</param>
    /// <returns>The builder.</returns>
    public NativeButtonBuilder<TModel> Reactive<TArgs>(ReactivePlan<TModel> plan, Func<NativeButton, Event<TArgs>> raised, Action<TArgs, Pipeline<TModel>> pipeline)
        where TArgs : JsObject, new()
        => throw new NotImplementedException();
}
