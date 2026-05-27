using System.Collections.Generic;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    internal sealed class ReactionSequenceDraft<TModel> where TModel : class
    {
        private readonly List<ReactionGraph> _orderedBlocks = new List<ReactionGraph>();
        private readonly List<ReactionGraph> _pendingSyncReactions = new List<ReactionGraph>();
        private List<BranchCase>? _pendingBranchCases;
        private int _branchStartsAfterReactionCount;
        private HttpRequestBuilder<TModel>? _pendingRequest;
        private ParallelBuilder<TModel>? _pendingParallelRequests;

        internal bool HasPendingCondition => _pendingBranchCases is not null;

        internal HttpRequestBuilder<TModel> BeginHttp(PlanBuildContext context)
        {
            FlushPendingAsyncReaction();
            FlushPendingBranch();
            var builder = new HttpRequestBuilder<TModel>(context);
            _pendingRequest = builder;
            return builder;
        }

        internal ParallelBuilder<TModel> BeginParallel(PlanBuildContext context)
        {
            FlushPendingAsyncReaction();
            FlushPendingBranch();
            var builder = new ParallelBuilder<TModel>(context);
            _pendingParallelRequests = builder;
            return builder;
        }

        internal void BeginConditional()
        {
            FlushPendingAsyncReaction();
            FlushPendingBranch();
        }

        internal void SetConditionalBranches(List<BranchCase> branches)
        {
            _pendingBranchCases = branches;
            _branchStartsAfterReactionCount = _pendingSyncReactions.Count;
        }

        internal void FlushSegment()
        {
            FlushPendingAsyncReaction();
            FlushPendingBranch();
            FlushPendingSyncReactions();
        }

        internal ReactionGraph BuildReaction()
        {
            FlushSegment();
            return _orderedBlocks.Count == 1
                ? _orderedBlocks[0]
                : ReactionGraph.Sequence(_orderedBlocks);
        }

        internal List<ReactionGraph> BuildReactions()
        {
            return new List<ReactionGraph> { BuildReaction() };
        }

        internal void AddCommand(ReactionGraph reaction)
        {
            FlushPendingAsyncReaction();
            _pendingSyncReactions.Add(reaction);
        }

        private void FlushPendingAsyncReaction()
        {
            if (_pendingRequest is not null)
            {
                FlushPendingSyncReactions();
                _orderedBlocks.Add(ReactionGraph.Request(_pendingRequest.BuildRequest()));
                _pendingRequest = null;
                return;
            }

            if (_pendingParallelRequests is not null)
            {
                FlushPendingSyncReactions();
                _orderedBlocks.Add(_pendingParallelRequests.BuildReaction());
                _pendingParallelRequests = null;
            }
        }

        private void FlushPendingBranch()
        {
            if (_pendingBranchCases is null)
                return;

            var pendingSyncReactions = TakePendingSyncReactions();
            AppendSequence(pendingSyncReactions, start: 0, count: _branchStartsAfterReactionCount);
            _orderedBlocks.Add(ReactionGraph.Branch(_pendingBranchCases));
            AppendSequence(
                pendingSyncReactions,
                start: _branchStartsAfterReactionCount,
                count: pendingSyncReactions.Count - _branchStartsAfterReactionCount);

            _pendingBranchCases = null;
            _branchStartsAfterReactionCount = 0;
        }

        private void FlushPendingSyncReactions()
        {
            if (_pendingSyncReactions.Count == 0)
                return;

            _orderedBlocks.Add(ReactionGraph.Sequence(TakePendingSyncReactions()));
        }

        private List<ReactionGraph> TakePendingSyncReactions()
        {
            var reactions = new List<ReactionGraph>(_pendingSyncReactions);
            _pendingSyncReactions.Clear();
            return reactions;
        }

        private void AppendSequence(List<ReactionGraph> reactions, int start, int count)
        {
            if (count > 0)
                _orderedBlocks.Add(ReactionGraph.Sequence(reactions.GetRange(start, count)));
        }
    }
}
