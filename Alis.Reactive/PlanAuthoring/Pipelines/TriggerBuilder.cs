using System;
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

        /// <summary>Runs a pipeline for each message of one kind a declared server-sent event stream sends.</summary>
        /// <typeparam name="TStream">The stream's declaration.</typeparam>
        /// <param name="stream">The stream at its URL: <c>ServerPushStream.At&lt;TStream&gt;(url)</c>.</param>
        /// <param name="message">Selects the declared message, for example <c>s =&gt; s.Alert</c>.</param>
        /// <param name="pipeline">Builds the reaction graph for each message.</param>
        public TriggerBuilder<TModel> ServerPush<TStream>(ServerPushSource<TStream> stream,
            Func<TStream, ServerPushMessage> message,
            Action<PipelineBuilder<TModel>> pipeline)
            where TStream : ServerPushStream, new()
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (message == null) throw new ArgumentNullException(nameof(message));
            var url = stream.Url;
            return On(objects => objects.Stream(url), message(stream.Stream).Name, pipeline);
        }

        /// <summary>Runs a pipeline for each message of one kind a declared server-sent event stream sends, reading its data.</summary>
        /// <typeparam name="TStream">The stream's declaration.</typeparam>
        /// <typeparam name="TPayload">The message's data, read by camel-case JSON names, so the server writes it with that naming.</typeparam>
        /// <param name="stream">The stream at its URL: <c>ServerPushStream.At&lt;TStream&gt;(url)</c>.</param>
        /// <param name="message">Selects the declared message, for example <c>s =&gt; s.Alert</c>.</param>
        /// <param name="pipeline">Builds the reaction graph, given the message's data and the pipeline.</param>
        public TriggerBuilder<TModel> ServerPush<TStream, TPayload>(ServerPushSource<TStream> stream,
            Func<TStream, ServerPushMessage<TPayload>> message,
            Action<Data<TPayload>, PipelineBuilder<TModel>> pipeline)
            where TStream : ServerPushStream, new()
            where TPayload : class, new()
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (message == null) throw new ArgumentNullException(nameof(message));
            var url = stream.Url;
            return On(objects => objects.Stream(url), message(stream.Stream).Name, pipeline);
        }

        /// <summary>Runs a pipeline each time a declared hub's server calls one of its client methods.</summary>
        /// <typeparam name="THub">The hub's declaration.</typeparam>
        /// <param name="hub">The hub at its URL: <c>SignalRHub.At&lt;THub&gt;(url)</c>.</param>
        /// <param name="message">Selects the declared method, for example <c>h =&gt; h.StatusChanged</c>.</param>
        /// <param name="pipeline">Builds the reaction graph for each call.</param>
        public TriggerBuilder<TModel> SignalR<THub>(SignalRSource<THub> hub,
            Func<THub, SignalRMessage> message,
            Action<PipelineBuilder<TModel>> pipeline)
            where THub : SignalRHub, new()
        {
            if (hub == null) throw new ArgumentNullException(nameof(hub));
            if (message == null) throw new ArgumentNullException(nameof(message));
            var url = hub.Url;
            return On(objects => objects.Hub(url), message(hub.Hub).Name, pipeline);
        }

        /// <summary>Runs a pipeline each time a declared hub's server calls one of its client methods, reading the argument.</summary>
        /// <typeparam name="THub">The hub's declaration.</typeparam>
        /// <typeparam name="TPayload">The method's argument, read by camel-case JSON names, the names SignalR's default protocol writes.</typeparam>
        /// <param name="hub">The hub at its URL: <c>SignalRHub.At&lt;THub&gt;(url)</c>.</param>
        /// <param name="message">Selects the declared method, for example <c>h =&gt; h.StatusChanged</c>.</param>
        /// <param name="pipeline">Builds the reaction graph, given the argument and the pipeline.</param>
        public TriggerBuilder<TModel> SignalR<THub, TPayload>(SignalRSource<THub> hub,
            Func<THub, SignalRMessage<TPayload>> message,
            Action<Data<TPayload>, PipelineBuilder<TModel>> pipeline)
            where THub : SignalRHub, new()
            where TPayload : class, new()
        {
            if (hub == null) throw new ArgumentNullException(nameof(hub));
            if (message == null) throw new ArgumentNullException(nameof(message));
            var url = hub.Url;
            return On(objects => objects.Hub(url), message(hub.Hub).Name, pipeline);
        }

        private TriggerBuilder<TModel> On(Func<ObjectTable, string> on, string eventName, Action<PipelineBuilder<TModel>> pipeline)
        {
            _context.On(on, eventName, PipelineBuilder<TModel>.Build(_context, pipeline));
            return this;
        }

        private TriggerBuilder<TModel> On<TPayload>(Func<ObjectTable, string> on, string eventName, Action<Data<TPayload>, PipelineBuilder<TModel>> pipeline)
        {
            var payload = new Data<TPayload>(Receiver.Event);
            _context.On(on, eventName, PipelineBuilder<TModel>.Build(_context, p => pipeline(payload, p)));
            return this;
        }
    }
}
