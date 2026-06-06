using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Continues an ordered condition branch after a matching <c>Then</c> case.
    /// </summary>
    /// <remarks>
    /// Branch order is preserved in the Reactive Plan. Use <c>ElseIf(...)</c>
    /// for additional guards and <c>Else(...)</c> for the final default branch.
    /// </remarks>
    /// <typeparam name="TModel">The view model that owns the branch pipeline.</typeparam>
    public sealed class BranchBuilder<TModel> where TModel : class
    {
        private readonly List<BranchCase> _cases;
        private bool _hasDefaultCase;

        internal PipelineBuilder<TModel> Pipeline { get; }

        internal BranchBuilder(PipelineBuilder<TModel> pipeline, List<BranchCase> cases)
        {
            Pipeline = pipeline;
            _cases = cases;
        }

        /// <summary>Adds the next ordered <c>ElseIf</c> branch from the triggering event payload.</summary>
        /// <typeparam name="TPayload">The event payload contract supplied by the trigger callback.</typeparam>
        /// <typeparam name="TProp">The CLR type used to shape the selected event payload value.</typeparam>
        /// <param name="payload">The event payload placeholder supplied by the trigger callback.</param>
        /// <param name="path">Selects the payload value to compare at runtime.</param>
        /// <returns>A builder for choosing the guard comparison.</returns>
        public ConditionSourceBuilder<TModel, TProp> ElseIf<TPayload, TProp>(
            TPayload payload,
            Expression<Func<TPayload, TProp>> path)
        {
            EnsureElseIfCanBeAdded();

            var source = PayloadTypedSource<TPayload, TProp>.FromEvent(path);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>Adds the next ordered <c>ElseIf</c> branch from the active HTTP response body.</summary>
        /// <typeparam name="TResponse">The response body contract for the active response route.</typeparam>
        /// <typeparam name="TProp">The CLR type used to shape the selected response-body value.</typeparam>
        /// <param name="responseBody">The response body placeholder supplied by the response route callback.</param>
        /// <param name="path">Selects the response value to compare at runtime.</param>
        /// <returns>A builder for choosing the guard comparison.</returns>
        public ConditionSourceBuilder<TModel, TProp> ElseIf<TResponse, TProp>(
            ResponseBody<TResponse> responseBody,
            Expression<Func<TResponse, TProp>> path)
            where TResponse : class
        {
            EnsureElseIfCanBeAdded();

            var source = responseBody.Read(path);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>Adds the next ordered <c>ElseIf</c> branch from a typed value source.</summary>
        /// <typeparam name="TProp">The CLR type carried by the typed value source.</typeparam>
        /// <param name="source">A typed value source accepted by conditions.</param>
        /// <returns>A builder for choosing the guard comparison.</returns>
        public ConditionSourceBuilder<TModel, TProp> ElseIf<TProp>(TypedSource<TProp> source)
        {
            EnsureElseIfCanBeAdded();

            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>Adds the default branch that runs when no earlier branch matched.</summary>
        /// <param name="pipeline">Builds the reactions for the default branch.</param>
        public void Else(Action<PipelineBuilder<TModel>> pipeline)
        {
            EnsureDefaultCanBeAdded();

            var reactionPipeline = new PipelineBuilder<TModel>(Pipeline.Context);
            pipeline(reactionPipeline);
            _cases.Add(BranchCase.Default(reactionPipeline.BuildReaction()));
            _hasDefaultCase = true;
        }

        internal void AddBranch(BranchCase branchCase)
        {
            EnsureBranchCanBeAdded();
            _cases.Add(branchCase);
        }

        private void EnsureElseIfCanBeAdded()
        {
            if (_hasDefaultCase)
                throw new InvalidOperationException("Cannot add ElseIf after Else.");
        }

        private void EnsureDefaultCanBeAdded()
        {
            if (_hasDefaultCase)
                throw new InvalidOperationException("Else already called.");
        }

        private void EnsureBranchCanBeAdded()
        {
            if (_hasDefaultCase)
                throw new InvalidOperationException("Cannot add branches after Else.");
        }
    }
}
