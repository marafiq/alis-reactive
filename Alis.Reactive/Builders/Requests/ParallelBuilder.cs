using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
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

        public ParallelBuilder<TModel> OnAllSettled(Action<PipelineBuilder<TModel>> pipeline)
        {
            var pb = new PipelineBuilder<TModel>(_context);
            pipeline(pb);
            _onSettled = pb.BuildReaction();
            return this;
        }

        internal Reaction BuildReaction(List<Reaction> preFetch)
        {
            var requestReactions = _branches.Select(r => Reaction.Request(r)).ToList();

            if (preFetch != null && preFetch.Count > 0)
                requestReactions.InsertRange(0, preFetch);

            return Reaction.Parallel(requestReactions, _onSettled);
        }
    }
}
