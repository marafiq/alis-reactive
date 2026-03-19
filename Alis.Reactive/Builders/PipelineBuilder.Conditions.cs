using System.Linq.Expressions;
using System;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.Descriptors.Guards;

namespace Alis.Reactive.Builders
{
    public partial class PipelineBuilder<TModel> where TModel : class
    {
        /// <summary>
        /// Starts a conditional branch on an event payload property.
        /// If a previous conditional block exists, flushes it as a completed segment
        /// so each When().Then().Else() block evaluates independently.
        /// </summary>
        public ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(
            TPayload payload,
            Expression<Func<TPayload, TProp>> path)
        {
            // Flush previous segment if there are already branches (second+ When call)
            if (ConditionalBranches != null && ConditionalBranches.Count > 0)
                FlushSegment();

            SetMode(PipelineMode.Conditional);

            var source = new EventArgSource<TPayload, TProp>(path);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>
        /// Starts a conditional branch on a component property.
        /// If a previous conditional block exists, flushes it as a completed segment.
        /// </summary>
        public ConditionSourceBuilder<TModel, TProp> When<TProp>(TypedSource<TProp> source)
        {
            if (ConditionalBranches != null && ConditionalBranches.Count > 0)
                FlushSegment();

            SetMode(PipelineMode.Conditional);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>
        /// Starts a Confirm guard — an async halting condition that pauses the pipeline
        /// and shows a dialog to the user.
        /// </summary>
        public GuardBuilder<TModel> Confirm(string message)
        {
            if (ConditionalBranches != null && ConditionalBranches.Count > 0)
                FlushSegment();

            SetMode(PipelineMode.Conditional);

            return new GuardBuilder<TModel>(new ConfirmGuard(message), this);
        }
    }
}
