using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    internal sealed class ParallelDraft
    {
        private readonly List<RequestPlan> _branches = new List<RequestPlan>();
        private ParallelCompletion _completion = ParallelCompletion.None;

        internal void AddBranch(RequestPlan request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            _branches.Add(request);
        }

        internal void CompleteWhenAllSettled(ReactionGraph reaction)
        {
            if (reaction == null) throw new ArgumentNullException(nameof(reaction));
            _completion = ParallelCompletion.OnSettled(reaction);
        }

        internal ReactionGraph ToReaction()
        {
            if (_branches.Count == 0)
                throw new InvalidOperationException(
                    "Parallel requires at least one HTTP request branch.");

            var reactions = new List<ReactionGraph>();
            foreach (var branch in _branches)
                reactions.Add(ReactionGraph.Request(branch));

            return ReactionGraph.Parallel(reactions, _completion);
        }
    }
}
