using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Wires browser triggers to reactive workflows.
    /// </summary>
    /// <remarks>
    /// Accessed via <c>Html.On(plan, t =&gt; t.DomReady(...).CustomEvent(...))</c>.
    /// Triggers can be chained: each call adds an independent workflow to the plan.
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    public sealed class TriggerBuilder<TModel> where TModel : class
    {
        private readonly PlanBuildContext _context;

        internal TriggerBuilder(ReactivePlan<TModel> plan, PlanBuildContext context)
        {
            _context = context;
        }

        /// <summary>Registers a workflow that fires when the page loads.</summary>
        /// <param name="pipeline">Builds the commands to execute on page load.</param>
        /// <returns>This builder for chaining additional triggers.</returns>
        public TriggerBuilder<TModel> DomReady(Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            AddBehaviors(StartsWhen.PageReady(), pb);
            return this;
        }

        /// <summary>Registers a workflow that fires when a named custom event is dispatched.</summary>
        /// <param name="eventName">The event name to listen for, matching a <c>p.Dispatch("name")</c> call.</param>
        /// <param name="pipeline">Builds the commands to execute when the event fires.</param>
        /// <returns>This builder for chaining additional triggers.</returns>
        public TriggerBuilder<TModel> CustomEvent(string eventName, Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            AddBehaviors(StartsWhen.DocumentEvent(eventName), pb);
            return this;
        }

        /// <summary>Registers a workflow that fires when a named custom event is dispatched, with a typed payload.</summary>
        /// <typeparam name="TPayload">The event payload type.</typeparam>
        /// <param name="eventName">The event name to listen for.</param>
        /// <param name="pipeline">Builds the commands. The payload provides typed access to event properties.</param>
        /// <returns>This builder for chaining additional triggers.</returns>
        public TriggerBuilder<TModel> CustomEvent<TPayload>(string eventName,
            Action<TPayload, PipelineBuilder<TModel>> pipeline)
            where TPayload : new()
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(new TPayload(), pb);
            AddBehaviors(StartsWhen.DocumentEvent(eventName), pb);
            return this;
        }

        /// <summary>Registers a workflow that fires when the server sends an event via Server-Sent Events.</summary>
        /// <param name="url">The SSE endpoint URL.</param>
        /// <param name="pipeline">Builds the commands to execute on each server event.</param>
        /// <returns>This builder for chaining additional triggers.</returns>
        public TriggerBuilder<TModel> ServerPush(string url, Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            AddBehaviors(StartsWhen.ServerPush(url), pb);
            return this;
        }

        /// <summary>Registers a workflow that fires on a specific SSE event type.</summary>
        /// <param name="url">The SSE endpoint URL.</param>
        /// <param name="eventType">The SSE event type to filter on.</param>
        /// <param name="pipeline">Builds the commands to execute.</param>
        /// <returns>This builder for chaining additional triggers.</returns>
        public TriggerBuilder<TModel> ServerPush(string url, string eventType, Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            AddBehaviors(StartsWhen.ServerPush(url, eventType), pb);
            return this;
        }

        /// <summary>Registers a workflow for a specific SSE event type with a typed payload.</summary>
        /// <typeparam name="TPayload">The event payload type.</typeparam>
        /// <param name="url">The SSE endpoint URL.</param>
        /// <param name="eventType">The SSE event type to filter on.</param>
        /// <param name="pipeline">Builds the commands. The payload provides typed access to event properties.</param>
        /// <returns>This builder for chaining additional triggers.</returns>
        public TriggerBuilder<TModel> ServerPush<TPayload>(string url, string eventType,
            Action<TPayload, PipelineBuilder<TModel>> pipeline)
            where TPayload : new()
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(new TPayload(), pb);
            AddBehaviors(StartsWhen.ServerPush(url, eventType), pb);
            return this;
        }

        /// <summary>Registers a workflow that fires when a SignalR hub method is called.</summary>
        /// <param name="hubUrl">The SignalR hub URL.</param>
        /// <param name="methodName">The hub method name to listen for.</param>
        /// <param name="pipeline">Builds the commands to execute.</param>
        /// <returns>This builder for chaining additional triggers.</returns>
        public TriggerBuilder<TModel> SignalR(string hubUrl, string methodName,
            Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            AddBehaviors(StartsWhen.SignalR(hubUrl, methodName), pb);
            return this;
        }

        /// <summary>Registers a workflow for a SignalR hub method with a typed payload.</summary>
        /// <typeparam name="TPayload">The hub method payload type.</typeparam>
        /// <param name="hubUrl">The SignalR hub URL.</param>
        /// <param name="methodName">The hub method name to listen for.</param>
        /// <param name="pipeline">Builds the commands. The payload provides typed access to event properties.</param>
        /// <returns>This builder for chaining additional triggers.</returns>
        public TriggerBuilder<TModel> SignalR<TPayload>(string hubUrl, string methodName,
            Action<TPayload, PipelineBuilder<TModel>> pipeline)
            where TPayload : new()
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(new TPayload(), pb);
            AddBehaviors(StartsWhen.SignalR(hubUrl, methodName), pb);
            return this;
        }

        private void AddBehaviors(StartsWhen trigger, PipelineBuilder<TModel> pb)
        {
            foreach (var reaction in pb.BuildReactions())
                _context.AddBehavior(Behavior.On(trigger, reaction));
        }
    }
}
