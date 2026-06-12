using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Authors trigger-to-reaction entries for a Reactive Plan.
    /// </summary>
    /// <remarks>
    /// Accessed via <c>Html.On(plan, t =&gt; t.DomReady(...).CustomEvent(...))</c>.
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

        /// <summary>Adds a document <c>CustomEvent</c> trigger without a typed payload contract.</summary>
        /// <param name="eventName">Event name, matching <c>p.Dispatch("name")</c> or host-page dispatch.</param>
        /// <param name="pipeline">Builds the reaction graph for each matching event.</param>
        public TriggerBuilder<TModel> CustomEvent(string eventName, Action<PipelineBuilder<TModel>> pipeline)
        {
            var reactionPipeline = new PipelineBuilder<TModel>(_context);
            pipeline(reactionPipeline);
            AddBehavior(StartsWhen.DocumentEvent(eventName), reactionPipeline);
            return this;
        }

        /// <summary>Adds a document <c>CustomEvent</c> trigger with a typed payload authoring scope.</summary>
        /// <typeparam name="TPayload">Event-detail contract used to author payload path reads.</typeparam>
        /// <param name="eventName">Event name, matching <c>p.Dispatch("name")</c> or host-page dispatch.</param>
        /// <param name="pipeline">Builds the reaction graph using the typed event payload scope.</param>
        public TriggerBuilder<TModel> CustomEvent<TPayload>(string eventName,
            Action<TPayload, PipelineBuilder<TModel>> pipeline)
            where TPayload : new()
        {
            var reactionPipeline = new PipelineBuilder<TModel>(_context);
            pipeline(new TPayload(), reactionPipeline);
            AddBehavior(StartsWhen.DocumentEvent(eventName), reactionPipeline);
            return this;
        }

        /// <summary>Adds an EventSource trigger that runs a pipeline for every SSE message.</summary>
        /// <param name="url">SSE endpoint URL.</param>
        /// <param name="pipeline">Builds the reaction graph for each server-sent message.</param>
        public TriggerBuilder<TModel> ServerPush(string url, Action<PipelineBuilder<TModel>> pipeline)
        {
            var reactionPipeline = new PipelineBuilder<TModel>(_context);
            pipeline(reactionPipeline);
            AddBehavior(StartsWhen.ServerPush(url), reactionPipeline);
            return this;
        }

        /// <summary>Adds an EventSource trigger filtered by SSE event type.</summary>
        /// <param name="url">SSE endpoint URL.</param>
        /// <param name="eventType">SSE event type that must match before the pipeline runs.</param>
        /// <param name="pipeline">Builds the reaction graph for each matching server-sent event.</param>
        public TriggerBuilder<TModel> ServerPush(string url, string eventType, Action<PipelineBuilder<TModel>> pipeline)
        {
            var reactionPipeline = new PipelineBuilder<TModel>(_context);
            pipeline(reactionPipeline);
            AddBehavior(StartsWhen.ServerPush(url, eventType), reactionPipeline);
            return this;
        }

        /// <summary>Adds a typed SSE trigger and exposes the event data as a payload scope.</summary>
        /// <typeparam name="TPayload">SSE data contract used to author payload path reads.</typeparam>
        /// <param name="url">SSE endpoint URL.</param>
        /// <param name="eventType">SSE event type that must match before the pipeline runs.</param>
        /// <param name="pipeline">Builds the reaction graph using the typed SSE payload scope.</param>
        public TriggerBuilder<TModel> ServerPush<TPayload>(string url, string eventType,
            Action<TPayload, PipelineBuilder<TModel>> pipeline)
            where TPayload : new()
        {
            var reactionPipeline = new PipelineBuilder<TModel>(_context);
            pipeline(new TPayload(), reactionPipeline);
            AddBehavior(StartsWhen.ServerPush(url, eventType), reactionPipeline);
            return this;
        }

        /// <summary>Adds a SignalR hub-method trigger without a typed payload contract.</summary>
        /// <param name="hubUrl">SignalR hub URL.</param>
        /// <param name="methodName">Hub method name to listen for.</param>
        /// <param name="pipeline">Builds the reaction graph for each hub method invocation.</param>
        public TriggerBuilder<TModel> SignalR(string hubUrl, string methodName,
            Action<PipelineBuilder<TModel>> pipeline)
        {
            var reactionPipeline = new PipelineBuilder<TModel>(_context);
            pipeline(reactionPipeline);
            AddBehavior(StartsWhen.SignalR(hubUrl, methodName), reactionPipeline);
            return this;
        }

        /// <summary>Adds a SignalR hub-method trigger with a typed payload authoring scope.</summary>
        /// <typeparam name="TPayload">Hub method payload contract used to author payload path reads.</typeparam>
        /// <param name="hubUrl">SignalR hub URL.</param>
        /// <param name="methodName">Hub method name to listen for.</param>
        /// <param name="pipeline">Builds the reaction graph using the typed hub method payload scope.</param>
        public TriggerBuilder<TModel> SignalR<TPayload>(string hubUrl, string methodName,
            Action<TPayload, PipelineBuilder<TModel>> pipeline)
            where TPayload : new()
        {
            var reactionPipeline = new PipelineBuilder<TModel>(_context);
            pipeline(new TPayload(), reactionPipeline);
            AddBehavior(StartsWhen.SignalR(hubUrl, methodName), reactionPipeline);
            return this;
        }

        private void AddBehavior(StartsWhen trigger, PipelineBuilder<TModel> reactionPipeline)
        {
            _context.AddBehavior(Behavior.On(trigger, reactionPipeline.BuildReaction()));
        }
    }
}
