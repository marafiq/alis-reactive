using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Descriptors.Guards;
using Alis.Reactive.Descriptors.Reactions;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Holds a single guard and provides And(), Or(), Not(), Then().
    /// And/Or support both lambda (for nesting) and direct source (matching ElseIf pattern).
    /// Then() creates a branch and returns BranchBuilder.
    /// </summary>
    public sealed class GuardBuilder<TModel> where TModel : class
    {
        internal Guard Guard { get; }

        // Back-references — only one is set depending on the entry path.
        private readonly PipelineBuilder<TModel>? _pipeline;
        private readonly BranchBuilder<TModel>? _branchBuilder;

        internal GuardBuilder(Guard guard, PipelineBuilder<TModel> pipeline)
        {
            Guard = guard;
            _pipeline = pipeline;
        }

        internal GuardBuilder(Guard guard, BranchBuilder<TModel> branchBuilder)
        {
            Guard = guard;
            _branchBuilder = branchBuilder;
        }

        internal GuardBuilder(Guard guard)
        {
            Guard = guard;
        }

        // --- Direct And/Or (matching ElseIf pattern) ---

        /// <summary>
        /// Direct AND — starts a new typed source for the second operand.
        /// Result: AllGuard with this guard + the operator applied on the new source.
        /// </summary>
        public ConditionSourceBuilder<TModel, TProp> And<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path)
        {
            var source = new EventArgSource<TPayload, TProp>(path);
            return new ConditionSourceBuilder<TModel, TProp>(
                source, CompositionMode.All, Guard, _pipeline, _branchBuilder);
        }

        /// <summary>
        /// Direct OR — starts a new typed source for the second operand.
        /// Result: AnyGuard with this guard + the operator applied on the new source.
        /// </summary>
        public ConditionSourceBuilder<TModel, TProp> Or<TPayload, TProp>(
            TPayload payload, Expression<Func<TPayload, TProp>> path)
        {
            var source = new EventArgSource<TPayload, TProp>(path);
            return new ConditionSourceBuilder<TModel, TProp>(
                source, CompositionMode.Any, Guard, _pipeline, _branchBuilder);
        }

        // --- Direct And/Or with TypedSource (source-vs-source) ---

        public ConditionSourceBuilder<TModel, TProp> And<TProp>(TypedSource<TProp> source)
        {
            return new ConditionSourceBuilder<TModel, TProp>(
                source, CompositionMode.All, Guard, _pipeline, _branchBuilder);
        }

        public ConditionSourceBuilder<TModel, TProp> Or<TProp>(TypedSource<TProp> source)
        {
            return new ConditionSourceBuilder<TModel, TProp>(
                source, CompositionMode.Any, Guard, _pipeline, _branchBuilder);
        }

        // --- Lambda And/Or (for complex nesting) ---

        /// <summary>
        /// Combines this guard with another using AND (all must match).
        /// Flattens nested AllGuards so chained .And().And() produces a single flat AllGuard.
        /// </summary>
        public GuardBuilder<TModel> And(
            Func<ConditionStart<TModel>, GuardBuilder<TModel>> inner)
        {
            var innerResult = inner(new ConditionStart<TModel>());
            var guards = new List<Guard>();
            FlattenAllStatic(Guard, guards);
            FlattenAllStatic(innerResult.Guard, guards);
            return WrapGuard(new AllGuard(guards));
        }

        /// <summary>
        /// Combines this guard with another using OR (any must match).
        /// Flattens nested AnyGuards so chained .Or().Or() produces a single flat AnyGuard.
        /// </summary>
        public GuardBuilder<TModel> Or(
            Func<ConditionStart<TModel>, GuardBuilder<TModel>> inner)
        {
            var innerResult = inner(new ConditionStart<TModel>());
            var guards = new List<Guard>();
            FlattenAnyStatic(Guard, guards);
            FlattenAnyStatic(innerResult.Guard, guards);
            return WrapGuard(new AnyGuard(guards));
        }

        // --- Not ---

        /// <summary>
        /// Wraps the current guard in an InvertGuard (logical NOT).
        /// </summary>
        public GuardBuilder<TModel> Not()
        {
            return WrapGuard(new InvertGuard(Guard));
        }

        // --- Then ---

        /// <summary>
        /// Terminates the guard chain and starts the reaction body for this branch.
        /// Returns a BranchBuilder that allows ElseIf/Else chaining.
        /// </summary>
        public BranchBuilder<TModel> Then(Action<PipelineBuilder<TModel>> configure)
        {
            var pb = new PipelineBuilder<TModel>();
            configure(pb);
            var reaction = pb.BuildReaction();
            var branch = new Branch(Guard, reaction);

            if (_branchBuilder != null)
            {
                _branchBuilder.AddBranch(branch);
                return _branchBuilder;
            }

            if (_pipeline == null)
                throw new InvalidOperationException(
                    "Then() requires a pipeline context. Use PipelineBuilder.When() or BranchBuilder.ElseIf() to start a condition.");

            var branches = new List<Branch> { branch };
            _pipeline.SetConditionalBranches(branches);
            return new BranchBuilder<TModel>(_pipeline, branches);
        }

        internal GuardBuilder<TModel> WrapGuard(Guard combined)
        {
            if (_pipeline != null)
                return new GuardBuilder<TModel>(combined, _pipeline);
            if (_branchBuilder != null)
                return new GuardBuilder<TModel>(combined, _branchBuilder);
            return new GuardBuilder<TModel>(combined);
        }

        internal static void FlattenAllStatic(Guard guard, List<Guard> target)
        {
            if (guard is AllGuard all) target.AddRange(all.Guards);
            else target.Add(guard);
        }

        internal static void FlattenAnyStatic(Guard guard, List<Guard> target)
        {
            if (guard is AnyGuard any) target.AddRange(any.Guards);
            else target.Add(guard);
        }
    }
}
