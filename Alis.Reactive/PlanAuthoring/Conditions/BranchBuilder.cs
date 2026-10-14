using System;
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
        private readonly PlanBuildContext _context;
        private readonly BranchStep _branch;

        internal BranchBuilder(PlanBuildContext context, BranchStep branch)
        {
            _context = context;
            _branch = branch;
        }

        /// <summary>Adds the next ordered <c>ElseIf</c> branch from a typed value source.</summary>
        /// <typeparam name="TProp">Source value type.</typeparam>
        /// <param name="source">Typed condition value source.</param>
        public ConditionSourceBuilder<TModel, TProp, GuardBuilder<TModel>> ElseIf<TProp>(TypedSource<TProp> source)
        {
            if (_branch.HasElse)
                throw new InvalidOperationException("Cannot add ElseIf after Else.");

            return new ConditionSourceBuilder<TModel, TProp, GuardBuilder<TModel>>(
                source, graph => new GuardBuilder<TModel>(graph, ConditionContinuation<TModel>.ForBranch(this)));
        }

        /// <summary>Adds the default branch that runs when no earlier branch matched.</summary>
        /// <param name="pipeline">Builds the reactions for the default branch.</param>
        public void Else(Action<PipelineBuilder<TModel>> pipeline)
        {
            if (_branch.HasElse)
                throw new InvalidOperationException("Else already called.");

            _branch.SetElse(PipelineBuilder<TModel>.Build(_context, pipeline));
        }

        internal void AddArm(ConditionGraph condition, Action<PipelineBuilder<TModel>> pipeline) =>
            _branch.AddArm(condition, PipelineBuilder<TModel>.Build(_context, pipeline));
    }
}
