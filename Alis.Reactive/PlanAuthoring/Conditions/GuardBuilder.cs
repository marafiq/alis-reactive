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

        /// <summary>Adds an all-of comparison against the current event payload.</summary>
        /// <typeparam name="TPayload">The event payload contract supplied by the trigger callback.</typeparam>
        /// <typeparam name="TProp">The selected payload value type.</typeparam>
        /// <param name="payload">The event payload placeholder supplied by the trigger callback.</param>
        /// <param name="path">Selects the payload value to compare at runtime.</param>
        /// <returns>A builder for choosing the added comparison.</returns>
        public ConditionSourceBuilder<TModel, TProp> And<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path)
        {
            var source = PayloadTypedSource<TPayload, TProp>.FromEvent(path);
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.All(ConditionGraph));
        }

        /// <summary>Adds an all-of comparison against the active HTTP response body.</summary>
        /// <typeparam name="TResponse">The response body contract for the active response route.</typeparam>
        /// <typeparam name="TProp">The selected response value type.</typeparam>
        /// <param name="responseBody">The response body placeholder supplied by the response route callback.</param>
        /// <param name="path">Selects the response value to compare at runtime.</param>
        /// <returns>A builder for choosing the added comparison.</returns>
        public ConditionSourceBuilder<TModel, TProp> And<TResponse, TProp>(
            ResponseBody<TResponse> responseBody, Expression<Func<TResponse, TProp>> path)
            where TResponse : class
        {
            var source = responseBody.Read(path);
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.All(ConditionGraph));
        }

        /// <summary>Adds an any-of comparison against the current event payload.</summary>
        /// <typeparam name="TPayload">The event payload contract supplied by the trigger callback.</typeparam>
        /// <typeparam name="TProp">The selected payload value type.</typeparam>
        /// <param name="payload">The event payload placeholder supplied by the trigger callback.</param>
        /// <param name="path">Selects the payload value to compare at runtime.</param>
        /// <returns>A builder for choosing the added comparison.</returns>
        public ConditionSourceBuilder<TModel, TProp> Or<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path)
        {
            var source = PayloadTypedSource<TPayload, TProp>.FromEvent(path);
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.Any(ConditionGraph));
        }

        /// <summary>Adds an any-of comparison against the active HTTP response body.</summary>
        /// <typeparam name="TResponse">The response body contract for the active response route.</typeparam>
        /// <typeparam name="TProp">The selected response value type.</typeparam>
        /// <param name="responseBody">The response body placeholder supplied by the response route callback.</param>
        /// <param name="path">Selects the response value to compare at runtime.</param>
        /// <returns>A builder for choosing the added comparison.</returns>
        public ConditionSourceBuilder<TModel, TProp> Or<TResponse, TProp>(
            ResponseBody<TResponse> responseBody, Expression<Func<TResponse, TProp>> path)
            where TResponse : class
        {
            var source = responseBody.Read(path);
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.Any(ConditionGraph));
        }

        /// <summary>Adds an all-of comparison against a typed value source.</summary>
        /// <typeparam name="TProp">The value type exposed by the source.</typeparam>
        /// <param name="source">A typed value source accepted by conditions.</param>
        /// <returns>A builder for choosing the added comparison.</returns>
        public ConditionSourceBuilder<TModel, TProp> And<TProp>(TypedSource<TProp> source)
        {
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.All(ConditionGraph));
        }

        /// <summary>Adds an any-of comparison against a typed value source.</summary>
        /// <typeparam name="TProp">The value type exposed by the source.</typeparam>
        /// <param name="source">A typed value source accepted by conditions.</param>
        /// <returns>A builder for choosing the added comparison.</returns>
        public ConditionSourceBuilder<TModel, TProp> Or<TProp>(TypedSource<TProp> source)
        {
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.Any(ConditionGraph));
        }

        /// <summary>Composes this guard with a grouped all-of condition.</summary>
        /// <param name="inner">Builds the grouped condition to compose with this guard.</param>
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

        /// <summary>Composes this guard with a grouped any-of condition.</summary>
        /// <param name="inner">Builds the grouped condition to compose with this guard.</param>
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

        /// <summary>Negates the current condition.</summary>
        /// <returns>A new guard with the negated condition.</returns>
        public GuardBuilder<TModel> Not()
        {
            return WrapCondition(PlanModel.ConditionGraph.Not(ConditionGraph));
        }

        /// <summary>Starts the branch that runs when this guard matches.</summary>
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
