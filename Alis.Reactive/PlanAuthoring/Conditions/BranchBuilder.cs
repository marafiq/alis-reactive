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
    /// <typeparam name="TModel">View model that owns the branch pipeline.</typeparam>
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

        /// <summary>Adds the next ordered <c>ElseIf</c> branch from a typed value source.</summary>
        /// <typeparam name="TProp">Source value type.</typeparam>
        /// <param name="source">Typed condition value source.</param>
        public ConditionSourceBuilder<TModel, TProp, GuardBuilder<TModel>> ElseIf<TProp>(TypedSource<TProp> source)
        {
            EnsureElseIfCanBeAdded();

            return new ConditionSourceBuilder<TModel, TProp, GuardBuilder<TModel>>(
                source, graph => new GuardBuilder<TModel>(graph, ConditionContinuation<TModel>.ForBranch(this)));
        }

        /// <summary>Adds the default branch that runs when no earlier branch matched.</summary>
        /// <param name="pipeline">Builds the reactions for the default branch.</param>
        public void Else(Action<PipelineBuilder<TModel>> pipeline)
        {
            EnsureDefaultCanBeAdded();

            _cases.Add(BranchCase.Default(PipelineBuilder<TModel>.Build(Pipeline.Context, pipeline)));
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
