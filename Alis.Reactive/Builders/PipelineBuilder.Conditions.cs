using System.Linq.Expressions;
using System;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    public partial class PipelineBuilder<TModel> where TModel : class
    {
        /// <summary>Starts a conditional branch from an event payload property.</summary>
        public ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(
            TPayload payload,
            Expression<Func<TPayload, TProp>> path)
        {
            FlushPendingConditionIfNeeded();
            _draft.BeginConditional();

            var source = PayloadTypedSource<TPayload, TProp>.FromEvent(path);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>Starts a conditional branch from an HTTP response body property.</summary>
        public ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(
            ResponseBody<TPayload> responseBody,
            Expression<Func<TPayload, TProp>> path)
            where TPayload : class
        {
            FlushPendingConditionIfNeeded();
            _draft.BeginConditional();

            var source = responseBody.Read(path);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>Starts a conditional branch from a typed source (component, plugin, or URL value).</summary>
        public ConditionSourceBuilder<TModel, TProp> When<TProp>(TypedSource<TProp> source)
        {
            FlushPendingConditionIfNeeded();
            _draft.BeginConditional();
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>Adds a user confirmation guard before proceeding with the pipeline.</summary>
        /// <param name="message">The confirmation message shown to the user.</param>
        public GuardBuilder<TModel> Confirm(string message)
        {
            FlushPendingConditionIfNeeded();
            _draft.BeginConditional();

            return new GuardBuilder<TModel>(Condition.Confirm(message), this);
        }

        private void FlushPendingConditionIfNeeded()
        {
            if (_draft.HasPendingCondition)
                FlushSegment();
        }
    }
}
