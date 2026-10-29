using System;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Authors trigger-to-reaction entries for a Reactive Plan.
    /// </summary>
    /// <remarks>
    /// Accessed via <c>Html.On(plan, t =&gt; t.DomReady(...).CustomEvent(CareEvents.Flagged, ...))</c>.
    /// Each trigger method appends an independent <c>Behavior</c> entry. Chaining
    /// does not combine multiple triggers into one reaction. The generated runtime
    /// wires DOM, EventSource, or SignalR listeners only when the plan boots.
    /// </remarks>
    /// <typeparam name="TModel">View model that owns model-bound component IDs.</typeparam>
    public sealed class TriggerBuilder<TModel> where TModel : class
    {
        private readonly PlanBuildContext _context;

        internal TriggerBuilder(ReactivePlan<TModel> plan, PlanBuildContext context)
        {
            _context = context;
        }

        /// <summary>Runs a reaction pipeline after the DOM is ready and Reactive Plan listeners are wired.</summary>
        /// <param name="pipeline">Builds the reaction graph for the <c>DomReady</c> trigger.</param>
        public TriggerBuilder<TModel> DomReady(Action<PipelineBuilder<TModel>> pipeline)
        {
            return On(objects => objects.Document(), "ready", pipeline);
        }

        /// <summary>Runs a pipeline each time a declared document event is raised.</summary>
        /// <param name="appEvent">The event, declared once with <see cref="AppEvent.Declare(string, string)"/>.</param>
        /// <param name="pipeline">Builds the reaction graph for each raise.</param>
        public TriggerBuilder<TModel> CustomEvent(AppEvent appEvent, Action<PipelineBuilder<TModel>> pipeline)
        {
            if (appEvent == null) throw new ArgumentNullException(nameof(appEvent));
            return On(objects => objects.Document(), appEvent.Name, pipeline);
        }

        /// <summary>Runs a pipeline each time a declared document event is raised, reading its payload.</summary>
        /// <typeparam name="TPayload">The payload the event declares. A member is read by its camel-case JSON name (<c>MRN</c> reads <c>mrn</c>), the name <c>Dispatch</c> writes.</typeparam>
        /// <param name="appEvent">The event, declared once with <see cref="AppEvent.Declare{TPayload}(string, string)"/>.</param>
        /// <param name="pipeline">Builds the reaction graph, given the payload (<c>payload.Get(x =&gt; x.UnitId)</c>) and the pipeline.</param>
        public TriggerBuilder<TModel> CustomEvent<TPayload>(AppEvent<TPayload> appEvent,
            Action<Data<TPayload>, PipelineBuilder<TModel>> pipeline)
            where TPayload : class, new()
        {
            if (appEvent == null) throw new ArgumentNullException(nameof(appEvent));
            return On(objects => objects.Document(), appEvent.Name, pipeline);
        }

        /// <summary>Runs a pipeline each time a declared document event is raised, without reading its payload.</summary>
        /// <typeparam name="TPayload">The payload the event declares.</typeparam>
        /// <param name="appEvent">The event, declared once with <see cref="AppEvent.Declare{TPayload}(string, string)"/>.</param>
        /// <param name="pipeline">Builds the reaction graph for each raise.</param>
        public TriggerBuilder<TModel> CustomEvent<TPayload>(AppEvent<TPayload> appEvent,
            Action<PipelineBuilder<TModel>> pipeline)
            where TPayload : class, new()
        {
            if (appEvent == null) throw new ArgumentNullException(nameof(appEvent));
            return On(objects => objects.Document(), appEvent.Name, pipeline);
        }

        /// <summary>Runs a pipeline each time a declared server-sent event stream sends an event, reading its data.</summary>
        /// <typeparam name="TStream">The stream's declaration.</typeparam>
        /// <typeparam name="TArgs">The declaration of the event's data, whose members are read by their camel-case JSON names.</typeparam>
        /// <param name="stream">The stream at its URL: <c>ServerPushStream.At&lt;TStream&gt;(url)</c>.</param>
        /// <param name="eventSelector">Selects the declared event, for example <c>s =&gt; s.Alert</c>.</param>
        /// <param name="pipeline">Builds the reaction graph, given the event's data, live, and the pipeline.</param>
        public TriggerBuilder<TModel> ServerPush<TStream, TArgs>(ServerPushSource<TStream> stream,
            Func<TStream, Event<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TStream : ServerPushStream, new()
            where TArgs : JsObject
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (eventSelector == null) throw new ArgumentNullException(nameof(eventSelector));
            var url = stream.Url;
            return On(objects => objects.Stream(url), eventSelector(stream.Stream), pipeline);
        }

        /// <summary>Runs a pipeline each time a declared server-sent event stream sends an event that carries no data the page reads.</summary>
        /// <typeparam name="TStream">The stream's declaration.</typeparam>
        /// <param name="stream">The stream at its URL: <c>ServerPushStream.At&lt;TStream&gt;(url)</c>.</param>
        /// <param name="eventSelector">Selects the declared event, for example <c>s =&gt; s.AllClear</c>.</param>
        /// <param name="pipeline">Builds the reaction graph for each event.</param>
        public TriggerBuilder<TModel> ServerPush<TStream>(ServerPushSource<TStream> stream,
            Func<TStream, Event> eventSelector,
            Action<PipelineBuilder<TModel>> pipeline)
            where TStream : ServerPushStream, new()
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
            return ServerPush<TStream, NoPayload>(stream, eventSelector, (_, p) => pipeline(p));
        }

        /// <summary>Runs a pipeline each time a declared hub's server calls one of its client methods, reading the argument, or each time the connection reconnects.</summary>
        /// <typeparam name="THub">The hub's declaration.</typeparam>
        /// <typeparam name="TArgs">The declaration of the argument, whose members are read by their camel-case JSON names.</typeparam>
        /// <param name="hub">The hub at its URL: <c>SignalRHub.At&lt;THub&gt;(url)</c>.</param>
        /// <param name="eventSelector">Selects the declared event, for example <c>h =&gt; h.StatusChanged</c> or <c>h =&gt; h.Reconnected</c>.</param>
        /// <param name="pipeline">Builds the reaction graph, given the argument, live, and the pipeline.</param>
        public TriggerBuilder<TModel> SignalR<THub, TArgs>(SignalRSource<THub> hub,
            Func<THub, Event<TArgs>> eventSelector,
            Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where THub : SignalRHub, new()
            where TArgs : JsObject
        {
            if (hub == null) throw new ArgumentNullException(nameof(hub));
            if (eventSelector == null) throw new ArgumentNullException(nameof(eventSelector));
            var url = hub.Url;
            return On(objects => objects.Hub(url), eventSelector(hub.Hub), pipeline);
        }

        /// <summary>Runs a pipeline each time a declared hub's server calls one of its client methods with no argument, or each time the connection drops or closes.</summary>
        /// <typeparam name="THub">The hub's declaration.</typeparam>
        /// <param name="hub">The hub at its URL: <c>SignalRHub.At&lt;THub&gt;(url)</c>.</param>
        /// <param name="eventSelector">Selects the declared event, for example <c>h =&gt; h.BoardCleared</c> or <c>h =&gt; h.Reconnecting</c>.</param>
        /// <param name="pipeline">Builds the reaction graph for each call.</param>
        public TriggerBuilder<TModel> SignalR<THub>(SignalRSource<THub> hub,
            Func<THub, Event> eventSelector,
            Action<PipelineBuilder<TModel>> pipeline)
            where THub : SignalRHub, new()
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
            return SignalR<THub, NoPayload>(hub, eventSelector, (_, p) => pipeline(p));
        }

        private TriggerBuilder<TModel> On(Func<PlanObjects, string> on, string eventName, Action<PipelineBuilder<TModel>> pipeline)
        {
            _context.On(on, G.NoPath, eventName, PipelineBuilder<TModel>.Build(_context, pipeline));
            return this;
        }

        private TriggerBuilder<TModel> On<TPayload>(Func<PlanObjects, string> on, string eventName, Action<Data<TPayload>, PipelineBuilder<TModel>> pipeline)
        {
            var payload = new Data<TPayload>(Target.Event);
            _context.On(on, G.NoPath, eventName, PipelineBuilder<TModel>.Build(_context, p => pipeline(payload, p)));
            return this;
        }

        // A declared event of a stream or a hub: its payload is live inside the callback, as a component's is.
        private TriggerBuilder<TModel> On<TArgs>(Func<PlanObjects, string> on, Event<TArgs> declaredEvent, Action<TArgs, PipelineBuilder<TModel>> pipeline)
            where TArgs : JsObject
        {
            if (declaredEvent == null) throw new ArgumentException("Select one of the declared events, for example s => s.Alert.", nameof(declaredEvent));
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
            if (declaredEvent.ObjectPath.Count > 0)
                throw new ArgumentException(
                    $"'{declaredEvent.Name}' is an event of '{string.Join(".", declaredEvent.ObjectPath)}', an object the stream or hub " +
                    "holds. A stream or a hub raises its own events only: declare the event on the stream or the hub itself.",
                    nameof(declaredEvent));
            var args = declaredEvent.Args;
            args.BindTo(new EventPayloadBinding(_context));
            _context.On(on, G.NoPath, declaredEvent.Name, PipelineBuilder<TModel>.Build(_context, p => pipeline(args, p)));
            return this;
        }
    }
}
