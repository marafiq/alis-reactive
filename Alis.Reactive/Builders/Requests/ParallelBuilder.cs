using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    /// <summary>Builds a set of HTTP requests that execute concurrently.</summary>
    public class ParallelBuilder<TModel> where TModel : class
    {
        private readonly PlanBuildContext _context;
        private readonly List<Request> _branches = new List<Request>();
        private Reaction _onSettled;

        internal ParallelBuilder(PlanBuildContext context)
        {
            _context = context;
        }

        internal void AddBranch(Action<HttpRequestBuilder<TModel>> request)
        {
            var builder = new HttpRequestBuilder<TModel>(_context);
            request(builder);
            _branches.Add(builder.BuildRequest());
        }

        /// <summary>Executes commands after all parallel requests complete.</summary>
        public ParallelBuilder<TModel> OnAllSettled(Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            var reaction = pb.BuildReaction();
            var isPlainSequence = reaction is SequenceReaction;
            if (!isPlainSequence)
                throw new InvalidOperationException(
                    "OnAllSettled only supports plain commands (sequential). " +
                    "Conditions, HTTP, and parallel pipelines are not valid here.");
            _onSettled = reaction;
            return this;
        }

        internal Reaction BuildReaction(List<Reaction> preFetch)
        {
            var requestReactions = _branches.Select(r => Reaction.Request(r)).ToList();

            var hasPreFetchCommands = preFetch.Count > 0;
            if (hasPreFetchCommands)
                requestReactions.InsertRange(0, preFetch);

            return Reaction.Parallel(requestReactions, _onSettled);
        }
    }
}
