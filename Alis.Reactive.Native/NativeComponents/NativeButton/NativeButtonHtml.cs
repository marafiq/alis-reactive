using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    private readonly string _id;
    private readonly string _text;
    private readonly Dictionary<ReactivePlan<TModel>, Rendered<NativeButton, TModel>> _renderedIn = new();
    private bool _hasCssClass;
    private string _cssClass = "";

    internal NativeButtonBuilder(string id, string text)
    {
        _id = id;
        _text = text;
    }

    /// <summary>Sets the CSS classes of the button's element.</summary>
    /// <param name="names">The class names, separated by spaces.</param>
    /// <returns>The builder.</returns>
    public NativeButtonBuilder<TModel> CssClass(string names)
    {
        _hasCssClass = true;
        _cssClass = names;
        return this;
    }

    /// <summary>Runs a pipeline each time the button raises an event, with the event's payload, from the plan's start.</summary>
    /// <param name="plan">The view's plan.</param>
    /// <param name="raised">The event, as a member of the button's declaration.</param>
    /// <param name="pipeline">The steps to run on each raise, with what the event's listener receives.</param>
    /// <param name="callerFile">The file of the call, which the compiler supplies.</param>
    /// <param name="callerLine">The line of the call, which the compiler supplies.</param>
    /// <returns>The builder.</returns>
    public NativeButtonBuilder<TModel> Reactive<TArgs>(ReactivePlan<TModel> plan, Func<NativeButton, Event<TArgs>> raised, Action<TArgs, Pipeline<TModel>> pipeline, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        where TArgs : JsObject, new()
        => Reactive(plan, raised, HandlerPolicy.Overlap, pipeline, callerFile, callerLine);

    /// <summary>Runs a pipeline each time the button raises an event, with the event's payload, from the plan's start, under a policy for a raise that comes while its last run is in flight.</summary>
    /// <param name="plan">The view's plan.</param>
    /// <param name="raised">The event, as a member of the button's declaration.</param>
    /// <param name="policy">What a raise does while the run the last raise started is in flight.</param>
    /// <param name="pipeline">The steps to run on each raise, with what the event's listener receives.</param>
    /// <param name="callerFile">The file of the call, which the compiler supplies.</param>
    /// <param name="callerLine">The line of the call, which the compiler supplies.</param>
    /// <returns>The builder.</returns>
    public NativeButtonBuilder<TModel> Reactive<TArgs>(ReactivePlan<TModel> plan, Func<NativeButton, Event<TArgs>> raised, HandlerPolicy policy, Action<TArgs, Pipeline<TModel>> pipeline, [CallerFilePath] string callerFile = "", [CallerLineNumber] int callerLine = 0)
        where TArgs : JsObject, new()
    {
        RenderedIn(plan).Reactive(raised, policy, pipeline, callerFile, callerLine);
        return this;
    }

    private Rendered<NativeButton, TModel> RenderedIn(ReactivePlan<TModel> plan)
    {
        if (_renderedIn.TryGetValue(plan, out var button)) return button;
        var rendered = plan.Render<NativeButton>(_id, this);
        _renderedIn.Add(plan, rendered);
        return rendered;
    }
}
