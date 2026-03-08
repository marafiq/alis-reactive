using System;
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
            _plan.AddEntry(new Entry(
                new DomReadyTrigger(),
                pb.BuildReaction()
            ));
            return this;
        }

        public TriggerBuilder<TModel> CustomEvent(string eventName, Action<PipelineBuilder<TModel>> configure)
        {
            var pb = new PipelineBuilder<TModel>();
            configure(pb);
            _plan.AddEntry(new Entry(
                new CustomEventTrigger(eventName),
                pb.BuildReaction()
            ));
            return this;
        }

        public TriggerBuilder<TModel> CustomEvent<TPayload>(string eventName,
            Action<TPayload, PipelineBuilder<TModel>> configure)
            where TPayload : new()
        {
            var pb = new PipelineBuilder<TModel>();
            configure(new TPayload(), pb);
            _plan.AddEntry(new Entry(
                new CustomEventTrigger(eventName),
                pb.BuildReaction()
            ));
            return this;
        }
    }
}
