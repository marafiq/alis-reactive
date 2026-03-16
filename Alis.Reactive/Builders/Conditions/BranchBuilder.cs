using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Descriptors.Reactions;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Allows chaining ElseIf/Else branches after an initial When().Then() block.
    /// Each branch is added to the shared branch list that backs the ConditionalReaction.
    /// </summary>
    public sealed class BranchBuilder<TModel> where TModel : class
    {
        private readonly List<Branch> _branches;
        private bool _elseCalled;

        internal PipelineBuilder<TModel> Pipeline { get; }

        internal BranchBuilder(PipelineBuilder<TModel> pipeline, List<Branch> branches)
        {
            Pipeline = pipeline;
            _branches = branches;
        }

        /// <summary>
        /// Adds a new conditional branch with a typed guard.
        /// TProp is inferred from the expression — operators demand TProp operands.
        /// </summary>
        public ConditionSourceBuilder<TModel, TProp> ElseIf<TPayload, TProp>(
            TPayload payload,
            Expression<Func<TPayload, TProp>> path)
        {
            if (_elseCalled)
                throw new InvalidOperationException(
                    "Cannot add ElseIf after Else. Else must be the last branch.");

            var source = new EventArgSource<TPayload, TProp>(path);
            return new ConditionSourceBuilder<TModel, TProp>(source, this);
        }

        /// <summary>
        /// Adds a fallback branch with no guard (always matches if reached).
        /// This must be the last branch.
        /// </summary>
        public void Else(Action<PipelineBuilder<TModel>> configure)
        {
            if (_elseCalled)
                throw new InvalidOperationException(
                    "Else has already been called. Only one Else branch is allowed and it must be last.");

            var pb = new PipelineBuilder<TModel>();
            configure(pb);
            var reaction = pb.BuildReaction();
            _branches.Add(new Branch(null, reaction));
            _elseCalled = true;
        }

        /// <summary>
        /// Called by GuardBuilder.Then() to add a branch to the shared list.
        /// </summary>
        internal void AddBranch(Branch branch)
        {
            if (_elseCalled)
                throw new InvalidOperationException(
                    "Cannot add branches after Else. Else must be the last branch.");

            _branches.Add(branch);
        }
    }
}
