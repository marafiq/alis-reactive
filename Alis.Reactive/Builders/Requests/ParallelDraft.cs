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

        internal void CompleteWhenAllSettled(Reaction reaction)
        {
            if (reaction == null) throw new ArgumentNullException(nameof(reaction));
            _completion = ParallelCompletion.OnSettled(reaction);
        }

        internal Reaction ToReaction()
        {
            if (_branches.Count == 0)
                throw new InvalidOperationException(
                    "Parallel requires at least one HTTP request branch.");

            var reactions = new List<Reaction>();
            foreach (var branch in _branches)
                reactions.Add(Reaction.Request(branch));

            return Reaction.Parallel(reactions, _completion);
        }
    }
}
