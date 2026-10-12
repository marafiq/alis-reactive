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
            var reactionPipeline = new PipelineBuilder<TModel>(_context);
            pipeline(reactionPipeline);
            AddBehavior(StartsWhen.PageReady(), reactionPipeline);
            return this;
        }

        /// <summary>Runs a pipeline each time a declared document event is raised.</summary>
        /// <param name="appEvent">The event, declared once with <see cref="AppEvent.Named(string)"/>.</param>
        /// <param name="pipeline">Builds the reaction graph for each raise.</param>
        public TriggerBuilder<TModel> CustomEvent(AppEvent appEvent, Action<PipelineBuilder<TModel>> pipeline)
        {
            if (appEvent == null) throw new ArgumentNullException(nameof(appEvent));
            return On(StartsWhen.DocumentEvent(appEvent.Name), pipeline);
        }

        /// <summary>Runs a pipeline each time a declared document event is raised, reading its payload.</summary>
        /// <typeparam name="TPayload">The payload the event declares. A member is read by its camel-case JSON name (<c>MRN</c> reads <c>mrn</c>), the name <c>Dispatch</c> writes.</typeparam>
        /// <param name="appEvent">The event, declared once with <see cref="AppEvent.Named{TPayload}(string)"/>.</param>
        /// <param name="pipeline">Builds the reaction graph, given the payload and the pipeline.</param>
        public TriggerBuilder<TModel> CustomEvent<TPayload>(AppEvent<TPayload> appEvent,
            Action<TPayload, PipelineBuilder<TModel>> pipeline)
            where TPayload : class, new()
        {
            if (appEvent == null) throw new ArgumentNullException(nameof(appEvent));
            return On(StartsWhen.DocumentEvent(appEvent.Name), pipeline);
        }

        /// <summary>Runs a pipeline each time a declared document event is raised, without reading its payload.</summary>
        /// <typeparam name="TPayload">The payload the event declares.</typeparam>
        /// <param name="appEvent">The event, declared once with <see cref="AppEvent.Named{TPayload}(string)"/>.</param>
        /// <param name="pipeline">Builds the reaction graph for each raise.</param>
        public TriggerBuilder<TModel> CustomEvent<TPayload>(AppEvent<TPayload> appEvent,
            Action<PipelineBuilder<TModel>> pipeline)
            where TPayload : class, new()
        {
            if (appEvent == null) throw new ArgumentNullException(nameof(appEvent));
            return On(StartsWhen.DocumentEvent(appEvent.Name), pipeline);
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
            return On(StartsWhen.ServerPush(stream.Url, message(stream.Stream).Name), pipeline);
        }

        /// <summary>Runs a pipeline for each message of one kind a declared server-sent event stream sends, reading its data.</summary>
        /// <typeparam name="TStream">The stream's declaration.</typeparam>
        /// <typeparam name="TPayload">The message's data, read by camel-case JSON names, so the server writes it with that naming.</typeparam>
        /// <param name="stream">The stream at its URL: <c>ServerPushStream.At&lt;TStream&gt;(url)</c>.</param>
        /// <param name="message">Selects the declared message, for example <c>s =&gt; s.Alert</c>.</param>
        /// <param name="pipeline">Builds the reaction graph, given the message's data and the pipeline.</param>
        public TriggerBuilder<TModel> ServerPush<TStream, TPayload>(ServerPushSource<TStream> stream,
            Func<TStream, ServerPushMessage<TPayload>> message,
            Action<TPayload, PipelineBuilder<TModel>> pipeline)
            where TStream : ServerPushStream, new()
            where TPayload : class, new()
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (message == null) throw new ArgumentNullException(nameof(message));
            return On(StartsWhen.ServerPush(stream.Url, message(stream.Stream).Name), pipeline);
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
            return On(StartsWhen.SignalR(hub.Url, message(hub.Hub).Name), pipeline);
        }

        /// <summary>Runs a pipeline each time a declared hub's server calls one of its client methods, reading the argument.</summary>
        /// <typeparam name="THub">The hub's declaration.</typeparam>
        /// <typeparam name="TPayload">The method's argument, read by camel-case JSON names, the names SignalR's default protocol writes.</typeparam>
        /// <param name="hub">The hub at its URL: <c>SignalRHub.At&lt;THub&gt;(url)</c>.</param>
        /// <param name="message">Selects the declared method, for example <c>h =&gt; h.StatusChanged</c>.</param>
        /// <param name="pipeline">Builds the reaction graph, given the argument and the pipeline.</param>
        public TriggerBuilder<TModel> SignalR<THub, TPayload>(SignalRSource<THub> hub,
            Func<THub, SignalRMessage<TPayload>> message,
            Action<TPayload, PipelineBuilder<TModel>> pipeline)
            where THub : SignalRHub, new()
            where TPayload : class, new()
        {
            if (hub == null) throw new ArgumentNullException(nameof(hub));
            if (message == null) throw new ArgumentNullException(nameof(message));
            return On(StartsWhen.SignalR(hub.Url, message(hub.Hub).Name), pipeline);
        }

        private TriggerBuilder<TModel> On(StartsWhen trigger, Action<PipelineBuilder<TModel>> pipeline)
        {
            var reactionPipeline = new PipelineBuilder<TModel>(_context);
            pipeline(reactionPipeline);
            AddBehavior(trigger, reactionPipeline);
            return this;
        }

        private TriggerBuilder<TModel> On<TPayload>(StartsWhen trigger, Action<TPayload, PipelineBuilder<TModel>> pipeline)
            where TPayload : new()
        {
            var reactionPipeline = new PipelineBuilder<TModel>(_context);
            pipeline(new TPayload(), reactionPipeline);
            AddBehavior(trigger, reactionPipeline);
            return this;
        }

        private void AddBehavior(StartsWhen trigger, PipelineBuilder<TModel> reactionPipeline)
        {
            _context.AddBehavior(Behavior.On(trigger, reactionPipeline.BuildReaction()));
        }
    }
}
