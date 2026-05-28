using System.Collections.Generic;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    internal sealed class ReactionPipelineDraft<TModel> where TModel : class
    {
        private readonly List<ReactionGraph> _orderedBlocks = new List<ReactionGraph>();
        private readonly List<ReactionGraph> _pendingSyncReactions = new List<ReactionGraph>();
        private PendingBranch _pendingBranch = PendingBranch.None;
        private PendingAsyncReaction<TModel> _pendingAsyncReaction = PendingAsyncReaction<TModel>.None;

        internal HttpRequestBuilder<TModel> BeginHttp(PlanBuildContext context)
        {
            FlushPendingAsyncReaction();
            FlushPendingBranch();
            var builder = new HttpRequestBuilder<TModel>(context);
            _pendingAsyncReaction = PendingAsyncReaction<TModel>.Request(builder);
            return builder;
        }

        internal ParallelBuilder<TModel> BeginParallel(PlanBuildContext context)
        {
            FlushPendingAsyncReaction();
            FlushPendingBranch();
            var builder = new ParallelBuilder<TModel>(context);
            _pendingAsyncReaction = PendingAsyncReaction<TModel>.Parallel(builder);
            return builder;
        }

        internal void BeginBranch()
        {
            FlushPendingAsyncReaction();
            FlushPendingBranch();
        }

        internal void SetConditionalBranches(List<BranchCase> branches)
        {
            _pendingBranch = PendingBranch.Cases(
                branches,
                _pendingSyncReactions.Count);
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

        internal void AddCommand(ReactionGraph reaction)
        {
            FlushPendingAsyncReaction();
            _pendingSyncReactions.Add(reaction);
        }

        private void FlushPendingAsyncReaction()
        {
            if (!_pendingAsyncReaction.HasReaction)
                return;

            FlushPendingSyncReactions();
            _orderedBlocks.Add(_pendingAsyncReaction.BuildReaction());
            _pendingAsyncReaction = PendingAsyncReaction<TModel>.None;
        }

        private void FlushPendingBranch()
        {
            if (!_pendingBranch.HasCases)
                return;

            var pendingSyncReactions = TakePendingSyncReactions();
            _pendingBranch.AppendTo(_orderedBlocks, pendingSyncReactions);
            _pendingBranch = PendingBranch.None;
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

        private sealed class PendingAsyncReaction<T> where T : class
        {
            private readonly System.Func<ReactionGraph> _buildReaction;

            private PendingAsyncReaction(bool hasReaction, System.Func<ReactionGraph> buildReaction)
            {
                HasReaction = hasReaction;
                _buildReaction = buildReaction;
            }

            internal static PendingAsyncReaction<T> None { get; } =
                new PendingAsyncReaction<T>(
                    hasReaction: false,
                    buildReaction: () => throw new System.InvalidOperationException("No pending async reaction exists."));

            internal static PendingAsyncReaction<T> Request(HttpRequestBuilder<T> request) =>
                new PendingAsyncReaction<T>(
                    hasReaction: true,
                    buildReaction: () => ReactionGraph.Request(request.BuildRequest()));

            internal static PendingAsyncReaction<T> Parallel(ParallelBuilder<T> parallelRequests) =>
                new PendingAsyncReaction<T>(
                    hasReaction: true,
                    buildReaction: parallelRequests.BuildReaction);

            internal bool HasReaction { get; }

            internal ReactionGraph BuildReaction() => _buildReaction();
        }

        private sealed class PendingBranch
        {
            private readonly List<BranchCase> _cases;
            private readonly int _startsAfterReactionCount;

            private PendingBranch(List<BranchCase> cases, int startsAfterReactionCount)
            {
                _cases = cases;
                _startsAfterReactionCount = startsAfterReactionCount;
            }

            internal static PendingBranch None { get; } =
                new PendingBranch(new List<BranchCase>(), 0);

            internal bool HasCases => _cases.Count > 0;

            internal static PendingBranch Cases(
                List<BranchCase> cases,
                int startsAfterReactionCount) =>
                new PendingBranch(cases, startsAfterReactionCount);

            internal void AppendTo(
                List<ReactionGraph> orderedBlocks,
                List<ReactionGraph> pendingSyncReactions)
            {
                AppendSequence(
                    orderedBlocks,
                    pendingSyncReactions,
                    start: 0,
                    count: _startsAfterReactionCount);
                orderedBlocks.Add(ReactionGraph.Branch(_cases));
                AppendSequence(
                    orderedBlocks,
                    pendingSyncReactions,
                    start: _startsAfterReactionCount,
                    count: pendingSyncReactions.Count - _startsAfterReactionCount);
            }

            private static void AppendSequence(
                List<ReactionGraph> orderedBlocks,
                List<ReactionGraph> reactions,
                int start,
                int count)
            {
                if (count > 0)
                    orderedBlocks.Add(ReactionGraph.Sequence(reactions.GetRange(start, count)));
            }
        }
    }
}
