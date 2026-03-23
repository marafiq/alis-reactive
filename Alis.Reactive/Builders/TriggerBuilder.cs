using System;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Builds trigger–reaction entries and adds them to the plan.
    /// Entry point for wiring triggers via <c>Html.On()</c>.
    /// </summary>
    public sealed class TriggerBuilder<TModel> where TModel : class
    {
        private readonly IReactivePlan<TModel> _plan;

        /// <summary>Creates a trigger builder that adds entries to the given plan.</summary>
        public TriggerBuilder(IReactivePlan<TModel> plan)
        {
            _plan = plan;
        }

        /// <summary>Wires a reaction that executes when the DOM is ready.</summary>
        public TriggerBuilder<TModel> DomReady(Action<PipelineBuilder<TModel>> configure)
        {
            var pb = new PipelineBuilder<TModel>();
            configure(pb);
            AddEntryWithContexts(new DomReadyTrigger(), pb);
            return this;
        }

        /// <summary>Wires a reaction that executes when the named custom event fires.</summary>
        public TriggerBuilder<TModel> CustomEvent(string eventName, Action<PipelineBuilder<TModel>> configure)
        {
            var pb = new PipelineBuilder<TModel>();
            configure(pb);
            AddEntryWithContexts(new CustomEventTrigger(eventName), pb);
            return this;
        }

        /// <summary>Wires a reaction with a typed payload that executes when the named custom event fires.</summary>
        public TriggerBuilder<TModel> CustomEvent<TPayload>(string eventName,
            Action<TPayload, PipelineBuilder<TModel>> configure)
            where TPayload : new()
        {
            var pb = new PipelineBuilder<TModel>();
            configure(new TPayload(), pb);
            AddEntryWithContexts(new CustomEventTrigger(eventName), pb);
            return this;
        }

        /// <summary>Wires a reaction that fires when the SSE stream sends any message.</summary>
        public TriggerBuilder<TModel> ServerPush(string url, Action<PipelineBuilder<TModel>> configure)
        {
            var pb = new PipelineBuilder<TModel>();
            configure(pb);
            AddEntryWithContexts(new ServerPushTrigger(url), pb);
            return this;
        }

        /// <summary>Wires a reaction that fires when the SSE stream sends a named event type.</summary>
        public TriggerBuilder<TModel> ServerPush(string url, string eventType, Action<PipelineBuilder<TModel>> configure)
        {
            var pb = new PipelineBuilder<TModel>();
            configure(pb);
            AddEntryWithContexts(new ServerPushTrigger(url, eventType), pb);
            return this;
        }

        /// <summary>Wires a reaction with typed payload when the SSE stream sends a named event type.</summary>
        public TriggerBuilder<TModel> ServerPush<TPayload>(string url, string eventType,
            Action<TPayload, PipelineBuilder<TModel>> configure)
            where TPayload : new()
        {
            var pb = new PipelineBuilder<TModel>();
            configure(new TPayload(), pb);
            AddEntryWithContexts(new ServerPushTrigger(url, eventType), pb);
            return this;
        }

        /// <summary>Wires a reaction that fires when the server invokes the named Hub method.</summary>
        public TriggerBuilder<TModel> SignalR(string hubUrl, string methodName,
            Action<PipelineBuilder<TModel>> configure)
        {
            var pb = new PipelineBuilder<TModel>();
            configure(pb);
            AddEntryWithContexts(new SignalRTrigger(hubUrl, methodName), pb);
            return this;
        }

        /// <summary>Wires a reaction with typed payload when the server invokes the named Hub method.</summary>
        public TriggerBuilder<TModel> SignalR<TPayload>(string hubUrl, string methodName,
            Action<TPayload, PipelineBuilder<TModel>> configure)
            where TPayload : new()
        {
            var pb = new PipelineBuilder<TModel>();
            configure(new TPayload(), pb);
            AddEntryWithContexts(new SignalRTrigger(hubUrl, methodName), pb);
            return this;
        }

        private void AddEntryWithContexts(Trigger trigger, PipelineBuilder<TModel> pb)
        {
            foreach (var reaction in pb.BuildReactions())
                _plan.AddEntry(new Entry(trigger, reaction));
        }
    }
}
