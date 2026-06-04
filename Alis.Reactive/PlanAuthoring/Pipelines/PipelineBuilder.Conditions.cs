using System.Linq.Expressions;
using System;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    public partial class PipelineBuilder<TModel> where TModel : class
    {
        /// <summary>Starts a conditional branch from an event payload property.</summary>
        /// <typeparam name="TPayload">The event payload type.</typeparam>
        /// <typeparam name="TProp">The payload value type compared by this branch.</typeparam>
        /// <param name="payload">The typed event payload placeholder supplied by the trigger callback.</param>
        /// <param name="path">The payload property used as the condition source.</param>
        /// <returns>A condition source builder for choosing the comparison operation.</returns>
        public ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(
            TPayload payload,
            Expression<Func<TPayload, TProp>> path)
        {
            _draft.BeginBranch();

            var source = PayloadTypedSource<TPayload, TProp>.FromEvent(path);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>Starts a conditional branch from an HTTP response body property.</summary>
        /// <typeparam name="TPayload">The HTTP response body type.</typeparam>
        /// <typeparam name="TProp">The response value type compared by this branch.</typeparam>
        /// <param name="responseBody">The response body placeholder supplied by the response route callback.</param>
        /// <param name="path">The response property used as the condition source.</param>
        /// <returns>A condition source builder for choosing the comparison operation.</returns>
        public ConditionSourceBuilder<TModel, TProp> When<TPayload, TProp>(
            ResponseBody<TPayload> responseBody,
            Expression<Func<TPayload, TProp>> path)
            where TPayload : class
        {
            _draft.BeginBranch();

            var source = responseBody.Read(path);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>Starts a conditional branch from a typed source (component, plugin, or URL value).</summary>
        /// <typeparam name="TProp">The source value type.</typeparam>
        /// <param name="source">The value source used by the condition.</param>
        /// <returns>A condition source builder for choosing the comparison operation.</returns>
        public ConditionSourceBuilder<TModel, TProp> When<TProp>(TypedSource<TProp> source)
        {
            _draft.BeginBranch();
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>Adds a user confirmation guard before proceeding with the pipeline.</summary>
        /// <param name="message">The confirmation message shown to the user.</param>
        /// <returns>A guard builder for configuring the accepted and rejected branches.</returns>
        public GuardBuilder<TModel> Confirm(string message)
        {
            _draft.BeginBranch();

            return new GuardBuilder<TModel>(ConditionGraph.Confirm(message), this);
        }
    }
}
