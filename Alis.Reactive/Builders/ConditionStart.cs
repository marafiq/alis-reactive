using System;
using System.Linq.Expressions;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Entry point for inner guards inside And/Or lambdas.
    /// Provides the When method that starts a typed condition source chain.
    /// </summary>
    public sealed class ConditionStart<TModel> where TModel : class
    {
        internal ConditionStart() { }

        /// <summary>
        /// Begins a condition on an event payload property.
        /// TProp is inferred from the expression — operators demand TProp operands.
        /// </summary>
        public ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(
            TPayload payload,
            Expression<Func<TPayload, TProp>> path)
        {
            var source = ExpressionPathHelper.ToEventPath(path);
            return new ConditionSourceBuilder<TModel, TProp>(source);
        }
    }
}
