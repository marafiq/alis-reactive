using System;
using System.Collections.Generic;
using Alis.Reactive.Declarations;
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
        => Handles(DocumentPlugin.Ready, DocumentPlugin.NoPayload, Policy.Overlap, () => pipeline(new Pipeline<TModel>(_writer)));

    /// <summary>Runs a pipeline each time an app event that carries no payload is raised.</summary>
    /// <param name="appEvent">The event, as the app declares it.</param>
    /// <param name="pipeline">The steps to run on each raise.</param>
    /// <returns>The trigger, to handle another.</returns>
    public Trigger<TModel> CustomEvent(AppEvent appEvent, Action<Pipeline<TModel>> pipeline) => CustomEvent(appEvent, HandlerPolicy.Overlap, pipeline);

    /// <summary>Runs a pipeline each time an app event that carries no payload is raised, under a policy for a raise that comes while its last run is in flight.</summary>
    /// <param name="appEvent">The event, as the app declares it.</param>
    /// <param name="policy">What a raise does while the run the last raise started is in flight.</param>
    /// <param name="pipeline">The steps to run on each raise.</param>
    /// <returns>The trigger, to handle another.</returns>
    public Trigger<TModel> CustomEvent(AppEvent appEvent, HandlerPolicy policy, Action<Pipeline<TModel>> pipeline)
        => Handles(appEvent.Name, DocumentPlugin.NoPayload, policy.Grammar, () => pipeline(new Pipeline<TModel>(_writer)));

    /// <summary>Runs a pipeline each time an app event is raised, with the event's payload.</summary>
    /// <remarks>
    /// The payload reads the same whether a step dispatched it or markup raised it: a member markup carries as text
    /// reads as its number, boolean, enum or date.
    /// </remarks>
    /// <typeparam name="TPayload">The class of the payload.</typeparam>
    /// <param name="appEvent">The event, as the app declares it.</param>
    /// <param name="pipeline">The steps to run on each raise, with the payload the raise carries.</param>
    /// <returns>The trigger, to handle another.</returns>
    public Trigger<TModel> CustomEvent<TPayload>(AppEvent<TPayload> appEvent, Action<Data<TPayload>, Pipeline<TModel>> pipeline)
        => CustomEvent(appEvent, HandlerPolicy.Overlap, pipeline);

    /// <summary>Runs a pipeline each time an app event is raised, with the event's payload, under a policy for a raise that comes while its last run is in flight.</summary>
    /// <remarks>
    /// The payload reads the same whether a step dispatched it or markup raised it: a member markup carries as text
    /// reads as its number, boolean, enum or date.
    /// </remarks>
    /// <typeparam name="TPayload">The class of the payload.</typeparam>
    /// <param name="appEvent">The event, as the app declares it.</param>
    /// <param name="policy">What a raise does while the run the last raise started is in flight.</param>
    /// <param name="pipeline">The steps to run on each raise, with the payload the raise carries.</param>
    /// <returns>The trigger, to handle another.</returns>
    public Trigger<TModel> CustomEvent<TPayload>(AppEvent<TPayload> appEvent, HandlerPolicy policy, Action<Data<TPayload>, Pipeline<TModel>> pipeline)
        => Handles(appEvent.Name, DocumentPlugin.DetailOfTheEvent, policy.Grammar, () => pipeline(new Data<TPayload>(_writer), new Pipeline<TModel>(_writer)));

    private Trigger<TModel> Handles(string raised, IReadOnlyList<PathStep> payload, Policy policy, Action pipeline)
    {
        var document = DocumentPlugin.DeclaredIn(_writer.Content);
        var steps = _writer.Handler(pipeline);
        _writer.Content.Handles(new Handler(document, [], raised, payload, PathMembers.Required, policy, steps));
        return this;
    }
}
