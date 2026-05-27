using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Composes conditions with And/Or/Not and branches with Then/ElseIf/Else.
    /// </summary>
    /// <remarks>
    /// Obtained after a comparison operator: <c>p.When(source).Gt(5).Then(...).Else(...)</c>.
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    public sealed class GuardBuilder<TModel> where TModel : class
    {
        internal Condition Condition { get; }

        private readonly ConditionContinuation<TModel> _continuation;

        internal GuardBuilder(Condition condition, PipelineBuilder<TModel> pipeline)
            : this(condition, ConditionContinuation<TModel>.ForPipeline(pipeline))
        {
        }

        internal GuardBuilder(Condition condition, BranchBuilder<TModel> branchBuilder)
            : this(condition, ConditionContinuation<TModel>.ForBranch(branchBuilder))
        {
        }

        internal GuardBuilder(Condition condition)
            : this(condition, ConditionContinuation<TModel>.Standalone)
        {
        }

        internal GuardBuilder(Condition condition, ConditionContinuation<TModel> continuation)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            _continuation = continuation ?? throw new ArgumentNullException(nameof(continuation));
        }

        /// <summary>Adds an AND condition from an event payload property.</summary>
        public ConditionSourceBuilder<TModel, TProp> And<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path)
        {
            var source = PayloadTypedSource<TPayload, TProp>.FromEvent(path);
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.All(Condition));
        }

        /// <summary>Adds an AND condition from an HTTP response body property.</summary>
        public ConditionSourceBuilder<TModel, TProp> And<TPayload, TProp>(
            ResponseBody<TPayload> responseBody, Expression<Func<TPayload, TProp>> path)
            where TPayload : class
        {
            var source = responseBody.Read(path);
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.All(Condition));
        }

        /// <summary>Adds an OR condition from an event payload property.</summary>
        public ConditionSourceBuilder<TModel, TProp> Or<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path)
        {
            var source = PayloadTypedSource<TPayload, TProp>.FromEvent(path);
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.Any(Condition));
        }

        /// <summary>Adds an OR condition from an HTTP response body property.</summary>
        public ConditionSourceBuilder<TModel, TProp> Or<TPayload, TProp>(
            ResponseBody<TPayload> responseBody, Expression<Func<TPayload, TProp>> path)
            where TPayload : class
        {
            var source = responseBody.Read(path);
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.Any(Condition));
        }

        /// <summary>Adds an AND condition from a typed source.</summary>
        public ConditionSourceBuilder<TModel, TProp> And<TProp>(TypedSource<TProp> source)
        {
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.All(Condition));
        }

        /// <summary>Adds an OR condition from a typed source.</summary>
        public ConditionSourceBuilder<TModel, TProp> Or<TProp>(TypedSource<TProp> source)
        {
            return new ConditionSourceBuilder<TModel, TProp>(
                source, _continuation, ConditionComposition.Any(Condition));
        }

        /// <summary>Adds an AND condition built from a nested condition expression.</summary>
        public GuardBuilder<TModel> And(
            Func<ConditionStart<TModel>, GuardBuilder<TModel>> inner)
        {
            var innerResult = inner(new ConditionStart<TModel>());
            var terms = new List<Condition>();
            ConditionComposition.FlattenAll(Condition, terms);
            ConditionComposition.FlattenAll(innerResult.Condition, terms);
            return WrapCondition(PlanModel.Condition.All(terms.ToArray()));
        }

        /// <summary>Adds an OR condition built from a nested condition expression.</summary>
        public GuardBuilder<TModel> Or(
            Func<ConditionStart<TModel>, GuardBuilder<TModel>> inner)
        {
            var innerResult = inner(new ConditionStart<TModel>());
            var terms = new List<Condition>();
            ConditionComposition.FlattenAny(Condition, terms);
            ConditionComposition.FlattenAny(innerResult.Condition, terms);
            return WrapCondition(PlanModel.Condition.Any(terms.ToArray()));
        }

        /// <summary>Inverts the current condition.</summary>
        /// <returns>A new guard with the negated condition.</returns>
        public GuardBuilder<TModel> Not()
        {
            return WrapCondition(PlanModel.Condition.Not(Condition));
        }

        /// <summary>Executes the pipeline when the condition is true. Returns a branch builder for ElseIf/Else.</summary>
        /// <param name="pipeline">Builds the commands to execute when the condition is met.</param>
        /// <returns>A branch builder for chaining ElseIf and Else cases.</returns>
        public BranchBuilder<TModel> Then(Action<PipelineBuilder<TModel>> pipeline)
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
            return _continuation.Then(Condition, pipeline);
        }

        internal GuardBuilder<TModel> WrapCondition(Condition combined)
        {
            return _continuation.Wrap(combined);
        }
    }
}
