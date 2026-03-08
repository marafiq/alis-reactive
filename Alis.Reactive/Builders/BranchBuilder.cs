using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Descriptors.Guards;
using Alis.Reactive.Descriptors.Reactions;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Allows chaining ElseIf/Else branches after an initial When().Then() block.
    /// Each branch is added to the shared branch list that backs the ConditionalReaction.
    /// </summary>
    public sealed class BranchBuilder<TModel> where TModel : class
    {
        private readonly PipelineBuilder<TModel> _pipeline;
        private readonly List<Branch> _branches;

        internal BranchBuilder(PipelineBuilder<TModel> pipeline, List<Branch> branches)
        {
            _pipeline = pipeline;
            _branches = branches;
        }

        /// <summary>
        /// Adds a new conditional branch with a guard.
        /// </summary>
        public ConditionSourceBuilder<TModel> ElseIf<TPayload>(
            TPayload payload,
            Expression<Func<TPayload, object?>> path)
        {
            var source = ExpressionPathHelper.ToEventPath(path);
            var propertyType = ExpressionPathHelper.GetPropertyType(path);
            return new ConditionSourceBuilder<TModel>(source, propertyType, this);
        }

        /// <summary>
        /// Adds a fallback branch with no guard (always matches if reached).
        /// This must be the last branch.
        /// </summary>
        public void Else(Action<PipelineBuilder<TModel>> configure)
        {
            var pb = new PipelineBuilder<TModel>();
            configure(pb);
            var reaction = new SequentialReaction(pb.Commands);
            _branches.Add(new Branch(null, reaction));
        }

        /// <summary>
        /// Called by GuardBuilder.Then() to add a branch to the shared list.
        /// </summary>
        internal void AddBranch(Branch branch)
        {
            _branches.Add(branch);
        }
    }
}
