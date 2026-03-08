using System;
using System.Collections.Generic;
using Alis.Reactive.Descriptors.Guards;
using Alis.Reactive.Descriptors.Reactions;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Holds a single guard and provides And(), Or(), Then().
    /// And/Or take a lambda that receives a ConditionStart and wrap results in AllGuard/AnyGuard.
    /// Then() creates a branch and returns BranchBuilder.
    /// </summary>
    public sealed class GuardBuilder<TModel> where TModel : class
    {
        internal Guard Guard { get; }

        // Back-references — only one is set depending on the entry path.
        private readonly PipelineBuilder<TModel>? _pipeline;
        private readonly BranchBuilder<TModel>? _branchBuilder;

        /// <summary>
        /// Constructor for top-level: from PipelineBuilder.When()
        /// </summary>
        internal GuardBuilder(Guard guard, PipelineBuilder<TModel> pipeline)
        {
            Guard = guard;
            _pipeline = pipeline;
        }

        /// <summary>
        /// Constructor for chained branches: from BranchBuilder.ElseIf()
        /// </summary>
        internal GuardBuilder(Guard guard, BranchBuilder<TModel> branchBuilder)
        {
            Guard = guard;
            _branchBuilder = branchBuilder;
        }

        /// <summary>
        /// Constructor for inner guards: used in And/Or lambdas (no pipeline or branch).
        /// </summary>
        internal GuardBuilder(Guard guard)
        {
            Guard = guard;
        }

        /// <summary>
        /// Combines this guard with another using AND (all must match).
        /// Flattens nested AllGuards so chained .And().And() produces a single flat AllGuard.
        /// </summary>
        public GuardBuilder<TModel> And(
            Func<ConditionStart<TModel>, GuardBuilder<TModel>> inner)
        {
            var start = new ConditionStart<TModel>();
            var innerResult = inner(start);
            var guards = new List<Guard>();
            Flatten<AllGuard>(Guard, guards);
            Flatten<AllGuard>(innerResult.Guard, guards);
            var combined = new AllGuard(guards);

            if (_pipeline != null)
                return new GuardBuilder<TModel>(combined, _pipeline);
            if (_branchBuilder != null)
                return new GuardBuilder<TModel>(combined, _branchBuilder);
            return new GuardBuilder<TModel>(combined);
        }

        /// <summary>
        /// Combines this guard with another using OR (any must match).
        /// Flattens nested AnyGuards so chained .Or().Or() produces a single flat AnyGuard.
        /// </summary>
        public GuardBuilder<TModel> Or(
            Func<ConditionStart<TModel>, GuardBuilder<TModel>> inner)
        {
            var start = new ConditionStart<TModel>();
            var innerResult = inner(start);
            var guards = new List<Guard>();
            Flatten<AnyGuard>(Guard, guards);
            Flatten<AnyGuard>(innerResult.Guard, guards);
            var combined = new AnyGuard(guards);

            if (_pipeline != null)
                return new GuardBuilder<TModel>(combined, _pipeline);
            if (_branchBuilder != null)
                return new GuardBuilder<TModel>(combined, _branchBuilder);
            return new GuardBuilder<TModel>(combined);
        }

        /// <summary>
        /// Terminates the guard chain and starts the reaction body for this branch.
        /// Returns a BranchBuilder that allows ElseIf/Else chaining.
        /// </summary>
        public BranchBuilder<TModel> Then(Action<PipelineBuilder<TModel>> configure)
        {
            var pb = new PipelineBuilder<TModel>();
            configure(pb);
            var reaction = new SequentialReaction(pb.Commands);
            var branch = new Branch(Guard, reaction);

            if (_branchBuilder != null)
            {
                // Chained ElseIf — add branch to existing BranchBuilder
                _branchBuilder.AddBranch(branch);
                return _branchBuilder;
            }

            // First branch — create BranchBuilder and set branches on pipeline
            if (_pipeline == null)
                throw new InvalidOperationException(
                    "Then() requires a pipeline context. Use PipelineBuilder.When() or BranchBuilder.ElseIf() to start a condition.");

            var branches = new List<Branch> { branch };
            _pipeline.SetConditionalBranches(branches);
            return new BranchBuilder<TModel>(_pipeline, branches);
        }

        /// <summary>
        /// Flattens a guard into the target list. If the guard is the same composite
        /// type as TComposite, its children are added directly (flattened).
        /// Otherwise the guard is added as-is.
        /// </summary>
        private static void Flatten<TComposite>(Guard guard, List<Guard> target)
            where TComposite : Guard
        {
            if (guard is AllGuard all && typeof(TComposite) == typeof(AllGuard))
                target.AddRange(all.Guards);
            else if (guard is AnyGuard any && typeof(TComposite) == typeof(AnyGuard))
                target.AddRange(any.Guards);
            else
                target.Add(guard);
        }
    }
}
