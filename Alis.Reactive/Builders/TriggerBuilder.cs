using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Wires browser triggers (page load, custom events, server-sent events, and SignalR)
    /// to workflows that execute in the browser.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Accessed via <c>Html.On(plan, t =&gt; t.DomReady(...).CustomEvent(...))</c>.
    /// Triggers can be chained: each call adds an independent trigger-workflow pair to the plan.
    /// </para>
    /// <para>
    /// Avoid defining the same event name twice in the same view. Duplicate listeners
    /// are an antipattern unless there is a legitimate reason to split the workflow.
    /// </para>
    /// </remarks>
    /// <typeparam name="TModel">The view model type, providing compile-time expression paths.</typeparam>
    public sealed class TriggerBuilder<TModel> where TModel : class
    {
        private readonly ReactivePlan<TModel> _plan;

        /// <summary>
        /// NEVER make public. Constructed exclusively by <c>Html.On(plan, ...)</c>.
        /// Public constructors would let devs create orphaned builders not connected to a plan.
        /// </summary>
        internal TriggerBuilder(ReactivePlan<TModel> plan)
        {
            _plan = plan;
        }

        /// <summary>
        /// Wires a workflow that executes when the page finishes loading.
        /// </summary>
        /// <remarks>
        /// DomReady workflows run after all document-event listeners are wired, so
        /// <c>Dispatch("x")</c> inside a DomReady safely reaches any <c>CustomEvent("x", ...)</c>
        /// defined in the same plan.
        /// </remarks>
        /// <param name="pipeline">Builds the workflow actions (element mutations, dispatches, HTTP calls, etc.).</param>
        /// <returns>This builder for chaining additional triggers.</returns>
        public TriggerBuilder<TModel> DomReady(Action<PipelineBuilder<TModel>> pipeline)
        {
            var scope = _plan.Authoring.CreateDomReadyScope();
            var pb = new PipelineBuilder<TModel>(_plan.Authoring, scope);
            pipeline(pb);
            AddWorkflow(scope, pb);
            return this;
        }

        /// <summary>
        /// Wires a workflow that executes when the named custom event fires in the browser.
        /// </summary>
        /// <param name="eventName">The event name to listen for (e.g. <c>"order-submitted"</c>).</param>
        /// <param name="pipeline">Builds the workflow actions.</param>
        /// <returns>This builder for chaining additional triggers.</returns>
        public TriggerBuilder<TModel> CustomEvent(string eventName, Action<PipelineBuilder<TModel>> pipeline)
        {
            var scope = _plan.Authoring.CreateDocumentEventScope(eventName);
            var pb = new PipelineBuilder<TModel>(_plan.Authoring, scope);
            pipeline(pb);
            AddWorkflow(scope, pb);
            return this;
        }

        /// <summary>
        /// Wires a workflow with a typed payload that executes when the named custom event fires.
        /// </summary>
        /// <remarks>
        /// The <typeparamref name="TPayload"/> instance is used only for compile-time type inference;
        /// its property values are never read.
        /// </remarks>
        /// <typeparam name="TPayload">The event payload type, providing typed access to payload properties.</typeparam>
        /// <param name="eventName">The event name to listen for.</param>
        /// <param name="pipeline">Receives the typed payload instance and the pipeline builder.</param>
        /// <returns>This builder for chaining additional triggers.</returns>
        public TriggerBuilder<TModel> CustomEvent<TPayload>(string eventName,
            Action<TPayload, PipelineBuilder<TModel>> pipeline)
            where TPayload : new()
        {
            var scope = _plan.Authoring.CreateDocumentEventScope(eventName);
            var pb = new PipelineBuilder<TModel>(_plan.Authoring, scope);
            pipeline(new TPayload(), pb);
            AddWorkflow(scope, pb);
            return this;
        }

        /// <summary>
        /// Wires a workflow that fires when a Server-Sent Events stream sends any message.
        /// </summary>
        /// <param name="url">The SSE endpoint URL.</param>
        /// <param name="pipeline">Builds the workflow actions.</param>
        /// <returns>This builder for chaining additional triggers.</returns>
        public TriggerBuilder<TModel> ServerPush(string url, Action<PipelineBuilder<TModel>> pipeline)
        {
            var scope = _plan.Authoring.CreateServerPushScope(url, null);
            var pb = new PipelineBuilder<TModel>(_plan.Authoring, scope);
            pipeline(pb);
            AddWorkflow(scope, pb);
            return this;
        }

        /// <summary>
        /// Wires a workflow that fires when the SSE stream sends a specific named event type.
        /// </summary>
        /// <param name="url">The SSE endpoint URL.</param>
        /// <param name="eventType">The SSE event type to filter for.</param>
        /// <param name="pipeline">Builds the workflow actions.</param>
        /// <returns>This builder for chaining additional triggers.</returns>
        public TriggerBuilder<TModel> ServerPush(string url, string eventType, Action<PipelineBuilder<TModel>> pipeline)
        {
            var scope = _plan.Authoring.CreateServerPushScope(url, eventType);
            var pb = new PipelineBuilder<TModel>(_plan.Authoring, scope);
            pipeline(pb);
            AddWorkflow(scope, pb);
            return this;
        }

        /// <summary>
        /// Wires a workflow with a typed payload when the SSE stream sends a named event type.
        /// </summary>
        /// <typeparam name="TPayload">The event payload type, providing typed access to payload properties.</typeparam>
        /// <param name="url">The SSE endpoint URL.</param>
        /// <param name="eventType">The SSE event type to filter for.</param>
        /// <param name="pipeline">Receives the typed payload instance and the pipeline builder.</param>
        /// <returns>This builder for chaining additional triggers.</returns>
        public TriggerBuilder<TModel> ServerPush<TPayload>(string url, string eventType,
            Action<TPayload, PipelineBuilder<TModel>> pipeline)
            where TPayload : new()
        {
            var scope = _plan.Authoring.CreateServerPushScope(url, eventType);
            var pb = new PipelineBuilder<TModel>(_plan.Authoring, scope);
            pipeline(new TPayload(), pb);
            AddWorkflow(scope, pb);
            return this;
        }

        /// <summary>
        /// Wires a workflow that fires when the server invokes the named SignalR Hub method.
        /// </summary>
        /// <param name="hubUrl">The SignalR hub endpoint URL.</param>
        /// <param name="methodName">The hub method name to listen for.</param>
        /// <param name="pipeline">Builds the workflow actions.</param>
        /// <returns>This builder for chaining additional triggers.</returns>
        public TriggerBuilder<TModel> SignalR(string hubUrl, string methodName,
            Action<PipelineBuilder<TModel>> pipeline)
        {
            var scope = _plan.Authoring.CreateSignalRScope(hubUrl, methodName);
            var pb = new PipelineBuilder<TModel>(_plan.Authoring, scope);
            pipeline(pb);
            AddWorkflow(scope, pb);
            return this;
        }

        /// <summary>
        /// Wires a workflow with a typed payload when the server invokes the named SignalR Hub method.
        /// </summary>
        /// <typeparam name="TPayload">The event payload type, providing typed access to payload properties.</typeparam>
        /// <param name="hubUrl">The SignalR hub endpoint URL.</param>
        /// <param name="methodName">The hub method name to listen for.</param>
        /// <param name="pipeline">Receives the typed payload instance and the pipeline builder.</param>
        /// <returns>This builder for chaining additional triggers.</returns>
        public TriggerBuilder<TModel> SignalR<TPayload>(string hubUrl, string methodName,
            Action<TPayload, PipelineBuilder<TModel>> pipeline)
            where TPayload : new()
        {
            var scope = _plan.Authoring.CreateSignalRScope(hubUrl, methodName);
            var pb = new PipelineBuilder<TModel>(_plan.Authoring, scope);
            pipeline(new TPayload(), pb);
            AddWorkflow(scope, pb);
            return this;
        }

        private void AddWorkflow(WorkflowScope scope, PipelineBuilder<TModel> pipeline)
        {
            foreach (var action in pipeline.BuildActions())
                _plan.AddWorkflow(scope, action);
        }
    }
}
