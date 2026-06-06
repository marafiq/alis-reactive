using System.Linq.Expressions;
using System;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    public partial class PipelineBuilder<TModel> where TModel : class
    {
        /// <summary>Starts a branch whose guard reads from the current event payload.</summary>
        /// <typeparam name="TPayload">The event payload contract supplied by the trigger callback.</typeparam>
        /// <typeparam name="TProp">The CLR type used to shape the selected event payload value.</typeparam>
        /// <param name="payload">The typed event payload placeholder supplied by the trigger callback.</param>
        /// <param name="path">Selects the payload value to compare at runtime.</param>
        /// <returns>A builder for choosing the guard comparison.</returns>
        public ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(
            TPayload payload,
            Expression<Func<TPayload, TProp>> path)
        {
            _draft.BeginBranch();

            var source = PayloadTypedSource<TPayload, TProp>.FromEvent(path);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>Starts a branch whose guard reads from the active HTTP response body.</summary>
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
            _draft.BeginBranch();

            var source = responseBody.Read(path);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>Starts a branch whose guard reads from a typed value source.</summary>
        /// <typeparam name="TProp">The CLR type carried by the typed value source.</typeparam>
        /// <param name="source">A typed value source accepted by conditions.</param>
        /// <returns>A builder for choosing the guard comparison.</returns>
        public ConditionSourceBuilder<TModel, TProp> When<TProp>(TypedSource<TProp> source)
        {
            _draft.BeginBranch();
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>Adds a user-decision guard before the pipeline continues.</summary>
        /// <param name="message">The confirmation message shown at the user-decision runtime boundary.</param>
        /// <returns>A guard builder for configuring accepted and rejected branches.</returns>
        public GuardBuilder<TModel> Confirm(string message)
        {
            _draft.BeginBranch();

            return new GuardBuilder<TModel>(ConditionGraph.Confirm(message), this);
        }
    }
}
