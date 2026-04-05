using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    public sealed class TriggerBuilder<TModel> where TModel : class
    {
        private readonly ReactivePlan<TModel> _plan;
        private readonly PlanBuildContext _context;

        internal TriggerBuilder(ReactivePlan<TModel> plan, PlanBuildContext context)
        {
            _plan = plan;
            _context = context;
        }

        public TriggerBuilder<TModel> DomReady(Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            AddBehaviors(StartsWhen.PageReady(), pb);
            return this;
        }

        public TriggerBuilder<TModel> CustomEvent(string eventName, Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            AddBehaviors(StartsWhen.DocumentEvent(eventName), pb);
            return this;
        }

        public TriggerBuilder<TModel> CustomEvent<TPayload>(string eventName,
            Action<TPayload, PipelineBuilder<TModel>> pipeline)
            where TPayload : new()
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(new TPayload(), pb);
            AddBehaviors(StartsWhen.DocumentEvent(eventName), pb);
            return this;
        }

        public TriggerBuilder<TModel> ServerPush(string url, Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            AddBehaviors(StartsWhen.ServerPush(url), pb);
            return this;
        }

        public TriggerBuilder<TModel> ServerPush(string url, string eventType, Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            AddBehaviors(StartsWhen.ServerPush(url, eventType), pb);
            return this;
        }

        public TriggerBuilder<TModel> ServerPush<TPayload>(string url, string eventType,
            Action<TPayload, PipelineBuilder<TModel>> pipeline)
            where TPayload : new()
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(new TPayload(), pb);
            AddBehaviors(StartsWhen.ServerPush(url, eventType), pb);
            return this;
        }

        public TriggerBuilder<TModel> SignalR(string hubUrl, string methodName,
            Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            AddBehaviors(StartsWhen.SignalR(hubUrl, methodName), pb);
            return this;
        }

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
