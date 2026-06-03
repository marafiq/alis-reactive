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

        /// <summary>Combines this guard with an event-payload comparison using <c>AND</c>.</summary>
        public ConditionSourceBuilder<TModel, TProp> And<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path)
        {
            var source = PayloadTypedSource<TPayload, TProp>.FromEvent(path);
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.All(ConditionGraph));
        }

        /// <summary>Combines this guard with an HTTP response-body comparison using <c>AND</c>.</summary>
        public ConditionSourceBuilder<TModel, TProp> And<TPayload, TProp>(
            ResponseBody<TPayload> responseBody, Expression<Func<TPayload, TProp>> path)
            where TPayload : class
        {
            var source = responseBody.Read(path);
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.All(ConditionGraph));
        }

        /// <summary>Combines this guard with an event-payload comparison using <c>OR</c>.</summary>
        public ConditionSourceBuilder<TModel, TProp> Or<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path)
        {
            var source = PayloadTypedSource<TPayload, TProp>.FromEvent(path);
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.Any(ConditionGraph));
        }

        /// <summary>Combines this guard with an HTTP response-body comparison using <c>OR</c>.</summary>
        public ConditionSourceBuilder<TModel, TProp> Or<TPayload, TProp>(
            ResponseBody<TPayload> responseBody, Expression<Func<TPayload, TProp>> path)
            where TPayload : class
        {
            var source = responseBody.Read(path);
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.Any(ConditionGraph));
        }

        /// <summary>Combines this guard with another typed value source using <c>AND</c>.</summary>
        public ConditionSourceBuilder<TModel, TProp> And<TProp>(TypedSource<TProp> source)
        {
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.All(ConditionGraph));
        }

        /// <summary>Combines this guard with another typed value source using <c>OR</c>.</summary>
        public ConditionSourceBuilder<TModel, TProp> Or<TProp>(TypedSource<TProp> source)
        {
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.Any(ConditionGraph));
        }

        /// <summary>Combines this guard with a nested condition expression using <c>AND</c>.</summary>
        public GuardBuilder<TModel> And(
            Func<ConditionStart<TModel>, GuardBuilder<TModel>> inner)
        {
            var innerResult = inner(new ConditionStart<TModel>());
            var terms = new List<ConditionGraph>();
            ConditionComposition.FlattenAll(ConditionGraph, terms);
            ConditionComposition.FlattenAll(innerResult.ConditionGraph, terms);
            return WrapCondition(PlanModel.ConditionGraph.All(terms.ToArray()));
        }

        /// <summary>Combines this guard with a nested condition expression using <c>OR</c>.</summary>
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
