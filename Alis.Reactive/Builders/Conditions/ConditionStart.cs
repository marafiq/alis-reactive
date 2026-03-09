using System;
using System.Linq.Expressions;
using Alis.Reactive.Descriptors.Guards;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Entry point for inner guards inside And/Or lambdas.
    /// Provides the When method that starts a typed condition source chain.
    /// Also provides Confirm() for async halting conditions.
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
            var source = new EventArgSource<TPayload, TProp>(path);
            return new ConditionSourceBuilder<TModel, TProp>(source);
        }

        /// <summary>
        /// Creates a ConfirmGuard — an async halting condition that pauses the pipeline
        /// and shows a dialog to the user.
        /// </summary>
        public GuardBuilder<TModel> Confirm(string message)
        {
            return new GuardBuilder<TModel>(new ConfirmGuard(message));
        }
    }
}
