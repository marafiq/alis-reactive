using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Continues a condition branch chain after a matching <c>Then</c> case.
    /// </summary>
    /// <remarks>
    /// Use <c>ElseIf(...)</c> to add more ordered branches, or <c>Else(...)</c>
    /// to add the final default branch.
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

        /// <summary>Starts an ordered <c>ElseIf</c> branch from an event payload property.</summary>
        public ConditionSourceBuilder<TModel, TProp> ElseIf<TPayload, TProp>(
            TPayload payload,
            Expression<Func<TPayload, TProp>> path)
        {
            EnsureElseIfCanBeAdded();

            var source = PayloadTypedSource<TPayload, TProp>.FromEvent(path);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>Starts an ordered <c>ElseIf</c> branch from an HTTP response body property.</summary>
        public ConditionSourceBuilder<TModel, TProp> ElseIf<TPayload, TProp>(
            ResponseBody<TPayload> responseBody,
            Expression<Func<TPayload, TProp>> path)
            where TPayload : class
        {
            EnsureElseIfCanBeAdded();

            var source = responseBody.Read(path);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>Starts an ordered <c>ElseIf</c> branch from a typed value source.</summary>
        public ConditionSourceBuilder<TModel, TProp> ElseIf<TProp>(TypedSource<TProp> source)
        {
            EnsureElseIfCanBeAdded();

            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>Adds the default branch that runs when no previous condition matched.</summary>
        /// <param name="pipeline">Builds the commands for the default branch.</param>
        public void Else(Action<PipelineBuilder<TModel>> pipeline)
        {
            EnsureDefaultCanBeAdded();

            var pb = new PipelineBuilder<TModel>(Pipeline.Context);
            pipeline(pb);
            _cases.Add(BranchCase.Default(pb.BuildReaction()));
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
