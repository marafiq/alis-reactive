using System;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Entry point for nested condition expressions composed inside <c>And</c> and <c>Or</c> guards.
    /// </summary>
    /// <typeparam name="TModel">The view model that owns the condition being authored.</typeparam>
    public sealed class ConditionStart<TModel> where TModel : class
    {
        internal ConditionStart() { }

        /// <summary>Starts a nested condition from the current event payload.</summary>
        /// <typeparam name="TPayload">The event payload contract supplied by the trigger callback.</typeparam>
        /// <typeparam name="TProp">The CLR type used to shape the selected event payload value.</typeparam>
        /// <param name="payload">The event payload placeholder supplied by the trigger callback.</param>
        /// <param name="path">Selects the payload value to compare at runtime.</param>
        /// <returns>A builder for choosing the guard comparison.</returns>
        public ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(
            TPayload payload,
            Expression<Func<TPayload, TProp>> path)
        {
            var source = PayloadTypedSource<TPayload, TProp>.FromEvent(path);
            return new ConditionSourceBuilder<TModel, TProp>(source);
        }

        /// <summary>Starts a nested condition from the active HTTP response body.</summary>
        /// <typeparam name="TResponse">The response body contract for the active response route.</typeparam>
        /// <typeparam name="TProp">The CLR type used to shape the selected response-body value.</typeparam>
        /// <param name="responseBody">The response body placeholder supplied by the response route callback.</param>
        /// <param name="path">Selects the response value to compare at runtime.</param>
        /// <returns>A builder for choosing the guard comparison.</returns>
        public ConditionSourceBuilder<TModel, TProp> When<TResponse, TProp>(
            ResponseBody<TResponse> responseBody,
            Expression<Func<TResponse, TProp>> path)
            where TResponse : class
        {
            var source = responseBody.Read(path);
            return new ConditionSourceBuilder<TModel, TProp>(source);
        }

        /// <summary>Starts a nested condition from a typed value source.</summary>
        /// <typeparam name="TProp">The CLR type carried by the typed value source.</typeparam>
        /// <param name="source">A typed value source accepted by conditions.</param>
        /// <returns>A builder for choosing the guard comparison.</returns>
        public ConditionSourceBuilder<TModel, TProp> When<TProp>(TypedSource<TProp> source)
        {
            return new ConditionSourceBuilder<TModel, TProp>(source);
        }

        /// <summary>Creates a user-decision guard for a nested condition expression.</summary>
        /// <param name="message">The confirmation message shown at the user-decision runtime boundary.</param>
        /// <returns>A guard that can be composed with surrounding condition terms.</returns>
        public GuardBuilder<TModel> Confirm(string message)
        {
            return new GuardBuilder<TModel>(ConditionGraph.Confirm(message));
        }
    }
}
