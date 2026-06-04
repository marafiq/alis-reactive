using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Declares trigger-to-reaction behaviors on a Reactive Plan.
    /// </summary>
    /// <remarks>
    /// Accessed via <c>Html.On(plan, t =&gt; t.DomReady(...).CustomEvent(...))</c>.
    /// Each trigger call appends an independent behavior; chaining does not combine
    /// multiple triggers into one reaction.
    /// </remarks>
    /// <typeparam name="TModel">The view model that owns model-bound component IDs.</typeparam>
    public sealed class TriggerBuilder<TModel> where TModel : class
    {
        private readonly PlanBuildContext _context;

        internal TriggerBuilder(ReactivePlan<TModel> plan, PlanBuildContext context)
        {
            _context = context;
        }

        /// <summary>Runs a reaction pipeline once when the page is ready for Reactive Plan boot.</summary>
        /// <param name="pipeline">Builds the reaction graph for the page-ready trigger.</param>
        public TriggerBuilder<TModel> DomReady(Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            AddBehaviors(StartsWhen.PageReady(), pb);
            return this;
        }

        /// <summary>Listens for a named document <c>CustomEvent</c> without a typed payload contract.</summary>
        /// <param name="eventName">The event name, matching <c>p.Dispatch("name")</c> or host-page dispatch.</param>
        /// <param name="pipeline">Builds the reaction graph for each matching event.</param>
        public TriggerBuilder<TModel> CustomEvent(string eventName, Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            AddBehaviors(StartsWhen.DocumentEvent(eventName), pb);
            return this;
        }

        /// <summary>Listens for a named document <c>CustomEvent</c> with a typed payload authoring scope.</summary>
        /// <typeparam name="TPayload">The event-detail contract used to author payload path reads.</typeparam>
        /// <param name="eventName">The event name, matching <c>p.Dispatch("name")</c> or host-page dispatch.</param>
        /// <param name="pipeline">Builds the reaction graph using the typed event payload scope.</param>
        public TriggerBuilder<TModel> CustomEvent<TPayload>(string eventName,
            Action<TPayload, PipelineBuilder<TModel>> pipeline)
            where TPayload : new()
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(new TPayload(), pb);
            AddBehaviors(
                StartsWhen.DocumentEvent(eventName, PayloadContract.ForPayload(typeof(TPayload))),
                pb);
            return this;
        }

        /// <summary>Subscribes to an EventSource endpoint and runs a pipeline for every SSE message.</summary>
        /// <param name="url">The SSE endpoint URL.</param>
        /// <param name="pipeline">Builds the reaction graph for each server-sent message.</param>
        public TriggerBuilder<TModel> ServerPush(string url, Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            AddBehaviors(StartsWhen.ServerPush(url), pb);
            return this;
        }

        /// <summary>Subscribes to an EventSource endpoint and filters by SSE event type.</summary>
        /// <param name="url">The SSE endpoint URL.</param>
        /// <param name="eventType">The SSE event type that must match before the pipeline runs.</param>
        /// <param name="pipeline">Builds the reaction graph for each matching server-sent event.</param>
        public TriggerBuilder<TModel> ServerPush(string url, string eventType, Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            AddBehaviors(StartsWhen.ServerPush(url, eventType), pb);
            return this;
        }

        /// <summary>Subscribes to a typed SSE event and exposes the event data as a payload scope.</summary>
        /// <typeparam name="TPayload">The SSE data contract used to author payload path reads.</typeparam>
        /// <param name="url">The SSE endpoint URL.</param>
        /// <param name="eventType">The SSE event type that must match before the pipeline runs.</param>
        /// <param name="pipeline">Builds the reaction graph using the typed SSE payload scope.</param>
        public TriggerBuilder<TModel> ServerPush<TPayload>(string url, string eventType,
            Action<TPayload, PipelineBuilder<TModel>> pipeline)
            where TPayload : new()
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(new TPayload(), pb);
            AddBehaviors(
                StartsWhen.ServerPush(url, eventType, PayloadContract.ForPayload(typeof(TPayload))),
                pb);
            return this;
        }

        /// <summary>Subscribes to a SignalR hub method without a typed payload contract.</summary>
        /// <param name="hubUrl">The SignalR hub URL.</param>
        /// <param name="methodName">The hub method name to listen for.</param>
        /// <param name="pipeline">Builds the reaction graph for each hub method invocation.</param>
        public TriggerBuilder<TModel> SignalR(string hubUrl, string methodName,
            Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            AddBehaviors(StartsWhen.SignalR(hubUrl, methodName), pb);
            return this;
        }

        /// <summary>Subscribes to a SignalR hub method with a typed payload authoring scope.</summary>
        /// <typeparam name="TPayload">The hub method payload contract used to author payload path reads.</typeparam>
        /// <param name="hubUrl">The SignalR hub URL.</param>
        /// <param name="methodName">The hub method name to listen for.</param>
        /// <param name="pipeline">Builds the reaction graph using the typed hub method payload scope.</param>
        public TriggerBuilder<TModel> SignalR<TPayload>(string hubUrl, string methodName,
            Action<TPayload, PipelineBuilder<TModel>> pipeline)
            where TPayload : new()
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(new TPayload(), pb);
            AddBehaviors(
                StartsWhen.SignalR(hubUrl, methodName, PayloadContract.ForPayload(typeof(TPayload))),
                pb);
            return this;
        }

        private void AddBehaviors(StartsWhen trigger, PipelineBuilder<TModel> pb)
        {
            _context.AddBehavior(Behavior.On(trigger, pb.BuildReaction()));
        }
    }
}
