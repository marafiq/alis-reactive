using System.Linq.Expressions;
using System;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    public partial class PipelineBuilder<TModel> where TModel : class
    {
        public ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(
            TPayload payload,
            Expression<Func<TPayload, TProp>> path)
        {
            if (ConditionalBranches != null && ConditionalBranches.Count > 0)
                FlushSegment();

            SetMode(PipelineMode.Conditional);

            var source = new EventArgSource<TPayload, TProp>(path);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        public ConditionSourceBuilder<TModel, TProp> When<TProp>(TypedSource<TProp> source)
        {
            if (ConditionalBranches != null && ConditionalBranches.Count > 0)
                FlushSegment();

            SetMode(PipelineMode.Conditional);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        public GuardBuilder<TModel> Confirm(string message)
        {
            if (ConditionalBranches != null && ConditionalBranches.Count > 0)
                FlushSegment();

            SetMode(PipelineMode.Conditional);

            return new GuardBuilder<TModel>(Condition.Confirm(message), this);
        }
    }
}
