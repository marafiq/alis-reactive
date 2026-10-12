using System.Linq.Expressions;
using System;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    public partial class PipelineBuilder<TModel> where TModel : class
    {
        /// <summary>Starts a branch whose guard reads from the triggering event payload.</summary>
        /// <typeparam name="TPayload">Trigger payload contract.</typeparam>
        /// <typeparam name="TProp">Selected event-payload value type.</typeparam>
        /// <param name="payload">Typed event payload placeholder supplied by the trigger callback.</param>
        /// <param name="path">Payload value compared at runtime.</param>
        public ConditionSourceBuilder<TModel, TProp, GuardBuilder<TModel>> When<TPayload, TProp>(
            TPayload payload,
            Expression<Func<TPayload, TProp>> path)
        {
            var active = Active;
            active._draft.BeginBranch();

            var source = PayloadTypedSource<TPayload, TProp>.FromEvent(path);
            return new ConditionSourceBuilder<TModel, TProp, GuardBuilder<TModel>>(
                source, graph => new GuardBuilder<TModel>(graph, ConditionContinuation<TModel>.ForPipeline(active)));
        }

        /// <summary>Starts a branch whose guard reads from the active HTTP response body.</summary>
        /// <typeparam name="TResponse">Response body contract for the active response route.</typeparam>
        /// <typeparam name="TProp">Selected response-body value type.</typeparam>
        /// <param name="responseBody">Response body placeholder supplied by the response route callback.</param>
        /// <param name="path">Selects the response value to compare at runtime.</param>
        public ConditionSourceBuilder<TModel, TProp, GuardBuilder<TModel>> When<TResponse, TProp>(
            ResponseBody<TResponse> responseBody,
            Expression<Func<TResponse, TProp>> path)
            where TResponse : class
        {
            var active = Active;
            active._draft.BeginBranch();

            var source = responseBody.Read(path);
            return new ConditionSourceBuilder<TModel, TProp, GuardBuilder<TModel>>(
                source, graph => new GuardBuilder<TModel>(graph, ConditionContinuation<TModel>.ForPipeline(active)));
        }

        /// <summary>Starts a branch whose guard reads from a typed value source.</summary>
        /// <typeparam name="TProp">Source value type.</typeparam>
        /// <param name="source">Typed condition value source.</param>
        public ConditionSourceBuilder<TModel, TProp, GuardBuilder<TModel>> When<TProp>(TypedSource<TProp> source)
        {
            var active = Active;
            active._draft.BeginBranch();
            return new ConditionSourceBuilder<TModel, TProp, GuardBuilder<TModel>>(
                source, graph => new GuardBuilder<TModel>(graph, ConditionContinuation<TModel>.ForPipeline(active)));
        }

        /// <summary>Adds a user-decision guard before the pipeline continues.</summary>
        /// <param name="message">User-decision confirmation message.</param>
        public GuardBuilder<TModel> Confirm(string message)
        {
            var active = Active;
            active._draft.BeginBranch();

            return new GuardBuilder<TModel>(ConditionGraph.Confirm(message), ConditionContinuation<TModel>.ForPipeline(active));
        }
    }
}
