using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    internal sealed class ParallelDraft
    {
        private readonly List<Request> _branches = new List<Request>();
        private ParallelCompletion _completion = ParallelCompletion.None;

        internal void AddBranch(Request request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            _branches.Add(request);
        }

        internal void CompleteWhenAllSettled(Reaction reaction)
        {
            if (reaction == null) throw new ArgumentNullException(nameof(reaction));
            _completion = ParallelCompletion.OnSettled(reaction);
        }

        internal Reaction ToReaction(IReadOnlyList<Reaction> preFetch)
        {
            if (preFetch == null) throw new ArgumentNullException(nameof(preFetch));
            if (_branches.Count == 0)
                throw new InvalidOperationException(
                    "Parallel requires at least one HTTP request branch.");

            var reactions = new List<Reaction>();
            var hasPreFetchCommands = preFetch.Count > 0;
            if (hasPreFetchCommands)
                reactions.AddRange(preFetch);

            foreach (var branch in _branches)
                reactions.Add(Reaction.Request(branch));

            return Reaction.Parallel(reactions, _completion);
        }
    }
}
