using System;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Entry point for building standalone condition expressions used in nested And/Or calls.
    /// </summary>
    /// <typeparam name="TModel">The view model type.</typeparam>
    public sealed class ConditionStart<TModel> where TModel : class
    {
        internal ConditionStart() { }

        /// <summary>Starts a condition from an event payload property.</summary>
        public ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(
            TPayload payload,
            Expression<Func<TPayload, TProp>> path)
        {
            var source = new EventArgSource<TPayload, TProp>(path);
            return new ConditionSourceBuilder<TModel, TProp>(source);
        }

        /// <summary>Starts a condition from an HTTP response body property.</summary>
        public ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(
            ResponseBody<TPayload> responseBody,
            Expression<Func<TPayload, TProp>> path)
            where TPayload : class
        {
            var source = responseBody.Read(path);
            return new ConditionSourceBuilder<TModel, TProp>(source);
        }

        /// <summary>Starts a condition from a typed source.</summary>
        public ConditionSourceBuilder<TModel, TProp> When<TProp>(TypedSource<TProp> source)
        {
            return new ConditionSourceBuilder<TModel, TProp>(source);
        }

        /// <summary>Creates a user confirmation guard that prompts before proceeding.</summary>
        /// <param name="message">The confirmation message shown to the user.</param>
        public GuardBuilder<TModel> Confirm(string message)
        {
            return new GuardBuilder<TModel>(Condition.Confirm(message));
        }
    }
}
