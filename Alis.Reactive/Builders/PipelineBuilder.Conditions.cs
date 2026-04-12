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
            SetMode(PipelineMode.Conditional);

            var source = new EventArgSource<TPayload, TProp>(path);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>Starts a conditional branch from an HTTP response body property.</summary>
        public ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(
            ResponseBody<TPayload> responseBody,
            Expression<Func<TPayload, TProp>> path)
            where TPayload : class
        {
            FlushPendingConditionIfNeeded();
            SetMode(PipelineMode.Conditional);

            var source = responseBody.Read(path);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>Starts a conditional branch from a typed source (component, plugin, or URL value).</summary>
        public ConditionSourceBuilder<TModel, TProp> When<TProp>(TypedSource<TProp> source)
        {
            FlushPendingConditionIfNeeded();
            SetMode(PipelineMode.Conditional);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>Adds a user confirmation guard before proceeding with the pipeline.</summary>
        /// <param name="message">The confirmation message shown to the user.</param>
        public GuardBuilder<TModel> Confirm(string message)
        {
            FlushPendingConditionIfNeeded();
            SetMode(PipelineMode.Conditional);

            return new GuardBuilder<TModel>(Condition.Confirm(message), this);
        }

        private void FlushPendingConditionIfNeeded()
        {
            var hasPendingCondition = ConditionalBranches != null && ConditionalBranches.Count > 0;
            if (hasPendingCondition)
                FlushSegment();
        }
    }
}
