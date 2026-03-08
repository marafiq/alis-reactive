using System;
using System.Linq.Expressions;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Entry point for inner guards inside And/Or lambdas.
    /// Provides the When method that starts a condition source chain.
    /// </summary>
    public sealed class ConditionStart<TModel> where TModel : class
    {
        internal ConditionStart() { }

        /// <summary>
        /// Begins a condition on an event payload property.
        /// The payload instance is used only for type inference; its value is ignored.
        /// </summary>
        public ConditionSourceBuilder<TModel> When<TPayload>(
            TPayload payload,
            Expression<Func<TPayload, object?>> path)
        {
            var source = ExpressionPathHelper.ToEventPath(path);
            var propertyType = ExpressionPathHelper.GetPropertyType(path);
            return new ConditionSourceBuilder<TModel>(source, propertyType);
        }
    }
}
