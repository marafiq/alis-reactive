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
        /// TProp is inferred from the expression — operators on the returned builder
        /// demand TProp operands (e.g. int property → Gte(int), string → Eq(string)).
        /// </summary>
        public ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(
            TPayload payload,
            Expression<Func<TPayload, TProp>> path)
        {
            SetMode(PipelineMode.Conditional);

            var source = new EventArgSource<TPayload, TProp>(path);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>
        /// Starts a conditional branch on a component property.
        /// TProp is inferred from the TypedSource — operators on the returned builder
        /// demand TProp operands (e.g. decimal property → Gt(0m)).
        /// Usage: p.When(comp.Value()).Gt(0m).Then(...)
        /// </summary>
        public ConditionSourceBuilder<TModel, TProp> When<TProp>(TypedSource<TProp> source)
        {
            SetMode(PipelineMode.Conditional);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>
        /// Starts a Confirm guard — an async halting condition that pauses the pipeline
        /// and shows a dialog to the user.
        /// </summary>
        public GuardBuilder<TModel> Confirm(string message)
        {
            SetMode(PipelineMode.Conditional);

            return new GuardBuilder<TModel>(new ConfirmGuard(message), this);
        }
    }
}
