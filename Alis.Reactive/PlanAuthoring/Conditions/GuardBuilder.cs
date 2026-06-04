using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Represents a completed condition expression that can be composed further or attached to a branch.
    /// </summary>
    /// <remarks>
    /// Created after a comparison operator, for example
    /// <c>p.When(source).Gt(5).Then(...).Else(...)</c>.
    /// </remarks>
    /// <typeparam name="TModel">The view model that owns the pipeline being guarded.</typeparam>
    public sealed class GuardBuilder<TModel> where TModel : class
    {
        internal ConditionGraph ConditionGraph { get; }

        private readonly ConditionContinuation<TModel> _continuation;

        internal GuardBuilder(ConditionGraph condition, PipelineBuilder<TModel> pipeline)
            : this(condition, ConditionContinuation<TModel>.ForPipeline(pipeline))
        {
        }

        internal GuardBuilder(ConditionGraph condition, BranchBuilder<TModel> branchBuilder)
            : this(condition, ConditionContinuation<TModel>.ForBranch(branchBuilder))
        {
        }

        internal GuardBuilder(ConditionGraph condition)
            : this(condition, ConditionContinuation<TModel>.Standalone)
        {
        }

        internal GuardBuilder(ConditionGraph condition, ConditionContinuation<TModel> continuation)
        {
            ConditionGraph = condition ?? throw new ArgumentNullException(nameof(condition));
            _continuation = continuation ?? throw new ArgumentNullException(nameof(continuation));
        }

        /// <summary>Requires this guard and an event-payload comparison to both pass.</summary>
        /// <typeparam name="TPayload">The event payload contract supplied by the trigger callback.</typeparam>
        /// <typeparam name="TProp">The selected payload value type.</typeparam>
        /// <param name="payload">The typed event payload placeholder supplied by the trigger callback.</param>
        /// <param name="path">Selects the payload value to compare at runtime.</param>
        /// <returns>A builder for choosing the added comparison.</returns>
        public ConditionSourceBuilder<TModel, TProp> And<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path)
        {
            var source = PayloadTypedSource<TPayload, TProp>.FromEvent(path);
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.All(ConditionGraph));
        }

        /// <summary>Requires this guard and an HTTP response-body comparison to both pass.</summary>
        /// <typeparam name="TPayload">The response body contract for the active response route.</typeparam>
        /// <typeparam name="TProp">The selected response value type.</typeparam>
        /// <param name="responseBody">The response body placeholder supplied by the response route callback.</param>
        /// <param name="path">Selects the response value to compare at runtime.</param>
        /// <returns>A builder for choosing the added comparison.</returns>
        public ConditionSourceBuilder<TModel, TProp> And<TPayload, TProp>(
            ResponseBody<TPayload> responseBody, Expression<Func<TPayload, TProp>> path)
            where TPayload : class
        {
            var source = responseBody.Read(path);
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.All(ConditionGraph));
        }

        /// <summary>Allows this guard or an event-payload comparison to pass.</summary>
        /// <typeparam name="TPayload">The event payload contract supplied by the trigger callback.</typeparam>
        /// <typeparam name="TProp">The selected payload value type.</typeparam>
        /// <param name="payload">The typed event payload placeholder supplied by the trigger callback.</param>
        /// <param name="path">Selects the payload value to compare at runtime.</param>
        /// <returns>A builder for choosing the added comparison.</returns>
        public ConditionSourceBuilder<TModel, TProp> Or<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path)
        {
            var source = PayloadTypedSource<TPayload, TProp>.FromEvent(path);
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.Any(ConditionGraph));
        }

        /// <summary>Allows this guard or an HTTP response-body comparison to pass.</summary>
        /// <typeparam name="TPayload">The response body contract for the active response route.</typeparam>
        /// <typeparam name="TProp">The selected response value type.</typeparam>
        /// <param name="responseBody">The response body placeholder supplied by the response route callback.</param>
        /// <param name="path">Selects the response value to compare at runtime.</param>
        /// <returns>A builder for choosing the added comparison.</returns>
        public ConditionSourceBuilder<TModel, TProp> Or<TPayload, TProp>(
            ResponseBody<TPayload> responseBody, Expression<Func<TPayload, TProp>> path)
            where TPayload : class
        {
            var source = responseBody.Read(path);
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.Any(ConditionGraph));
        }

        /// <summary>Requires this guard and a typed runtime value source comparison to both pass.</summary>
        /// <typeparam name="TProp">The runtime value type exposed by the source.</typeparam>
        /// <param name="source">A typed value source accepted by conditions.</param>
        /// <returns>A builder for choosing the added comparison.</returns>
        public ConditionSourceBuilder<TModel, TProp> And<TProp>(TypedSource<TProp> source)
        {
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.All(ConditionGraph));
        }

        /// <summary>Allows this guard or a typed runtime value source comparison to pass.</summary>
        /// <typeparam name="TProp">The runtime value type exposed by the source.</typeparam>
        /// <param name="source">A typed value source accepted by conditions.</param>
        /// <returns>A builder for choosing the added comparison.</returns>
        public ConditionSourceBuilder<TModel, TProp> Or<TProp>(TypedSource<TProp> source)
        {
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.Any(ConditionGraph));
        }

        /// <summary>Requires this guard and a nested condition expression to both pass.</summary>
        /// <param name="inner">Builds the nested condition expression to compose with this guard.</param>
        /// <returns>A guard containing the composed condition.</returns>
        public GuardBuilder<TModel> And(
            Func<ConditionStart<TModel>, GuardBuilder<TModel>> inner)
        {
            var innerResult = inner(new ConditionStart<TModel>());
            var terms = new List<ConditionGraph>();
            ConditionComposition.FlattenAll(ConditionGraph, terms);
            ConditionComposition.FlattenAll(innerResult.ConditionGraph, terms);
            return WrapCondition(PlanModel.ConditionGraph.All(terms.ToArray()));
        }

        /// <summary>Allows this guard or a nested condition expression to pass.</summary>
        /// <param name="inner">Builds the nested condition expression to compose with this guard.</param>
        /// <returns>A guard containing the composed condition.</returns>
        public GuardBuilder<TModel> Or(
            Func<ConditionStart<TModel>, GuardBuilder<TModel>> inner)
        {
            var innerResult = inner(new ConditionStart<TModel>());
            var terms = new List<ConditionGraph>();
            ConditionComposition.FlattenAny(ConditionGraph, terms);
            ConditionComposition.FlattenAny(innerResult.ConditionGraph, terms);
            return WrapCondition(PlanModel.ConditionGraph.Any(terms.ToArray()));
        }

        /// <summary>Inverts the current condition.</summary>
        /// <returns>A new guard with the negated condition.</returns>
        public GuardBuilder<TModel> Not()
        {
            return WrapCondition(PlanModel.ConditionGraph.Not(ConditionGraph));
        }

        /// <summary>Starts the branch that executes when this guard evaluates to true.</summary>
        /// <param name="pipeline">Builds the commands for the matching branch.</param>
        /// <returns>A branch builder for optional <c>ElseIf</c> and <c>Else</c> cases.</returns>
        public BranchBuilder<TModel> Then(Action<PipelineBuilder<TModel>> pipeline)
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
            return _continuation.Then(ConditionGraph, pipeline);
        }

        internal GuardBuilder<TModel> WrapCondition(ConditionGraph combined)
        {
            return _continuation.Wrap(combined);
        }
    }
}
