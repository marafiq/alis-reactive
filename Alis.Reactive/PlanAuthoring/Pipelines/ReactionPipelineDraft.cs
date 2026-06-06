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
            AppendPendingAsyncReaction();
            AppendPendingBranch();
            var builder = new HttpRequestBuilder<TModel>(context);
            _pendingAsyncReaction = PendingAsyncReaction<TModel>.Request(builder);
            return builder;
        }

        internal ParallelBuilder<TModel> BeginParallel(PlanBuildContext context)
        {
            AppendPendingAsyncReaction();
            AppendPendingBranch();
            var builder = new ParallelBuilder<TModel>(context);
            _pendingAsyncReaction = PendingAsyncReaction<TModel>.Parallel(builder);
            return builder;
        }

        internal void BeginBranch()
        {
            AppendPendingAsyncReaction();
            AppendPendingBranch();
        }

        internal void SetConditionalBranches(List<BranchCase> branches)
        {
            _pendingBranch = PendingBranch.Cases(
                branches,
                _pendingSyncReactions.Count);
        }

        internal void AppendPendingSegment()
        {
            AppendPendingAsyncReaction();
            AppendPendingBranch();
            AppendPendingSyncReactions();
        }

        internal ReactionGraph BuildReaction()
        {
            AppendPendingSegment();
            return _orderedBlocks.Count == 1
                ? _orderedBlocks[0]
                : ReactionGraph.Sequence(_orderedBlocks);
        }

        internal void AddReaction(ReactionGraph reaction)
        {
            AppendPendingAsyncReaction();
            _pendingSyncReactions.Add(reaction);
        }

        private void AppendPendingAsyncReaction()
        {
            _pendingAsyncReaction.AppendTo(_orderedBlocks, AppendPendingSyncReactions);
            _pendingAsyncReaction = PendingAsyncReaction<TModel>.None;
        }

        private void AppendPendingBranch()
        {
            if (!_pendingBranch.HasCases)
                return;

            var pendingSyncReactions = TakePendingSyncReactions();
            _pendingBranch.AppendTo(_orderedBlocks, pendingSyncReactions);
            _pendingBranch = PendingBranch.None;
        }

        private void AppendPendingSyncReactions()
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

        private abstract class PendingAsyncReaction<T> where T : class
        {
            internal static PendingAsyncReaction<T> None { get; } =
                new NoPendingAsyncReaction();

            internal static PendingAsyncReaction<T> Request(HttpRequestBuilder<T> request) =>
                new PendingRequestReaction(request);

            internal static PendingAsyncReaction<T> Parallel(ParallelBuilder<T> parallelRequests) =>
                new PendingParallelReaction(parallelRequests);

            internal abstract void AppendTo(List<ReactionGraph> orderedBlocks, System.Action appendPendingSyncReactions);

            private sealed class NoPendingAsyncReaction : PendingAsyncReaction<T>
            {
                internal override void AppendTo(List<ReactionGraph> orderedBlocks, System.Action appendPendingSyncReactions)
                {
                }
            }

            private sealed class PendingRequestReaction : PendingAsyncReaction<T>
            {
                private readonly HttpRequestBuilder<T> _request;

                internal PendingRequestReaction(HttpRequestBuilder<T> request)
                {
                    _request = request;
                }

                internal override void AppendTo(List<ReactionGraph> orderedBlocks, System.Action appendPendingSyncReactions)
                {
                    appendPendingSyncReactions();
                    orderedBlocks.Add(ReactionGraph.Request(_request.BuildRequest()));
                }
            }

            private sealed class PendingParallelReaction : PendingAsyncReaction<T>
            {
                private readonly ParallelBuilder<T> _parallelRequests;

                internal PendingParallelReaction(ParallelBuilder<T> parallelRequests)
                {
                    _parallelRequests = parallelRequests;
                }

                internal override void AppendTo(List<ReactionGraph> orderedBlocks, System.Action appendPendingSyncReactions)
                {
                    appendPendingSyncReactions();
                    orderedBlocks.Add(_parallelRequests.BuildReaction());
                }
            }
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
