using System;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;

namespace Alis.Reactive.Builders
{
    public sealed class TriggerBuilder<TModel> where TModel : class
    {
        private readonly IReactivePlan<TModel> _plan;

        public TriggerBuilder(IReactivePlan<TModel> plan)
        {
            _plan = plan;
        }

        public TriggerBuilder<TModel> DomReady(Action<PipelineBuilder<TModel>> configure)
        {
            var pb = new PipelineBuilder<TModel>();
            configure(pb);
            AddEntryWithContexts(new DomReadyTrigger(), pb);
            return this;
        }

        public TriggerBuilder<TModel> CustomEvent(string eventName, Action<PipelineBuilder<TModel>> configure)
        {
            var pb = new PipelineBuilder<TModel>();
            configure(pb);
            AddEntryWithContexts(new CustomEventTrigger(eventName), pb);
            return this;
        }

        public TriggerBuilder<TModel> CustomEvent<TPayload>(string eventName,
            Action<TPayload, PipelineBuilder<TModel>> configure)
            where TPayload : new()
        {
            var pb = new PipelineBuilder<TModel>();
            configure(new TPayload(), pb);
            AddEntryWithContexts(new CustomEventTrigger(eventName), pb);
            return this;
        }

        private void AddEntryWithContexts(Trigger trigger, PipelineBuilder<TModel> pb)
        {
            _plan.AddEntry(new Entry(trigger, pb.BuildReaction()));
            (_plan as ReactivePlan<TModel>)?.RegisterBuildContexts(pb.BuildContexts);
        }
    }
}
