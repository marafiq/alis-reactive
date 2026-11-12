using System;
using Alis.Reactive.Execution;
using Alis.Reactive.Schema;

namespace Alis.Reactive.TriggersAndPipelines;

/// <summary>
/// What starts a pipeline of a view's plan: a view names it in <c>Html.On(plan, trigger =&gt; ..)</c>.
/// </summary>
/// <typeparam name="TModel">The model of the view that writes the plan.</typeparam>
public sealed class Trigger<TModel>
{
    private readonly BlockWriter _writer;

    internal Trigger(BlockWriter writer)
    {
        _writer = writer;
    }

    /// <summary>Runs a pipeline once the page has started, or once the content a slot loaded has started.</summary>
    /// <remarks>
    /// It runs after every handler of that start listens, so an event the pipeline raises reaches each of them.
    /// </remarks>
    /// <param name="pipeline">The steps to run.</param>
    /// <returns>The trigger, to handle another.</returns>
    public Trigger<TModel> DomReady(Action<Pipeline<TModel>> pipeline)
    {
        var document = DocumentPlugin.DeclaredIn(_writer.Content);
        var steps = _writer.Block(() => pipeline(new Pipeline<TModel>(_writer)));
        _writer.Content.Handles(new Handler(document, [], DocumentPlugin.Ready, [], PathMembers.Required, Policy.Overlap, steps));
        return this;
    }
}
